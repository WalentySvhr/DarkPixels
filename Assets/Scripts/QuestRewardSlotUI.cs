using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class QuestRewardSlotUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Елементи UI")]
    public Image icon;
    public TextMeshProUGUI amountText;

    private Item currentItem;
    private int currentAmount;

    private bool isPointerOver = false;
    private bool isOpened = false;

    public void SetupReward(Item item, int amount)
    {
        if (item == null)
        {
            gameObject.SetActive(false);
            return;
        }

        currentItem = item;
        currentAmount = amount;

        if (icon != null)
        {
            icon.sprite = item.icon;
            icon.enabled = true;
        }

        if (amountText != null)
        {
            if (amount > 1 || item.isStackable)
            {
                amountText.text = amount.ToString();
                amountText.gameObject.SetActive(true);
            }
            else
            {
                amountText.gameObject.SetActive(false);
            }
        }

        gameObject.SetActive(true);
    }

    // Відстежуємо, чи взагалі палець/мишка знаходяться над цим слотом
    public void OnPointerEnter(PointerEventData eventData)
    {
        isPointerOver = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isPointerOver = false;
        CloseInfo();
    }

    private void Update()
    {
        if (currentItem == null || ItemInfoManager.Instance == null) return;

        // Перевіряємо фізичне натискання (мишка 0 або будь-який тач на екрані)
        bool isPressing = Input.GetMouseButton(0) || (Input.touchCount > 0);

        if (isPointerOver && isPressing)
        {
            if (!isOpened)
            {
                ItemInfoManager.Instance.UpdateInfo(currentItem);
                isOpened = true;
            }
        }
        else
        {
            if (isOpened)
            {
                CloseInfo();
            }
        }
    }

    private void CloseInfo()
    {
        if (isOpened && ItemInfoManager.Instance != null)
        {
            ItemInfoManager.Instance.HideInfo();
            isOpened = false;
        }
    }

    private void OnDisable()
    {
        isPointerOver = false;
        CloseInfo();
    }
}