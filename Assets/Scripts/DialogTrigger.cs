using UnityEngine;

public class DialogTrigger : MonoBehaviour
{
    [Header("Налаштування діалогу")]
    public DialogData currentDialog;

    private bool playerInRange = false;

    private void OnMouseDown()
    {
        // Перевірку EventSystem видалено для уникнення блокування кліків інтерфейсом

        if (playerInRange)
        {
            TriggerInteraction();
        }
    }

    public void TriggerInteraction()
    {
        // 1. Перевірка на вже відкритий UI
        if (DialogManager.Instance != null && DialogManager.Instance.dialogPanel != null)
        {
            if (DialogManager.Instance.dialogPanel.activeInHierarchy) return;
        }

        // 2. Запуск діалогу
        if (currentDialog != null && DialogManager.Instance != null)
        {
            DialogManager.Instance.StartDialog(currentDialog, null);
        }
        else
        {
            Debug.LogWarning($"Дані діалогу або DialogManager відсутні на {gameObject.name}!");
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player")) playerInRange = true;
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player")) playerInRange = false;
    }
}