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
    public TextMeshProUGUI rewardText;
    public Button trackButton;

    [Header("Налаштування кнопки стеження")]
    [Tooltip("Перетягни сюди ТЕКСТ, який знаходиться ВСЕРЕДИНІ кнопки стеження")]
    public TextMeshProUGUI trackButtonText;
    [Tooltip("Текст кнопки у звичайному стані")]
    public string normalButtonText = "Стежити";
    [Tooltip("Текст кнопки, коли цей квест уже відстежується")]
    public string trackingButtonText = "Стежимо";

    [Header("Кольори кнопки стеження")]
    [Tooltip("Колір кнопки у звичайному стані")]
    public Color normalButtonColor = Color.white;
    [Tooltip("Колір кнопки, коли цей квест уже відстежується")]
    public Color trackingButtonColor = Color.gray;

    // НОВІ ЗМІННІ ДЛЯ КОЛЬОРУ ТЕКСТУ КНОПКИ
    [Tooltip("Колір тексту у звичайному стані")]
    public Color normalTextColor = Color.black;
    [Tooltip("Колір тексту, коли цей квест уже відстежується")]
    public Color trackingTextColor = Color.white;

    [Header("Налаштування Текстів (Локалізація)")]
    public string targetPrefix = "Що треба: ";
    public string talkToNpcText = "Що треба: Поговорити з NPC";
    public string rewardPrefix = "Нагорода: ";
    public string coinsSuffix = " монет";
    public string defaultRewardText = "Досвід або Повага";

    private string npcTargetID;

    void Start()
    {
        if (trackButton != null)
        {
            trackButton.onClick.AddListener(OnTrackClick);
        }
    }

    public void SetupQuestSlot(QuestData data, string npcID)
    {
        if (data == null) return;

        npcTargetID = npcID;

        // 1. Заповнюємо Назву
        if (titleText != null) titleText.text = data.questName;

        // === ОНОВЛЕНО: Динамічна заміна шаблонів {amount} та {level} в описі ===
        if (descriptionText != null)
        {
            string finalDescription = data.description;

            if (!string.IsNullOrEmpty(finalDescription))
            {
                // Замінюємо {amount} на реальну кількість із QuestData
                if (finalDescription.Contains("{amount}"))
                {
                    finalDescription = finalDescription.Replace("{amount}", data.requiredAmount.ToString());
                }

                // Замінюємо {level} на потрібний рівень вежі із QuestData
                if (finalDescription.Contains("{level}"))
                {
                    finalDescription = finalDescription.Replace("{level}", data.requiredTowerLevel.ToString());
                }
            }

            descriptionText.text = finalDescription;
        }

        // 2. Заповнюємо Цілі квесту
        if (targetText != null)
        {
            targetText.text = !string.IsNullOrEmpty(data.targetID) ? targetPrefix + data.targetID : talkToNpcText;
        }

        // 3. Заповнюємо Нагороди (з урахуванням гарного імені предмета)
        if (rewardText != null)
        {
            StringBuilder rewardBuilder = new StringBuilder(rewardPrefix);
            bool hasAnyReward = false;

            if (data.goldReward > 0)
            {
                rewardBuilder.Append(data.goldReward).Append(coinsSuffix);
                hasAnyReward = true;
            }

            if (data.itemRewards != null && data.itemRewards.Length > 0)
            {
                if (hasAnyReward) rewardBuilder.Append(" +  ");
                for (int i = 0; i < data.itemRewards.Length; i++)
                {
                    if (data.itemRewards[i] != null)
                    {
                        // Беремо красиве ім'я предмета, якщо воно є, інакше назву файлу
                        string itemDisplayName = !string.IsNullOrEmpty(data.itemRewards[i].itemName)
                            ? data.itemRewards[i].itemName
                            : data.itemRewards[i].name;

                        rewardBuilder.Append(itemDisplayName);

                        if (i < data.itemRewards.Length - 1) rewardBuilder.Append(", ");
                        hasAnyReward = true;
                    }
                }
            }

            if (!hasAnyReward) rewardBuilder.Append(defaultRewardText);
            rewardText.text = rewardBuilder.ToString();
        }

        // === Перевіряємо при створенні, чи саме цей квест зараз відстежується ===
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
                foreach (QuestSlotUI slot in allSlots)
                {
                    slot.SetTrackingState(false);
                }

                SetTrackingState(true);
            }
        }
    }

    public void SetTrackingState(bool isTracking)
    {
        // Зміна кольору фону кнопки
        if (trackButton != null)
        {
            Image buttonImage = trackButton.GetComponent<Image>();
            if (buttonImage != null)
            {
                buttonImage.color = isTracking ? trackingButtonColor : normalButtonColor;
            }
        }

        // Зміна тексту та кольору тексту всередині кнопки
        if (trackButtonText != null)
        {
            trackButtonText.text = isTracking ? trackingButtonText : normalButtonText;
            trackButtonText.color = isTracking ? trackingTextColor : normalTextColor;
        }
    }
}