using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace ScreenShotTool
{
    public partial class RectSelectWindow : Form
    {
        // ==============================================================================
        //  Form
        // ==============================================================================

        public int tmp_x = 0;
        public int tmp_y = 0;
        public int tmp_w = 0;
        public int tmp_h = 0;
        public int monitorId = 0;

        public RectSelectWindow(int screenId, int x, int y, int w, int h)
        {
            InitializeComponent();

            Opacity = 0.9;

            var screen_loc = Screen.AllScreens[screenId].Bounds;

            tmp_x = screen_loc.X + x;
            tmp_y = screen_loc.Y + y;

            tmp_w = w;
            tmp_h = h;
        }

        private void RectSelectWindow_Load(object sender, EventArgs e)
        {

            Left = tmp_x;
            Top = tmp_y;
            Width = tmp_w;
            Height = tmp_h;
            WindowState = FormWindowState.Normal;
            TopMost = true;
        }

        private void 閉じるToolStripMenuItem_Click(object sender, EventArgs e)
        {
            tmp_x = Left;
            tmp_y = Top;
            tmp_w = Width;
            tmp_h = Height;

            Close();
        }



        // ==============================================================================
        //  マウス操作
        // ==============================================================================



        //---------------
        // 背景操作
        //---------------
        private const int WM_NCLBUTTONDOWN = 0xA1;
        private const int HT_CAPTION = 0x2;

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr SendMessage(IntPtr hWnd, int Msg, IntPtr wParam, IntPtr lParam);
        [DllImportAttribute("user32.dll")]
        private static extern bool ReleaseCapture();

        private void Panel_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                //マウスのキャプチャを解除
                ReleaseCapture();
                //タイトルバーでマウスの左ボタンが押されたことにする
                SendMessage(Handle, WM_NCLBUTTONDOWN, (IntPtr)HT_CAPTION, IntPtr.Zero);
            }
        }


        //---------------
        // 右下操作
        //---------------

        private Point mousePoint_RD;
        private void panel2_MouseDown(object sender, MouseEventArgs e)
        {
            if ((e.Button & MouseButtons.Left) == MouseButtons.Left)
            {
                //位置を記憶する
                mousePoint_RD = new Point(e.X, e.Y);
            }
        }

        private void panel2_MouseMove(object sender, MouseEventArgs e)
        {
            if ((e.Button & MouseButtons.Left) == MouseButtons.Left)
            {
                Width += e.X - mousePoint_RD.X;
                Height += e.Y - mousePoint_RD.Y;
            }
        }


        //---------------
        // 左下操作
        //---------------
        private Point mousePoint_LD;
        private void panel5_MouseDown(object sender, MouseEventArgs e)
        {
            if ((e.Button & MouseButtons.Left) == MouseButtons.Left)
            {
                //位置を記憶する
                mousePoint_LD = new Point(e.X, e.Y);
            }
        }

        private void panel5_MouseMove(object sender, MouseEventArgs e)
        {
            if ((e.Button & MouseButtons.Left) == MouseButtons.Left)
            {
                Left += e.X - mousePoint_LD.X;
                Width -= e.X - mousePoint_LD.X;
                Height += e.Y - mousePoint_LD.Y;
            }
        }


        //---------------
        // 右上操作
        //---------------
        private Point mousePoint_RU;
        private void panel3_MouseDown(object sender, MouseEventArgs e)
        {
            if ((e.Button & MouseButtons.Left) == MouseButtons.Left)
            {
                //位置を記憶する
                mousePoint_RU = new Point(e.X, e.Y);
            }
        }

        private void panel3_MouseMove(object sender, MouseEventArgs e)
        {
            if ((e.Button & MouseButtons.Left) == MouseButtons.Left)
            {
                Top += e.Y - mousePoint_RU.Y;
                Width += e.X - mousePoint_RU.X;
                Height -= e.Y - mousePoint_RU.Y;
            }
        }

        //---------------
        // 左上操作
        //---------------
        private Point mousePoint_LU;
        private void panel4_MouseDown(object sender, MouseEventArgs e)
        {
            if ((e.Button & MouseButtons.Left) == MouseButtons.Left)
            {
                //位置を記憶する
                mousePoint_LU = new Point(e.X, e.Y);
            }
        }

        private void panel4_MouseMove(object sender, MouseEventArgs e)
        {
            if ((e.Button & MouseButtons.Left) == MouseButtons.Left)
            {
                Top += e.Y - mousePoint_LU.Y;
                Left += e.X - mousePoint_LU.X;
                Width -= e.X - mousePoint_LU.X;
                Height -= e.Y - mousePoint_LU.Y;
            }
        }
    }
}
