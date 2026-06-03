using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class StoryQuestWrapper
{
    [Tooltip("Перетягни сюди файл квесту (звідси скрипт сам візьме назву квесту)")]
    public QuestData questData;

    [Tooltip("ID твого NPC на карті (наприклад: AlchemistNPC), до якого має вести стрілочка")]
    public string npcTargetID;
}

public class QuestListUI : MonoBehaviour
{
    [Header("Налаштування префабу")]
    public GameObject questSlotPrefab;
    public Transform contentParent;

    [Header("СПИСОК СЮЖЕТНИХ КВЕСТІВ")]
    [Tooltip("Натискай '+', додавай елементи і зв'язуй квест із потрібним NPC!")]
    public List<StoryQuestWrapper> storyQuests = new List<StoryQuestWrapper>();

    void Awake()
    {
        if (contentParent == null) contentParent = transform;
    }

    public void RefreshList()
    {
        // Очищаємо старі кнопки
        foreach (Transform child in contentParent)
        {
            Destroy(child.gameObject);
        }

        // Створюємо нові кнопки
        foreach (StoryQuestWrapper item in storyQuests)
        {
            if (item.questData == null) continue;

            // === ФІКС №1: ПЕРЕВІРКА НА ЗАВЕРШЕНІ КВЕСТИ ===
            // Якщо QuestManager каже, що цей квест уже виконано — пропускаємо його
            if (QuestManager.Instance != null && QuestManager.Instance.IsQuestCompleted(item.questData.name))
            {
                continue; // Ідемо до наступного квесту в списку, цей не малюємо
            }

            // === ФІКС №2 (ОПЦІОНАЛЬНО): ХОВАЄМО КВЕСТ, ЯКЩО ВІН ЗАРАЗ АКТИВНИЙ ===
            // Якщо ти вже підійшов до NPC і взяв цей квест, і він став "поточним",
            // можна сховати його з цього списку доступних завдань. 
            // Якщо хочеш увімкнути цю логіку — просто розкоментуй два рядки нижче:
            // if (QuestManager.Instance != null && QuestManager.Instance.currentQuest == item.questData)
            //     continue;

            GameObject newSlot = Instantiate(questSlotPrefab, contentParent);
            QuestSlotUI slotScript = newSlot.GetComponent<QuestSlotUI>();

            if (slotScript != null)
            {
                slotScript.SetupQuestSlot(item.questData, item.npcTargetID);
            }
        }
    }
}