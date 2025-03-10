import boto3
import json
from botocore.exceptions import ClientError
from datetime import datetime

# AWS 리소스 및 클라이언트 초기화
dynamoDb = boto3.resource('dynamodb')
userInfoTable = dynamoDb.Table('OneMoreTimeUserInfo')
attendanceRewardTable = dynamoDb.Table('OneMoreTimeAttendanceRewardTable')
lambdaClient = boto3.client('lambda')

maxAttendanceDay = 21

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
#########################################

############# Return Values #############
# 0     : Already received the attendence reward.
# -1    : Data load failed
# -2    : Over Max Attendance Day
# -3    : Already Received Today Attendance Reward
# -4    : Error By Update User Item Quantity 
# else  : Success ( return result json - AttendanceLevel, LastAttendanceReceivedTime)
# #########################################
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

    attendanceLevel = userData.get('AttendanceLevel', 0)
    lastAttendanceTs = int(userData.get('LastAttendanceReceivedTime', 0))
    lastAttendanceTime = datetime.fromtimestamp(lastAttendanceTs) if lastAttendanceTs else None
    now = datetime.now()

    # 월 또는 년이 바뀌었으면 출석 기록 초기화
    if lastAttendanceTime and (now.year, now.month) > (lastAttendanceTime.year, lastAttendanceTime.month):
        attendanceLevel = 0
        lastAttendanceTime = None

    # 이미 오늘 출석 보상을 받았다면 종료
    if lastAttendanceTime and lastAttendanceTime.date() == now.date():
        return 0

    # 최대 출석일수를 초과한 경우
    if attendanceLevel >= maxAttendanceDay:
        return -2

    # 보상 데이터 불러오기
    try:
        rewardResponse = attendanceRewardTable.get_item(Key={'Month': now.month, 'Day': attendanceLevel + 1})
        rewardItem = rewardResponse.get('Item')
        if not rewardItem:
            rewardResponse = attendanceRewardTable.get_item(Key={'Month': 0, 'Day': attendanceLevel + 1})
            rewardItem = rewardResponse.get('Item')
            if not rewardItem:
                print("No reward found")
                return -1
    except ClientError as e:
        print(e.response['Error']['Message'])
        return -1

    # 보상 지급 시작
    rewardItemIds = rewardItem.get('RewardItemIds', [])
    rewardAmounts = rewardItem.get('RewardAmounts', [])

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

    # 보상 완료 후 출석일수와 출석시간 갱신
    attendanceLevel += 1
    newTimestamp = int(now.timestamp())
    try:
        userInfoTable.update_item(
            Key={'AccountId': accountId},
            UpdateExpression="SET AttendanceLevel = :attLv, LastAttendanceReceivedTime = :rcvTime",
            ExpressionAttributeValues={
                ':attLv': attendanceLevel,
                ':rcvTime': newTimestamp
            }
        )
    except ClientError as e:
        print(e.response['Error']['Message'])
        return -1

    return {
        'AttendanceLevel': attendanceLevel,
        'LastAttendanceReceivedTime': newTimestamp
    }
