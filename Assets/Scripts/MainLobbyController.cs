using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MainLobbyController: MonoBehaviour
{
    [SerializeField] UIMainLobbyAttendancePanelView AttendancePanel;

    public static MainLobbyController Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
        StartCoroutine(TickMinute());
    }

    IEnumerator TickMinute()
    {
        GameData.DummyTime = GameData.DummyTime.AddMinutes(1);
        yield return new WaitForSecondsRealtime(60f);
    }

    public void ShowAttendancePanel()
    {
        AttendancePanel.gameObject.SetActive(true);
    }

    public void ReceiveAttendanceReward(int attendanceDate, AttendanceReward attendanceReward)
    {
        Dictionary<ulong, double> rewardItems = attendanceReward.items.ToDictionary(item => item.itemId,
                item => item.quantity
                );

        foreach (var rewardItem in rewardItems)
            User.Instance.AddQuantityOfItem(rewardItem.Key, rewardItem.Value);

        AttendancePanel.AttendanceRewardPannel.ShowAttendanceRewardPanel(rewardItems, attendanceDate);
    }

    public void ShowRestPointRewardReceiveConfirmWindow(RestPointReward[] restPointRewards)
    {
        Dictionary<ulong, double> rewardItems = restPointRewards.SelectMany(x => x.items)
            .GroupBy(items => items.itemId)
            .ToDictionary(g => g.Key,
            g => g.Sum(item => item.quantity)
                );

        foreach (var rewardItem in rewardItems)
            User.Instance.AddQuantityOfItem(rewardItem.Key, rewardItem.Value);

        AttendancePanel.AttendanceRewardPannel.ShowRestPointRewardsPanel(rewardItems);
        AttendancePanel.ResetRestPoint();
    }
}
