using UnityEngine;

public class InventoryToggle : MonoBehaviour
{
    [Header("Налаштування вікна")]
    public GameObject inventoryPanel; // Сюди перетягни панель інвентарю

    // Цей метод відкриває/закриває (Toggle)
    public void Toggle()
    {
        if (inventoryPanel == null) return;

        bool isActive = inventoryPanel.activeSelf;
        inventoryPanel.SetActive(!isActive);

        // Якщо відкрили — оновлюємо іконки
        if (!isActive) UpdateUIInPanel();
    }

    // Окремий метод ТІЛЬКИ для закриття (зручно для кнопки "Х")
    public void CloseInventory()
    {
        if (inventoryPanel != null)
        {
            inventoryPanel.SetActive(false);
        }
    }

    // Допоміжний метод для оновлення картинок
    private void UpdateUIInPanel()
    {
        InventoryUI ui = inventoryPanel.GetComponentInChildren<InventoryUI>();
        if (ui != null) ui.UpdateUI();
    }
    void Update()
    {
        // Клавіша Escape в Unity на Android відповідає системній кнопці "Назад"
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (inventoryPanel != null && inventoryPanel.activeSelf)
            {
                CloseInventory();
            }
        }
    }
}