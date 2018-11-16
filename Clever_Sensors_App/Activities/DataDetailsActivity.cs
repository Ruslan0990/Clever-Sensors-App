using Android.App;
using Android.OS;
using MikePhil.Charting.Charts;
using MikePhil.Charting.Data;
using MikePhil.Charting.Components;
using MikePhil.Charting.Interfaces.Datasets;
using Android.Content.PM;
using Android.Views;
using Android.Widget;
using System.Linq;
using Android.Graphics;
using System.Collections.Generic;
using Android.Content;
using Clever_Sensors_App.Database;

namespace Clever_Sensors_App.Activities
{
    [Activity(Label = "Tracking Details",  ScreenOrientation = ScreenOrientation.Portrait)]
    public class DataDetailsActivity : BaseActivity
    {
        LineChart mAccelChart, mOrientationChart;
        DataBaseHelper mDataBaseHelper;
        const int max_OnCreateEntries = 1000;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.DataDetailsLayout);
            Android.Support.V7.Widget.Toolbar toolbar = FindViewById<Android.Support.V7.Widget.Toolbar>(Resource.Id.toolbar);
            if (toolbar != null)
            {
                SetSupportActionBar(toolbar);
                SupportActionBar.SetDisplayShowHomeEnabled(true);
                SupportActionBar.SetDisplayHomeAsUpEnabled(true);
            }
            TextView mText = FindViewById<TextView>(Resource.Id.textView1);
            mAccelChart = FindViewById<LineChart>(Resource.Id.accelerometer_chart);
            mOrientationChart = FindViewById<LineChart>(Resource.Id.orientation_chart);

            if (savedInstanceState == null)
            {
                // Get the selected run StartTicks as ID from the intent extra.
                int StartTicksID = Intent.GetIntExtra("StartTicks", 0);
                // load database
                mDataBaseHelper = new DataBaseHelper();
                var motionData = mDataBaseHelper.GetMotionDataItem(StartTicksID);
                mText.Text = motionData.StartDate.ToString("g");
                // setup charts
                 PrepareLineChart(mAccelChart, 1, motionData.AccXList.ToList(), motionData.AccYList.ToList(), motionData.AccZList.ToList());
                 PrepareLineChart(mOrientationChart, 2, motionData.OrientXList.ToList(), motionData.OrientYList.ToList(), motionData.OrientZList.ToList());
                //await Task.WhenAll(t1, t2);
                mAccelChart.NotifyDataSetChanged();
                mOrientationChart.NotifyDataSetChanged();
            }
        }

        // Set apearance of MPandroidChart here
        private void PrepareLineChart(LineChart mChart, int chartIdx, List<float> AccXList, List<float> AccYList, List<float> AccZList)
        {
            mChart.Description.Enabled = false;            
            mChart.SetTouchEnabled(true);
            mChart.SetScaleEnabled(false);
            mChart.SetDrawGridBackground(false);
            mChart.AnimateX(1000);
            mChart.SetDrawBorders(true);
            mChart.SetHardwareAccelerationEnabled(true);
            XAxis xl = mChart.XAxis;
            xl.SetDrawGridLines(true);
            xl.SetAvoidFirstLastClipping(true);
            xl.Enabled = true;
            YAxis leftAxis = mChart.AxisLeft;
            leftAxis.SetDrawGridLines(false);
            leftAxis.SetDrawGridLines(true);
            YAxis rightAxis = mChart.AxisRight;
            rightAxis.Enabled = false;
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
                CreateLineDataSet(Color.Magenta, "X", AccXList),
                CreateLineDataSet(Color.LimeGreen, "Y", AccYList),
                CreateLineDataSet(Color.DarkBlue, "Z", AccZList)
            };
            mChart.Data = new LineData(sets);
            mChart.SetVisibleXRangeMaximum(100);
        }

        private LineDataSet CreateLineDataSet(Color mcolor, string mLabel, List<float> MotionList)
        {
            LineDataSet set = new LineDataSet(null, "Data")
            {
                AxisDependency = YAxis.AxisDependency.Left,
                LineWidth = 2.5f,
                Color = mcolor,
                HighlightEnabled = false,
                Label = mLabel
            };
            set.SetDrawValues(false);
            set.SetDrawCircles(false);
            set.SetMode(LineDataSet.Mode.CubicBezier);
            set.CubicIntensity = 0.1f;

            if (MotionList.Count > max_OnCreateEntries)
            {
                for (int i = 0; i < max_OnCreateEntries; i++)
                {
                    set.AddEntry(new Entry(i, MotionList[i]));
                }
            }
            else
            {
                for (int i = 0; i < MotionList.Count; i++)
                {
                    set.AddEntry(new Entry(i, MotionList[i]));
                }
            }        
            return set;
        }

        public override void OnBackPressed()
        {
             GoToMain();
        }
               
        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            if (item.ItemId == Android.Resource.Id.Home)
            {
                GoToMain();
                return true;
            }
            return base.OnOptionsItemSelected(item);
        }
        private void GoToMain()
        {
            var intent = new Intent(this, typeof(MainActivity));
            intent.SetFlags(ActivityFlags.ClearTop);
            StartActivity(intent);
            OverridePendingTransition(Resource.Animation.abc_fade_in, Resource.Animation.abc_slide_out_bottom);
        }
    }
}