using UnityEngine;
using UnityEngine.UI;

public class QuestUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Button cancelButton;

    void Start()
    {
        if (cancelButton != null)
        {
            // Додаємо слухача: при натисканні викликається метод з QuestManager
            cancelButton.onClick.AddListener(OnCancelButtonClick);
        }
    }

    private void OnCancelButtonClick()
    {
        if (QuestManager.Instance != null)
        {
            // Викликаємо метод, який ми додали минулого разу
            QuestManager.Instance.CancelCurrentQuest();

            // Додатково: можна додати звук натискання або лог
            Debug.Log("UI: Кнопку скасування натиснуто.");
        }
    }

    // Можна додати метод для приховування кнопки, якщо квест не можна скасувати
    public void ToggleCancelButton(bool canCancel)
    {
        if (cancelButton != null)
        {
            cancelButton.gameObject.SetActive(canCancel);
        }
    }
}