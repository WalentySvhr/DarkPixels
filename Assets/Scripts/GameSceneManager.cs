using UnityEngine;

public class GameSceneManager : MonoBehaviour
{
    private void Awake()
    {
        // Фіксуємо ТІЛЬКИ горизонтальний режим (лівий або правий)
        Screen.orientation = ScreenOrientation.LandscapeRight;

        // Дозволяємо повертати телефон на 180 градусів (в інший ландшафт), 
        // але забороняємо вертикаль
        Screen.autorotateToPortrait = false;
        Screen.autorotateToPortraitUpsideDown = false;
        Screen.autorotateToLandscapeLeft = true;
        Screen.autorotateToLandscapeRight = true;
    }
}