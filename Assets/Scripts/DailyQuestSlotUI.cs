using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using System.Collections;

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

    [Header("Tracking Component Colors")]
    public Color normalButtonColor = Color.white;
    public Color trackingButtonColor = Color.gray;

    // НОВІ ЗМІННІ ДЛЯ КОЛЬОРУ ТЕКСТУ
    public Color normalTextColor = Color.black;
    public Color trackingTextColor = Color.white;

    [Header("Button Text Settings")]
    public string textInProgress = "В процесі";
    public string textClaimReward = "Забрати {amount}";
    public string textClaimed = "Отримано";

    private int myQuestIndex;
    private bool isDescriptionOpen = true; // РЕВЕРС: Тепер за замовчуванням TRUE
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

        // РЕВЕРС: Опис відкритий відразу при створенні/оновленні карти квесту
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

        // Оновлюємо розміри інтерфейсу, щоб відкритий текст не вилазив за межі контейнера при спавні
        RebuildUI();
    }

    public void SetTrackingState(bool isTracking)
    {
        if (trackButton != null)
        {
            // Зміна кольору самої кнопки (фон)
            Image buttonImage = trackButton.GetComponent<Image>();
            if (buttonImage != null)
            {
                buttonImage.color = isTracking ? trackingButtonColor : normalButtonColor;
            }

            // Зміна тексту та кольору тексту кнопки
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

    // Виніс оновлення інтерфейсу в окремий метод, щоб викликати його і при старті, і при кліках
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