using System;
using System.IO;
using System.Collections.Generic;
using System.Threading;
using Meebey.SmartIrc4net;

namespace Constantinople
{
	class Constantinople
	{
		public static IrcClient irc = new IrcClient();
		public static void Main (string[] args)
		{
			Console.WriteLine ("Constantinople IRC Bot (or was it Istanbul?)");
			Console.WriteLine ("Setting up IRC bot...");
			irc.Encoding = System.Text.Encoding.UTF8;
			irc.SendDelay = 200;
			irc.ActiveChannelSyncing = true;
			IrcBot handler = new IrcBot ();
			irc.OnChannelMessage += handler.OnChannelMessage;
			// INI setup
			Dictionary<string, string> dict = new Dictionary<string, string> ();
			string[] lines = File.ReadAllLines (args [0]);
			foreach (var s in lines) {
				var split = s.Split ('=');
				dict.Add (split [0], split [1]);
			}
			string admin = dict ["admins"];
			if (admin.Contains (",")) {
				string[] admins = admin.Split (',');
				for (int i=0; i<admins.Length; i++) {
					//Thread.Sleep (500);
					Console.WriteLine ("Adding admin " + admins[i]);
					handler.admins.AddLast (admins [i]);
				}
			} else {
				Console.WriteLine ("Adding admin " + admin);
				handler.admins.AddLast (admin);
			}
			try {
				Console.WriteLine ("Connecting...");
				irc.Connect (dict["server"], 6667);
				irc.Login (dict["nickname"], dict["nickname"]);
				string chan = dict["channels"];
				if (chan.Contains (",")) {
					string[] chans = chan.Split (',');
					for (int i=0; i<chans.Length; i++) {
						Console.WriteLine ("Joining " + chans[i]);
						irc.RfcJoin (chans[i]);
					}
				} else {
					Console.WriteLine ("Joining " + chan);
					irc.RfcJoin (chan);
				}
				irc.SendMessage (SendType.Message, "NickServ", "identify " + dict["nickservpass"]);
				irc.Listen ();
				irc.Disconnect ();
			} catch (ConnectionException e) {
				return;
			}
		}
	}
}
