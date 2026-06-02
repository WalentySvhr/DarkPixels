using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class SceneFader : MonoBehaviour
{
    public Image fadeImage;
    public float fadeSpeed = 1.5f;

    void Awake()
    {
        // При старті сцени робимо її прозорою (Fade In)
        fadeImage.enabled = true;
        StartCoroutine(FadeIn());
    }

    public void FadeTo(string sceneName)
    {
        fadeImage.enabled = true;
        StartCoroutine(FadeOut(sceneName));
    }

    IEnumerator FadeIn()
    {
        float t = 1f;
        while (t > 0f)
        {
            t -= Time.deltaTime * fadeSpeed;
            fadeImage.color = new Color(0, 0, 0, t);
            yield return null;
        }
        fadeImage.enabled = false; // Вимикаємо тільки картинку, а не об'єкт
    }

    IEnumerator FadeOut(string sceneName)
    {
        fadeImage.enabled = true;
        float t = 0f;

        while (t < 1f) // Чекаємо, поки екран почорніє
        {
            t += Time.deltaTime * fadeSpeed;
            fadeImage.color = new Color(0, 0, 0, t);
            yield return null;
        }

        // Більше ніяких маніпуляцій з орієнтацією тут! 
        // Цим займається GameSceneManager. Просто вантажимо сцену.
        SceneManager.LoadScene(sceneName);
    }
}