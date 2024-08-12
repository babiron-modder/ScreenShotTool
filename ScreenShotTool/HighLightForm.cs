using System;
using System.Windows.Forms;

namespace ScreenShotTool
{
    public partial class HighLightForm : Form
    {
        int tmp_x = 0;
        int tmp_y = 0;
        int tmp_w = 0;
        int tmp_h = 0;

        public HighLightForm(int screenId)
        {
            InitializeComponent();

            TopMost = true;
            var screen_loc = Screen.AllScreens[screenId].Bounds;

            tmp_x = screen_loc.X;
            tmp_y = screen_loc.Y;
            tmp_w = screen_loc.Width;
            tmp_h = screen_loc.Height;

        }

        public HighLightForm(int screenId, int x, int y, int width, int height)
        {
            InitializeComponent();

            FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;

            TopMost = true;
            var screen_loc = Screen.AllScreens[screenId].Bounds;

            tmp_x = screen_loc.X + x;
            tmp_y = screen_loc.Y + y;
            tmp_w = width;
            tmp_h = height;
        }

        double current_speed = 0.001;
        private void HighLightForm_Load(object sender, EventArgs e)
        {
            Left = tmp_x;
            Top = tmp_y;
            Width = tmp_w;
            Height = tmp_h;

            Opacity = 0.95;
            current_speed = 0.001;
            timer1.Interval = 1;
            timer1.Enabled = true;
        }


        private void timer1_Tick(object sender, EventArgs e)
        {
            Opacity -= current_speed;
            current_speed *= 1.13;
            if(Opacity == 0)
            {
                Close();
            }
        }
    }
}
