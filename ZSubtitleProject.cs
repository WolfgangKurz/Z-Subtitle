﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Grabacr07.KanColleViewer.Composition;
using Grabacr07.KanColleWrapper;
using System.Windows.Controls;

using Grabacr07.KanColleViewer.Views.Controls;
using Grabacr07.KanColleViewer.Models;

using Nekoxy;
using TrotiNet;
using System.Reflection;
using System.Threading;
using System.Runtime.InteropServices;
using System.Reactive.Linq;

using Grabacr07.KanColleWrapper.Models.Raw;

namespace ZSubtitle
{
	[Export(typeof(IPlugin))]
	[ExportMetadata("Guid", "0F4929C2-060D-40FC-B33A-1845F7A06CF2")]
	[ExportMetadata("Title", "Z-Subtitle")]
	[ExportMetadata("Description", "Z-Subtitle for KanColleViewer")]
	[ExportMetadata("Version", "1.2.1")]
	[ExportMetadata("Author", "BeerAdmiral")] // wolfgangkurzdev@gmail.com
	[ExportMetadata("AuthorURL", "http://swaytwig.com/")]
	public class ZSubtitleProject : IPlugin
	{
		internal static kcsapi_mst_shipgraph[] Shipgraph { get; private set; }
		internal SubtitlePatcher patcher { get; private set; }

		internal int ProxyPort { get; } = 40728;

		public void Initialize()
		{
			this.patcher = new SubtitlePatcher();

			KanColleClient.Current.Proxy.api_start2.TryParse<kcsapi_start2>().Subscribe(s =>
			{
				Shipgraph = s.Data.api_mst_shipgraph;
			});

			bool started = false;
			KanColleClient.Current.Proxy.SessionSource.Where(x => !started).Subscribe(x =>
				{
					started = true;
					HttpProxy.UpstreamProxyConfig = new ProxyConfig(ProxyConfigType.SpecificProxy, "localhost", ProxyPort);
					
				});

			NoCacheProxy.OnSoundRequested += (path) =>
			{
				if (!path.StartsWith("/kcs/sound/")) return;
				Subtitle(path);
			};

			var Server = new SafeTcpServer(ProxyPort, false);
			Server.Start(NoCacheProxy.CreateProxy);

			Server.InitListenFinished.WaitOne();
			if (Server.InitListenException != null)
				throw Server.InitListenException;

			new Thread(() => Translator.getVoiceLength("")).Start();
			Grabacr07.KanColleViewer.Application.Current.Exit += async (s, e) =>
			{
				Server.Shutdown();

				try
				{
					await Task.Delay(3000);
					Environment.Exit(0);
				}
				catch { }
			};
		}

		private void Subtitle(string path)
		{
			var app = Grabacr07.KanColleViewer.Application.Current;
			app.Dispatcher.Invoke(() =>
			{
				var host = WPFTool.FindElement<KanColleHost>(app.MainWindow.Content as System.Windows.UIElement);
				patcher.Prepare(host, app.Dispatcher);
			});

			new Thread(() =>
			{
				string[] part = path.Split('/');
				if (part.Length < 4) return;

				string text = "";
				switch (part[3])
				{
					case "titlecall":
						if (part.Length != 6) return;
						text = Translator.Add(DialogueType.Titlecall, part[4], part[5].Split('.')[0]);
						break;
					case "kc9999":
						if (part.Length != 5) return;
						text = Translator.Add(DialogueType.NPC, "npc", part[4].Split('.')[0]);
						break;
					default:
						if (part.Length != 5) return;
						text = Translator.Add(DialogueType.Shipgirl, part[3].Substring(2), part[4].Split('.')[0]);
						break;
				}
				if (string.IsNullOrWhiteSpace(text)) return;

				float voiceLength = Translator.getVoiceLength(text) + 1000.0f;
				patcher.Update(text, (int)(voiceLength * 1.5f));
			}).Start();
		}
	}
}
