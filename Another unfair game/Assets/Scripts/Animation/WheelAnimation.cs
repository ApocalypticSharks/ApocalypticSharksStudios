// 22.12.2025 AI-Tag
// This was created with the help of Assistant, a Unity Artificial Intelligence product.

using System;
using UnityEditor;
using UnityEngine;
using System.Collections;

public class WheelAnimation : MonoBehaviour
{
    [SerializeField]
    private RectTransform wheelTransform; // Assign the Wheel RectTransform here
    [SerializeField]
    private float rotationSpeed = 500f; // Adjust the speed of rotation
    [SerializeField]
    private float rotationDuration = 500f; // Adjust the duration of rotation
    [SerializeField]
    private int segmentCount; // Total number of segments on the wheel

    private float segmentAngle; // Angle per segment

    private void Start()
    {
        segmentAngle = 360f / segmentCount; // Calculate the angle for each segment
    }

    public void RotateToSelectedSegment(int selectedIndex)
    {
        float targetAngle = -selectedIndex * segmentAngle; // Calculate the target angle
        StartCoroutine(SpinToTarget(targetAngle));
    }

    private IEnumerator SpinToTarget(float targetAngle)
    {
        float currentAngle = wheelTransform.eulerAngles.z;
        float totalRotation = Mathf.DeltaAngle(currentAngle, targetAngle);

        while (rotationDuration > 0.1f)
        {
            float step = Mathf.Sign(totalRotation) * rotationSpeed * Time.deltaTime;
            rotationSpeed -= Time.deltaTime * 50;
            wheelTransform.Rotate(0, 0, step);
            rotationDuration -= Time.deltaTime;
            yield return null;
        }

        while (Mathf.Abs(totalRotation) > 0.1f)
        {
            float step = Mathf.Sign(totalRotation) * rotationSpeed * Time.deltaTime;
            if (Mathf.Abs(step) > Mathf.Abs(totalRotation))
            {
                step = totalRotation; // Ensure we don't overshoot
            }
            if (rotationSpeed > 20)
                rotationSpeed -= Time.deltaTime * 50;

            wheelTransform.Rotate(0, 0, step);
            totalRotation -= step;

            yield return null;
        }

        wheelTransform.eulerAngles = new Vector3(0, 0, targetAngle); // Snap to the exact angle
    }
}
