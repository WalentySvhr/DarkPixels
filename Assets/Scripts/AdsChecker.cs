using UnityEngine;
using Unity.Services.LevelPlay;
using System;

public class AdsChecker : MonoBehaviour
{
    [Header("Налаштування нагород")]
    public int freeGoldAmount = 100;
    public float boostMultiplier = 3f;
    public float boostDuration = 120f;

    [Header("Налаштування Діамантів за рекламу")]
    public int freeDiamondsAmount = 3;
    public int maxDiamondAds = 3;
    public float diamondAdCooldownDuration = 1800f;

    public static AdsChecker Instance;

    private string appKey = "2638917fd";

    // НОВЕ: У версії 9.4.1 обов'язково потрібен ID рекламного блоку.
    // Ви зможете створити його в панелі LevelPlay у меню "Setup -> Ad units".
    private string adUnitId = "alt6gaqjlknpqea6";

    // НОВЕ: Об'єкт, який керує саме відео за винагороду
    private LevelPlayRewardedAd rewardedAd;

    public enum RewardType
    {
        FreeGold,
        DoubleLoot,
        CoinBoostX3,
        RevivePlayer,
        FreeDiamonds
    }

    private RewardType currentRewardType;

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
        Debug.Log("AdsManager: Запуск ініціалізації LevelPlay...");

        // 1. Підписуємося на події запуску SDK
        LevelPlay.OnInitSuccess += SdkInitializationCompletedEvent;
        LevelPlay.OnInitFailed += SdkInitializationFailedEvent;

