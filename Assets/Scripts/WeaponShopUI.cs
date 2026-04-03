using UnityEngine;
using System.Collections.Generic;

public class WeaponShopUI : MonoBehaviour
{
    [Header("Налаштування панелі")]
    public GameObject shopPanel;
    public WeaponTraderNPC traderNPC;

    [Header("Налаштування префаба")]
    public GameObject slotPrefab;
    public Transform container;

    [Header("Список товарів")]
    public List<WeaponData> allWeapons;

    // Список для зберігання посилань на створені слоти
    private List<ShopWeaponSlot> spawnedSlots = new List<ShopWeaponSlot>();

    void Start()
    {
        GenerateShopSlots();
    }

    // Метод для створення слотів (викликається один раз при старті або відкритті)
    public void GenerateShopSlots()
    {
        // Очищаємо контейнер і список
        foreach (Transform child in container) Destroy(child.gameObject);
        spawnedSlots.Clear();

        foreach (WeaponData data in allWeapons)
        {
            GameObject newSlot = Instantiate(slotPrefab, container);
            ShopWeaponSlot slotScript = newSlot.GetComponent<ShopWeaponSlot>();

            if (slotScript != null)
            {
                slotScript.weaponToSell = data;
                slotScript.price = data.price;
                slotScript.UpdateUI();

                // Додаємо слот у список для майбутніх оновлень
                spawnedSlots.Add(slotScript);
            }
        }
    }

    // НОВИЙ МЕТОД: Оновлює візуал усіх слотів (викликай його після покупки)
    public void RefreshShopUI()
    {
        foreach (ShopWeaponSlot slot in spawnedSlots)
        {
            if (slot != null)
            {
                slot.UpdateUI();
            }
        }
    }

    // МЕТОД ДЛЯ ЗАКРИТТЯ ПАНЕЛІ
    public void CloseShopPanel()
    {
        if (shopPanel != null)
        {
            shopPanel.SetActive(false);
        }

        if (traderNPC != null)
        {
            NPCPatrol patrol = traderNPC.GetComponent<NPCPatrol>();
            if (patrol != null) patrol.StopInteraction();
        }

        Debug.Log("Магазин зброї закрито.");
    }
}