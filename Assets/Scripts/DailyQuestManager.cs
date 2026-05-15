using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class DailyQuestManager : MonoBehaviour
{
    public static DailyQuestManager Instance { get; private set; }

    [Header("Quest Database")]
    [Tooltip("Перетягни сюди всі створені ScriptableObjects квестів")]
    public List<DailyQuestSO> allPossibleDailies;

    [Header("Current State")]
    public List<ActiveDailyQuest> activeDailies = new List<ActiveDailyQuest>();

    private string SavePath => Application.persistentDataPath + "/daily_quests.json";

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        LoadDailiesProgress();
    }

    // Додавання прогресу (викликай цей метод з інших скриптів)
    public void AddProgress(DailyQuestType type, int amount = 1)
    {
        bool progressChanged = false;

        foreach (var quest in activeDailies)
        {
            if (!quest.isCompleted && quest.questData.questType == type)
            {
                quest.currentProgress += amount;
                progressChanged = true;

                if (quest.currentProgress >= quest.questData.targetAmount)
                {
                    quest.currentProgress = quest.questData.targetAmount;
                    quest.isCompleted = true;
                    Debug.Log($"[DailyQuest] Виконано: {quest.questData.questName}");

                    // Якщо це не мета-квест, додаємо прогрес до мета-квесту "Ідеальний день"
                    if (type != DailyQuestType.CompleteOtherDailies)
                    {
                        AddProgress(DailyQuestType.CompleteOtherDailies, 1);
                    }
                }
            }
        }

        if (progressChanged)
        {
            SaveDailiesProgress();
            // Тут можна викликати оновлення UI: DailyUI.UpdateUI();
        }
    }

    // Забрати нагороду (викликається кнопкою в UI)

    public void ClaimReward(int questIndex)
    {
        if (questIndex < 0 || questIndex >= activeDailies.Count) return;

        var quest = activeDailies[questIndex];

        if (quest.isCompleted && !quest.isRewardClaimed)
        {
            quest.isRewardClaimed = true;

            // --- ДОДАЄМО МОНЕТИ ГРАВЦЮ ---
            if (InventoryManager.Instance != null)
            {
                // Викликаємо твій метод ChangeCoins і передаємо суму нагороди з SO
                InventoryManager.Instance.ChangeCoins(quest.questData.goldReward);
                Debug.Log($"Отримано нагороду: {quest.questData.goldReward} монет за {quest.questData.questName}");
            }
            else
            {
                Debug.LogError("InventoryManager.Instance не знайдено! Нагороду не видано.");
            }
            // ------------------------------

            SaveDailiesProgress();
        }
    }

    private void GenerateNewDailies()
    {
        activeDailies.Clear();

        // Щоб квести не повторювались, створюємо тимчасовий список
        List<DailyQuestSO> availableQuests = new List<DailyQuestSO>(allPossibleDailies);

        // Вибираємо 3 випадкові квести
        for (int i = 0; i < 3; i++)
        {
            if (availableQuests.Count == 0) break;

            int randomIndex = UnityEngine.Random.Range(0, availableQuests.Count);
            DailyQuestSO selectedQuest = availableQuests[randomIndex];

            activeDailies.Add(new ActiveDailyQuest
            {
                questData = selectedQuest,
                currentProgress = 0,
                isCompleted = false,
                isRewardClaimed = false
            });

            // Видаляємо вибраний, щоб не дублювався
            availableQuests.RemoveAt(randomIndex);
        }

        Debug.Log("Згенеровано нові щоденні квести!");
    }

    private void SaveDailiesProgress()
    {
        DailySaveData saveData = new DailySaveData
        {
            lastLoginDate = DateTime.Now.ToString("yyyy-MM-dd")
        };

        foreach (var quest in activeDailies)
        {
            saveData.savedQuests.Add(new QuestSaveItem
            {
                questID = quest.questData.questID,
                currentProgress = quest.currentProgress,
                isCompleted = quest.isCompleted,
                isRewardClaimed = quest.isRewardClaimed
            });
        }

        string json = JsonUtility.ToJson(saveData, true);
        File.WriteAllText(SavePath, json);
    }

    private void LoadDailiesProgress()
    {
        if (!File.Exists(SavePath))
        {
            GenerateNewDailies();
            SaveDailiesProgress();
            return;
        }

        string json = File.ReadAllText(SavePath);
        DailySaveData loadedData = JsonUtility.FromJson<DailySaveData>(json);

        string today = DateTime.Now.ToString("yyyy-MM-dd");
        if (loadedData.lastLoginDate != today)
        {
            GenerateNewDailies();
            SaveDailiesProgress();
            return;
        }

        activeDailies.Clear();
        foreach (var savedQuest in loadedData.savedQuests)
        {
            DailyQuestSO originalQuestSO = allPossibleDailies.Find(q => q.questID == savedQuest.questID);

            if (originalQuestSO != null)
            {
                activeDailies.Add(new ActiveDailyQuest
                {
                    questData = originalQuestSO,
                    currentProgress = savedQuest.currentProgress,
                    isCompleted = savedQuest.isCompleted,
                    isRewardClaimed = savedQuest.isRewardClaimed
                });
            }
        }
    }
}