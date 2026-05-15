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

    [Header("Button Text Settings")]
    public string textInProgress = "В процесі";
    public string textClaimReward = "Забрати {amount}";
    public string textClaimed = "Отримано";

    private int myQuestIndex;
    private bool isDescriptionOpen = false;

    public void Setup(ActiveDailyQuest quest, int index)
    {
        myQuestIndex = index;

        // Встановлюємо заголовок
        titleText.text = quest.questData.questName;

        // --- НОВЕ: Форматуємо опис ---
        // Підтягуємо опис із SO та замінюємо теги {target} і {reward} на реальні значення
        string parsedDescription = quest.questData.description
            .Replace("{target}", quest.questData.targetAmount.ToString())
            .Replace("{reward}", quest.questData.goldReward.ToString());

        descriptionText.text = parsedDescription;
        // ------------------------------

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
    }

    private void ToggleDescription()
    {
        isDescriptionOpen = !isDescriptionOpen;
        descriptionText.gameObject.SetActive(isDescriptionOpen);

        // Якщо контейнер не оновлює свій розмір автоматично, 
        // можна примусово оновити Layout:
        // LayoutRebuilder.ForceRebuildLayoutImmediate(GetComponent<RectTransform>());
    }

    private void OnClaimClicked()
    {
        DailyQuestManager.Instance.ClaimReward(myQuestIndex);

        claimButton.interactable = false;
        claimButtonText.text = textClaimed;
    }
}