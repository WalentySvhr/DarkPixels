using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using System.Collections;

public class InventorySlot : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler, IPointerDownHandler, IPointerUpHandler, IPointerClickHandler
{
    [Header("Налаштування слота")]
    public bool isWeaponEquipmentSlot = false;
    public bool isAmuletEquipmentSlot = false;
    public bool isHotbarSlot = false;

    public Image icon;
    public TextMeshProUGUI stackText;

    public Item currentItem;
    public int currentAmount;

    private Canvas groupCanvas;
    private CanvasGroup canvasGroup;

    [Header("Налаштування затискання")]
    public float holdTime = 0.5f;
    private Coroutine holdCoroutine;

    private float lastClickTime;
    private const float doubleClickThreshold = 0.3f;

    private void Awake()
    {
        groupCanvas = GetComponentInParent<Canvas>();
        if (icon != null)
        {
            canvasGroup = icon.gameObject.GetComponent<CanvasGroup>();
            if (canvasGroup == null) canvasGroup = icon.gameObject.AddComponent<CanvasGroup>();
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (currentItem != null) holdCoroutine = StartCoroutine(HoldTimer());
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        StopHoldTimer();
        if (ItemInfoManager.Instance != null) ItemInfoManager.Instance.HideInfo();
    }

    private IEnumerator HoldTimer()
    {
        yield return new WaitForSeconds(holdTime);
        if (currentItem != null && ItemInfoManager.Instance != null)
        {
            ItemInfoManager.Instance.UpdateInfo(currentItem);
        }
    }

    private void StopHoldTimer()
    {
        if (holdCoroutine != null) { StopCoroutine(holdCoroutine); holdCoroutine = null; }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (currentItem == null) return;

        // Для Хотбару використовуємо відразу (один клік)
        if (isHotbarSlot)
        {
            HandleAction();
        }
        else // Для інвентарю - подвійний клік
        {
            float timeSinceLastClick = Time.time - lastClickTime;
            if (timeSinceLastClick <= doubleClickThreshold)
            {
                StopHoldTimer();
                HandleAction();
            }
            lastClickTime = Time.time;
        }
    }

    private void HandleAction()
    {
        if (ItemInfoManager.Instance != null) ItemInfoManager.Instance.HideInfo();

        // Якщо це слот екіпіровки, дія по кліку зазвичай не потрібна (або зняття)
        if (isWeaponEquipmentSlot || isAmuletEquipmentSlot) return;

        if (currentItem is AmuletData amulet)
        {
            PlayerEquipment eq = FindFirstObjectByType<PlayerEquipment>();
            if (eq != null) eq.EquipAmulet(amulet);
        }
        else if (currentItem is WeaponData weapon)
        {
            PlayerCombat combat = FindFirstObjectByType<PlayerCombat>();
            if (combat != null) combat.EquipWeapon(weapon);
        }
        else
        {
            // Використання зілля
            InventoryManager.Instance.UseItem(currentItem);
        }
    }

    // --- DRAG & DROP ---
    public void OnBeginDrag(PointerEventData eventData)
    {
        StopHoldTimer();
        if (currentItem == null) return;
        canvasGroup.alpha = 0.6f;
        canvasGroup.blocksRaycasts = false;
        icon.transform.SetParent(groupCanvas.transform);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (currentItem == null) return;
        icon.rectTransform.anchoredPosition += eventData.delta / groupCanvas.scaleFactor;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = true;
        icon.transform.SetParent(this.transform);
        icon.rectTransform.anchoredPosition = Vector2.zero;
    }

    public void AddItem(Item newItem, int amount)
    {
        if (newItem == null) { ClearSlot(); return; }

        currentItem = newItem;
        currentAmount = amount;
        icon.sprite = newItem.icon;
        icon.enabled = true;
        icon.color = Color.white;

        if (stackText != null)
        {
            // ВИПРАВЛЕННЯ: Показуємо текст стаку завжди, якщо предмет стакається 
            // Це важливо для Хотбару, щоб бачити кількість
            if (newItem.isStackable)
            {
                stackText.text = amount.ToString();
                stackText.gameObject.SetActive(true);
            }
            else
            {
                stackText.gameObject.SetActive(false);
            }
        }

        // Повідомляємо системи про екіпірування, якщо предмет потрапив у спец-слот
        if (isWeaponEquipmentSlot && newItem is WeaponData weaponData)
        {
            PlayerCombat combat = FindFirstObjectByType<PlayerCombat>();
            if (combat != null) combat.EquipWeapon(weaponData);
            InventoryManager.Instance.EquipItem(newItem, true);
        }

        if (isAmuletEquipmentSlot && newItem is AmuletData amulet)
        {
            PlayerEquipment equipment = FindFirstObjectByType<PlayerEquipment>();
            if (equipment != null) equipment.EquipAmulet(amulet);
            InventoryManager.Instance.EquipItem(newItem, false);
        }
    }

    public void ClearSlot()
    {
        // Якщо очищаємо слот екіпіровки - знімаємо предмет з гравця
        if (isWeaponEquipmentSlot)
        {
            PlayerCombat combat = FindFirstObjectByType<PlayerCombat>();
            if (combat != null) combat.EquipWeapon(null);
            InventoryManager.Instance.UnequipItem(true);
        }

        if (isAmuletEquipmentSlot)
        {
            PlayerEquipment equipment = FindFirstObjectByType<PlayerEquipment>();
            if (equipment != null) equipment.UnequipAmulet();
            InventoryManager.Instance.UnequipItem(false);
        }

        currentItem = null;
        currentAmount = 0;
        icon.sprite = null;
        icon.enabled = false;
        if (stackText != null) stackText.gameObject.SetActive(false);
    }

    public void OnDrop(PointerEventData eventData)
    {
        InventorySlot sourceSlot = eventData.pointerDrag.GetComponent<InventorySlot>();
        if (sourceSlot != null && sourceSlot != this)
        {
            // Перевірка на тип (зброя до зброї і т.д.)
            if (this.isWeaponEquipmentSlot && sourceSlot.currentItem != null && !(sourceSlot.currentItem is WeaponData)) return;
            if (this.isAmuletEquipmentSlot && sourceSlot.currentItem != null && !(sourceSlot.currentItem is AmuletData)) return;

            Item itemToMove = sourceSlot.currentItem;
            int amountToMove = sourceSlot.currentAmount;
            Item itemToReplace = this.currentItem;
            int amountToReplace = this.currentAmount;

            // Логіка обміну
            this.AddItem(itemToMove, amountToMove);

            if (itemToReplace != null)
                sourceSlot.AddItem(itemToReplace, amountToReplace);
            else
                sourceSlot.ClearSlot();

            // Оновлюємо весь UI через менеджер, щоб спрацювала фільтрація "Equipped"
            InventoryManager.Instance.UpdateUI();
        }
    }
}