using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class DialogManager : MonoBehaviour
{
    public static DialogManager Instance { get; private set; }

    [Header("UI Елементи")]
    public GameObject dialogPanel;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI dialogText;
    public Image portraitImage;

    [Header("Кнопки Квесту (Посилання)")]
    public GameObject questButtonsPanel;
    public Button acceptButton;
    public TextMeshProUGUI acceptButtonText;
    public Button declineButton;
    public TextMeshProUGUI declineButtonText;

    [Header("Тексти кнопок (Налаштування)")]
    public string questAcceptText = "Прийняти";
    public string questDeclineText = "Відмовитись";
    public string rewardAcceptText = "Забрати нагороду";
    public string rewardDeclineText = "Пізніше";

    [Header("Налаштування")]
    public float typingSpeed = 0.02f;

    private Queue<string> sentences;
    private bool isTyping = false;
    private string currentSentence = "";
    private QuestGiver currentGiver;

    // Змінено з NPCPatrol на MonoBehaviour
    private MonoBehaviour currentNPC;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        sentences = new Queue<string>();
        if (dialogPanel != null) dialogPanel.SetActive(false);
        if (questButtonsPanel != null) questButtonsPanel.SetActive(false);
    }

    void Update()
    {
        bool buttonsActive = questButtonsPanel != null && questButtonsPanel.activeInHierarchy;

        if (dialogPanel != null && dialogPanel.activeInHierarchy && Input.GetMouseButtonDown(0) && !buttonsActive)
        {
            DisplayNextSentence();
        }
    }

    // --- ОСНОВНІ МЕТОДИ ЗАПУСКУ ---

    public void StartStaticDialog(string text, DialogData data, MonoBehaviour npc = null)
    {
        currentNPC = npc;
        PrepareDialogUI(data);
        if (questButtonsPanel != null) questButtonsPanel.SetActive(false);

        sentences.Clear();
        sentences.Enqueue(text);
        DisplayNextSentence();
    }

    public void StartQuestDialog(string text, QuestGiver giver, DialogData data, MonoBehaviour npc = null)
    {
        currentNPC = npc;
        currentGiver = giver;
        PrepareDialogUI(data);

        if (acceptButtonText != null) acceptButtonText.text = questAcceptText;
        if (declineButtonText != null) declineButtonText.text = questDeclineText;

        sentences.Clear();
        sentences.Enqueue(text);
        DisplayNextSentence();
    }

    public void StartCompletionDialog(string text, QuestGiver giver, DialogData data, MonoBehaviour npc = null)
    {
        currentNPC = npc;
        currentGiver = giver;
        PrepareDialogUI(data);

        if (acceptButtonText != null) acceptButtonText.text = rewardAcceptText;
        if (declineButtonText != null) declineButtonText.text = rewardDeclineText;

        sentences.Clear();
        sentences.Enqueue(text);
        DisplayNextSentence();
    }

    public void StartDialog(DialogData dialog, MonoBehaviour npc = null)
    {
        currentNPC = npc;
        PrepareDialogUI(dialog);

        if (questButtonsPanel != null) questButtonsPanel.SetActive(false);

        sentences.Clear();
        foreach (string sentence in dialog.sentences)
        {
            sentences.Enqueue(sentence);
        }

        DisplayNextSentence();
    }

    // --- ДОПОМІЖНІ МЕТОДИ ---

    private void PrepareDialogUI(DialogData data)
    {
        dialogPanel.SetActive(true);

        if (nameText != null) nameText.text = data.npcName;

        if (portraitImage != null)
        {
            if (data.npcPortrait != null)
            {
                portraitImage.sprite = data.npcPortrait;
                portraitImage.enabled = true;
            }
            else
            {
                portraitImage.enabled = false;
            }
        }

        if (questButtonsPanel != null) questButtonsPanel.SetActive(false);
    }

    public void DisplayNextSentence()
    {
        if (isTyping)
        {
            StopAllCoroutines();
            dialogText.text = currentSentence;
            isTyping = false;
            return;
        }

        if (sentences.Count == 0)
        {
            CheckForFinalState();
            return;
        }

        currentSentence = sentences.Dequeue();
        StartCoroutine(TypeSentence(currentSentence));
    }

    private void CheckForFinalState()
    {
        if (currentGiver != null)
        {
            if (questButtonsPanel != null) questButtonsPanel.SetActive(true);
        }
        else
        {
            EndDialog();
        }
    }

    IEnumerator TypeSentence(string sentence)
    {
        isTyping = true;
        dialogText.text = "";
        foreach (char letter in sentence.ToCharArray())
        {
            dialogText.text += letter;
            yield return new WaitForSeconds(typingSpeed);
        }
        isTyping = false;
    }

    public void OnAcceptQuest()
    {
        if (currentGiver != null)
        {
            QuestManager qm = QuestManager.Instance;
            QuestData activeQuest = currentGiver.GetRelevantQuest();

            if (activeQuest != null)
            {
                string qName = activeQuest.name;
                if (qm.currentQuest != null && qName == qm.currentQuest.name && qm.currentProgress >= qm.currentQuest.requiredAmount)
                {
                    currentGiver.CompleteQuest();
                }
                else
                {
                    currentGiver.AcceptQuest();
                }
            }
        }
        EndDialog();
    }

    public void OnDeclineQuest()
    {
        EndDialog();
    }

    void EndDialog()
    {
        dialogPanel.SetActive(false);
        if (questButtonsPanel != null) questButtonsPanel.SetActive(false);
        currentGiver = null;

        if (currentNPC != null)
        {
            // Використовуємо SendMessage, щоб викликати метод, якщо він існує в іншому скрипті
            currentNPC.SendMessage("StopInteraction", SendMessageOptions.DontRequireReceiver);
            currentNPC = null;
        }
    }
}