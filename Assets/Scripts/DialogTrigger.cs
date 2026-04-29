using UnityEngine;
// Цей клас відповідає за запуск діалогу при тапі на NPC. Він перевіряє, чи гравець знаходиться в межах дозволеної відстані для взаємодії, і якщо так — запускає діалог через DialogManager. Цей скрипт кріпиться до того ж об'єкта, що й NPCPatrol, і використовує дані з DialogData для відображення відповідного діалогу.
public class DialogTrigger : MonoBehaviour
{
    [Header("Файл діалогу")]
    public DialogData currentDialog;

    [Header("Налаштування Тапу")]
    [Tooltip("На якій відстані гравець має бути, щоб тап спрацював")]
    public float interactRange = 2.5f;

    private NPCPatrol npcPatrol;
    private Transform playerTransform; // Щоб знати, де гравець

    void Start()
    {
        npcPatrol = GetComponent<NPCPatrol>();

        // Шукаємо гравця за тегом (переконайся, що на твоєму гравці стоїть Tag "Player")
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
        }
    }

    // Цей метод ідеально відловлює ТАП пальцем по об'єкту на Android
    private void OnMouseDown()
    {
        if (playerTransform == null) return;

        // Рахуємо відстань від гравця до NPC
        float distance = Vector2.Distance(transform.position, playerTransform.position);

        // Якщо ми близько — говоримо
        if (distance <= interactRange)
        {
            TriggerDialog();
        }
        else
        {
            Debug.Log("Підійдіть ближче, щоб поговорити!");
        }
    }

    public void TriggerDialog()
    {
        // Перевіряємо, чи панель діалогу ВЖЕ не відкрита (щоб не перезапускати тапнувши двічі)
        if (DialogManager.Instance.dialogPanel.activeInHierarchy) return;

        if (currentDialog != null && DialogManager.Instance != null)
        {
            DialogManager.Instance.StartDialog(currentDialog, npcPatrol);
        }
    }
}