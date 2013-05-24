using System;
using System.Threading;
using Meebey.SmartIrc4net;

namespace Constantinople
{
	public class ModeThread
	{
		public enum Option
		{
			Ban,
			Unban,
			Kick
		}
		private Option mode;
		private string mask;
		private string chan;
		public ModeThread (ModeOption m, string ma, string c)
		{
			mode = m;
			mask = ma;
			chan = c;
		}
		public void ThreadRun ()
		{
			// Set op on ourselves if we need to
			bool shouldRmv = false;
			if (!Constantinople.irc.GetChannel (chan).Ops.Contains (Constantinople.irc.Nickname)) {
				shouldRmv = true;
				Constantinople.irc.SendMessage (SendType.Message, "ChanServ", "op " + chan);
				Thread.Sleep (200); // 200 milliseconds to get op
			}
			switch (mode) {
			case Option.Ban:
				Constantinople.irc.Ban (chan, mask);
				break;
			case Option.Unban:
				Constantinople.irc.Unban (chan, mask);
				break;
			case Option.Kick:
				Constantinople.irc.RfcKick (chan, mask);
				break;
			}
			// TODO: Match hostmasks, and kick users if they match. (Regex?)
			// If we had to get op, remove it
			if (shouldRmv) {
				Constantinople.irc.SendMessage (SendType.Message, "ChanServ", "deop " + chan);
			}
		}
	}
}

