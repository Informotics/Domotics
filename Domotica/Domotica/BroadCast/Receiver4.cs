using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.Support.V7.App;
using System.Net.Sockets;

namespace Domotica.BroadCast
{
    [BroadcastReceiver(Enabled = true)]
    public class Receiver4 : BroadcastReceiver
    {
        public override void OnReceive(Context context, Intent intent)
        {
            MainActivity.socket.Send(Encoding.ASCII.GetBytes("y"));
        }
    }
}