using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    [Header("UI Panels")]
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private GameObject aboutPanel;

    private void Awake()
    {
        // ГАРАНТІЯ: повертаємо час до норми (1) при завантаженні сцени меню.
        // Це миттєво "оживить" кнопки, які блокувалися через Time.timeScale = 0 у грі.
        Time.timeScale = 1f;
    }

    private void Start()
    {
        // На старті меню показуємо лише головну панель, інші ховаємо про всяк випадок
        if (mainMenuPanel != null) mainMenuPanel.SetActive(true);
        if (settingsPanel != null) settingsPanel.SetActive(false);
        if (aboutPanel != null) aboutPanel.SetActive(false);
    }

    // --- ПЕРЕМИКАННЯ ВІКОН У МЕНЮ ---

    public void OpenSettings()
    {
        if (mainMenuPanel != null) mainMenuPanel.SetActive(false);
        if (settingsPanel != null) settingsPanel.SetActive(true);

        // Якщо ми в головному меню — час зупиняти не треба! 
        // Тому примусово тримаємо його в нормі, навіть якщо SettingsManager захоче його зупинити.
        Time.timeScale = 1f;
    }

    public void OpenAbout()
    {
        // Якщо після перезавантаження сцени посилання зникло — шукаємо панель на сцені за назвою
        if (aboutPanel == null) aboutPanel = GameObject.Find("AboutPanelName"); // Впиши сюди точну назву об'єкта про автора з ієрархії
        if (mainMenuPanel == null) mainMenuPanel = GameObject.Find("MainMenuPanelName"); // точна назва головної панелі

        if (mainMenuPanel != null) mainMenuPanel.SetActive(false);
        if (aboutPanel != null) aboutPanel.SetActive(true);
    }

    public void CloseWindows() // Повернення в головне меню
    {
        if (settingsPanel != null) settingsPanel.SetActive(false);
        if (aboutPanel != null) aboutPanel.SetActive(false);
        if (mainMenuPanel != null) mainMenuPanel.SetActive(true);

        // Про всяк випадок повертаємо час до норми при закритті будь-яких вікон
        Time.timeScale = 1f;
    }

    // --- ЗАВАНТАЖЕННЯ ІНШИХ СЦЕН ---

    public void LoadGame()
    {
        // Завантажуємо саму гру
        SceneManager.LoadScene("Game");
    }

    public void QuitGame()
    {
        Application.Quit();
        Debug.Log("Game Closed");
    }
}