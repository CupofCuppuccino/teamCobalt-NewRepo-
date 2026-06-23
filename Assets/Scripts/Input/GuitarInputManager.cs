using UnityEngine;
using UnityEngine.SceneManagement;

public class GuitarInputManager : MonoBehaviour
{
    public static GuitarInputManager Instance;
    public static event System.Action<int> OnStringPlayed;

    [Header("场景名称")]
    public string mainSceneName = "MainScene";
    public string endingSceneName = "EndingScene";
    public string transitionSceneName = "TransitionScene";
    public string titleSceneName = "TitleScene";

    private bool skipTriggered = false;  // ★ 添加

    void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        KeyboardGuitarSimulator.OnStringPlayedStatic += OnStringPlayed;
        GuitarBluetoothInput.OnStringPlayed += OnStringPlayed;
    }

    void Update()
    {
        string currentScene = SceneManager.GetActiveScene().name;

        if (Input.GetKeyDown(KeyCode.Alpha1)) OnStringPlayed?.Invoke(1);
        if (Input.GetKeyDown(KeyCode.Alpha2)) OnStringPlayed?.Invoke(2);
        if (Input.GetKeyDown(KeyCode.Alpha3)) OnStringPlayed?.Invoke(3);

        if (Input.GetKeyDown(KeyCode.Alpha4) && currentScene == mainSceneName)
        {
            Debug.Log($"⏭️ 按 4：{currentScene} → {endingSceneName}");
            SceneManager.LoadScene(endingSceneName);
        }

        if (Input.GetKeyDown(KeyCode.Alpha1) && currentScene == endingSceneName)
        {
            Debug.Log($"⏭️ 按 1：{currentScene} → {transitionSceneName}");
            SceneManager.LoadScene(transitionSceneName);
        }

        if (Input.GetKeyDown(KeyCode.Alpha1) && currentScene == transitionSceneName)
        {
            Debug.Log($"⏭️ 按 1：{currentScene} → {titleSceneName}");
            SceneManager.LoadScene(titleSceneName);
        }
    }

    // ★ 添加 ResetSkip 方法
    public void ResetSkip()
    {
        skipTriggered = false;
        Debug.Log("🔄 GuitarInputManager: 跳过标记已重置");
    }

    void OnDestroy()
    {
        KeyboardGuitarSimulator.OnStringPlayedStatic -= OnStringPlayed;
        GuitarBluetoothInput.OnStringPlayed -= OnStringPlayed;
    }
}