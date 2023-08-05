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

    public IEnumerator IncrementClock(int hours)
    {
        float startHourRotation = hourArrow.eulerAngles.z;  // Initial hour rotation
        float endHourRotation = startHourRotation - (30f*hours);  // Final hour rotation

        float startMinuteRotation = minuteArrow.eulerAngles.z;  // Initial minute rotation
        float endMinuteRotation = startMinuteRotation - 360;  // Final minute rotation

        float elapsed = 0.0f;  // Time elapsed since the start of rotation

        while (elapsed < 1.5f)
        {
            // Calculate new rotations.
            float newHourRotation = Mathf.Lerp(startHourRotation, endHourRotation, elapsed / 1.5f);
            float newMinuteRotation = Mathf.Lerp(startMinuteRotation, endMinuteRotation, elapsed / 1.5f);

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
