using UnityEngine;
using System;

public class FishingZone : MonoBehaviour
{
    [Header("Об'єкти візуалізації")]
    public GameObject hookIcon;
    public GameObject waterAnimation;

    [Header("Параметри зони")]
    public int maxAttempts = 5;
    public float cooldownMinutes = 30f;

    private int currentAttempts;
    private bool isPlayerInside = false;
    private bool isCoolingDown = false;
    private DateTime nextAvailableTime;

    private FishDrop fishDropLogic;

    void Start()
    {
        currentAttempts = maxAttempts;
        fishDropLogic = GetComponent<FishDrop>();

        if (hookIcon != null) hookIcon.SetActive(false);
        if (waterAnimation != null) waterAnimation.SetActive(true);
    }

    private void OnMouseDown()
    {
        if (isPlayerInside && !isCoolingDown && currentAttempts > 0)
        {
            CatchFish();
        }
    }

    void CatchFish()
    {
        Item caughtItem = fishDropLogic.GetRandomFishItem();

        // ВИКОРИСТОВУЄМО INSTANCE (Синглтон) замість GameObject.Find
        if (caughtItem != null && InventoryManager.Instance != null)
        {
            bool wasAdded = InventoryManager.Instance.Add(caughtItem);

            if (wasAdded)
            {
                currentAttempts--;
                // Беремо itemName з твого ScriptableObject
                Debug.Log($"Успіх! Спіймано: {caughtItem.itemName}");

                // Викликаємо віконце
                if (LootPopupManager.Instance != null)
                {
                    LootPopupManager.Instance.ShowLoot(caughtItem);
                }
                else
                {
                    Debug.LogWarning("LootPopupManager не знайдено на сцені!");
                }
            }
            else
            {
                Debug.Log("Інвентар повний!");
            }
        }

        if (currentAttempts <= 0) StartCooldown();
    }

    void StartCooldown()
    {
        isCoolingDown = true;
        if (hookIcon != null) hookIcon.SetActive(false);
        if (waterAnimation != null) waterAnimation.SetActive(false);
        nextAvailableTime = DateTime.Now.AddMinutes(cooldownMinutes);
    }

    void Update()
    {
        if (isCoolingDown && DateTime.Now >= nextAvailableTime)
        {
            isCoolingDown = false;
            currentAttempts = maxAttempts;
            if (waterAnimation != null) waterAnimation.SetActive(true);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !isCoolingDown)
        {
            isPlayerInside = true;
            if (hookIcon != null) hookIcon.SetActive(true);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInside = false;
            if (hookIcon != null) hookIcon.SetActive(false);
        }
    }
}