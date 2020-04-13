using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Globalization;

public static class DateTimeExtensions
{
    public static DateTime RemoveMilliseconds(this DateTime dt)
    {
        return dt.AddMilliseconds(-dt.Millisecond);
    }

    public static DateTime RoundDownByMinute(this DateTime dt, int interval = 1)
    {
        if (interval < 1)
        {
            interval = 1;
        }
        var minute = dt.Minute - dt.Minute%interval;
        return new DateTime(dt.Year, dt.Month, dt.Day, dt.Hour, 0, 0).AddMinutes(minute);
    }

    public static DateTime RoundByMinute(this DateTime dt, int interval = 1)
    {
        if(interval < 1)
        {
            interval = 1;
        }
        var minute = dt.Minute + (interval - dt.Minute%interval);
        return new DateTime(dt.Year, dt.Month, dt.Day, dt.Hour, 0, 0).AddMinutes(minute);
    }

    public static DateTime ToPreviousDay(this DateTime dt, DayOfWeek dayOfWeek)
    {
        if (dt.DayOfWeek == dayOfWeek)
        {
            return dt;
        }
        while (true)
        {
            dt = dt.AddDays(-1);
            if (dt.DayOfWeek == dayOfWeek)
            {
                return dt;
            }
        }
    }

    public static DateTime ToNextDay(this DateTime dt, DayOfWeek dayOfWeek)
    {
        if(dt.DayOfWeek == dayOfWeek)
        {
            return dt;
        }
        while(true)
        {
            dt = dt.AddDays(1);
            if(dt.DayOfWeek == dayOfWeek)
            {
                return dt;
            }
        }
    }

    public static DateTime AddWeeks(this DateTime dt, double value)
    {
        return dt.AddDays(value*7);
    }

    public static int ToWeekNumber(this DateTime date, DayOfWeek startDayOfWeek = DayOfWeek.Monday)
    {
        return CultureInfo.CurrentUICulture.Calendar.GetWeekOfYear(date.Date, CalendarWeekRule.FirstFourDayWeek, startDayOfWeek);  
    }
}