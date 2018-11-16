using Android.App;
using Android.Content;
using Android.OS;
using Android.Hardware;
using Android.Runtime;
using Android.Support.V4.Content;
using static Android.OS.PowerManager;
using System;
using Clever_Sensors_App.Database;
using Android.Support.V4.App;
using System.Linq;
using Realms;
using Clever_Sensors_App.Activities;
using Android.Widget;
using System.Collections.Generic;
using Android.Gms.Location;
using Android.Gms.Tasks;
using Android.Util;

namespace Clever_Sensors_App.Services
{
    [Service]
    public class SensorTrackingService : Service, ISensorEventListener
    {
        static readonly string TAG = typeof(SensorTrackingService).FullName;
        public static Context context;
        SensorManager mSensorManager;
        Sensor mAccel, mMagn;
        WakeLock wklock;
        Realm mDatabase;
        MotionSensorData mSensorData;
        TrackingMetaData mMetaData;

        int data_idx;
        float[] accVector = new float[3];
        float[] orientVector = new float[3];
        
        bool acc_set = false;
        bool orient_set = false;        
        long lastTimestamp = 0;
        const long update_time = 70000000; //in ns --> 70ms
        const int min_tracking_time = 5; //in s 
        bool isRunning = false;
        private PendingIntent pendingIntent;
        GoogleActivityReceiver mGoogleActivityReceiver ;

        public override void OnCreate()
        {
            base.OnCreate();
            context = this;
            mSensorManager = (SensorManager)GetSystemService(SensorService);
            mAccel = mSensorManager.GetDefaultSensor(SensorType.Accelerometer);
            mMagn = mSensorManager.GetDefaultSensor(SensorType.RotationVector);
            mDatabase = Realm.GetInstance();
            var mPowermanager = (PowerManager)GetSystemService(PowerService);
            wklock = mPowermanager.NewWakeLock(WakeLockFlags.Partial, TAG );
        }

        private List<ActivityTransition> InitActivityTransitions()
        {
            List<ActivityTransition> transitions = new List<ActivityTransition>();
            transitions.Add(new ActivityTransition.Builder().SetActivityType(DetectedActivity.Walking)
                .SetActivityTransition(ActivityTransition.ActivityTransitionEnter).Build());

             transitions.Add(new ActivityTransition.Builder().SetActivityType(DetectedActivity.Walking)
                .SetActivityTransition(ActivityTransition.ActivityTransitionExit).Build());

            transitions.Add(new ActivityTransition.Builder().SetActivityType(DetectedActivity.Running)
                .SetActivityTransition(ActivityTransition.ActivityTransitionEnter).Build());

             transitions.Add(new ActivityTransition.Builder().SetActivityType(DetectedActivity.Running)
                .SetActivityTransition(ActivityTransition.ActivityTransitionExit).Build());

            transitions.Add(new ActivityTransition.Builder().SetActivityType(DetectedActivity.OnFoot)
                .SetActivityTransition(ActivityTransition.ActivityTransitionEnter).Build());

            transitions.Add(new ActivityTransition.Builder().SetActivityType(DetectedActivity.OnFoot)
               .SetActivityTransition(ActivityTransition.ActivityTransitionExit).Build());

            transitions.Add(new ActivityTransition.Builder().SetActivityType(DetectedActivity.Still)
                .SetActivityTransition(ActivityTransition.ActivityTransitionEnter).Build());

             transitions.Add(new ActivityTransition.Builder().SetActivityType(DetectedActivity.Still)
                .SetActivityTransition(ActivityTransition.ActivityTransitionExit).Build());

            return transitions;
        }

        public class OnFailureListener : Java.Lang.Object, IOnFailureListener
        {
            public void OnFailure(Java.Lang.Exception e)
            {
                // error handling
                Log.Debug("SensorTrackingService:", " Received OnFailureListener");
            }
        }

        public class OnSuccessListener : Java.Lang.Object, IOnSuccessListener
        {
            public void OnSuccess(Java.Lang.Object result)
            {
                Log.Debug("SensorTrackingService:", " Received OnSuccess");
                //Toast.MakeText(context, "OnSuccessListener receiced OnSuccess", ToastLength.Short).Show();
            }
        }

