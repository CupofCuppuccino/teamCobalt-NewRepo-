using Sngty;
using UnityEngine;
using UnityEngine.Events;


public class ExampleCommunicator : MonoBehaviour
{

    public SingularityManager mySingularityManager;

    public string DeviceName;


    private DeviceSignature myDevice;


    void Start()
    {
        Debug.Log("ExampleCommunicator Start");

        if (mySingularityManager == null)
        {
            Debug.LogWarning("⚠️ mySingularityManager 为空，跳过蓝牙初始化");
            return;
        }

        var devices =
        mySingularityManager.GetPairedDevices();


        Debug.Log(
        "Paired devices:"
        + devices.Count
        );


        foreach (var device in devices)
        {
            Debug.Log(
            "DEVICE:"
            + device.name
            );


            if (device.name == DeviceName)
            {
                myDevice = device;


                mySingularityManager
                .ConnectToDevice(myDevice);


                Debug.Log(
                "Trying connect ESP32"
                );


                return;
            }
        }


        Debug.LogError(
        "ESP32 not found"
        );
    }



    public void onConnected()
    {
        Debug.Log("ESP32 connected");
    }



    public void onMessageRecieved(string message)
    {
        Debug.Log(
        "ESP32 RAW:"
        + message
        );


        message = message.Trim();


        if (message.Contains(","))
        {
            GuitarBluetoothInput.Instance
            ?.ParseMessage(message);
        }
    }


    public void onError(string error)
    {
        Debug.LogError(error);
    }
}