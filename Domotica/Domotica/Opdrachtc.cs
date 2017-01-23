using Android.App;
using Android.Widget;
using Android.OS;
using System;
using Android.Content;
using Android.Views;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Timers;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using Android.Graphics;



namespace Domotica
{
    [Activity(Label = "@string/application_name", MainLauncher = false, Theme = "@style/Theme.Custom",  Icon = "@drawable/icon")]
    public class Opdrachtc : Activity
    {
        Button cknop;

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

            SetContentView(Resource.Layout.C);

            cknop = FindViewById<Button>(Resource.Id.cknop);

            if (cknop != null)
            {
                cknop.Click += (sender, e) =>
                {
                    MainActivity.socket.Send(Encoding.ASCII.GetBytes("g"));
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