using UnityEngine;
using TMPro;

public class GameClockController : MonoBehaviour
{
    [Header("ClockUI")]
    [SerializeField] private TMP_Text clockText;
    private float elapsedTime;
    [SerializeField] private float timeScale = 144f; //This approximatley makes 12 hours pass in 5 minutes
    [SerializeField] private float timeInADay = 86400f; //This is 24 hours in seconds
    [SerializeField] private float startingHour = 8f; //This is the hour of the day you start in, using numbers higher than 12 for PM

    [Header("Alarms")]
    [SerializeField] private float takeMeds = 12f; //The time (in hours) for when the "Take Meds" notice will appear
    [SerializeField] private float eatFood = 16f; //The time (in hours) for when the "Eat Food" notice will appear
    [SerializeField] private float goToSleep = 20f;  //The time (in hours) for when the "Go To Sleep" notice will appear



    private void Start()
    {
        StartDay();
    }

    private void Update()
    {
        elapsedTime += Time.deltaTime * timeScale;
        elapsedTime %= timeInADay;
        UpdateClockUI();
        CheckTime();
    }

    void UpdateClockUI()
    {
        int hour = Mathf.FloorToInt(elapsedTime / 3600f);
        int min = Mathf.FloorToInt((elapsedTime - hour * 3600f) / 60f);

        string ampm = hour < 12 ? "AM" : "PM";
        hour %= 12;
        if (hour == 0)
        {
            hour = 12;
        }

        string clockString = string.Format("{0:00}:{1:00} {2}", hour, min, ampm);
        clockText.text = clockString;
    }

    void StartDay()
    {
        elapsedTime = startingHour * 3600f;
    }

    void CheckTime()
    {
        if (elapsedTime > (takeMeds * 3600f))
        {

        }
        if (elapsedTime > (eatFood * 3600f))
        {

        }
        if (elapsedTime > (goToSleep * 3600f))
        {

        }
    }
}
