// Xamarin/C# app voor de besturing van een Arduino (Uno met Ethernet Shield) m.b.v. een socket-interface.
// Dit programma werkt samen met het Arduino-programma DomoticaServer.ino
// De besturing heeft betrekking op het aan- en uitschakelen van een Arduino pin, waar een led aan kan hangen of, 
// t.b.v. het Domotica project, een RF-zender waarmee een klik-aan-klik-uit apparaat bestuurd kan worden.
//
// De socket-communicatie werkt in is gebaseerd op een timer, waarbij het opvragen van gegevens van de 
// Arduino (server) m.b.v. een Timer worden gerealisseerd.
//
// Werking: De communicatie met de (Arduino) server is gebaseerd op een socket-interface. Het IP- en Port-nummer
// is instelbaar. Na verbinding kunnen, middels een eenvoudig commando-protocol, opdrachten gegeven worden aan 
// de server (bijv. pin aan/uit). Indien de server om een response wordt gevraagd (bijv. led-status of een
// sensorwaarde), wordt deze in een 4-bytes ASCII-buffer ontvangen, en op het scherm geplaatst. Alle commando's naar 
// de server zijn gecodeerd met 1 char.
//
// Aanbeveling: Bestudeer het protocol in samenhang met de code van de Arduino server.
// Het default IP- en Port-nummer (zoals dat in het GUI verschijnt) kan aangepast worden in de file "Strings.xml". De
// ingestelde waarde is gebaseerd op je eigen netwerkomgeving, hier (en in de Arduino-code) is dat een router, die via DHCP
// in het segment 192.168.1.x IP-adressen uitgeeft.
// 
// Resource files:
//   Main.axml (voor het grafisch design, in de map Resources->layout)
//   Strings.xml (voor alle statische strings in het interface (ook het default IP-adres), in de map Resources->values)
// 
// De software is verder gedocumenteerd in de code. Tijdens de colleges wordt er nadere uitleg over gegeven.
// 
// Versie 1.2, 16/12/2016
// S. Oosterhaven
//
using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Timers;
using Android.App;
using Android.Content;
using Android.Views;
using Android.Widget;
using Android.OS;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using Android.Graphics;
using System.Threading.Tasks;
using Domotica.BroadCast;


namespace Domotica
{
    //BEGIN SPLASHSCREEN
    [Activity(Theme = "@style/MyTheme.Splash", MainLauncher = true, NoHistory = true)]
    public class SplashActivity : Activity
    {
        static readonly string TAG = "X:" + typeof(SplashActivity).Name;

        public override void OnCreate(Bundle savedInstanceState, PersistableBundle persistentState)
        {
            base.OnCreate(savedInstanceState, persistentState);
            Window.RequestFeature(WindowFeatures.NoTitle);      //Verberg de statusbar en de titel

        }
        protected override void OnResume()
        {
            base.OnResume();

            Task startupWork = new Task(() => {
                Task.Delay(1000);  // Laat het splashscreen nog wel even zien als je te snel laad.
            });

            startupWork.ContinueWith(t => {
                StartActivity(new Intent(Application.Context, typeof(MainActivity)));   //Als je klaar bent met laden, start mainactivity
            }, TaskScheduler.FromCurrentSynchronizationContext());

            startupWork.Start();
        }
    }
    //EIND SPLASHSCREEN
    [Activity(Label = "@string/application_name", MainLauncher = false, Icon = "@drawable/icon", Theme ="@style/Theme.Custom", ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait)]