        public override StartCommandResult OnStartCommand(Intent intent, StartCommandFlags flags, int startId)
        {
            base.OnStartCommand(intent, flags, startId);

            if (intent.Action.Equals(Constants.ACTION_START_SERVICE))
            {
                RegisterForegroundService();               
                data_idx = 1;
                isRunning = true;
                var startTicks = (int)(DateTime.Now.Ticks >> 23); // retain bits 23 to 55
                var mStartDate = DateTimeOffset.Now;
                mMetaData = new TrackingMetaData
                {
                    StartTicks = startTicks,
                    StartDate = mStartDate,
                };
                mSensorData = new MotionSensorData
                {
                    StartTicks = startTicks,
                    StartDate = mStartDate
                };                
                mSensorManager?.RegisterListener(this, mAccel, SensorDelay.Ui);
                mSensorManager?.RegisterListener(this, mMagn, SensorDelay.Ui);              
                wklock.Acquire();

                List<ActivityTransition> transitions = InitActivityTransitions();
                ActivityTransitionRequest request = new ActivityTransitionRequest(transitions);

                pendingIntent = PendingIntent.GetBroadcast(context, 0, new Intent("GSM_TRANSITIONS_RECEIVER_ACTION"), PendingIntentFlags.UpdateCurrent);

                mGoogleActivityReceiver = new GoogleActivityReceiver();
                LocalBroadcastManager.GetInstance(context).RegisterReceiver(mGoogleActivityReceiver, new IntentFilter("GSM_TRANSITIONS_RECEIVER_ACTION"));

                var task = ActivityRecognition.GetClient(context).RequestActivityTransitionUpdates(request, pendingIntent);
                task.AddOnSuccessListener(new OnSuccessListener());
                task.AddOnFailureListener(new OnFailureListener());

            }
            else if (intent.Action.Equals(Constants.ACTION_STOP_SERVICE))
            {
                StopForeground(true);               
                StopSelf();              
            }
            return StartCommandResult.Sticky;
        }

        public async override void OnDestroy()
        {
            isRunning = false;
            ActivityRecognition.GetClient(this).RemoveActivityTransitionUpdates(pendingIntent);

            
            if (mGoogleActivityReceiver!=null)
            {
                LocalBroadcastManager.GetInstance(this).UnregisterReceiver(mGoogleActivityReceiver);
                mGoogleActivityReceiver = null;                
            }

            Bundle b = new Bundle();
            var duration1 = DateTimeOffset.Now.Subtract(mMetaData.StartDate).TotalSeconds;
            if (duration1> min_tracking_time)
            {
                mMetaData.Duration = duration1;
                await mDatabase.WriteAsync(tempRealm =>
                {
                    tempRealm.Add(mSensorData);
                    tempRealm.Add(mMetaData);
                });
                b.PutBoolean("SavingSuccessful", true);
            }
            else
            {
                b.PutBoolean("SavingSuccessful", false);
                //using (var h = new Handler(Looper.MainLooper)) h.Post(() => 
                //{
                //    Toast.MakeText(this.ApplicationContext, "Activity was too short to be saved.", ToastLength.Short).Show();
                //});
            }
            Intent onStop_intent = new Intent("StoppingSignal");
            onStop_intent.PutExtra("report", b);
            LocalBroadcastManager.GetInstance(context).SendBroadcast(onStop_intent);
            mSensorManager.UnregisterListener(this);
            base.OnDestroy();
        }
        
        void RegisterForegroundService()
        {
            NotificationCompat.Builder builder = GetNotificationBuilder( "SensorApp.notification.CHANNEL_ID_FOREGROUND");
            builder.SetContentTitle(Resources.GetString(Resource.String.app_name));
            builder.SetContentText(Resources.GetString(Resource.String.notification_text));
            builder.SetSmallIcon(Resource.Drawable.ic_run);
            builder.SetContentIntent(BuildIntentToShowTrackingActivity());
            builder.SetOngoing(true);
            builder.AddAction(BuildStopServiceAction());            
            StartForeground(Constants.SERVICE_RUNNING_NOTIFICATION_ID, builder.Build());
        }

        private NotificationCompat.Builder GetNotificationBuilder(string channelId )
        {
            NotificationCompat.Builder builder;
            if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
            {
                if (GetSystemService(NotificationService) is NotificationManager notificationManager)
                {
                    NotificationChannel nChannel = new NotificationChannel(channelId, Resources.GetString(Resource.String.app_name), NotificationImportance.High);

                    nChannel.Description = "SensorTracking";
                    nChannel.EnableVibration(false);
                    notificationManager.CreateNotificationChannel(nChannel);
                }
                builder = new NotificationCompat.Builder(context, channelId);
            }
            else
            {
                builder = new NotificationCompat.Builder(context);
            }
            return builder;
        }

