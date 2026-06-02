using UnityEngine;
using Unity.Services.LevelPlay;
using System;

public class AdsChecker : MonoBehaviour
{
    [Header("Режим тестування")]
    [Tooltip("Якщо TRUE — реальна реклама не вантажиться, а нагорода видається миттєво.")]
    public bool isTestMode = true;

    [Header("Налаштування нагород")]
    public int freeGoldAmount = 100; // Кількість золота за перегляд
    public float boostMultiplier = 3f; // Множник монет для буста
    public float boostDuration = 120f; // Тривалість буста в секундах

    [Header("Налаштування Діамантів за рекламу")]
    public int freeDiamondsAmount = 3; // Кількість діамантів за 1 перегляд
    public int maxDiamondAds = 3; // Максимальна кількість переглядів до відкату
    public float diamondAdCooldownDuration = 1800f; // Час відкату в секундах (1800с = 30хв)

    // Паттерн Singleton для легкого доступу з інших скриптів (AdsChecker.Instance...)
    public static AdsChecker Instance;

    // Ключі для ініціалізації LevelPlay (IronSource)
    private string appKey = "2638917fd";
    private string adUnitId = "alt6gaqjlknpqea6";

    // Об'єкт, що відповідає за завантаження та показ відео з винагородою
    private LevelPlayRewardedAd rewardedAd;

    // Типи нагород, які можна отримати за рекламу
    public enum RewardType
    {
        FreeGold,
        DoubleLoot,
        CoinBoostX3,
        RevivePlayer,
        FreeDiamonds
    }

    // Зберігає тип нагороди, яку гравець запросив перед початком відео
    private RewardType currentRewardType;

    // Змінні для відстеження лімітів (діаманти)
    private int diamondAdsWatchedCount = 0; // Скільки відео на діаманти вже переглянуто
    private float cooldownTimer = 0f; // Поточний час таймера відкату
    private bool isDiamondAdOnCooldown = false; // Чи активний зараз відкат

    void Awake()
    {
        // Налаштування Singleton: гарантуємо, що на сцені лише один такий об'єкт
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Не знищувати при переході між сценами
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        // Якщо тестовий режим ВИМКНЕНО — ініціалізуємо реальну рекламу
        if (!isTestMode)
        {
            Debug.Log("AdsManager: Запуск ініціалізації LevelPlay...");

            // Підписуємося на події успішної/неуспішної ініціалізації SDK
            LevelPlay.OnInitSuccess += SdkInitializationCompletedEvent;
            LevelPlay.OnInitFailed += SdkInitializationFailedEvent;

            // Запускаємо ініціалізацію
            LevelPlay.Init(appKey);
        }
        else
        {
            // Якщо тестовий режим УВІМКНЕНО — просто виводимо повідомлення в консоль
            Debug.Log("<color=magenta>[AdsSystem] ТЕСТОВИЙ РЕЖИМ УВІМКНЕНО. Реальна реклама відключена.</color>");
        }
    }

    void Update()
    {
        // Обробка таймера відкату для реклами на діаманти
        if (isDiamondAdOnCooldown)
        {
            cooldownTimer -= Time.deltaTime; // Віднімаємо час кожного кадру

            // Якщо час вийшов
            if (cooldownTimer <= 0)
            {
                isDiamondAdOnCooldown = false; // Вимикаємо відкат
                cooldownTimer = 0f; // Скидаємо таймер
                diamondAdsWatchedCount = 0; // Скидаємо лічильник переглядів
                Debug.Log("<color=green>[AdsSystem] Відкат реклами на діаманти завершено! Можна дивитися знову.</color>");
            }
        }
    }

