using TMPro;
using UnityEngine;

public class TimelineScript : MonoBehaviour
{
    [Header("Time Settings")]
    [SerializeField] private float secondsPerMinute = 3f;

    private float time;

    [Header("Time Info")]
    private int minutes;
    private int hours;
    private int days = 1;
    private int years = 2000;

    [Header("Months")]
    private string currentMonth;
    private int monthCounter = 1;

    private string[] months =
    {
        "January", "February", "March",
        "April", "May", "June",
        "July", "August", "September",
        "October", "November", "December"
    };

    [Header("UI")]
    [SerializeField] private TextMeshProUGUI timelineText;

    private void Start()
    {
        currentMonth = months[monthCounter - 1];
        UpdateTimelineUI();
    }

    private void Update()
    {
        time += Time.deltaTime;

        while (time >= secondsPerMinute)
        {
            time -= secondsPerMinute;
            CountTimeUp();
        }
    }

    private void CountTimeUp()
    {
        minutes++;

        if (minutes >= 60)
        {
            minutes = 0;
            hours++;
        }

        if (hours >= 24)
        {
            hours = 0;
            days++;
            DayCalculator();
        }

        UpdateTimelineUI();
    }

    private void DayCalculator()
    {
        int daysInMonth = 31;

        if (monthCounter == 4 || monthCounter == 6||
            monthCounter == 9 || monthCounter == 11)
        {
            daysInMonth = 30;
        }
        else if (monthCounter == 2)
        {
            daysInMonth = 28;

            if (years % 4 == 0)
            {
                daysInMonth = 29;
            }
        }

        if (days > daysInMonth)
        {
            days = 1;
            MonthCalculator();
        }
    }
    private void MonthCalculator()
    {
        monthCounter++;

        if (monthCounter > 12)
        {
            monthCounter = 1;
            years++;
        }

        currentMonth = months[monthCounter - 1];
    }

    private void UpdateTimelineUI()
    {
        timelineText.text =
            hours.ToString("00") + ":" +
            minutes.ToString("00") + " | " +
            days.ToString("00") + " " +
            currentMonth + " " +
            years.ToString();
    }
}