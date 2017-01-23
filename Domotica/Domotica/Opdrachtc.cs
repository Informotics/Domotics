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
    [Activity(Label = "@string/application_name", MainLauncher = false, Theme = "@style/Theme.Red", Icon = "@drawable/icon")]
    public class Opdrachtc : Activity, GestureDetector.IOnGestureListener
    {

        public bool OnDown(MotionEvent e)
        {
            return true;
        }
        public bool OnFling(MotionEvent e1, MotionEvent e2, float velocityX, float velocityY)
        {
            if (velocityX < 0)
            { }
            else if (velocityX > 0)
            {
                Intent intent = new Intent(this, typeof(Opdrachtb));
                this.StartActivity(intent);
                OverridePendingTransition(Resource.Animation.Leftin, Resource.Animation.Rightout);
            }
            else { }
            return true;
        }
        public void OnLongPress(MotionEvent e) { }
        public bool OnScroll(MotionEvent e1, MotionEvent e2, float distanceX, float distanceY)
        {
            return true;
        }
        public void OnShowPress(MotionEvent e) { }
        public bool OnSingleTapUp(MotionEvent e)
        {
            return false;
        }
        public override bool OnTouchEvent(MotionEvent e)
        {
            _gestureDetector.OnTouchEvent(e);
            return false;
        }
        private GestureDetector _gestureDetector;

        Button cknop;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            _gestureDetector = new GestureDetector(this);
            //statusbar settings
            this.Title = "Domotica App";
            this.Window.AddFlags(WindowManagerFlags.DrawsSystemBarBackgrounds);
            RequestWindowFeature(WindowFeatures.ActionBar);
            var actionBar = this.ActionBar;
            actionBar.NavigationMode = ActionBarNavigationMode.Tabs;

            var tab1 = this.ActionBar.NewTab();
            tab1.SetIcon(Resource.Drawable.a);
            tab1.TabSelected += btnA_Click;

            var tab2 = this.ActionBar.NewTab();
            tab2.SetIcon(Resource.Drawable.b);
            tab2.TabSelected += btnB_Click;

            var tab3 = this.ActionBar.NewTab();
            tab3.SetIcon(Resource.Drawable.c);
            tab3.TabSelected += (sender, e) => { };

            actionBar.AddTab(tab3);
            actionBar.AddTab(tab1, 0, false);
            actionBar.AddTab(tab2, 1, false);


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
            OverridePendingTransition(Resource.Animation.Leftin, Resource.Animation.Rightout);
        }

        public void btnA_Click(object sender, EventArgs e)
        {
            Intent intent = new Intent(this, typeof(MainActivity));
            this.StartActivity(intent);
            OverridePendingTransition(Resource.Animation.Leftin, Resource.Animation.Rightout);
        }
        public override bool OnPrepareOptionsMenu(IMenu menu)
        {
            //Prevent menu items from being duplicated.
            menu.Clear();

            MenuInflater.Inflate(Resource.Menu.menu, menu);
            return base.OnPrepareOptionsMenu(menu);
        }

        //Executes an action when a menu button is pressed.
        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {
                case Resource.Id.exit:
                    Intent intent = new Intent(this, typeof(Opdrachtsettings));
                    this.StartActivity(intent);
                    OverridePendingTransition(Resource.Animation.Rightin, Resource.Animation.Leftout);
                    return true;
                default: return true;
            }
        }
    }
}