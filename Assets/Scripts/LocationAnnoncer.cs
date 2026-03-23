using UnityEngine;
using TMPro;
using System.Collections;

public class LocationAnnouncer : MonoBehaviour
{
    public TextMeshProUGUI locationText; // Перетягни сюди LocationNameText
    public float fadeInDuration = 1.5f;   // Час появи
    public float stayDuration = 2.0f;    // Скільки текст висить на екрані
    public float fadeOutDuration = 1.5f;  // Час зникнення

    private Coroutine currentFade;

    void Start()
    {
        // На початку текст має бути порожнім і прозорим
        if (locationText != null)
        {
            Color c = locationText.color;
            c.a = 0;
            locationText.color = c;
        }
    }

    // Головний метод, який ми будемо викликати при ТП
    public void ShowLocation(string name)
    {
        if (locationText == null) return;

        // Якщо попередня анімація ще йде — зупиняємо її
        if (currentFade != null) StopCoroutine(currentFade);
        
        currentFade = StartCoroutine(FadeSequence(name));
    }

    private IEnumerator FadeSequence(string name)
    {
        locationText.text = name;

        // Плавна поява (Fade In)
        yield return StartCoroutine(FadeText(0, 1, fadeInDuration));

        // Пауза (Текст висить на екрані)
        yield return new WaitForSeconds(stayDuration);

        // Плавне зникнення (Fade Out)
        yield return StartCoroutine(FadeText(1, 0, fadeOutDuration));
    }

    private IEnumerator FadeText(float startAlpha, float endAlpha, float duration)
    {
        float elapsed = 0;
        Color c = locationText.color;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            c.a = Mathf.Lerp(startAlpha, endAlpha, elapsed / duration);
            locationText.color = c;
            yield return null;
        }
    }
}