using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    public float speed = 2f;
    public float checkRadius = 5f;

    [Header("Settings")]
    public bool isAggroedByDamage = false;
    public bool spriteFacingLeft = false;

    [Header("References")]
    public Transform hpBarTransform;

    private Transform target;
    private Rigidbody2D rb;
    private Vector2 moveDirection;
    private bool isSearching = false; // Змінна для відстеження стану

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null) target = playerObj.transform;
    }

    void Update()
    {
        if (target == null) return;

        float distance = Vector2.Distance(transform.position, target.position);

        // Перевіряємо умови агресії
        isSearching = isAggroedByDamage || distance <= checkRadius;

        if (isSearching)
        {
            moveDirection = (target.position - transform.position).normalized;
            // Розвартаємо тільки коли бачимо/агримось
            HandleFlip();
        }
        else
        {
            moveDirection = Vector2.zero;
            // Коли гравець далеко — HandleFlip НЕ викликається, 
            // тому ворог стоїть так, як стояв востаннє.
        }
    }

    void FixedUpdate()
    {
        rb.MovePosition(rb.position + moveDirection * speed * Time.fixedDeltaTime);
    }

    void HandleFlip()
    {
        if (target == null) return;

        float directionToPlayer = target.position.x - transform.position.x;

        if (Mathf.Abs(directionToPlayer) > 0.1f)
        {
            float scaleX = (directionToPlayer > 0) ? 1 : -1;

            if (spriteFacingLeft) scaleX *= -1;

            transform.localScale = new Vector3(scaleX, 1, 1);

            if (hpBarTransform != null)
            {
                // ХП бар завжди дивиться прямо (компенсуємо розворот батька)
                hpBarTransform.localScale = new Vector3(scaleX, 1, 1);
            }
        }
    }
}