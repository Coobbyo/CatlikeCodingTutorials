using System;
using UnityEngine;

public class RealTimeClock : MonoBehaviour
{
    private const float hoursToDegrees = 30f, minutesToDegrees = 6f, secondsToDegrees = 6f;

    [SerializeField] private bool continuousRotation; //Toggle between discrete and continuous rotation
	[SerializeField] private Transform hourPivot, minutePivot, secondsPivot;

	private void Update()
    {
        if(continuousRotation)
        {
            TimeSpan time = DateTime.Now.TimeOfDay;
            hourPivot.localRotation =
                Quaternion.Euler(0f, 0f, hoursToDegrees * (float)time.TotalHours);
            minutePivot.localRotation =
                Quaternion.Euler(0f, 0f, minutesToDegrees * (float)time.TotalMinutes);
            secondsPivot.localRotation =
                Quaternion.Euler(0f, 0f, secondsToDegrees * (float)time.TotalSeconds);
        }
        else
        {
            DateTime time = DateTime.Now;
            hourPivot.localRotation =
                Quaternion.Euler(0f, 0f, hoursToDegrees * time.Hour);
            minutePivot.localRotation =
                Quaternion.Euler(0f, 0f, minutesToDegrees * time.Minute);
            secondsPivot.localRotation =
                Quaternion.Euler(0f, 0f, secondsToDegrees * time.Second);
        }
		
    }
}
