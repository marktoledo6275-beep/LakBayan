using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneController : MonoBehaviour
{
    [Header("Scene Names")]
    [SerializeField] private string mainMenuScene = "MainMenu";
    [SerializeField] private string defaultWorldScene = "MainWorld";

    [Header("Optional Loading UI")]
    [SerializeField] private GameObject loadingOverlay;
    [SerializeField] private float minimumLoadingOverlayTime = 0.25f;

    public static SceneController Instance { get; private set; }

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
    }

    public void LoadScene(string sceneName)
    {
        if (string.IsNullOrWhiteSpace(sceneName) || isLoading)
        {
            return;
        }

        if (!Application.CanStreamedLevelBeLoaded(sceneName))
        {
            Debug.LogWarning($"Scene '{sceneName}' is not in the active build list yet. Add it through File > Build Profiles before trying to load it.", this);
            return;
        }

        StartCoroutine(LoadSceneRoutine(sceneName));
    }

    public void LoadSceneFromButton(string sceneName)
    {
        LoadScene(sceneName);
    }

    public void LoadMainMenu()
    {
        LoadScene(mainMenuScene);
    }

    public void LoadWorldScene(string sceneName)
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.SetLastWorldScene(sceneName);
        }

        LoadScene(sceneName);
    }

    public void LoadMiniGameScene(string sceneName)
    {
        Scene activeScene = SceneManager.GetActiveScene();

        if (GameManager.Instance != null && activeScene.name != sceneName)
        {
            GameManager.Instance.SetLastWorldScene(activeScene.name);
        }

        LoadScene(sceneName);
    }

    public void LoadQuizScene(string sceneName)
    {
        Scene activeScene = SceneManager.GetActiveScene();

        if (GameManager.Instance != null && activeScene.name != sceneName)
        {
            GameManager.Instance.SetLastWorldScene(activeScene.name);
        }

        LoadScene(sceneName);
    }

    public void ReturnToLastWorldScene()
    {
        string targetScene = GameManager.Instance != null
            ? GameManager.Instance.LastWorldSceneName
            : defaultWorldScene;

        LoadScene(targetScene);
    }

    private IEnumerator LoadSceneRoutine(string sceneName)
    {
        isLoading = true;

        if (loadingOverlay != null)
        {
            loadingOverlay.SetActive(true);
        }

        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName);

        if (operation == null)
        {
            Debug.LogWarning($"Scene load failed for '{sceneName}'. Unity did not return a valid load operation.", this);

            if (loadingOverlay != null)
            {
                loadingOverlay.SetActive(false);
            }

            isLoading = false;
            yield break;
        }

        float timer = 0f;

        while (!operation.isDone || timer < minimumLoadingOverlayTime)
        {
            timer += Time.unscaledDeltaTime;
            yield return null;
        }

        if (loadingOverlay != null)
        {
            loadingOverlay.SetActive(false);
        }

        isLoading = false;
    }
}