        // 2. Ініціалізуємо SDK
        LevelPlay.Init(appKey);
    }

    // ==========================================
    // СТВОРЕННЯ ТА ЗАВАНТАЖЕННЯ РЕКЛАМИ
    // ==========================================
    private void SdkInitializationCompletedEvent(LevelPlayConfiguration config)
    {
        Debug.Log("[AdsSystem] LevelPlay ініціалізовано успішно! Створюємо об'єкт реклами...");

        // 3. Створюємо інстанс реклами з вашим Ad Unit ID
        rewardedAd = new LevelPlayRewardedAd(adUnitId);

        // 4. Підписуємося на події саме цієї реклами (Новий синтаксис 9.4.1)
        rewardedAd.OnAdLoaded += OnAdLoaded;
        rewardedAd.OnAdLoadFailed += OnAdLoadFailed;
        rewardedAd.OnAdDisplayed += OnAdDisplayed;
        rewardedAd.OnAdDisplayFailed += OnAdDisplayFailed;
        rewardedAd.OnAdClosed += OnAdClosed;
        rewardedAd.OnAdRewarded += OnAdRewarded;

        // 5. У новій версії відео НЕ вантажиться автоматично. Робимо це вручну.
        rewardedAd.LoadAd();
    }

    private void SdkInitializationFailedEvent(LevelPlayInitError error)
    {
        Debug.LogError($"[AdsSystem] Помилка ініціалізації SDK: {error.ToString()}");
    }

    void Update()
    {
        if (isDiamondAdOnCooldown)
        {
            cooldownTimer -= Time.deltaTime;

            if (cooldownTimer <= 0)
            {
                isDiamondAdOnCooldown = false;
                cooldownTimer = 0f;
                diamondAdsWatchedCount = 0;
                Debug.Log("<color=green>[AdsSystem] Відкат реклами на діаманти завершено! Можна дивитися знову.</color>");
            }
        }
    }

    // ==========================================
    // ВИКЛИК РЕКЛАМИ
    // ==========================================
    public void RequestAd(RewardType type)
    {
        if (type == RewardType.FreeDiamonds && isDiamondAdOnCooldown)
        {
            int minutesRemaining = Mathf.CeilToInt(cooldownTimer / 60f);
            Debug.LogWarning($"[AdsSystem] Реклама на діаманти заблокована! Залишилось: {minutesRemaining} хв.");

            if (TowerUIManager.Instance != null)
            {
                TowerUIManager.Instance.ShowNotification($"Реклама на відкаті! Почекайте {minutesRemaining} хв.");
            }
            return;
        }

        currentRewardType = type;
        Debug.Log($"Запит на рекламу для події: {type}...");

        // Перевіряємо через новий об'єкт, чи відео вже завантажене
        if (rewardedAd != null && rewardedAd.IsAdReady())
        {
            rewardedAd.ShowAd();
        }
        else
        {
            Debug.LogWarning("[AdsSystem] Реклама ще не завантажилась! Спробуйте пізніше.");

            // Якщо сталася якась затримка, пробуємо форсувати завантаження
            if (rewardedAd != null) rewardedAd.LoadAd();

            if (TowerUIManager.Instance != null)
            {
                TowerUIManager.Instance.ShowNotification("Відео ще не готове. Зачекайте кілька секунд.");
            }
        }
    }

    // ==========================================
    // КОЛБЕКИ ВІД LEVELPLAY REWARDED AD (SDK 9.4.1)
    // ==========================================
    private void OnAdLoaded(LevelPlayAdInfo adInfo)
    {
        Debug.Log("[AdsSystem] Відео успішно завантажено в кеш!");
    }

    private void OnAdLoadFailed(LevelPlayAdError error)
    {
        Debug.LogError($"[AdsSystem] Помилка завантаження відео в кеш: {error.ToString()}");
    }

    private void OnAdDisplayed(LevelPlayAdInfo adInfo)
    {
        Debug.Log("<color=orange>[AdsSystem] Реклама на екрані. СТАВИМО ГРУ НА ПАУЗУ.</color>");
        Time.timeScale = 0f;
        AudioListener.pause = true;
    }

    private void OnAdDisplayFailed(LevelPlayAdInfo adInfo, LevelPlayAdError error)
    {
        Debug.LogError($"[AdsSystem] Помилка під час показу реклами: {error.ToString()}");

        Time.timeScale = 1f;
        AudioListener.pause = false;

        // Пробуємо завантажити нове відео
        if (rewardedAd != null) rewardedAd.LoadAd();
    }

    private void OnAdClosed(LevelPlayAdInfo adInfo)
    {
        Debug.Log("<color=green>[AdsSystem] Реклама закрита. ВІДНОВЛЮЄМО ГРУ.</color>");
        Time.timeScale = 1f;
        AudioListener.pause = false;

        // ОБОВ'ЯЗКОВО: У новій версії ми маємо самі попросити завантажити наступне відео після закриття
        rewardedAd.LoadAd();
    }

    private void OnAdRewarded(LevelPlayAdInfo adInfo, LevelPlayReward reward)
    {
        Debug.Log("<color=green>[AdsSystem] Відео переглянуто повністю! Видаємо нагороду.</color>");
        RewardPlayer();
    }

    // ==========================================
    // ВИДАЧА НАГОРОД (Залишилося без змін)
    // ==========================================
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
                if (DungeonAdUI.Instance != null) DungeonAdUI.Instance.HideBoostButton();
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

            case RewardType.FreeDiamonds:
                if (InventoryManager.Instance != null)
                {
                    InventoryManager.Instance.ChangeDiamonds(freeDiamondsAmount);
                    diamondAdsWatchedCount++;

                    Debug.Log($"<color=cyan>Нагорода видана: {freeDiamondsAmount} діамантів! Переглянуто: {diamondAdsWatchedCount}/{maxDiamondAds}</color>");

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

    // ==========================================
    // ЗБЕРЕЖЕННЯ / ЗАВАНТАЖЕННЯ ЛІМІТІВ (Залишилося без змін)
    // ==========================================
    public GameData CaptureAdsState(GameData data)
    {
        data.diamondAdsWatched = diamondAdsWatchedCount;
        data.diamondAdsCooldown = cooldownTimer;
        data.isDiamondAdOnCooldown = isDiamondAdOnCooldown;
        return data;
    }

    public void LoadAdsState(GameData data)
    {
        diamondAdsWatchedCount = data.diamondAdsWatched;
        cooldownTimer = data.diamondAdsCooldown;
        isDiamondAdOnCooldown = data.isDiamondAdOnCooldown;

        Debug.Log($"[AdsSystem] Дані завантажено. Переглядів: {diamondAdsWatchedCount}/3, Відкат: {cooldownTimer}с (Активний: {isDiamondAdOnCooldown})");
    }

    public bool CanWatchDiamondAd(out float timeLeft)
    {
        timeLeft = cooldownTimer;
        return !isDiamondAdOnCooldown;
    }

    // Очищення пам'яті
    void OnDestroy()
    {
        LevelPlay.OnInitSuccess -= SdkInitializationCompletedEvent;
        LevelPlay.OnInitFailed -= SdkInitializationFailedEvent;

        if (rewardedAd != null)
        {
            rewardedAd.OnAdLoaded -= OnAdLoaded;
            rewardedAd.OnAdLoadFailed -= OnAdLoadFailed;
            rewardedAd.OnAdDisplayed -= OnAdDisplayed;
            rewardedAd.OnAdDisplayFailed -= OnAdDisplayFailed;
            rewardedAd.OnAdClosed -= OnAdClosed;
            rewardedAd.OnAdRewarded -= OnAdRewarded;
            rewardedAd.Dispose();
        }
    }
}