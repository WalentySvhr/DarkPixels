using UnityEngine;

public enum WeaponType { Melee, Ranged }

[CreateAssetMenu(fileName = "NewWeapon", menuName = "RPG/Weapon")]
public class WeaponData : ScriptableObject
{
    public string weaponName;
    public WeaponType type;
    public Sprite weaponIcon; // Іконка для відображення в магазині
    public int price;         // Ціна зброї

    [Header("Stats")]
    public int damage = 10;
    public float attackRange = 2f;
    public float cooldown = 0.5f; // Час між атаками
    public float damageDelay = 0.2f; // Час від початку анімації до моменту нанесення урону

    [Header("Visuals")]
    public GameObject visualPrefab; // Префаб меча/лука (модель)
    public string speedParameterName = "AttackSpeed"; // Назва Float параметра в Animator

    [Header("Ranged Settings")]
    public GameObject projectilePrefab; // Стріла/снаряд
    public float shootForce = 20f;

    // Метод для автоматичного оновлення швидкості аніматора
    public void UpdateAnimationSpeed(Animator anim)
    {
        if (anim != null && !string.IsNullOrEmpty(speedParameterName))
        {
            // Формула: 1 ділимо на кулдаун. 
            // Якщо кулдаун 0.5с -> швидкість 2.0 (анімація вдвічі швидша)
            float animSpeed = 1f / cooldown;
            anim.SetFloat(speedParameterName, animSpeed);
        }
    }
}