using UnityEngine;

// 1. Додано новий тип: CollectItems
public enum QuestType { Talk, ReachLocation, KillSpecific, SurviveTime, KillInTower, CollectItems }

[CreateAssetMenu(fileName = "NewQuest", menuName = "Quests/StoryQuest")]
public class QuestData : ScriptableObject
{
    [Header("Загальна інформація")]
    public string questName;
    [TextArea] public string description;

    [Header("Ціль квесту")]
    public QuestType type;
    [Tooltip("ID NPC, назва зони або ім'я ворога (не використовується для збору предметів)")]
    public string targetID;

    [Tooltip("Кількість (ворогів, часу або предметів для збору)")]
    public int requiredAmount = 1;
    public int requiredTowerLevel; // Наприклад, 10
    public bool requiresReturnToNPC;

    [Header("Для квестів типу CollectItems")]
    [Tooltip("Предмет, який потрібно зібрати (сюди перетягуємо ScriptableObject шкіри, руди тощо)")]
    public Item itemToCollect;

    // Якщо тобі обов'язково потрібен саме префаб (наприклад, щоб квест сам спавнив цей предмет у світі),
    // розкоментуй цей рядок:
    // [Tooltip("Префаб предмета, який випадає в світі")]
    // public GameObject itemPrefabToDrop;

    [Header("Нагороди за квест")]
    public int goldReward;
    public int experienceReward;

    [Header("Нагороди (Предмети)")]
    public Item[] itemRewards;

    [Header("Прогресія")]
    [Tooltip("Квест, який активується автоматично після завершення цього")]
    public QuestData nextQuest;
}