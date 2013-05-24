using System;
using System.Collections;
using System.Collections.Generic;

namespace Constantinople
{
	public class Voter
	{
		public enum Style {
			FreeForm,
			YesNo
		}
		public enum Type {
			AlternativeVote,
			FirstPastThePost
		}
		Type t = Type.FirstPastThePost;
		Style st = Style.FreeForm;
		Dictionary<string, LinkedList<string>> votes = new Dictionary<string, LinkedList<string>>();
		public Voter ()
		{
		}
		public Voter (Voter.Type type)
		{
			t = type;
		}
		public Voter (Voter.Style style)
		{
			st = style;
		}
		public Voter (Voter.Type type, Voter.Style style)
		{
			t = type;
			st = style;
		}
		public bool AddVote (string hostname, string choice)
		{
			if (!votes.ContainsKey (hostname)) {
				if (st == Style.YesNo) {
					switch(choice) {
					case "for":
					case "against":
					case "abstain":
						break;
					default:
						return false;
					}
				}
				LinkedList<String> tmp = new LinkedList<string>();
				tmp.AddLast(choice);
				votes.Add (hostname, tmp);
				return true;
			}
			return false;
		}
		public bool AddVote (string hostname, string[] choice)
		{
			if (t == Type.FirstPastThePost) {
				return AddVote (hostname, choice[0]);
			}
			if (!votes.ContainsKey (hostname)) {
				LinkedList<String> tmp = new LinkedList<string>();
				for (int i = 0; i < choice.Length; i++) {
					tmp.AddLast(choice[i]);
				}
				votes.Add (hostname, tmp);
				return true;
			}
			return false;
		}
		public bool RemoveVote (string hostname)
		{
			return votes.Remove (hostname);
		}
		public bool HasVoted (string hostname)
		{
			return votes.ContainsKey (hostname);
		}

		public Dictionary<string, int> DoFTTPVote (Dictionary<string, LinkedList<string>> votes)
		{
			IEnumerator voteEnum = votes.Values.GetEnumerator();
			Dictionary<string, int> n = new Dictionary<string, int>();
			while (voteEnum.MoveNext()) {
				IEnumerator choice2 = ((LinkedList<string>)voteEnum.Current).GetEnumerator();
				while (choice2.MoveNext()) {
					if(n.ContainsKey ((string)choice2.Current)) {
						n[(string)choice2.Current] += 1;
					} else {
						n.Add((string)choice2.Current, 1);
					}
				}
			}
			return n;
		}

		public Dictionary<string, int> DoFTTPVote ()
		{
			return DoFTTPVote(votes);
		}

		public string AlternativeVote ()
		{
			bool done = false;
			Dictionary<string, int> tmp1 = new Dictionary<string, int>();
			Dictionary<string, LinkedList<string>> tmp2 = new Dictionary<string, LinkedList<string>>();
			int tmp3 = 0;
			string tmp4 = "";
			Dictionary<string, LinkedList<string>> tmp5 = new Dictionary<string, LinkedList<string>>();
			LinkedList<string> tmp6 = new LinkedList<string>();
			tmp2 = votes;
			while (!done)
			{
				tmp1 = DoFTTPVote (tmp2);
				// Sort by least popular choice
				while (tmp1.GetEnumerator().MoveNext())
				{
					if (tmp3 > tmp1.GetEnumerator().Current.Value)
					{
						tmp3 = tmp1.GetEnumerator().Current.Value;
						tmp4 = tmp1.GetEnumerator().Current.Key;
					}
				}
				if (tmp1.Count == 1)
				{
					// We're done.
					done = true;
					break;
				}
				// Now eliminate the least popular choice, unless it is the only choice left.
				while (tmp2.GetEnumerator().MoveNext())
				{
					tmp6 = tmp2.GetEnumerator().Current.Value;
					tmp6.Remove(tmp4);
					tmp5.Add (tmp2.GetEnumerator().Current.Key, tmp6);
				}
				tmp2 = tmp5;
			}
			return tmp4;
		}
		public string GetHumanReadableTotal ()
		{
			string s = "";
			switch (t) {
			case Type.AlternativeVote:
				// We're not done yet!
				// Let's eliminate the least popular choice
				// TODO: Implement!
				s = AlternativeVote();
				break;
			case Type.FirstPastThePost:
				Dictionary<string, int> n = DoFTTPVote();
				IEnumerator nameEnum = n.Keys.GetEnumerator();
				while (nameEnum.MoveNext()) {
					s += (string)nameEnum.Current + ": " + n[(string)nameEnum.Current] + " ";
				}
				break;
			}
			return s.Trim();
		}
	}
}

