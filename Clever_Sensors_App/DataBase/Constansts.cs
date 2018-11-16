using System;
using Android.Gms.Location;

namespace Clever_Sensors_App.Database
{
    public static class Constants
    {     
        public const int SERVICE_RUNNING_NOTIFICATION_ID = 10000;
        public const string SERVICE_STARTED_KEY = "has_service_been_started";
        public const string BROADCAST_MESSAGE_KEY = "broadcast_message";
        public const string ACTION_START_SERVICE = "SensorApp.action.START_SERVICE";
        public const string ACTION_STOP_SERVICE = "SensorApp.action.STOP_SERVICE";
        public const string ACTION_MAIN_ACTIVITY = "SensorApp.action.MAIN_ACTIVITY";

        public const string KEY_CURRENT_THEME = "current_theme";
        public const string LILAC_THEME = "lilac";
        public const string MINT_THEME = "mint";

        internal static readonly int[] MonitoredActivities = {
            DetectedActivity.Still,
            DetectedActivity.OnFoot,
            DetectedActivity.Walking,
            DetectedActivity.Running,
            DetectedActivity.OnBicycle,
            DetectedActivity.InVehicle,
            DetectedActivity.Tilting,
            DetectedActivity.Unknown
        };

        public static string ToReadableString(this TimeSpan span)
        {
            string formatted = string.Format("{0}{1}{2}{3}",
                span.Duration().Days > 0 ? string.Format("{0:0} day{1}, ", span.Days, span.Days == 1 ? string.Empty : "s") : string.Empty,
                span.Duration().Hours > 0 ? string.Format("{0:0} hour{1}, ", span.Hours, span.Hours == 1 ? string.Empty : "s") : string.Empty,
                span.Duration().Minutes > 0 ? string.Format("{0:0} min{1}, ", span.Minutes, span.Minutes == 1 ? string.Empty : "s") : string.Empty,
                span.Duration().Seconds > 0 ? string.Format("{0:0} sec{1}", span.Seconds, span.Seconds == 1 ? string.Empty : "s") : string.Empty);

            if (formatted.EndsWith(", ")) formatted = formatted.Substring(0, formatted.Length - 2);
            if (string.IsNullOrEmpty(formatted)) formatted = "0 sec";
            return formatted;
        }
    }
}