using UnityEngine;
using System.Collections;

public class TopDownLoot : MonoBehaviour
{
    [Header("Налаштування розльоту")]
    public float minJumpHeight = 1.0f;
    public float maxJumpHeight = 2.0f;
    public float jumpDuration = 0.5f;
    public float spreadRadius = 1.5f;

    [Header("Налаштування анімації (після падіння)")]
    public bool useAnimation = true;
    public float rotationSpeed = 50f;      // Швидкість обертання (градуси в сек)
    public float floatAmplitude = 0.2f;    // Висота коливання (вгору-вниз)
    public float floatSpeed = 2f;        // Швидкість коливання

    [Header("Посилання")]
    public Transform visualChild;

    private bool isFlying = true;
    private float startAnimY;

    void Start()
    {
        if (visualChild == null && transform.childCount > 0)
            visualChild = transform.GetChild(0);

        float height = Random.Range(minJumpHeight, maxJumpHeight);

        Vector3 randomDirection = Random.insideUnitCircle * spreadRadius;
        Vector3 targetPosition = transform.position + new Vector3(randomDirection.x, randomDirection.y, 0);

        StartCoroutine(SimulateLootDrop(targetPosition, height));
    }

    void Update()
    {
        // Анімація виконується тільки тоді, коли предмет закінчив розліт
        if (!isFlying && useAnimation && visualChild != null)
        {
            // 1. Обертання навколо осі Y (або Z для 2D, вибирай за смаком)
            visualChild.Rotate(Vector3.up * rotationSpeed * Time.deltaTime);

            // 2. Коливання вгору-вниз (синусоїда)
            float newY = Mathf.Sin(Time.time * floatSpeed) * floatAmplitude;
            visualChild.localPosition = new Vector3(0, newY, 0);
        }
    }

    IEnumerator SimulateLootDrop(Vector3 targetPos, float height)
    {
        isFlying = true;
        float timer = 0;
        Vector3 startPos = transform.position;

        while (timer < jumpDuration)
        {
            timer += Time.deltaTime;
            float progress = timer / jumpDuration;

            // Рух по землі
            transform.position = Vector3.Lerp(startPos, targetPos, progress);

            // Стрибок
            float yOffset = 4 * height * progress * (1 - progress);
            if (visualChild != null)
                visualChild.localPosition = new Vector3(0, yOffset, 0);

            yield return null;
        }

        transform.position = targetPos;
        isFlying = false; // Політ закінчено, вмикаємо анімацію в Update
    }
}