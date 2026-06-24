using UnityEngine;
using System;
using System.IO.Ports;


public class GuitarBluetoothInput : MonoBehaviour
{

    public static GuitarBluetoothInput Instance;


    public static event Action<int> OnStringPlayed;



    public bool useUSBSerial = false;


    public string portName = "COM3";


    public int baudRate = 9600;



    SerialPort serialPort;


    int lastS1 = 1;
    int lastS2 = 1;
    int lastS3 = 1;



    void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }


        Instance = this;

        DontDestroyOnLoad(gameObject);


        Debug.Log(
        "GuitarBluetoothInput Awake");
    }



    void Start()
    {

        if (!useUSBSerial)
        {
            Debug.Log(
            "Using Android Bluetooth");
            return;
        }



        try
        {
            serialPort =
            new SerialPort(
            portName,
            baudRate);


            serialPort.ReadTimeout = 50;


            serialPort.Open();


            Debug.Log(
            "Serial Connected");

        }
        catch (Exception e)
        {
            Debug.LogError(
            e.Message);
        }

    }





    void Update()
    {

        if (serialPort == null ||
           !serialPort.IsOpen)
            return;



        try
        {

            string msg =
            serialPort.ReadLine();


            ParseMessage(msg.Trim());

        }
        catch { }

    }






    public void ParseMessage(string msg)
    {

        Debug.Log(
        "Guitar Receive:" + msg);



        string[] p =
        msg.Split(',');



        if (p.Length != 3)
            return;



        int s1 = int.Parse(p[0]);
        int s2 = int.Parse(p[1]);
        int s3 = int.Parse(p[2]);



        if (lastS1 == 1 && s1 == 0)
        {
            Debug.Log("STRING1");
            OnStringPlayed?.Invoke(1);
        }


        if (lastS2 == 1 && s2 == 0)
        {
            Debug.Log("STRING2");
            OnStringPlayed?.Invoke(2);
        }


        if (lastS3 == 1 && s3 == 0)
        {
            Debug.Log("STRING3");
            OnStringPlayed?.Invoke(3);
        }



        lastS1 = s1;
        lastS2 = s2;
        lastS3 = s3;

    }

}