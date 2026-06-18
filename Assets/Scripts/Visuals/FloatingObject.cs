using UnityEngine;

public class FloatingObject : MonoBehaviour
{
    public float floatSpeed = 0.5f;
    public float floatAmount = 0.2f;
    public float rotationSpeed = 10f;

    Vector3 startPos;
    float randomOffset;

    void Start()
    {
        startPos = transform.position;

        randomOffset =
        Random.Range(
        0f,
        Mathf.PI * 2f
        );
    }

    void Update()
    {
        float yOffset =
        Mathf.Sin(
        (Time.time + randomOffset)
        * floatSpeed
        ) * floatAmount;

        transform.position =
        startPos +
        Vector3.up * yOffset;

        transform.Rotate(
        Vector3.up *
        rotationSpeed *
        Time.deltaTime
        );
    }
}