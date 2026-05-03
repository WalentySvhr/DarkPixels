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
    private QuestGiver questGiver; // Додано для перевірки квестів
    private Transform playerTransform;

    void Start()
    {
        npcPatrol = GetComponent<NPCPatrol>();
        questGiver = GetComponent<QuestGiver>(); // Шукаємо QuestGiver на цьому ж NPC

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null) playerTransform = player.transform;

        if (questPoint == null) questPoint = GetComponent<QuestPoint>();
    }

    private void OnMouseDown()
    {
        if (playerTransform == null) return;

        float distance = Vector2.Distance(transform.position, playerTransform.position);

        if (distance <= interactRange)
        {
            TriggerInteraction();
        }
    }

    public void TriggerInteraction()
    {
        // Якщо діалог вже відкритий — нічого не робимо
        if (DialogManager.Instance.dialogPanel.activeInHierarchy) return;

        // ПРІОРИТЕТ 1: Якщо на NPC є QuestGiver, нехай він сам вирішує, який діалог показати
        if (questGiver != null)
        {
            questGiver.Interact(); // Це запустить логіку з перевіркою виконаних квестів
            return;
        }

        // ПРІОРИТЕТ 2: Якщо QuestGiver немає, просто запускаємо звичайний діалог
        if (currentDialog != null && DialogManager.Instance != null)
        {
            // Використовуємо новий метод StartDialog для DialogData
            DialogManager.Instance.StartDialog(currentDialog);

            // Якщо це технічний NPC (QuestPoint), фіксуємо взаємодію (наприклад, для квесту "Дійди до точки")
            if (questPoint != null)
            {
                questPoint.Interact();
            }
        }
    }
}