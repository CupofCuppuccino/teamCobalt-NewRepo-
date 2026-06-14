using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ForeArmRotation : MonoBehaviour
{
    public Transform Hand;
    public bool Invert = false;
    private int Orientation = 1;

    private void Start()
    {
        if (Invert) Orientation = -1;
    }
    void Update()
    {
        gameObject.transform.localEulerAngles = new Vector3(gameObject.transform.localEulerAngles.x,
                                                        Orientation * Hand.transform.localEulerAngles.z,
                                                        gameObject.transform.localEulerAngles.z);
    }
}
