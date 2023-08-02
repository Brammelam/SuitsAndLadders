using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Clock : MonoBehaviour
{

    public Transform hourArrow, minuteArrow;
    public SpriteRenderer clockFace;
    public Color green, red;

    public void UpdateClockColor(int ahead)
    {
        if (ahead > 0) clockFace.color = red; else if (ahead < 0) clockFace.color = green; else clockFace.color = Color.white;
    }

    public void IncrementClock(int hours)
    {
        // Call coroutine to start the rotation.
        StartCoroutine(SmoothRotate(30f * hours, 360f * hours, 0.5f));  // Here we're rotating 30 degrees over 0.5 seconds. Adjust the values as necessary.
    }

    private IEnumerator SmoothRotate(float hourAngle, float minuteAngle, float duration)
    {
        float startHourRotation = hourArrow.eulerAngles.z;  // Initial hour rotation
        float endHourRotation = startHourRotation - hourAngle;  // Final hour rotation

        float startMinuteRotation = minuteArrow.eulerAngles.z;  // Initial minute rotation
        float endMinuteRotation = startMinuteRotation - minuteAngle;  // Final minute rotation

        float elapsed = 0.0f;  // Time elapsed since the start of rotation

        while (elapsed < duration)
        {
            // Calculate new rotations.
            float newHourRotation = Mathf.Lerp(startHourRotation, endHourRotation, elapsed / duration);
            float newMinuteRotation = Mathf.Lerp(startMinuteRotation, endMinuteRotation, elapsed / duration);

            // Update arrows' rotations.
            hourArrow.eulerAngles = new Vector3(0f, 0f, newHourRotation);
            minuteArrow.eulerAngles = new Vector3(0f, 0f, newMinuteRotation);

            // Update elapsed time.
            elapsed += Time.deltaTime;

            yield return null;
        }

        // Ensure the rotation is exactly the final rotation at the end.
        hourArrow.eulerAngles = new Vector3(0f, 0f, endHourRotation);
        minuteArrow.eulerAngles = new Vector3(0f, 0f, endMinuteRotation);
    }
}
