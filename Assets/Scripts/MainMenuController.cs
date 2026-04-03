using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    public SceneFader fader; // Перетягни сюди FadePanel з інспектора

    private void Awake()
    {
        ApplyMenuOrientation();
    }

    private void Start()
    {
        // Повторна перевірка при старті (корисно для редактора)
        ApplyMenuOrientation();
    }

    private void ApplyMenuOrientation()
    {
        // 1. Фіксуємо портрет
        Screen.orientation = ScreenOrientation.Portrait;

        // 2. Налаштування автоповороту (тільки вертикально)
        Screen.autorotateToPortrait = true;
        Screen.autorotateToPortraitUpsideDown = false;
        Screen.autorotateToLandscapeLeft = false;
        Screen.autorotateToLandscapeRight = false;
    }

    public void StartGame()
    {
        if (fader != null)
        {
            fader.FadeTo("Game");
        }
        else
        {
            Debug.LogError("Забув перетягнути SceneFader в інспекторі!");
            SceneManager.LoadScene("Game");
        }
    }

    // Твій новий метод для захисту орієнтації в меню
    private void OnRectTransformDimensionsChange()
    {
        // Якщо система або редактор спробує зробити Ландшафт — повертаємо в Портрет
        if (Screen.orientation != ScreenOrientation.Portrait)
        {
            Screen.orientation = ScreenOrientation.Portrait;
        }
    }
}