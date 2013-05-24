using System;
using System.Collections.Generic;
using System.Threading;
using Meebey.SmartIrc4net;

namespace Constantinople
{
	public class IrcBot
	{
		Dictionary<string, Logger> logs = new Dictionary<string, Logger>();
		Dictionary<string, Voter> votes = new Dictionary<string, Voter>();
		public LinkedList<string> admins = new LinkedList<string>();
		public IrcBot ()
		{
		}
		public void EnableChannel (string channel)
		{
			logs.Add (channel, new Logger());
		}

		public void OnJoin (object sender, JoinEventArgs e)
		{
			if (!logs.ContainsKey (e.Data.Channel))
				EnableChannel (e.Data.Channel);
			logs [e.Data.Channel].AddLine (e.Data.Nick + " has joined " + e.Channel);
		}

		public void OnPart (object sender, PartEventArgs e)
		{
			if (!logs.ContainsKey (e.Data.Channel))
				EnableChannel (e.Data.Channel);
			logs [e.Data.Channel].AddLine (e.Who + " has left " + e.Channel + ": " + e.PartMessage);
		}

		public void OnKick (object sender, KickEventArgs e)
		{
			if (!logs.ContainsKey (e.Data.Channel))
				EnableChannel (e.Data.Channel);
			logs [e.Data.Channel].AddLine (e.Who + " was kicked from " + e.Channel + " by " + e.Whom + ": " + e.KickReason);
		}

		private bool VerifyArgs (string[] full, int max)
		{
			try {
				object s = full [max+1];
				return true;
			} catch (IndexOutOfRangeException e) {
				return false;
			}
		}

		public void OnChannelMessage (object sender, IrcEventArgs e)
		{
			if (!e.Data.MessageArray [0].StartsWith ("!")) {
				if (!logs.ContainsKey (e.Data.Channel))
					EnableChannel (e.Data.Channel);
				logs [e.Data.Channel].AddLine ("[" + e.Data.Channel + "] <" + e.Data.Nick + "> " + e.Data.Message);
			}
			switch (e.Data.MessageArray [0]) {
			case "!startvoting":
				if (admins.Contains(e.Data.Host) && !votes.ContainsKey(e.Data.Channel)) {
					votes.Add (e.Data.Channel, new Voter(Voter.Style.YesNo));
					Constantinople.irc.SendMessage (SendType.Message, e.Data.Channel, "Voting enabled for " + e.Data.Channel + "! Use !vote yes|no|abstain to vote.");
				}
				break;
			case "!startfreevoting":
				if (admins.Contains(e.Data.Host) && !votes.ContainsKey(e.Data.Channel)) {
					votes.Add (e.Data.Channel, new Voter(Voter.Style.FreeForm));
					Constantinople.irc.SendMessage (SendType.Message, e.Data.Channel, "Voting enabled for " + e.Data.Channel + "! Use !vote [option] to vote.");
				}
				break;
			case "!startaltvoting":
				if (admins.Contains(e.Data.Host) && !votes.ContainsKey(e.Data.Channel)) {
					votes.Add (e.Data.Channel, new Voter(Voter.Type.AlternativeVote, Voter.Style.FreeForm));
					Constantinople.irc.SendMessage (SendType.Message, e.Data.Channel, "Voting enabled for " + e.Data.Channel + "! Use !vote [option] to vote.");
				}
				break;
			case "!stopvoting":
				if (admins.Contains(e.Data.Host) && votes.ContainsKey(e.Data.Channel)) {
					Constantinople.irc.SendMessage (SendType.Message, e.Data.Channel, "Voting disabled for " + e.Data.Channel + "!");
					Constantinople.irc.SendMessage (SendType.Message, e.Data.Channel, "Result(s): " + votes[e.Data.Channel].GetHumanReadableTotal());
					votes.Remove (e.Data.Channel);
				}
				break;
			case "!vote":
				if (votes.ContainsKey(e.Data.Channel) && VerifyArgs (e.Data.MessageArray, 0)) {
					if (votes[e.Data.Channel].AddVote (e.Data.Host, e.Data.MessageArray[1].ToLower())) {
						Constantinople.irc.SendMessage (SendType.Message, e.Data.Channel, "You have voted.");
					} else {
						Constantinople.irc.SendMessage (SendType.Message, e.Data.Channel, "You have already voted, use !rmvote to remove your vote.");
					}
				}
				break;
			case "!abvote":
				if (votes.ContainsKey(e.Data.Channel) && VerifyArgs (e.Data.MessageArray, 1)) {
					if (admins.Contains (e.Data.Host) || Constantinople.irc.GetChannel (e.Data.Channel).Ops.Contains (e.Data.Nick)) {
						if (votes[e.Data.Channel].AddVote (e.Data.MessageArray[1], e.Data.MessageArray[2].ToLower())) {
							Constantinople.irc.SendMessage (SendType.Message, e.Data.Channel, "You have voted.");
						} else {
							Constantinople.irc.SendMessage (SendType.Message, e.Data.Channel, "You have already voted, use !rmvote to remove your vote.");
						}
					}
				}
				break;
			case "!rmvote":
				if (votes.ContainsKey(e.Data.Channel)) {
					if (votes[e.Data.Channel].RemoveVote (e.Data.Host)) {
						Constantinople.irc.SendMessage (SendType.Message, e.Data.Channel, "You have removed your vote.");
					} else {
						Constantinople.irc.SendMessage (SendType.Message, e.Data.Channel, "You have not voted!");
					}
				}
				break;
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
				if (VerifyArgs (e.Data.MessageArray, 0)) {
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
				if (admins.Contains(e.Data.Host) && VerifyArgs (e.Data.MessageArray, 0)) {
					string[] chans = Constantinople.irc.GetChannels();
					for (int i=0;i<chans.Length;i++) {
						Thread oThread = new Thread(new ThreadStart(new ModeThread(ModeThread.Option.Ban, e.Data.MessageArray[1], chans[i]).ThreadRun));
						oThread.Start();
					}
				}
				break;
			case "!globalunban":
				if (admins.Contains(e.Data.Host) && VerifyArgs (e.Data.MessageArray, 0)) {
					string[] chans = Constantinople.irc.GetChannels();
					for (int i=0;i<chans.Length;i++) {
						Thread oThread = new Thread(new ThreadStart(new ModeThread(ModeThread.Option.Unban, e.Data.MessageArray[1], chans[i]).ThreadRun));
						oThread.Start();
					}
				}
				break;
			}
		}
	}
}