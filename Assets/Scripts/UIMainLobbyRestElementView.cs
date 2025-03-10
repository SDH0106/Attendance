using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIMainLobbyRestElementView : MonoBehaviour
{
    [SerializeField] Sprite DefaultSlotSprite;
    [SerializeField] Sprite EventSlotSprite;

    Toggle toggle;
    ToggleGroup toggleGroup;
    Image elementSlotImage;
    TextMeshProUGUI itemName;
    GameObject selectCheckBox;
    GameObject bonusTextBox;
    Transform ItemImageGroup;

    RestPointReward reward;

    public void OnLoadComponent()
    {
        elementSlotImage = GetComponent<Image>();
        toggle = GetComponent<Toggle>();
        ItemImageGroup = transform.Find("ItemImageGroup");
        itemName = transform.Find("ItemName").GetComponent<TextMeshProUGUI>();
        selectCheckBox = transform.Find("SelectImage").gameObject;
        bonusTextBox = transform.Find("BonusText").gameObject;
        toggleGroup = transform.parent.GetComponent<ToggleGroup>();
        toggle.group = toggleGroup;
    }

    public void Initialize(RestPointReward reward)
    {
        this.reward = reward;
        bool isEvent = !string.IsNullOrEmpty(reward.eventCode);
        elementSlotImage.sprite = isEvent ? EventSlotSprite : DefaultSlotSprite;
        bonusTextBox.SetActive(isEvent);

        itemName.text = "";

        for (int i = 0; i < reward.items.Length; ++i)
        {
            Image iconImage = null;

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
                iconImage = transform.Find("ItemImage").GetComponent<Image>();
            }

            iconImage.sprite = Resources.Load<Sprite>(GameData.Instance.GetItemIconPath(reward.items[i].itemId));

            itemName.text = string.Join("\n", $"{GameData.Instance.GetItemName(reward.items[i].itemId)} x {reward.items[i].quantity}");
        }

        toggle.enabled = !isEvent;
    }

    public RestPointReward GetReward()
    {
        return reward;
    }
}
