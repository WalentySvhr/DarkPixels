using UnityEngine;

public class WeaponManager : MonoBehaviour
{
    // Enum для вибору типу зброї
    public enum WeaponType { Sword, Bow }
    public WeaponType currentWeapon = WeaponType.Sword;

    [Header("Ownership")]
    public bool hasSword = true;
    public bool hasBow = false;

    [Header("Prices")]
    public int bowPrice = 50;
    public GameObject bowCheckmark;
  public GameObject[] weaponChecks;
  public GameObject[] weapons;
    private PlayerInventory inventory;

    void Start()
    {
        inventory = GetComponent<PlayerInventory>();
        
        // При старті завжди беремо меч, якщо він є
        if (hasSword)
        {
            EquipWeapon(0); // 0 = Меч
        }
    }

    // Метод для кнопки "Купити Лук"
   public void BuyBow()
{
    if (hasBow)
    {
        Debug.Log("Лук вже куплено!");
        return;
    }

    if (inventory.currentCoins >= bowPrice)
    {
        inventory.AddCoins(-bowPrice);
        hasBow = true;

        Debug.Log("Лук куплено!");

        bowCheckmark.SetActive(true); // показати галочку

        EquipWeapon(1);
    }
    else
    {
        Debug.Log("Недостатньо монет!");
    }
}
    // Метод для кнопок перемикання (викликається з UI)
    // 0 = Sword, 1 = Bow
  public void EquipWeapon(int weaponIndex)
{
    WeaponType targetType = (WeaponType)weaponIndex;

    if (targetType == WeaponType.Sword && hasSword)
    {
        currentWeapon = WeaponType.Sword;
        Debug.Log("В руках Меч");
    }
    else if (targetType == WeaponType.Bow && hasBow)
    {
        currentWeapon = WeaponType.Bow;
        Debug.Log("В руках Лук");
    }
    else
    {
        Debug.Log("Цю зброю ще не куплено!");
        return;
    }

    // вимикаємо всі галочки
    foreach (GameObject check in weaponChecks)
    {
        check.SetActive(false);
    }

    // включаємо потрібну галочку
    weaponChecks[weaponIndex].SetActive(true);

    // вимикаємо всю зброю
    foreach (GameObject weapon in weapons)
    {
        weapon.SetActive(false);
    }

    // включаємо потрібну зброю
    weapons[weaponIndex].SetActive(true);
}
}