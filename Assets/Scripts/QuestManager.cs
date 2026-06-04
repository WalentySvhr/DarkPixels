using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class QuestManager : MonoBehaviour
{
    public static QuestManager Instance;

    // Подія, яка сповіщає всіх NPC, що стан квестів змінився
    public System.Action OnQuestStateChanged;

    [Header("Current Quest State")]
    public QuestData currentQuest;
    public int currentProgress = 0;
    private bool isTransitioning = false;

    [Header("UI Texts")]
    public string returnToNPCText = "Повернись до NPC за нагородою";

    [Header("Логіка випадіння нагород")]
    public GameObject droppedItemPrefab; // Твій базовий префаб предмета, який лежить на землі
    public GameObject goldPopupPrefab;   // Префаб для тексту золота

    public List<string> completedQuests = new List<string>();
    private List<QuestPoint> allPoints = new List<QuestPoint>();

    [Header("Unique Drops Logic")]
    public List<string> droppedUniqueItems = new List<string>(); // Список ID предметів, що вже випали

    [Header("UI References")]
    public TextMeshProUGUI goalText;
    public GameObject questPanel;
    public Animator uiAnimator;

    // === КЕРУВАННЯ СТРІЛКОЮ ===
    [Header("Quest Arrow Reference")]
    public GameObject questArrow;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void Start()
    {
        if (currentQuest != null)
        {
            InitializeQuest(currentQuest);
        }
        else
        {
            if (questPanel != null) questPanel.SetActive(false);

            // === КЕРУВАННЯ СТРІЛКОЮ: Вимикаємо на старті, якщо квесту немає ===
            if (questArrow != null) questArrow.SetActive(false);
        }
    }

    // --- ДОПОМІЖНІ МЕТОДИ ---

    private string CleanName(string rawName)
    {
        if (string.IsNullOrEmpty(rawName)) return "";
        return rawName.Replace("(Clone)", "").Trim();
    }

    public bool IsQuestCompleted(string questName)
    {
        if (string.IsNullOrWhiteSpace(questName)) return false;
        if (completedQuests == null) return false;

        string search = new string(questName.ToLower().Where(char.IsLetterOrDigit).ToArray());
        bool isFound = false;

        foreach (string q in completedQuests)
        {
            if (string.IsNullOrWhiteSpace(q)) continue;
            string cleanedQ = new string(q.ToLower().Where(char.IsLetterOrDigit).ToArray());
            if (cleanedQ == search)
            {
                isFound = true;
            }
        }
        return isFound;
    }

    // --- ЛОГІКА КВЕСТІВ ---

    public void InitializeQuest(QuestData newQuest)
    {
        if (newQuest == null)
        {
            Debug.LogWarning("<color=red>[QuestManager]</color> Спроба ініціалізувати пустий квест (null)!");
            return;
        }

        if (IsQuestCompleted(newQuest.name))
        {
            Debug.Log($"<color=orange>[QuestManager]</color> Квест <b>{newQuest.name}</b> вже є у списку виконаних. Ініціалізація скасована.");
            return;
        }

        Debug.Log($"<color=green>[QuestManager]</color> Починаємо новий квест: <b>{newQuest.name}</b>");

        currentQuest = newQuest;
        currentProgress = 0;
        isTransitioning = false;

        // Якщо це квест на збір предметів, одразу перевіряємо інвентар
        if (currentQuest.type == QuestType.CollectItems)
        {
            UpdateCollectItemProgress();
        }

        if (questPanel != null) questPanel.SetActive(true);
        if (uiAnimator != null) uiAnimator.SetTrigger("Appear");

        // === ФІКС ЗЕЛЕНОГО ТЕКСТУ: Повертаємо тексту базовий білий колір ===
        if (goalText != null) goalText.color = Color.white;

        UpdateUI();
        OnQuestStateChanged?.Invoke(); // Сповіщаємо NPC
    }

    public void UpdateCollectItemProgress()
    {
        if (currentQuest == null || currentQuest.type != QuestType.CollectItems || isTransitioning) return;

        int itemsInInventory = 0;

        if (InventoryManager.Instance != null && currentQuest.itemToCollect != null)
        {
            itemsInInventory = InventoryManager.Instance.GetItemCount(currentQuest.itemToCollect);
        }

        currentProgress = itemsInInventory;

        if (currentProgress > currentQuest.requiredAmount)
        {
            currentProgress = currentQuest.requiredAmount;
        }

        UpdateUI();
        OnQuestStateChanged?.Invoke();

        if (currentProgress >= currentQuest.requiredAmount && !currentQuest.requiresReturnToNPC)
        {
            Debug.Log("<color=green>Квест на збір виконано автоматично!</color>");
            GiveQuestRewards();
            StartCoroutine(CompleteQuestRoutine());
        }
    }

    public void OnQuestAction(QuestType actionType, string id, int amount = 1)
    {
        if (currentQuest == null || isTransitioning) return;

        bool isTargetMatch = string.IsNullOrEmpty(currentQuest.targetID) || currentQuest.targetID == id;

        if (currentQuest.type == actionType && isTargetMatch)
        {
            if (currentQuest.requiredTowerLevel > 0)
            {
                if (TowerManager.Instance.currentFloor != currentQuest.requiredTowerLevel) return;
            }

            currentProgress += amount;

            if (currentProgress > currentQuest.requiredAmount)
            {
                currentProgress = currentQuest.requiredAmount;
            }

            UpdateUI();
            OnQuestStateChanged?.Invoke();

            if (currentProgress >= currentQuest.requiredAmount)
            {
                if (!currentQuest.requiresReturnToNPC)
                {
                    Debug.Log("<color=green>Квест виконано автоматично (у полі)!</color>");
                    GiveQuestRewards();
                    StartCoroutine(CompleteQuestRoutine());
                }
            }
        }
    }

    public void FinishQuestFromNPC()
    {
        if (currentQuest != null && currentProgress >= currentQuest.requiredAmount)
        {
            GiveQuestRewards();
            StartCoroutine(CompleteQuestRoutine());
        }
    }

    // === НОВИЙ МЕТОД: СКАСУВАННЯ ПОТОЧНОГО КВЕСТУ ===
    public void CancelCurrentQuest()
    {
        // Не дозволяємо скасовувати, якщо квесту немає або він зараз у процесі завершення
        if (currentQuest == null || isTransitioning) return;

        Debug.Log($"<color=orange>[QuestManager]</color> Квест <b>{currentQuest.name}</b> скасовано гравцем.");

        // Очищаємо дані квесту
        currentQuest = null;
        currentProgress = 0;

        // Ховаємо інтерфейс квесту
        if (questPanel != null) questPanel.SetActive(false);

        // Вимикаємо стрілку навігації
        if (questArrow != null) questArrow.SetActive(false);

        // Сповіщаємо NPC, щоб вони оновили свої іконки (квест знову стане доступним для взяття)
        OnQuestStateChanged?.Invoke();
    }

    private void GiveQuestRewards()
    {
        if (currentQuest == null) return;

        if (currentQuest.goldReward > 0 && InventoryManager.Instance != null)
        {
            InventoryManager.Instance.ChangeCoins(currentQuest.goldReward);
            ShowGoldPopup(currentQuest.goldReward);
            Debug.Log($"<color=yellow>Нагорода: Отримано {currentQuest.goldReward} монет!</color>");
        }

        if (currentQuest.experienceReward > 0)
        {
            Debug.Log($"<color=cyan>Нагорода: Отримано {currentQuest.experienceReward} XP!</color>");
        }

        if (currentQuest.itemRewards != null && currentQuest.itemRewards.Length > 0)
        {
            foreach (Item rewardItem in currentQuest.itemRewards)
            {
                if (rewardItem != null)
                {
                    DropItemOnGround(rewardItem);
                }
            }
        }
    }

    private void ShowGoldPopup(int amount)
    {
        if (goldPopupPrefab == null) return;

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        Vector3 spawnPos = player != null ? player.transform.position : transform.position;
        spawnPos.y += 1.0f;

        spawnPos.x += UnityEngine.Random.Range(-0.3f, 0.3f);
        GameObject popup = Instantiate(goldPopupPrefab, spawnPos, Quaternion.identity);

        GoldPopup script = popup.GetComponent<GoldPopup>();
        if (script != null)
        {
            script.Setup(amount);
        }
    }

    public void DropItemOnGround(Item itemData)
    {
        if (droppedItemPrefab == null)
        {
            Debug.LogError("<color=red>[QuestManager]</color> Не призначено droppedItemPrefab в Інспекторі! Предмет втрачено.");
            return;
        }

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        Vector3 dropPosition = player != null ? player.transform.position : transform.position;

        GameObject droppedItem = Instantiate(droppedItemPrefab, dropPosition, Quaternion.identity);

        ItemPickup pickupScript = droppedItem.GetComponent<ItemPickup>();
        if (pickupScript != null)
        {
            pickupScript.item = itemData;
        }

        SpriteRenderer sr = droppedItem.GetComponentInChildren<SpriteRenderer>();
        if (sr != null && itemData.icon != null)
        {
            sr.sprite = itemData.icon;
        }
    }

    private IEnumerator CompleteQuestRoutine()
    {
        isTransitioning = true;
        QuestData completedQuest = currentQuest;

        string nameToAdd = CleanName(completedQuest.name);
        if (!completedQuests.Contains(nameToAdd))
        {
            completedQuests.Add(nameToAdd);
        }

        if (goalText != null)
        {
            goalText.text = $"<s>{completedQuest.description}</s>";
            goalText.color = Color.green;
        }

        OnQuestStateChanged?.Invoke();

        yield return new WaitForSeconds(2.0f);

        QuestData next = completedQuest.nextQuest;
        if (next != null)
        {
            if (goalText != null) goalText.color = Color.white;
            InitializeQuest(next);
        }
        else
        {
            currentQuest = null;
            if (questPanel != null) questPanel.SetActive(false);

            // === КЕРУВАННЯ СТРІЛКОЮ: Ховаємо стрілку, якщо ланцюжок квестів закінчився ===
            if (questArrow != null) questArrow.SetActive(false);

            OnQuestStateChanged?.Invoke();
        }
    }

    public bool TryDropQuestItem(string itemID)
    {
        if (currentQuest == null) return false;

        bool isNeeded = (currentQuest.targetID == itemID);

        if (isNeeded)
        {
            if (currentQuest.type == QuestType.CollectItems)
            {
                if (currentProgress < currentQuest.requiredAmount)
                {
                    return true;
                }
            }
            else
            {
                return true;
            }
        }

        return false;
    }

    // === МЕТОД ОНОВЛЕННЯ UI (ЧИСТИЙ ВІД СТАРИХ ПРАПОРЦІВ) ===
    public void UpdateUI()
    {
        if (currentQuest != null && goalText != null)
        {
            // === КЕРУВАННЯ СТРІЛКОЮ ===
            if (questArrow != null)
            {
                // Стрілка вимикається автоматично, коли прогрес досягає необхідного (наприклад, 1/1 для Reach Location)
                if (currentProgress >= currentQuest.requiredAmount)
                {
                    questArrow.SetActive(false);
                }
                else
                {
                    questArrow.SetActive(true);
                }
            }

            if (currentQuest.requiresReturnToNPC && currentProgress >= currentQuest.requiredAmount)
            {
                goalText.text = returnToNPCText;
                return;
            }

            string formattedDescription = currentQuest.description;

            if (!string.IsNullOrEmpty(formattedDescription))
            {
                formattedDescription = formattedDescription.Replace("{level}", currentQuest.requiredTowerLevel.ToString());
                formattedDescription = formattedDescription.Replace("{amount}", currentQuest.requiredAmount.ToString());

                if (!string.IsNullOrEmpty(currentQuest.targetID))
                {
                    formattedDescription = formattedDescription.Replace("{target}", currentQuest.targetID);
                }
            }

            string progressInfo = currentQuest.requiredAmount > 1
                ? $" ({currentProgress}/{currentQuest.requiredAmount})"
                : "";

            goalText.text = $"{formattedDescription}{progressInfo}";
        }
    }

    public void RegisterPoint(QuestPoint point) { if (!allPoints.Contains(point)) allPoints.Add(point); }
    public Transform GetTargetTransform(string id) { return allPoints.FirstOrDefault(p => p != null && p.pointID == id)?.transform; }
    private void OnDestroy() { allPoints.Clear(); }

    public GameData CaptureQuestState(GameData data)
    {
        data.currentQuestID = currentQuest != null ? CleanName(currentQuest.name) : "";
        data.questProgress = currentProgress;
        data.completedQuestIDs = new List<string>(completedQuests);
        return data;
    }

    public void LoadQuestState(GameData data)
    {
        completedQuests = new List<string>(data.completedQuestIDs);
        if (!string.IsNullOrEmpty(data.currentQuestID))
        {
            QuestData loadedQuest = Resources.Load<QuestData>("Quests/" + data.currentQuestID);
            if (loadedQuest != null)
            {
                currentQuest = loadedQuest;
                currentProgress = data.questProgress;
                isTransitioning = false;

                if (currentQuest.type == QuestType.CollectItems)
                {
                    UpdateCollectItemProgress();
                }

                if (questPanel != null) questPanel.SetActive(true);
                UpdateUI();
                OnQuestStateChanged?.Invoke();
            }
        }
    }
}