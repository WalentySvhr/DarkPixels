using UnityEngine;
using System.Collections;

public enum PetState
{
    Idle,   // Пет стоїть і відпочиває на точці блукання
    Follow, // Пет активно біжить за гравцем, який рухається
    Wander  // Пет нудьгує і рандомно ходить навколо гравця
}

public class PetAI : MonoBehaviour
{
    [Header("Налаштування станів")]
    public PetState currentState = PetState.Follow;
    [Tooltip("Через скільки секунд нерухомості гравця пет почне нудьгувати")]
    public float timeToWander = 3f;
    [Tooltip("Радіус випадкового блукання навколо гравця")]
    public float wanderRadius = 3f;
    [Tooltip("Мінімальний та максимальний час відпочинку між зміною точок блукання")]
    public float minWanderDelay = 2f;
    public float maxWanderDelay = 5f;

    private PetFollower motor;
    private Transform player;

    private float playerIdleTimer = 0f;
    private Vector3 lastPlayerPosition;
    private bool isWanderRoutineRunning = false;
    private Vector3 wanderTarget;

    void Start()
    {
        motor = GetComponent<PetFollower>();

        if (motor != null && motor.playerTarget != null)
        {
            player = motor.playerTarget;
        }
        else
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null) player = playerObj.transform;
        }

        if (player != null) lastPlayerPosition = player.position;
    }

    void Update()
    {
        if (player == null || motor == null) return;

        // Перевіряємо, чи рухається гравець (з урахуванням мікро-похибок)
        bool isPlayerMoving = Vector2.Distance(player.position, lastPlayerPosition) > 0.05f;
        lastPlayerPosition = player.position;

        if (isPlayerMoving)
        {
            // Господар кудись пішов -> скасовуємо блукання і біжимо за ним
            if (isWanderRoutineRunning)
            {
                StopAllCoroutines();
                isWanderRoutineRunning = false;
            }
            currentState = PetState.Follow;
            playerIdleTimer = 0f;
        }
        else
        {
            // Господар стоїть на місці -> накопичуємо таймер нудьги
            if (currentState == PetState.Follow)
            {
                playerIdleTimer += Time.deltaTime;
                if (playerIdleTimer >= timeToWander)
                {
                    currentState = PetState.Wander;
                }
            }
        }

        // Виконуємо логіку поточного стану ШІ
        ExecuteStateLogic();
    }

    void ExecuteStateLogic()
    {
        switch (currentState)
        {
            case PetState.Follow:
                // Ціль руху — координати гравця, зупиняємось на стандартній дистанції
                motor.movementTarget = player.position;
                motor.currentStopDistance = motor.stopDistance;
                break;

            case PetState.Wander:
                // Запускаємо корутину блукання, якщо вона ще не активна
                if (!isWanderRoutineRunning)
                {
                    StartCoroutine(WanderRoutine());
                }
                break;

            case PetState.Idle:
                // Пет стоїть на місці блукання і чекає (керується корутиною)
                break;
        }
    }

    private IEnumerator WanderRoutine()
    {
        isWanderRoutineRunning = true;

        while (currentState == PetState.Wander)
        {
            // 1. Генеруємо випадкову точку всередині кола навколо гравця
            Vector2 randomCircle = Random.insideUnitCircle * wanderRadius;
            wanderTarget = player.position + new Vector3(randomCircle.x, randomCircle.y, 0);

            // 2. Відправляємо туди пета (дистанція зупинки мінімальна, щоб підійти точно в точку)
            motor.movementTarget = wanderTarget;
            motor.currentStopDistance = 0.2f;

            // 3. Чекаємо, поки пет дійде до точки (або вийде час, якщо він десь застряг у колайдері)
            float timeout = 3f;
            while (Vector2.Distance(transform.position, wanderTarget) > 0.3f && timeout > 0f)
            {
                timeout -= Time.deltaTime;
                yield return null;
            }

            // 4. Пет дійшов до точки блукання, переводимо його в Idle для відпочинку
            PetState previousState = currentState;
            currentState = PetState.Idle;
            motor.movementTarget = transform.position; // Наказуємо зафіксуватися на місці

            // Випадковий час відпочинку помічника
            float restTime = Random.Range(minWanderDelay, maxWanderDelay);
            yield return new WaitForSeconds(restTime);

            // Якщо за час відпочинку гравець не побіг, повертаємо стан Wander для нової точки
            if (currentState == PetState.Idle)
            {
                currentState = PetState.Wander;
            }
        }

        isWanderRoutineRunning = false;
    }
}