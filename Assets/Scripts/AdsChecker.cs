using UnityEngine;
using Unity.Services.LevelPlay;
using System; // Потрібно для Action

public class AdsChecker : MonoBehaviour
{
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
                InventoryManager.Instance.ChangeCoins(100);
                Debug.Log("<color=yellow>Нагорода видана: 100 золота!</color>");
                break;

            case RewardType.CoinBoostX3:
                // 2. Викликаємо наш таймер: х3 на 120 секунд (2 хвилини)
                InventoryManager.Instance.ActivateCoinBoost(3f, 10f);
                Debug.Log("<color=yellow>Нагорода видана: Буст монет х3 на 2 хвилини!</color>");
                break;

            case RewardType.DoubleLoot:
                // Тут буде код для подвоєння луту
                InventoryManager.Instance.ChangeCoins(500); // наприклад, даємо багато золота
                Debug.Log("<color=yellow>Нагорода видана: Подвійний лут з боса (500 золота)!</color>");
                break;

                // case RewardType.RevivePlayer:
                //     // Тут буде код для воскресіння
                //     Debug.Log("<color=green>Гравець воскрес!</color>");
                //     break;

        }
    }
}