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
    private BoxAreaSpawner spawner; // Посилання на спавнер
    private bool isQuestCompleted = false; // Щоб уникнути подвійного виклику

    void Awake()
    {
        rewardSystem = GetComponent<AreaReward>();
        spawner = GetComponent<BoxAreaSpawner>();
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

        Debug.Log($"<color=gold>Квест у {areaName} виконано!</color>");

        // 1. Фарбуємо текст у зелений та міняємо напис
        if (progressText != null)
        {
            progressText.color = Color.green;
            progressText.text = "ВИКОНАНО!";
        }

        // 2. Видаємо нагороду
        if (rewardSystem != null)
        {
            rewardSystem.SpawnReward(transform.position);
        }

        // 3. Запускаємо таймер на зникнення панелі
        StartCoroutine(HidePanelAfterDelay(5f)); // 5 секунд повисить і зникне

        // Скидаємо логіку (але UI вже під контролем корутини)
        currentKills = 0;
    }


    IEnumerator HidePanelAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        // Якщо гравець все ще в зоні, просто вимикаємо панель
        if (questPanel != null)
        {
            questPanel.SetActive(false);
        }

        // Повертаємо дефолтний колір тексту для наступного разу
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