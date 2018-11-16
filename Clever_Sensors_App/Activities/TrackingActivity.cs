using Android.App;
using Android.OS;
using Android.Views;
using MikePhil.Charting.Charts;
using MikePhil.Charting.Data;
using MikePhil.Charting.Components;
using MikePhil.Charting.Interfaces.Datasets;
using Android.Graphics;
using System.Collections.Generic;
using Android.Content.PM;
using Android.Content;
using Android.Support.V4.Content;
using Clever_Sensors_App.Services;
using Clever_Sensors_App.Database;
using Android.Support.Graphics.Drawable;
using Android.Support.Design.Widget;

namespace Clever_Sensors_App.Activities
{
    [Activity(Label = "Tracking", LaunchMode = LaunchMode.SingleTask, ScreenOrientation = ScreenOrientation.Portrait)]
    public class TrackingActivity : BaseActivity
    {
        Intent startServiceIntent;
        Intent stopServiceIntent;
        public bool isStarted = false;
        public FloatingTextButton switchServiceButton;
        CoordinatorLayout mCoordinator;
        public LineChart mAccelChart, mOrientationChart;
        XYZDataReceiver mySensorDataReceiver;
        public VectorDrawableCompat startIcon, stopIcon;
        StopSignalReceiver myStopSignalReceiver;
        const int max_entries = 80;
        const int max_entries_saved = 100;

        private static TrackingActivity myInstance;
        public static TrackingActivity GetInstace()
        {
            return myInstance;
        }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Window.AddFlags(WindowManagerFlags.DrawsSystemBarBackgrounds);
            myInstance = this;
            SetContentView(Resource.Layout.TrackingActivityLayout);            
            OnNewIntent(Intent);
            if (savedInstanceState != null)
                isStarted = savedInstanceState.GetBoolean(Constants.SERVICE_STARTED_KEY, false);

            startServiceIntent = new Intent(this, typeof(SensorTrackingService));
            startServiceIntent.SetAction(Constants.ACTION_START_SERVICE);

            stopServiceIntent = new Intent(this, typeof(SensorTrackingService));
            stopServiceIntent.SetAction(Constants.ACTION_STOP_SERVICE);

            switchServiceButton = FindViewById<FloatingTextButton>(Resource.Id.start_service_button);
            switchServiceButton.Click += SwitchService_Click;

            mCoordinator = FindViewById<CoordinatorLayout>(Resource.Id.coordinator);

            //AnimatedVectorDrawableCompat aa= AnimatedVectorDrawableCompat.Create(this,Resource.Drawable; 
            startIcon = VectorDrawableCompat.Create(Resources, Resource.Drawable.ic_play, Theme);
            stopIcon = VectorDrawableCompat.Create(Resources, Resource.Drawable.ic_stop, Theme);

            if (isStarted)
            {
                switchServiceButton.Text = GetText(Resource.String.stop);
                switchServiceButton.LeftIconDrawable =(stopIcon);
            }
            else
            {
                switchServiceButton.LeftIconDrawable = (startIcon);
                switchServiceButton.Text = GetText(Resource.String.start);
            }

            Android.Support.V7.Widget.Toolbar toolbar = FindViewById<Android.Support.V7.Widget.Toolbar>(Resource.Id.toolbar);

            if (toolbar != null)
            {
                SetSupportActionBar(toolbar);
                SupportActionBar.SetDisplayShowHomeEnabled(true);
                SupportActionBar.SetDisplayHomeAsUpEnabled(true);
            }            
            mySensorDataReceiver = new XYZDataReceiver();
            myStopSignalReceiver = new StopSignalReceiver();
            LocalBroadcastManager.GetInstance(this).RegisterReceiver(mySensorDataReceiver, new IntentFilter("SensorBroadcast"));
            LocalBroadcastManager.GetInstance(this).RegisterReceiver(myStopSignalReceiver, new IntentFilter("StoppingSignal"));
        }

        protected override void OnStart()
        {
            base.OnStart();
            mAccelChart = FindViewById<LineChart>(Resource.Id.accelerometer_chart);
            mOrientationChart = FindViewById<LineChart>(Resource.Id.orientation_chart);
        }

