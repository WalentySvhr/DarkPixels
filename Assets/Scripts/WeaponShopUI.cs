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

    private List<ShopWeaponSlot> spawnedSlots = new List<ShopWeaponSlot>();

    void Start()
    {
        GenerateShopSlots();
    }

    public void GenerateShopSlots()
    {
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
                spawnedSlots.Add(slotScript);
            }
        }
    }

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
    public void OpenShopPanel()
    {
        if (shopPanel != null)
        {
            shopPanel.SetActive(true);
            Debug.Log("Магазин зброї відкрито.");
        }
    }

    public void CloseShopPanel()
    {
        if (shopPanel != null)
        {
            shopPanel.SetActive(false);
        }

        // ВИПРАВЛЕНО: Замість прямого посилання на NPCPatrol використовуємо SendMessage.
        // Це автоматично викличе StopInteraction() у будь-якого компонента на traderNPC, 
        // який має такий метод.
        if (traderNPC != null)
        {
            traderNPC.SendMessage("StopInteraction", SendMessageOptions.DontRequireReceiver);
        }

        Debug.Log("Магазин зброї закрито.");
    }
}