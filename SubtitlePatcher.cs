using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using Grabacr07.KanColleViewer.Views.Controls;
using Grabacr07.KanColleViewer;
using Grabacr07.KanColleWrapper;
using mshtml;
using Dispatcher = System.Windows.Threading.Dispatcher;

namespace ZSubtitle
{
	internal class SubtitlePatcher
	{
		private WebBrowser browser { get; set; }
		private IHTMLElement layer { get; set; }

		private Dispatcher dispatcher { get; set; }
		private bool Prepared { get; set; } = false;

		public void Prepare(KanColleHost host, Dispatcher dispatcher)
		{
			if (Prepared) return;
			this.Prepared = true;

			this.dispatcher = dispatcher;
			this.dispatcher.Invoke(() =>
			{
				try
				{
					this.browser = host.WebBrowser;

					var document = browser.Document as HTMLDocument;
					if (document == null) return;

					var gameFrame = document.getElementById("game_frame");
					if (gameFrame == null)
					{
						if (document.url.Contains(".swf?"))
							gameFrame = document.body;
					}

					var target = gameFrame?.document as HTMLDocument;
					if (target != null) {
						target.createStyleSheet().cssText = "#zsubtitle_display { position:fixed; left:10px; right:10px; bottom:10px; text-align:center; z-index:999999;"
														+ " text-shadow:0 0 3px #000, 0 0 4px #000, 0 0 6px #000, 0 1px 11px #000; word-break:keep-all;"
														+ " font-family:'MalgunGothic',sans-serif; font-size:22px; color:#fff; pointer-events:none !important }";

						layer = target.createElement("div");
						layer.id = "zsubtitle_display";
						target.appendChild(layer as IHTMLDOMNode);
					}
				}
				catch (Exception ex)
				{
					Debug.WriteLine(ex);
					return;
				}
			});
		}
		public void Update(string Text, int Time)
		{
			if (layer == null) return;

			this.dispatcher.Invoke(async () =>
			{
				try
				{
					layer.innerHTML = Text;
					layer.style.setAttribute("transition", "");
					layer.style.setAttribute("opacity", 1);

					await Task.Delay(Time);

					layer.style.setAttribute("transition", "opacity 2s linear");
					layer.style.setAttribute("opacity", 0);
				}
				catch (Exception ex)
				{
					Debug.WriteLine(ex);
					return;
				}
			});
		}
	}
}
