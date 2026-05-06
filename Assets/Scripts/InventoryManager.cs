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
    public InventoryUI inventoryUI;
    public HotbarUI hotbarScript;

    [Header("Поточна екіпіровка (Тільки назви)")]
    public string currentWeaponName;
    public string currentAmuletName;
    // НОВЕ: Окремі назви для двох кілець
    public string currentRing1Name;
    public string currentRing2Name;

    [Header("Гроші")]
    public int coins = 0;
    public TextMeshProUGUI[] coinTexts;

    [Header("Система Бафів")]
    public float coinMultiplier = 1f;
    private Coroutine activeBoostCoroutine;

    [Header("UI Бафів (Таймер)")]
    public GameObject buffUIContainer;
    public TextMeshProUGUI buffTimerText;

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

    // Метод видалення працює правильно: він зменшує кількість і видаляє тільки одну пачку (stack)
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

    // ОНОВЛЕНО: Тепер приймає номер слота для кілець
    public void EquipItem(Item item, string slotType, int ringSlotIndex = 0)
    {
        if (slotType == "Weapon") currentWeaponName = item.name;
        else if (slotType == "Amulet") currentAmuletName = item.name;
        else if (slotType == "Ring")
        {
            if (ringSlotIndex == 1) currentRing1Name = item.name;
            else if (ringSlotIndex == 2) currentRing2Name = item.name;
        }
    }

    // ОНОВЛЕНО: Для зняття предмета
    public void UnequipItem(string slotType, int ringSlotIndex = 0)
    {
        if (slotType == "Weapon") currentWeaponName = "";
        else if (slotType == "Amulet") currentAmuletName = "";
        else if (slotType == "Ring")
        {
            if (ringSlotIndex == 1) currentRing1Name = "";
            else if (ringSlotIndex == 2) currentRing2Name = "";
        }
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

    public void ActivateCoinBoost(float multiplier, float durationInSeconds)
    {
        if (activeBoostCoroutine != null) StopCoroutine(activeBoostCoroutine);
        activeBoostCoroutine = StartCoroutine(CoinBoostRoutine(multiplier, durationInSeconds));
    }

    private System.Collections.IEnumerator CoinBoostRoutine(float multiplier, float duration)
    {
        coinMultiplier = multiplier;
        float timeRemaining = duration;

        if (buffUIContainer != null) buffUIContainer.SetActive(true);

        while (timeRemaining > 0)
        {
            if (buffTimerText != null)
            {
                int minutes = Mathf.FloorToInt(timeRemaining / 60);
                int seconds = Mathf.FloorToInt(timeRemaining % 60);
                buffTimerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
            }
            yield return new WaitForSeconds(1f);
            timeRemaining -= 1f;
        }

        coinMultiplier = 1f;
        if (buffUIContainer != null) buffUIContainer.SetActive(false);
    }

    public void AddMobCoins(int baseAmount)
    {
        int finalAmount = Mathf.RoundToInt(baseAmount * coinMultiplier);
        ChangeCoins(finalAmount);
    }
}