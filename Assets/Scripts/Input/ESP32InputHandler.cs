// Assets/Scripts/Input/ESP32InputHandler.cs
using UnityEngine;
using System.Collections;
using System.Text.RegularExpressions;

#if UNITY_ANDROID && !UNITY_EDITOR
using UnityEngine.Android;
#endif

public class ESP32InputHandler : MonoBehaviour
{
    [Header("蓝牙设置")]
    public string deviceName = "teamCobalt";  // ESP32 蓝牙名称
    public float reconnectInterval = 2f;      // 重连间隔
    
    [Header("调试")]
    public bool debugMode = true;
    public string lastReceivedData = "";
    
    // 弦状态
    private int string1State = 1;
    private int string2State = 1;
    private int string3State = 1;
    
    // 上一次状态（用于检测拨弦事件）
    private int lastString1State = 1;
    private int lastString2State = 1;
    private int lastString3State = 1;
    
    // 蓝牙连接状态
    private bool isConnected = false;
    private BluetoothConnector connector;
    
    // 弦拨动事件（供其他脚本订阅）
    public event System.Action<int> OnStringPlayed;  // 参数：弦编号 1, 2, 3
    
    void Start()
    {
        // 在 Android 上请求蓝牙权限
        #if UNITY_ANDROID && !UNITY_EDITOR
        if (!Permission.HasUserAuthorizedPermission(Permission.CoarseLocation))
        {
            Permission.RequestUserPermission(Permission.CoarseLocation);
        }
        #endif
        
        // 初始化蓝牙连接
        #if !UNITY_EDITOR
        ConnectToESP32();
        #else
        Debug.Log("🔵 [编辑器模式] 模拟 ESP32 输入，按键盘 1/2/3 测试");
        #endif
    }
    
    void ConnectToESP32()
    {
        // 使用 Unity 的 Bluetooth LE 或 Serial 连接
        // 这里用 Serial（需要导入 System.IO.Ports）
        try
        {
            // 注意：Unity 默认不支持 System.IO.Ports，需要添加
            // 在 Player Settings → Other Settings → .NET 4.x
            // 并在 Scripting Define Symbols 中添加: ENABLE_SERIAL
            
            #if ENABLE_SERIAL
            connector = new BluetoothConnector(deviceName);
            connector.OnDataReceived += OnDataReceived;
            connector.OnConnected += () => {
                isConnected = true;
                Debug.Log("✅ 蓝牙连接成功: " + deviceName);
            };
            connector.OnDisconnected += () => {
                isConnected = false;
                Debug.Log("❌ 蓝牙断开连接");
                // 尝试重连
                Invoke("ConnectToESP32", reconnectInterval);
            };
            connector.Connect();
            #else
            Debug.LogWarning("⚠️ 未启用 ENABLE_SERIAL，请添加 Scripting Define Symbol");
            #endif
        }
        catch (System.Exception e)
        {
            Debug.LogError("蓝牙连接失败: " + e.Message);
        }
    }
    
    /// <summary>
    /// 接收到蓝牙数据时调用
    /// </summary>
    void OnDataReceived(string data)
    {
        if (debugMode)
        {
            lastReceivedData = data;
        }
        
        // 解析数据格式: "1,1,0" 或 "0,1,1"
        string[] parts = data.Split(',');
        if (parts.Length == 3)
        {
            try
            {
                int s1 = int.Parse(parts[0].Trim());
                int s2 = int.Parse(parts[1].Trim());
                int s3 = int.Parse(parts[2].Trim());
                
                // 更新状态
                lastString1State = string1State;
                lastString2State = string2State;
                lastString3State = string3State;
                
                string1State = s1;
                string2State = s2;
                string3State = s3;
                
                // 检测拨弦事件（从 1 变为 0 = 拨弦）
                if (string1State == 0 && lastString1State == 1)
                {
                    Debug.Log("🎸 弦1 拨动");
                    OnStringPlayed?.Invoke(1);
                }
                if (string2State == 0 && lastString2State == 1)
                {
                    Debug.Log("🎸 弦2 拨动");
                    OnStringPlayed?.Invoke(2);
                }
                if (string3State == 0 && lastString3State == 1)
                {
                    Debug.Log("🎸 弦3 拨动");
                    OnStringPlayed?.Invoke(3);
                }
            }
            catch (System.Exception e)
            {
                Debug.LogWarning("解析数据失败: " + e.Message + " 数据: " + data);
            }
        }
    }
    
    void Update()
    {
        // ===============================
        // 编辑器模式：键盘模拟
        // ===============================
        #if UNITY_EDITOR
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            Debug.Log("🎸 [模拟] 弦1 拨动");
            OnStringPlayed?.Invoke(1);
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            Debug.Log("🎸 [模拟] 弦2 拨动");
            OnStringPlayed?.Invoke(2);
        }
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            Debug.Log("🎸 [模拟] 弦3 拨动");
            OnStringPlayed?.Invoke(3);
        }
        #endif
    }
    
    public bool IsConnected()
    {
        return isConnected;
    }
    
    public int GetStringState(int stringId)
    {
        switch (stringId)
        {
            case 1: return string1State;
            case 2: return string2State;
            case 3: return string3State;
            default: return 1;
        }
    }
    
    void OnDestroy()
    {
        if (connector != null)
        {
            connector.Disconnect();
        }
    }
}