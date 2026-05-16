using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class DailyQuestButtonUI : MonoBehaviour
{
    public static DailyQuestButtonUI Instance { get; private set; }

    [Header("Visual Settings")]
    public Image buttonImage;          // Зображення кнопки (календаря)
    public Color normalColor = Color.white; // Звичайний колір кнопки
    public Color flashColor = Color.yellow;  // Колір, яким вона буде блимати (напр. жовтий або зелений)
    public float flashSpeed = 4f;      // Швидкість блимання

    [Header("Optional Notification Dot")]
    [Tooltip("Червона цятка поверх кнопки (можна залишити пустою)")]
    public GameObject notificationDot;

    private Coroutine flashCoroutine;
    private bool isFlashing = false;

    private void Awake()
    {
        Instance = this;
        if (buttonImage == null) buttonImage = GetComponent<Image>();
    }

    private void Start()
    {
        // Невелика затримка при старті, щоб DailyQuestManager встиг завантажити JSON
        Invoke(nameof(RefreshButtonState), 0.2f);
    }

    // Цей метод ми будемо викликати щоразу, коли змінюється стан квестів
    public void RefreshButtonState()
    {
        if (DailyQuestManager.Instance == null) return;

        // Перевіряємо, чи є хоч один виконаний квест, нагороду за який ще не забрали
        if (DailyQuestManager.Instance.HasUnclaimedRewards())
        {
            StartFlashing();
        }
        else
        {
            StopFlashing();
        }
    }

    private void StartFlashing()
    {
        if (isFlashing) return;
        isFlashing = true;

        if (notificationDot != null) notificationDot.SetActive(true);
        flashCoroutine = StartCoroutine(FlashRoutine());
    }

    private void StopFlashing()
    {
        if (!isFlashing) return;
        isFlashing = false;

        if (flashCoroutine != null) StopCoroutine(flashCoroutine);
        if (notificationDot != null) notificationDot.SetActive(false);

        // Повертаємо кнопці звичайний колір
        if (buttonImage != null) buttonImage.color = normalColor;
    }

    // Корутина для плавного пульсування кольору туди-сюди
    private IEnumerator FlashRoutine()
    {
        while (isFlashing)
        {
            // Використовуємо математичний синус для створення гладкої хвилі (від 0 до 1)
            float pingPong = (Mathf.Sin(Time.time * flashSpeed) + 1f) / 2f;

            if (buttonImage != null)
            {
                buttonImage.color = Color.Lerp(normalColor, flashColor, pingPong);
            }
            yield return null;
        }
    }
}