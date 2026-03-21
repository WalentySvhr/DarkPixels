using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InventorySlot : MonoBehaviour
{
    public Image icon;
    public TextMeshProUGUI stackText;

    public void AddItem(Item newItem, int amount)
    {
        if (newItem == null) return;

        icon.sprite = newItem.icon;
        icon.enabled = true;
        icon.color = Color.white; // Скидаємо колір на білий, щоб бачити банку

        if (stackText != null)
        {
            if (newItem.isStackable && amount > 1)
            {
                stackText.text = amount.ToString();
                stackText.gameObject.SetActive(true);
            }
            else
            {
                stackText.gameObject.SetActive(false);
            }
        }
    }

    public void ClearSlot()
    {
        icon.sprite = null;
        icon.enabled = false;
        if (stackText != null) stackText.gameObject.SetActive(false);
    }
}