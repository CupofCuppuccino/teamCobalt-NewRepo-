// Assets/Scripts/Input/BluetoothConnector.cs
using UnityEngine;
using System;
using System.IO.Ports;
using System.Threading;
using System.Text;

public class BluetoothConnector
{
    private SerialPort serialPort;
    private Thread readThread;
    private bool isRunning = false;
    private string deviceName;
    private string portName;
    
    public event Action<string> OnDataReceived;
    public event Action OnConnected;
    public event Action OnDisconnected;
    
    public BluetoothConnector(string name)
    {
        deviceName = name;
        FindPort();
    }
    
    void FindPort()
    {
        // 获取所有串口
        string[] ports = SerialPort.GetPortNames();
        Debug.Log("可用串口: " + string.Join(", ", ports));
        
        // 尝试找到蓝牙串口（通常是 COM 开头）
        foreach (string port in ports)
        {
            try
            {
                SerialPort testPort = new SerialPort(port, 9600);
                testPort.Open();
                testPort.Write("AT+NAME?\r\n");
                System.Threading.Thread.Sleep(500);
                string response = testPort.ReadExisting();
                testPort.Close();
                
                if (response.Contains(deviceName))
                {
                    portName = port;
                    Debug.Log($"找到蓝牙设备 {deviceName} 在端口 {port}");
                    return;
                }
            }
            catch { /* 忽略无法访问的端口 */ }
        }
        
        // 如果没找到，使用第一个可用端口
        if (ports.Length > 0)
        {
            portName = ports[0];
            Debug.LogWarning($"未找到 {deviceName}，使用默认端口 {portName}");
        }
    }
    
    public void Connect()
    {
        if (string.IsNullOrEmpty(portName))
        {
            Debug.LogError("没有可用的蓝牙端口");
            return;
        }
        
        try
        {
            serialPort = new SerialPort(portName, 9600, Parity.None, 8, StopBits.One);
            serialPort.ReadTimeout = 100;
            serialPort.WriteTimeout = 100;
            serialPort.Open();
            
            if (serialPort.IsOpen)
            {
                isRunning = true;
                OnConnected?.Invoke();
                
                // 启动读取线程
                readThread = new Thread(ReadData);
                readThread.Start();
                Debug.Log($"✅ 蓝牙已连接: {portName}");
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"蓝牙连接失败: {e.Message}");
        }
    }
    
    void ReadData()
    {
        while (isRunning && serialPort != null && serialPort.IsOpen)
        {
            try
            {
                string data = serialPort.ReadLine();
                if (!string.IsNullOrEmpty(data))
                {
                    OnDataReceived?.Invoke(data.Trim());
                }
            }
            catch (TimeoutException)
            {
                // 读取超时，继续循环
            }
            catch (Exception e)
            {
                Debug.LogWarning("读取数据异常: " + e.Message);
                isRunning = false;
                OnDisconnected?.Invoke();
            }
        }
    }
    
    public void SendData(string data)
    {
        if (serialPort != null && serialPort.IsOpen)
        {
            try
            {
                serialPort.WriteLine(data);
            }
            catch (Exception e)
            {
                Debug.LogWarning("发送数据失败: " + e.Message);
            }
        }
    }
    
    public void Disconnect()
    {
        isRunning = false;
        if (readThread != null && readThread.IsAlive)
        {
            readThread.Join(1000);
        }
        
        if (serialPort != null && serialPort.IsOpen)
        {
            serialPort.Close();
        }
        
        OnDisconnected?.Invoke();
        Debug.Log("🔴 蓝牙已断开");
    }
}