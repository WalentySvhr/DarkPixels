using UnityEngine;
using TMPro;
using System.Collections;
// Цей скрипт відповідає за логіку квесту в зоні. Він відстежує кількість вбитих ворогів, оновлює UI та видає нагороду при виконанні квесту.


public class AreaQuestManager : MonoBehaviour
{
    [Header("Quest Info")]
    public string areaName = "Ліс Мутантів";
    public int killsRequired = 100;
    public float resetTimeHours = 1f;

    [Header("UI References")]
    public GameObject questPanel;
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI progressText;
    public TextMeshProUGUI timerText;

    private int currentKills = 0;
    private float lastKillTime;
    private bool isPlayerInside = false;
    private AreaReward rewardSystem;
    private PolygonAreaSpawner spawner; // Посилання на спавнер
    private bool isQuestCompleted = false; // Щоб уникнути подвійного виклику

    void Awake()
    {
        rewardSystem = GetComponent<AreaReward>();
        spawner = GetComponent<PolygonAreaSpawner>();
    }

    void Start()
    {
        if (questPanel != null) questPanel.SetActive(false);
        lastKillTime = Time.time;
    }

    void Update()
    {
        // Перевірка на обнулення прогресу через бездіяльність
        if (currentKills > 0 && currentKills < killsRequired)
        {
            float timeSinceLastKill = Time.time - lastKillTime;
            if (timeSinceLastKill > resetTimeHours * 3600f)
            {
                ResetQuest();
            }
        }
    }

    // Цей метод викликатиметься зі скрипта спавнера
    public void OnEnemyKilled()
    {
        // ЯКЩО СПАВНЕР НА ПАУЗІ — КВЕСТ НЕ ЗАРАХОВУЄТЬСЯ
        if (spawner != null && spawner.isOnLongBreak) return;

        // ЯКЩО КВЕСТ ВЖЕ ВИКОНАНО, АЛЕ ЩЕ НЕ СКИНУТО — ІГНОРУЄМО
        if (isQuestCompleted) return;

        currentKills++;
        lastKillTime = Time.time;
        UpdateUI();

        if (currentKills >= killsRequired)
        {
            CompleteQuest();
        }
    }

    private void CompleteQuest()
    {
        isQuestCompleted = true;

        if (progressText != null)
        {
            progressText.color = Color.green;
            progressText.text = "ВИКОНАНО!";
        }

        if (rewardSystem != null)
        {
            rewardSystem.SpawnReward(transform.position);
        }

        // Зупиняємо попередні корутини, якщо вони були, щоб уникнути конфліктів
        StopAllCoroutines();
        StartCoroutine(HidePanelAfterDelay(5f));
    }

    IEnumerator HidePanelAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        // Тільки тепер скидаємо вбивства
        currentKills = 0;

        if (questPanel != null) questPanel.SetActive(false);

        if (progressText != null)
        {
            progressText.color = Color.white;
        }

        isQuestCompleted = false;
        UpdateUI();
    }

    private void ResetQuest()
    {
        currentKills = 0;
        isQuestCompleted = false;
        UpdateUI();
    }

    private void UpdateUI()
    {
        if (titleText != null) titleText.text = areaName;

        // Додаємо статус "Очікування", якщо зона на паузі
        if (spawner != null && spawner.isOnLongBreak)
        {
            if (progressText != null) progressText.text = "<color=red>Зона зачищена</color>";
        }
        else
        {
            if (progressText != null) progressText.text = $"Убито: {currentKills} / {killsRequired}";
        }
    }

    // Активація UI при вході в зону колайдера
    private void OnTriggerEnter2D(Collider2D other)
    {
        // Додаємо перевірку !isQuestCompleted
        if (other.CompareTag("Player") && !isQuestCompleted)
        {
            isPlayerInside = true;
            UpdateUI();
            if (questPanel != null) questPanel.SetActive(true);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInside = false;
            if (questPanel != null) questPanel.SetActive(false);
        }
    }
}