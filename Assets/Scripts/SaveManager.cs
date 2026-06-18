using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine.SceneManagement;
using System.Linq;
using TMPro;

[System.Serializable]
public class GameData
{
    public float posX, posY, posZ;
    public int coins;
    public int diamonds;
    public int inventorySpace = 20;
    public int petSpace = 6;
    public float currentHealth;
    public List<ItemSaveEntry> backpack = new List<ItemSaveEntry>();
    public List<ItemSaveEntry> petBackpack = new List<ItemSaveEntry>();
    public List<EquippedItemSaveEntry> equippedItems = new List<EquippedItemSaveEntry>();

    // --- НОВЕ: Список для збереження куплених рівнів магій ---
    public List<AbilitySaveEntry> unlockedAbilities = new List<AbilitySaveEntry>();
    public string equippedAbilityID; // Збереження ID вміння на кнопці

    public string currentQuestID;
    public int questProgress;
    public List<FishingSpotSaveEntry> activeCooldowns = new List<FishingSpotSaveEntry>();
    public List<string> unlockedTrueObjects = new List<string>();
    public List<string> completedQuestIDs = new List<string>();

    public int diamondAdsWatched;
    public float diamondAdsCooldown;
    public bool isDiamondAdOnCooldown;

    public int maxTowerFloor;
    public int maxTowerKills;
    public int victoryCount;
    public bool alreadyReviewed;
}

// --- НОВЕ: Клас-контейнер для збереження однієї абілки ---
[System.Serializable]
public class AbilitySaveEntry
{
    public string abilityKey; // Унікальний ключ (наприклад, "Ability_Damage_Zone_Level")
    public int level;         // Куплений рівень
}

[System.Serializable]
public class EquippedItemSaveEntry
{
    public string slotKey;
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

    [Header("Auto Save Settings")]
    public bool enableAutoSave = true;
    [Tooltip("Інтервал автозбереження в секундах")]
    public float autoSaveInterval = 60f;
    private Coroutine autoSaveCoroutine;

    [Header("UI Settings")]
    [Tooltip("Перетягни сюди головний об'єкт індикатора з Canvas")]
    public GameObject autoSaveIndicatorUI;

    [Tooltip("Перетягни сюди компонент TextMeshPro, який знаходиться всередині індикатора")]
    public TextMeshProUGUI saveNotificationText;

    [Tooltip("Текст, який буде виводитися при АВТОМАТИЧНОМУ збереженні")]
    public string autoSaveMessage = "Автозбереження...";

    [Tooltip("Чи показувати цей індикатор при РУЧНОМУ збереженні?")]
    public bool showUiOnManualSave = true;

    [Tooltip("Текст, який буде виводитися при РУЧНОМУ збереженні")]
    public string manualSaveMessage = "Гру збережено!";

    [Tooltip("Скільки секунд індикатор буде висіти на екрані")]
    public float uiDisplayDuration = 2.5f;
    private Coroutine uiAnimationCoroutine;

    public GameData CurrentData { get; private set; } = new GameData();

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            savePath = Path.Combine(Application.persistentDataPath, "gamesave.json");

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

    void Start()
    {
        if (autoSaveIndicatorUI != null) autoSaveIndicatorUI.SetActive(false);

        if (enableAutoSave)
        {
            StartAutoSave();
        }
    }

    public void StartAutoSave()
    {
        if (autoSaveCoroutine != null) StopCoroutine(autoSaveCoroutine);
        autoSaveCoroutine = StartCoroutine(AutoSaveLoop());
    }

    private IEnumerator AutoSaveLoop()
    {
        while (enableAutoSave)
        {
            yield return new WaitForSeconds(autoSaveInterval);
            SaveGame(false);
        }
    }

