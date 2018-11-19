using System;
using System.Collections.Generic;
using Realms;

namespace Clever_Sensors_App.Database
{
    public class MotionSensorData : RealmObject
    {
        [PrimaryKey]
        public int StartTicks { get; set; }

        public double Duration { get; set; }

        public DateTimeOffset StartDate { get; set; }

        public IList<long> TimestampList { get;  }

        public IList<float> AccXList { get;  }
        public IList<float> AccYList { get;  }
        public IList<float> AccZList { get;  }

        public IList<float> OrientXList { get;  }
        public IList<float> OrientYList { get;  }
        public IList<float> OrientZList { get;  }
    }
}