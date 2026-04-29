using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
// Цей клас керує діалогами в грі. Він відповідає за відкриття/закриття панелі діалогу, відображення тексту та імені NPC, а також за ефект друкування тексту по одній літері.
// Він кріпиться на окремому об'єкті в сцені (наприклад, DialogManager) і має статичну властивість Instance для легкого доступу з інших скриптів (наприклад, з NPCPatrol).

public class DialogManager : MonoBehaviour
{
    public static DialogManager Instance { get; private set; }

    [Header("UI Елементи")]
    public GameObject dialogPanel; // Сама панель діалогу
    public TextMeshProUGUI nameText; // Текст імені
    public TextMeshProUGUI dialogText; // Текст репліки
    public Image portraitImage; // Картинка NPC (опціонально)

    [Header("Налаштування")]
    public float typingSpeed = 0.02f; // Швидкість друку літер

    private Queue<string> sentences;
    private bool isTyping = false;
    private string currentSentence = "";
    private NPCPatrol currentNPC; // Щоб знати, кого відпустити після діалогу

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        sentences = new Queue<string>();
        if (dialogPanel != null) dialogPanel.SetActive(false);
    }

    void Update()
    {
        // Якщо діалог відкритий і гравець клікає мишкою — йдемо до наступної репліки
        if (dialogPanel != null && dialogPanel.activeInHierarchy && Input.GetMouseButtonDown(0))
        {
            DisplayNextSentence();
        }
    }

    public void StartDialog(DialogData dialog, NPCPatrol npc = null)
    {
        currentNPC = npc; // Запам'ятовуємо, з ким говоримо

        // Зупиняємо NPC
        if (currentNPC != null) currentNPC.StartInteraction();

        dialogPanel.SetActive(true);
        nameText.text = dialog.npcName;

        // Якщо є портрет — показуємо, якщо ні — ховаємо
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

        // Завантажуємо всі репліки в чергу
        foreach (string sentence in dialog.sentences)
        {
            sentences.Enqueue(sentence);
        }

        DisplayNextSentence();
    }

    public void DisplayNextSentence()
    {
        // Якщо текст ще друкується, а гравець клікнув — виводимо текст миттєво цілком
        if (isTyping)
        {
            StopAllCoroutines();
            dialogText.text = currentSentence;
            isTyping = false;
            return;
        }

        // Якщо репліки закінчилися — закриваємо діалог
        if (sentences.Count == 0)
        {
            EndDialog();
            return;
        }

        currentSentence = sentences.Dequeue();
        StartCoroutine(TypeSentence(currentSentence));
    }

    IEnumerator TypeSentence(string sentence)
    {
        isTyping = true;
        dialogText.text = "";

        // Виводимо по одній літері
        foreach (char letter in sentence.ToCharArray())
        {
            dialogText.text += letter;
            yield return new WaitForSeconds(typingSpeed);
        }

        isTyping = false;
    }

    void EndDialog()
    {
        dialogPanel.SetActive(false);
        // Відпускаємо NPC, щоб він пішов далі
        if (currentNPC != null) currentNPC.StopInteraction();
    }
}