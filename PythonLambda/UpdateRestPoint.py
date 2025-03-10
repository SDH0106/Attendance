import boto3
from botocore.exceptions import ClientError
from boto3.dynamodb.conditions import Key, Attr
from datetime import datetime, timezone

dynamoDb = boto3.resource('dynamodb')
userInfoTable = dynamoDb.Table('OneMoreTimeUserInfo')

############# Input Values #############
# AccountId
########################################

############# Return Values #############
# -1    : Data load failed
# -2    : Update Skip By First Login 
# -3    : LogoutTime Error
# else  : Success ( return result json - RestPoint, LogoutTime)
#########################################

# 휴식 포인트 갱신 함수
def lambda_handler(event, context):

    accountId = event['AccountId']
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
    
    restPoint = userData.get('RestPoint', 0)
    lastLogoutTs = int(userData.get('LogoutTime', 0))
    logoutTime = datetime.fromtimestamp(lastLogoutTs) if lastLogoutTs else None
    now = datetime.now(timezone.utc)

    # 로그아웃 시각의 값이 초기값인 경우 스킵
    if lastLogoutTs and lastLogoutTs == 0:
        return -2

    # 잘못된 로그아웃 시각이 적용된 경우(현재보다 더 미래의 시각)
    if logoutTime and logoutTime >= now:
        return -3
    
    # 미접속 1시간당 10포인트의 비율로 휴식 포인트 증가
    timeSpan = now - logoutTime
    restPoint += timeSpan.total_seconds() / 360
    restPoint = restPoint if restPoint <= 300 else 300
    
    # 휴식 포인트, 로그아웃 시각 갱신
    nowTimeStamp = int(now.timestamp())
    try:
        userInfoTable.update_item(
            Key={'AccountId': accountId},
            UpdateExpression="SET RestPoint = :restPoint, LogoutTime = :logoutTime",
            ExpressionAttributeValues={
                ':restPoint': restPoint,
                ':logoutTime': nowTimeStamp
            }
        )
    except ClientError as e:
        print(e.response['Error']['Message'])
        return -1
    
    return {
        'RestPoint': restPoint,
        'LogoutTime' : nowTimeStamp
    }