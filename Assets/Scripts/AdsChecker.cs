using UnityEngine;
using Unity.Services.LevelPlay;
using System;
using System.Collections; // Потрібно для корутин

public class AdsChecker : MonoBehaviour
{
    [Header("Налаштування нагород (Можна міняти в Інспекторі)")]
    public int freeGoldAmount = 100;
    public float boostMultiplier = 3f;
    public float boostDuration = 120f;

    [Header("Налаштування Діамантів за рекламу")]
    public int freeDiamondsAmount = 3;
    public int maxDiamondAds = 3;
    public float diamondAdCooldownDuration = 1800f;

    public static AdsChecker Instance;

    private string appKey = "2638917fd";

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
        LevelPlay.Init(appKey);
        Debug.Log("AdsManager ініціалізовано. Режим: ІМІТАЦІЯ.");
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

    public void RequestAd(RewardType type)
    {
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

        // Запускаємо нашу симуляцію перегляду
        StartCoroutine(SimulateAdRoutine());
    }

    // ==========================================
    // СИМУЛЯЦІЯ ПАУЗИ ТА ВІДНОВЛЕННЯ ГРИ
    // ==========================================
    private IEnumerator SimulateAdRoutine()
    {
        Debug.Log("<color=orange>[AdsSystem] Реклама на екрані. СТАВИМО ГРУ НА ПАУЗУ.</color>");
        Time.timeScale = 0f;        // Зупиняємо всі ігрові таймери та рух
        AudioListener.pause = true; // Глушимо звуки

        // Оскільки Time.timeScale = 0, звичайний WaitForSeconds НЕ ПРАЦЮЄ. 
        // Використовуємо WaitForSecondsRealtime, щоб почекати 2 реальні секунди.
        yield return new WaitForSecondsRealtime(2f);

        Debug.Log("<color=green>[AdsSystem] Реклама закрита. ВІДНОВЛЮЄМО ГРУ.</color>");
        Time.timeScale = 1f;         // Повертаємо ігровий час
        AudioListener.pause = false; // Вмикаємо звуки назад

        // Видаємо нагороду після того, як відео "закінчилося"
        RewardPlayer();
    }

    // ==========================================
    // ВИДАЧА НАГОРОД
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
    // ЗБЕРЕЖЕННЯ / ЗАВАНТАЖЕННЯ ЛІМІТІВ
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
}