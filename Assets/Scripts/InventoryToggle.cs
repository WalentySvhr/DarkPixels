using UnityEngine;

public class InventoryToggle : MonoBehaviour
{
    [Header("Налаштування вікна")]
    public GameObject inventoryPanel; // Сюди перетягни панель інвентарю

    // Цей метод відкриває/закриває (Toggle)
    public void Toggle()
    {
        if (inventoryPanel == null) return;

        bool nextState = !inventoryPanel.activeSelf;
        inventoryPanel.SetActive(nextState);

        // === ГЛОБАЛЬНИЙ ЗАПОБІЖНИК (ОНОВЛЕНО НА ЛІЧИЛЬНИК) ===
        if (nextState)
        {
            UIManager.RegisterWindowOpen();
        }
        else
        {
            UIManager.RegisterWindowClose();
        }

        // Якщо відкрили — оновлюємо іконки
        if (nextState) UpdateUIInPanel();
    }

    // Окремий метод ТІЛЬКИ для закриття (зручно для кнопки "Х")
    public void CloseInventory()
    {
        if (inventoryPanel != null)
        {
            // Перевіряємо, чи інвентар взагалі був відкритий, 
            // щоб не зменшувати лічильник в UIManager вхолосту
            if (inventoryPanel.activeSelf)
            {
                inventoryPanel.SetActive(false);

                // === ГЛОБАЛЬНИЙ ЗАПОБІЖНИК (ОНОВЛЕНО НА ЛІЧИЛЬНИК) ===
                UIManager.RegisterWindowClose();
            }
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