using UnityEngine;

[CreateAssetMenu(fileName = "NewAmulet", menuName = "RPG/Amulet")]
public class AmuletData : Item
{
    [Header("Бонуси кулона")]
    public int bonusMaxHealth = 50; // Скільки ХП додає кулон
    public int bonusDamage = 5;     // (Опціонально) Скільки урону додає
}