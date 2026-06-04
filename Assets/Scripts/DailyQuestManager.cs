using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

// --- СТРУКТУРА ДЛЯ ІНСПЕКТОРУ (ЯК У СЮЖЕТНИХ КВЕСТАХ) ---
[System.Serializable]
public class DailyQuestConfig
{
    public DailyQuestSO questData;
    public bool canBeTracked;
    [Tooltip("ID елітного моба, назва NPC чи маркер зони (наприклад: AlchemistNPC)")]
    public string targetID;
}

public class DailyQuestManager : MonoBehaviour
{
    public static DailyQuestManager Instance { get; private set; }

    // ПОДІЯ: викликається щоразу, коли змінюється стан дейліків (прогрес, новий день, вибір стеження)
    public event Action OnDailyQuestsChanged;

    [Header("Quest Database")]
    [Tooltip("Налаштуй квести та їхні цілі прямо тут, як у твоїх сюжетних квестах!")]
    public List<DailyQuestConfig> allPossibleDailies = new List<DailyQuestConfig>();

    [Header("Current State")]
    public List<ActiveDailyQuest> activeDailies = new List<ActiveDailyQuest>();

    // === СТАН СТЕЖЕННЯ ЗА КВЕСТОМ ===
    [HideInInspector]
    public int trackedDailyIndex = -1; // -1 означає, що жоден дейлік зараз не відстежується

    // Внутрішній список для зберігання всіх точок дейліків на поточній сцені
    private List<DailyQuestPoint> registeredDailyPoints = new List<DailyQuestPoint>();

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

                    // Якщо квест, який виковали, зараз відстежувався — вимикаємо трекінг
                    int index = activeDailies.IndexOf(quest);
                    if (trackedDailyIndex == index)
                    {
                        SetTrackedDaily(index); // Перемикач вимкне його, бо індекси збігаються
                    }

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

            // Сповіщаємо інтерфейс про зміни через подію
            OnDailyQuestsChanged?.Invoke();

            if (DailyQuestButtonUI.Instance != null)
            {
                DailyQuestButtonUI.Instance.RefreshButtonState();
            }
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

            if (InventoryManager.Instance != null)
            {
                InventoryManager.Instance.ChangeCoins(quest.questData.goldReward);
                Debug.Log($"Отримано нагороду: {quest.questData.goldReward} монет за {quest.questData.questName}");
            }
            else
            {
                Debug.LogError("InventoryManager.Instance не знайдено! Нагороду не видано.");
            }

            SaveDailiesProgress();

            // Сповіщаємо інтерфейс про зміни через подію
            OnDailyQuestsChanged?.Invoke();

            if (DailyQuestButtonUI.Instance != null)
            {
                DailyQuestButtonUI.Instance.RefreshButtonState();
            }
        }
    }

    // КЕРУВАННЯ СТЕЖЕННЯМ ЗА ДЕЙЛІКАМИ ЗА ID ЦІЛІ
    public void SetTrackedDaily(int index)
    {
        if (trackedDailyIndex == index)
        {
            trackedDailyIndex = -1;
            Debug.Log("[DailyQuest] Стеження вимкнено");
        }
        else
        {
            trackedDailyIndex = index;
            var targetQuest = activeDailies[index];
            string questTargetID = targetQuest.targetID;
            Debug.Log($"[DailyQuest] Тепер стежимо за: {targetQuest.questData.questName}. Шукаємо об'єкт з ID: {questTargetID}");

            // === ОЦЕЙ ЗВ'ЯЗОК ОЖИВИТЬ СТРІЛКУ МИТТЄВО ===
            if (QuestArrow.Instance != null)
            {
                // Скидаємо ручний сюжетний фокус, бо дейліки мають зараз вищий пріоритет
                QuestArrow.Instance.ClearOverrideTarget();
                // Пробуджуємо стрілку в ієрархії
                QuestArrow.Instance.gameObject.SetActive(true);
            }
        }

        OnDailyQuestsChanged?.Invoke();
    }

    private void GenerateNewDailies()
    {
        trackedDailyIndex = -1;
        activeDailies.Clear();

        List<DailyQuestConfig> availableQuests = new List<DailyQuestConfig>(allPossibleDailies);

        for (int i = 0; i < 3; i++)
        {
            if (availableQuests.Count == 0) break;

            int randomIndex = UnityEngine.Random.Range(0, availableQuests.Count);
            DailyQuestConfig selected = availableQuests[randomIndex];

            activeDailies.Add(new ActiveDailyQuest
            {
                questData = selected.questData,
                canBeTracked = selected.canBeTracked,
                targetID = selected.targetID,
                currentProgress = 0,
                isCompleted = false,
                isRewardClaimed = false
            });

            availableQuests.RemoveAt(randomIndex);
        }

        Debug.Log("Згенеровано нові щоденні квести!");
        OnDailyQuestsChanged?.Invoke();
    }

    private void SaveDailiesProgress()
    {
        DailySaveData saveData = new DailySaveData { lastLoginDate = DateTime.Now.ToString("yyyy-MM-dd") };

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
            DailyQuestConfig originalConfig = allPossibleDailies.Find(q => q.questData != null && q.questData.questID == savedQuest.questID);

            if (originalConfig != null)
            {
                activeDailies.Add(new ActiveDailyQuest
                {
                    questData = originalConfig.questData,
                    canBeTracked = originalConfig.canBeTracked,
                    targetID = originalConfig.targetID,
                    currentProgress = savedQuest.currentProgress,
                    isCompleted = savedQuest.isCompleted,
                    isRewardClaimed = savedQuest.isRewardClaimed
                });
            }
        }

        OnDailyQuestsChanged?.Invoke();

        if (DailyQuestButtonUI.Instance != null)
        {
            DailyQuestButtonUI.Instance.RefreshButtonState();
        }
    }

    public bool HasUnclaimedRewards()
    {
        foreach (var quest in activeDailies)
        {
            if (quest.isCompleted && !quest.isRewardClaimed) return true;
        }
        return false;
    }

    // СИСТЕМА НАВІГАЦІЇ ТА РЕЄСТРАЦІЇ ТОЧОК ДЕЙЛІКІВ
    public void RegisterDailyPoint(DailyQuestPoint point)
    {
        if (!registeredDailyPoints.Contains(point)) registeredDailyPoints.Add(point);
    }

    public void UnregisterDailyPoint(DailyQuestPoint point)
    {
        if (registeredDailyPoints.Contains(point)) registeredDailyPoints.Remove(point);
    }

    public Transform GetTrackedTargetTransform()
    {
        if (trackedDailyIndex == -1 || trackedDailyIndex >= activeDailies.Count) return null;

        string activeTargetID = activeDailies[trackedDailyIndex].targetID;
        if (string.IsNullOrEmpty(activeTargetID)) return null;

        DailyQuestPoint targetPoint = registeredDailyPoints.Find(p => p.targetID == activeTargetID);
        return targetPoint != null ? targetPoint.transform : null;
    }
}