    // ==========================================
    // ВИКЛИК РЕКЛАМИ (АБО ВИДАЧА ТЕСТОВОЇ НАГОРОДИ)
    // ==========================================
    public void RequestAd(RewardType type)
    {
        // Перевірка 1: Чи не знаходиться реклама на діаманти на відкаті
        if (type == RewardType.FreeDiamonds && isDiamondAdOnCooldown)
        {
            int minutesRemaining = Mathf.CeilToInt(cooldownTimer / 60f);
            Debug.LogWarning($"[AdsSystem] Реклама на діаманти заблокована! Залишилось: {minutesRemaining} хв.");

            if (TowerUIManager.Instance != null)
            {
                TowerUIManager.Instance.ShowNotification($"Реклама на відкаті! Почекайте {minutesRemaining} хв.");
            }
            return; // Перериваємо виконання, реклама не покажеться
        }

        // Запам'ятовуємо, за що саме маємо видати нагороду
        currentRewardType = type;

        // ПЕРЕВІРКА НА ТЕСТОВИЙ РЕЖИМ
        if (isTestMode)
        {
            Debug.Log($"<color=magenta>[AdsSystem] ТЕСТ: Видаємо нагороду {type} без перегляду відео.</color>");
            RewardPlayer(); // Видаємо нагороду одразу
        }
        else
        {
            // РЕАЛЬНА РЕКЛАМА
            if (rewardedAd != null && rewardedAd.IsAdReady())
            {
                rewardedAd.ShowAd(); // Показуємо відео
            }
            else
            {
                Debug.LogWarning("[AdsSystem] Реклама ще не завантажилась! Спробуйте пізніше.");
                if (rewardedAd != null) rewardedAd.LoadAd(); // Форсуємо завантаження

                if (TowerUIManager.Instance != null)
                {
                    TowerUIManager.Instance.ShowNotification("Відео ще не готове. Зачекайте кілька секунд.");
                }
            }
        }
    }

    // ==========================================
    // ІНІЦІАЛІЗАЦІЯ ТА КОЛБЕКИ LEVELPLAY
    // ==========================================
    private void SdkInitializationCompletedEvent(LevelPlayConfiguration config)
    {
        Debug.Log("[AdsSystem] LevelPlay ініціалізовано успішно! Створюємо об'єкт реклами...");

        // Створюємо об'єкт реклами та прив'язуємо ID
        rewardedAd = new LevelPlayRewardedAd(adUnitId);

        // Підписуємося на події відео
        rewardedAd.OnAdLoaded += OnAdLoaded;
        rewardedAd.OnAdLoadFailed += OnAdLoadFailed;
        rewardedAd.OnAdDisplayed += OnAdDisplayed;
        rewardedAd.OnAdDisplayFailed += OnAdDisplayFailed;
        rewardedAd.OnAdClosed += OnAdClosed;
        rewardedAd.OnAdRewarded += OnAdRewarded;

        // Завантажуємо перше відео у фоні
        rewardedAd.LoadAd();
    }

    private void SdkInitializationFailedEvent(LevelPlayInitError error)
    {
        Debug.LogError($"[AdsSystem] Помилка ініціалізації SDK: {error.ToString()}");
    }

    private void OnAdLoaded(LevelPlayAdInfo adInfo) { Debug.Log("[AdsSystem] Відео успішно завантажено в кеш!"); }

    private void OnAdLoadFailed(LevelPlayAdError error) { Debug.LogError($"[AdsSystem] Помилка завантаження: {error.ToString()}"); }

    private void OnAdDisplayed(LevelPlayAdInfo adInfo)
    {
        // Коли відео з'явилося на екрані — зупиняємо час і звук у грі
        Debug.Log("<color=orange>[AdsSystem] Реклама на екрані. СТАВИМО ГРУ НА ПАУЗУ.</color>");
        Time.timeScale = 0f;
        AudioListener.pause = true;
    }

    private void OnAdDisplayFailed(LevelPlayAdInfo adInfo, LevelPlayAdError error)
    {
        // Якщо відео зламалося під час показу — повертаємо гру до життя
        Time.timeScale = 1f;
        AudioListener.pause = false;
        if (rewardedAd != null) rewardedAd.LoadAd(); // Пробуємо завантажити нове
    }