        protected override void OnNewIntent(Intent intent)
        {
            if (intent == null)
            {
                return;
            }
            var bundle = intent.Extras;
            if (bundle != null)
            {
                if (bundle.ContainsKey(Constants.SERVICE_STARTED_KEY))
                {
                    isStarted = true;
                }
            } 
        }

        protected override void OnSaveInstanceState(Bundle outState)
        {
            outState.PutBoolean(Constants.SERVICE_STARTED_KEY, isStarted);
            base.OnSaveInstanceState(outState);
        }

        void SwitchService_Click(object sender, System.EventArgs e)
        {            
            if (!isStarted)
            {
                switchServiceButton.Clickable = false;
                PrepareLineChart(mAccelChart, 1);
                PrepareLineChart(mOrientationChart,2);
                PrepareLineChart(mOrientationChart,2);
                if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
                    StartForegroundService(startServiceIntent);
                else
                    StartService(startServiceIntent);
                isStarted = true;
                switchServiceButton.Text = GetText(Resource.String.stop);
                switchServiceButton.LeftIconDrawable = (stopIcon);
                switchServiceButton.Clickable = true;
            }
            else
            {
                switchServiceButton.LeftIconDrawable = (startIcon);
                switchServiceButton.Text = GetText(Resource.String.start);
                StopService(stopServiceIntent);
                isStarted = false;
            }
        }

        // Set apearance of MPandroidChart here
        private void PrepareLineChart(LineChart mChart, int chartIdx )
        {
            mChart.Clear();
            mChart.Description.Enabled = false;
            mChart.SetVisibleXRangeMaximum(max_entries);
            mChart.SetTouchEnabled(false);
            mChart.SetScaleEnabled(false);
            mChart.SetDrawGridBackground(false);
            mChart.SetDrawBorders(true);
            mChart.SetHardwareAccelerationEnabled(true);
            var legend = mChart.Legend;
            if (chartIdx == 1)
            {
                legend.Enabled = false;
            }
            else
            {
                legend.Enabled = true;
                legend.TextSize = 12f;
            }
            var sets = new List<ILineDataSet>
            {
                CreateLineDataSet(Color.Magenta, "X"),
                CreateLineDataSet(Color.LimeGreen , "Y"),
                CreateLineDataSet(Color.DarkBlue , "Z")
            };
            LineData data = new LineData(sets);
            mChart.Data = data;
            XAxis xl = mChart.XAxis;
            xl.SetDrawGridLines(true);
            xl.SetAvoidFirstLastClipping(true);
            xl.Enabled = true;
            YAxis leftAxis = mChart.AxisLeft;
            leftAxis.SetDrawGridLines(false);
            leftAxis.SetDrawGridLines(true);
            leftAxis.SpaceTop = 10f;
            YAxis rightAxis = mChart.AxisRight;
            rightAxis.Enabled = false;
        }

        public void UpdateSensorEntries(int broadcast_idx, float[] accVector, float[]  orientVector)
        {
            UpdatePlot(mOrientationChart, broadcast_idx, orientVector);
            UpdatePlot(mAccelChart, broadcast_idx, accVector);
        }

        private void UpdatePlot(LineChart mChart  , int broadcast_idx, float[] dataVector)
        {
            LineData data = mChart.LineData;
            if (data != null)
            {
                ILineDataSet setx = (ILineDataSet)data.DataSets[0];
                ILineDataSet sety = (ILineDataSet)data.DataSets[1];
                ILineDataSet setz = (ILineDataSet)data.DataSets[2];

                // add new plot entry
                data.AddEntry(new Entry(broadcast_idx, dataVector[0]), 0);
                data.AddEntry(new Entry(broadcast_idx, dataVector[1]), 1);
                data.AddEntry(new Entry(broadcast_idx, dataVector[2]), 2);
                mChart.NotifyDataSetChanged();
                // limit the number of visible entries
                mChart.SetVisibleXRangeMaximum(max_entries);

                // remove plot entries outside of current view
                if (setx.EntryCount == max_entries_saved)
                {
                    setx.RemoveEntry(0);
                    sety.RemoveEntry(0);
                    setz.RemoveEntry(0);
                    mChart.AxisLeft.SpaceTop = 10f;
                }
                // move to the latest entry
                mChart.MoveViewToX(broadcast_idx);
            }
        }

