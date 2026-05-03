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

    [Header("Кнопки Квесту")]
    public GameObject questButtonsPanel; // Об'єкт-батько для двох кнопок
    public Button acceptButton;
    public Button declineButton;

    [Header("Налаштування")]
    public float typingSpeed = 0.02f;

    private Queue<string> sentences;
    private bool isTyping = false;
    private string currentSentence = "";
    private NPCPatrol currentNPC;
    private DialogData currentDialogData; // Зберігаємо посилання на дані діалогу

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
        // Додаємо перевірку: якщо панель кнопок активна, ми не перемикаємо текст кліком
        bool buttonsActive = questButtonsPanel != null && questButtonsPanel.activeInHierarchy;

        if (dialogPanel != null && dialogPanel.activeInHierarchy && Input.GetMouseButtonDown(0) && !buttonsActive)
        {
            DisplayNextSentence();
        }
    }

    public void StartDialog(DialogData dialog, NPCPatrol npc = null)
    {
        currentNPC = npc;
        currentDialogData = dialog; // Запам'ятовуємо дані діалогу

        if (currentNPC != null) currentNPC.StartInteraction();

        dialogPanel.SetActive(true);
        if (questButtonsPanel != null) questButtonsPanel.SetActive(false); // Ховаємо кнопки на початку

        nameText.text = dialog.npcName;

        if (portraitImage != null)
        {
            if (dialog.npcPortrait != null)
            {
                portraitImage.sprite = dialog.npcPortrait;
                portraitImage.enabled = true;
            }
            else portraitImage.enabled = false;
        }

        sentences.Clear();
        foreach (string sentence in dialog.sentences)
        {
            sentences.Enqueue(sentence);
        }

        DisplayNextSentence();
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

        // Якщо черга порожня - це був останній текст, тепер перевіряємо квест
        if (sentences.Count == 0)
        {
            CheckForQuest();
            return;
        }

        currentSentence = sentences.Dequeue();
        StartCoroutine(TypeSentence(currentSentence));
    }

    private void CheckForQuest()
    {
        if (currentDialogData == null)
        {
            Debug.LogError("Помилка: currentDialogData порожній!");
            EndDialog();
            return;
        }

        if (currentDialogData.questToStart != null)
        {
            Debug.Log("Спроба активувати панель кнопок...");
            if (questButtonsPanel != null)
            {
                questButtonsPanel.SetActive(true);
                // Перевірка, чи він дійсно став активним
                Debug.Log("Стан панелі після SetActive: " + questButtonsPanel.activeSelf);
            }
            else
            {
                Debug.LogError("Помилка: questButtonsPanel не призначений в інспекторі!");
            }
        }
        else
        {
            Debug.Log("Квесту немає, просто завершуємо діалог.");
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
        if (currentDialogData != null && currentDialogData.questToStart != null)
        {
            QuestManager.Instance.InitializeQuest(currentDialogData.questToStart);

            // Важливо: повідомити NPC, що квест прийнято, щоб він прибрав знак "?"
            if (currentNPC != null)
            {
                QuestGiver giver = currentNPC.GetComponent<QuestGiver>();
                if (giver != null) giver.AcceptQuest();
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
        if (currentNPC != null) currentNPC.StopInteraction();
    }
}