using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class FisherShopSlot : MonoBehaviour
{
    public Image icon;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI priceText;

    private Item currentItem;
    private bool isSellSlot;

    public void Setup(Item item, bool isSelling)
    {
        currentItem = item;
        isSellSlot = isSelling;

        if (icon != null) icon.sprite = item.icon;
        if (nameText != null) nameText.text = item.itemName;
        if (priceText != null) priceText.text = item.price.ToString() + " золота";
    }

    public void OnClick()
    {
        if (currentItem == null) return;

        if (isSellSlot)
            FisherShopManager.Instance.SellItem(currentItem);
        else
            FisherShopManager.Instance.BuyItem(currentItem);
    }
}