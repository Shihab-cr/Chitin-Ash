using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTransactionManager : MonoBehaviour
{
    public static SceneTransactionManager Instance { get; private set; }

    [Header("References")]
    [SerializeField] private CanvasGroup blackScreen;
    [SerializeField] private LoadingBarController loadingBarController;

    [Header("Settings")]
    [SerializeField] private float fadeDuration = 1.0f;

    private bool isLoading;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (blackScreen != null)
        {
            blackScreen.alpha = 0f;
            blackScreen.blocksRaycasts = false;
        }
    }

    public void LoadSceneWithFade(string sceneName)
    {
        if (isLoading)
            return;

        StartCoroutine(LoadSceneRoutine(() => SceneManager.LoadSceneAsync(sceneName)));
    }

    public void LoadSceneWithFade(int sceneBuildIndex)
    {
        if (isLoading)
            return;

        StartCoroutine(LoadSceneRoutine(() => SceneManager.LoadSceneAsync(sceneBuildIndex)));
    }

    private IEnumerator LoadSceneRoutine(Func<AsyncOperation> loadOperation)
    {
        isLoading = true;
        Time.timeScale = 1f;

        if (blackScreen != null)
        {
            blackScreen.blocksRaycasts = true;
            yield return StartCoroutine(Fade(1f));
        }

        loadingBarController.ResetProgress();
        loadingBarController.Show();

        AsyncOperation operation = loadOperation();

        if (operation == null)
        {
            Debug.LogError("Scene failed to load. Check the scene name/index and Build Settings.");
            loadingBarController.Hide();

            if (blackScreen != null)
            {
                yield return StartCoroutine(Fade(0f));
                blackScreen.blocksRaycasts = false;
            }

            isLoading = false;
            yield break;
        }

        operation.allowSceneActivation = false;

        while (operation.progress < 0.9f)
        {
            float progress = operation.progress / 0.9f;
            loadingBarController.SetTargetProgress(progress);
            loadingBarController.UpdateDisplay();
            yield return null;
        }

        loadingBarController.SetTargetProgress(1f);

        while (!loadingBarController.IsComplete)
        {
            loadingBarController.UpdateDisplay();
            yield return null;
        }

        operation.allowSceneActivation = true;

        while (!operation.isDone)
        {
            yield return null;
        }

        loadingBarController.Hide();

        if (blackScreen != null)
        {
            yield return StartCoroutine(Fade(0f));
            blackScreen.blocksRaycasts = false;
        }

        isLoading = false;
    }

    private IEnumerator Fade(float targetAlpha)
    {
        float startAlpha = blackScreen.alpha;
        float time = 0f;

        while (time < fadeDuration)
        {
            time += Time.unscaledDeltaTime;
            float t = fadeDuration > 0f ? time / fadeDuration : 1f;
            blackScreen.alpha = Mathf.Lerp(startAlpha, targetAlpha, t);
            yield return null;
        }

        blackScreen.alpha = targetAlpha;
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }
}