    private void OnAdClosed(LevelPlayAdInfo adInfo)
    {
        // Коли гравець закрив відео (хрестиком) — відновлюємо гру
        Debug.Log("<color=green>[AdsSystem] Реклама закрита. ВІДНОВЛЮЄМО ГРУ.</color>");
        Time.timeScale = 1f;
        AudioListener.pause = false;
        rewardedAd.LoadAd(); // ОБОВ'ЯЗКОВО вантажимо наступне відео
    }

    private void OnAdRewarded(LevelPlayAdInfo adInfo, LevelPlayReward reward)
    {
        // Цей метод викликається ТІЛЬКИ сервером, якщо відео переглянуто повністю
        Debug.Log("<color=green>[AdsSystem] Відео переглянуто повністю! Видаємо нагороду.</color>");
        RewardPlayer();
    }

    // ==========================================
    // ЛОГІКА ВИДАЧІ НАГОРОД
    // ==========================================
    private void RewardPlayer()
    {
        // Перевіряємо, яку нагороду було запрошено
        switch (currentRewardType)
        {
            case RewardType.FreeGold:
                InventoryManager.Instance.ChangeCoins(freeGoldAmount);
                break;

            case RewardType.CoinBoostX3:
                InventoryManager.Instance.ActivateCoinBoost(boostMultiplier, boostDuration);
                if (DungeonAdUI.Instance != null) DungeonAdUI.Instance.HideBoostButton();
                break;

            case RewardType.RevivePlayer:
                PlayerHealth player = FindFirstObjectByType<PlayerHealth>();
                if (player != null) player.Revive();
                else Debug.LogError("PlayerHealth не знайдено на сцені!");
                break;

            case RewardType.FreeDiamonds:
                if (InventoryManager.Instance != null)
                {
                    // Видаємо діаманти
                    InventoryManager.Instance.ChangeDiamonds(freeDiamondsAmount);

                    // Збільшуємо лічильник переглядів
                    diamondAdsWatchedCount++;

                    // Якщо досягли ліміту (наприклад, 3 з 3) — вмикаємо таймер відкату
                    if (diamondAdsWatchedCount >= maxDiamondAds)
                    {
                        isDiamondAdOnCooldown = true;
                        cooldownTimer = diamondAdCooldownDuration;
                        Debug.Log("<color=orange>[AdsSystem] Ліміт переглядів (3/3). Реклама йде на відкат.</color>");
                    }
                }
                break;
        }
    }

    // ==========================================
    // ЗБЕРЕЖЕННЯ / ЗАВАНТАЖЕННЯ ДАНИХ (Save System)
    // ==========================================
    public GameData CaptureAdsState(GameData data)
    {
        // Записуємо поточний стан реклами у файл збереження
        data.diamondAdsWatched = diamondAdsWatchedCount;
        data.diamondAdsCooldown = cooldownTimer;
        data.isDiamondAdOnCooldown = isDiamondAdOnCooldown;
        return data;
    }

    public void LoadAdsState(GameData data)
    {
        // Відновлюємо стан реклами при запуску гри
        diamondAdsWatchedCount = data.diamondAdsWatched;
        cooldownTimer = data.diamondAdsCooldown;
        isDiamondAdOnCooldown = data.isDiamondAdOnCooldown;
    }

    public bool CanWatchDiamondAd(out float timeLeft)
    {
        // Зручний метод для UI, щоб перевірити, чи можна показувати кнопку
        timeLeft = cooldownTimer;
        return !isDiamondAdOnCooldown;
    }

    // ==========================================
    // ОЧИЩЕННЯ ПАМ'ЯТІ ПРИ ВИХОДІ
    // ==========================================
    void OnDestroy()
    {
        // Відписуємося від подій, щоб уникнути витоків пам'яті
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