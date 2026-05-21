using System.Collections.Generic;
using UnityEngine;

public class PetInventoryUI : MonoBehaviour
{
    [Header("Батьківський об'єкт для слотів (Grid Layout)")]
    public Transform petsParent;

    [Header("Префаб UI слота для пета")]
    public GameObject petSlotPrefab;

    private List<InventorySlot> petSlots = new List<InventorySlot>();

    private void Awake()
    {
        // Автоматично підписуємо цей UI в InventoryManager
        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.petInventoryUI = this;
        }
    }

    private void Start()
    {
        InitPetSlots();
        UpdatePetUI();
    }

    // Створюємо порожні слоти під ліміт петів (наприклад, 6 слотів)
    private void InitPetSlots()
    {
        // Очищаємо старі об'єкти, якщо вони були в інспекторі
        foreach (Transform child in petsParent)
        {
            Destroy(child.gameObject);
        }
        petSlots.Clear();

        int maxSpace = InventoryManager.Instance != null ? InventoryManager.Instance.petSpace : 6;

        for (int i = 0; i < maxSpace; i++)
        {
            GameObject newSlot = Instantiate(petSlotPrefab, petsParent);
            InventorySlot slotScript = newSlot.GetComponent<InventorySlot>();

            if (slotScript != null)
            {
                petSlots.Add(slotScript);
                slotScript.ClearSlot(); // Спочатку робимо його порожнім
            }
        }
    }

    // Оновлюємо іконки петів
    public void UpdatePetUI()
    {
        if (InventoryManager.Instance == null || petSlots == null || petSlots.Count == 0) return;

        // Проходимо по всіх створених слотах
        for (int i = 0; i < petSlots.Count; i++)
        {
            // --- ВАЖЛИВО: Завжди спочатку очищаємо слот ---
            // Це гарантує, що старі дані або "фантомні" іконки зникнуть
            petSlots[i].ClearSlot();

            // Якщо в списку є елемент для цього слота
            if (i < InventoryManager.Instance.petItems.Count)
            {
                var petStack = InventoryManager.Instance.petItems[i];

                // Додаємо предмет. 
                // Переконайтеся, що AddItem всередині НЕ викликає 
                // логіку EquipPet, а тільки малює іконку!
                petSlots[i].AddItem(petStack.item, petStack.amount);
            }
            // Якщо i >= petItems.Count, слот залишається порожнім після ClearSlot()
        }
    }
}