using System;
using System.Collections.Generic;
using System.Linq;

using MonoTouch.Foundation;
using MonoTouch.UIKit;
using Xamarin.Forms;
using LibPDBinding;
using System.IO;

namespace LibPDTest
{
    [Register("AppDelegate")]
    public partial class AppDelegate : UIApplicationDelegate
    {
        UIWindow window;

        public override bool FinishedLaunching(UIApplication app, NSDictionary options)
        {
            try
            {
                LibPD.ReInit();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            Forms.Init();

            var audioController = new PdAudioController ();
            audioController.Active = true;
            LibPD.OpenPatch (Path.Combine (NSBundle.MainBundle.ResourcePath, "test.pd"));

            window = new UIWindow (UIScreen.MainScreen.Bounds);
            
            window.RootViewController = GetMainPage ().CreateViewController ();
            window.MakeKeyAndVisible ();
            
            return true;
        }

        static Page GetMainPage()
        {    
            return new ContentPage
            { 
                Content = new Label
                {
                    Text = "Hello, Forms!",
                    VerticalOptions = LayoutOptions.CenterAndExpand,
                    HorizontalOptions = LayoutOptions.CenterAndExpand,
                },
            };
        }
    }

}