    private IEnumerator ShowSaveIndicator(string message)
    {
        if (autoSaveIndicatorUI != null)
        {
            if (saveNotificationText != null)
            {
                saveNotificationText.text = message;
            }

            autoSaveIndicatorUI.SetActive(true);
            yield return new WaitForSeconds(uiDisplayDuration);
            autoSaveIndicatorUI.SetActive(false);
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

    public bool SaveGame(bool isManual = true)
    {
        if (isManual)
        {
            Debug.Log("<color=orange>ЗБЕРЕЖЕННЯ ВИКЛИКАНО ЗІ СКРИПТА:</color>\n" + StackTraceUtility.ExtractStackTrace());
        }

        if (!SaveForbiddenZone.CanSave)
        {
            if (isManual) Debug.LogWarning("Збереження неможливе всередині башта!");
            return false;
        }

        if (TowerManager.Instance != null && TowerManager.Instance.IsTowerRunActive)
        {
            return false;
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

            data.equippedItems.Clear();
            foreach (var pair in InventoryManager.Instance.equippedItems)
            {
                if (pair.Value != null)
                {
                    data.equippedItems.Add(new EquippedItemSaveEntry { slotKey = pair.Key, itemName = pair.Value.name });
                }
            }

            data.backpack.Clear();
            foreach (var stack in InventoryManager.Instance.items)
            {
                if (stack?.item != null)
                    data.backpack.Add(new ItemSaveEntry { itemName = stack.item.name, amount = stack.amount });
            }

            data.petBackpack.Clear();
            foreach (var stack in InventoryManager.Instance.petItems)
            {
                if (stack?.item != null)
                    data.petBackpack.Add(new ItemSaveEntry { itemName = stack.item.name, amount = stack.amount });
            }

            data.inventorySpace = InventoryManager.Instance.space;
            data.petSpace = InventoryManager.Instance.petSpace;
        }

        // --- НОВЕ: ЗБЕРЕЖЕННЯ РІВНІВ МАГІЙ ---
        data.unlockedAbilities.Clear();
        AbilitySO[] allAbilities = Resources.LoadAll<AbilitySO>("Abilities");
        foreach (var ability in allAbilities)
        {
            if (ability != null)
            {
                data.unlockedAbilities.Add(new AbilitySaveEntry
                {
                    abilityKey = ability.GetSaveKey(),
                    level = ability.currentLevel
                });
            }
        }

        // 🌟 ОНОВЛЕННЯ: ЗБЕРЕЖЕННЯ ЕКІПІРОВАНОГО СКІЛА НА КНОПЦІ
        if (AbilityManager.Instance != null)
        {
            data.equippedAbilityID = AbilityManager.Instance.GetEquippedAbilityIDForSave();
        }
        else
        {
            // Якщо менеджер недоступний, залишаємо попереднє значення, щоб не втратити його
            data.equippedAbilityID = CurrentData.equippedAbilityID;
        }

        if (QuestManager.Instance != null) data = QuestManager.Instance.CaptureQuestState(data);
        if (AdsChecker.Instance != null) data = AdsChecker.Instance.CaptureAdsState(data);
        if (TowerManager.Instance != null) data = TowerManager.Instance.CaptureTowerState(data);

        PlayerHealth health = Object.FindFirstObjectByType<PlayerHealth>();
        if (health != null) data.currentHealth = health.currentHealth;

        data.victoryCount = CurrentData.victoryCount;
        data.alreadyReviewed = CurrentData.alreadyReviewed;

        CurrentData = data;

        File.WriteAllText(savePath, JsonUtility.ToJson(data, true));

        if (isManual)
            Debug.Log("<color=green>[SaveSystem]</color> Дані успішно зафіксовані (Ручне збереження).");
        else
            Debug.Log("<color=green>[SaveSystem]</color> Дані успішно зафіксовані (Автозбереження).");

        if (isManual && showUiOnManualSave)
        {
            if (uiAnimationCoroutine != null) StopCoroutine(uiAnimationCoroutine);
            uiAnimationCoroutine = StartCoroutine(ShowSaveIndicator(manualSaveMessage));
        }
        else if (!isManual)
        {
            if (uiAnimationCoroutine != null) StopCoroutine(uiAnimationCoroutine);
            uiAnimationCoroutine = StartCoroutine(ShowSaveIndicator(autoSaveMessage));
        }

        return true;
    }

    public void LoadGame()
    {
        if (!File.Exists(savePath)) return;

        GameData data = JsonUtility.FromJson<GameData>(File.ReadAllText(savePath));
        CurrentData = data;

        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.coins = data.coins;
            InventoryManager.Instance.diamonds = data.diamonds;

            InventoryManager.Instance.equippedItems.Clear();
            InventoryManager.Instance.UpdateCoinUI();
            InventoryManager.Instance.UpdateDiamondUI();

            InventoryManager.Instance.items.Clear();
            foreach (var entry in data.backpack)
            {
                Item loadedItem = Resources.Load<Item>("Items/" + entry.itemName);
                if (loadedItem != null)
                    InventoryManager.Instance.items.Add(new InventoryManager.ItemStack(loadedItem, entry.amount));
            }

            if (data.inventorySpace > 0)
                InventoryManager.Instance.space = data.inventorySpace;
            if (data.petSpace > 0)
                InventoryManager.Instance.petSpace = data.petSpace;

            InventoryManager.Instance.UpdateUI();

            InventoryManager.Instance.petItems.Clear();
            foreach (var entry in data.petBackpack)
            {
                Item loadedPet = Resources.Load<Item>("Items/" + entry.itemName);
                if (loadedPet != null)
                    InventoryManager.Instance.petItems.Add(new InventoryManager.ItemStack(loadedPet, entry.amount));
            }
            InventoryManager.Instance.UpdatePetUI();
        }

        // --- ЗАВАНТАЖЕННЯ РІВНІВ МАГІЙ ---
        AbilitySO[] allAbilities = Resources.LoadAll<AbilitySO>("Abilities");
        foreach (var ability in allAbilities)
        {
            if (ability != null)
            {
                ability.SetLoadedLevel(0);

                var savedEntry = data.unlockedAbilities.Find(x => x.abilityKey == ability.GetSaveKey());
                if (savedEntry != null)
                {
                    ability.SetLoadedLevel(savedEntry.level);
                }
                else
                {
                    ability.SetLoadedLevel(0);
                }

#if UNITY_EDITOR
                UnityEditor.EditorUtility.SetDirty(ability);
#endif
            }
        }

#if UNITY_EDITOR
        UnityEditor.AssetDatabase.SaveAssets();
#endif

        if (QuestManager.Instance != null) QuestManager.Instance.LoadQuestState(data);
        if (AdsChecker.Instance != null) AdsChecker.Instance.LoadAdsState(data);
        if (TowerManager.Instance != null) TowerManager.Instance.LoadTowerState(data);

        PlayerHealth health = Object.FindFirstObjectByType<PlayerHealth>();
        if (health != null) health.currentHealth = (int)data.currentHealth;

        StopAllCoroutines();
        if (autoSaveIndicatorUI != null) autoSaveIndicatorUI.SetActive(false);

        if (enableAutoSave) StartAutoSave();

        StartCoroutine(ApplyEquipmentAfterLoad(data));
        StartCoroutine(ApplyPlayerPositionAfterLoad(data));
    }

    private System.Collections.IEnumerator ApplyEquipmentAfterLoad(GameData data)
    {
        yield return new WaitForEndOfFrame();

        if (InventoryManager.Instance == null)
        {
            Debug.LogError("[SaveSystem] InventoryManager не знайдено! Неможливо відновити екіпіровку.");
            yield break;
        }

        InventorySlot[] allSlots = Object.FindObjectsByType<InventorySlot>(FindObjectsInactive.Include, FindObjectsSortMode.None);

        foreach (var slot in allSlots)
        {
            string key = "";
            string baseSlotType = "";

            if (slot.isWeaponEquipmentSlot) { key = "Weapon"; baseSlotType = "Weapon"; }
            else if (slot.isAmuletEquipmentSlot) { key = "Amulet"; baseSlotType = "Amulet"; }
            else if (slot.isBeltEquipmentSlot) { key = "Belt"; baseSlotType = "Belt"; }
            else if (slot.isRingEquipmentSlot) { key = $"Ring_{slot.ringSlotIndex}"; baseSlotType = "Ring"; }
            else if (slot.isPetEquipmentSlot) { key = "Pet"; baseSlotType = "Pet"; }
            else if (slot.isHelmetEquipmentSlot) { key = "Helmet"; baseSlotType = "Helmet"; }
            else if (slot.isChestplateEquipmentSlot) { key = "Chestplate"; baseSlotType = "Chestplate"; }
            else if (slot.isBracersEquipmentSlot) { key = "Bracers"; baseSlotType = "Bracers"; }
            else if (slot.isBootsEquipmentSlot) { key = "Boots"; baseSlotType = "Boots"; }

            if (string.IsNullOrEmpty(key)) continue;

            var entry = data.equippedItems.Find(x => x.slotKey == key);
            if (entry != null && !string.IsNullOrEmpty(entry.itemName))
            {
                Item item = Resources.Load<Item>("Items/" + entry.itemName);
                if (item != null)
                {
                    slot.AddItem(item, 1);
                    InventoryManager.Instance.EquipItem(item, baseSlotType, slot.ringSlotIndex);
                }
            }
        }
    }

    private System.Collections.IEnumerator ApplyPlayerPositionAfterLoad(GameData data)
    {
        yield return new WaitForEndOfFrame();

        GameObject player = GameObject.FindGameObjectWithTag("Player");

        if (player != null)
        {
            Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
            RigidbodyType2D originalBodyType = RigidbodyType2D.Dynamic;

            if (rb != null)
            {
                originalBodyType = rb.bodyType;
                rb.bodyType = RigidbodyType2D.Kinematic;
                rb.linearVelocity = Vector2.zero;
                rb.position = new Vector2(data.posX, data.posY);
            }

            player.transform.position = new Vector3(data.posX, data.posY, data.posZ);

            if (rb != null)
            {
                rb.bodyType = originalBodyType;
            }

            var vcam = Object.FindFirstObjectByType<Cinemachine.CinemachineVirtualCamera>();
            if (vcam != null)
            {
                vcam.PreviousStateIsValid = false;
            }

            Debug.Log($"<color=cyan>[SaveSystem]</color> Позиція гравця відновлена. Камера Cinemachine відцентрована.");
        }
    }

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

    public void OnReviewSuccessfullyShown()
    {
        CurrentData.alreadyReviewed = true;
        SaveGame();
    }
}