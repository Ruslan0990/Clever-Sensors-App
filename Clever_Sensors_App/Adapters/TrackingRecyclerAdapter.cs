using System;
using Android.Views;
using Android.Widget;
using Android.Support.V7.Widget;
using System.Collections.Generic;
using Clever_Sensors_App.Database;
using Android.Support.Graphics.Drawable;
using Android.Content;

namespace Clever_Sensors_App.Adapters
{
    class TrackingRecyclerAdapter : RecyclerView.Adapter
    {
        public event EventHandler<int> ItemClick;
        
        List<TrackingMetaData> mDataList;
        Context mContext;
        public TrackingRecyclerAdapter(Context context )
        {
            mContext = context;      
        }

        public void RefreshSensorsData(List<TrackingMetaData> dataList)
        {
            mDataList?.Clear();
            mDataList = dataList;
            NotifyDataSetChanged();
        }

        public void OnItemDismiss(int position)
        {
            mDataList.RemoveAt(position);
            NotifyItemRemoved(position);
        }


        public override int ItemCount => mDataList.Count;

        public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
        {
            var vh = holder as RunCardViewHolder;
            var mData = mDataList[position];
            vh.DateTextView.Text = mData.StartDate.ToString("g");
            TimeSpan duration = TimeSpan.FromSeconds(mData.Duration);
            vh.ActivityTextView.Text =  "Activity" ;
            vh.DurationTextView.Text = Constants.ToReadableString(duration);
            var runIcon = VectorDrawableCompat.Create(mContext.Resources, Resource.Drawable.ic_run, mContext.Theme);
            vh.ActivityTextView.SetCompoundDrawablesWithIntrinsicBounds(runIcon, null, null, null);
            vh.ItemView.Click += delegate
            {
                ItemClick?.Invoke(this, mData.StartTicks);
            };
        }

        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            View itemView = LayoutInflater.From(parent.Context).
                       Inflate(Resource.Layout.RunCardView, parent, false);
            return new RunCardViewHolder(itemView);
        }
    }

    class RunCardViewHolder : RecyclerView.ViewHolder
    {
        public TextView DateTextView { get; private set; }
        public TextView ActivityTextView { get; private set; }
        public TextView DurationTextView { get; private set; }

        public RunCardViewHolder(View itemView)
        : base(itemView)
        {
            DateTextView = itemView.FindViewById<TextView>(Resource.Id.time_txt);
            ActivityTextView = itemView.FindViewById<TextView>(Resource.Id.activity_txt);
            DurationTextView = itemView.FindViewById<TextView>(Resource.Id.time_duration_txt);
        }
    }
}