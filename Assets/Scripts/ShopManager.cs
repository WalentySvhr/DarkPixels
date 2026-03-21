using UnityEngine;
using UnityEngine.UI;

public class ShopManager : MonoBehaviour
{
    public GameObject shopPanel; 

    // Викликається кнопкою "Відкрити Магазин"
    public void OpenShop()
    {
        
        shopPanel.SetActive(true);
        Time.timeScale = 0f; // Пауза гри
    }

    // Викликається кнопкою "Закрити" всередині панелі магазину
    public void CloseShop()
    {
        shopPanel.SetActive(false);
        Time.timeScale = 1f; // Гра продовжується
    }

    // Логіка кнопок
    public void BuyBow() { /* Твій код покупки */ }
    public void SelectSword() { /* weaponManager.SetWeapon(Sword); */ }
    public void SelectBow() { /* weaponManager.SetWeapon(Bow); */ }
}