using UnityEngine;

public class MapManager : MonoBehaviour
{
    // Синглтон для зручного доступу з інших скриптів
    public static MapManager Instance { get; private set; }


    [Header("UI Елементи Мапи")]
    [SerializeField] private GameObject fullMapWindow;  // Об'єкт великої мапи на весь екран

    [SerializeField] private GameObject miniMapWindow;  // Об'єкт мінікарти на екрані гри


    [Header("Налаштування геймплею")]

    [SerializeField] private bool pauseGameOnOpen = true; // Чи ставити гру на паузу


    private void Awake()

    {
        // Ініціалізація синглтона

        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        // Перевіряємо, щоб на старті велика мапа була точно вимкнена
        if (fullMapWindow != null)

        {
            fullMapWindow.SetActive(false);
        }

    }

    /// <summary>
    /// Відкриває велику мапу. Викликається при тапі на мінікарту.
    /// </summary>
    public void OpenFullMap()

    {

        if (fullMapWindow == null) return;

        // Вмикаємо велике вікно

        fullMapWindow.SetActive(true);

        // Ховаємо мінікарту, щоб не заважала

        if (miniMapWindow != null)
        {

            miniMapWindow.SetActive(false);
        }

        // Якщо увімкнена пауза — зупиняємо час у грі
        if (pauseGameOnOpen)
        {
            Time.timeScale = 0f;
        }

    }


    /// <summary>
    /// Закриває велику мапу. Викликається кнопкою закриття або тапом по фону.
    /// </summary>  
    public void CloseFullMap()
    {

        if (fullMapWindow == null) return;


        // Вимикаємо велике вікно

        fullMapWindow.SetActive(false);



        // Повертаємо мінікарту на екран

        if (miniMapWindow != null)

        {

            miniMapWindow.SetActive(true);

        }



        // Повертаємо звичайний хід часу

        if (pauseGameOnOpen)

        {

            Time.timeScale = 1f;

        }

    }

}