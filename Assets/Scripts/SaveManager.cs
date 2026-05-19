using UnityEngine;
using System.Collections.Generic;
using System.IO;
using UnityEngine.SceneManagement;
using System.Linq;

[System.Serializable]
public class GameData
{
    public float posX, posY, posZ;
    public int coins;
    public int diamonds;
    public float currentHealth;
    public List<ItemSaveEntry> backpack = new List<ItemSaveEntry>();

    // --- НОВЕ: Список для збереження куплених петів ---
    public List<ItemSaveEntry> petBackpack = new List<ItemSaveEntry>();

    // --- УНІВЕРСАЛЬНЕ ЗБЕРЕЖЕННЯ ЕКІПІРОВКИ ---
    public List<EquippedItemSaveEntry> equippedItems = new List<EquippedItemSaveEntry>();

    public string currentQuestID;
    public int questProgress;
    public List<FishingSpotSaveEntry> activeCooldowns = new List<FishingSpotSaveEntry>();
    public List<string> completedQuestIDs = new List<string>();

    // --- Збереження лімітів та таймерів реклами на діаманти ---
    public int diamondAdsWatched;
    public float diamondAdsCooldown;
    public bool isDiamondAdOnCooldown;

    // --- Збереження рекорду Башні ---
    public int maxTowerFloor;

    // ============================================================================
    // --- НОВЕ ДЛЯ GOOGLE PLAY IN-APP REVIEW ---
    // ============================================================================
    public int victoryCount;      // Кількість перемог на аренах
    public bool alreadyReviewed;  // Чи було успішно показано вікно відгуку
}

[System.Serializable]
public class EquippedItemSaveEntry
{
    public string slotKey; // Наприклад: "Weapon", "Belt", "Ring_1", "Pet"
    public string itemName;
}

[System.Serializable]
public class ItemSaveEntry
{
    public string itemName;
    public int amount;
}

[System.Serializable]
public class FishingSpotSaveEntry
{
    public string spotID;
    public long unlockTime;
}

