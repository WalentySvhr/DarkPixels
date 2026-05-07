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

    [Header("Icons: Магазин (після квесту)")]
    public GameObject shopIcon; // Іконка кошика або монетки
    public GameObject minimapShopIcon;

    [Header("Dialog Texts")]
    public string welcomeDialog = "Вітаю! Допоможи мені з однією справою...";
    public string progressDialog = "Ти ще не виконав моє прохання.";
    public string completeDialog = "Чудова робота! Ось твоя нагорода.";
    public string alreadyDoneDialog = "Дякую ще раз за допомогу!";
    public string busyDialog = "Я бачу, ти вже маєш завдання. Спочатку заверши його!";

    [Header("Shop Settings (Після завершення квесту)")]
    public ShopData shopData;

    void Start()
    {
        InvokeRepeating(nameof(UpdateIcon), 0.5f, 0.5f);
    }

    void LateUpdate()
    {
        // Додаємо нові іконки в список для виправлення повороту
        FixIconTransform(questionMarkIcon);
        FixIconTransform(exclamationMarkIcon);
        FixIconTransform(shopIcon);
        FixIconTransform(minimapQuestionMarkIcon);
        FixIconTransform(minimapExclamationMarkIcon);
        FixIconTransform(minimapShopIcon);
    }

    private void FixIconTransform(GameObject icon)
    {
        if (icon != null && icon.activeSelf)
        {
            icon.transform.rotation = Quaternion.identity;
            Vector3 localScale = icon.transform.localScale;
            localScale.x = Mathf.Abs(localScale.x) * Mathf.Sign(transform.lossyScale.x);
            icon.transform.localScale = localScale;
        }
    }

    public QuestData GetRelevantQuest()
    {
        QuestData current = questToOffer;
        QuestManager qm = QuestManager.Instance;
        if (qm == null) return null;

        while (current != null && qm.IsQuestCompleted(current.name))
        {
            current = current.nextQuest;
        }
        return current;
    }

    public void UpdateIcon()
    {
        // Спочатку вимикаємо ВСІ іконки
        if (questionMarkIcon != null) questionMarkIcon.SetActive(false);
        if (minimapQuestionMarkIcon != null) minimapQuestionMarkIcon.SetActive(false);
        if (exclamationMarkIcon != null) exclamationMarkIcon.SetActive(false);
        if (minimapExclamationMarkIcon != null) minimapExclamationMarkIcon.SetActive(false);
        if (shopIcon != null) shopIcon.SetActive(false);
        if (minimapShopIcon != null) minimapShopIcon.SetActive(false);

        QuestData activeQuestForNPC = GetRelevantQuest();
        QuestManager qm = QuestManager.Instance;

        // ЛОГІКА МАГАЗИНУ: Якщо квестів немає, але є ShopData
        if (activeQuestForNPC == null)
        {
            if (shopData != null)
            {
                if (shopIcon != null) shopIcon.SetActive(true);
                if (minimapShopIcon != null) minimapShopIcon.SetActive(true);
            }
            return;
        }

        // ЛОГІКА КВЕСТІВ (залишається як була)
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
            if (questionMarkIcon != null) questionMarkIcon.SetActive(true);
            if (minimapQuestionMarkIcon != null) minimapQuestionMarkIcon.SetActive(true);
        }
    }

    private string CleanName(string rawName) => rawName.Replace("(Clone)", "").Trim().ToLower();

    public void Interact()
    {
        QuestData activeQuestForNPC = GetRelevantQuest();
        QuestManager qm = QuestManager.Instance;
        DialogManager dm = DialogManager.Instance;

        if (activeQuestForNPC == null)
        {
            if (shopData != null)
            {
                ShopManager.Instance.OpenShop(shopData);
            }
            else if (!string.IsNullOrEmpty(alreadyDoneDialog))
            {
                dm.StartStaticDialog(alreadyDoneDialog, npcDialogData);
            }
            return;
        }

        if (qm.currentQuest != null && CleanName(qm.currentQuest.name) == CleanName(activeQuestForNPC.name))
        {
            if (activeQuestForNPC.requiresReturnToNPC && qm.currentProgress >= activeQuestForNPC.requiredAmount)
                dm.StartCompletionDialog(completeDialog, this, npcDialogData);
            else
                dm.StartStaticDialog(progressDialog, npcDialogData);
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
        if (activeQuestForNPC != null) QuestManager.Instance.InitializeQuest(activeQuestForNPC);
        UpdateIcon();
    }

    public void CompleteQuest()
    {
        QuestManager qm = QuestManager.Instance;
        if (qm.currentQuest != null)
        {
            if (qm.currentQuest.type == QuestType.CollectItems && InventoryManager.Instance != null && qm.currentQuest.itemToCollect != null)
            {
                InventoryManager.Instance.RemoveItems(qm.currentQuest.itemToCollect, qm.currentQuest.requiredAmount);
            }
            qm.FinishQuestFromNPC();
        }
        UpdateIcon();
    }

    private void OnMouseDown()
    {
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject()) return;
        Interact();
    }

    private string GetGameObjectPath(GameObject obj)
    {
        string path = "/" + obj.name;
        while (obj.transform.parent != null) { obj = obj.transform.parent.gameObject; path = "/" + obj.name + path; }
        return path;
    }
}