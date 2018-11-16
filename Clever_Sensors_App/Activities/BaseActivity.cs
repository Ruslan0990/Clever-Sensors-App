using Android.App;
using Android.Content;
using Android.OS;
using Android.Support.V7.App;
using Android.Preferences;
using Clever_Sensors_App.Database;

namespace Clever_Sensors_App.Activities
{
    [Activity(Label = "BaseActivity")]
    public class BaseActivity : AppCompatActivity
    {
        ISharedPreferences sharedPref;
        bool darkThemeOn;
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            sharedPref = PreferenceManager.GetDefaultSharedPreferences(this);
            darkThemeOn = sharedPref.GetBoolean(Constants.KEY_CURRENT_THEME, true);

            SetAppTheme(darkThemeOn);
        }

        protected  override void  OnResume()
        {
            base.OnResume();
            var currentThemeSetting = sharedPref.GetBoolean(Constants.KEY_CURRENT_THEME, true);
            if (darkThemeOn != currentThemeSetting)
                Recreate();
        }

        private void SetAppTheme(bool mDarkThemeOn)
        {
            if (mDarkThemeOn )
            {
                this.SetTheme(Resource.Style.Theme_App_Mint);
            }
            else
            {
                this.SetTheme(Resource.Style.Theme_App_Lilac);
            }
        }
    }
}