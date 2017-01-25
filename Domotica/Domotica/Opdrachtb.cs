using Android.App;
using Android.Widget;
using Android.OS;
using System;
using Android.Content;
using Domotica.BroadCast;
using Android.Views;
using System.Timers;

namespace Domotica
{
    [Activity(Label = "@string/application_name", MainLauncher = false, Theme = "@style/Theme.Green", Icon = "@drawable/icon")]
    public class Opdrachtb : Activity, GestureDetector.IOnGestureListener
    {
        public bool OnDown(MotionEvent e)
        {
            return true;
        }
        public bool OnFling(MotionEvent e1, MotionEvent e2, float velocityX, float velocityY)
        {
            int velocity = (int)Math.Ceiling(velocityX);
            if (velocity < 0)
            {
                Intent intent = new Intent(this, typeof(Opdrachtc));
                this.StartActivity(intent);
                OverridePendingTransition(Resource.Animation.Rightin, Resource.Animation.Leftout);
            }
            else if (velocity > 0)
            {
                Intent intent = new Intent(this, typeof(MainActivity));
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

        TextView time_display, textViewTimerStateValue;
        Button pickt_button;
        Button btnRepeating;
        Button btnCancel;
        Timer timerClock;

        private int hour;
        private int minute;
        private int hour1;
        private int minute1;


        const int TIME_DIALOG_ID = 0;

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
            tab2.TabSelected += (sender, e) => { };

            var tab3 = this.ActionBar.NewTab();
            tab3.SetIcon(Resource.Drawable.c);
            tab3.TabSelected += btnC_Click;

            actionBar.AddTab(tab2);
            actionBar.AddTab(tab1, 0, false);
            actionBar.AddTab(tab3, 2, false);

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.B);

            time_display = FindViewById<TextView>(Resource.Id.timeDisplay);
            pickt_button = FindViewById<Button>(Resource.Id.pickTime);
            textViewTimerStateValue = FindViewById<TextView>(Resource.Id.textViewTimerStateValue);

            btnRepeating = FindViewById<Button>(Resource.Id.btnRepeating);
            btnCancel = FindViewById<Button>(Resource.Id.btnCancel);

            // timer object, running clock
            timerClock = new System.Timers.Timer() { Interval = 1000, Enabled = true }; // Interval >= 1000
            timerClock.Elapsed += (obj, args) =>
            {
                RunOnUiThread(() =>
                {
                    textViewTimerStateValue.Text = DateTime.Now.ToString("HH:mm:ss");
                });
            };

            btnRepeating.Click += delegate
            {
                UpdateDisplay();
                StartAlarm();
                StartAlarm2();
                StartAlarm3();
                StartAlarm4();
            };

            btnCancel.Click += delegate
            {
                CancelAlarm0();
                CancelAlarm();
                CancelAlarm3();
                CancelAlarm4();
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
            OverridePendingTransition(Resource.Animation.Rightin, Resource.Animation.Leftout);
        }

        public void btnA_Click(object sender, EventArgs e)
        {
            Intent intent = new Intent(this, typeof(MainActivity));
            this.StartActivity(intent);
            OverridePendingTransition(Resource.Animation.Leftin, Resource.Animation.Rightout);
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
            manager.Set(AlarmType.ElapsedRealtimeWakeup, (SystemClock.ElapsedRealtime() + (hour1 * 3600 * 1000) + (minute1 * 60 * 1000)), pendingIntent);
            Toast.MakeText(this, "Alarm set", ToastLength.Long).Show();
        }

        private void StartAlarm2()
        {
            AlarmManager manager = (AlarmManager)GetSystemService(Context.AlarmService);
            Intent myIntent;
            PendingIntent pendingIntent;
            myIntent = new Intent(this, typeof(Receiver2));
            pendingIntent = PendingIntent.GetBroadcast(this, 0, myIntent, 0);
            manager.SetRepeating(AlarmType.ElapsedRealtimeWakeup, (SystemClock.ElapsedRealtime() + (hour1 * 3600 * 1000) + (minute1 * 60 * 1000) + 60 * 1000), 60 * 1000, pendingIntent);
        }

        private void StartAlarm3()
        {
            AlarmManager manager = (AlarmManager)GetSystemService(Context.AlarmService);
            Intent myIntent;
            PendingIntent pendingIntent;
            myIntent = new Intent(this, typeof(Receiver3));
            pendingIntent = PendingIntent.GetBroadcast(this, 0, myIntent, 0);
            manager.Set(AlarmType.ElapsedRealtimeWakeup, (SystemClock.ElapsedRealtime() + (hour1 * 3600 * 1000) + ((minute1 - 1) * 60 * 1000)), pendingIntent);
        }

        private void StartAlarm4()
        {
            AlarmManager manager = (AlarmManager)GetSystemService(Context.AlarmService);
            Intent myIntent;
            PendingIntent pendingIntent;
            myIntent = new Intent(this, typeof(Receiver4));
            pendingIntent = PendingIntent.GetBroadcast(this, 0, myIntent, 0);
            manager.Set(AlarmType.ElapsedRealtimeWakeup, (SystemClock.ElapsedRealtime() + (hour1 * 3600 * 1000) + ((minute1 + 1) * 60 * 1000)), pendingIntent);
        }

        private void CancelAlarm0()
        {
            AlarmManager manager = (AlarmManager)GetSystemService(Context.AlarmService);
            Intent myIntent;
            PendingIntent pendingIntent;
            myIntent = new Intent(this, typeof(AlarmNotificationReceiver));
            pendingIntent = PendingIntent.GetBroadcast(this, 0, myIntent, 0);
            manager.Set(AlarmType.ElapsedRealtimeWakeup, (SystemClock.ElapsedRealtime() + (hour1 * 3600 * 1000) + (minute1 * 60 * 1000)), pendingIntent);
            Toast.MakeText(this, "Alarm set", ToastLength.Long).Show();
            manager.Cancel(pendingIntent);

        }

        private void CancelAlarm3()
        {
            AlarmManager manager = (AlarmManager)GetSystemService(Context.AlarmService);
            Intent myIntent;
            PendingIntent pendingIntent;
            myIntent = new Intent(this, typeof(Receiver3));
            pendingIntent = PendingIntent.GetBroadcast(this, 0, myIntent, 0);
            manager.Set(AlarmType.ElapsedRealtimeWakeup, (SystemClock.ElapsedRealtime() + (hour1 * 3600 * 1000) + ((minute1 - 1) * 60 * 1000)), pendingIntent);
            manager.Cancel(pendingIntent);

        }

        private void CancelAlarm4()
        {
            AlarmManager manager = (AlarmManager)GetSystemService(Context.AlarmService);
            Intent myIntent;
            PendingIntent pendingIntent;
            myIntent = new Intent(this, typeof(Receiver4));
            pendingIntent = PendingIntent.GetBroadcast(this, 0, myIntent, 0);
            manager.Set(AlarmType.ElapsedRealtimeWakeup, (SystemClock.ElapsedRealtime() + (hour1 * 3600 * 1000) + ((minute1 + 1) * 60 * 1000)), pendingIntent);
            manager.Cancel(pendingIntent);

        }

        private void CancelAlarm()
        {
            AlarmManager manager = (AlarmManager)GetSystemService(Context.AlarmService);
            Intent myIntent;
            PendingIntent pendingIntent;
            myIntent = new Intent(this, typeof(Receiver2));
            pendingIntent = PendingIntent.GetBroadcast(this, 0, myIntent, 0);
            manager.SetRepeating(AlarmType.ElapsedRealtimeWakeup, (SystemClock.ElapsedRealtime() + (hour1 * 3600 * 1000) + (minute1 * 60 * 1000) + 60 * 1000), 60 * 1000, pendingIntent);
            manager.Cancel(pendingIntent);
            Toast.MakeText(this, "Alarm canceled", ToastLength.Long).Show();
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