using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance;

    [Header("Налаштування інвентарю")]
    public List<ItemStack> items = new List<ItemStack>();
    public int space = 20;

    [Header("UI посилання (Окремі)")]
    public InventoryUI inventoryUI; // Скрипт для основної сітки сумки
    public HotbarUI hotbarScript;   // НОВИЙ окремий скрипт для хотбару

    [Header("Поточна екіпіровка (Тільки назви)")]
    public string currentWeaponName;
    public string currentAmuletName;

    [Header("Гроші")]
    public int coins = 0;
    public TextMeshProUGUI[] coinTexts;

    [Header("Система Бафів")]
    public float coinMultiplier = 1f; // Стандартний множник (х1)
    private Coroutine activeBoostCoroutine; // Зберігаємо посилання на таймер

    [Header("UI Бафів (Таймер)")]
    public GameObject buffUIContainer; // Об'єкт, який тримає іконку і текст бафу (BuffPanel)
    public TextMeshProUGUI buffTimerText; // Сам текст таймера

    [System.Serializable]
    public class ItemStack
    {
        public Item item;
        public int amount;
        public ItemStack(Item newItem, int newAmount) { item = newItem; amount = newAmount; }
    }

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        UpdateCoinUI();
        UpdateUI();

        // Ховаємо UI бафу на старті гри, якщо він призначений
        if (buffUIContainer != null) buffUIContainer.SetActive(false);
    }

    public bool Add(Item item)
    {
        if (item.isStackable)
        {
            ItemStack stack = items.Find(s => s.item == item && s.amount < item.maxStackSize);
            if (stack != null)
            {
                stack.amount++;
                UpdateUI();
                return true;
            }
        }

        if (items.Count >= space) return false;

        items.Add(new ItemStack(item, 1));
        UpdateUI();
        return true;
    }

    public void Remove(Item item)
    {
        ItemStack stack = items.Find(s => s.item == item);
        if (stack != null)
        {
            stack.amount--;
            if (stack.amount <= 0) items.Remove(stack);
            UpdateUI();
        }
    }

    public void EquipItem(Item item, bool isWeapon)
    {
        if (isWeapon) currentWeaponName = item.name;
        else currentAmuletName = item.name;
    }

    public void UnequipItem(bool isWeapon)
    {
        if (isWeapon) currentWeaponName = "";
        else currentAmuletName = "";
    }

    public void ChangeCoins(int amount)
    {
        coins += amount;
        if (coins < 0) coins = 0;
        UpdateCoinUI();
    }

    public void UpdateCoinUI()
    {
        if (coinTexts == null) return;
        foreach (TextMeshProUGUI textElement in coinTexts)
        {
            if (textElement != null) textElement.text = " " + coins;
        }
    }

    public void UseItem(Item item)
    {
        ItemStack stack = items.Find(s => s.item == item);
        if (stack == null) return;

        if (item.healValue > 0)
        {
            PlayerHealth health = FindFirstObjectByType<PlayerHealth>();
            if (health != null)
            {
                health.Heal(item.healValue);
                stack.amount--;
                if (stack.amount <= 0) items.Remove(stack);

                UpdateUI();
            }
        }
    }

    public void UpdateUI()
    {
        if (inventoryUI != null) inventoryUI.UpdateUI();
        if (hotbarScript != null) hotbarScript.UpdateHotbar();

        if (ShopManager.Instance != null && ShopManager.Instance.gameObject.activeInHierarchy)
        {
            try { ShopManager.Instance.RefreshShop(); }
            catch (System.Exception) { }
        }
    }

    // Функція запуску таймера
    public void ActivateCoinBoost(float multiplier, float durationInSeconds)
    {
        if (activeBoostCoroutine != null) StopCoroutine(activeBoostCoroutine);
        activeBoostCoroutine = StartCoroutine(CoinBoostRoutine(multiplier, durationInSeconds));
    }

    // ОНОВЛЕНИЙ таймер (Корутина) з оновленням UI
    private System.Collections.IEnumerator CoinBoostRoutine(float multiplier, float duration)
    {
        coinMultiplier = multiplier;
        float timeRemaining = duration;

        // Вмикаємо UI бафу
        if (buffUIContainer != null) buffUIContainer.SetActive(true);
        Debug.Log($"<color=green>Баф активовано! Множник монет: х{multiplier} на {duration} сек.</color>");

        // Цикл буде працювати, поки час не закінчиться
        while (timeRemaining > 0)
        {
            // Оновлюємо текст (форматуємо як Хвилини:Секунди)
            if (buffTimerText != null)
            {
                int minutes = Mathf.FloorToInt(timeRemaining / 60);
                int seconds = Mathf.FloorToInt(timeRemaining % 60);
                buffTimerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
            }

            // Чекаємо рівно 1 секунду
            yield return new WaitForSeconds(1f);

            // Віднімаємо 1 секунду від загального часу
            timeRemaining -= 1f;
        }

        // Коли час вийшов - повертаємо все як було
        coinMultiplier = 1f;
        if (buffUIContainer != null) buffUIContainer.SetActive(false);
        Debug.Log("<color=red>Час бафу вийшов! Множник знову х1.</color>");
    }

    // Функція ТІЛЬКИ для мобів
    public void AddMobCoins(int baseAmount)
    {
        int finalAmount = Mathf.RoundToInt(baseAmount * coinMultiplier);
        ChangeCoins(finalAmount);

        if (coinMultiplier > 1f)
        {
            Debug.Log($"З моба випало {baseAmount} золота. Завдяки бафу гравець отримує {finalAmount}!");
        }
    }
}