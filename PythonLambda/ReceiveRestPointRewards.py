import boto3
import json
from botocore.exceptions import ClientError
from boto3.dynamodb.conditions import Key, Attr

dynamoDb = boto3.resource('dynamodb')
userInfoTable = dynamoDb.Table('OneMoreTimeUserInfo')
restRewardTable = dynamoDb.Table('OneMoreTimeRestRewardTable')
lambdaClient = boto3.client('lambda')

############## Constants ###############
RestPointCap = 300 # 휴식 보상을 받을 수 있는 휴식포인트 값
########################################

# 아이템 ID에 따른 카테고리 결정
def getItemCategory(itemId):
    if itemId in (1, 2, 3):
        return 'special'
    if itemId // 100000000 == 4:
        return 'pet'
    if itemId // 100000000 == 8:
        return 'costume'
    return 'item'

############# Input Values #############
# AccountId
# SelectedIndex
# EventCode
# SelectedEventIndex
########################################

############# Return Values #############
# -1    : Data load failed
# -2    : Error By Selected Reward Slot Index
# -3    : No Reward Items for Event Code
# -4    : Error By Update User Item Quantity 
# -5    : Not enough rest point.
# else  : Success ( return result json - restPoint)
#########################################

# 휴식 포인트 보상 제공 함수
def lambda_handler(event, context):
    accountId = event.get('AccountId')   

    if not accountId:
        print("AccountId is missing in the event")
        return -1

    # 사용자 데이터 로드
    try:
        response = userInfoTable.get_item(Key={'AccountId': accountId})
        userData = response.get('Item')
        if not userData:
            print(f"No user data found for AccountId: {accountId}")
            return -1
        print(f"Load User data: {userData.get('AccountId')}")
    except ClientError as e:
        print(e.response['Error']['Message'])
        return -1
    
    restPointStr = userData.get('RestPoint', '0')
    restPoint = float(restPointStr)
    if restPoint < RestPointCap:
        print(f"Not enough rest point. RestPoint: {restPoint}")
        return -5
    else:
        restPoint = RestPointCap
    print(f'Rest Point : {restPoint}')
    eventCode = event.get('EventCode', 'normal')
    selectedIndex = event.get('SelectedIndex', -1)
    selectedEventIndex = event.get('SelectedEventIndex', 0)

    if selectedIndex == -1:
        print("event parameter SelectedIndex is missing")
        return -2

    # 공통 보상 중 선택한 보상 데이터 불러오기
    try:
        rewardResponse = restRewardTable.get_item(Key={'EventCode': 'normal', 'SlotId': selectedIndex})
        rewardItem = rewardResponse.get('Item')
        if not rewardItem:
            print(f"No reward found for selected slot index: {selectedIndex}")
            return -2
        rewardItemIds = rewardItem.get('RewardItemIds', [])
        rewardAmounts = rewardItem.get('RewardAmounts', [])
    except ClientError as e:
        print(e.response['Error']['Message'])
        return -2

    # 특정 EventCode 보상 데이터 불러오기(아무 이벤트가 없는 기본 EventCode인 'normal' 제외)
    if eventCode != 'normal':
        try:
            rewardResponse = restRewardTable.get_item(Key={'EventCode': eventCode, 'SlotId':selectedEventIndex})
            rewardItem = rewardResponse.get('Item')
            if not rewardItem:
                print(f"No rewards found for EventCode: {eventCode}")
                return -3
            eventRewardItemIds = rewardItem.get('RewardItemIds', None)
            eventRewardAmounts = rewardItem.get('RewardAmounts', None)
            if eventRewardItemIds and eventRewardAmounts:
                rewardItemIds.extend(eventRewardItemIds)
                rewardAmounts.extend(eventRewardAmounts)
            else:
                print(f"{eventCode} EventCode Item's RewardItemIds or RewardAmounts is None")
                return -3 
        except ClientError as e:
            print(e.response['Error']['Message'])
            return -3
    
    # 보상 지급 시작
    for itemId, amount in zip(rewardItemIds, rewardAmounts):
        itemCategory = getItemCategory(itemId)        

        payload = {
            "AccountId": accountId,
            "ItemId": int(itemId),
            "Quantity": float(amount),
            "Modifier": "add",
            "ItemCategory": itemCategory
        }
        try:
            lambdaResponse = lambdaClient.invoke(
                FunctionName='SetUserItem',
                InvocationType='RequestResponse',
                LogType='Tail',
                Payload=json.dumps(payload)
            )
            responsePayload = json.loads(lambdaResponse['Payload'].read())
            print (responsePayload)
            if 'Quantity' not in responsePayload or responsePayload.get('Quantity') is None or responsePayload.get('Quantity', 0) == -1:
                return -4
        except ClientError as e:
            print(e.response['Error']['Message'])
            return -4

    # 보상 완료 후 휴식 게이지 초기화
    restPoint = 0
    try:
        userInfoTable.update_item(
            Key={'AccountId': accountId},
            UpdateExpression="SET RestPoint = :restPoint",
            ExpressionAttributeValues={
                ':restPoint': str(restPoint)
            }
        )
    except ClientError as e:
        print(e.response['Error']['Message'])
        return -1

    return {
        'RestPoint': restPoint,
    }