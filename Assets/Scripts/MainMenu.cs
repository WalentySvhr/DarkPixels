using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.IO;
//що робить цей скрипт:
//Цей скрипт керує головним меню гри, дозволяючи гравцю почати нову гру, продовжити існуючу або вийти з гри. Він також перевіряє наявність збереження, щоб активувати кнопку "Пр    одовжити" та показує попередження при спробі почати нову гру, якщо існує збереження.
public class MainMenu : MonoBehaviour
{
    [Header("Налаштування")]
    public string gameSceneName = "Game";

    [Header("UI Елементи")]
    public GameObject warningPanel;
    public Button continueButton;

    private string savePath;

    void Start()
    {
        savePath = Path.Combine(Application.persistentDataPath, "gamesave.json");

        if (warningPanel != null) warningPanel.SetActive(false);

        CheckSaveFile();
    }

    // Перевіряємо наявність сейву для активації кнопки "Продовжити"
    private void CheckSaveFile()
    {
        if (continueButton != null)
        {
            continueButton.interactable = File.Exists(savePath);
        }
    }

    // ЛОГІКА: НОВА ГРА
    public void OnNewGameClicked()
    {
        if (File.Exists(savePath))
        {
            if (warningPanel != null) warningPanel.SetActive(true);
        }
        else
        {
            StartGameScene();
        }
    }

    public void ConfirmNewGame()
    {
        if (File.Exists(savePath)) File.Delete(savePath);
        StartGameScene();
    }

    public void CancelNewGame()
    {
        if (warningPanel != null) warningPanel.SetActive(false);
    }

    private void StartGameScene()
    {
        SceneManager.LoadScene(gameSceneName);
    }

    // ЛОГІКА: ПРОДОВЖИТИ
    public void ContinueGame()
    {
        if (File.Exists(savePath))
        {
            // Готуємо SaveManager до завантаження після зміни сцени
            if (SaveManager.Instance != null)
            {
                SaveManager.Instance.PrepareLoad();
                SceneManager.LoadScene(gameSceneName);
            }
        }
    }

    public void QuitGame()
    {
        Application.Quit();
        Debug.Log("Гра закрита");
    }
}