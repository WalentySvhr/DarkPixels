using UnityEngine;

public class UIManager : MonoBehaviour
{
    private static int activeWindowsCount = 0;

    // Посилання на головні панелі (перетягніть їх в інспекторі до UIManager)
    [Header("Головні панелі для перевірки")]
    public GameObject inventoryPanel;
    public GameObject dialogPanel;
    public GameObject shopPanel;
    public GameObject dailyQuestPanel;

    public static bool IsAnyWindowOpen
    {
        get
        {
            // Якщо лічильник каже, що щось відкрито, але ми хочемо 100% впевненості:
            if (activeWindowsCount > 0) return true;
            return false;
        }
    }

    public static void RegisterWindowOpen()
    {
        activeWindowsCount++;
        if (activeWindowsCount < 0) activeWindowsCount = 1; // Страховка від мінусу
        Debug.Log($"[UIManager] Вікно відкрито. Активних вікон: {activeWindowsCount}");
    }

    public static void RegisterWindowClose()
    {
        activeWindowsCount--;

        // ЗАХИСТ 1: Лічильник не може бути меншим за нуль
        if (activeWindowsCount < 0) activeWindowsCount = 0;

        Debug.Log($"[UIManager] Вікно закрито. Активних вікон: {activeWindowsCount}");
    }

    // Супер-метод захисту: викликайте його, наприклад, при завантаженні сцени або якщо щось зависло
    public void ForceResetCounter()
    {
        activeWindowsCount = 0;

        // Автоматично перевіряємо реальний стан вікон, якщо вони підключені
        if (inventoryPanel != null && inventoryPanel.activeSelf) activeWindowsCount++;
        if (dialogPanel != null && dialogPanel.activeSelf) activeWindowsCount++;
        if (shopPanel != null && shopPanel.activeSelf) activeWindowsCount++;
        if (dailyQuestPanel != null && dailyQuestPanel.activeSelf) activeWindowsCount++;

        Debug.Log($"[UIManager] Виконано примусове скидання. Реально активних вікон: {activeWindowsCount}");
    }
}