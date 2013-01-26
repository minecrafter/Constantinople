using System;
using System.Collections.Generic;
using System.Threading;
using Meebey.SmartIrc4net;

namespace Constantinople
{
	public class IrcBot
	{
		Dictionary<string, Logger> s = new Dictionary<string, Logger>();
		public LinkedList<string> admins = new LinkedList<string>();
		public IrcBot ()
		{
		}
		public void EnableChannel (string channel)
		{
			s.Add (channel, new Logger());
		}
		public void OnChannelMessage (object sender, IrcEventArgs e)
		{
			if (!e.Data.MessageArray [0].StartsWith ("!")) {
				if (!s.ContainsKey (e.Data.Channel))
					EnableChannel (e.Data.Channel);
				s [e.Data.Channel].AddLine ("[" + e.Data.Channel + "] <" + e.Data.Nick + "> " + e.Data.Message);
			}
			switch (e.Data.MessageArray [0]) {
			case "!startlogging":
				if (admins.Contains(e.Data.Host)) {
					s[e.Data.Channel].Disabled = false;
					s[e.Data.Channel].Clear();
					Constantinople.irc.SendMessage (SendType.Message, e.Data.Channel, "Logging enabled for " + e.Data.Channel);
				}
				break;
			case "!pauselogging":
				if (admins.Contains(e.Data.Host) && !s[e.Data.Channel].Disabled) {
					s[e.Data.Channel].Disabled = true;
					Constantinople.irc.SendMessage (SendType.Message, e.Data.Channel, "Logging paused for " + e.Data.Channel);
				}
				break;
			case "!resumelogging":
				if (admins.Contains(e.Data.Host) && s[e.Data.Channel].Disabled) {
					s[e.Data.Channel].Disabled = true;
					Constantinople.irc.SendMessage (SendType.Message, e.Data.Channel, "Logging paused for " + e.Data.Channel);
				}
				break;
			case "!stoplogging":
				if (admins.Contains(e.Data.Host)) {
					s[e.Data.Channel].Disabled = true;
					s[e.Data.Channel].Clear();
					Constantinople.irc.SendMessage (SendType.Message, e.Data.Channel, "Logging disabled for " + e.Data.Channel);
				}
				break;
			case "!pastebinlog":
				if (admins.Contains(e.Data.Host)) {
					Constantinople.irc.SendMessage (SendType.Message, e.Data.Channel, "Sending log, please wait...");
					Thread oThread = new Thread(new ThreadStart(new PastebinCaPoster(s[e.Data.Channel].ToString(), e.Data.Channel).ThreadRun));
					oThread.Start();
				}
				break;
			case "!greplog":
				using (System.IO.StringReader reader = new System.IO.StringReader(s[e.Data.Channel].ToString())) {
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