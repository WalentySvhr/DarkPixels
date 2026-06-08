using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DailyQuestSlotUI : MonoBehaviour
{
    [Header("UI Elements")]
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI progressText;
    public Slider progressSlider;
    public Button claimButton;
    public TextMeshProUGUI claimButtonText;

    // НОВЕ ПОЛЕ: Окремий текст для відображення нагороди
    [Tooltip("Текст, де буде відображатися нагорода за квест")]
    public TextMeshProUGUI rewardText;

    [Header("Description Settings")]
    [Tooltip("Кнопка, яка висить на заголовку квесту")]
    public Button titleButton;
    [Tooltip("Текст опису квесту")]
    public TextMeshProUGUI descriptionText;

    [Header("Tracking Settings")]
    public Button trackButton;
    public TextMeshProUGUI trackButtonText;
    public string textTrack = "Стежити";
    public string textTracking = "Стежиться";

    [Header("Tracking Component Colors")]
    public Color normalButtonColor = Color.white;
    public Color trackingButtonColor = Color.gray;

    [Header("Tracking Text Colors")]
    public Color normalTextColor = Color.black;
    public Color trackingTextColor = Color.white;

    [Header("Button Text Settings")]
    public string textInProgress = "В процесі";
    public string textClaimReward = "Забрати";
    public string textClaimed = "Отримано";

    [Header("Reward & Localization Settings")]
    [Tooltip("Шаблон виведення нагороди. Можна використовувати TMP теги, наприклад:\n<color=#FFCC00>Нагорода:</color> {reward}")]
    public string rewardTemplate = "<color=#FFCC00>Нагорода:</color> {reward}";
    [Tooltip("Формат виведення кількості монет")]
    public string coinsFormat = "<color=#FFFF00>{amount} монет</color>";
    public string defaultRewardText = "Досвід";

    private int myQuestIndex;
    private bool isDescriptionOpen = true;
    private RectTransform rectTransform;
    private ActiveDailyQuest myQuest;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    public void Setup(ActiveDailyQuest quest, int index)
    {
        if (quest == null || quest.questData == null) return;

        myQuest = quest;
        myQuestIndex = index;

        // 1. Заповнюємо Назву
        titleText.text = quest.questData.questName;

        // 2. Заповнюємо Опис (залишаємо тільки {target}, оскільки {reward} винесено)
        string rawDescription = quest.questData.description ?? "";
        string parsedDescription = rawDescription.Replace("{target}", quest.questData.targetAmount.ToString());
        descriptionText.text = parsedDescription;

        // 3. Заповнюємо Прогрес
        progressText.text = $"{quest.currentProgress} / {quest.questData.targetAmount}";
        progressSlider.maxValue = quest.questData.targetAmount;
        progressSlider.value = quest.currentProgress;

        // === ОНОВЛЕНО: Виведення нагороди в окреме текстове поле ===
        if (rewardText != null)
        {
            string rewardContent = defaultRewardText;

            if (quest.questData.goldReward > 0)
            {
                rewardContent = coinsFormat.Replace("{amount}", quest.questData.goldReward.ToString());
            }

            // Підставляємо сформований текст нагороди у головний шаблон
            rewardText.text = rewardTemplate.Replace("{reward}", rewardContent);
        }

        // Керування відображенням опису при спавні
        isDescriptionOpen = true;
        if (descriptionText != null)
        {
            descriptionText.gameObject.SetActive(true);
        }

        if (titleButton != null)
        {
            titleButton.onClick.RemoveAllListeners();
            titleButton.onClick.AddListener(ToggleDescription);
        }

        claimButton.onClick.RemoveAllListeners();
        claimButton.onClick.AddListener(OnClaimClicked);

        // 4. Стан кнопки отримання нагороди (тепер текст чистий, без цифр)
        if (quest.isRewardClaimed)
        {
            claimButton.interactable = false;
            claimButtonText.text = textClaimed;
        }
        else if (quest.isCompleted)
        {
            claimButton.interactable = true;
            claimButtonText.text = textClaimReward;
        }
        else
        {
            claimButton.interactable = false;
            claimButtonText.text = textInProgress;
        }

        // 5. Логіка стеження
        if (trackButton != null)
        {
            bool isTrackable = quest.canBeTracked && !quest.isCompleted;

            if (isTrackable)
            {
                trackButton.gameObject.SetActive(true);
                trackButton.onClick.RemoveAllListeners();
                trackButton.onClick.AddListener(OnTrackClicked);

                bool isCurrentlyTracked = (DailyQuestManager.Instance.trackedDailyIndex == myQuestIndex);
                SetTrackingState(isCurrentlyTracked);
            }
            else
            {
                trackButton.gameObject.SetActive(false);
            }
        }

        RebuildUI();
    }

    public void SetTrackingState(bool isTracking)
    {
        if (trackButton != null)
        {
            Image buttonImage = trackButton.GetComponent<Image>();
            if (buttonImage != null)
            {
                buttonImage.color = isTracking ? trackingButtonColor : normalButtonColor;
            }

            if (trackButtonText != null)
            {
                trackButtonText.text = isTracking ? textTracking : textTrack;
                trackButtonText.color = isTracking ? trackingTextColor : normalTextColor;
            }
        }
    }

    private void ToggleDescription()
    {
        if (descriptionText == null) return;

        isDescriptionOpen = !isDescriptionOpen;
        descriptionText.gameObject.SetActive(isDescriptionOpen);

        RebuildUI();
    }

    private void RebuildUI()
    {
        Canvas.ForceUpdateCanvases();
        if (rectTransform != null)
            LayoutRebuilder.ForceRebuildLayoutImmediate(rectTransform);

        if (transform.parent != null && transform.parent is RectTransform parentRect)
            LayoutRebuilder.ForceRebuildLayoutImmediate(parentRect);
    }

    private void OnClaimClicked()
    {
        DailyQuestManager.Instance.ClaimReward(myQuestIndex);
        claimButton.interactable = false;
        claimButtonText.text = textClaimed;
        if (trackButton != null) trackButton.gameObject.SetActive(false);
    }

    private void OnTrackClicked()
    {
        if (DailyQuestManager.Instance == null) return;

        bool isAlreadyTracked = (DailyQuestManager.Instance.trackedDailyIndex == myQuestIndex);

        if (isAlreadyTracked)
        {
            DailyQuestManager.Instance.SetTrackedDaily(-1);
            SetTrackingState(false);
        }
        else
        {
            DailyQuestManager.Instance.SetTrackedDaily(myQuestIndex);

            DailyQuestSlotUI[] allSlots = transform.parent.GetComponentsInChildren<DailyQuestSlotUI>();
            foreach (DailyQuestSlotUI slot in allSlots)
            {
                slot.SetTrackingState(false);
            }
            SetTrackingState(true);
        }
    }
}
