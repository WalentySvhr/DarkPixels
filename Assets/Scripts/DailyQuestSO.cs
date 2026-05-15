using System.Collections.Generic;
using UnityEngine;

// Всі типи дій, які ми обговорювали
public enum DailyQuestType 
{ 
    ClearTowerFloors,   // Для "Альпініста"
    KillEnemies,        // Для "М'ясорубки"
    KillElite,          // Для "Мисливця за головами"
    SellResources,      // Для "Постачальника"
    SpendGold,          // Для "Щедрого клієнта"
    CatchFish,          // Для "Тихого полювання"
    CatchRareFish,      // Для "Великого улову"
    CompleteOtherDailies// Для "Ідеального дня" (мета-квест)
}

[CreateAssetMenu(fileName = "NewDailyQuest", menuName = "Quests/Daily Quest")]
public class DailyQuestSO : ScriptableObject
{
    public string questID; // Наприклад: "q_tower_01" (має бути унікальним!)
    public string questName;
    [TextArea] public string description;
    public DailyQuestType questType;
    public int targetAmount;
    public int goldReward;
}

// --- Класи для зберігання прогресу ---

[System.Serializable]
public class ActiveDailyQuest
{
    public DailyQuestSO questData;
    public int currentProgress;
    public bool isCompleted;
    public bool isRewardClaimed;
}

[System.Serializable]
public class DailySaveData
{
    public string lastLoginDate;
    public List<QuestSaveItem> savedQuests = new List<QuestSaveItem>();
}

[System.Serializable]
public class QuestSaveItem
{
    public string questID;
    public int currentProgress;
    public bool isCompleted;
    public bool isRewardClaimed;
}