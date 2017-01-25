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
            //NotificationCompat.Builder builder = new NotificationCompat.Builder(context);

            MainActivity.socket.Send(Encoding.ASCII.GetBytes("y"));

            //NotificationManager manager = (NotificationManager)context.GetSystemService(Context.NotificationService);
            //manager.Notify(1, builder.Build());
        }
    }
}