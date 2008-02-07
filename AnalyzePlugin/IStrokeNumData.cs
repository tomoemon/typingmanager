using System;
using System.Collections.Generic;
using System.Text;

namespace Plugin
{
    public interface IStrokeNumData
    {
        int TotalType { get;}
        int TotalApp { get;}
        int TotalDay { get;}
        int TodayTotalType { get;}
        int TodayTotalApp { get;}
        int YesterdayTotalType { get;}
        int YesterdayTotalApp { get;}

        int[] GetMinuteStroke(int start, int count);
        int[] GetHourStroke(int start, int count);
        int[] GetDayStroke(DateTime start, int count);
    }
}
