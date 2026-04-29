using UnityEngine;

public class MobileQuitHandler : MonoBehaviour
{
    [Header("UI Reference")]
    public GameObject quitPanel; // Перетягни сюди свою панель QuitCanvas

    void Update()
    {
        // Якщо натиснуто "Назад" на телефоні або Escape
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            // Якщо вікно вже відкрите — закриваємо його (відміна)
            // Якщо закрите — відкриваємо
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

    public void ShowQuitPanel()
    {
        quitPanel.SetActive(true);
        Time.timeScale = 0f; // Ставимо гру на паузу, поки гравець думає
    }

    public void CancelQuit()
    {
        quitPanel.SetActive(false);
        Time.timeScale = 1f; // Повертаємо гру до життя
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