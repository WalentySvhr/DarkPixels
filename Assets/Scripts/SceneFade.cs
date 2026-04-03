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
        fadeImage.enabled = false; // Вимикаємо тільки картинку, а не об'єкт!
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

        // ТЕПЕР ЕКРАН ЧОРНИЙ. Можна тихо змінити орієнтацію
        if (sceneName == "MainMenu")
        {
            Screen.orientation = ScreenOrientation.Portrait;
        }
        else
        {
            Screen.orientation = ScreenOrientation.LandscapeLeft;
        }

        // Даємо системі один кадр, щоб "переварити" поворот
        yield return new WaitForEndOfFrame();

        SceneManager.LoadScene(sceneName);
    }
}