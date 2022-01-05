using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SMSApi.Models
{
    public class AttendanceSettings
    {
        public static string TimeIn = "09:00";
        public static string SUCCESS = "SUCCESS";
        public static string ERROR = "ERROR";
        public static string TIME_IN = "TIME_IN";
        public static string TIME_OUT = "TIME_OUT";
        public static bool TodaysAttendance = false;
        public static DateTime TodaysDate = DateTime.Now;
    }
}