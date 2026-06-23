using UnityEngine;
using UnityEngine.SceneManagement;

public class GuitarInputManager : MonoBehaviour
{
    public static GuitarInputManager Instance;
    public static event System.Action<int> OnStringPlayed;

    [Header("场景跳转")]
    public string mainSceneName = "MainScene";
    public string endingSceneName = "EndingScene";
    public string transitionSceneName = "TransitionScene";
    public string titleSceneName = "TitleScene";

    private bool skipTriggered = false;

    void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // 订阅输入事件
        KeyboardGuitarSimulator.OnStringPlayedStatic += HandleInput;
        GuitarBluetoothInput.OnStringPlayed += HandleInput;

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        ResetSkip();
        Debug.Log($"[GuitarInputManager] 场景加载: {scene.name}，跳过标记已重置");
    }

    void Update()
    {
        // ★★★ 键盘检测（所有场景通用）★★★
        if (Input.GetKeyDown(KeyCode.Alpha1)) HandleInput(1);
        if (Input.GetKeyDown(KeyCode.Alpha2)) HandleInput(2);
        if (Input.GetKeyDown(KeyCode.Alpha3)) HandleInput(3);

        // ★ 键盘 4：MainScene 快速跳转
        if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            string currentScene = SceneManager.GetActiveScene().name;
            if (currentScene == mainSceneName)
            {
                Debug.Log($"[跳过] 按 4 键：{currentScene} → {endingSceneName}");
                SceneManager.LoadScene(endingSceneName);
            }
        }
    }

    private void HandleInput(int stringId)
    {
        Debug.Log($"🎸 GuitarInputManager 收到弦 {stringId}");
        
        // 转发给所有订阅者
        OnStringPlayed?.Invoke(stringId);

        string scene = SceneManager.GetActiveScene().name;

        // ★ EndingScene: 按 1 跳到 TransitionScene
        if (scene == endingSceneName && stringId == 1 && !skipTriggered)
        {
            TriggerSkip(transitionSceneName);
        }

        // ★ TransitionScene: 按 1 跳到 TitleScene
        if (scene == transitionSceneName && stringId == 1 && !skipTriggered)
        {
            TriggerSkip(titleSceneName);
        }

        // ★ TitleScene: 按 1 或 2 可以由 GameIntroFlowController 处理
        // 这里不处理，让 GameIntroFlowController 自己处理
    }

    void TriggerSkip(string targetScene)
    {
        if (skipTriggered) return;
        skipTriggered = true;
        Debug.Log($"[跳过] 触发！加载 {targetScene}");
        SceneManager.LoadScene(targetScene);
    }

    public void ResetSkip()
    {
        skipTriggered = false;
    }

    void OnDestroy()
    {
        KeyboardGuitarSimulator.OnStringPlayedStatic -= HandleInput;
        GuitarBluetoothInput.OnStringPlayed -= HandleInput;
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
}