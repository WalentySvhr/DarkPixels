using UnityEngine;
using System.Collections;

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
    private FishDrop fishDropLogic;

    // --- Змінні для нової системи збереження часу ---
    private string mySpotID;
    private long _readyTime;
    private bool _isAvailable = false; // Зі старту false, поки не перевіримо збереження

    void Start()
    {
        currentAttempts = maxAttempts;
        fishDropLogic = GetComponent<FishDrop>();

        // Беремо унікальне ім'я об'єкта (наприклад "FishingPrefab (1)") як його ID
        mySpotID = gameObject.name;

        if (hookIcon != null) hookIcon.SetActive(false);
        if (waterAnimation != null) waterAnimation.SetActive(false); // Ховаємо, поки не завантажаться дані

        // Запускаємо перевірку збережень і часу
        StartCoroutine(InitializeSpot());
    }

    IEnumerator InitializeSpot()
    {
        // Чекаємо, поки TimeManager отримає точний час з інтернету
        while (TimeManager.Instance == null || !TimeManager.Instance.IsReady())
        {
            yield return null;
        }

        CheckSavedCooldown();
    }

    void CheckSavedCooldown()
    {
        _isAvailable = true; // За замовчуванням вважаємо, що спот вільний

        // Шукаємо цей спот у списку збережених "на паузі"
        foreach (var entry in SaveManager.Instance.GetActiveCooldowns())
        {
            if (entry.spotID == mySpotID)
            {
                long currentTime = TimeManager.Instance.GetCurrentUnixTime();

                if (currentTime < entry.unlockTime)
                {
                    // Час ще не вийшов!
                    _isAvailable = false;
                    _readyTime = entry.unlockTime;
                    currentAttempts = 0;

                    if (waterAnimation != null) waterAnimation.SetActive(false);
                    StartCoroutine(WaitUntilReady());
                }
                else
                {
                    // Час вийшов, поки гравця не було в грі. Очищаємо запис.
                    SaveManager.Instance.RemoveCooldown(mySpotID);
                }
                break;
            }
        }

        // Якщо спот вільний - показуємо воду
        if (_isAvailable && waterAnimation != null)
        {
            waterAnimation.SetActive(true);
        }
    }

    private void OnMouseDown()
    {
        if (isPlayerInside && _isAvailable && currentAttempts > 0)
        {
            CatchFish();
        }
    }

    void CatchFish()
    {
        Item caughtItem = fishDropLogic.GetRandomFishItem();

        if (caughtItem != null && InventoryManager.Instance != null)
        {
            bool wasAdded = InventoryManager.Instance.Add(caughtItem);

            if (wasAdded)
            {
                currentAttempts--;
                Debug.Log($"Успіх! Спіймано: {caughtItem.itemName}");

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

        // Якщо спроби закінчилися - запускаємо таймер
        if (currentAttempts <= 0)
        {
            StartCooldown();
        }
    }

    void StartCooldown()
    {
        _isAvailable = false;

        if (hookIcon != null) hookIcon.SetActive(false);
        if (waterAnimation != null) waterAnimation.SetActive(false);

        // Рахуємо час у майбутньому
        long currentTime = TimeManager.Instance.GetCurrentUnixTime();
        _readyTime = currentTime + (long)(cooldownMinutes * 60);

        // ЗАПИСУЄМО У ФАЙЛ ЗБЕРЕЖЕННЯ
        SaveManager.Instance.RegisterCooldown(mySpotID, _readyTime);

        // Запускаємо відлік
        StartCoroutine(WaitUntilReady());
    }

    IEnumerator WaitUntilReady()
    {
        // Перевіряємо час кожні 5 секунд
        while (TimeManager.Instance.GetCurrentUnixTime() < _readyTime)
        {
            yield return new WaitForSeconds(5f);
        }

        // --- ЧАС ВИЙШОВ ---
        _isAvailable = true;
        currentAttempts = maxAttempts;

        if (waterAnimation != null) waterAnimation.SetActive(true);

        // Якщо гравець стояв біля споту, коли він з'явився - показуємо іконку гачка
        if (isPlayerInside && hookIcon != null) hookIcon.SetActive(true);

        // Видаляємо запис про перезарядку зі збережень
        SaveManager.Instance.RemoveCooldown(mySpotID);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInside = true;
            // Показуємо гачок ТІЛЬКИ якщо спот доступний
            if (_isAvailable && hookIcon != null) hookIcon.SetActive(true);
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