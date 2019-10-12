using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace Minesweeper
{

    sealed partial class App : Application
    {

        public App()
        {
            this.InitializeComponent();
            this.Suspending += OnSuspending;
        }

        protected override void OnLaunched(LaunchActivatedEventArgs _e)
        {
            if (!(Window.Current.Content is MainPage))
            {
                Window.Current.Content = new MainPage();
            }

            if (_e.PrelaunchActivated == false)
            {
                Window.Current.Activate();
            }
        }

        private async void OnSuspending(object _sender, SuspendingEventArgs _e)
        {
            var deferral = _e.SuspendingOperation.GetDeferral();
            var page = (Window.Current.Content as MainPage);
            if (page != null)
            {
                await page.SaveGame();
            }
            deferral.Complete();
        }
    }
}
