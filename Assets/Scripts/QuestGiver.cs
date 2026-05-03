using UnityEngine;

public class QuestGiver : MonoBehaviour
{
    [Header("Data References")]
    public QuestData questToOffer;
    public DialogData npcDialogData; // ДОДАНО: файл з ім'ям та портретом NPC
    public GameObject questionMarkIcon;

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
        if (questionMarkIcon == null || questToOffer == null) return;

        string qName = questToOffer.name;

        // Перевіряємо стан через менеджер
        bool isCompleted = QuestManager.Instance.completedQuests.Contains(qName);
        bool isActive = QuestManager.Instance.currentQuest != null && QuestManager.Instance.currentQuest.name == qName;

        // Знак питання зникає, якщо квест активний або вже виконаний
        questionMarkIcon.SetActive(!isCompleted && !isActive);
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
            // Тепер передаємо ТЕКСТ та ДАНІ NPC (для портрета)
            dm.StartStaticDialog(alreadyDoneDialog, npcDialogData);
            return;
        }

        // 2. Якщо цей квест зараз активний
        if (qm.currentQuest != null && qm.currentQuest.name == qName)
        {
            if (questToOffer.requiresReturnToNPC && qm.currentProgress >= questToOffer.requiredAmount)
            {
                // Діалог завершення з даними NPC
                dm.StartCompletionDialog(completeDialog, this, npcDialogData);
            }
            else
            {
                // Діалог прогресу з даними NPC
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