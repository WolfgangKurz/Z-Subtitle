using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Runtime.InteropServices;

namespace ZSubtitle
{
	internal class Win32
	{
		private enum GWL
		{
			Style = -16,
			ExStyle = -20
		}
		private enum WS
		{
			Child = 0x40000000
		}
		private enum WS_EX
		{
			Transparent = 0x20,
			ToolWindow = 0x80,
			Layered = 0x80000,
			NoActivate = 0x8000000
		}
		private enum SWP
		{
			NoSize = 0x0001,
			NoMove = 0x0002,
			NoRedraw = 0x008,
			ShowWindow = 0x0040
		}

		[DllImport("user32.dll", SetLastError = true)]
		private static extern IntPtr SetParent(IntPtr hWndChild, IntPtr hWndNewParent);

		[DllImport("user32.dll", EntryPoint = "GetWindowLongPtr")]
		private static extern int GetWindowLong(IntPtr hWnd, GWL nIndex);
		[DllImport("user32.dll", EntryPoint = "SetWindowLongPtr")]
		private static extern int SetWindowLong(IntPtr hWnd, GWL nIndex, int dwNewLong);

		[DllImport("user32.dll")]
		private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, SWP uFlags);
		private static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);

		public static void ClickThrough(IntPtr Handle)
		{
			WS_EX wl = (WS_EX)GetWindowLong(Handle, GWL.ExStyle);
			wl = wl | WS_EX.Transparent | WS_EX.NoActivate | WS_EX.ToolWindow;
			SetWindowLong(Handle, GWL.ExStyle, (int)wl);
		}
		public static void SetParentWindow(IntPtr Child, IntPtr Parent)
		{
			SetParent(Child, Parent);

			WS wl = (WS)GetWindowLong(Child, GWL.Style);
			wl = wl | WS.Child;
			SetWindowLong(Child, GWL.Style, (int)wl);
		}
		public static void SetTopMost(IntPtr Handle)
		{
			SetWindowPos(Handle, HWND_TOPMOST, 0, 0, 0, 0, SWP.NoMove | SWP.NoSize | SWP.NoRedraw);
		}
		public static void MoveTo(IntPtr Handle, int X, int Y)
		{
			SetWindowPos(Handle, IntPtr.Zero, X, Y, 0, 0, SWP.NoSize);
		}
		public static void SetTopMostMove(IntPtr Handle, int X, int Y)
		{
			SetWindowPos(Handle, HWND_TOPMOST, X, Y, 0, 0, SWP.NoSize);
		}
	}
}
