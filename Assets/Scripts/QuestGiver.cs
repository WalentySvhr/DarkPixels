using UnityEngine;

public class QuestGiver : MonoBehaviour
{
    public QuestData questToOffer;
    public GameObject questionMarkIcon; // Слот для об'єкта "?" 

    private bool hasAccepted = false;

    void Start()
    {
        UpdateIcon();
    }

    void UpdateIcon()
    {
        // Показуємо знак питання, тільки якщо квест не прийнято
        if (questionMarkIcon != null)
        {
            questionMarkIcon.SetActive(!hasAccepted);
        }
    }

    // Викличте це, коли гравець натискає "Прийняти" в діалозі
    public void AcceptQuest()
    {
        hasAccepted = true;
        UpdateIcon();
    }
}