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
# else  : Success ( return result json - LastLogoutTime)
#########################################

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

    logoutTs = int(userData.get('LastLogoutTime', 0))
    logoutTime = datetime.fromtimestamp(logoutTs) if logoutTs else None
    now = datetime.now(timezone.utc)

    # 로그아웃 시각 갱신
    nowTimeStamp = int(now.timestamp())
    try:
        userInfoTable.update_item(
            Key={'AccountId': accountId},
            UpdateExpression="SET LastLogoutTime = :logoutTime",
            ExpressionAttributeValues={
                ':logoutTime': nowTimeStamp
            }
        )
    except ClientError as e:
        print(e.response['Error']['Message'])
        return -1
    
    return {
        'LastLogoutTime' : nowTimeStamp
    }
