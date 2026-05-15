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
    public TextMeshProUGUI claimButtonText; // Текст на кнопці

    private int myQuestIndex; // Який це квест по рахунку (0, 1 або 2)

    public void Setup(ActiveDailyQuest quest, int index)
    {
        myQuestIndex = index;

        // Встановлюємо тексти
        titleText.text = quest.questData.questName;
        progressText.text = $"{quest.currentProgress} / {quest.questData.targetAmount}";

        // Налаштовуємо слайдер
        progressSlider.maxValue = quest.questData.targetAmount;
        progressSlider.value = quest.currentProgress;

        // Логіка кнопки "Забрати"
        claimButton.onClick.RemoveAllListeners();
        claimButton.onClick.AddListener(OnClaimClicked);

        if (quest.isRewardClaimed)
        {
            // Нагорода вже забрана
            claimButton.interactable = false;
            claimButtonText.text = "Отримано";
        }
        else if (quest.isCompleted)
        {
            // Квест виконано, чекає на клік
            claimButton.interactable = true;
            claimButtonText.text = $"Забрати {quest.questData.goldReward}";
        }
        else
        {
            // В процесі
            claimButton.interactable = false;
            claimButtonText.text = "В процесі";
        }
    }

    private void OnClaimClicked()
    {
        // Кажемо менеджеру видати нагороду
        DailyQuestManager.Instance.ClaimReward(myQuestIndex);

        // Оновлюємо цей конкретний слот, щоб кнопка стала "Отримано"
        claimButton.interactable = false;
        claimButtonText.text = "Отримано";
    }
}