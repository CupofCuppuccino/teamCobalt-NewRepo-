using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
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


        StartCoroutine(RegisterInput());
    }


    IEnumerator RegisterInput()
    {
        yield return null;


        KeyboardGuitarSimulator.OnStringPlayedStatic
            += OnStringPlayed;


        GuitarBluetoothInput.OnStringPlayed
            += OnStringPlayed;


        Debug.Log(
            "GuitarInputManager 输入注册完成"
        );
    }

    void Update()
    {
        string currentScene =
            SceneManager.GetActiveScene().name;


        if (Keyboard.current != null)
        {
            if (Keyboard.current.digit1Key.wasPressedThisFrame)
                OnStringPlayed?.Invoke(1);


            if (Keyboard.current.digit2Key.wasPressedThisFrame)
                OnStringPlayed?.Invoke(2);


            if (Keyboard.current.digit3Key.wasPressedThisFrame)
                OnStringPlayed?.Invoke(3);


            if (Keyboard.current.digit4Key.wasPressedThisFrame
                && currentScene == mainSceneName)
            {
                SceneManager.LoadScene(endingSceneName);
            }
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