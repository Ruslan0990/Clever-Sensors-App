using Android.Content;
using Android.OS;
using Android.Widget;
using Android.Support.V7.Preferences;

namespace Clever_Sensors_App.Activities
{
    public class SettingsFragment : PreferenceFragmentCompat
    {
        Context context;
        Toast toast;
        Preference colorPref;

        public override void OnCreatePreferences(Bundle savedInstanceState, string rootKey)
        {
            AddPreferencesFromResource(Resource.Xml.Preferences);
            context = Activity.ApplicationContext;            
            colorPref = FindPreference("current_theme");
            colorPref.PreferenceClick += OnSettingsClicked;
        }
        public override void OnPause()
        {
            colorPref.PreferenceClick -= OnSettingsClicked;
            base.OnPause();
        }
        
        void OnSettingsClicked(object sender, Preference.PreferenceClickEventArgs e)
        {
            Activity.Recreate();
            toast = Toast.MakeText(context, "Settings Changed", ToastLength.Short);
            toast.Show();
        }
    }
}