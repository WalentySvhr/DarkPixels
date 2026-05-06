using UnityEngine;
using UnityEngine.EventSystems;
using System.Linq;
using System.Collections.Generic;

public class QuestGiver : MonoBehaviour
{
    [Header("Data References")]
    public QuestData questToOffer;
    public DialogData npcDialogData;

    [Header("Icons: Новий квест")]
    public GameObject questionMarkIcon;
    public GameObject minimapQuestionMarkIcon;

    [Header("Icons: Квест готовий до здачі")]
    public GameObject exclamationMarkIcon;
    public GameObject minimapExclamationMarkIcon;

    [Header("Dialog Texts")]
    public string welcomeDialog = "Вітаю! Допоможи мені з однією справою...";
    public string progressDialog = "Ти ще не виконав моє прохання.";
    public string completeDialog = "Чудова робота! Ось твоя нагорода.";
    public string alreadyDoneDialog = "Дякую ще раз за допомогу!";
    public string busyDialog = "Я бачу, ти вже маєш завдання. Спочатку заверши його!";

    void Start()
    {
        // Цей рядок змінить колір об'єкта в ієрархії, щоб ти бачив, який саме скрипт працює
        Debug.Log($"<color=white>Я скрипт на об'єкті: </color> <color=orange>{gameObject.name}</color>. Повний шлях: {GetGameObjectPath(gameObject)}");

        InvokeRepeating(nameof(UpdateIcon), 0.5f, 0.5f);
    }

    // --- НОВИЙ БЛОК: ФІКСАЦІЯ ІКОНОК ---
    void LateUpdate()
    {
        // Щокадру фіксуємо положення іконок, щоб вони не перевертались разом з NPC
        FixIconTransform(questionMarkIcon);
        FixIconTransform(exclamationMarkIcon);
        FixIconTransform(minimapQuestionMarkIcon);
        FixIconTransform(minimapExclamationMarkIcon);
    }

    private void FixIconTransform(GameObject icon)
    {
        if (icon != null && icon.activeSelf)
        {
            // 1. Захист від повороту (якщо NPC розвертається через Rotation Y = 180)
            icon.transform.rotation = Quaternion.identity;

            // 2. Захист від віддзеркалення (якщо NPC розвертається через Scale X = -1)
            Vector3 localScale = icon.transform.localScale;
            // "Мінус на мінус дає плюс": якщо батько має мінус, ставимо мінус іконці, щоб вона виглядала нормально
            localScale.x = Mathf.Abs(localScale.x) * Mathf.Sign(transform.lossyScale.x);
            icon.transform.localScale = localScale;
        }
    }
    // ------------------------------------

    // Допоміжний метод, щоб побачити точне місце в Hierarchy
    private string GetGameObjectPath(GameObject obj)
    {
        string path = "/" + obj.name;
        while (obj.transform.parent != null)
        {
            obj = obj.transform.parent.gameObject;
            path = "/" + obj.name + path;
        }
        return path;
    }

    private string CleanName(string rawName)
    {
        if (string.IsNullOrEmpty(rawName)) return "";
        // Видаляємо пробіли, (Clone) та переводимо в нижній регістр для 100% порівняння
        return rawName.Replace("(Clone)", "").Trim().ToLower();
    }

    private bool IsQuestCompleted(string questName)
    {
        // ЗАХИСТ: Якщо менеджер ще не ініціалізувався, просто кажемо "ні"
        if (QuestManager.Instance == null) return false;

        string cleanedSearchName = CleanName(questName);
        List<string> completedList = QuestManager.Instance.completedQuests;

        // ЗАХИСТ: Якщо список чомусь не створений
        if (completedList == null) return false;

        return completedList.Any(q => CleanName(q) == cleanedSearchName);
    }

    public QuestData GetRelevantQuest()
    {
        QuestData current = questToOffer;
        QuestManager qm = QuestManager.Instance;

        // ЗАХИСТ: Якщо менеджера немає, нічого не робимо
        if (qm == null) return null;

        // ЗМІНА: Тепер ми використовуємо метод з QuestManager, а не локальний!
        while (current != null && qm.IsQuestCompleted(current.name))
        {
            current = current.nextQuest;
        }

        if (current != null && (string.IsNullOrWhiteSpace(current.description) || current.requiredAmount <= 0))
        {
            return null;
        }

        return current;
    }

    public void UpdateIcon()
    {
        if (questionMarkIcon != null) questionMarkIcon.SetActive(false);
        if (minimapQuestionMarkIcon != null) minimapQuestionMarkIcon.SetActive(false);
        if (exclamationMarkIcon != null) exclamationMarkIcon.SetActive(false);
        if (minimapExclamationMarkIcon != null) minimapExclamationMarkIcon.SetActive(false);

        QuestData activeQuestForNPC = GetRelevantQuest();
        if (activeQuestForNPC == null) return;

        QuestManager qm = QuestManager.Instance;

        if (qm.currentQuest != null)
        {
            string heldQuestName = CleanName(qm.currentQuest.name);
            string npcQuestName = CleanName(activeQuestForNPC.name);

            if (heldQuestName == npcQuestName)
            {
                if (activeQuestForNPC.requiresReturnToNPC && qm.currentProgress >= activeQuestForNPC.requiredAmount)
                {
                    if (exclamationMarkIcon != null) exclamationMarkIcon.SetActive(true);
                    if (minimapExclamationMarkIcon != null) minimapExclamationMarkIcon.SetActive(true);
                }
                return;
            }
        }
        else
        {
            // Тільки якщо квесту немає — вмикаємо "?"
            if (questionMarkIcon != null) questionMarkIcon.SetActive(true);
            if (minimapQuestionMarkIcon != null) minimapQuestionMarkIcon.SetActive(true);
        }
    }

    public void Interact()
    {
        QuestData activeQuestForNPC = GetRelevantQuest();
        QuestManager qm = QuestManager.Instance;
        DialogManager dm = DialogManager.Instance;

        if (activeQuestForNPC == null)
        {
            dm.StartStaticDialog(alreadyDoneDialog, npcDialogData);
            return;
        }

        if (qm.currentQuest != null && CleanName(qm.currentQuest.name) == CleanName(activeQuestForNPC.name))
        {
            if (activeQuestForNPC.requiresReturnToNPC && qm.currentProgress >= activeQuestForNPC.requiredAmount)
            {
                dm.StartCompletionDialog(completeDialog, this, npcDialogData);
            }
            else
            {
                dm.StartStaticDialog(progressDialog, npcDialogData);
            }
            return;
        }

        if (qm.currentQuest != null)
        {
            dm.StartStaticDialog(busyDialog, npcDialogData);
            return;
        }

        dm.StartQuestDialog(welcomeDialog, this, npcDialogData);
    }

    public void AcceptQuest()
    {
        QuestData activeQuestForNPC = GetRelevantQuest();
        if (activeQuestForNPC != null)
        {
            QuestManager.Instance.InitializeQuest(activeQuestForNPC);
            Debug.Log($"[QUEST] Прийнято: {QuestManager.Instance.currentQuest?.name}");
        }
        UpdateIcon();
    }

    private void OnMouseDown()
    {
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject()) return;
        Interact();
    }
}