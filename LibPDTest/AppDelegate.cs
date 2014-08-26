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

            var audioController = new PdAudioController();
            audioController.Active = true;
            LibPD.OpenPatch(Path.Combine(NSBundle.MainBundle.ResourcePath, "test.pd"));

            window = new UIWindow(UIScreen.MainScreen.Bounds);
            
            window.RootViewController = GetMainPage().CreateViewController();
            window.MakeKeyAndVisible();
            
            return true;
        }

        static Page GetMainPage()
        {    
            var slider = new Slider
            {
                VerticalOptions = LayoutOptions.CenterAndExpand,
                Minimum = 0,
                Maximum = 440
            };

            slider.ValueChanged += (object sender, ValueChangedEventArgs e) =>
            {                
                LibPD.SendFloat("frq", (float)e.NewValue);                   
            };

            var trigger = new Button
            {               
                Text = "Trigger",
                VerticalOptions = LayoutOptions.Center,
            };

            trigger.Clicked += (object sender, EventArgs e) =>
            {
                LibPD.SendBang("trigger");
            };

            return new ContentPage
            { 
                Content = new StackLayout
                {
                    Spacing = 0,
                    VerticalOptions = LayoutOptions.Center,
                    Children =
                    {
                        trigger,                    
                        slider,
                    }
                }
            };
        }
    }

}

