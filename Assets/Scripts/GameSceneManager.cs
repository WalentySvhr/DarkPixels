using UnityEngine;

public class GameSceneManager : MonoBehaviour
{
    private void Awake()
    {
        ApplyGameOrientation();
    }

    private void Start()
    {
        // Повторюємо в Start, бо редактор Unity часто скидає налаштування 
        // відразу після Awake при зміні розширення
        ApplyGameOrientation();
    }

    private void ApplyGameOrientation()
    {
        // 1. Дозволяємо автоповорот
        Screen.orientation = ScreenOrientation.AutoRotation;

        // 2. Забороняємо портретні режими
        Screen.autorotateToPortrait = false;
        Screen.autorotateToPortraitUpsideDown = false;

        // 3. Дозволяємо обидва ландшафтні режими
        Screen.autorotateToLandscapeLeft = true;
        Screen.autorotateToLandscapeRight = true;

        // 4. ПРИМУСОВО виставляємо ландшафт зараз
        // Це «лікує» баг редактора, коли він перемикається на Portrait
        Screen.orientation = ScreenOrientation.LandscapeLeft;
    }

    // Додатковий захист для редактора: 
    // якщо ти міняєш девайс під час гри, цей метод спрацює при зміні розміру вікна
    private void OnRectTransformDimensionsChange()
    {
        if (Screen.orientation == ScreenOrientation.Portrait)
        {
            Screen.orientation = ScreenOrientation.LandscapeLeft;
        }
    }
}