using UnityEngine;
using TMPro; // Не забудьте встановити TextMeshPro
using System.Collections;

public class LocationAnnouncer : MonoBehaviour
{
    public static LocationAnnouncer Instance { get; private set; }

    [Header("UI Елементи")]
    [SerializeField] private TextMeshProUGUI locationText; // Посилання на об'єкт тексту
    [SerializeField] private CanvasGroup canvasGroup;    // Для ефекту прозорості (плавна поява)

    [Header("Налаштування анімації")]
    [SerializeField] private float fadeDuration = 1f;    // Час появи/зникнення
    [SerializeField] private float displayDuration = 2f; // Скільки часу текст тримається

    private Coroutine currentCoroutine;

    private void Awake()
    {
        // Реалізація Singleton
        if (Instance == null)
        {
            Instance = this;
            // Якщо хочете, щоб текст не зникав при переході між сценами:
            // DontDestroyOnLoad(gameObject); 
        }
        else
        {
            Destroy(gameObject);
        }

        // Початковий стан — текст невидимий
        if (canvasGroup != null) canvasGroup.alpha = 0f;
    }

    public void ShowLocation(string name)
    {
        if (locationText == null) return;

        // Зупиняємо попередню анімацію, якщо вона ще йде
        if (currentCoroutine != null) StopCoroutine(currentCoroutine);

        currentCoroutine = StartCoroutine(FadeSequence(name));
    }

    private IEnumerator FadeSequence(string name)
    {
        locationText.text = name;

        // Поява
        yield return StartCoroutine(Fade(1f));

        // Очікування
        yield return new WaitForSeconds(displayDuration);

        // Зникнення
        yield return StartCoroutine(Fade(0f));
    }
    // Замість керування CanvasGroup, міняємо колір тексту
    private IEnumerator Fade(float targetAlpha)
    {
        Color startColor = locationText.color;
        Color endColor = new Color(startColor.r, startColor.g, startColor.b, targetAlpha);
        float time = 0;

        while (time < fadeDuration)
        {
            time += Time.deltaTime;
            locationText.color = Color.Lerp(startColor, endColor, time / fadeDuration);
            yield return null;
        }
    }
}