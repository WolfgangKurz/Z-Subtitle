using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Windows.Forms;

namespace ZSubtitle
{
    public partial class frmText : Form
    {
        private Bitmap buffer = null;
        private float timeAlpha = 1.0f;

        private System.Windows.Controls.WebBrowser ParentBrowser;

        // System.Windows.Forms.WebBrowser -> Windows Form WebBrowser
        // System.Windows.Controls.WebBrowser -> WPF WebBrowser
        public frmText(System.Windows.Controls.WebBrowser Parent)
        {
            InitializeComponent();
            Win32.ClickThrough(this.Handle);

            this.Location = new Point(0, 0);
            this.WindowState = FormWindowState.Maximized;

            this.ParentBrowser = Parent;
            Win32.SetParentWindow(this.Handle, Parent.Handle);

            Parent.SizeChanged += (s, e) => ResizeToParent();
            this.Resize += (s, e) => ResizeToParent();
            ResizeToParent();
        }
        private void ResizeToParent()
        {
            this.Size = new System.Drawing.Size(
                (int)ParentBrowser.Width,
                (int)ParentBrowser.Height
            );
            RenderScreen();
        }

        private string TextToDraw { get; set; }
        public void RenderText(string text, float Time)
        {
            this.TextToDraw = text;
            this.timeAlpha = Time;
        }

        private void RenderScreen()
        {
            Bitmap secondBuffer = new Bitmap(800, 480, PixelFormat.Format32bppArgb);

            using (Graphics g = Graphics.FromImage(secondBuffer))
            {
                g.SmoothingMode = SmoothingMode.HighQuality;
                g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SingleBitPerPixelGridFit;
                g.Clear(Color.FromArgb(0, 0, 0, 0));

                if (!string.IsNullOrEmpty(this.TextToDraw))
                {
                    var f = new Font(
                        new FontFamily(ZSettings.Get("FontName")?.ToString() ?? "맑은 고딕"),
                        ZSettings.Get<float>("FontSIze").GetValueOrDefault(16.0f),
                        ZSettings.Get<bool>("FontBold").GetValueOrDefault(true) ? FontStyle.Bold : FontStyle.Regular
                    );

                    SizeF sz = g.MeasureString(TextToDraw, f, 800);
                    sz.Width += 10;
                    Rectangle rc = new Rectangle(
                        (int)(800 / 2 - sz.Width / 2) + 8,
                        (int)(480 - sz.Height - 5),
                        (int)sz.Width - 16,
                        (int)sz.Height * 2
                    );

                    var format = StringFormat.GenericTypographic;
                    format.Alignment = StringAlignment.Center;

                    var x = GetStringPath(this.TextToDraw, g.DpiY, rc, f, format);

                    g.DrawPath(new Pen(Color.FromArgb(0xD6, 0, 0, 0), 2.83f), x);
                    g.FillPath(Brushes.White, x);
                }
            }

            buffer = new Bitmap(this.ClientSize.Width, this.ClientSize.Height, PixelFormat.Format32bppArgb);
            using (Graphics g = Graphics.FromImage(buffer))
            {
                g.SmoothingMode = SmoothingMode.HighQuality;
                g.InterpolationMode = InterpolationMode.HighQualityBilinear;
                g.Clear(Color.FromArgb(0, 0, 0, 0));

                float a = timeAlpha > 1.0f ? 1.0f : timeAlpha;
                a *= ZSettings.Get<float>("Opacity").GetValueOrDefault(0.9f);
                float[][] matrixItems ={
                    new float[] {1, 0, 0, 0, 0},
                    new float[] {0, 1, 0, 0, 0},
                    new float[] {0, 0, 1, 0, 0},
                    new float[] {0, 0, 0, a, 0},
                    new float[] {0, 0, 0, 0, 1}
                };
                ColorMatrix colorMatrix = new ColorMatrix(matrixItems);

                ImageAttributes attr = new ImageAttributes();
                attr.SetColorMatrix(colorMatrix, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);

                g.DrawImage(
                    secondBuffer,
                    this.ClientRectangle,
                    0, 0, secondBuffer.Width, secondBuffer.Height,
                    GraphicsUnit.Pixel,
                    attr
                );
            }
            Win32.UpdateScreen(this.Handle, buffer, this.ClientSize);
        }

        GraphicsPath GetStringPath(string s, float dpi, RectangleF rect, Font font, StringFormat format)
        {
            GraphicsPath path = new GraphicsPath();
            float emSize = dpi * font.SizeInPoints / 72;
            path.AddString(s, font.FontFamily, (int)font.Style, emSize, rect, format);
            return path;
        }

        private void tmrAlpha_Tick(object sender, EventArgs e)
        {
            if (timeAlpha > 0)
            {
                timeAlpha -= 0.05f;
                ResizeToParent();
            }
        }
    }
}
