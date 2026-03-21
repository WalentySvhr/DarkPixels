using UnityEngine;
using UnityEngine.UI; // Обов'язково додаємо для роботи з Image

public class WeaponUI : MonoBehaviour
{
    public Image weaponIconDisplay; // Сюди перетягнеш компонент Image
    public Sprite swordSprite;     // Сюди перетягнеш картинку меча
    public Sprite bowSprite;       // Сюди перетягнеш картинку лука

    private WeaponManager weaponManager;

    void Start()
    {
        weaponManager = FindAnyObjectByType<WeaponManager>();
        UpdateIcon(); // Встановити початкову іконку
    }

    void Update()
    {
        // Можна оновлювати кожен кадр, або краще викликати метод UpdateIcon() 
        // безпосередньо з WeaponManager, коли зброя змінюється.
        UpdateIcon();
    }

    public void UpdateIcon()
    {
        if (weaponManager == null) return;

        if (weaponManager.currentWeapon == WeaponManager.WeaponType.Sword)
        {
            weaponIconDisplay.sprite = swordSprite;
        }
        else if (weaponManager.currentWeapon == WeaponManager.WeaponType.Bow)
        {
            weaponIconDisplay.sprite = bowSprite;
        }
    }
}