        PendingIntent BuildIntentToShowTrackingActivity()
        {
            var notificationIntent = new Intent(this, typeof(TrackingActivity));
            notificationIntent.SetAction(Constants.ACTION_MAIN_ACTIVITY);
            notificationIntent.PutExtra(Constants.SERVICE_STARTED_KEY, true);
            return PendingIntent.GetActivity(this, 0, notificationIntent, PendingIntentFlags.UpdateCurrent);
        }

        NotificationCompat.Action BuildStopServiceAction()
        {
            var stopServiceIntent = new Intent(this, GetType());
            stopServiceIntent.SetAction(Constants.ACTION_STOP_SERVICE);
            var stopServicePendingIntent = PendingIntent.GetService(this, 0, stopServiceIntent, 0);
            return new NotificationCompat.Action.Builder(Resource.Drawable.ic_stop,
                                                          GetText(Resource.String.stop_tracking),
                                                          stopServicePendingIntent).Build();
        }

       public void OnSensorChanged(SensorEvent e)
        {
            switch (e.Sensor.Type)
            {
                case SensorType.Accelerometer:
                   accVector =  e.Values.ToArray();
                    acc_set = true;
                    break;
                case SensorType.RotationVector:
                    orientVector = e.Values.ToArray();
                    orient_set = true;
                    break;
                default:
                    break;
            }            
            if (orient_set && acc_set)
            {
                long aa = (e.Timestamp - lastTimestamp);
                if (  aa > update_time)
                {
                    lastTimestamp = e.Timestamp;
                    //  Broadcasting data to activity
                    Intent intent = new Intent("SensorBroadcast");
                    Bundle b = new Bundle();
                    b.PutFloatArray("accVector", accVector);
                    b.PutFloatArray("orientVector", orientVector);
                    b.PutInt("broadcast_idx", data_idx);
                    b.PutLong("lastTimestamp", lastTimestamp);
                    intent.PutExtra("sensor_data", b);
                    LocalBroadcastManager.GetInstance(context).SendBroadcast(intent);

                   if (isRunning)
                    { 
                        // save data to memory
                        mSensorData.TimestampList.Add(e.Timestamp);
                        mSensorData.AccXList.Add(accVector[0]);
                        mSensorData.AccYList.Add(accVector[1]);
                        mSensorData.AccZList.Add(accVector[2]);
                        mSensorData.OrientXList.Add(orientVector[0]);
                        mSensorData.OrientYList.Add(orientVector[1]);
                        mSensorData.OrientZList.Add(orientVector[2]);
                    }
                    data_idx += 1;
                    orient_set = false;
                    acc_set = false;                  
                }
            }
        }

        public void OnAccuracyChanged(Sensor sensor, [GeneratedEnum] SensorStatus accuracy)
        {
        }

        public override IBinder OnBind(Intent intent)
        {
            return null;
        }
    }

    [BroadcastReceiver(Enabled = true )]
    class GoogleActivityReceiver : BroadcastReceiver
    {
        public override void OnReceive(Context context, Intent intent)
        {
            // bug  this is never called????? 
            // bug in google play location service activity recognition ?
            // https://forums.xamarin.com/discussion/140484/xamarin-android-google-activity-recognition-transition-api-not-working
            Toast.MakeText(context, "OnReceive is GoogleActivityReceiver called ", ToastLength.Long).Show();

            if (ActivityTransitionResult.HasResult(intent))
            {
                ActivityTransitionResult result = ActivityTransitionResult.ExtractResult(intent);
                
                Log.Debug("GoogleActivityReceiver", " Received :ActivityTransitionResult");

                //for (ActivityTransitionEvent event : result.getTransitionEvents()) {
                //    String activity = toActivityString(event.getActivityType());
                //    String transitionType = toTransitionType(event.getTransitionType());
                //    mLogFragment.getLogView()
                //            .println("Transition: "
                //                    + activity + " (" + transitionType + ")" + "   "
                //                    + new SimpleDateFormat("HH:mm:ss", Locale.US)
                //                    .format(new Date()));
                //}
            }
        }            
    }
}