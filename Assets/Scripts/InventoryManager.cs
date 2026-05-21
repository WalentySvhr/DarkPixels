using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance;

    [Header("Налаштування інвентарю")]
    public List<ItemStack> items = new List<ItemStack>();
    public int space = 20;

    // --- НОВЕ: Окремий список та налаштування для петів ---
    [Header("Інвентар Петів")]
    public List<ItemStack> petItems = new List<ItemStack>();
    public int petSpace = 6; // Максимальна кількість петів у колекції
    public PetInventoryUI petInventoryUI; // Скрипт інтерфейсу петів (створимо згодом)

    [Header("UI посилання")]
    public InventoryUI inventoryUI;
    public HotbarUI hotbarScript;
    public PlayerEquipment playerEquipment;

    [Header("Поточна екіпіровка (Універсальна система)")]
    public Dictionary<string, Item> equippedItems = new Dictionary<string, Item>();

    [Header("Гроші та Валюта")]
    public int coins = 0;
    public TextMeshProUGUI[] coinTexts;

    // --- НОВЕ: Змінні для преміум-валюти (Діамантів) ---
    public int diamonds = 0;
    public TextMeshProUGUI[] diamondTexts;

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
        UpdateDiamondUI(); // --- Оновлюємо UI діамантів при старті ---
        UpdateUI();
        UpdatePetUI();     // --- Початкове оновлення UI петів ---
        if (buffUIContainer != null) buffUIContainer.SetActive(false);
    }

    public bool Add(Item item)
    {
        if (item == null) return false;

        // --- НОВЕ: Перевірка, чи це петомець ---
        if (item.type == ItemType.Pet)
        {
            // Перевіряємо, чи такий пет вже є в колекції (вони унікальні, копії не потрібні)
            ItemStack existingPet = petItems.Find(s => s.item == item);
            if (existingPet != null) return false;

            // Перевіряємо, чи є вільне місце в сумці для петів
            if (petItems.Count >= petSpace) return false;

            // Додаємо в окремий список петів
            petItems.Add(new ItemStack(item, 1));
            UpdatePetUI();
            return true;
        }

        // --- СТАРИЙ КОД ДЛЯ ЗВИЧАЙНИХ ПРЕДМЕТІВ ---
        if (item.isStackable)
        {
            ItemStack stack = items.Find(s => s.item == item && s.amount < item.maxStackSize);
            if (stack != null)
            {
                stack.amount++;
                UpdateUI();
                if (QuestManager.Instance != null) QuestManager.Instance.UpdateCollectItemProgress();
                return true;
            }
        }

        if (items.Count >= space) return false;

        items.Add(new ItemStack(item, 1));
        UpdateUI();

        if (QuestManager.Instance != null) QuestManager.Instance.UpdateCollectItemProgress();
        return true;
    }

    public void Remove(Item item)
    {
        if (item == null) return;

        // --- НОВЕ: Видалення з інвентарю петів, якщо це пет ---
        if (item.type == ItemType.Pet)
        {
            ItemStack petStack = petItems.Find(s => s.item == item);
            if (petStack != null)
            {
                petItems.Remove(petStack);
                UpdatePetUI();
            }
            return;
        }

        // --- СТАРИЙ КОД ВИДАЛЕННЯ ---
        ItemStack stack = items.Find(s => s.item == item);
        if (stack != null)
        {
            if (!item.isStackable)
            {
                items.Remove(stack);
            }
            else
            {
                stack.amount--;
                if (stack.amount <= 0) items.Remove(stack);
            }
            UpdateUI();
        }
    }

    public int GetItemCount(Item itemToFind)
    {
        if (itemToFind == null) return 0;

        // --- НОВЕ: Перевірка кількості для петів ---
        if (itemToFind.type == ItemType.Pet)
        {
            ItemStack petStack = petItems.Find(s => s.item == itemToFind);
            return petStack != null ? petStack.amount : 0;
        }

        int count = 0;
        foreach (ItemStack stack in items)
        {
            if (stack.item == itemToFind) count += stack.amount;
        }
        return count;
    }

    public void RemoveItems(Item itemToRemove, int amountToRemove)
    {
        if (itemToRemove == null) return;

        // --- НОВЕ: Видалення петів через RemoveItems (про всяк випадок для квестів) ---
        if (itemToRemove.type == ItemType.Pet)
        {
            Remove(itemToRemove);
            return;
        }

        int remainingToRemove = amountToRemove;

        for (int i = items.Count - 1; i >= 0; i--)
        {
            ItemStack stack = items[i];
            if (stack.item == itemToRemove)
            {
                if (stack.amount >= remainingToRemove)
                {
                    stack.amount -= remainingToRemove;
                    remainingToRemove = 0;
                    if (stack.amount <= 0) items.RemoveAt(i);
                    break;
                }
                else
                {
                    remainingToRemove -= stack.amount;
                    items.RemoveAt(i);
                }
            }
        }
        UpdateUI();
    }

    public void EquipItem(Item itemToEquip, string slotType, int slotIndex = 0)
    {
        string slotKey = slotIndex > 0 ? $"{slotType}_{slotIndex}" : slotType;

        // Якщо в цьому слоті вже щось є - знімаємо його
        if (equippedItems.ContainsKey(slotKey))
        {
            UnequipItem(slotType, slotIndex);
        }

        // Записуємо в інвентар (дані)
        equippedItems[slotKey] = itemToEquip;

        // --- НОВЕ: ФАКТИЧНО ОДЯГАЄМО ПРЕДМЕТ НА ГРАВЦЯ ---
        if (playerEquipment != null)
        {
            if (itemToEquip is WeaponData weapon) playerEquipment.EquipWeapon(weapon);
            else if (itemToEquip is PetData pet) playerEquipment.EquipPet(pet);
            else if (itemToEquip is AmuletData amulet) playerEquipment.EquipAmulet(amulet);
            else if (itemToEquip is BeltData belt) playerEquipment.EquipBelt(belt);
            else if (itemToEquip is RingData ring) playerEquipment.EquipRing(ring, slotIndex);
        }

        Debug.Log($"[InventoryManager] В слот {slotKey} записано та одягнено {itemToEquip.name}");
    }

    public void UnequipItem(string slotType, int slotIndex = 0)
    {
        string slotKey = slotIndex > 0 ? $"{slotType}_{slotIndex}" : slotType;

        if (equippedItems.ContainsKey(slotKey))
        {
            Item itemToReturn = equippedItems[slotKey];
            equippedItems.Remove(slotKey);

            // --- НОВЕ: ФАКТИЧНО ЗНІМАЄМО ПРЕДМЕТ З ГРАВЦЯ ---
            if (playerEquipment != null)
            {
                if (itemToReturn is WeaponData) playerEquipment.UnequipWeapon();
                else if (itemToReturn is PetData) playerEquipment.UnequipPet();
                else if (itemToReturn is AmuletData) playerEquipment.UnequipAmulet();
                else if (itemToReturn is BeltData) playerEquipment.UnequipBelt();
                else if (itemToReturn is RingData) playerEquipment.UnequipRing(slotIndex);
            }

            Debug.Log($"[InventoryManager] Зі слота {slotKey} знято предмет: {itemToReturn.name}");
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

    // --- МЕТОДИ: Керування балансом та відображенням діамантів ---
    public void ChangeDiamonds(int amount)
    {
        diamonds += amount;
        if (diamonds < 0) diamonds = 0;
        UpdateDiamondUI();
    }

    public void UpdateDiamondUI()
    {
        if (diamondTexts == null) return;
        foreach (TextMeshProUGUI textElement in diamondTexts)
        {
            if (textElement != null) textElement.text = " " + diamonds;
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
            try { ShopManager.Instance.RefreshShop(); } catch (System.Exception) { }
        }

        if (StatsUI.Instance != null && StatsUI.Instance.gameObject.activeInHierarchy)
        {
            StatsUI.Instance.UpdateStatsUI();
        }

    }


    // --- НОВЕ: Метод оновлення інтерфейсу для вкладки петів ---
    public void UpdatePetUI()
    {
        if (petInventoryUI != null)
        {
            petInventoryUI.UpdatePetUI();
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
    // ✅ Новий метод для перевірки дублювання предметів
    public bool Contains(Item item)
    {
        if (item == null) return false;

        if (item.type == ItemType.Pet)
        {
            return petItems.Exists(s => s.item == item);
        }

        return items.Exists(s => s.item == item);
    }

}