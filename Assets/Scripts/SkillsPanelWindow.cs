using System.Collections.Generic;
using UnityEngine;

public class SkillsPanelWindow : MonoBehaviour
{
    [Header("Префаб слоту вміння")]
    [SerializeField] private GameObject abilitySlotPrefab;

    [Header("Куди спавнити префаби (всередину ScrollRect чи Grid)")]
    [SerializeField] private Transform contentContainer;

    [Header("Список усіх скілів гри")]
    [SerializeField] private List<AbilitySO> allGameAbilities;

    // ВИПРАВЛЕНО: Правильне оголошення звичайного списку
    private List<AbilitySlotUI> spawnedSlots = new List<AbilitySlotUI>();

    private void OnEnable()
    {
        // Перемальовуємо вікно щоразу, коли гравець його відкриває
        RefreshWindow();
    }

    public void RefreshWindow()
    {
        // 1. Спочатку очищаємо старі префаби зі сцени
        foreach (Transform child in contentContainer)
        {
            Destroy(child.gameObject);
        }

        // 2. Очищаємо наш список посилань
        spawnedSlots.Clear();

        // 3. Спавнима префаб для кожного скіла з нашого списку
        foreach (AbilitySO ability in allGameAbilities)
        {
            GameObject slotInstance = Instantiate(abilitySlotPrefab, contentContainer);
            AbilitySlotUI slotScript = slotInstance.GetComponent<AbilitySlotUI>();

            if (slotScript != null)
            {
                slotScript.Initialize(ability);

                // Зберігаємо посилання у список (знадобиться, якщо захочеш оновлювати вікно без перестворення)
                spawnedSlots.Add(slotScript);
            }
        }
    }
}