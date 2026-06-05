using UnityEngine;

public class QuestPoint : MonoBehaviour
{
    public QuestType pointType;
    public string pointID;
    public bool destroyOnComplete = true;

    private void Start()
    {
        // Реєструємо цю точку в базі даних менеджера
        if (QuestManager.Instance != null)
        {
            QuestManager.Instance.RegisterPoint(this);
        }
    }

    // Використовуємо Stay, щоб зона спрацювала, навіть якщо 
    // ви стояли в ній під час діалогу і прийняття квесту
    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // Перевіряємо, чи гравцю дійсно зараз потрібна ця зона
            if (IsThisQuestActive())
            {
                // Зараховуємо квест
                QuestManager.Instance.OnQuestAction(pointType, pointID);

                // Вимикаємо локацію ТІЛЬКИ якщо квест був потрібен
                if (destroyOnComplete && pointType == QuestType.ReachLocation)
                {
                    gameObject.SetActive(false);
                }
            }
        }
    }

    // Метод-помічник для перевірки активного квесту
    private bool IsThisQuestActive()
    {
        if (QuestManager.Instance == null || QuestManager.Instance.currentQuest == null)
            return false;

        return QuestManager.Instance.currentQuest.type == pointType &&
               QuestManager.Instance.currentQuest.targetID == pointID;
    }

    // Якщо це NPC, викликаємо цей метод через вашу систему діалогів
    public void Interact()
    {
        QuestManager.Instance.OnQuestAction(QuestType.Talk, pointID);
    }
}