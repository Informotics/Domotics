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
    [Activity(Label = "@string/application_name", MainLauncher = false, Theme = "@style/Theme.Yellow",  Icon = "@drawable/icon")]
    public class Opdrachtsettings : Activity
    {

       
    
        Button A;
        Button B;
        Button C;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
           
            //statusbar settings
            this.Title = "Settings";
            this.Window.AddFlags(WindowManagerFlags.DrawsSystemBarBackgrounds);

            SetContentView(Resource.Layout.settings);

            A = FindViewById<RadioButton>(Resource.Id.radioButton1);
            B = FindViewById<RadioButton>(Resource.Id.radioButton2);
            C = FindViewById<RadioButton>(Resource.Id.radioButton3);

            ISharedPreferences prefs = Application.Context.GetSharedPreferences("PREF_NAME", FileCreationMode.Private);
            var pagina = prefs.GetInt("pagina", 0);
            switch (pagina)
            {
                case 2:
                    B.PerformClick();
                    break;
                case 3:
                    C.PerformClick();
                    break;
                default:
                    A.PerformClick();
                    break;
            }



            var value1 = prefs.GetInt("pagina", 0);
            if (A != null)
            {
                A.Click += (sender, e) =>
                {
                    Toast.MakeText(this, "A geselecteerd", ToastLength.Short).Show();
                    ISharedPreferencesEditor editor = prefs.Edit();
                    editor.PutInt("pagina", 1);
                    editor.Apply();
                };
            }
            if (B != null)
            {
                B.Click += (sender, e) =>
                {
                    Toast.MakeText(this, "B geselecteerd", ToastLength.Short).Show();
                    ISharedPreferencesEditor editor = prefs.Edit();
                    editor.PutInt("pagina", 2);
                    editor.Apply();
                };
            }
            if (C != null)
            {
                C.Click += (sender, e) =>
                {
                    Toast.MakeText(this, "C geselecteerd", ToastLength.Short).Show();
                    ISharedPreferencesEditor editor = prefs.Edit();
                    editor.PutInt("pagina", 3);
                    editor.Apply();
                };
            }
        }
    }
}