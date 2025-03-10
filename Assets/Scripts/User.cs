using Defective.JSON;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public partial class User
{
    static User _instance;
    public const string SavedJsonPrefsKey = "SavedJson";
    public Action OnValuesChange;

    public static User Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new User();
            }
            return _instance;
        }
    }
    User()
    {
        LoadJson();
    }

    public enum UserPropertyCategory
    {
        UserAttendanceLevel, lastAttendanceRewardReceivedTime, RestPoint, LastLogoutTime
    }

    void LoadJson() 
    {
        JSONObject loadJson = new JSONObject(PlayerPrefs.GetString(SavedJsonPrefsKey, ""));

        string valueString = "";

        loadJson.GetField(out valueString, UserPropertyCategory.LastLogoutTime.ToString(), default(DateTime).ToString());
        GameData.Instance.LastLogoutTime = DateTime.Parse(valueString);

        loadJson.GetField(out valueString, User.UserPropertyCategory.UserAttendanceLevel.ToString(), "0");
        int.TryParse(valueString, out _attendanceLevel);

        loadJson.GetField(out valueString, User.UserPropertyCategory.lastAttendanceRewardReceivedTime.ToString(), default(DateTime).ToString());
        _lastAttendanceReceivedTime = DateTime.Parse(valueString);

        loadJson.GetField(out valueString, UserPropertyCategory.RestPoint.ToString(), "0");
        double.TryParse(valueString, out _restPoint);
    }

    public void Save()
    {
        JSONObject saveJson = new JSONObject(JSONObject.Type.Object);

        saveJson.AddField(UserPropertyCategory.UserAttendanceLevel.ToString(), _attendanceLevel.ToString());
        saveJson.AddField(UserPropertyCategory.lastAttendanceRewardReceivedTime.ToString(), _lastAttendanceReceivedTime.ToString());
    }

    public List<UserItem> UserItems = new List<UserItem>();

    UserItem CreateItem(ulong itemId)
    {
        if (GameData.Instance.GetItem(itemId) == null)
            return null;

        UserItem item = new UserItem(itemId, 0);

        UserItems.Add(item);
        return item;
    }

    public double GetQuantityOfItem(ulong itemId)
    {
        double count = 0;

        if (UserItems == null)
            return 0;

        foreach (UserItem item in UserItems)
        {
            if (item.ItemId == itemId)
            {
                count += item.Quantity;
            }
        }

        return count;
    }

    public bool AddQuantityOfItem(ulong itemId, double addNum)
    {
        UserItem item = null;
        if (GetQuantityOfItem(itemId) == 0)
        {
            item = CreateItem(itemId);
        }
        else
        {
            foreach (UserItem CurrentItem in UserItems)
            {
                if (CurrentItem.ItemId == itemId && CurrentItem.Quantity <= CurrentItem.QuantityGroupPer)
                    item = CurrentItem;
            }
        }
        while (addNum > 0)
        {
            addNum = item.AddQuantity(addNum);
            if (addNum > 0)
                item = CreateItem(itemId);
        }

        NotifyValuesChanged();

        return true;
    }

    public bool SubQuantityOfItem(ulong itemId, double subNum)
    {
        if (subNum > GetQuantityOfItem(itemId))
            return false;

        UserItem item = null;
        for (int i = UserItems.Count; i > 0; i--)
        {
            item = UserItems[i - 1];
            if (item.ItemId == itemId)
            {
                subNum = item.SubQuantity(subNum);
            }
            if (item.Quantity <= 0)
            {
                UserItems.Remove(item);
            }
            if (subNum <= 0)
                break;
        }

        NotifyValuesChanged();

        return true;

    }

    void NotifyValuesChanged()
    {
        if (OnValuesChange != null)
            OnValuesChange();
    }
}
