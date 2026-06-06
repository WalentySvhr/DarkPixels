using UnityEngine;
using System.Collections.Generic;

public class DailyQuestWindowUI : MonoBehaviour
{
    [Header("References")]
    public GameObject windowPanel; // Сюди перетягни панель квестів
    public Transform questListContainer;
    public GameObject questSlotPrefab;

    private List<DailyQuestSlotUI> spawnedSlots = new List<DailyQuestSlotUI>();

    // МЕТОД ОДИН В ОДИН ЯК В ІНВЕНТАРІ
    public void ToggleWindow()
    {
        if (windowPanel == null) return;

        bool nextState = !windowPanel.activeSelf;
        windowPanel.SetActive(nextState);

        // === ГЛОБАЛЬНИЙ ЗАПОБІЖНИК ===
        if (nextState)
        {
            UIManager.RegisterWindowOpen();
            RefreshUI(); // Оновлюємо квести тільки при відкритті
        }
        else
        {
            UIManager.RegisterWindowClose();
        }
    }

    // МЕТОД ДЛЯ КНОПКИ "Х"
    public void CloseWindow()
    {
        if (windowPanel != null)
        {
            if (windowPanel.activeSelf)
            {
                windowPanel.SetActive(false);
                UIManager.RegisterWindowClose();
            }
        }
    }

    public void RefreshUI()
    {
        if (DailyQuestManager.Instance == null) return;

        var activeQuests = DailyQuestManager.Instance.activeDailies;
        if (activeQuests == null) return; // Страховка від пустого списку

        while (spawnedSlots.Count < activeQuests.Count)
        {
            GameObject newSlotObj = Instantiate(questSlotPrefab, questListContainer);
            DailyQuestSlotUI slotUI = newSlotObj.GetComponent<DailyQuestSlotUI>();
            spawnedSlots.Add(slotUI);
        }

        for (int i = 0; i < activeQuests.Count; i++)
        {
            if (spawnedSlots[i] != null)
            {
                spawnedSlots[i].gameObject.SetActive(true);
                spawnedSlots[i].Setup(activeQuests[i], i);
            }
        }

        for (int i = activeQuests.Count; i < spawnedSlots.Count; i++)
        {
            if (spawnedSlots[i] != null) spawnedSlots[i].gameObject.SetActive(false);
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (windowPanel != null && windowPanel.activeSelf)
            {
                CloseWindow();
            }
        }
    }
}