        private LineDataSet CreateLineDataSet(Color mcolor, string mLabel)
        {
            LineDataSet set = new LineDataSet(null, "Data")
            {
                AxisDependency = YAxis.AxisDependency.Left,
                LineWidth = 3f,
                Color = mcolor,
                HighlightEnabled = false,
                Label = mLabel
            };
            set.SetDrawValues(false);
            set.SetDrawCircles(false);
            set.SetMode(LineDataSet.Mode.CubicBezier);
            set.CubicIntensity = 0.1f;
            set.AddEntry(new Entry(0, 0));
            return set;
        }
               
        public override void OnBackPressed()
        {
            CheckBackPressed();
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            if (item.ItemId == Android.Resource.Id.Home)
            {
                CheckBackPressed();
                return true;
            }
            return base.OnOptionsItemSelected(item);
        }

        private void CheckBackPressed()
        {
            if (isStarted)
            {
                var dialog = new Android.App.AlertDialog.Builder(this);
                var alert = dialog.Create();
                alert.SetTitle("Are you sure?");
                alert.SetMessage("Running process will be stopped.");
                alert.SetButton("OK", (c, ev) =>
                {
                    StopandGoToMain();
                });
                alert.SetButton2("CANCEL", (c, ev) => { });
                alert.Show();
            }
            else
            {
                StopandGoToMain();
            }
        }

        private void StopandGoToMain()
        {
            isStarted = false;
            StopService(stopServiceIntent); 
            var intent = new Intent(this, typeof(MainActivity));
            intent.SetFlags(ActivityFlags.ClearTop);
            StartActivity(intent);
            OverridePendingTransition(Resource.Animation.abc_fade_in, Resource.Animation.abc_slide_out_bottom);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            LocalBroadcastManager.GetInstance(this).UnregisterReceiver(mySensorDataReceiver);
            LocalBroadcastManager.GetInstance(this).UnregisterReceiver(myStopSignalReceiver);
            switchServiceButton.Click -= SwitchService_Click;
        }
        public  void ShowSnackOnFinish( bool wasSuccess)
        {
            if (wasSuccess)
            {
                Snackbar.Make(mCoordinator, "Activity successfully saved", Snackbar.LengthShort)
               .SetAction("Action", (View.IOnClickListener)null).Show();
            }
            else
            {
                Snackbar.Make(mCoordinator, "Activity was too short.", Snackbar.LengthShort)
                .SetAction("Action", (View.IOnClickListener)null).SetActionTextColor(Color.Red).Show();
            }
        }
    }

    [BroadcastReceiver(Enabled = true, Exported = false)]
    public class XYZDataReceiver : BroadcastReceiver
    {
        public override void OnReceive(Context context, Intent intent)
        {
            Bundle b = intent.GetBundleExtra("sensor_data");
            var accVector    =   b.GetFloatArray("accVector");
            var orientVector =   b.GetFloatArray("orientVector");
            var broadcast_idx  = b.GetInt("broadcast_idx");            
            TrackingActivity.GetInstace().UpdateSensorEntries(broadcast_idx, accVector, orientVector);       
        }
    }

    [BroadcastReceiver(Enabled = true, Exported = false)]
    public class StopSignalReceiver : BroadcastReceiver
    {
        public override void OnReceive(Context context, Intent intent)
        {
            TrackingActivity.GetInstace().isStarted = false;
            TrackingActivity.GetInstace().switchServiceButton.LeftIconDrawable = (TrackingActivity.GetInstace().startIcon);
            TrackingActivity.GetInstace().switchServiceButton.Text = TrackingActivity.GetInstace().GetText(Resource.String.start);
            Bundle b = intent.GetBundleExtra("report");
            var wasSuccess = b.GetBoolean("SavingSuccessful",false);
            TrackingActivity.GetInstace().ShowSnackOnFinish(wasSuccess);
        }
    }
}