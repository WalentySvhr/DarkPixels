using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

// Використовуємо інтерфейси натискання та відпускання пальця
public class MerchantItemSlot : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [Header("Товар, що продається")]
    public Item itemToSell;

    [Header("UI Елементи")]
    public Image iconImage;

    void Start()
    {
        // Встанови тут ту змінну іконки, яку ми виправили у минулому кроці (наприклад, icon або sprite)
        if (itemToSell != null && iconImage != null)
        {
            // iconImage.sprite = itemToSell.icon; 
        }
    }

    // ТАПНУВ І ТРИМАЄШ: Спрацьовує в момент, коли палець торкнувся екрану на іконці
    public void OnPointerDown(PointerEventData eventData)
    {
        if (itemToSell != null && ItemInfoManager.Instance != null)
        {
            // Примусово відкриваємо та оновлюємо інфо
            ItemInfoManager.Instance.UpdateInfo(itemToSell);
        }
    }

    // ВІДПУСТИВ: Спрацьовує в момент, коли палець відірвався від екрану
    public void OnPointerUp(PointerEventData eventData)
    {
        if (ItemInfoManager.Instance != null)
        {
            // Миттєво ховаємо вікно опису
            ItemInfoManager.Instance.HideInfo();
        }
    }
}