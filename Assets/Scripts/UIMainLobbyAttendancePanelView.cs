using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIMainLobbyAttendancePanelView : MonoBehaviour
{
    [SerializeField] UIMainLobbyAttendanceRewardPanel rewardPannel;
    public UIMainLobbyAttendanceRewardPanel AttendanceRewardPannel { get { return rewardPannel; } }

    [SerializeField] RectTransform attendanceElementGroup;
    [SerializeField] RectTransform restElementGroup;
    [SerializeField] RectTransform restUIGaugeGroup;
    [SerializeField] Button restPointRewardReceiveButton;
    [SerializeField] TextMeshProUGUI attendancePanelTitle;
    [SerializeField] TextMeshProUGUI restPanelTitle;
    [SerializeField] Slider restPointGaugeSlider;
    [SerializeField] TextMeshProUGUI restPointText;
    [SerializeField] TextMeshProUGUI restGaugeDiscription;
    [SerializeField] TextMeshProUGUI restPointRewardButtonText;

    GameObject attandanceElement;
    GameObject lastAttandanceElement;
    GameObject restElement;

    UIMainLobbyAttendanceElementView[] attendanceElements;

    const float lastAttendanceElementSizeMultiple = 1.45f;
    const int totalAttendanceDay = 21;

    int todayAttendanceLevel;
    double restPoint;

    DateTime panelOnUpdateTime;

    private void Awake()
    {
        OnLoadComponents();
        Initialize();
    }

    void OnLoadComponents()
    {
        attandanceElement = Resources.Load<GameObject>("UI/MainLobby/ElementPrefabs/UIAttendanceElement");
        lastAttandanceElement = Resources.Load<GameObject>("UI/MainLobby/ElementPrefabs/UILastAttendanceElement");
        restElement = Resources.Load<GameObject>("UI/MainLobby/ElementPrefabs/UIRestElement");

        if (attendanceElements == null)
            attendanceElements = new UIMainLobbyAttendanceElementView[totalAttendanceDay];

        for (int i = 0; i < attendanceElementGroup.childCount; i++)
            attendanceElements[i] = attendanceElementGroup.GetChild(i).GetComponent<UIMainLobbyAttendanceElementView>();

        attendanceElements[totalAttendanceDay - 1] = transform.Find("AttendancePanel/UILastAttendanceElement").GetComponent<UIMainLobbyAttendanceElementView>();
    }

    private void Initialize()
    {
        todayAttendanceLevel = !User.Instance.IsReceivedAttendanceToday() ? User.Instance.AttendanceLevel : User.Instance.AttendanceLevel - 1;

        InstantiateAttendaceElements();
        InstantiateRestElements();

        restPoint = Math.Truncate(User.Instance.RestPoint / 10) * 10;
        restPointText.text = $"{restPoint} / {GameData.MaxRestPoint}";
        restPointGaugeSlider.value = (float)(restPoint / GameData.MaxRestPoint);

        restPointRewardReceiveButton.interactable = restPoint >= GameData.MaxRestPoint;
    }

    private void OnEnable()
    {
        UpdateAttendanceElementsSize();
        UpdateRestElementsSize();
        OnUpdatePanel();
        User.Instance.OnValuesChange += OnUpdatePanel;
    }

    private void OnDisable()
    {
        User.Instance.OnValuesChange -= OnUpdatePanel;
    }

    private void Update()
    {
        if (panelOnUpdateTime.Day != GameData.DummyTime.Day)
            OnUpdatePanel();
    }

    void OnUpdatePanel()
    {
        panelOnUpdateTime = GameData.DummyTime;
        User.Instance.ResetAttendanceIfMonthChanged();

        todayAttendanceLevel = !User.Instance.IsReceivedAttendanceToday() ? User.Instance.AttendanceLevel : User.Instance.AttendanceLevel - 1;

        InitializeAttendanceElements();
    }

    void UpdateAttendanceElementsSize()
    {
        float elementSize = Mathf.Clamp(attendanceElementGroup.rect.width / 5, 0, 120);
        attendanceElementGroup.GetComponent<GridLayoutGroup>().cellSize = new Vector2(elementSize, elementSize);

        elementSize = elementSize * lastAttendanceElementSizeMultiple;
        attendanceElements[totalAttendanceDay - 1].GetComponent<RectTransform>().sizeDelta = new Vector2(elementSize, elementSize);
    }
    
    void UpdateRestElementsSize()
    {
        float elementSize = Mathf.Clamp(restElementGroup.rect.width / restElementGroup.childCount, 0, 125);
        restElementGroup.GetComponent<GridLayoutGroup>().cellSize = new Vector2(elementSize, elementSize);
    }

    void InstantiateAttendaceElements()
    {
        for (int i = 0; i < attendanceElements.Length; ++i)
        {
            UIMainLobbyAttendanceElementView element = null;

            if (attendanceElements[i] == null)
            {
                if (i < attendanceElements.Length - 1)
                {
                    element = Instantiate(attandanceElement).GetComponent<UIMainLobbyAttendanceElementView>();
                    element.transform.SetParent(attendanceElementGroup);
                }
                else if(i == attendanceElements.Length - 1)
                {
                    element = Instantiate(lastAttandanceElement).GetComponent<UIMainLobbyAttendanceElementView>();
                    element.transform.SetParent(attendanceElementGroup.parent);
                }

                attendanceElements[i] = element;
            }
            else
                element = attendanceElements[i];

            element.OnLoadComponents();
            element.Initialize(todayAttendanceLevel, i, attendanceElements.Length);
        }
    }

    void InstantiateRestElements()
    {
        int index = 0;

        var sortList = GameData.Instance.RestPointRewards.OrderBy(x => !string.IsNullOrEmpty(x.eventCode));

        foreach (var restPointReward in sortList)
        {
            if (!string.IsNullOrEmpty(restPointReward.eventCode) && restPointReward.eventCode != GameData.EventCode)
                continue;

            UIMainLobbyRestElementView element;

            if (restElementGroup.childCount <= index)
                element = Instantiate(restElement, restElementGroup).GetComponent<UIMainLobbyRestElementView>();
            else
                element = restElementGroup.GetChild(index).GetComponent<UIMainLobbyRestElementView>();

            element.OnLoadComponent();
            element.Initialize(restPointReward);

            index++;
        }
    }

    void InitializeAttendanceElements()
    {
        for (int i = 0; i < attendanceElements.Length; ++i)
            attendanceElements[i].GetComponent<UIMainLobbyAttendanceElementView>().Initialize(todayAttendanceLevel, i, attendanceElements.Length);
    }

    public void ClickReceiveRestPointRewardButton()
    {
        if (restPoint < GameData.MaxRestPoint)
            return;

        var restElements = restElementGroup.GetComponentsInChildren<Toggle>();

        int selectedIndex = restElements.Where(x => x.isOn).First().transform.GetSiblingIndex();
        RestPointReward selectedReward = restElements[selectedIndex].gameObject.GetComponent<UIMainLobbyRestElementView>().GetReward();

        List<RestPointReward> rewards;

        if (GameData.Instance.RestPointRewards.Any(x => x.eventCode == GameData.EventCode))
            rewards = GameData.Instance.RestPointRewards.Where(x => x.eventCode == GameData.EventCode).ToList();
        else
            rewards = new List<RestPointReward>();

        rewards.Add(selectedReward);

        MainLobbyController.Instance.ShowRestPointRewardReceiveConfirmWindow(rewards.ToArray());
    }

    public void ResetRestPoint()
    {
        User.Instance.RestPoint = 0;
        User.Instance.Save();

        restPoint = 0;

        restPointText.text = $"{restPoint} / {GameData.MaxRestPoint}";
        restPointGaugeSlider.value = (float)(restPoint / GameData.MaxRestPoint);
        restPointRewardReceiveButton.interactable = restPoint >= GameData.MaxRestPoint;
    }
#if UNITY_EDITOR
    void OnGUI()
    {
        if (GUILayout.Button("Rest Point Up"))
        {
            User.Instance.RestPoint = GameData.MaxRestPoint;
            User.Instance.Save();

            restPoint = User.Instance.RestPoint;

            restPointText.text = $"{restPoint} / {GameData.MaxRestPoint}";
            restPointGaugeSlider.value = (float)(restPoint / GameData.MaxRestPoint);
            restPointRewardReceiveButton.interactable = restPoint >= GameData.MaxRestPoint;
        }

        if(GUILayout.Button("Day Up"))
        {
            GameData.DummyTime = GameData.DummyTime.AddDays(1);

            Debug.Log(GameData.DummyTime.ToString());

            User.Instance.OnValuesChange();
        }

        if (GUILayout.Button("Month Up"))
        {
            GameData.DummyTime = GameData.DummyTime.AddMonths(1);

            User.Instance.OnValuesChange();
        }
    }
#endif
}
