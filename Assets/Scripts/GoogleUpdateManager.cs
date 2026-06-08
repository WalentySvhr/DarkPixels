using System.Collections;
using UnityEngine;
using Google.Play.AppUpdate;
using Google.Play.Common;

public class GoogleUpdateManager : MonoBehaviour
{
    private AppUpdateManager appUpdateManager;

    private void Start()
    {
        appUpdateManager = new AppUpdateManager();
        StartCoroutine(CheckForUpdates());
    }

    private IEnumerator CheckForUpdates()
    {
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
        if (appUpdateManager != null)
        {
            StartCoroutine(ResumeUpdate());
        }
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