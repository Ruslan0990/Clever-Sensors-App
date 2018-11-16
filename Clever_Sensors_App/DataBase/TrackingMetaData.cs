using System;
using Realms;

namespace Clever_Sensors_App.Database
{
    public class TrackingMetaData : RealmObject
    {

        [PrimaryKey]
        public int StartTicks { get; set; }

        public double Duration { get; set; }

        public DateTimeOffset StartDate { get; set; }

    }
}