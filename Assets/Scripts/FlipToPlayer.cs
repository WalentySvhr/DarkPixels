using UnityEngine;

public class FlipToPlayer : MonoBehaviour
{
    [Header("Settings")]
    public string playerTag = "Player";
    public bool startFacingRight = true;

    [Header("References")]
    // --- НОВА ЗМІННА ---
    public Transform hpBarTransform; // Перетягни сюди об'єкт ХП бару в інспекторі

    private Transform playerTransform;
    private bool isFacingRight;

    void Start()
    {
        isFacingRight = startFacingRight;

        GameObject playerObj = GameObject.FindGameObjectWithTag(playerTag);
        if (playerObj != null)
        {
            playerTransform = playerObj.transform;
        }
    }

    void Update()
    {
        if (playerTransform == null) return;

        if (playerTransform.position.x > transform.position.x && !isFacingRight)
        {
            Flip();
        }
        else if (playerTransform.position.x < transform.position.x && isFacingRight)
        {
            Flip();
        }
    }

    void Flip()
    {
        isFacingRight = !isFacingRight;

        // 1. Розвертаємо самого ворога (як і раніше)
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;

        // --- НОВИЙ БЛОК КОДУ ---
        // 2. Тепер компенсуємо розворот ХП бару
        if (hpBarTransform != null)
        {
            // Беремо ПОТОЧНИЙ локальний масштаб бару (який вже вивернувся батьком)
            Vector3 hpScale = hpBarTransform.localScale;

            // І множимо його X на -1, щоб вивернути його НАЗАД у правильний стан
            hpScale.x *= -1;

            // Призначаємо виправлений масштаб
            hpBarTransform.localScale = hpScale;
        }
    }
}