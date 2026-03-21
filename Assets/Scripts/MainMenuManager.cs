using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    private void Awake()
    {
        // Фіксуємо ТІЛЬКИ портретний режим
        Screen.orientation = ScreenOrientation.Portrait;
        
        // Додатковий захист: вимикаємо автоповорот
        Screen.autorotateToLandscapeLeft = false;
        Screen.autorotateToLandscapeRight = false;
        Screen.autorotateToPortrait = true;
    }

    public void StartGame()
    {
        SceneManager.LoadScene("GameLevel");
    }
}