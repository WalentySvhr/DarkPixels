using UnityEngine;

public class QuestPoint : MonoBehaviour
{
    public QuestType pointType;
    public string pointID;
    public bool destroyOnComplete = true;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            QuestManager.Instance.OnQuestAction(pointType, pointID);

            if (destroyOnComplete && pointType == QuestType.ReachLocation)
                gameObject.SetActive(false);
        }
    }
    private void Start()
    {
        // Реєструємо цю точку в базі даних менеджера
        if (QuestManager.Instance != null)
        {
            QuestManager.Instance.RegisterPoint(this);
        }
    }

    // Якщо це NPC, викликаємо цей метод через вашу систему діалогів
    public void Interact()
    {
        QuestManager.Instance.OnQuestAction(QuestType.Talk, pointID);
    }
}