using Android.App;
using Android.Content.PM;
using Android.OS;
using ImageCircle.Forms.Plugin.Droid;
using Com.OneSignal;
using Android.Content;

namespace GDG_SP.Droid
{
    [Activity(Label = "GDG-SP", Icon = "@drawable/ic_launcher", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsApplicationActivity
    {
        public static Context c;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            c = this;

            global::Xamarin.Forms.Forms.Init(this, bundle);
            Xamarin.Forms.DependencyService.Register<Dependencies_Android>();

            ImageCircleRenderer.Init();

            OneSignal.NotificationOpened exampleNotificationOpenedDelegate = delegate (string message, System.Collections.Generic.Dictionary<string, object> additionalData, bool isActive)
            {
                try
                {
                    MainPage.openEvent = (int)additionalData["eventid"];
                }
                catch (System.Exception e)
                {
                    System.Console.WriteLine(e.StackTrace);
                }
            };

            OneSignal.Init(exampleNotificationOpenedDelegate);

            LoadApplication(new App());
        }
    }
}

