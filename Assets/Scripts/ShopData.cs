using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "NewShop", menuName = "RPG/Shop")]
public class ShopData : ScriptableObject
{
    [Header("Налаштування торговця")]
    public string shopName;

    [Tooltip("За скільки відсотків від ціни NPC купує твої речі? (0.5 = 50%)")]
    public float sellMultiplier = 0.5f;

    [Header("Асортимент")]
    public List<Item> itemsForSale;
}