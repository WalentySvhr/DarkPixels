using UnityEngine;

public class QuestGiver : MonoBehaviour
{
    [Header("Data References")]
    public QuestData questToOffer;
    public DialogData npcDialogData; // Файл з ім'ям та портретом NPC

    [Header("Icons")]
    public GameObject questionMarkIcon; // Твій звичайний об'єкт "?" над головою
    public GameObject minimapQuestionMarkIcon; // ДОДАНО: Велика іконка для мінімапи

    [Header("Dialog Texts")]
    public string welcomeDialog = "Вітаю! Допоможи мені з однією справою...";
    public string progressDialog = "Ти ще не виконав моє прохання.";
    public string completeDialog = "Чудова робота! Ось твоя нагорода.";
    public string alreadyDoneDialog = "Дякую ще раз за допомогу!";

    void Start()
    {
        UpdateIcon();
    }

    public void UpdateIcon()
    {
        if (questToOffer == null) return;

        string qName = questToOffer.name;

        // Перевіряємо стан через менеджер
        bool isCompleted = QuestManager.Instance.completedQuests.Contains(qName);
        bool isActive = QuestManager.Instance.currentQuest != null && QuestManager.Instance.currentQuest.name == qName;

        // Визначаємо, чи потрібно показувати іконки (квест не прийнято і не виконано)
        bool shouldShowIcon = !isCompleted && !isActive;

        // Керуємо звичайним знаком питання
        if (questionMarkIcon != null)
        {
            questionMarkIcon.SetActive(shouldShowIcon);
        }

        // Керуємо знаком питання для мінімапи (логіка абсолютно така ж)
        if (minimapQuestionMarkIcon != null)
        {
            minimapQuestionMarkIcon.SetActive(shouldShowIcon);
        }
    }

    public void Interact()
    {
        if (questToOffer == null || npcDialogData == null)
        {
            Debug.LogWarning("QuestGiver: Відсутній QuestData або DialogData!");
            return;
        }

        string qName = questToOffer.name;
        QuestManager qm = QuestManager.Instance;
        DialogManager dm = DialogManager.Instance;

        // 1. Якщо квест вже виконаний назавжди
        if (qm.completedQuests.Contains(qName))
        {
            dm.StartStaticDialog(alreadyDoneDialog, npcDialogData);
            return;
        }

        // 2. Якщо цей квест зараз активний
        if (qm.currentQuest != null && qm.currentQuest.name == qName)
        {
            if (questToOffer.requiresReturnToNPC && qm.currentProgress >= questToOffer.requiredAmount)
            {
                dm.StartCompletionDialog(completeDialog, this, npcDialogData);
            }
            else
            {
                dm.StartStaticDialog(progressDialog, npcDialogData);
            }
            return;
        }

        // 3. Якщо квест новий
        dm.StartQuestDialog(welcomeDialog, this, npcDialogData);
    }

    public void AcceptQuest()
    {
        QuestManager.Instance.InitializeQuest(questToOffer);
        UpdateIcon();
    }
}