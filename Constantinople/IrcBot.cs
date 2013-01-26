using System;
using System.Collections.Generic;
using System.Threading;
using Meebey.SmartIrc4net;

namespace Constantinople
{
	public class IrcBot
	{
		Dictionary<string, Logger> logs = new Dictionary<string, Logger>();
		public LinkedList<string> admins = new LinkedList<string>();
		public IrcBot ()
		{
		}
		public void EnableChannel (string channel)
		{
			logs.Add (channel, new Logger());
		}
		public void OnChannelMessage (object sender, IrcEventArgs e)
		{
			if (!e.Data.MessageArray [0].StartsWith ("!")) {
				if (!logs.ContainsKey (e.Data.Channel))
					EnableChannel (e.Data.Channel);
				logs [e.Data.Channel].AddLine ("[" + e.Data.Channel + "] <" + e.Data.Nick + "> " + e.Data.Message);
			}
			switch (e.Data.MessageArray [0]) {
			case "!startlogging":
				if (admins.Contains(e.Data.Host) && logs[e.Data.Channel].Disabled) {
					logs[e.Data.Channel].Disabled = false;
					logs[e.Data.Channel].Clear();
					Constantinople.irc.SendMessage (SendType.Message, e.Data.Channel, "Logging enabled for " + e.Data.Channel);
				}
				break;
			case "!clearlog":
				if (admins.Contains(e.Data.Host)) {
					logs[e.Data.Channel].Clear();
					Constantinople.irc.SendMessage (SendType.Message, e.Data.Channel, "Log cleared for " + e.Data.Channel);
				}
				break;
			case "!resumelogging":
				if (admins.Contains(e.Data.Host) && logs[e.Data.Channel].Disabled) {
					logs[e.Data.Channel].Disabled = true;
					Constantinople.irc.SendMessage (SendType.Message, e.Data.Channel, "Logging resumed for " + e.Data.Channel);
				}
				break;
			case "!stoplogging":
				if (admins.Contains(e.Data.Host) && !logs[e.Data.Channel].Disabled) {
					logs[e.Data.Channel].Disabled = true;
					Constantinople.irc.SendMessage (SendType.Message, e.Data.Channel, "Logging stopped for " + e.Data.Channel + ". Use !resumelogging to resume it, or !clearlog to clear the log.");
				}
				break;
			case "!pastebinlog":
				if (admins.Contains(e.Data.Host)) {
					Constantinople.irc.SendMessage (SendType.Message, e.Data.Channel, "Sending log, please wait...");
					Thread oThread = new Thread(new ThreadStart(new PastebinCaPoster(logs[e.Data.Channel].ToString(), e.Data.Channel).ThreadRun));
					oThread.Start();
				}
				break;
			case "!greplog":
				using (System.IO.StringReader reader = new System.IO.StringReader(logs[e.Data.Channel].ToString())) {
					bool read=true;
					while(read) {
						string s2 = reader.ReadLine();
						if(s2 == null || !s2.StartsWith("[")) {
							read=false;
						}
						if(read && s2.ToLower().Contains(e.Data.MessageArray[1].ToLower())) {
							Constantinople.irc.SendMessage (SendType.Message, e.Data.Channel, s2);
						}
					}
				}
				break;
			case "!globalnotice":
				if (admins.Contains(e.Data.Host)) {
					string[] chans = Constantinople.irc.GetChannels();
					for (int i=0;i<chans.Length;i++) {
						Constantinople.irc.SendMessage(SendType.Notice, chans[i], "4GLOBAL NOTICE (from " + e.Data.Nick + "): 14" + e.Data.Message.Replace("!globalnotice ", ""));
					}
				}
				break;
			case "!globalban":
				if (admins.Contains(e.Data.Host)) {
					string[] chans = Constantinople.irc.GetChannels();
					for (int i=0;i<chans.Length;i++) {
						Thread oThread = new Thread(new ThreadStart(new ModeThread(ModeOption.Ban, e.Data.MessageArray[1], chans[i]).ThreadRun));
						oThread.Start();
					}
				}
				break;
			case "!globalunban":
				if (admins.Contains(e.Data.Host)) {
					string[] chans = Constantinople.irc.GetChannels();
					for (int i=0;i<chans.Length;i++) {
						Thread oThread = new Thread(new ThreadStart(new ModeThread(ModeOption.Unban, e.Data.MessageArray[1], chans[i]).ThreadRun));
						oThread.Start();
					}
				}
				break;
			}
		}
	}
}