using UnityEngine;
using System.Collections.Generic;

public class InventoryUI : MonoBehaviour
{
    [Header("Обов'язкові посилання")]
    public Transform itemsParent; // Об'єкт Grid (сітка інвентарю)
    public InventoryManager inventory; // Посилання на InventoryManager

    // --- НОВЕ: Панелі вкладок для перемикання ---
    [Header("Панелі Вкладок")]
    [Tooltip("Головний об'єкт Grid або ScrollView зі звичайними предметами")]
    public GameObject itemsPanel;
    [Tooltip("Новий об'єкт Grid або ScrollView, де налаштований PetInventoryUI")]
    public GameObject petsPanel;

    private InventorySlot[] slots; // Список слотів у сітці
    private bool isPetsTabActive = false; // Прапорець, яка вкладка зараз відкрита

    void Awake()
    {
        // Знаходимо слоти один раз при старті
        RefreshSlots();

        // Автоматично реєструємо цей UI в InventoryManager, якщо забув перетягнути в інспекторі
        if (inventory == null) inventory = InventoryManager.Instance;
        if (inventory != null) inventory.inventoryUI = this;
    }

    void Start()
    {
        // При старті завжди відкриваємо звичайні предмети
        ShowItemsTab();
    }

    public void RefreshSlots()
    {
        if (itemsParent != null)
        {
            slots = itemsParent.GetComponentsInChildren<InventorySlot>(true);
        }
    }

    public void UpdateUI()
    {
        if (inventory == null) return;

        // Якщо зараз відкрита вкладка петів, звичайний інвентар оновлювати не треба
        if (isPetsTabActive) return;

        if (slots == null || slots.Length == 0) RefreshSlots();

        // Малюємо те, що є в сумці, у слотах:
        for (int i = 0; i < slots.Length; i++)
        {
            if (i < inventory.items.Count)
            {
                slots[i].AddItem(inventory.items[i].item, inventory.items[i].amount);
            }
            else
            {
                slots[i].ClearSlot();
            }
        }
    }

    // ==========================================
    // --- НОВІ МЕТОДИ: Перемикання вкладок ---
    // ==========================================

    // Метод для кнопки "Предмети"
    public void ShowItemsTab()
    {
        isPetsTabActive = false;

        if (itemsPanel != null) itemsPanel.SetActive(true);
        if (petsPanel != null) petsPanel.SetActive(false);

        UpdateUI(); // Перемальовуємо звичайні предмети
    }

    // Метод для кнопки "Помічники" (Петі)
    public void ShowPetsTab()
    {
        isPetsTabActive = true;

        if (itemsPanel != null) itemsPanel.SetActive(false);
        if (petsPanel != null) petsPanel.SetActive(true);

        // Викликаємо оновлення сітки петів через менеджер
        if (inventory != null)
        {
            inventory.UpdatePetUI();
        }
    }

    // Автоматично оновлюємо UI при відкритті вікна інвентарю
    void OnEnable()
    {
        // При відкритті всього вікна повертаємося на вкладку предметів, або просто оновлюємо поточну
        if (isPetsTabActive)
            ShowPetsTab();
        else
            ShowItemsTab();
    }
}