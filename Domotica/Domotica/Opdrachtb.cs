using Android.App;
using Android.Widget;
using Android.OS;
using System;
using Android.Content;
using Domotica.BroadCast;
using Android.Views;

namespace Domotica
{
    [Activity(Label = "@string/application_name", MainLauncher = false, Theme = "@style/Theme.Custom", Icon = "@drawable/icon")]
    public class Opdrachtb : Activity
    {
        private TextView time_display;
        private Button pickt_button;

        private int hour;
        private int minute;
        private int hour1;
        private int minute1;

        const int TIME_DIALOG_ID = 0;

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
            tab2.TabSelected += (sender, e) => { };

            var tab3 = this.ActionBar.NewTab();
            tab3.SetText("C");
            tab3.TabSelected += btnC_Click;

            actionBar.AddTab(tab2);
            actionBar.AddTab(tab1, 0, false);
            actionBar.AddTab(tab3, 2, false);
            actionBar.SetSelectedNavigationItem(1);

            // Set our view from the "main" layout resource
            SetContentView (Resource.Layout.B);

            time_display = FindViewById<TextView>(Resource.Id.timeDisplay);
            pickt_button = FindViewById<Button>(Resource.Id.pickTime);

            var btnRepeating = FindViewById<Button>(Resource.Id.btnRepeating);
            var btnCancel = FindViewById<Button>(Resource.Id.btnCancel);

            btnRepeating.Click += delegate
            {
                UpdateDisplay();
                StartAlarm();
            };

            btnCancel.Click += delegate
            {
                CancelAlarm();
            };

            pickt_button.Click += (o, e) => ShowDialog(TIME_DIALOG_ID);

            hour = DateTime.Now.Hour;
            minute = DateTime.Now.Minute;

            UpdateDisplay();

        }

        public void btnC_Click(object sender, EventArgs e)
        {
            Intent intent = new Intent(this, typeof(Opdrachtc));
            this.StartActivity(intent);
        }

        public void btnA_Click(object sender, EventArgs e)
        {
            Intent intent = new Intent(this, typeof(MainActivity));
            this.StartActivity(intent);
        }

        //klok

        private void UpdateDisplay()
        {
            string time = string.Format("{0}:{1}", hour, minute.ToString().PadLeft(2, '0'));
            time_display.Text = time;

            hour1 = hour - DateTime.Now.Hour;

            if (hour1 < 0)
            {
                hour1 = hour1 + 24;
            }

            minute1 = minute - DateTime.Now.Minute;

            if (minute1 < 0)
            {
                minute1 = minute1 + 60;
                hour1 = hour1 - 1;
            }
        }

        private void TimePickerCallback(object sender, TimePickerDialog.TimeSetEventArgs e)
        {
            hour = e.HourOfDay;
            minute = e.Minute;

            UpdateDisplay();
        }

        protected override Dialog OnCreateDialog(int id)
        {
            if (id == TIME_DIALOG_ID)
                return new TimePickerDialog(this, TimePickerCallback, hour, minute, true);

            return null;
        }

        //wekker

        private void StartAlarm()
        {
            AlarmManager manager = (AlarmManager)GetSystemService(Context.AlarmService);
            Intent myIntent;
            PendingIntent pendingIntent;
            myIntent = new Intent(this, typeof(AlarmNotificationReceiver));
            pendingIntent = PendingIntent.GetBroadcast(this, 0, myIntent, 0);
            manager.SetRepeating(AlarmType.ElapsedRealtimeWakeup, (SystemClock.ElapsedRealtime() + (hour1 * 3600 * 1000) + (minute1 * 60 * 1000)), 60 * 1000, pendingIntent);
            Toast.MakeText(this, "Alarm set", ToastLength.Long).Show();
        }

        private void CancelAlarm()
        {
            AlarmManager manager = (AlarmManager)GetSystemService(Context.AlarmService);
            Intent myIntent;
            PendingIntent pendingIntent;
            myIntent = new Intent(this, typeof(AlarmNotificationReceiver));
            pendingIntent = PendingIntent.GetBroadcast(this, 0, myIntent, 0);
            manager.Cancel(pendingIntent);
            Toast.MakeText(this, "Alarm canceled", ToastLength.Long).Show();
        }
    }
}