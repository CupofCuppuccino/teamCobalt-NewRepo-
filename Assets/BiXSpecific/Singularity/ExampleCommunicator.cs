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

        var devices =
            mySingularityManager.GetPairedDevices();


        foreach (var device in devices)
        {
            Debug.Log(
                "Found device: " + device.name
            );


            if (device.name == DeviceName)
            {
                myDevice = device;

                mySingularityManager
                    .ConnectToDevice(myDevice);

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

        message = message.Trim();


        Debug.Log(
            "ESP32 => " + message
        );


        if (message.Contains(","))
        {
            if (GuitarBluetoothInput.Instance != null)
            {
                GuitarBluetoothInput.Instance
                    .ParseMessage(message);
            }
        }

    }


    public void onError(string error)
    {
        Debug.LogError(error);
    }
}