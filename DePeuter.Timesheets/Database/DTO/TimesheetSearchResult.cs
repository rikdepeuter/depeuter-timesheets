using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DePeuter.Timesheets.Constants;
using Xceed.Wpf.Toolkit.PropertyGrid.Attributes;

namespace DePeuter.Timesheets.Database.DTO
{
    public class TimesheetSearchResult : SearchResultBase
    {
        [SqlField("vchUsername")]
        public string Username { get; set; }
        [Obsolete]
        [SqlField("intJobId")]
        public int? JobId { get; set; }
        [SqlField("dteStartTime")]
        public DateTime StartTime { get; set; }
        [SqlField("dteEndTime")]
        public DateTime EndTime { get; set; }
        [SqlField("intType")]
        public int Type { get; set; }
        [SqlField("vchTaskNumber")]
        public string TaskNumber { get; set; }
        [SqlField("vchDescription")]
        public string Description { get; set; }

        [Obsolete]
        [SqlField("vchJobCode")]
        public string JobCode { get; set; }
        [Obsolete]
        [SqlField("vchClient")]
        public string Client { get; set; }
        [Obsolete]
        [SqlField("vchProject")]
        public string Project { get; set; }

        public double TotalMinutes
        {
            get
            {
                return (EndTime - StartTime).TotalMinutes;
            }
        }

        public int WeekNumber { get; private set; }
        //public string DateDisplay { get { return StartTime.ToString("dd/MM/yyyy ddd"); } }
        public string StartTimeDisplay { get { return StartTime.ToString("HH:mm"); } }
        public string EndTimeDisplay { get { return EndTime.ToString("HH:mm"); } }

        public TimesheetSearchResult CalculateWeekNumber()
        {
            if(WeekNumber == 0)
            {
                WeekNumber = StartTime.ToWeekNumber();
            }
            return this;
        }

        public string OverviewDescription
        {
            get
            {
                var sb = new StringBuilder();
                sb.AppendFormat("{0:HH:mm}", StartTime);
                if(!string.IsNullOrEmpty(TaskNumber))
                {
                    sb.AppendFormat(" [{0}]", TaskNumber);
                }
                if(!string.IsNullOrEmpty(Description))
                {
                    sb.AppendFormat(" {0}", Description);
                }
                return sb.ToString();
            }
        }

        public string ToolTip
        {
            get
            {
                return GetToolTip(Id, StartTime, EndTime, Type, TaskNumber, Description);
            }
        }

        public static string GetToolTip(int id, DateTime startTime, DateTime endTime, int type, string taskNumber, string description)
        {
            var duration = endTime - startTime;
            var tooltip = new StringBuilder();
            if(id <= 0)
            {
                tooltip.Append("<New item>");
            }
            else if(type == (int)TimesheetType.Break)
            {
                tooltip.Append("<Break>");
            }
            else
            {
                if(!string.IsNullOrEmpty(taskNumber))
                {
                    tooltip.AppendFormat("[{0}] ", taskNumber);
                }
                tooltip.Append(description);
            }
            tooltip.AppendLine();
            tooltip.AppendFormat("{0:HH:mm} - {1:HH:mm} ({2}:{3:00})", startTime, endTime, duration.Hours, duration.Minutes);
            return tooltip.ToString();
        }
    }
}
