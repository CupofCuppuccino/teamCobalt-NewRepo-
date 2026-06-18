using System;
using UnityEngine;

public class GuitarInputManager : MonoBehaviour
{
    // 游戏逻辑订阅这个事件
    public static event Action<int> OnStringPlayed;


    void OnEnable()
    {
        // 键盘模拟输入
        KeyboardGuitarSimulator.OnStringPlayedStatic += HandleInput;

        // ESP32硬件输入
        GuitarBluetoothInput.OnStringPlayed += HandleInput;
    }



    void OnDisable()
    {
        KeyboardGuitarSimulator.OnStringPlayedStatic -= HandleInput;

        GuitarBluetoothInput.OnStringPlayed -= HandleInput;
    }



    private void HandleInput(int stringId)
    {
        Debug.Log(
            $"GuitarInputManager received string {stringId}"
        );

        OnStringPlayed?.Invoke(stringId);
    }
}