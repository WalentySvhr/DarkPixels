using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI; // Додаємо для роботи з Button
using TMPro;
using System.Collections;

public class GameButtons : MonoBehaviour
{
    [Header("Налаштування кнопки збереження")]
    public Button saveButton; // Перетягни сюди саму КНОПКУ (не текст)
    public TextMeshProUGUI saveButtonText;
    public string defaultText = "Зберегти";
    public string successText = "Збережено";
    public string forbiddenText = "Збереження заборонено"; // Текст, коли не можна

    void Update()
    {
        // Перевіряємо статус зони кожен кадр
        if (saveButton != null)
        {
            // Якщо ми в зоні, де не можна зберігатися - кнопка вимикається
            saveButton.interactable = SaveForbiddenZone.CanSave;

            // Додатково можна міняти текст, щоб гравець розумів чому
            if (!SaveForbiddenZone.CanSave)
            {
                saveButtonText.text = forbiddenText;
            }
            else if (saveButtonText.text == forbiddenText)
            {
                // Повертаємо назад, якщо вийшли з зони
                saveButtonText.text = defaultText;
            }
        }
    }

    public void OnSaveButtonClicked()
    {
        // Подвійна перевірка про всяк випадок
        if (SaveManager.Instance != null && SaveForbiddenZone.CanSave)
        {
            SaveManager.Instance.SaveGame();

            if (saveButtonText != null)
            {
                StopAllCoroutines();
                StartCoroutine(ShowSaveStatus());
            }
        }
    }

    private IEnumerator ShowSaveStatus()
    {
        saveButtonText.text = successText;
        yield return new WaitForSeconds(2f);

        // Повертаємо дефолтний текст тільки якщо ми НЕ в зоні заборони
        if (SaveForbiddenZone.CanSave)
            saveButtonText.text = defaultText;
    }

    // Решта твоїх методів (OnLoadButtonClicked і т.д.) залишаються без змін
}