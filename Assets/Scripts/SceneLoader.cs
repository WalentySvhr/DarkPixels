using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    [Header("UI Panels")]
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private GameObject aboutPanel;

    // --- ПЕРЕМИКАННЯ ВІКОН У МЕНЮ ---

    public void OpenSettings()
    {
        mainMenuPanel.SetActive(false);
        settingsPanel.SetActive(true);
    }

    public void OpenAbout()
    {
        mainMenuPanel.SetActive(false);
        aboutPanel.SetActive(true);
    }

    public void CloseWindows() // Повернення в головне меню
    {
        settingsPanel.SetActive(false);
        aboutPanel.SetActive(false);
        mainMenuPanel.SetActive(true);
    }

    // --- ЗАВАНТАЖЕННЯ ІНШИХ СЦЕН ---

    public void LoadGame()
    {
        // Завантажуємо саму гру (переконайся, що сцена "Game" додана в Build Settings)
        SceneManager.LoadScene("Game");
    }

    public void QuitGame()
    {
        Application.Quit();
        Debug.Log("Game Closed");
    }
}