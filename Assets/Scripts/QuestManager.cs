using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class QuestManager : MonoBehaviour
{
    public static QuestManager Instance;

    [Header("Current Quest State")]
    public QuestData currentQuest;
    public int currentProgress = 0; // Змінено на public для легшої перевірки в NPC
    private bool isTransitioning = false;
    [Header("UI Texts")]
    [Tooltip("Текст, який показується, коли квест виконано, але треба здати його NPC")]
    public string returnToNPCText = "Повернись до NPC за нагородою";

    // Список завершених квестів (для збереження та NPC)
    public List<string> completedQuests = new List<string>();

    private List<QuestPoint> allPoints = new List<QuestPoint>();

    [Header("UI References")]
    public TextMeshProUGUI goalText;
    public GameObject questPanel;
    public Animator uiAnimator;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
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

    // --- СИСТЕМА РЕЄСТРАЦІЇ ТОЧОК ---

    public void RegisterPoint(QuestPoint point)
    {
        if (!allPoints.Contains(point)) allPoints.Add(point);
    }

    public Transform GetTargetTransform(string id)
    {
        foreach (var point in allPoints)
        {
            if (point != null && point.pointID == id)
                return point.transform;
        }
        return null;
    }

    // --- ЛОГІКА КВЕСТІВ ---

    public void InitializeQuest(QuestData newQuest)
    {
        // Якщо цей квест вже у списку виконаних — ігноруємо
        if (completedQuests.Contains(newQuest.name)) return;

        currentQuest = newQuest;
        currentProgress = 0;
        isTransitioning = false;

        if (questPanel != null) questPanel.SetActive(true);
        if (uiAnimator != null) uiAnimator.SetTrigger("Appear");

        UpdateUI();
    }

    public void OnQuestAction(QuestType actionType, string id, int amount = 1)
    {
        if (currentQuest == null || isTransitioning) return;

        // ДОДАНО: Перевіряємо, чи ID збігається, АБО чи поле Target ID в квесті порожнє (тобто зараховуємо будь-яку ціль)
        bool isTargetMatch = string.IsNullOrEmpty(currentQuest.targetID) || currentQuest.targetID == id;

        if (currentQuest.type == actionType && isTargetMatch)
        {
            if (currentQuest.requiredTowerLevel > 0)
            {
                int currentFloor = TowerManager.Instance.currentFloor;
                if (currentFloor != currentQuest.requiredTowerLevel) return;
            }

            currentProgress += amount;
            UpdateUI();

            if (currentProgress >= currentQuest.requiredAmount)
            {
                // Якщо треба повернутися до NPC — чекаємо взаємодії з ним
                if (currentQuest.requiresReturnToNPC)
                {
                    UpdateUI();
                }
                else
                {
                    StartCoroutine(CompleteQuestRoutine());
                }
            }
        }
    }

    // Метод для виклику з скрипта NPC, коли гравець прийшов здавати квест
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

        // Фіксуємо виконання в списку
        if (currentQuest != null)
        {
            if (!completedQuests.Contains(currentQuest.name))
                completedQuests.Add(currentQuest.name);
        }

        if (goalText != null)
        {
            goalText.text = $"<s>{currentQuest.description}</s>";
            goalText.color = Color.green;
        }

        yield return new WaitForSeconds(2.5f);

        QuestData next = currentQuest.nextQuest;

        if (next != null)
        {
            if (goalText != null) goalText.color = Color.white;
            InitializeQuest(next);
        }
        else
        {
            currentQuest = null;
            if (questPanel != null) questPanel.SetActive(false);
            Debug.Log("Всі сюжетні квести завершені!");
        }
    }

    void UpdateUI()
    {
        if (currentQuest != null && goalText != null)
        {
            if (currentQuest.requiresReturnToNPC && currentProgress >= currentQuest.requiredAmount)
            {
                // Використовуємо змінну замість жорсткого тексту
                goalText.text = returnToNPCText;
                return;
            }

            string progressInfo = currentQuest.requiredAmount > 1
                ? $" ({currentProgress}/{currentQuest.requiredAmount})"
                : "";

            goalText.text = $"{currentQuest.description}{progressInfo}";
        }
    }

    private void OnDestroy()
    {
        allPoints.Clear();
    }

    // --- ЗБЕРЕЖЕННЯ ---

    public GameData CaptureQuestState(GameData data)
    {
        if (currentQuest != null)
        {
            data.currentQuestID = currentQuest.name;
            data.questProgress = currentProgress;
        }
        else
        {
            data.currentQuestID = "";
        }

        // Зберігаємо копію списку завершених квестів
        data.completedQuestIDs = new List<string>(completedQuests);
        return data;
    }

    public void LoadQuestState(GameData data)
    {
        // Відновлюємо список завершених
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
            }
        }
    }
}