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

        // Встановлюємо заголовок
        titleText.text = quest.questData.questName;

        // Форматуємо опис
        string rawDescription = quest.questData.description ?? "";
        string parsedDescription = rawDescription
            .Replace("{target}", quest.questData.targetAmount.ToString())
            .Replace("{reward}", quest.questData.goldReward.ToString());

        descriptionText.text = parsedDescription;

        // Встановлюємо текст прогресу
        progressText.text = $"{quest.currentProgress} / {quest.questData.targetAmount}";

        // Налаштовуємо слайдер
        progressSlider.maxValue = quest.questData.targetAmount;
        progressSlider.value = quest.currentProgress;

        // Завжди ховаємо опис при першому відкритті
        isDescriptionOpen = false;
        descriptionText.gameObject.SetActive(false);

        // Налаштовуємо кнопку заголовка (для відкриття/закриття опису)
        if (titleButton != null)
        {
            titleButton.onClick.RemoveAllListeners();
            titleButton.onClick.AddListener(ToggleDescription);
        }

        // Логіка кнопки "Забрати"
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

        // === ВИПРАВЛЕНО: РОЗУМНА ЛОГІКА КНОПКИ СТЕЖЕННЯ ===
        if (trackButton != null)
        {
            // Тепер беремо canBeTracked напряму з ActiveDailyQuest сесії, а не з Scriptable Object
            bool isTrackable = quest.canBeTracked && !quest.isCompleted;

            if (isTrackable)
            {
                trackButton.gameObject.SetActive(true);
                trackButton.onClick.RemoveAllListeners();
                trackButton.onClick.AddListener(OnTrackClicked);

                // Перевіряємо через менеджер, чи саме цей квест зараз відстежується
                if (DailyQuestManager.Instance.trackedDailyIndex == myQuestIndex)
                {
                    trackButtonText.text = textTracking;
                }
                else
                {
                    trackButtonText.text = textTrack;
                }
            }
            else
            {
                // Якщо квест не передбачає трекінгу або вже виконаний — ховаємо кнопку
                trackButton.gameObject.SetActive(false);
            }
        }
    }

    private void ToggleDescription()
    {
        if (descriptionText == null) return;

        isDescriptionOpen = !isDescriptionOpen;
        descriptionText.gameObject.SetActive(isDescriptionOpen);

        Canvas.ForceUpdateCanvases();

        if (rectTransform != null)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(rectTransform);
        }

        if (transform.parent != null && transform.parent is RectTransform parentRect)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(parentRect);
        }
    }

    private void OnClaimClicked()
    {
        DailyQuestManager.Instance.ClaimReward(myQuestIndex);
        claimButton.interactable = false;
        claimButtonText.text = textClaimed;

        // Якщо квест здано, ховаємо кнопку стеження
        if (trackButton != null) trackButton.gameObject.SetActive(false);
    }

    private void OnTrackClicked()
    {
        if (DailyQuestManager.Instance == null) return;

        // Передаємо команду менеджеру встановити цей квест як активний для трекінгу
        DailyQuestManager.Instance.SetTrackedDaily(myQuestIndex);
    }
}