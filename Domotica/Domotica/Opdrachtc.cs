using Android.App;
using Android.Widget;
using Android.OS;
using System;
using Android.Content;
using Android.Views;
using System.Net.Sockets;
using System.Text;


namespace Domotica
{
    [Activity(Label = "@string/application_name", MainLauncher = false, Theme = "@style/Theme.Custom",  Icon = "@drawable/icon")]
    public class Opdrachtc : Activity
    {
        Button cknop;
        Socket socket = null;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            //statusbar settings
            this.Title = "Domotica App";
            this.Window.AddFlags(WindowManagerFlags.DrawsSystemBarBackgrounds);
            RequestWindowFeature(WindowFeatures.ActionBar);
            var actionBar = this.ActionBar;
            actionBar.NavigationMode = ActionBarNavigationMode.Tabs;

            var tab1 = this.ActionBar.NewTab();
            tab1.SetText("A");
            tab1.TabSelected += btnA_Click;

            var tab2 = this.ActionBar.NewTab();
            tab2.SetText("B");
            tab2.TabSelected += btnB_Click;

            var tab3 = this.ActionBar.NewTab();
            tab3.SetText("C");
            tab3.TabSelected += (sender, e) => { };

            actionBar.AddTab(tab3);
            actionBar.AddTab(tab1, 0, false);
            actionBar.AddTab(tab2, 1, false);
            actionBar.SetSelectedNavigationItem(2);

            //var tab1 = this.ActionBar.NewTab();
            //tab1.SetText("A");
            //tab1.TabSelected += (sender, e) =>
            //{
            //    Intent intent = new Intent(this, typeof(MainActivity));
            //    this.StartActivity(intent);
            //};
            //actionBar.AddTab(tab1);

            //var tab2 = this.ActionBar.NewTab();
            //tab2.SetText("B");
            //tab2.TabSelected += (sender, e) =>
            //{
            //    Intent intent = new Intent(this, typeof(Opdrachtb));
            //    this.StartActivity(intent);
            //};
            //actionBar.AddTab(tab2);

            //var tab3 = this.ActionBar.NewTab();
            //tab3.SetText("C");
            //tab3.TabSelected += (sender, e) =>
            //{
            //    Intent intent = new Intent(this, typeof(Opdrachtc));
            //    this.StartActivity(intent);
            //};
            //actionBar.AddTab(tab3);

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.C);

            cknop = FindViewById<Button>(Resource.Id.cknop);

            if (cknop != null)
            {
                cknop.Click += (sender, e) =>
                {
                    socket.Send(Encoding.ASCII.GetBytes("g"));
                };
            }

        }

        public void btnB_Click(object sender, EventArgs e)
        {
            Intent intent = new Intent(this, typeof(Opdrachtb));
            this.StartActivity(intent);
        }

        public void btnA_Click(object sender, EventArgs e)
        {
            Intent intent = new Intent(this, typeof(MainActivity));
            this.StartActivity(intent);
        }
    }
}