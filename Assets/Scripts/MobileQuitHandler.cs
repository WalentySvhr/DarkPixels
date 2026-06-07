using UnityEngine;

public class MobileQuitHandler : MonoBehaviour
{
    [Header("UI Reference")]
    public GameObject quitPanel; // Перетягни сюди свою панель QuitCanvas

    private void Awake()
    {
        // ГАРАНТІЯ: Як тільки завантажується головне меню, ми ПРИМУСОВО 
        // повертаємо час до норми. Це миттєво оживить усі кнопки!
        Time.timeScale = 1f;
    }

    private void Start()
    {
        // Ховаємо панель при старті гри, якщо забули сховати в редакторі
        if (quitPanel != null)
            quitPanel.SetActive(false);
    }

    void Update()
    {
        // Якщо натиснуто "Назад" на телефоні або Escape
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (quitPanel != null)
            {
                if (quitPanel.activeSelf)
                {
                    CancelQuit();
                }
                else
                {
                    ShowQuitPanel();
                }
            }
        }
    }

    public void ShowQuitPanel()
    {
        if (quitPanel != null)
        {
            quitPanel.SetActive(true);

            // За прикладом OpenSettings() в меню: час зупиняти НЕ ТРЕБА!
            // Примусово тримаємо його в нормі (1f), щоб анімації та кнопки працювали.
            Time.timeScale = 1f;
        }
    }

    public void CancelQuit()
    {
        if (quitPanel != null)
        {
            quitPanel.SetActive(false);
            Time.timeScale = 1f; // Гарантуємо нормальний хід часу
        }
    }

    public void ConfirmQuit()
    {
        Debug.Log("Вихід підтверджено");
        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}