public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance;
    private string savePath;
    private bool needsToLoad = false;

    // Кеш для поточних завантажених даних, щоб мати легкий доступ до victoryCount та alreadyReviewed
    public GameData CurrentData { get; private set; } = new GameData();

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            savePath = Path.Combine(Application.persistentDataPath, "gamesave.json");

            // Завантажуємо початкові дані в кеш відразу при старті програми, якщо файл існує
            if (File.Exists(savePath))
            {
                CurrentData = JsonUtility.FromJson<GameData>(File.ReadAllText(savePath));
            }
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnEnable() => SceneManager.sceneLoaded += OnSceneLoaded;
    private void OnDisable() => SceneManager.sceneLoaded -= OnSceneLoaded;

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (needsToLoad && scene.name == "Game")
        {
            LoadGame();
            needsToLoad = false;
        }
    }

    public void PrepareLoad() => needsToLoad = true;

    public void SaveGame()
    {
        if (!SaveForbiddenZone.CanSave)
        {
            Debug.LogWarning("Збереження неможливе всередині башти!");
            return;
        }

        GameData data = new GameData();
        GameObject player = GameObject.FindGameObjectWithTag("Player");

        if (player != null)
        {
            data.posX = player.transform.position.x;
            data.posY = player.transform.position.y;
            data.posZ = player.transform.position.z;
        }

        if (InventoryManager.Instance != null)
        {
            data.coins = InventoryManager.Instance.coins;
            data.diamonds = InventoryManager.Instance.diamonds;

            // --- УНІВЕРСАЛЬНЕ ЗБЕРЕЖЕННЯ СЛОВНИКА ЕКІПІРОВКИ ---
            data.equippedItems.Clear();
            foreach (var pair in InventoryManager.Instance.equippedItems)
            {
                if (pair.Value != null)
                {
                    data.equippedItems.Add(new EquippedItemSaveEntry { slotKey = pair.Key, itemName = pair.Value.name });
                }
            }

            // Збереження звичайного інвентарю
            data.backpack.Clear();
            foreach (var stack in InventoryManager.Instance.items)
            {
                if (stack?.item != null)
                    data.backpack.Add(new ItemSaveEntry { itemName = stack.item.name, amount = stack.amount });
            }

            // --- НОВЕ: Збереження інвентарю Петів ---
            data.petBackpack.Clear();
            foreach (var stack in InventoryManager.Instance.petItems)
            {
                if (stack?.item != null)
                    data.petBackpack.Add(new ItemSaveEntry { itemName = stack.item.name, amount = stack.amount });
            }
        }

        if (QuestManager.Instance != null)
        {
            data = QuestManager.Instance.CaptureQuestState(data);
        }

        if (AdsChecker.Instance != null)
        {
            data = AdsChecker.Instance.CaptureAdsState(data);
        }

        if (TowerManager.Instance != null)
        {
            data = TowerManager.Instance.CaptureTowerState(data);
        }

        PlayerHealth health = Object.FindFirstObjectByType<PlayerHealth>();
        if (health != null) data.currentHealth = health.currentHealth;

        // Зберігаємо також значення для відгуків, які зараз лежать у кеші
        data.victoryCount = CurrentData.victoryCount;
        data.alreadyReviewed = CurrentData.alreadyReviewed;

        // Оновлюємо наш робочий кеш даних перед записом на диск
        CurrentData = data;

        File.WriteAllText(savePath, JsonUtility.ToJson(data, true));
        Debug.Log("<color=green>[SaveSystem]</color> Дані успішно зафіксовані.");
    }

    public void LoadGame()
    {
        if (!File.Exists(savePath)) return;

        GameData data = JsonUtility.FromJson<GameData>(File.ReadAllText(savePath));
        CurrentData = data; // Записуємо завантажені дані в кеш

        GameObject player = GameObject.FindGameObjectWithTag("Player");

        if (player != null)
        {
            Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.linearVelocity = Vector2.zero;
                rb.position = new Vector2(data.posX, data.posY);
            }
            player.transform.position = new Vector3(data.posX, data.posY, data.posZ);
        }

        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.coins = data.coins;
            InventoryManager.Instance.diamonds = data.diamonds;

            InventoryManager.Instance.equippedItems.Clear();

            InventoryManager.Instance.UpdateCoinUI();
            InventoryManager.Instance.UpdateDiamondUI();

            // Завантаження звичайного інвентарю
            InventoryManager.Instance.items.Clear();
            foreach (var entry in data.backpack)
            {
                Item loadedItem = Resources.Load<Item>("Items/" + entry.itemName);
                if (loadedItem != null)
                    InventoryManager.Instance.items.Add(new InventoryManager.ItemStack(loadedItem, entry.amount));
            }
            InventoryManager.Instance.UpdateUI();

            // --- НОВЕ: Завантаження інвентарю Петів ---
            InventoryManager.Instance.petItems.Clear();
            foreach (var entry in data.petBackpack)
            {
                Item loadedPet = Resources.Load<Item>("Items/" + entry.itemName);
                if (loadedPet != null)
                    InventoryManager.Instance.petItems.Add(new InventoryManager.ItemStack(loadedPet, entry.amount));
            }
            InventoryManager.Instance.UpdatePetUI();
        }

        if (QuestManager.Instance != null)
        {
            QuestManager.Instance.LoadQuestState(data);
        }

        if (AdsChecker.Instance != null)
        {
            AdsChecker.Instance.LoadAdsState(data);
        }

        if (TowerManager.Instance != null)
        {
            TowerManager.Instance.LoadTowerState(data);
        }

        PlayerHealth health = Object.FindFirstObjectByType<PlayerHealth>();
        if (health != null) health.currentHealth = (int)data.currentHealth;

        StopAllCoroutines();
        StartCoroutine(ApplyEquipmentAfterLoad(data));
    }

    private System.Collections.IEnumerator ApplyEquipmentAfterLoad(GameData data)
    {
        yield return new WaitForSeconds(0.5f);

        InventorySlot[] allSlots = Object.FindObjectsByType<InventorySlot>(FindObjectsInactive.Include, FindObjectsSortMode.None);

        foreach (var slot in allSlots)
        {
            string key = "";
            if (slot.isWeaponEquipmentSlot) key = "Weapon";
            else if (slot.isAmuletEquipmentSlot) key = "Amulet";
            else if (slot.isBeltEquipmentSlot) key = "Belt";
            else if (slot.isRingEquipmentSlot) key = $"Ring_{slot.ringSlotIndex}";
            else if (slot.isPetEquipmentSlot) key = "Pet"; // --- НОВЕ: Ключ для пета ---

            if (string.IsNullOrEmpty(key)) continue;

            var entry = data.equippedItems.Find(x => x.slotKey == key);
            if (entry != null && !string.IsNullOrEmpty(entry.itemName))
            {
                Item item = Resources.Load<Item>("Items/" + entry.itemName);
                if (item != null)
                {
                    slot.AddItem(item, 1);
                    Debug.Log($"<color=yellow>[SaveSystem]</color> Відновлено {key}: {item.name}");
                }
            }
        }
    }

    // --- МЕТОДИ ДЛЯ РИБАЛКИ ---
    public List<FishingSpotSaveEntry> GetActiveCooldowns()
    {
        if (!File.Exists(savePath)) return new List<FishingSpotSaveEntry>();
        GameData data = JsonUtility.FromJson<GameData>(File.ReadAllText(savePath));
        return data.activeCooldowns;
    }

    public void RegisterCooldown(string id, long time)
    {
        GameData data = new GameData();
        if (File.Exists(savePath)) data = JsonUtility.FromJson<GameData>(File.ReadAllText(savePath));

        data.activeCooldowns.RemoveAll(x => x.spotID == id);
        data.activeCooldowns.Add(new FishingSpotSaveEntry { spotID = id, unlockTime = time });

        File.WriteAllText(savePath, JsonUtility.ToJson(data, true));
    }

    public void RemoveCooldown(string id)
    {
        if (!File.Exists(savePath)) return;
        GameData data = JsonUtility.FromJson<GameData>(File.ReadAllText(savePath));

        int removed = data.activeCooldowns.RemoveAll(x => x.spotID == id);
        if (removed > 0) File.WriteAllText(savePath, JsonUtility.ToJson(data, true));
    }

    // ============================================================================
    // --- НОВИЙ МЕТОД: ФІКСАЦІЯ ПОКАЗУ ВІДГУКУ GOOGLE PLAY ---
    // ============================================================================
    public void OnReviewSuccessfullyShown()
    {
        // Перемикаємо прапорець, щоб більше ніколи не показувати вікно відгуку
        CurrentData.alreadyReviewed = true;

        // Робимо повне збереження JSON, щоб зберегти цю мітку на диску пристрою
        SaveGame();

        Debug.Log("<color=green>[SaveSystem]</color> Стан відгуку збережено: більше вікно викликатися не буде.");
    }
}