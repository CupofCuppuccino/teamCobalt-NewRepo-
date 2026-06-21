using UnityEngine;

public class FloatingDebris : MonoBehaviour
{
    public float floatSpeed = 0.3f;
    public float floatAmount = 0.2f;
    public float rotationSpeed = 10f;
    public float driftSpeed = 0.05f;

    Vector3 startPosition;
    float randomOffset;
    Vector3 driftDirection;

    void Start()
    {
        startPosition = transform.position;

        randomOffset =
        Random.Range(
        0f,
        Mathf.PI * 2f
        );

        driftDirection =
        Random.onUnitSphere;

        driftDirection.y *= 0.3f;

        driftDirection =
        driftDirection.normalized;

        transform.rotation =
        Random.rotation;
    }

    void Update()
    {
        float yOffset =
        Mathf.Sin(
        (Time.time + randomOffset)
        * floatSpeed
        ) * floatAmount;

        startPosition +=
        driftDirection *
        driftSpeed *
        Time.deltaTime;

        transform.position =
        startPosition +
        Vector3.up * yOffset;

        transform.Rotate(
        Vector3.up,
        rotationSpeed *
        Time.deltaTime,
        Space.Self
        );

        transform.Rotate(
        Vector3.right,
        rotationSpeed *
        0.3f *
        Time.deltaTime,
        Space.Self
        );
    }
}