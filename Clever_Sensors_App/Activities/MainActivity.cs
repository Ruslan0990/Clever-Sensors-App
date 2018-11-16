using Android.App;
using Android.OS;
using Android.Views;
using Android.Content.PM;
using System;
using Android.Support.Design.Widget;
using Android.Support.V7.Widget;
using Android.Content;
using Clever_Sensors_App.Database;
using Clever_Sensors_App.Adapters;
using Android.Support.V7.Widget.Helper;
using Android.Support.V7.App;
using Android.Support.V4.Widget;
using System.Threading.Tasks;

namespace Clever_Sensors_App.Activities
{
    [Activity(Label = "@string/app_name", MainLauncher = true, ScreenOrientation = ScreenOrientation.Portrait)]
    public class MainActivity : BaseActivity
    {
        RecyclerView mRecyclerView;
        TrackingRecyclerAdapter mAdapter;
        FloatingActionButton fab;
        DataBaseHelper mDataBaseHelper;
        SwipeRefreshLayout swipeContainer;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.activity_main);            
            Toolbar toolbar = FindViewById<Toolbar>(Resource.Id.toolbar);
            SetSupportActionBar(toolbar);

            fab = FindViewById<FloatingActionButton>(Resource.Id.fab);
            fab.Click += FabOnClick;

            swipeContainer = FindViewById<SwipeRefreshLayout>(Resource.Id.slSwipeContainer);
            swipeContainer.SetColorSchemeResources(Android.Resource.Color.HoloOrangeDark);
            swipeContainer.Refresh += SwipeContainer_Refresh;

            mRecyclerView = FindViewById<RecyclerView>(Resource.Id.recyclerView);

            var mLayoutManager = new LinearLayoutManager(this);
            mRecyclerView.SetLayoutManager(mLayoutManager);

            mDataBaseHelper = new DataBaseHelper();
            
            mAdapter = new TrackingRecyclerAdapter(this);
            mAdapter.ItemClick += OnItemClick;      
            mRecyclerView.SetAdapter(mAdapter);
            mAdapter.RefreshSensorsData(mDataBaseHelper.GetMetaDataItems());

            // Add a touch helper to the recycler view for user swipe deletion
            (new ItemTouchHelper(new SwipeToDeleteHelper(mAdapter, mDataBaseHelper))).AttachToRecyclerView(mRecyclerView);
        }

        // React on swipe to refresh
        async void SwipeContainer_Refresh(object sender, EventArgs e)
        {
            mAdapter.RefreshSensorsData(mDataBaseHelper.GetMetaDataItems());
            await Task.Delay(600);
            (sender as SwipeRefreshLayout).Refreshing = false;
        }

        protected override void  OnResume()
        {
            base.OnResume();
            mAdapter.RefreshSensorsData(mDataBaseHelper.GetMetaDataItems());
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            fab.Click -= FabOnClick;
            mAdapter.ItemClick -= OnItemClick;
            swipeContainer.Refresh -= SwipeContainer_Refresh;
        }

        void OnItemClick(object sender, int StartTicksID)
        {
            var intent = new Intent(this, typeof(DataDetailsActivity));
            intent.PutExtra("StartTicks", StartTicksID );
            StartActivity(intent);
            OverridePendingTransition(Resource.Animation.abc_grow_fade_in_from_bottom, Resource.Animation.abc_fade_out);
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater.Inflate(Resource.Menu.menu_main, menu);
            return true;
        }
        private void FabOnClick(object sender, EventArgs eventArgs)
        {    
            StartActivity(typeof(TrackingActivity));
            OverridePendingTransition(Resource.Animation.abc_grow_fade_in_from_bottom, Resource.Animation.abc_fade_out);        
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            int id = item.ItemId;
            if (id == Resource.Id.action_settings)
            {
                StartActivity(typeof(SettingsActivity));
            }
            return base.OnOptionsItemSelected(item);
        }
    }

    internal class SwipeToDeleteHelper : ItemTouchHelper.SimpleCallback
    {
        TrackingRecyclerAdapter mAdapter;
        DataBaseHelper mDBHelper;

        internal SwipeToDeleteHelper(TrackingRecyclerAdapter adapter, DataBaseHelper dbHelper) : base(0, ItemTouchHelper.Left | ItemTouchHelper.Right)
        {
            mAdapter = adapter;
            mDBHelper = dbHelper;
        }

        public override bool OnMove(RecyclerView recyclerView, RecyclerView.ViewHolder viewHolder, RecyclerView.ViewHolder target)
        {
            throw new NotImplementedException();
        }

        public override void OnSwiped(RecyclerView.ViewHolder viewHolder, int direction)
        {
            int position = viewHolder.AdapterPosition;
            mAdapter.OnItemDismiss(position);
            var dataList = mDBHelper.GetMetaDataItems();
            mDBHelper.DeleteFromDatabase(dataList[position].StartTicks);
        }  
    }
}