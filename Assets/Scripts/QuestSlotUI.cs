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
    [Tooltip("Колір кнопки у звичайному стані")]
    public Color normalButtonColor = Color.white;
    [Tooltip("Колір кнопки, коли цей квест уже відстежується")]
    public Color trackingButtonColor = Color.gray;

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

        // 1. Заповнюємо Назву, Опис та Цілі
        if (titleText != null) titleText.text = data.questName;
        if (descriptionText != null) descriptionText.text = data.description;

        if (targetText != null)
        {
            targetText.text = !string.IsNullOrEmpty(data.targetID) ? targetPrefix + data.targetID : talkToNpcText;
        }

        // 2. Заповнюємо Нагороди
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
                if (hasAnyReward) rewardBuilder.Append(" + ");
                for (int i = 0; i < data.itemRewards.Length; i++)
                {
                    if (data.itemRewards[i] != null)
                    {
                        // === ФІКС ТУТ: беремо itemName, якщо він є, інакше назву файлу ===
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

        // === ОНОВЛЕНО: Перевіряємо при створенні, чи саме цей квест зараз відстежується ===
        bool isCurrentlyTracking = (QuestArrow.Instance != null && QuestArrow.Instance.CurrentOverrideTargetID == npcTargetID);
        SetTrackingState(isCurrentlyTracking);
    }

    void OnTrackClick()
    {
        if (string.IsNullOrEmpty(npcTargetID)) return;

        if (QuestArrow.Instance != null)
        {
            // Перевіряємо, чи цей конкретний квест ВЖЕ відстежується прямо зараз
            bool isAlreadyTrackingThis = (QuestArrow.Instance.CurrentOverrideTargetID == npcTargetID);

            if (isAlreadyTrackingThis)
            {
                // === СЦЕНАРІЙ А: Квест уже відстежувався ➡️ НАЖАЛИ ПОВТОРНО ➡️ ВИМИКАЄМО ===
                QuestArrow.Instance.ClearOverrideTarget();
                SetTrackingState(false); // Повертаємо кнопці звичайний колір і текст "Стежити"
            }
            else
            {
                // === СЦЕНАРІЙ Б: Квест не відстежувався ➡️ ВМИКАЄМО СТЕЖЕННЯ ===
                QuestArrow.Instance.TrackNPC(npcTargetID);

                // Скидаємо візуал УСІХ інших кнопок у списку в нормальний стан
                QuestSlotUI[] allSlots = transform.parent.GetComponentsInChildren<QuestSlotUI>();
                foreach (QuestSlotUI slot in allSlots)
                {
                    slot.SetTrackingState(false);
                }

                // А цю конкретну кнопку робимо активною ("Стежимо" + сірий колір)
                SetTrackingState(true);
            }
        }
    }

    // Допоміжний метод, який змінює колір та текст кнопці
    public void SetTrackingState(bool isTracking)
    {
        // Міняємо колір самої плашки кнопки (Image)
        if (trackButton != null)
        {
            Image buttonImage = trackButton.GetComponent<Image>();
            if (buttonImage != null)
            {
                buttonImage.color = isTracking ? trackingButtonColor : normalButtonColor;
            }
        }

        // Міняємо текст всередині кнопки
        if (trackButtonText != null)
        {
            trackButtonText.text = isTracking ? trackingButtonText : normalButtonText;
        }
    }
}