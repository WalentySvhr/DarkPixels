using UnityEngine;
using System.Collections.Generic;

public class InventoryUI : MonoBehaviour
{
    [Header("Обов'язкові посилання")]
    public Transform itemsParent; // Об'єкт Grid (сітка інвентарю)
    public InventoryManager inventory; // Посилання на InventoryManager

    // --- Панелі вкладок для перемикання ---
    [Header("Панелі Вкладок")]
    [Tooltip("Головний об'єкт Grid або ScrollView зі звичайними предметами")]
    public GameObject itemsPanel;
    [Tooltip("Новий об'єкт Grid або ScrollView, де налаштований PetInventoryUI")]
    public GameObject petsPanel;

    private InventorySlot[] slots; // Список усіх слотів у сітці (максимальна кількість)
    private bool isPetsTabActive = false; // Прапорець, яка вкладка зараз відкрита

    void Awake()
    {
        // Знаходимо всі слоти (включаючи приховані) один раз при старті
        RefreshSlots();

        // Автоматично реєструємо цей UI в InventoryManager, якщо забули перетягнути в інспекторі
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
            // Передаємо 'true' у параметри, щоб Unity знаходив навіть неактивні (вимкнені) GameObjects
            slots = itemsParent.GetComponentsInChildren<InventorySlot>(true);
        }
    }

    public void UpdateUI()
    {
        if (inventory == null) return;

        // Якщо зараз відкрита вкладка петів, звичайний інвентар оновлювати не треба
        if (isPetsTabActive) return;

        if (slots == null || slots.Length == 0) RefreshSlots();

        // Отримуємо актуальний ліміт куплених слотів з InventoryManager
        int allowedSpace = inventory.space;

        // Проходимо циклом по абсолютно всіх ручних префабах, що лежать в itemsParent (наприклад, по всіх 40)
        for (int i = 0; i < slots.Length; i++)
        {
            // Якщо індекс слота менший за поточний ліміт — гравець його вже купив/має
            if (i < allowedSpace)
            {
                // Активуємо слот, якщо він був прихований
                if (!slots[i].gameObject.activeSelf)
                {
                    slots[i].gameObject.SetActive(true);
                }

                // Відображаємо предмет, якщо він є у списку сумки
                if (i < inventory.items.Count)
                {
                    slots[i].AddItem(inventory.items[i].item, inventory.items[i].amount);
                }
                else
                {
                    slots[i].ClearSlot(); // Якщо предметів менше, ніж куплених слотів — слот порожній
                }
            }
            else
            {
                // Якщо індекс слота виходить за рамки купленного простору — повністю ховаємо його з UI
                if (slots[i].gameObject.activeSelf)
                {
                    slots[i].gameObject.SetActive(false);
                }
            }
        }
    }

    // ==========================================
    // --- МЕТОДИ: Перемикання вкладок ---
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
        if (isPetsTabActive)
            ShowPetsTab();
        else
            ShowItemsTab();
    }
}