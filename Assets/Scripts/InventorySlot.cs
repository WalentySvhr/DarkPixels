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
    public bool isRingEquipmentSlot = false;
    public bool isBeltEquipmentSlot = false; // --- ДОДАНО: Слот для пояса ---
    [Tooltip("Для кілець: вкажи 1 або 2. Для інших слотів залиш 0.")]
    public int ringSlotIndex = 0;
    public bool isHotbarSlot = false;

    [Header("Візуал (Плейсхолдер)")]
    [Tooltip("Сюди перетягни сіру фонову іконку (ImageWeapon, ImageAmulet тощо)")]
    public GameObject placeholderImage;

    [Header("Елементи UI")]
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

        if (isHotbarSlot)
        {
            HandleAction();
        }
        else
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

        // Блокуємо використання кліком, якщо предмет вже одягнений в будь-який слот екіпіровки
        if (isWeaponEquipmentSlot || isAmuletEquipmentSlot || isRingEquipmentSlot || isBeltEquipmentSlot) return;

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
        else if (currentItem is RingData ring)
        {
            PlayerEquipment eq = FindFirstObjectByType<PlayerEquipment>();
            if (eq != null) eq.EquipRing(ring, 1);
        }
        else if (currentItem is BeltData belt)
        {
            PlayerEquipment eq = FindFirstObjectByType<PlayerEquipment>();
            if (eq != null) eq.EquipBelt(belt);
        }
        else
        {
            InventoryManager.Instance.UseItem(currentItem);
        }
    }

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

        if (placeholderImage != null) placeholderImage.SetActive(false);

        if (stackText != null)
        {
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

        // --- ЕКІПІРОВКА ПРЕДМЕТІВ ---
        if (isWeaponEquipmentSlot && newItem is WeaponData weaponData)
        {
            PlayerCombat combat = FindFirstObjectByType<PlayerCombat>();
            if (combat != null) combat.EquipWeapon(weaponData);
            InventoryManager.Instance.EquipItem(newItem, "Weapon");
        }
        else if (isAmuletEquipmentSlot && newItem is AmuletData amulet)
        {
            PlayerEquipment equipment = FindFirstObjectByType<PlayerEquipment>();
            if (equipment != null) equipment.EquipAmulet(amulet);
            InventoryManager.Instance.EquipItem(newItem, "Amulet");
        }
        else if (isRingEquipmentSlot && newItem is RingData ring)
        {
            PlayerEquipment equipment = FindFirstObjectByType<PlayerEquipment>();
            if (equipment != null) equipment.EquipRing(ring, ringSlotIndex);
            InventoryManager.Instance.EquipItem(newItem, "Ring", ringSlotIndex);
        }
        else if (isBeltEquipmentSlot && newItem is BeltData belt)
        {
            PlayerEquipment equipment = FindFirstObjectByType<PlayerEquipment>();
            if (equipment != null) equipment.EquipBelt(belt);
            InventoryManager.Instance.EquipItem(newItem, "Belt");
        }
    }

    public void ClearSlot()
    {
        if (isWeaponEquipmentSlot)
        {
            PlayerCombat combat = FindFirstObjectByType<PlayerCombat>();
            if (combat != null) combat.EquipWeapon(null);
            InventoryManager.Instance.UnequipItem("Weapon");
        }
        else if (isAmuletEquipmentSlot)
        {
            PlayerEquipment equipment = FindFirstObjectByType<PlayerEquipment>();
            if (equipment != null) equipment.UnequipAmulet();
            InventoryManager.Instance.UnequipItem("Amulet");
        }
        else if (isRingEquipmentSlot)
        {
            PlayerEquipment equipment = FindFirstObjectByType<PlayerEquipment>();
            if (equipment != null) equipment.UnequipRing(ringSlotIndex);
            InventoryManager.Instance.UnequipItem("Ring", ringSlotIndex);
        }
        else if (isBeltEquipmentSlot)
        {
            PlayerEquipment equipment = FindFirstObjectByType<PlayerEquipment>();
            if (equipment != null) equipment.UnequipBelt();
            InventoryManager.Instance.UnequipItem("Belt");
        }

        currentItem = null;
        currentAmount = 0;
        icon.sprite = null;
        icon.enabled = false;

        if (placeholderImage != null) placeholderImage.SetActive(true);

        if (stackText != null) stackText.gameObject.SetActive(false);
    }

    // --- МАГІЯ ВИДАЛЕННЯ ДУБЛІКАТА ТУТ ---
    public void OnDrop(PointerEventData eventData)
    {
        InventorySlot sourceSlot = eventData.pointerDrag.GetComponent<InventorySlot>();
        if (sourceSlot != null && sourceSlot != this)
        {
            // Перевірка, щоб у слоти падав ТІЛЬКИ правильний тип
            if (this.isWeaponEquipmentSlot && sourceSlot.currentItem != null && !(sourceSlot.currentItem is WeaponData)) return;
            if (this.isAmuletEquipmentSlot && sourceSlot.currentItem != null && !(sourceSlot.currentItem is AmuletData)) return;
            if (this.isRingEquipmentSlot && sourceSlot.currentItem != null && !(sourceSlot.currentItem is RingData)) return;
            if (this.isBeltEquipmentSlot && sourceSlot.currentItem != null && !(sourceSlot.currentItem is BeltData)) return;

            Item itemToMove = sourceSlot.currentItem;
            int amountToMove = sourceSlot.currentAmount;
            Item itemToReplace = this.currentItem;
            int amountToReplace = this.currentAmount;

            // Визначаємо, чи цей слот є слотом екіпіровки, і чи слот-джерело є слотом екіпіровки
            bool isThisEquipSlot = this.isWeaponEquipmentSlot || this.isAmuletEquipmentSlot || this.isRingEquipmentSlot || this.isBeltEquipmentSlot;
            bool isSourceEquipSlot = sourceSlot.isWeaponEquipmentSlot || sourceSlot.isAmuletEquipmentSlot || sourceSlot.isRingEquipmentSlot || sourceSlot.isBeltEquipmentSlot;

            // Якщо тягнемо зі звичайного інвентарю в екіпіровку
            bool equipping = isThisEquipSlot && !isSourceEquipSlot;
            // Якщо тягнемо з екіпіровки назад у звичайний інвентар
            bool unequipping = !isThisEquipSlot && isSourceEquipSlot;

            this.AddItem(itemToMove, amountToMove);

            if (itemToReplace != null)
                sourceSlot.AddItem(itemToReplace, amountToReplace);
            else
                sourceSlot.ClearSlot();

            // Видаляємо або додаємо в основний масив інвентарю
            if (InventoryManager.Instance != null)
            {
                if (equipping)
                {
                    InventoryManager.Instance.Remove(itemToMove); // ВИПРАВЛЕНО НА Remove
                    if (itemToReplace != null) InventoryManager.Instance.Add(itemToReplace); // ВИПРАВЛЕНО НА Add
                }
                else if (unequipping)
                {
                    InventoryManager.Instance.Add(itemToMove); // ВИПРАВЛЕНО НА Add
                    if (itemToReplace != null) InventoryManager.Instance.Remove(itemToReplace); // ВИПРАВЛЕНО НА Remove
                }
            }

            InventoryManager.Instance.UpdateUI();
        }
    }
}