using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Text;

public class QuestSlotUI : MonoBehaviour
{
    [Header("UI Елементи")]
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI descriptionText;
    public TextMeshProUGUI targetText;
    public TextMeshProUGUI rewardText; // Для тексту золота/досвіду
    public Button trackButton;

    [Header("Ручні Слоти Нагород (Іконки)")]
    [Tooltip("Перетягніть сюди готові слоти нагород, які ви розставили в префабі вручну")]
    public QuestRewardSlotUI[] manualRewardSlots;

    [Header("Налаштування кнопки стеження")]
    public TextMeshProUGUI trackButtonText;
    public string normalButtonText = "Стежити";
    public string trackingButtonText = "Стежимо";

    [Header("Кольори кнопки стеження")]
    public Color normalButtonColor = Color.white;
    public Color trackingButtonColor = Color.gray;
    public Color normalTextColor = Color.black;
    public Color trackingTextColor = Color.white;

    [Header("Налаштування Текстів (Локалізація)")]
    public string targetPrefix = "Що треба: ";
    public string talkToNpcText = "Що треба: Поговорити з NPC";
    public string rewardTemplate = "<color=#FFCC00>Нагорода:</color> {reward}";
    public string coinsFormat = "<color=#FFFF00>{amount} монет</color>";
    public string defaultRewardText = "Досвід або Повага";

    private string npcTargetID;

    void Start()
    {
        if (trackButton != null) trackButton.onClick.AddListener(OnTrackClick);
    }

    public void SetupQuestSlot(QuestData data, string npcID)
    {
        if (data == null) return;
        npcTargetID = npcID;

        // 1. Назва та опис квесту
        if (titleText != null) titleText.text = data.questName;

        if (descriptionText != null)
        {
            string finalDescription = data.description;
            if (!string.IsNullOrEmpty(finalDescription))
            {
                if (finalDescription.Contains("{amount}")) finalDescription = finalDescription.Replace("{amount}", data.requiredAmount.ToString());
                if (finalDescription.Contains("{level}")) finalDescription = finalDescription.Replace("{level}", data.requiredTowerLevel.ToString());
            }
            descriptionText.text = finalDescription;
        }

        // 2. Цілі квесту
        if (targetText != null)
        {
            targetText.text = !string.IsNullOrEmpty(data.targetID) ? targetPrefix + data.targetID : talkToNpcText;
        }

        // 3. Текстова нагорода (Золото / Досвід)
        if (rewardText != null)
        {
            StringBuilder txtBuilder = new StringBuilder();
            if (data.goldReward > 0)
            {
                txtBuilder.Append(coinsFormat.Replace("{amount}", data.goldReward.ToString()));
            }
            else
            {
                txtBuilder.Append(defaultRewardText);
            }
            rewardText.text = rewardTemplate.Replace("{reward}", txtBuilder.ToString());
        }

        // 4. ЗАПОВНЕННЯ РУЧНИХ СЛОТІВ ПРЕДМЕТАМИ
        if (manualRewardSlots != null && manualRewardSlots.Length > 0)
        {
            for (int i = 0; i < manualRewardSlots.Length; i++)
            {
                // Перевіряємо, чи є у даних квесту предмет для цього слота
                if (data.itemRewards != null && i < data.itemRewards.Length && data.itemRewards[i] != null)
                {
                    // Якщо предмет є — передаємо його в наш QuestRewardSlotUI (кількість = 1)
                    manualRewardSlots[i].SetupReward(data.itemRewards[i], 1);
                }
                else
                {
                    // Якщо предметів менше, ніж слотів — просто вимикаємо зайвий слот
                    manualRewardSlots[i].gameObject.SetActive(false);
                }
            }
        }

        // 5. Стан стеження
        bool isCurrentlyTracking = (QuestArrow.Instance != null && QuestArrow.Instance.CurrentOverrideTargetID == npcTargetID);
        SetTrackingState(isCurrentlyTracking);
    }

    void OnTrackClick()
    {
        if (string.IsNullOrEmpty(npcTargetID)) return;

        if (QuestArrow.Instance != null)
        {
            bool isAlreadyTrackingThis = (QuestArrow.Instance.CurrentOverrideTargetID == npcTargetID);

            if (isAlreadyTrackingThis)
            {
                QuestArrow.Instance.ClearOverrideTarget();
                SetTrackingState(false);
            }
            else
            {
                QuestArrow.Instance.TrackNPC(npcTargetID);
                QuestSlotUI[] allSlots = transform.parent.GetComponentsInChildren<QuestSlotUI>();
                foreach (QuestSlotUI slot in allSlots) slot.SetTrackingState(false);
                SetTrackingState(true);
            }
        }
    }

    public void SetTrackingState(bool isTracking)
    {
        if (trackButton != null)
        {
            Image buttonImage = trackButton.GetComponent<Image>();
            if (buttonImage != null) buttonImage.color = isTracking ? trackingButtonColor : normalButtonColor;
        }

        if (trackButtonText != null)
        {
            trackButtonText.text = isTracking ? trackingButtonText : normalButtonText;
            trackButtonText.color = isTracking ? trackingTextColor : normalTextColor;
        }
    }
}