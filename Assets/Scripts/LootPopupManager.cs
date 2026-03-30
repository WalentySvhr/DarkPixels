using UnityEngine;
using UnityEngine.UI;
using TMPro; // Якщо використовуєш TextMeshPro, інакше просто UnityEngine.UI.Text
using System.Collections;

public class LootPopupManager : MonoBehaviour
{
    // Одинак (Singleton) для легкого доступу з будь-якого скрипта
    public static LootPopupManager Instance { get; private set; }

    [Header("UI Елементи")]
    public TextMeshProUGUI fishNameText; // Або public Text fishNameText;
    public Image fishIcon;
    public GameObject windowContent; // Об'єкт Panel/Image, який ми будемо вмикати/вимикати

    [Header("Налаштування")]
    public float displayDuration = 2.5f; // Скільки секунд показувати вікно

    private Coroutine hideCoroutine;

    void Awake()
    {
        // Налаштування Одинака
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Не видаляти при зміні сцени (за бажанням)
        }
        else
        {
            Destroy(gameObject);
        }

        // При старті ховаємо вікно
        if (windowContent != null) windowContent.SetActive(false);
    }

    // Головний метод, який викликають інші скрипти
    public void ShowLoot(Item caughtItem)
    {
        if (caughtItem == null || windowContent == null) return;

        // Підставляємо дані
        if (fishNameText != null) fishNameText.text = "Спіймано: " + caughtItem.itemName;
        if (fishIcon != null)
        {
            fishIcon.sprite = caughtItem.icon;
            fishIcon.preserveAspect = true; // Щоб іконка не розтягувалася
        }

        // Показуємо вікно
        windowContent.SetActive(true);

        // Якщо вже йде таймер приховування, зупиняємо його
        if (hideCoroutine != null) StopCoroutine(hideCoroutine);

        // Запускаємо новий таймер
        hideCoroutine = StartCoroutine(HideAfterDelay());
    }

    IEnumerator HideAfterDelay()
    {
        yield return new WaitForSeconds(displayDuration);
        windowContent.SetActive(false);
        hideCoroutine = null;
    }
}