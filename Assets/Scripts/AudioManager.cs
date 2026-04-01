using System.Collections;
using UnityEngine;

public class AudioFadeIn : MonoBehaviour
{
    public AudioSource audioSource;
    public float fadeDuration = 2.0f; // Скільки секунд триватиме наростання
    public float targetVolume = 0.5f; // Фінальна гучність (від 0 до 1)

    void Start()
    {
        // Переконуємося, що початкова гучність 0
        audioSource.volume = 0;
        audioSource.Play();

        // Запускаємо плавне наростання
        StartCoroutine(StartFadeIn());
    }

    IEnumerator StartFadeIn()
    {
        float currentTime = 0;

        while (currentTime < fadeDuration)
        {
            currentTime += Time.deltaTime;
            // Плавно змінюємо гучність від 0 до targetVolume
            audioSource.volume = Mathf.Lerp(0, targetVolume, currentTime / fadeDuration);
            yield return null;
        }

        audioSource.volume = targetVolume;
    }
}