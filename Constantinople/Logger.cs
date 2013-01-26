using System;
using System.Text;

namespace Constantinople
{
	public class Logger
	{
		// Fancy wrapper ontop of StringBuilders... buffers?
		StringBuilder buff;
		public bool Disabled = true;
		public Logger ()
		{
			buff = new StringBuilder();
		}
		public void AddLine (string msg)
		{
			if (!Disabled)
				buff.AppendLine("[" + System.DateTime.Now.ToString("U") + "] " + msg);
		}
		public string ToString ()
		{
			return buff.ToString ();
		}
		public void Clear ()
		{
			buff.Clear ();
		}
	}
}

