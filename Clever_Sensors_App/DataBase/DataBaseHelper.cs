using Realms;
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

        public List<MotionSensorData> GetMetaDataItems()
        {
            return mDataBase.All<MotionSensorData>().OrderByDescending(p => p.StartTicks).ToList();
        }

        public MotionSensorData GetMotionDataItem(int ID )
        {
            return mDataBase.Find<MotionSensorData>(ID); 
        }

        public void DeleteFromDatabase(int my_id)
        {
            var denom1 = mDataBase.Find<MotionSensorData>(my_id);
            if (denom1 == null)  // If no entry with this id exists, do nothing.
                return;

            using (var transaction = mDataBase.BeginWrite())
            {
                mDataBase.Remove(denom1);
                transaction.Commit();
            }
        }
    }
}