using Android.App;
using Android.Content.PM;
using Android.Runtime;
using Android.OS;

namespace Mobile.Droid
{
    [Activity(Label = "Mobile", Icon = "@mipmap/icon", Theme = "@style/MainTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity, Application.IActivityLifecycleCallbacks
    {
        public void OnActivityCreated(Activity activity, Bundle savedInstanceState) { }

        public void OnActivityDestroyed(Activity activity) { }

        public void OnActivityPaused(Activity activity) { }

        public void OnActivityResumed(Activity activity) { }

        public void OnActivitySaveInstanceState(Activity activity, Bundle outState) { }

        public void OnActivityStarted(Activity activity) { }

        public void OnActivityStopped(Activity activity) { }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            TabLayoutResource = Resource.Layout.Tabbar;
            ToolbarResource = Resource.Layout.Toolbar;

            base.OnCreate(savedInstanceState);

            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            Xamarin.Forms.Forms.Init(this, savedInstanceState);
            
            LoadApplication(new App());
        }
        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }
    }
}