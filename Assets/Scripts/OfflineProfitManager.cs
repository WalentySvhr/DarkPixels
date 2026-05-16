using UnityEngine;
using System.Collections;

public class OfflineProfitManager : MonoBehaviour
{
    public static OfflineProfitManager Instance { get; private set; }

    [Header("Налаштування Економіки")]
    [Tooltip("Скільки монет заробляє місто за один цикл (наприклад, 2)")]
    public float coinsPerCycle = 2f;
    [Tooltip("Тривалість одного циклу в хвилинах (наприклад, 5)")]
    public float cycleDurationMinutes = 5f;
    [Tooltip("Максимальний час (у годинах), протягом якого накопичуються монети.")]
    public int maxOfflineHours = 12;

    [Header("Поточний стан")]
    public float accumulatedCoins = 0f; // Тепер float, щоб зберігати "копійки" між входами

    private const string LAST_EXIT_KEY = "Offline_LastExitTime";
    private const string ACCUMULATED_COINS_KEY = "Offline_AccumulatedCoinsFloat"; // Змінили ключ!

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        StartCoroutine(InitOfflineProfit());
    }

    private IEnumerator InitOfflineProfit()
    {
        while (TimeManager.Instance == null || !TimeManager.Instance.IsReady())
        {
            yield return null;
        }

        CalculateProfit();
    }

    private void CalculateProfit()
    {
        // Завантажуємо монети
        accumulatedCoins = PlayerPrefs.GetFloat(ACCUMULATED_COINS_KEY, 0f);

        string savedTimeStr = PlayerPrefs.GetString(LAST_EXIT_KEY, "");

        if (!string.IsNullOrEmpty(savedTimeStr) && long.TryParse(savedTimeStr, out long lastExitTime))
        {
            long currentTime = TimeManager.Instance.GetCurrentUnixTime();
            long diffSeconds = currentTime - lastExitTime;

            if (diffSeconds > 0)
            {
                float diffMinutes = diffSeconds / 60f;

                // Обмежуємо максимальний час накопичення
                float maxMinutes = maxOfflineHours * 60f;
                if (diffMinutes > maxMinutes) diffMinutes = maxMinutes;

                // Вираховуємо дохід за 1 хвилину
                float coinsPerMinute = coinsPerCycle / cycleDurationMinutes;

                // Додаємо нові монети (включаючи дробову частину)
                float newCoins = diffMinutes * coinsPerMinute;
                accumulatedCoins += newCoins;

                Debug.Log($"[Скарбниця] Вас не було {diffMinutes:F1} хв. Зібрано {Mathf.FloorToInt(newCoins)} повних монет.");
            }
        }

        SaveCurrentTimeAndCoins();
    }

    public void SaveCurrentTimeAndCoins()
    {
        if (TimeManager.Instance != null && TimeManager.Instance.IsReady())
        {
            PlayerPrefs.SetString(LAST_EXIT_KEY, TimeManager.Instance.GetCurrentUnixTime().ToString());
        }
        // Зберігаємо float
        PlayerPrefs.SetFloat(ACCUMULATED_COINS_KEY, accumulatedCoins);
        PlayerPrefs.Save();
    }

    public int ClaimCoins()
    {
        // Гравець отримує тільки цілі монети
        int toClaim = Mathf.FloorToInt(accumulatedCoins);

        // Віднімаємо цілі монети, але залишаємо дробові "копійки" на наступний раз
        accumulatedCoins -= toClaim;

        SaveCurrentTimeAndCoins();
        return toClaim;
    }

    // --- Для правильної роботи на мобільному ---
    private void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus)
        {
            SaveCurrentTimeAndCoins();
        }
        else
        {
            if (TimeManager.Instance != null && TimeManager.Instance.IsReady())
            {
                CalculateProfit();
            }
        }
    }

    private void OnApplicationQuit()
    {
        SaveCurrentTimeAndCoins();
    }
}