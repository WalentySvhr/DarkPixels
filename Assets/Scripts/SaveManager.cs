using UnityEngine;
using System.Collections.Generic;
using System.IO;
using UnityEngine.SceneManagement;

[System.Serializable]
public class GameData
{
    public float posX, posY, posZ;
    public int coins;
    public float currentHealth;
    public List<ItemSaveEntry> backpack = new List<ItemSaveEntry>();
    public string equippedWeaponName;
    public string equippedAmuletName;
    public string currentQuestID;
    public int questProgress;
    public List<FishingSpotSaveEntry> activeCooldowns = new List<FishingSpotSaveEntry>();
    public List<string> completedQuestIDs = new List<string>();
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

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            savePath = Path.Combine(Application.persistentDataPath, "gamesave.json");
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

    // ... (початок коду без змін)

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

        // --- ЗБЕРЕЖЕННЯ ІНВЕНТАРЯ ТА ГРОШЕЙ ---
        if (InventoryManager.Instance != null)
        {
            data.coins = InventoryManager.Instance.coins;
            data.equippedWeaponName = InventoryManager.Instance.currentWeaponName;
            data.equippedAmuletName = InventoryManager.Instance.currentAmuletName;

            data.backpack.Clear();
            foreach (var stack in InventoryManager.Instance.items)
            {
                if (stack?.item != null)
                    data.backpack.Add(new ItemSaveEntry { itemName = stack.item.name, amount = stack.amount });
            }
        }

        // --- ДОДАНО: ЗБЕРЕЖЕННЯ КВЕСТІВ ---
        if (QuestManager.Instance != null)
        {
            data = QuestManager.Instance.CaptureQuestState(data);
        }

        PlayerHealth health = Object.FindFirstObjectByType<PlayerHealth>();
        if (health != null) data.currentHealth = health.currentHealth;

        // Записуємо все в JSON
        File.WriteAllText(savePath, JsonUtility.ToJson(data, true));
        Debug.Log("<color=green>[SaveSystem]</color> Дані успішно зафіксовані.");
    }

    public void LoadGame()
    {
        if (!File.Exists(savePath)) return;

        GameData data = JsonUtility.FromJson<GameData>(File.ReadAllText(savePath));
        GameObject player = GameObject.FindGameObjectWithTag("Player");

        // --- ВІДНОВЛЕННЯ ПОЗИЦІЇ ---
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

        // --- ВІДНОВЛЕННЯ ІНВЕНТАРЯ ---
        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.coins = data.coins;
            InventoryManager.Instance.currentWeaponName = "";
            InventoryManager.Instance.currentAmuletName = "";
            InventoryManager.Instance.UpdateCoinUI();

            InventoryManager.Instance.items.Clear();
            foreach (var entry in data.backpack)
            {
                Item loadedItem = Resources.Load<Item>("Items/" + entry.itemName);
                if (loadedItem != null)
                    InventoryManager.Instance.items.Add(new InventoryManager.ItemStack(loadedItem, entry.amount));
            }
            InventoryManager.Instance.UpdateUI();
        }

        // --- ДОДАНО: ВІДНОВЛЕННЯ КВЕСТІВ ---
        if (QuestManager.Instance != null)
        {
            QuestManager.Instance.LoadQuestState(data);
        }

        PlayerHealth health = Object.FindFirstObjectByType<PlayerHealth>();
        if (health != null) health.currentHealth = (int)data.currentHealth;

        StopAllCoroutines();
        StartCoroutine(ApplyEquipmentAfterLoad(data));
    }

    // ... (решта коду ApplyEquipmentAfterLoad та методів для рибалки без змін)

    private System.Collections.IEnumerator ApplyEquipmentAfterLoad(GameData data)
    {
        // Збільшив час затримки, щоб UI точно встиг намалюватися.
        // Це часто буває проблемою в збірках на телефоні/симуляторі.
        yield return new WaitForSeconds(0.5f);

        InventorySlot[] allSlots = Object.FindObjectsByType<InventorySlot>(FindObjectsInactive.Include, FindObjectsSortMode.None);

        foreach (var slot in allSlots)
        {
            if (slot.isWeaponEquipmentSlot && !string.IsNullOrEmpty(data.equippedWeaponName))
            {
                Item wep = Resources.Load<Item>("Items/" + data.equippedWeaponName);
                if (wep != null)
                {
                    slot.AddItem(wep, 1);
                    Debug.Log($"<color=yellow>[SaveSystem]</color> Відновлено зброю: {wep.name}");
                }
            }

            if (slot.isAmuletEquipmentSlot && !string.IsNullOrEmpty(data.equippedAmuletName))
            {
                Item amu = Resources.Load<Item>("Items/" + data.equippedAmuletName);
                if (amu != null)
                {
                    slot.AddItem(amu, 1);
                    Debug.Log($"<color=yellow>[SaveSystem]</color> Відновлено амулет: {amu.name}");
                }
            }
        }
    }
    // --- МЕТОДИ ДЛЯ РИБАЛКИ ---
    public List<FishingSpotSaveEntry> GetActiveCooldowns()
    {
        // Читаємо файл, щоб дізнатися поточні перезарядки
        if (!File.Exists(savePath)) return new List<FishingSpotSaveEntry>();
        GameData data = JsonUtility.FromJson<GameData>(File.ReadAllText(savePath));
        return data.activeCooldowns;
    }

    public void RegisterCooldown(string id, long time)
    {
        GameData data = new GameData();
        if (File.Exists(savePath)) data = JsonUtility.FromJson<GameData>(File.ReadAllText(savePath));

        // Видаляємо старий запис, якщо він був, і додаємо новий
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
}