    public class MainActivity : Activity, GestureDetector.IOnGestureListener
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
                Intent intent = new Intent(this, typeof(Opdrachtb));
                this.StartActivity(intent);
                OverridePendingTransition(Resource.Animation.Rightin, Resource.Animation.Leftout);

            }
            else if (velocity > 0)
            { }
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


        // Variables (components/controls)
        // Controls on GUI
        Button toggleSchakelaar0, toggleSchakelaar1, toggleSchakelaar2;
        TextView textViewTimerStateValue;
        TextView textViewSensorValue, textViewSensorValue2, kloktijd;
        Timer timerClock, timerSockets;             // Timers   
        public static Socket socket = null;   
                            // Socket   
        List<Tuple<string, TextView>> commandList = new List<Tuple<string, TextView>>();  // List for commands and response places on UI
        int listIndex = 0;
        private GestureDetector _gestureDetector;


        //Initalisatie van variabelen voor klok
        const int timedialog = 0;
        private int hour;
        private int minute;
        private int hour1;
        private int minute1;

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
            tab1.TabSelected += (sender, e) => { };

            var tab2 = this.ActionBar.NewTab();
            tab2.SetIcon(Resource.Drawable.b);
            tab2.TabSelected += btnB_Click;

            var tab3 = this.ActionBar.NewTab();
            tab3.SetIcon(Resource.Drawable.c);
            tab3.TabSelected += btnC_Click;

            actionBar.AddTab(tab1);
            actionBar.AddTab(tab2);
            actionBar.AddTab(tab3);


            // Set our view from the "main" layout resource (strings are loaded from Recources -> values -> Strings.xml)
            SetContentView(Resource.Layout.Main);

            // find and set the controls, so it can be used in the code
            toggleSchakelaar0 = FindViewById<Button>(Resource.Id.toggleButton0);
            toggleSchakelaar1 = FindViewById<Button>(Resource.Id.toggleButton1);
            toggleSchakelaar2 = FindViewById<Button>(Resource.Id.toggleButton2);
            textViewTimerStateValue = FindViewById<TextView>(Resource.Id.textViewTimerStateValue);
            textViewSensorValue = FindViewById<TextView>(Resource.Id.textViewSensorValue);
            textViewSensorValue2 = FindViewById<TextView>(Resource.Id.textViewSensorValue2);
            kloktijd = FindViewById<TextView>(Resource.Id.kloktijd);

            // Init commandlist, scheduled by socket timer
            commandList.Add(new Tuple<string, TextView>("a", textViewSensorValue));
            commandList.Add(new Tuple<string, TextView>("b", textViewSensorValue2));
            commandList.Add(new Tuple<string, TextView>("d", toggleSchakelaar0));
            commandList.Add(new Tuple<string, TextView>("e", toggleSchakelaar1));
            commandList.Add(new Tuple<string, TextView>("f", toggleSchakelaar2));

            // timer object, running clock
            timerClock = new System.Timers.Timer() { Interval = 1000, Enabled = true }; // Interval >= 1000
            timerClock.Elapsed += (obj, args) =>
            {
                RunOnUiThread(() => {
                    textViewTimerStateValue.Text = DateTime.Now.ToString("HH:mm:ss");
                });
            };

            // timer object, check Arduino state
            // Only one command can be serviced in an timer tick, schedule from list
            timerSockets = new System.Timers.Timer() { Interval = 1000, Enabled = false }; // Interval >= 750
            timerSockets.Elapsed += (obj, args) =>
            {
                //RunOnUiThread(() =>
                //{
                    if (socket != null) // only if socket exists
                    {
                        // Send a command to the Arduino server on every tick (loop though list)
                        UpdateGUI(executeCommand(commandList[listIndex].Item1), commandList[listIndex].Item2);  //e.g. UpdateGUI(executeCommand("s"), textViewChangePinStateValue);
                        if (++listIndex >= commandList.Count) listIndex = 0;
                    }
                    else timerSockets.Enabled = false;  // If socket broken -> disable timer
                //});
            };

            //Connect met de arduino
            ConnectSocket();

            //Code voor schakelaar 1
            if (toggleSchakelaar0 != null)
            {
                toggleSchakelaar0.Click += (sender, e) =>
                {
                    socket.Send(Encoding.ASCII.GetBytes("x"));
                };
            }

            if (toggleSchakelaar1 != null)
            {
                toggleSchakelaar1.Click += (o, e) => ShowDialog(timedialog);
            }

            if (toggleSchakelaar2 != null)
            {
                toggleSchakelaar2.Click += (sender, e) =>
                {
                    socket.Send(Encoding.ASCII.GetBytes("t"));
                };
            }
        }

        // Code voor timer opdracht A
        private void UpdateDisplay()
        {
            string time = string.Format("{0}:{1}", hour, minute.ToString().PadLeft(2, '0'));
            kloktijd.Text = time;

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

        //Update de textview naar de gekozen tijd
        private void TimePickerCallback(object sender, TimePickerDialog.TimeSetEventArgs e)
        {
            hour = e.HourOfDay;
            minute = e.Minute;

            UpdateDisplay();
        }

        //Pop-up voor tijd kiezen
        protected override Dialog OnCreateDialog(int id)
        {
            if (id == timedialog)
                return new TimePickerDialog(this, TimePickerCallback, hour, minute, true);

            return null;
        }

        private void StartAlarm()
        {
            AlarmManager manager = (AlarmManager)GetSystemService(Context.AlarmService);
            Intent myIntent;
            PendingIntent pendingIntent;
            myIntent = new Intent(this, typeof(kloka));
            pendingIntent = PendingIntent.GetBroadcast(this, 0, myIntent, 0);
            manager.Set(AlarmType.ElapsedRealtimeWakeup, (SystemClock.ElapsedRealtime() + (hour1 * 3600 * 1000) + (minute1 * 60 * 1000)), pendingIntent);
            Toast.MakeText(this, "Alarm set", ToastLength.Long).Show();
        }

        //Ga naar opdracht B
        public void btnB_Click(object sender, EventArgs e)
        {
            Intent intent = new Intent(this, typeof(Opdrachtb));
            this.StartActivity(intent);
            OverridePendingTransition(Resource.Animation.Rightin, Resource.Animation.Leftout);
        }

        //Ga naar opdracht C
        public void btnC_Click(object sender, EventArgs e)
        {
            Intent intent = new Intent(this, typeof(Opdrachtc));
            this.StartActivity(intent);
            OverridePendingTransition(Resource.Animation.Rightin, Resource.Animation.Leftout);
        }

        //Send command to server and wait for response (blocking)
        //Method should only be called when socket existst
        public string executeCommand(string cmd)
        {
            byte[] buffer = new byte[4]; // response is always 4 bytes
            int bytesRead = 0;
            string result = "---";

            if (socket != null)
            {
                //Send command to server
                socket.Send(Encoding.ASCII.GetBytes(cmd));

                try //Get response from server
                {
                    //Store received bytes (always 4 bytes, ends with \n)
                    bytesRead = socket.Receive(buffer);  // If no data is available for reading, the Receive method will block until data is available,
                    //Read available bytes.              // socket.Available gets the amount of data that has been received from the network and is available to be read
                    while (socket.Available > 0) bytesRead = socket.Receive(buffer);
                    if (bytesRead == 4)
                        result = Encoding.ASCII.GetString(buffer, 0, bytesRead - 1); // skip \n
                    else result = "err";
                }
                catch (Exception exception) {
                    result = exception.ToString();
                    if (socket != null) {
                        socket.Close();
                        socket = null;
                    }
                }
            }
            return result;
        }

        //Update GUI based on Arduino response
        public void UpdateGUI(string result, TextView textview)
        {
            RunOnUiThread(() =>
            {
                if (result == "OFF") textview.SetTextColor(Color.Red);
                else if (result == " ON") textview.SetTextColor(Color.Green);
                else textview.SetTextColor(Color.Gray);  
                textview.Text = result;
            });
        }

        public void UpdateStop(char incoming)
        {
            RunOnUiThread(() =>
                {
                    if (incoming == 'x') toggleSchakelaar0.PerformClick();
                    else if (incoming == 'z') toggleSchakelaar2.PerformClick();
                });
        }

        // Connect to socket ip/prt (simple sockets)
        public void ConnectSocket()
        {
            RunOnUiThread(() =>
            {
                if (socket == null)                                       // create new socket
                {
                    try  // to connect to the server (Arduino).
                    {
                        socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                        socket.Connect(new IPEndPoint(IPAddress.Parse("192.168.0.103"), Convert.ToInt32("3300")));
                        if (socket.Connected)
                        {
                            timerSockets.Enabled = true;                //Activate timer for communication with Arduino     
                        }
                    } catch (Exception exception) {
                        timerSockets.Enabled = false;
                        if (socket != null)
                        {
                            socket.Close();
                            socket = null;
                        }
                    }
	            }
                else // disconnect socket
                {
                    socket.Close(); socket = null;
                    timerSockets.Enabled = false;
                }
            });
        }

        //Close the connection (stop the threads) if the application stops.
        protected override void OnStop()
        {
            base.OnStop();
        }

        //Close the connection (stop the threads) if the application is destroyed.
        protected override void OnDestroy()
        {
            base.OnDestroy();
        }

        //Prepare the Screen's standard options menu to be displayed.
        //public override bool OnPrepareOptionsMenu(IMenu menu)
        //{
        //   //Prevent menu items from being duplicated.
        //  menu.Clear();

        //    MenuInflater.Inflate(Resource.Menu.menu, menu);
        //    return base.OnPrepareOptionsMenu(menu);
        //}

        //Executes an action when a menu button is pressed.
        //public override bool OnOptionsItemSelected(IMenuItem item)
        //{
        //   switch (item.ItemId)
        //   {
        //      case Resource.Id.exit:
        //          //Force quit the application.
        //          System.Environment.Exit(0);
        //           return true;
        //      case Resource.Id.abort:
        //           return true;
        //   }
        // //    return base.OnOptionsItemSelected(item);
        //}
    }
}
