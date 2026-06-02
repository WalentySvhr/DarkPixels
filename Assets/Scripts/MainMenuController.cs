using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    public SceneFader fader;

    // Ми прибрали Awake, Start та OnRectTransformDimensionsChange, 
    // бо за орієнтацію тепер відповідає ваш глобальний менеджер.

    public void StartGame()
    {
        // Примусово фіксуємо екран перед переходом, щоб уникнути мікро-багів Unity
        Screen.orientation = ScreenOrientation.LandscapeLeft;

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
}