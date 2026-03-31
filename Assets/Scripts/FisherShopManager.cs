using UnityEngine;
using System.Collections.Generic;

public class FisherShopManager : MonoBehaviour
{
    public static FisherShopManager Instance;

    [Header("Префаби та Контейнери")]
    public GameObject slotPrefab;
    public Transform traderNPCContent;
    public Transform traderPlayerContent;

    [Header("Товари Рибалки")]
    public List<Item> fishShopItems;

    public InventoryManager playerInv;

    void Awake() => Instance = this;

    // void Start() => playerInv = FindObjectOfType<InventoryManager>();

    void OnEnable() => RefreshShop();

    public void RefreshShop()
    {
        UpdateGridNPC(traderNPCContent, fishShopItems);

        if (playerInv != null)
        {
            UpdateGridPlayer(traderPlayerContent, playerInv.items);
        }
    }

    private void UpdateGridNPC(Transform container, List<Item> items)
    {
        foreach (Transform child in container) Destroy(child.gameObject);

        foreach (Item item in items)
        {
            if (item == null) continue;
            GameObject obj = Instantiate(slotPrefab, container);

            // ВИПРАВЛЕНО: тепер шукаємо FisherShopSlot замість UI_ShopSlot
            FisherShopSlot slotScript = obj.GetComponent<FisherShopSlot>();
            if (slotScript != null)
            {
                slotScript.Setup(item, false);
            }
        }
    }

    private void UpdateGridPlayer(Transform container, List<InventoryManager.ItemStack> stacks)
    {
        foreach (Transform child in container) Destroy(child.gameObject);

        foreach (var stack in stacks)
        {
            if (stack == null || stack.item == null) continue;
            GameObject obj = Instantiate(slotPrefab, container);

            FisherShopSlot slotScript = obj.GetComponent<FisherShopSlot>();
            if (slotScript != null)
            {
                // ПЕРЕДАЄМО stack.amount
                slotScript.Setup(stack.item, true, stack.amount);
            }
        }
    }

    public void BuyItem(Item item)
    {
        if (playerInv.coins >= item.price)
        {
            if (playerInv.Add(item))
            {
                // Використовуємо наш новий метод замість простого мінусу
                playerInv.ChangeCoins(-item.price);
                RefreshShop();
            }
        }
    }

    public void SellItem(Item item)
    {
        playerInv.Remove(item);
        // Додаємо гроші через метод з оновленням UI
        playerInv.ChangeCoins(item.price);
        RefreshShop();
    }
}