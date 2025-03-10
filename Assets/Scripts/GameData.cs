using Defective.JSON;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameData
{
    static GameData _instance;
    public const int MaxRestPoint = 300;
    public static DateTime DummyTime;

    public static string EventCode
    {
        get
        {
            return PlayerPrefs.GetString("LastEventCode", "normal");
        }
    }

    public static GameData Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new GameData();
            }
            return _instance;
        }
    }
    GameData()
    {
        LoadJson();
    }

    public Dictionary<ulong, Item> Items = new Dictionary<ulong, Item>();

    public List<AttendanceReward> AttendanceRewards;
    public List<RestPointReward> RestPointRewards;

    public DateTime LastLogoutTime;

    void LoadJson()
    {
        LoadItemJson();
        LoadAttendanceRewardListJson();
        LoadRestPointRewardListJson();
    }

    void LoadItemJson()
    {
        TextAsset jsonText = (TextAsset)Resources.Load("JsonData/Item", typeof(TextAsset));
        JSONObject jsonObj = new JSONObject(jsonText.text);

        // JSON에 기록된 일반 아이템 추가
        for (int i = 0; i < jsonObj.count; i++)
        {
            ulong id = ulong.Parse(jsonObj[i].GetField("Id").stringValue);
            string nameId = jsonObj[i].GetField("Name").stringValue;

            Items.Add(id, new Item(id, int.Parse(jsonObj[i].GetField("Category").stringValue),
                    ulong.Parse(jsonObj[i].GetField("IconId").stringValue),
                    nameId, jsonObj[i].GetField("Description").stringValue, int.Parse(jsonObj[i].GetField("QuantityGroupPer").stringValue)));
        }
    }

    void LoadAttendanceRewardListJson()
    {
        AttendanceRewards = new List<AttendanceReward>();

        TextAsset jsonText = (TextAsset)Resources.Load("JsonData/AttendanceRewardList", typeof(TextAsset));
        JArray jArray = JArray.Parse(jsonText.text);

        for (int i = 0; i < jArray.Count; i++)
        {
            int month = jArray[i]["Month"].Value<int>();
            int day = jArray[i]["Day"].Value<int>();
            string rewardItemId = jArray[i]["RewardItemIds"].Value<string>();
            string rewardAmount = jArray[i]["RewardAmounts"].Value<string>();

            if (!string.IsNullOrEmpty(rewardItemId))
            {
                string[] itemIds = rewardItemId.Split(',').Select(item => item.Trim()).ToArray();
                string[] itemQuantites = rewardAmount.Split(',').Select(item => item.Trim()).ToArray();

                ItemPackage[] items = new ItemPackage[itemIds.Length];

                for (int j = 0; j < itemIds.Length; j++)
                {
                    ItemPackage item = new ItemPackage(ulong.Parse(rewardItemId), double.Parse(itemQuantites[j]), "");
                    items[j] = item;
                }

                AttendanceRewards.Add(new AttendanceReward(month, day, items));
            }
        }
    }

    void LoadRestPointRewardListJson()
    {
        RestPointRewards = new List<RestPointReward>();

        TextAsset jsonText = (TextAsset)Resources.Load("JsonData/RestPointRewardList", typeof(TextAsset));
        JArray jArray = JArray.Parse(jsonText.text);

        for (int i = 0; i < jArray.Count; i++)
        {
            string rewardItemId = jArray[i]["RewardItemIds"].Value<string>();
            string rewardAmount = jArray[i]["RewardAmounts"].Value<string>();
            string eventCode = jArray[i]["EventCode"].Value<string>();

            if (!string.IsNullOrEmpty(rewardItemId))
            {
                string[] itemIds = rewardItemId.Split(',').Select(item => item.Trim()).ToArray();
                string[] itemQuantites = rewardAmount.Split(',').Select(item => item.Trim()).ToArray();

                ItemPackage[] items = new ItemPackage[itemIds.Length];

                for (int j = 0; j < itemIds.Length; j++)
                {
                    ItemPackage item = new ItemPackage(ulong.Parse(rewardItemId), double.Parse(itemQuantites[j]), "");
                    items[j] = item;
                }

                RestPointRewards.Add(new RestPointReward(eventCode, items));
            }
        }
    }

    public Item GetItem(ulong itemId)
    {
        Item item = null;
        Items.TryGetValue(itemId, out item);
        return item;
    }

    public string GetItemName(ulong itemId)
    {
        ulong category = itemId / 100000000;
        switch (category)
        {
            // 아이템 종류별로 분류
            default:
                Item item = GetItem(itemId);
                if (item != null)
                    return item.Name;
                break;
        }
        return null;
    }

    public string GetItemIconPath(ulong itemId)
    {
        const string defaultPath = "UI/Icon/";

        ulong category = itemId / 100000000;
        switch (category)
        {
            default:
                Item item = GetItem(itemId);
                if (item != null)
                    return defaultPath + item.IconId.ToString("D10");
                break;
        }
        return null;
    }
}
