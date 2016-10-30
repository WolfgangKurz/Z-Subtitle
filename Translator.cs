using Grabacr07.KanColleWrapper;
using Grabacr07.KanColleWrapper.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace ZSubtitle
{
	public delegate void DialogueAddedEventHandler(DialogueData d);

	public class Translator
	{
		public static Translator Instance { get; } = new Translator();

		public event DialogueAddedEventHandler DialogueAdded;

		private List<DialogueData> dialogue;
		private dynamic quotes = null;

		private static Dictionary<string, ShipInfo> shipCache = new Dictionary<string, ShipInfo>();
		private static List<int> voiceDiffs = new List<int>{
			   0, 2475, 6547, 1471, 8691, 7847, 3595, 1767, 3311, 2507,
			9651, 5321, 4473, 7117, 5947, 9489, 2669, 8741, 6149, 1301,
			7297, 2975, 6413, 8391, 9705, 2243, 2091, 4231, 3107, 9499,
			4205, 6013, 3393, 6401, 6985, 3683, 9447, 3287, 5181, 7587,
			9353, 2135, 4947, 5405, 5223, 9457, 5767, 9265, 8191, 3927,
			3061, 2805, 3273, 7331
		};
		private static List<string> voiceNames = new List<string>
		{
			"", "Intro", "Poke(1)", "Poke(2)", "Poke(3)",
			"Construction", "[Defunct]", "Return", "Ranking",
			"Equip(1)", "Equip(2)", "Docking(1)", "Docking(2)",
			"Join", "Sortie",
			"Battle", "Attack", "Yasen(2)", "Yasen(1)",
			"Damaged(1)", "Damaged(2)", "Damaged(3)", "Sunk",
			"MVP", "Wedding", "Library",
			"Equip(3)", "Supply", "Married", "Idle",
			"H0000", "H0100", "H0200", "H0300", "H0400", "H0500",
			"H0600", "H0700", "H0800", "H0900", "H1000", "H1100",
			"H1200", "H1300", "H1400", "H1500", "H1600", "H1700",
			"H1800", "H1900", "H2000", "H2100", "H2200", "H2300"
		};

		private Translator()
		{
			dialogue = new List<DialogueData>();

			WebClient client = new WebClient();
			string quotesJson = client.DownloadString("https://raw.githubusercontent.com/KC3Kai/kc3-translations/master/data/kr/quotes.json");
			quotes = JsonConvert.DeserializeObject(quotesJson);
		}

		public static string Add(DialogueType type, string identifier, string filename)
		{
			DialogueData data = new DialogueData() { Time = DateTime.Now };
			string voiceLine = null;
			string newIdentifier = "";

			switch (type)
			{
				case DialogueType.Titlecall:
					data.Ship = "Title Call";
					newIdentifier = "titlecall_" + identifier;
					voiceLine = filename;
					voiceLine = Instance.quotes[newIdentifier][voiceLine] ?? null;
					break;
				case DialogueType.NPC:
					data.Ship = "NPC";
					newIdentifier = "npc";
					voiceLine = filename;
					voiceLine = Instance.quotes[newIdentifier][voiceLine] ?? null;
					break;
				case DialogueType.Shipgirl:
					ShipInfo master;

					if (shipCache.ContainsKey(identifier))
						master = shipCache[identifier];
					else
					{
						var temp = KanColleClient.Current.Master.Ships
							.Where(kvp => ZSubtitleProject.Shipgraph.Any(x => x.api_id == kvp.Value.Id && x.api_filename == identifier))
							.Select(kvp => kvp.Value);

						if (temp.Count() == 0) break;

						master = temp.FirstOrDefault();

						lock (shipCache)
							shipCache.Add(identifier, master);
					}
					newIdentifier = master.Id.ToString();
					data.Ship = master.Name;

					if (Instance.quotes?[newIdentifier] != null)
					{
						voiceLine = getVoiceLineName(master.Id, filename);

						do
						{
							if (Instance.quotes[newIdentifier][voiceLine] != null)
							{
								voiceLine = Instance.quotes[newIdentifier][voiceLine];
								break;
							}

							var temp = KanColleClient.Current.Master.Ships
								.Where(kvp => kvp.Value.RawData.api_aftershipid == newIdentifier)
								.Select(kvp => kvp.Value);

							if (temp.Count() == 0) break;
							master = temp.FirstOrDefault();

							newIdentifier = master.Id.ToString();
							data.Ship = master.Name;
							continue;
						} while (true);
					}

					break;
				default:
					data = null;
					break;
			}
			if (data == null) return "";

			if (voiceLine != null)
				data.Line = voiceLine;
			else
				data.Line = $"unknown (\"{newIdentifier}\" : \"{filename}\")";

			lock (Instance)
				Instance.dialogue.Add(data);

			Instance.DialogueAdded?.Invoke(data);
			return data.Line;
		}
		public static int getVoiceLength(string Text)
		{
			return (int)Translator.Instance.quotes["timing"]["baseMillisVoiceLine"]
				+ (int)Translator.Instance.quotes["timing"]["extraMillisPerChar"] * Text.Replace(" ", "").Length;
		}

		private static string getVoiceLineName(int id, string filename)
		{
			int computedDiff = getVoiceDiffByFilename(id, filename);
			if (computedDiff < 0) return filename;

			return voiceNames[computedDiff];
		}

		private static int getVoiceDiffByFilename(int id, string filename)
		{
			int number;
			bool success;
			success = int.TryParse(filename, out number);
			if (!success) throw new ArgumentException("Sound filename is not a number!");

			int k = (id + 7) * 17;
			int r = number - 100000;

			for (int i = 0; i < 1000; ++i)
			{
				int a = r / k;

				if (r % k == 0)
				{
					for (int j = 0; j < voiceDiffs.Count; j++)
						if (voiceDiffs[j] == a) return j;
				}

				r += 99173;
			}
			return -1;
		}
	}

	public enum DialogueType
	{
		Titlecall,
		NPC,
		Shipgirl
	}

	public class DialogueData
	{
		public DateTime Time { get; set; }
		public string Ship { get; set; }
		public string Line { get; set; }
	}
}
