using Realms;
using Clever_Sensors_App.Database;
using System.Collections.Generic;
using System.Linq;

namespace Clever_Sensors_App.Database
{
    public class DataBaseHelper
    {
        private Realm mDataBase;

        public   DataBaseHelper()
        {
            mDataBase = Realm.GetInstance();
        }

        public List<TrackingMetaData> GetMetaDataItems()
        {
            return mDataBase.All<TrackingMetaData>().OrderByDescending(p => p.StartTicks).ToList();
        }

        public MotionSensorData GetMotionDataItem(int ID )
        {
            return mDataBase.Find<MotionSensorData>(ID); 
        }

        public void DeleteFromDatabase(int my_id)
        {
            var denom1 = mDataBase.Find<TrackingMetaData>(my_id);
            if (denom1 == null)  // If no entry with this id exists, do nothing.
                return;
            var denom2 = mDataBase.Find<MotionSensorData>(my_id);

            using (var transaction = mDataBase.BeginWrite())
            {
                mDataBase.Remove(denom1);
                mDataBase.Remove(denom2);
                transaction.Commit();
            }
        }
    }
}