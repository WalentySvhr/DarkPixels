using UnityEngine;
using Unity.Services.LevelPlay;
using System;

public class AdsChecker : MonoBehaviour
{
    [Header("Налаштування нагород (Можна міняти в Інспекторі)")]
    public int freeGoldAmount = 100;
    public float boostMultiplier = 3f;
    public float boostDuration = 120f;
    public static AdsChecker Instance;

    private string appKey = "2638917fd";

    public enum RewardType
    {
        FreeGold,
        DoubleLoot,
        CoinBoostX3,
        RevivePlayer // <--- РОЗКОМЕНТУВАЛИ
    }

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

    public void RequestAd(RewardType type)
    {
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
                // --- ЛОГІКА ВІДРОДЖЕННЯ ---
                // Шукаємо гравця на сцені і викликаємо метод Revive()
                PlayerHealth player = FindFirstObjectByType<PlayerHealth>(); // FindObjectOfType замінено на сучасний FindFirstObjectByType (Unity 2023+)
                if (player != null)
                {
                    player.Revive();
                }
                else
                {
                    Debug.LogError("PlayerHealth не знайдено на сцені!");
                }
                break;
        }
    }
}