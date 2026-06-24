using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class GuitarInputManager : MonoBehaviour
{
    public static GuitarInputManager Instance;
    public static event System.Action<int> OnStringPlayed;

    [Header("场景名称")]
    public string mainSceneName = "MainScene";
    public string endingSceneName = "EndingScene";
    public string transitionSceneName = "TransitionScene";
    public string titleSceneName = "TitleScreen";

    private Keyboard keyboard;

    void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        
        keyboard = Keyboard.current;
        Debug.Log("🎸 GuitarInputManager Awake");
    }

    void OnEnable()
    {
        // ★ 订阅所有输入源
        KeyboardGuitarSimulator.OnStringPlayedStatic += OnAnyStringPlayed;
        GuitarBluetoothInput.OnStringPlayed += OnAnyStringPlayed;
        Debug.Log("🎸 GuitarInputManager 已订阅输入");
    }

    void OnDisable()
    {
        KeyboardGuitarSimulator.OnStringPlayedStatic -= OnAnyStringPlayed;
        GuitarBluetoothInput.OnStringPlayed -= OnAnyStringPlayed;
    }

    void Update()
    {
        string currentScene = SceneManager.GetActiveScene().name;

        // ★ 键盘直接检测（备用）
        if (keyboard == null) keyboard = Keyboard.current;
        if (keyboard != null)
        {
            if (keyboard.digit1Key.wasPressedThisFrame) OnStringPlayed?.Invoke(1);
            if (keyboard.digit2Key.wasPressedThisFrame) OnStringPlayed?.Invoke(2);
            if (keyboard.digit3Key.wasPressedThisFrame) OnStringPlayed?.Invoke(3);

            // MainScene: 按 4 跳到 Ending
            if (keyboard.digit4Key.wasPressedThisFrame && currentScene == mainSceneName)
            {
                Debug.Log($"⏭️ 按 4：{currentScene} → {endingSceneName}");
                SceneManager.LoadScene(endingSceneName);
            }
        }

        // ★ EndingScene: 按 1 跳到 Transition（键盘和吉他都可以触发）
        if (currentScene == endingSceneName)
        {
            // 键盘检测
            if (keyboard != null && keyboard.digit1Key.wasPressedThisFrame)
            {
                Debug.Log($"⏭️ 按 1：{currentScene} → {transitionSceneName}");
                SceneManager.LoadScene(transitionSceneName);
            }
        }

        // ★ TransitionScene: 按 1 跳到 Title
        if (currentScene == transitionSceneName)
        {
            if (keyboard != null && keyboard.digit1Key.wasPressedThisFrame)
            {
                Debug.Log($"⏭️ 按 1：{currentScene} → {titleSceneName}");
                SceneManager.LoadScene(titleSceneName);
            }
        }
    }

    // ★ 所有输入都经过这里转发
    private void OnAnyStringPlayed(int stringId)
    {
        Debug.Log($"🎸 GuitarInputManager 收到弦 {stringId}");
        
        string currentScene = SceneManager.GetActiveScene().name;

        // ★ EndingScene: 按 1 跳到 Transition
        if (currentScene == endingSceneName && stringId == 1)
        {
            Debug.Log($"⏭️ 吉他 EndingScene: 按 1 跳到 Transition1：{currentScene} → {transitionSceneName}");
            SceneManager.LoadScene(transitionSceneName);
            return;
        }

        // ★ TransitionScene: 按 1 跳到 Title
        if (currentScene == transitionSceneName && stringId == 1)
        {
            Debug.Log($"⏭️ 吉他 1TransitionScene: 按 1 跳到 Title：{currentScene} → {titleSceneName}");
            SceneManager.LoadScene(titleSceneName);
            return;
        }

        // ★ 转发给其他订阅者（WindBladeShooter 等）
        OnStringPlayed?.Invoke(stringId);
    }

    void OnDestroy()
    {
        KeyboardGuitarSimulator.OnStringPlayedStatic -= OnAnyStringPlayed;
        GuitarBluetoothInput.OnStringPlayed -= OnAnyStringPlayed;
    }
}