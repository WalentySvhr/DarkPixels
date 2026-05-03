using UnityEngine;

// 1. Перерахування типів квестів (публічне, щоб інші скрипти його бачили)
public enum QuestType { Talk, ReachLocation, KillSpecific, SurviveTime, KillInTower }

[CreateAssetMenu(fileName = "NewQuest", menuName = "Quests/StoryQuest")]
public class QuestData : ScriptableObject
{
    [Header("Загальна інформація")]
    public string questName;
    [TextArea] public string description;

    [Header("Ціль квесту")]
    public QuestType type;
    [Tooltip("ID NPC, назва зони або ім'я ворога")]
    public string targetID;
    public int requiredAmount = 1;
    // У твій ScriptableObject QuestData додай:
    public int requiredTowerLevel; // Наприклад, 10
    public bool requiresReturnToNPC;

    [Header("Прогресія")]
    [Tooltip("Квест, який активується автоматично після завершення цього")]
    public QuestData nextQuest; // Тепер це поле всередині класу
}