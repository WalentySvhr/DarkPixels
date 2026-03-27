using UnityEngine;

public class CoinAnimation : MonoBehaviour
{
    [Header("Левітація (Вгору-Вниз)")]
    public float floatAmplitude = 0.2f;
    public float floatSpeed = 2.5f;

    [Header("Обертання")]
    public float rotationSpeed = 150f;
    public Vector3 rotationAxis = Vector3.up; // Vector3.up для 3D-ефекту, Vector3.forward для 2D-крутіння

    [Header("Пульсація (Масштаб)")]
    public bool usePulse = true;
    public float pulseAmount = 0.1f;    // Наскільки монета розширюється
    public float pulseSpeed = 4f;       // Швидкість пульсації

    private Vector3 startPosition;
    private Vector3 startScale;
    private float timeOffset;           // Рандом для розсинхронізації

    void Start()
    {
        startPosition = transform.position;
        startScale = transform.localScale;

        // Генеруємо випадкове число, щоб кожна монета жила своїм життям
        timeOffset = Random.Range(0f, 100f);
    }

    void Update()
    {
        // 1. Соковита Левітація
        // Додаємо timeOffset, щоб монети поруч не рухалися вгору-вниз одночасно
        float verticalOffset = Mathf.Sin((Time.time + timeOffset) * floatSpeed) * floatAmplitude;
        transform.position = startPosition + new Vector3(0, verticalOffset, 0);

        // 2. Обертання
        transform.Rotate(rotationAxis * rotationSpeed * Time.deltaTime);

        // 3. Соковита Пульсація (дихання монети)
        if (usePulse)
        {
            float pulse = Mathf.Cos((Time.time + timeOffset) * pulseSpeed) * pulseAmount;
            transform.localScale = startScale + new Vector3(pulse, pulse, 0);
        }
    }
}