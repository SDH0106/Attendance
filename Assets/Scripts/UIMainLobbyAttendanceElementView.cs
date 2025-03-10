using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIMainLobbyAttendanceElementView : MonoBehaviour
{
    int elementIndex;

    Image elementSlotImage;
    TextMeshProUGUI dateText;
    Transform ItemImageGroup;
    TextMeshProUGUI itemName;
    GameObject attendanceCheckBox;

    AttendanceReward reward;

    int todayAttendanceLevel;

    int maxDate;

    Color normalSlotColor = new Color(0.75f, 0.75f, 0.75f, 1);

    public void OnLoadComponents()
    {
        elementSlotImage = GetComponent<Image>();
        dateText = transform.Find("Day").GetComponent<TextMeshProUGUI>();
        attendanceCheckBox = transform.Find("AttendanceCheckImage").gameObject;
        ItemImageGroup = transform.Find("ItemImageGroup");
        itemName = transform.Find("ItemName").GetComponent<TextMeshProUGUI>();
    }

    public void Initialize(int todayAttendanceLevel, int elementIndex, int maxDate)
    {
        int currentMonth = GameData.DummyTime.Month;
        this.elementIndex = elementIndex;
        this.todayAttendanceLevel = todayAttendanceLevel;
        this.maxDate = maxDate;

        reward = GameData.Instance.AttendanceRewards.Find(x => x.month == currentMonth && x.day == elementIndex + 1);

        if (reward == null)
            reward = GameData.Instance.AttendanceRewards.Find(x => x.month == 0 && x.day == elementIndex + 1);

        elementSlotImage.color = todayAttendanceLevel == elementIndex ? Color.cyan : normalSlotColor;
        dateText.text = (elementIndex + 1).ToString();

        for (int i = 0; i < reward.items.Length; ++i)
        {
            Image iconImage = null;
            itemName.text = "";

            if (ItemImageGroup.childCount <= i)
            {
                GameObject obj = new GameObject("ItemImage");
                obj.transform.parent = ItemImageGroup;
                obj.transform.localScale = Vector3.one;

                iconImage = obj.AddComponent<Image>();
                iconImage.preserveAspect = true;
            }
            else
            {
                iconImage = ItemImageGroup.GetChild(i).GetComponent<Image>();
            }

            iconImage.sprite = Resources.Load<Sprite>(GameData.Instance.GetItemIconPath(reward.items[i].itemId));

            itemName.text = string.Join("\n", $"{GameData.Instance.GetItemName(reward.items[i].itemId)} x {reward.items[i].quantity}");
        }

        attendanceCheckBox.SetActive(elementIndex == todayAttendanceLevel ? User.Instance.IsReceivedAttendanceToday() : elementIndex < todayAttendanceLevel);
    }

    public void OnClick()
    {
        if (elementIndex != todayAttendanceLevel || User.Instance.IsReceivedAttendanceToday())
            return;

        if (maxDate <= User.Instance.AttendanceLevel)
            return;

        attendanceCheckBox.SetActive(true);

        MainLobbyController.Instance.ReceiveAttendanceReward(User.Instance.AttendanceLevel + 1, reward);

        User.Instance.RecordAttendanceRecevied();
        Debug.Log("get item");
    }
}
