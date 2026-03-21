using UnityEngine;

public class HealthBarFix : MonoBehaviour
{
    private Vector3 initialScale;

    void Start()
    {
        // Запам'ятовуємо початковий масштаб (наприклад, 0.01, 0.01, 0.01)
        initialScale = transform.localScale;
    }

    void LateUpdate()
    {
        // 1. Фіксуємо поворот: смужка завжди рівна, незалежно від нахилу ворога
        transform.rotation = Quaternion.identity;

        // 2. Виправляємо дзеркальність:
        // Якщо батьківський об'єкт (ворог) розвернутий вліво (Scale.x < 0),
        // ми примусово тримаємо масштаб Canvas'а позитивним.
        Vector3 parentScale = transform.parent.localScale;
        
        transform.localScale = new Vector3(
            initialScale.x / (parentScale.x / Mathf.Abs(parentScale.x)), 
            initialScale.y, 
            initialScale.z
        );
    }
}