using System;
using System.Threading;
using Meebey.SmartIrc4net;

namespace Constantinople
{
	public class ModeThread
	{
		private ModeOption mode;
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
				Thread.Sleep (50); // 50 milliseconds to get op
			}
			switch (mode) {
			case ModeOption.Ban:
				Constantinople.irc.Ban (chan, mask);
				break;
			case ModeOption.Unban:
				Constantinople.irc.Unban (chan, mask);
				break;
			}
			// If we had to get op, remove it
			if (shouldRmv) {
				Constantinople.irc.SendMessage (SendType.Message, "ChanServ", "deop " + chan);
			}
		}
	}
}

