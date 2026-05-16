using UnityEngine;
using Unity.Services.LevelPlay;
using System;

public class AdsChecker : MonoBehaviour
{
    [Header("Налаштування нагород (Можна міняти в Інспекторі)")]
    public int freeGoldAmount = 100;
    public float boostMultiplier = 3f;
    public float boostDuration = 120f;

    // --- НОВЕ: Налаштування діамантів та лімітів ---
    [Header("Налаштування Діамантів за рекламу")]
    public int freeDiamondsAmount = 3; // Скільки діамантів даємо за 1 перегляд
    public int maxDiamondAds = 3;      // Максимум переглядів до відкату
    public float diamondAdCooldownDuration = 1800f; // Час відкату в секундах (наприклад, 1800с = 30 хвилин)

    public static AdsChecker Instance;

    private string appKey = "2638917fd";

    public enum RewardType
    {
        FreeGold,
        DoubleLoot,
        CoinBoostX3,
        RevivePlayer,
        FreeDiamonds // <--- НОВА НАГОРОДА
    }

    private RewardType currentRewardType;

    // --- НОВЕ: Змінні для відслідковування лімітів ---
    private int diamondAdsWatchedCount = 0;
    private float cooldownTimer = 0f;
    private bool isDiamondAdOnCooldown = false;

    void Awake()
    {
        if (Instance == null) { Instance = this; DontDestroyOnLoad(gameObject); }
        else { Destroy(gameObject); }
    }

    void Start()
    {
        LevelPlay.Init(appKey);
        Debug.Log("AdsManager ініціалізовано.");
    }

    // --- НОВЕ: Обробка таймера відкату в Update ---
    void Update()
    {
        if (isDiamondAdOnCooldown)
        {
            cooldownTimer -= Time.deltaTime;

            if (cooldownTimer <= 0)
            {
                // Відкат завершено, скидаємо ліміти
                isDiamondAdOnCooldown = false;
                cooldownTimer = 0f;
                diamondAdsWatchedCount = 0;
                Debug.Log("<color=green>[AdsSystem] Відкат реклами на діаманти завершено! Можна дивитися знову.</color>");
            }
        }
    }

    public void RequestAd(RewardType type)
    {
        // --- НОВЕ: Перевірка ліміту перед показом реклами на діаманти ---
        if (type == RewardType.FreeDiamonds && isDiamondAdOnCooldown)
        {
            int minutesRemaining = Mathf.CeilToInt(cooldownTimer / 60f);
            Debug.LogWarning($"[AdsSystem] Реклама на діаманти заблокована! Триває відкат. Залишилось: {minutesRemaining} хв.");

            if (TowerUIManager.Instance != null)
            {
                TowerUIManager.Instance.ShowNotification($"Реклама на відкаті! Почекайте {minutesRemaining} хв.");
            }
            return;
        }

        currentRewardType = type;
        Debug.Log($"Запит на рекламу для події: {type}...");

        // Імітація успішного перегляду
        RewardPlayer();
    }

    private void RewardPlayer()
    {
        switch (currentRewardType)
        {
            case RewardType.FreeGold:
                InventoryManager.Instance.ChangeCoins(freeGoldAmount);
                Debug.Log($"<color=yellow>Нагорода видана: {freeGoldAmount} золота!</color>");
                break;

            case RewardType.CoinBoostX3:
                InventoryManager.Instance.ActivateCoinBoost(boostMultiplier, boostDuration);
                Debug.Log($"<color=yellow>Нагорода видана: Буст монет х{boostMultiplier} на {boostDuration} сек!</color>");
                break;

            case RewardType.RevivePlayer:
                PlayerHealth player = FindFirstObjectByType<PlayerHealth>();
                if (player != null)
                {
                    player.Revive();
                }
                else
                {
                    Debug.LogError("PlayerHealth не знайдено на сцені!");
                }
                break;

            // --- НОВЕ: Видача діамантів та контроль ліміту ---
            case RewardType.FreeDiamonds:
                if (InventoryManager.Instance != null)
                {
                    InventoryManager.Instance.ChangeDiamonds(freeDiamondsAmount);
                    diamondAdsWatchedCount++;

                    Debug.Log($"<color=cyan>Нагорода видана: {freeDiamondsAmount} діамантів! Переглянуто: {diamondAdsWatchedCount}/{maxDiamondAds}</color>");

                    // Перевіряємо, чи досягнуто ліміту в 3 перегляди
                    if (diamondAdsWatchedCount >= maxDiamondAds)
                    {
                        isDiamondAdOnCooldown = true;
                        cooldownTimer = diamondAdCooldownDuration;
                        Debug.Log("<color=orange>[AdsSystem] Досягнуто ліміту переглядів (3/3). Реклама на діаманти йде на відкат.</color>");
                    }
                }
                break;
        }
    }
    // --- НОВЕ: Запис даних реклами у структуру збереження ---
    public GameData CaptureAdsState(GameData data)
    {
        data.diamondAdsWatched = diamondAdsWatchedCount;
        data.diamondAdsCooldown = cooldownTimer;
        data.isDiamondAdOnCooldown = isDiamondAdOnCooldown;
        return data;
    }

    // --- НОВЕ: Завантаження даних реклами ---
    public void LoadAdsState(GameData data)
    {
        diamondAdsWatchedCount = data.diamondAdsWatched;
        cooldownTimer = data.diamondAdsCooldown;
        isDiamondAdOnCooldown = data.isDiamondAdOnCooldown;

        Debug.Log($"[AdsSystem] Дані завантажено. Переглядів: {diamondAdsWatchedCount}/3, Відкат: {cooldownTimer}с (Активний: {isDiamondAdOnCooldown})");
    }

    // --- НОВЕ: Допоміжний метод для твого UI (Кнопки реклами) ---
    // Дозволяє дізнатися, чи активна кнопка, і скільки часу залишилося до кінця відкату
    public bool CanWatchDiamondAd(out float timeLeft)
    {
        timeLeft = cooldownTimer;
        return !isDiamondAdOnCooldown;
    }
}