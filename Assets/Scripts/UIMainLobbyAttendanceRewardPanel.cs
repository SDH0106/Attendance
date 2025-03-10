using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIMainLobbyAttendanceRewardPanel : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI _titleText;
    [SerializeField] Image _rewardIconImage;
    [SerializeField] TextMeshProUGUI _rewardDescriptionText;
    [SerializeField] Transform _iconGroupTransform;
    [SerializeField] UIMainLobbyAttendancePanelView _attendancePanelView;

    public void ShowAttendanceRewardPanel(Dictionary<ulong, double> rewardItems, int attendanceDate)
    {
        _titleText.text = "출석 보상";
        ShowPanel(rewardItems);
    }

    public void ShowRestPointRewardsPanel(Dictionary<ulong, double> rewardItems)
    {
        _titleText.text = "휴식 보상";
        ShowPanel(rewardItems);
    }

    public void ShowPanel(Sprite rewardIcon, string rewardDescription)
    {
        _rewardIconImage.sprite = rewardIcon;
        _rewardDescriptionText.text = rewardDescription;

        _iconGroupTransform.gameObject.SetActive(false);
        _rewardIconImage.enabled = true;
        gameObject.SetActive(true);
    }

    public void ShowPanel(Dictionary<ulong, double> rewardItems)
    {
        _rewardDescriptionText.text = null;

        foreach (var rewardItem in rewardItems)
        {
            GameObject obj = new GameObject("Image");
            obj.transform.parent = _iconGroupTransform;
            obj.transform.localScale = Vector3.one;

            Image iconImage = obj.AddComponent<Image>();
            iconImage.sprite = Resources.Load<Sprite>(GameData.Instance.GetItemIconPath(rewardItem.Key));
            iconImage.preserveAspect = true;
        }

        _rewardDescriptionText.text = string.Join("\n", rewardItems.Select(x => $"{GameData.Instance.GetItemName(x.Key)} x {x.Value}"));

        _rewardIconImage.enabled = false;
        _iconGroupTransform.gameObject.SetActive(true);
        gameObject.SetActive(true);
    }

    public void OnConfirm()
    {
        for (int i = 0; i < _iconGroupTransform.childCount; i++)
        {
            Destroy(_iconGroupTransform.GetChild(i).gameObject);
        }

        gameObject.SetActive(false);
    }
}
