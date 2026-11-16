using System;
using TMPro;
using UnityEngine;

public class TimelineScript : MonoBehaviour
{
    private float time; // telt de tijd op // 3 seconde = 1 minuut
    [SerializeField] private TextMeshProUGUI timelineText;

    [Header("Tijd info")]
    private float minutes;
    private float hours;
    private float days = 1;
    private float years = 2000;

    [Header("Maanden")]
    private string currentMonth = monthName.January.ToString();
    private int monthCounter = 1;
    enum monthName
    {
        January,
        February,
        March,
        April,
        May,
        June,
        July,
        August,
        September,
        October,
        November,
        December
    };

    private void Update()
    {
        time += Time.deltaTime;
        CountTimeUp();
    }

    private void CountTimeUp()
    {
        if (time >= 1)
        {
            minutes++;
            time = 0;
        }

        if (minutes >= 60) // 60
        {
            hours++;
            minutes = 0;
        }

        if (hours >= 24) // 24
        {
            days++;
            hours = 0;
        }
        
        if(days >= 28) // 28
        {
            MonthCalculator();

        }

        UpdateText();
    }
    
    private void UpdateText()
    {
        timelineText.text =
            hours.ToString() + ":0" + minutes.ToString() + "\n" + // uur & minuut
            days.ToString() + " " + currentMonth + " " + years.ToString(); // dag , maand, jaar
        if (minutes >= 10)
        {
            timelineText.text =
            hours.ToString() + ":" + minutes.ToString() + "\n" + // uur & minuut
            days.ToString() + " " + currentMonth + " " + years.ToString(); // dag , maand, jaar
        }

        
    }

    private void MonthCalculator()
    {
        if(days == 31)
        {
            Month31DaysCalc();
            days = 1;
        } else if(days == 30)
        {
            Month30DaysCalc();
            days= 1;
        } else if (days == 28)
        {
            if (monthCounter == 2)
                currentMonth = monthName.February.ToString();
                days = 1;
        }
    }
    
    private void Month30DaysCalc()
    {
        if (monthCounter == 13)
        {
            monthCounter = 1;
        } else if (monthCounter == 11)
        {
            currentMonth = monthName.November.ToString();
        } else if(monthCounter == 9){
            currentMonth = monthName.September.ToString();
        }
        else if (monthCounter == 6)
        {
            currentMonth = monthName.June.ToString();
        }
        else if (monthCounter == 4)
        {
            currentMonth = monthName.April.ToString();
        }
        monthCounter++;  
    }

    private void Month31DaysCalc()
    {
        if (monthCounter == 12)
        {
            currentMonth = monthName.December.ToString();
        }
        else if (monthCounter == 10)
        {
            currentMonth = monthName.October.ToString();
        }
        else if (monthCounter == 8)
        {
            currentMonth = monthName.August.ToString();
        }
        else if (monthCounter == 7)
        {
            currentMonth = monthName.July.ToString();
        }
        else if (monthCounter == 5)
        {
            currentMonth = monthName.May.ToString();
        }
        else if (monthCounter == 3)
        {
            currentMonth = monthName.March.ToString();
        }
        else if (monthCounter == 1)
        {
            currentMonth = monthName.January.ToString();
        }

        monthCounter++;
    }



}
