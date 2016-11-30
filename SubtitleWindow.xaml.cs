using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Threading;
using System.ComponentModel.Composition;
using System.Windows.Media;
using System.Windows.Controls;

using Grabacr07.KanColleViewer.Views.Controls;

namespace ZSubtitle
{
	/// <summary>
	/// SubtitleWindow.xaml에 대한 상호 작용 논리
	/// </summary>
	public partial class SubtitleWindow : Window
	{
		private float _TimeAlpha;
		public float TimeAlpha
		{
			get { return this._TimeAlpha; }
			private set
			{
				this._TimeAlpha = value;
				this.RootLayout.Opacity = Math.Min(
					1.0,
					TimeAlpha
				) * double.Parse((string)ZSettings.Get("Opacity") ?? "0.9");
			}
		}

		private KanColleHost ParentHost;
		private WebBrowser ParentBrowser => ParentHost?.WebBrowser;

		private IntPtr Handle
		{
			get
			{
				var _helper = new WindowInteropHelper(this);
				IntPtr _Handle = _helper?.Handle ?? IntPtr.Zero;
				return _Handle;
			}
		}
		private DispatcherTimer Timer { get; set; }

		public SubtitleWindow(KanColleHost Parent)
		{
			this.ParentHost = Parent;

			InitializeComponent();

			Type type = typeof(ZSubtitle.ZSubtitleProject);
			var attrs = type.GetCustomAttributes(typeof(ExportMetadataAttribute), true) as ExportMetadataAttribute[];
			var attr = attrs.FirstOrDefault(x => x.Name == "Version");
			string ver = attr.Value as string;
			ver = string.Format(
				"Z-Subtitle v{0}, 2016-11-01 updated",
				string.IsNullOrEmpty(ver) ? "Unknown" : ver
			);

			ParentBrowser.SizeChanged += (s, e) => PrepareDrawing();
			Grabacr07.KanColleViewer.Application.Current.MainWindow.Closing += (s, e) => this.Close();
			Grabacr07.KanColleViewer.Application.Current.Exit += (s, e) => this.Close();

			this.Timer = new DispatcherTimer();
			this.Timer.Interval = TimeSpan.FromMilliseconds(30);
			this.Timer.Tick += new EventHandler((s, e) =>
			{
				this.PositionPatch();

				if (TimeAlpha > 0)
					TimeAlpha = Math.Max(0, TimeAlpha - 0.03f);
			});
			this.Timer.Start();

			this.Show();
			this.RenderText(ver, 5.0f);
			PrepareDrawing();
		}

		private void PositionPatch()
		{
			var version = Environment.OSVersion.Version;
			if((version.Major == 6 && version.Minor >= 2) || version.Major > 6)
			{
				// Windows 8 or over
				Win32.MoveTo(this.Handle, 0, 0);
			}
			else
			{
				// Windows 7 or below
				Point pt = ParentBrowser.PointFromScreen(new Point(0, 0));
				Win32.MoveTo(this.Handle, -(int)pt.X, -(int)pt.Y);
			}
		}
		private void WindowPatch()
		{
			this.WindowState = WindowState.Maximized;

			var helper = new WindowInteropHelper(this);
			IntPtr Handle = helper.Handle;
			Win32.SetParentWindow(Handle, ParentBrowser.Handle);
			Win32.ClickThrough(Handle);
			Win32.SetTopMost(Handle);
			this.PositionPatch();
		}

		private void PrepareDrawing()
		{
			FontFamily FontFamily = new FontFamily(ZSettings.Get("FontName")?.ToString() ?? "MalgunGothic");
			FontWeight FontWeight = bool.Parse((string)ZSettings.Get("FontBold") ?? "true") ? FontWeights.Bold : FontWeights.Regular;

			double FontSize = double.Parse((string)ZSettings.Get("FontSIze") ?? "22.0");
			FontSize *= ParentBrowser.Width / 800;

			this.SubtitleText.FontFamily = FontFamily;
			this.SubtitleText.FontSize = FontSize;
			this.SubtitleText.FontWeight = FontWeight;

			this.Width = ParentBrowser.Width;
			this.Height = ParentBrowser.Height;
			this.WindowPatch();
		}

		public void RenderText(string text, float Time)
		{
			PrepareDrawing();

			this.SubtitleText.Text = text;
			this.TimeAlpha = Time;
		}
	}
}
