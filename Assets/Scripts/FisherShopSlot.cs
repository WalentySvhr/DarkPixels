using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using System.Collections;

public class FisherShopSlot : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerClickHandler
{
    public Image icon;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI priceText;
    public TextMeshProUGUI amountText;

    [Header("Текст кнопки (Купити/Продати)")]
    public TextMeshProUGUI actionButtonText;

    private Item currentItem;
    private bool isSellSlot;

    [Header("Налаштування затискання (Long Press)")]
    public float holdTime = 0.5f;
    private Coroutine holdCoroutine;

    [Header("Налаштування дабл-тапу")]
    private float lastClickTime;
    private const float doubleClickThreshold = 0.3f; // Час у секундах між тапами

    public void Setup(Item item, bool isSelling, int amount = 1)
    {
        currentItem = item;
        isSellSlot = isSelling;

        // --- ОБРОБКА ПОРОЖНЬОГО СЛОТА ---
        if (item == null)
        {
            if (icon != null) icon.gameObject.SetActive(false);
            if (nameText != null) nameText.gameObject.SetActive(false);
            if (priceText != null) priceText.gameObject.SetActive(false);
            if (amountText != null) amountText.gameObject.SetActive(false);
            if (actionButtonText != null) actionButtonText.gameObject.SetActive(false);

            return; // Виходимо з методу, залишаючи лише саму рамку префабу
        }

        // --- ОБРОБКА СЛОТА З ПРЕДМЕТОМ ---
        // Увімкнення елементів, якщо вони були вимкнені попередніми пустими слотами
        if (icon != null) icon.gameObject.SetActive(true);
        if (nameText != null) nameText.gameObject.SetActive(true);
        if (priceText != null) priceText.gameObject.SetActive(true);
        if (actionButtonText != null) actionButtonText.gameObject.SetActive(true);

        // Заповнення даними
        if (icon != null) icon.sprite = item.icon;
        if (nameText != null) nameText.text = item.itemName;

        if (actionButtonText != null)
            actionButtonText.text = isSellSlot ? "Продати" : "Купити";

        if (priceText != null)
        {
            if (isSellSlot && ShopManager.Instance != null && ShopManager.Instance.currentShop != null)
            {
                int displayPrice = Mathf.RoundToInt(item.price * ShopManager.Instance.currentShop.sellMultiplier);
                if (displayPrice < 1) displayPrice = 1;
                priceText.text = displayPrice.ToString() + " gold";
            }
            else
            {
                priceText.text = item.price.ToString() + " gold";
            }
        }

        if (amountText != null)
        {
            amountText.text = amount.ToString();
            amountText.gameObject.SetActive(amount > 1);
        }
    }

    // --- ЛОГІКА LONG PRESS (ІНФО) ---

    public void OnPointerDown(PointerEventData eventData)
    {
        // Взаємодія дозволена тільки якщо в слоті є предмет
        if (currentItem != null)
            holdCoroutine = StartCoroutine(HoldTimer());
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (currentItem == null) return;

        StopHoldTimer();
        if (ItemInfoManager.Instance != null)
            ItemInfoManager.Instance.HideInfo();
    }

    private IEnumerator HoldTimer()
    {
        yield return new WaitForSeconds(holdTime);
        if (currentItem != null && ItemInfoManager.Instance != null)
            ItemInfoManager.Instance.UpdateInfo(currentItem);
    }

    private void StopHoldTimer()
    {
        if (holdCoroutine != null)
        {
            StopCoroutine(holdCoroutine);
            holdCoroutine = null;
        }
    }

    // --- ЛОГІКА ВЛАСНОГО DOUBLE TAP (КУПІВЛЯ) ---

    public void OnPointerClick(PointerEventData eventData)
    {
        // Якщо слот пустий — ігноруємо будь-які кліки
        if (currentItem == null) return;

        float timeSinceLastClick = Time.time - lastClickTime;

        if (timeSinceLastClick <= doubleClickThreshold)
        {
            // Успішний дабл-тап
            Debug.Log("Власний дабл-тап: ВИКОНАННЯ ДІЇ");
            StopHoldTimer(); // Щоб інфо не вилізло випадково
            ExecuteAction();
            lastClickTime = 0; // Скидаємо таймер
        }
        else
        {
            // Одиночний тап
            Debug.Log("Одиночний тап зафіксовано");
            lastClickTime = Time.time;
        }
    }

    private void ExecuteAction()
    {
        if (ItemInfoManager.Instance != null)
            ItemInfoManager.Instance.HideInfo();

        if (isSellSlot)
            ShopManager.Instance.SellItem(currentItem);
        else
            ShopManager.Instance.BuyItem(currentItem);

        Debug.Log($"Предмет {currentItem.itemName} оброблено!");
    }

    public void OnClick() { }
}