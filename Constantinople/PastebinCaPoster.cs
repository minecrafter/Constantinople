using System;
using System.Collections.Specialized;
using System.Net;
using System.Text;

namespace Constantinople
{
	public class PastebinCaPoster
	{
		string text;
		string chan;
		public PastebinCaPoster (string msg, string c)
		{
			text = msg;
			chan = c;
		}

		public void ThreadRun( )
		{
			using (var wb = new WebClient())
			{
    			var data = new NameValueCollection();
    			data["content"] = text;
    			data["description"] = "Log until " + System.DateTime.Now.ToString("U");
				data["type"] = "1";
				data["expiry"] = "Never";
				data["name"] = "Constantinople";

    			var response = wb.UploadValues("http://pastebin.ca/quiet-paste.php?api=YAINjJZQQ2YDuxvUBZ+inZPOKfcbROJd", "POST", data);
				string resp = Encoding.ASCII.GetString(response);
				if(resp.StartsWith("SUCCESS:")) {
					Constantinople.irc.SendMessage(Meebey.SmartIrc4net.SendType.Message, this.chan, "Successfully pasted! " + resp.Replace("SUCCESS:", "http://pastebin.ca/"));
				} else {
					Constantinople.irc.SendMessage(Meebey.SmartIrc4net.SendType.Message, this.chan, "Unable to paste! " + resp);
				}
			}
		}
	}
}

