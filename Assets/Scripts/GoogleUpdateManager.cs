using System.Collections;
using UnityEngine;
using Google.Play.AppUpdate;
using Google.Play.Common;

public class GoogleUpdateManager : MonoBehaviour
{
    private AppUpdateManager appUpdateManager;

    private void Start()
    {
        // Перевіряємо, чи ми на Android і чи це НЕ редактор Unity
#if UNITY_ANDROID && !UNITY_EDITOR
        appUpdateManager = new AppUpdateManager();
        StartCoroutine(CheckForUpdates());
#else
        Debug.Log("[GoogleUpdateManager] Роботу плагіна вимкнено в редакторі ПК.");
#endif
    }

    private IEnumerator CheckForUpdates()
    {
        // Цей код виконається лише на Android, бо корутина викликається у Start() під умовою
        PlayAsyncOperation<AppUpdateInfo, AppUpdateErrorCode> appUpdateInfoOperation =
            appUpdateManager.GetAppUpdateInfo();

        yield return appUpdateInfoOperation;

        if (appUpdateInfoOperation.IsSuccessful)
        {
            var appUpdateInfoResult = appUpdateInfoOperation.GetResult();

            if (appUpdateInfoResult.UpdateAvailability == UpdateAvailability.UpdateAvailable)
            {
                Debug.Log("Знайдено оновлення! Запускаємо примусове вікно...");

                // Офіційний і єдиний робочий метод для версії 1.8.5
                var updateOptions = AppUpdateOptions.ImmediateAppUpdateOptions();

                var startUpdateOperation = appUpdateManager.StartUpdate(
                    appUpdateInfoResult,
                    updateOptions
                );

                yield return startUpdateOperation;
            }
        }
    }

    private void OnEnable()
    {
        // Додатковий захист для OnEnable, щоб він не викликався в редакторі
#if UNITY_ANDROID && !UNITY_EDITOR
        if (appUpdateManager != null)
        {
            StartCoroutine(ResumeUpdate());
        }
#endif
    }

    private IEnumerator ResumeUpdate()
    {
        var appUpdateInfoOperation = appUpdateManager.GetAppUpdateInfo();
        yield return appUpdateInfoOperation;

        if (appUpdateInfoOperation.IsSuccessful)
        {
            var appUpdateInfoResult = appUpdateInfoOperation.GetResult();

            if (appUpdateInfoResult.UpdateAvailability == UpdateAvailability.DeveloperTriggeredUpdateInProgress)
            {
                var updateOptions = AppUpdateOptions.ImmediateAppUpdateOptions();
                yield return appUpdateManager.StartUpdate(appUpdateInfoResult, updateOptions);
            }
        }
    }
}
