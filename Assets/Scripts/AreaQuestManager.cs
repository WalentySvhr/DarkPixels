using UnityEngine;
using TMPro;
using System.Collections;

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
    public TextMeshProUGUI timerText; // Опціонально: показувати скільки лишилось до скидання

    private int currentKills = 0;
    private float lastKillTime;
    private bool isPlayerInside = false;
    private AreaReward rewardSystem;

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
        currentKills++;
        lastKillTime = Time.time;
        UpdateUI();

        if (currentKills >= killsRequired)
        {
            CompleteQuest();
        }
    }
    void Awake()
    {
        rewardSystem = GetComponent<AreaReward>();
    }
    private void CompleteQuest()
    {
        Debug.Log($"<color=gold>Квест у {areaName} виконано!</color>");
        // Тут можна викликати метод видачі нагороди
        if (rewardSystem != null)
        {
            rewardSystem.SpawnReward(transform.position);
        }

        ResetQuest();
    }

    private void ResetQuest()
    {
        currentKills = 0;
        UpdateUI();
    }

    private void UpdateUI()
    {
        if (titleText != null) titleText.text = areaName;
        if (progressText != null) progressText.text = $"Убито: {currentKills} / {killsRequired}";
    }

    // Активація UI при вході в зону колайдера
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
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