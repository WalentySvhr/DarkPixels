using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target; 
    public float smoothSpeed = 0.125f;
    public Vector3 offset = new Vector3(0, 0, -10); // Важливо: Z = -10

    void LateUpdate()
    {
        if (target == null) return;

        // Беремо позицію гравця + наш офсет
        Vector3 desiredPosition = target.position + offset;

        // ПЕРЕСТРАХОВКА: примусово тримаємо камеру на відстані -10 по Z
        desiredPosition.z = -10f; 

        // Плавно рухаємося
        transform.position = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
    }
}