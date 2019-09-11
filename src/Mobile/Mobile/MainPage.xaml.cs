using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using Mobile.Networking;
using Xamarin.Forms;

namespace Mobile
{
    // Learn more about making custom code visible in the Xamarin.Forms previewer
    // by visiting https://aka.ms/xamarinforms-previewer
    [DesignTimeVisible(false)]
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
        }

        private async void Button_OnClicked(object sender, EventArgs e)
        {
            var networks = NetworkInterface.GetAllNetworkInterfaces().Select(x=>x.GetIPProperties());

            using (var d = new DiscoveryService())
            {
                d.OnDevice += (s, args) =>
                {
                    var info = args.DiscoveredDevice;
                };

                d.OnDeviceDisconnect += (s, args) => {
                    var info = args.DiscoveredDevice;
                };

                var task = await d.SearchAsync();
            }
        }
    }
}
