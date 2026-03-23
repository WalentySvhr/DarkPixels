using UnityEngine;
using System.Collections;

public class TopDownLoot : MonoBehaviour
{
    [Header("Jump Settings")]
    public float minJumpHeight = 1.5f;
    public float maxJumpHeight = 2.5f;
    public float jumpDuration = 0.6f;

    public Transform visualChild;

    void Start()
    {
        if (visualChild == null && transform.childCount > 0)
            visualChild = transform.GetChild(0);

        // Випадкова висота для кожної монетки
        float height = Random.Range(minJumpHeight, maxJumpHeight);
        StartCoroutine(SimulateJump(height));
    }

    IEnumerator SimulateJump(float height)
    {
        float timer = 0;
        while (timer < jumpDuration)
        {
            timer += Time.deltaTime;
            float progress = timer / jumpDuration;

            // Парабола для імітації висоти (Z-axis у 3D, але Y у нашому 2D)
            float yOffset = 4 * height * progress * (1 - progress);

            if (visualChild != null)
                visualChild.localPosition = new Vector3(0, yOffset, 0);

            yield return null;
        }

        if (visualChild != null)
            visualChild.localPosition = Vector3.zero;
    }
}