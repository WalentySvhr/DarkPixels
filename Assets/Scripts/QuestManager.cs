using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic; // Потрібно для списку точок

public class QuestManager : MonoBehaviour
{
    public static QuestManager Instance;

    [Header("Current Quest State")]
    public QuestData currentQuest;
    private int currentProgress = 0;
    private bool isTransitioning = false;

    // Список усіх квестових точок на сцені для точної навігації стрілки
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
        if (!allPoints.Contains(point))
        {
            allPoints.Add(point);
        }
    }

    public Transform GetTargetTransform(string id)
    {
        // Шукаємо ціль серед зареєстрованих точок
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

        // 1. Перевіряємо відповідність типу дії та ID цілі
        if (currentQuest.type == actionType && currentQuest.targetID == id)
        {
            // 2. Додаткова перевірка для квестів у вежі
            // Якщо у QuestData вказано requiredTowerLevel > 0, перевіряємо поточний поверх
            if (currentQuest.requiredTowerLevel > 0)
            {
                // Передбачаємо, що у тебе є TowerManager, який знає поточний поверх
                int currentFloor = TowerManager.Instance.currentFloor;

                if (currentFloor != currentQuest.requiredTowerLevel)
                {
                    // Якщо гравець вбив моба не на тому поверсі — ігноруємо
                    return;
                }
            }

            // 3. Зараховуємо прогрес
            currentProgress += amount;
            UpdateUI();

            // 4. Перевірка завершення умови (наприклад, вбив 10 мобів)
            if (currentProgress >= currentQuest.requiredAmount)
            {
                // Якщо квест передбачає повернення до NPC, ми не викликаємо CompleteQuestRoutine автоматично.
                // Замість цього просто оновлюємо текст у UpdateUI на "Повернись до міста".

                if (currentQuest.requiresReturnToNPC)
                {
                    UpdateUI(); // Текст зміниться на "Повернись за нагородою"
                }
                else
                {
                    StartCoroutine(CompleteQuestRoutine());
                }
            }
        }
    }

    private IEnumerator CompleteQuestRoutine()
    {
        isTransitioning = true;
        Debug.Log("Квест виконано: " + currentQuest.questName);

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
}