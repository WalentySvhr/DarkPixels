using UnityEngine;
using Unity.Services.LevelPlay;
using System; // Потрібно для Action

public class AdsChecker : MonoBehaviour
{
    [Header("Налаштування нагород (Можна міняти в Інспекторі)")]
    public int freeGoldAmount = 100;      // Скільки золота давати за звичайну рекламу
    public float boostMultiplier = 3f;    // Множник для бафу (х3)
    public float boostDuration = 120f;    // Час дії бафу в секундах
    public static AdsChecker Instance;

    private string appKey = "2638917fd";

    // Визначаємо типи можливих нагород (можеш додавати свої)
    public enum RewardType
    {
        FreeGold,       // Звичайне безкоштовне золото з меню
        DoubleLoot,     // Подвійний лут з боса
        // RevivePlayer    // Воскресіння гравця
        CoinBoostX3, // <--- НОВИЙ ТИП
    }

    // Змінна, яка запам'ятає, яку нагороду ми зараз чекаємо
    private RewardType currentRewardType;

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

    // Цей метод ми тепер можемо викликати з БУДЬ-ЯКОГО скрипта і сказати, ЯКУ нагороду хочемо
    public void RequestAd(RewardType type)
    {
        currentRewardType = type;
        Debug.Log($"Запит на рекламу для події: {type}...");

        // Коли підключиш справжню рекламу, тут буде перевірка isReady() і ShowAd()
        // А поки просто імітуємо успішний перегляд:
        RewardPlayer();
    }

    // Метод, який видає нагороду залежно від того, що ми замовляли
    private void RewardPlayer()
    {
        switch (currentRewardType)
        {
            case RewardType.FreeGold:
                // Використовуємо змінну freeGoldAmount замість цифри 100
                InventoryManager.Instance.ChangeCoins(freeGoldAmount);
                Debug.Log($"<color=yellow>Нагорода видана: {freeGoldAmount} золота!</color>");
                break;

            case RewardType.CoinBoostX3:
                // Використовуємо змінні для бафу
                InventoryManager.Instance.ActivateCoinBoost(boostMultiplier, boostDuration);
                Debug.Log($"<color=yellow>Нагорода видана: Буст монет х{boostMultiplier} на {boostDuration} сек!</color>");
                break;

                // case RewardType.RevivePlayer:
                //     Debug.Log("<color=green>Гравець воскрес!</color>");
                //     break;
        }
    }
}