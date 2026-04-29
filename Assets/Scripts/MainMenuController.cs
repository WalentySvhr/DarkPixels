using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    public SceneFader fader;

    private void Awake()
    {
        ApplyMenuOrientation();
    }

    private void Start()
    {
        ApplyMenuOrientation();
    }

    private void ApplyMenuOrientation()
    {
        // 1. Дозволяємо системі самостійно обирати між альбомними режимами
        Screen.orientation = ScreenOrientation.AutoRotation;

        // 2. Дозволяємо тільки альбомні орієнтації
        Screen.autorotateToPortrait = false;
        Screen.autorotateToPortraitUpsideDown = false;
        Screen.autorotateToLandscapeLeft = true;
        Screen.autorotateToLandscapeRight = true;
    }

    public void StartGame()
    {
        if (fader != null)
        {
            fader.FadeTo("Game");
        }
        else
        {
            Debug.LogError("Забув перетягнути SceneFader в інспектора!");
            SceneManager.LoadScene("Game");
        }
    }

    private void OnRectTransformDimensionsChange()
    {
        // Якщо раптом екран став вертикальним (баг або збій системи)
        // Повертаємо його в альбомний режим
        if (Screen.orientation == ScreenOrientation.Portrait ||
            Screen.orientation == ScreenOrientation.PortraitUpsideDown)
        {
            Screen.orientation = ScreenOrientation.AutoRotation;
        }
    }
}