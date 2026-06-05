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
    [Header("Tracking Colors")]
    public Color normalButtonColor = Color.white;
    public Color trackingButtonColor = Color.gray;

    [Header("Button Text Settings")]
    public string textInProgress = "В процесі";
    public string textClaimReward = "Забрати {amount}";
    public string textClaimed = "Отримано";

    private int myQuestIndex;
    private bool isDescriptionOpen = false;
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

        titleText.text = quest.questData.questName;

        string rawDescription = quest.questData.description ?? "";
        string parsedDescription = rawDescription
            .Replace("{target}", quest.questData.targetAmount.ToString())
            .Replace("{reward}", quest.questData.goldReward.ToString());

        descriptionText.text = parsedDescription;
        progressText.text = $"{quest.currentProgress} / {quest.questData.targetAmount}";
        progressSlider.maxValue = quest.questData.targetAmount;
        progressSlider.value = quest.currentProgress;

        isDescriptionOpen = false;
        descriptionText.gameObject.SetActive(false);

        if (titleButton != null)
        {
            titleButton.onClick.RemoveAllListeners();
            titleButton.onClick.AddListener(ToggleDescription);
        }

        claimButton.onClick.RemoveAllListeners();
        claimButton.onClick.AddListener(OnClaimClicked);

        if (quest.isRewardClaimed)
        {
            claimButton.interactable = false;
            claimButtonText.text = textClaimed;
        }
        else if (quest.isCompleted)
        {
            claimButton.interactable = true;
            claimButtonText.text = textClaimReward.Replace("{amount}", quest.questData.goldReward.ToString());
        }
        else
        {
            claimButton.interactable = false;
            claimButtonText.text = textInProgress;
        }

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
            trackButtonText.text = isTracking ? textTracking : textTrack;
        }
    }

    private void ToggleDescription()
    {
        if (descriptionText == null) return;

        isDescriptionOpen = !isDescriptionOpen;
        descriptionText.gameObject.SetActive(isDescriptionOpen);

        Canvas.ForceUpdateCanvases();
        if (rectTransform != null) LayoutRebuilder.ForceRebuildLayoutImmediate(rectTransform);
        if (transform.parent != null && transform.parent is RectTransform parentRect) LayoutRebuilder.ForceRebuildLayoutImmediate(parentRect);
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

        // Перевіряємо, чи цей квест вже відстежується
        bool isAlreadyTracked = (DailyQuestManager.Instance.trackedDailyIndex == myQuestIndex);

        if (isAlreadyTracked)
        {
            // Якщо вже відстежується — відміняємо (передаємо -1 або інший індикатор відміни)
            DailyQuestManager.Instance.SetTrackedDaily(-1);
            SetTrackingState(false);
        }
        else
        {
            // Якщо ні — встановлюємо цей
            DailyQuestManager.Instance.SetTrackedDaily(myQuestIndex);

            // Оновлюємо всі слоти (вимикаємо інші, включаємо цей)
            DailyQuestSlotUI[] allSlots = transform.parent.GetComponentsInChildren<DailyQuestSlotUI>();
            foreach (DailyQuestSlotUI slot in allSlots)
            {
                slot.SetTrackingState(false);
            }
            SetTrackingState(true);
        }
    }
}