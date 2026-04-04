using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class LootPopupManager : MonoBehaviour
{
    public static LootPopupManager Instance { get; private set; }

    [Header("UI Елементи")]
    public TextMeshProUGUI fishNameText;
    public Image fishIcon;
    public GameObject windowContent;

    [Header("Налаштування")]
    public float displayDuration = 2.5f;

    private Coroutine hideCoroutine;

    void Awake()
    {
        // Безпечний Синглтон для UI-елементів
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // При старті ховаємо вікно
        if (windowContent != null) windowContent.SetActive(false);
    }

    public void ShowLoot(Item caughtItem)
    {
        if (caughtItem == null || windowContent == null) return;

        // Підставляємо дані
        if (fishNameText != null) fishNameText.text = "Спіймано: " + caughtItem.itemName;
        if (fishIcon != null)
        {
            fishIcon.sprite = caughtItem.icon;
            fishIcon.preserveAspect = true;
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