using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using System.Linq; // Додано для зручної роботи зі списками

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

    public List<string> completedQuests = new List<string>();
    private List<QuestPoint> allPoints = new List<QuestPoint>();

    [Header("UI References")]
    public TextMeshProUGUI goalText;
    public GameObject questPanel;
    public Animator uiAnimator;

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
        }
    }

    // --- ДОПОМІЖНІ МЕТОДИ ---

    // Очищає ім'я від (Clone) та пробілів для надійного порівняння
    private string CleanName(string rawName)
    {
        if (string.IsNullOrEmpty(rawName)) return "";
        return rawName.Replace("(Clone)", "").Trim();
    }

    // Глобальна перевірка: чи виконаний квест? (Викликати з QuestGiver)
    public bool IsQuestCompleted(string questName)
    {
        if (string.IsNullOrWhiteSpace(questName)) return false;
        if (completedQuests == null) return false;

        // "Бульдозер": залишаємо виключно букви і цифри, все переводимо в нижній регістр
        string search = new string(questName.ToLower().Where(char.IsLetterOrDigit).ToArray());

        bool isFound = false;

        string debugMsg = $"<color=cyan>[QuestManager ПЕРЕВІРКА]</color> Шукаємо квест: <b>[{search}]</b>. У списку {completedQuests.Count} квестів:\n";

        foreach (string q in completedQuests)
        {
            if (string.IsNullOrWhiteSpace(q)) continue;

            // Застосовуємо таку ж жорстку очистку до елементів у списку
            string cleanedQ = new string(q.ToLower().Where(char.IsLetterOrDigit).ToArray());
            debugMsg += $" - В списку: <b>[{cleanedQ}]</b> (Оригінал: '{q}')\n";

            if (cleanedQ == search)
            {
                isFound = true;
            }
        }

        debugMsg += $"Результат пошуку: <color={(isFound ? "green>ЗНАЙДЕНО</color>" : "red>НЕ ЗНАЙДЕНО</color>")}";
        Debug.Log(debugMsg);

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

        if (questPanel != null) questPanel.SetActive(true);
        if (uiAnimator != null) uiAnimator.SetTrigger("Appear");

        UpdateUI();
        OnQuestStateChanged?.Invoke(); // Сповіщаємо NPC
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
            UpdateUI();
            OnQuestStateChanged?.Invoke(); // Оновлюємо іконки (на випадок появи "!")

            if (currentProgress >= currentQuest.requiredAmount)
            {
                if (!currentQuest.requiresReturnToNPC)
                {
                    StartCoroutine(CompleteQuestRoutine());
                }
            }
        }
    }

    public void FinishQuestFromNPC()
    {
        if (currentQuest != null && currentProgress >= currentQuest.requiredAmount)
        {
            StartCoroutine(CompleteQuestRoutine());
        }
    }

    private IEnumerator CompleteQuestRoutine()
    {
        isTransitioning = true;
        QuestData completedQuest = currentQuest; // Запам'ятовуємо, що виконали

        // 1. Додаємо в список виконаних (очищене ім'я)
        string nameToAdd = CleanName(completedQuest.name);
        if (!completedQuests.Contains(nameToAdd))
        {
            completedQuests.Add(nameToAdd);
        }

        // 2. Візуальне відображення в UI
        if (goalText != null)
        {
            goalText.text = $"<s>{completedQuest.description}</s>";
            goalText.color = Color.green;
        }

        // 3. МИТТЄВО сповіщаємо NPC, щоб вони прибрали іконки "!" або "?"
        // Ми робимо це до затримки, щоб гравець бачив результат відразу
        OnQuestStateChanged?.Invoke();

        yield return new WaitForSeconds(2.0f);

        // 4. Перехід до наступного
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
            OnQuestStateChanged?.Invoke(); // Фінальне оновлення іконок
        }
    }

    void UpdateUI()
    {
        if (currentQuest != null && goalText != null)
        {
            if (currentQuest.requiresReturnToNPC && currentProgress >= currentQuest.requiredAmount)
            {
                goalText.text = returnToNPCText;
                return;
            }

            string progressInfo = currentQuest.requiredAmount > 1
                ? $" ({currentProgress}/{currentQuest.requiredAmount})"
                : "";

            goalText.text = $"{currentQuest.description}{progressInfo}";
        }
    }

    // --- РЕЄСТРАЦІЯ ТОЧОК ---
    public void RegisterPoint(QuestPoint point) { if (!allPoints.Contains(point)) allPoints.Add(point); }
    public Transform GetTargetTransform(string id) { return allPoints.FirstOrDefault(p => p != null && p.pointID == id)?.transform; }
    private void OnDestroy() { allPoints.Clear(); }

    // --- ЗБЕРЕЖЕННЯ ---
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
                if (questPanel != null) questPanel.SetActive(true);
                UpdateUI();
                OnQuestStateChanged?.Invoke();
            }
        }
    }
}