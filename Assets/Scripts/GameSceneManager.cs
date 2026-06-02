using UnityEngine;
using UnityEngine.SceneManagement;

public class GameSceneManager : MonoBehaviour
{
    private static GameSceneManager _instance;

    private void Awake()
    {
        // Робимо об'єкт глобальним
        if (_instance != null)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;
        DontDestroyOnLoad(gameObject);

        // Підписуємося на подію: щоразу, коли завантажується нова сцена, 
        // буде викликатися метод OnSceneLoaded
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        // Обов'язково відписуємося при знищенні об'єкта, щоб уникнути помилок
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    // Цей метод автоматично спрацьовує після КОЖНОГО завантаження сцени
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        ApplyGameOrientation();
    }

    private void ApplyGameOrientation()
    {
        // 1. Спочатку жорстко форсуємо ландшафт, щоб "збити" портретний режим, 
        // який міг проскочити під час завантаження
        Screen.orientation = ScreenOrientation.LandscapeLeft;

        // 2. Налаштовуємо дозволені кути
        Screen.autorotateToPortrait = false;
        Screen.autorotateToPortraitUpsideDown = false;
        Screen.autorotateToLandscapeLeft = true;
        Screen.autorotateToLandscapeRight = true;

        // 3. Вмикаємо автоповорот
        Screen.orientation = ScreenOrientation.AutoRotation;
    }
}