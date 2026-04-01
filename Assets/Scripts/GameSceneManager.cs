using UnityEngine;

public class GameSceneManager : MonoBehaviour
{
    private void Awake()
    {
        // 1. Дозволяємо Unity самій вирішувати, як крутити екран (AutoRotation)
        Screen.orientation = ScreenOrientation.AutoRotation;

        // 2. Забороняємо вертикальні положення
        Screen.autorotateToPortrait = false;
        Screen.autorotateToPortraitUpsideDown = false;

        // 3. Дозволяємо обидва горизонтальні положення
        Screen.autorotateToLandscapeLeft = true;
        Screen.autorotateToLandscapeRight = true;
    }
}