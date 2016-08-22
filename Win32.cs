using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace ZSubtitle
{
    internal class Win32
    {
        private enum GWL
        {
            ExStyle = -20
        }

        private enum WS_EX
        {
            Transparent = 0x20,
            Layered = 0x80000
        }

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr SetParent(IntPtr hWndChild, IntPtr hWndNewParent);

        [DllImport("user32.dll", EntryPoint = "GetWindowLong")]
        private static extern int GetWindowLong(IntPtr hWnd, GWL nIndex);

        [DllImport("user32.dll", EntryPoint = "SetWindowLong")]
        private static extern int SetWindowLong(IntPtr hWnd, GWL nIndex, int dwNewLong);

        [DllImport("gdi32.dll", SetLastError = true)]
        private static extern IntPtr CreateCompatibleDC(IntPtr hdc);

        [DllImport("user32.dll")]
        private static extern IntPtr GetDC(IntPtr hWnd);

        [DllImport("gdi32.dll", SetLastError = true)]
        public static extern IntPtr SelectObject(IntPtr hdc, IntPtr hgdiobj);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);

        [DllImport("gdi32.dll", SetLastError = true)]
        public static extern int DeleteObject(IntPtr hobject);

        [DllImport("gdi32.dll", SetLastError = true)]
        public static extern int DeleteDC(IntPtr hdc);

        [DllImport("user32.dll", ExactSpelling = true, SetLastError = true)]
        private static extern bool UpdateLayeredWindow(IntPtr hwnd, IntPtr hdcDst, IntPtr pptDst, ref Size psize, IntPtr hdcSrc, ref Point pptSrc, uint crKey, [In] ref BLENDFUNCTION pblend, uint dwFlags);

        [DllImport("user32.dll", EntryPoint = "SetLayeredWindowAttributes")]
        private static extern bool SetLayeredWindowAttributes(IntPtr hWnd, int crKey, byte alpha, int dwFlags);

        [StructLayout(LayoutKind.Sequential)]
        private struct BLENDFUNCTION
        {
            public const int AC_SRC_OVER = 0x00;
            public const int AC_SRC_ALPHA = 0x01;

            public byte BlendOp;
            public byte BlendFlags;
            public byte SourceConstantAlpha;
            public byte AlphaFormat;

            public BLENDFUNCTION(byte op, byte flags, byte alpha, byte format)
            {
                BlendOp = op;
                BlendFlags = flags;
                SourceConstantAlpha = alpha;
                AlphaFormat = format;
            }
        }

        public static void ClickThrough(IntPtr Handle)
        {
            WS_EX wl = (WS_EX)GetWindowLong(Handle, GWL.ExStyle);
            wl = wl | WS_EX.Layered | WS_EX.Transparent;

            SetWindowLong(Handle, GWL.ExStyle, (int)wl);
            // SetLayeredWindowAttributes(Handle, 0, 255, 2);
        }
        public static void UpdateScreen(IntPtr Handle, Bitmap buffer, Size sizeDest)
        {
            BLENDFUNCTION bf = new BLENDFUNCTION(BLENDFUNCTION.AC_SRC_OVER, 0, 255, BLENDFUNCTION.AC_SRC_ALPHA);

            IntPtr hScreenDC = GetDC(IntPtr.Zero);

            IntPtr hMemDC = CreateCompatibleDC(hScreenDC);
            IntPtr hBitmap = IntPtr.Zero;
            IntPtr hOldBitmap = IntPtr.Zero;

            using (Graphics g = Graphics.FromImage(buffer))
            {
                Size szDest = sizeDest;
                Point ptSrc = Point.Empty;

                try
                {
                    hBitmap = buffer.GetHbitmap(Color.FromArgb(0));
                    hOldBitmap = SelectObject(hMemDC, hBitmap);

                    UpdateLayeredWindow(Handle, hScreenDC, IntPtr.Zero, ref szDest, hMemDC, ref ptSrc, 0, ref bf, 2);
                }
                finally
                {
                    if (hScreenDC != IntPtr.Zero) ReleaseDC(IntPtr.Zero, hScreenDC);
                    if (hBitmap != IntPtr.Zero)
                    {
                        SelectObject(hMemDC, hOldBitmap);
                        DeleteObject(hBitmap);
                    }
                    if (hMemDC != IntPtr.Zero) DeleteDC(hMemDC);
                }
            }
            ReleaseDC(IntPtr.Zero, hScreenDC);
        }

        public static void SetParentWindow(IntPtr child, IntPtr parent)
        {
            SetParent(child, parent);
        }
    }
}
