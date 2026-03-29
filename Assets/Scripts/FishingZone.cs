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

    private InventoryManager inventory; // Змінено на InventoryManager
    private FishDrop fishDropLogic;

    void Start()
    {
        currentAttempts = maxAttempts;
        // Шукаємо InventoryManager на гравцеві
        inventory = GameObject.FindGameObjectWithTag("Player")?.GetComponent<InventoryManager>();
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
        // Отримуємо дані предмета (Item) від FishDrop
        Item caughtItem = fishDropLogic.GetRandomFishItem();

        if (caughtItem != null && inventory != null)
        {
            // Використовуємо твій метод Add(item)
            bool wasAdded = inventory.Add(caughtItem);

            if (wasAdded)
            {
                currentAttempts--;
                Debug.Log($"Спіймано і додано в інвентар: {caughtItem.name}");
            }
            else
            {
                Debug.Log("Інвентар повний!");
                return; // Не знімаємо спробу, якщо риба не влізла
            }
        }

        if (currentAttempts <= 0) StartCooldown();
    }

    // --- Логіка таймера та тригерів залишається без змін ---
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