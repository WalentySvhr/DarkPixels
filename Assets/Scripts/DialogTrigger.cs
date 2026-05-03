using UnityEngine;

public class DialogTrigger : MonoBehaviour
{
    [Header("Файл діалогу")]
    public DialogData currentDialog;

    [Header("Квестові налаштування")]
    [Tooltip("Необов'язково: посилання на квест поінт, якщо цей діалог завершує квест")]
    public QuestPoint questPoint;

    [Header("Налаштування Тапу")]
    public float interactRange = 2.5f;

    private NPCPatrol npcPatrol;
    private Transform playerTransform;

    void Start()
    {
        npcPatrol = GetComponent<NPCPatrol>();
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null) playerTransform = player.transform;

        // Автоматично пробуємо знайти QuestPoint на цьому ж об'єкті, якщо не призначено в інспекторі
        if (questPoint == null) questPoint = GetComponent<QuestPoint>();
    }

    private void OnMouseDown()
    {
        if (playerTransform == null) return;

        float distance = Vector2.Distance(transform.position, playerTransform.position);

        if (distance <= interactRange)
        {
            TriggerDialog();
        }
    }

    public void TriggerDialog()
    {
        if (DialogManager.Instance.dialogPanel.activeInHierarchy) return;

        if (currentDialog != null && DialogManager.Instance != null)
        {
            DialogManager.Instance.StartDialog(currentDialog, npcPatrol);

            // ЗВ'ЯЗОК З КВЕСТОМ:
            // Якщо на NPC є QuestPoint, ми викликаємо його метод Interact
            if (questPoint != null)
            {
                questPoint.Interact();
            }
        }
    }
}