using KeyEvent;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace ScreenShotTool
{
    public partial class Form1 : Form
    {
        public delegate bool EnumMonitorsDelegate(IntPtr hMonitor, IntPtr hdcMonitor,
                                                 ref RECT lprcMonitor, IntPtr dwData);

        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool EnumDisplayMonitors(IntPtr hdc,
                                                      IntPtr lprcClip,
                                                      EnumMonitorsDelegate lpfnEnum,
                                                      IntPtr dwData);


        [DllImport("Shcore.dll", CharSet = CharSet.Unicode)]
        public static extern bool GetDpiForMonitor(IntPtr hmonitor, int dpiType, ref uint dpiX, ref uint dpiY);





        [DllImport("user32.dll")]
        private static extern IntPtr GetDC(IntPtr hwnd);

        [DllImport("gdi32.dll")]
        private static extern int BitBlt(IntPtr hDestDC,
            int x,
            int y,
            int nWidth,
            int nHeight,
            IntPtr hSrcDC,
            int xSrc,
            int ySrc,
            int dwRop);

        [DllImport("user32.dll")]
        private static extern IntPtr ReleaseDC(IntPtr hwnd, IntPtr hdc);


        private const int SRCCOPY = 0x00EE0086;



        [DllImport("user32.dll")]
        private static extern IntPtr GetWindowDC(IntPtr hwnd);

        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        private static extern int GetWindowRect(IntPtr hwnd, ref RECT lpRect);


        [System.Runtime.InteropServices.DllImport("User32.dll")]
        private extern static bool PrintWindow(IntPtr hwnd, IntPtr hDC, uint nFlags);

        [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern IntPtr MonitorFromWindow(IntPtr hwnd, int dwFlags);


        Dictionary<string, int> keyValues = new Dictionary<string, int> {
            {"PrintScreen",        (int)Keys.PrintScreen},
            {"Delete",             (int)Keys.Delete},
            {"Enter",              (int)Keys.Enter},
            {"Escape",             (int)Keys.Escape},
            {"CapsLock",           (int)Keys.CapsLock},
            {"Home",               (int)Keys.Home},
            {"End",                (int)Keys.End},
            {"PageDown",           (int)Keys.PageDown},
            {"PageUp",             (int)Keys.PageUp},
            {"Left",               (int)Keys.Left},
            {"Up",                 (int)Keys.Up},
            {"Down",               (int)Keys.Down},
            {"Right",              (int)Keys.Right},
            {"Space",              (int)Keys.Space},
            {"Pause",              (int)Keys.Pause},
            {"NumLock",            (int)Keys.NumLock},
            {"NumPad0",            (int)Keys.NumPad0},
            {"NumPad1",            (int)Keys.NumPad1},
            {"NumPad2",            (int)Keys.NumPad2},
            {"NumPad3",            (int)Keys.NumPad3},
            {"NumPad4",            (int)Keys.NumPad4},
            {"NumPad5",            (int)Keys.NumPad5},
            {"NumPad6",            (int)Keys.NumPad6},
            {"NumPad7",            (int)Keys.NumPad7},
            {"NumPad8",            (int)Keys.NumPad8},
            {"NumPad9",            (int)Keys.NumPad9},
            {"A",                  (int)Keys.A},
            {"B",                  (int)Keys.B},
            {"C",                  (int)Keys.C},
            {"D",                  (int)Keys.D},
            {"E",                  (int)Keys.E},
            {"F",                  (int)Keys.F},
            {"G",                  (int)Keys.G},
            {"H",                  (int)Keys.H},
            {"I",                  (int)Keys.I},
            {"J",                  (int)Keys.J},
            {"K",                  (int)Keys.K},
            {"L",                  (int)Keys.L},
            {"M",                  (int)Keys.M},
            {"N",                  (int)Keys.N},
            {"O",                  (int)Keys.O},
            {"P",                  (int)Keys.P},
            {"Q",                  (int)Keys.Q},
            {"R",                  (int)Keys.R},
            {"S",                  (int)Keys.S},
            {"T",                  (int)Keys.T},
            {"U",                  (int)Keys.U},
            {"V",                  (int)Keys.V},
            {"W",                  (int)Keys.W},
            {"X",                  (int)Keys.X},
            {"Y",                  (int)Keys.Y},
            {"Z",                  (int)Keys.Z},
            {"F1",                 (int)Keys.F1},
            {"F2",                 (int)Keys.F2},
            {"F3",                 (int)Keys.F3},
            {"F4",                 (int)Keys.F4},
            {"F5",                 (int)Keys.F5},
            {"F6",                 (int)Keys.F6},
            {"F7",                 (int)Keys.F7},
            {"F8",                 (int)Keys.F8},
            {"F9",                 (int)Keys.F9},
            {"F10",                (int)Keys.F10},
            {"F11",                (int)Keys.F11},
            {"F12",                (int)Keys.F12},
            {"F13",                (int)Keys.F13},
            {"F14",                (int)Keys.F14},
            {"F15",                (int)Keys.F15},
            {"F16",                (int)Keys.F16},
            {"F17",                (int)Keys.F17},
            {"F18",                (int)Keys.F18},
            {"F19",                (int)Keys.F19},
            {"F20",                (int)Keys.F20},
            {"F21",                (int)Keys.F21},
            {"F22",                (int)Keys.F22},
            {"F23",                (int)Keys.F23},
            {"F24",                (int)Keys.F24},
        };



        // ==============================================================================
        //  Form1
        // ==============================================================================
        InterceptKeyboard interceptKeyboard;
        public Form1()
        {
            InitializeComponent();

            // キー取得設定
            interceptKeyboard = new InterceptKeyboard();
            interceptKeyboard.KeyDownEvent += InterceptKeyboard_KeyDownEvent;
            interceptKeyboard.KeyUpEvent += InterceptKeyboard_KeyUpEvent;
            interceptKeyboard.Hook();
        }

        private static string save_point = "";
        private static int max_monitor_count = 0;  // モニター数
        private static int screenshot_type = 0;   // スクショタイプ
        private static int selected_monitor_id = 0;  // 撮影するモニタID
        private static int screenshot_position_X = 0;       // 矩形のX座標
        private static int screenshot_position_Y = 0;       // 矩形のX座標
        private static int screenshot_position_Width = 0;   // 矩形の横幅
        private static int screenshot_position_Height = 0;  // 矩形の高さ
        private static int screen_key_code = -1;  // スクショをするキーコード
        private static int screen_key_code_lshift = -1;  // スクショをするキーコード(左シフト)
        private static int screen_key_code_rshift = -1;  // スクショをするキーコード(右シフト)
        private static int screen_key_code_lctrl = -1;   // スクショをするキーコード(左Ctrl)
        private static int screen_key_code_rctrl = -1;   // スクショをするキーコード(右Ctrl)
        private static int screen_key_code_alt = -1;     // スクショをするキーコード(alt)
        private static bool screenshot_effect = true;     // スクショ時のエフェクト
        private static List<IntPtr> monitor_handles;
        private static List<Point> monitor_points;

        // 全モニターを取得
        private bool GetMonitors(IntPtr hMonitor, IntPtr hdcMonitor, ref RECT lprcMonitor, IntPtr dwData)
        {
            max_monitor_count++;
            monitor_handles.Add(hMonitor);
            monitor_points.Add(new Point(lprcMonitor.Left, lprcMonitor.Top));
            return true;
        }



        private void Form1_Load(object sender, EventArgs e)
        {

            // 撮影場所の指定
            textBox1.Text = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
            textBox1.Text = Path.Combine(textBox1.Text, "shotter");


            comboBox3.Items.Clear();
            foreach(KeyValuePair<string, int> kvp in keyValues)
            {
                comboBox3.Items.Add(kvp.Key);
            }
            comboBox3.SelectedIndex = 0;


            load_Displays();

            save_setting();

            return;
        }

        // 全画面ラジオボタン選択時
        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            panel1.Enabled = true;
            panel2.Enabled = false;
            button3.Enabled = true;
        }

        // アクティブウィンドウラジオボタン選択時
        private void radioButton34_CheckedChanged(object sender, EventArgs e)
        {
            panel1.Enabled = false;
            panel2.Enabled = false;
            button3.Enabled = true;
        }

        // 矩形ラジオボタン選択時
        private void radioButton4_CheckedChanged(object sender, EventArgs e)
        {
            panel1.Enabled = false;
            panel2.Enabled = true;
            button3.Enabled = true;
        }

        // コンボボックスの変更時
        private void comboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            button3.Enabled = true;
        }

        // numericUpdownの変更時
        private void numericUpDown_ValueChanged(object sender, EventArgs e)
        {
            button3.Enabled = true;
        }

        // checkboxの変更時
        private void checkBox_CheckedChanged(object sender, EventArgs e)
        {
            button3.Enabled = true;
        }

        // textboxの変更時
        private void textBox_TextChanged(object sender, EventArgs e)
        {
            button3.Enabled = true;
        }


        // 画面の再取得
        private void load_Displays()
        {
            // 全ディスプレイを見て枚数を調査
            if(monitor_handles == null)
            {
                monitor_handles = new List<IntPtr>();
                monitor_points = new List<Point>();
            }
            monitor_handles.Clear();
            monitor_points.Clear();
            max_monitor_count = 0;
            EnumDisplayMonitors(IntPtr.Zero, IntPtr.Zero, GetMonitors, IntPtr.Zero);

            comboBox1.Items.Clear();
            comboBox2.Items.Clear();

            // ウィンドウの調査結果をコンボボックスに反映
            for (int i = 0; i < max_monitor_count; ++i)
            {
                comboBox1.Items.Add("Display #" + i.ToString());
                comboBox2.Items.Add("Display #" + i.ToString());
            }
            comboBox1.SelectedIndex = 0;
            comboBox2.SelectedIndex = 0;
        }

        // 画面の再取得ボタン
        private void button6_Click(object sender, EventArgs e)
        {
            load_Displays();

            button3.Enabled = true;
        }


        // 全画面ハイライト
        private void button1_Click(object sender, EventArgs e)
        {
            var hili = new HighLightForm(comboBox1.SelectedIndex);
            hili.Show();
        }

        // 矩形ハイライト
        private void button2_Click(object sender, EventArgs e)
        {
            var hili = new HighLightForm(comboBox2.SelectedIndex,
                                         (int) numericUpDown1.Value,
                                         (int) numericUpDown2.Value,
                                         (int) numericUpDown3.Value,
                                         (int) numericUpDown4.Value);
            hili.Show();
        }

        // 矩形選択
        private void button7_Click(object sender, EventArgs e)
        {
            WindowState = FormWindowState.Minimized;

            var rect_form = new RectSelectWindow(comboBox2.SelectedIndex,
                                                 (int)numericUpDown1.Value,
                                                 (int)numericUpDown2.Value,
                                                 (int)numericUpDown3.Value,
                                                 (int)numericUpDown4.Value);
            rect_form.ShowDialog();

            numericUpDown1.Value = rect_form.tmp_x;
            numericUpDown2.Value = rect_form.tmp_y;
            numericUpDown3.Value = rect_form.tmp_w;
            numericUpDown4.Value = rect_form.tmp_h;

            WindowState = FormWindowState.Normal;

            button3.Enabled = true;
        }

        // フォルダの選択
        private void button5_Click(object sender, EventArgs e)
        {
            if (Directory.Exists(textBox1.Text))
            {
                folderBrowserDialog1.SelectedPath = textBox1.Text;
            }
            else
            {
                folderBrowserDialog1.SelectedPath = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
            }

            var result = folderBrowserDialog1.ShowDialog();

            if(result == DialogResult.OK)
            {
                textBox1.Text = folderBrowserDialog1.SelectedPath;

                button3.Enabled = true;
            }
        }



        // 適用ボタン
        private void button3_Click(object sender, EventArgs e)
        {
            save_setting();
        }

        // 設定の適用

        private void save_setting()
        {
            if (radioButton1.Checked)
            {
                // ウィンドウのスクリーンショット
                screenshot_type = 0;
                selected_monitor_id = comboBox1.SelectedIndex;
            }
            else if (radioButton2.Checked)
            {
                screenshot_type = 1;
            }
            else if (radioButton3.Checked)
            {
                screenshot_type = 2;
            }
            else if (radioButton4.Checked)
            {
                screenshot_type = 3;
                selected_monitor_id = comboBox2.SelectedIndex;

                screenshot_position_X = (int)numericUpDown1.Value;
                screenshot_position_Y = (int)numericUpDown2.Value;
                screenshot_position_Width = (int)numericUpDown3.Value;
                screenshot_position_Height = (int)numericUpDown4.Value;
            }

            // 保存場所の指定
            save_point = textBox1.Text;

            // スクショのキーコードの指定
            //screen_key_code = 44;
            screen_key_code = keyValues[comboBox3.Items[comboBox3.SelectedIndex].ToString()];


            // スクショのエフェクトの設定
            screenshot_effect = checkBox6.Checked;


            button3.Enabled = false;
        }



        // ==============================================================================
        //  KeyBinding
        // ==============================================================================

        // キーの状態
        private static bool[] keyboard_keys = new bool[256];


        private static void InterceptKeyboard_KeyDownEvent(object sender, InterceptKeyboard.OriginalKeyEventArg e)
        {
            //Console.WriteLine("Keydown KeyCode {0}", e.KeyCode);


            keyboard_keys[e.KeyCode] = true;
        }
        private static void InterceptKeyboard_KeyUpEvent(object sender, InterceptKeyboard.OriginalKeyEventArg e)
        {
            //Console.WriteLine("Keyup KeyCode {0}", e.KeyCode);

            if (keyboard_keys[e.KeyCode] && e.KeyCode == screen_key_code)
            {
                // スクショの作成
                switch (screenshot_type)
                {
                    case 0:
                        // 全画面
                        monitor_counter = 0;
                        EnumDisplayMonitors(IntPtr.Zero, IntPtr.Zero, RunEnumMonitor, IntPtr.Zero);
                        break;
                    case 1:
                        // アクティブウィンドウ（そのまま取得）
                        CaptureActiveWindow();
                        break;
                    case 2:
                        // アクティブウィンドウ（裏も取得）
                        CaptureActiveWindowUra();
                        break;
                    case 3:
                        // 矩形取得
                        monitor_counter = 0;
                        EnumDisplayMonitors(IntPtr.Zero, IntPtr.Zero, CaptureRect, IntPtr.Zero);
                        break;
                    default:
                        // 何もしない
                        break;
                }
            }

            keyboard_keys[e.KeyCode] = false;
        }

        // モニターをスクショ
        private static int monitor_counter = 0;
        private static bool RunEnumMonitor(IntPtr hMonitor, IntPtr hdcMonitor, ref RECT lprcMonitor, IntPtr dwData)
        {
            // 指定のモニターID以外は無視する
            if (monitor_counter++ != selected_monitor_id) { return true; }

            uint tmp_x = 0;
            uint tmp_y = 0;
            GetDpiForMonitor(hMonitor, 0, ref tmp_x, ref tmp_y);

            IntPtr disDC = GetDC(IntPtr.Zero);

            //Bitmapの作成
            Bitmap bmp = new Bitmap(lprcMonitor.Right - lprcMonitor.Left, lprcMonitor.Bottom - lprcMonitor.Top);
            // 画像のDPIを指定しておく
            bmp.SetResolution(tmp_x, tmp_y);
            //Graphicsの作成
            Graphics g = Graphics.FromImage(bmp);
            // 背景を塗っておく
            var rect1 = new Rectangle(0, 0, bmp.Width, bmp.Height);
            g.FillRectangle(Brushes.Black, rect1);
            //Graphicsのデバイスコンテキストを取得
            IntPtr hDC = g.GetHdc();
            //Bitmapに画像をコピーする
            BitBlt(hDC, 0, 0, bmp.Width, bmp.Height,
                disDC, lprcMonitor.Left, lprcMonitor.Top, SRCCOPY);
            //解放
            g.ReleaseHdc(hDC);
            g.Dispose();

            ReleaseDC(IntPtr.Zero, disDC);

            Directory.CreateDirectory(save_point);
            var path = Path.Combine(save_point, DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss-fff")+".png");
            bmp.Save(path, System.Drawing.Imaging.ImageFormat.Png);
            bmp.Dispose();


            if (screenshot_effect)
            {
                var hili = new HighLightForm(selected_monitor_id);
                hili.Show();
            }


            return true;
        }


        // 表取得

        private static void CaptureActiveWindow()
        {
            //アクティブなウィンドウのデバイスコンテキストを取得
            IntPtr hWnd = GetForegroundWindow();
            //ウィンドウの大きさを取得
            RECT winRect = new RECT();
            GetWindowRect(hWnd, ref winRect);

            IntPtr hMonitor = MonitorFromWindow(hWnd, 0);

            uint tmp_x = 0;
            uint tmp_y = 0;
            GetDpiForMonitor(hMonitor, 0, ref tmp_x, ref tmp_y);

            IntPtr disDC = GetDC(IntPtr.Zero);

            //Bitmapの作成
            Bitmap bmp = new Bitmap(winRect.Right - winRect.Left, winRect.Bottom - winRect.Top);

            // 画像のDPIを指定しておく
            bmp.SetResolution(tmp_x, tmp_y);
            //Graphicsの作成
            Graphics g = Graphics.FromImage(bmp);
            // 背景を塗っておく
            var rect1 = new Rectangle(0, 0, bmp.Width, bmp.Height);
            g.FillRectangle(Brushes.Black, rect1);
            //Graphicsのデバイスコンテキストを取得
            IntPtr hDC = g.GetHdc();
            //Bitmapに画像をコピーする
            BitBlt(hDC, 0, 0, bmp.Width, bmp.Height, disDC, winRect.Left, winRect.Top, SRCCOPY);
            //解放
            g.ReleaseHdc(hDC);
            g.Dispose();

            ReleaseDC(IntPtr.Zero, disDC);

            Directory.CreateDirectory(save_point);
            var path = Path.Combine(save_point, DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss-fff") + ".png");
            bmp.Save(path, System.Drawing.Imaging.ImageFormat.Png);

            if (screenshot_effect)
            {
                var hili = new HighLightForm(0); ;
                for(var i=0; i<monitor_handles.Count(); ++i)
                {
                    if(monitor_handles[i] == hMonitor)
                    {
                        hili = new HighLightForm(i, winRect.Left - monitor_points[i].X,
                                                 winRect.Top - monitor_points[i].Y, bmp.Width, bmp.Height);
                        break;
                    }
                }

                hili.Show();
            }

            // サイズが取れなくなるので解放は後にする
            bmp.Dispose();


        }

        // 裏取得
        public static void CaptureActiveWindowUra()
        {
            //アクティブなウィンドウのデバイスコンテキストを取得
            IntPtr hWnd = GetForegroundWindow();
            IntPtr winDC = GetWindowDC(hWnd);
            //ウィンドウの大きさを取得
            RECT winRect = new RECT();
            GetWindowRect(hWnd, ref winRect);


            IntPtr hMonitor = MonitorFromWindow(hWnd, 0);
            uint tmp_x = 0;
            uint tmp_y = 0;
            GetDpiForMonitor(hMonitor, 0, ref tmp_x, ref tmp_y);


            //Bitmapの作成
            Bitmap bmp = new Bitmap(winRect.Right - winRect.Left,
                winRect.Bottom - winRect.Top);
            //Graphicsの作成
            Graphics g = Graphics.FromImage(bmp);
            // 背景を塗っておく
            var rect1 = new Rectangle(0, 0, bmp.Width, bmp.Height);
            g.FillRectangle(Brushes.Black, rect1);
            //Graphicsのデバイスコンテキストを取得
            IntPtr hDC = g.GetHdc();
            PrintWindow(hWnd, hDC, 0);

            //解放
            g.ReleaseHdc(hDC);
            g.Dispose();
            ReleaseDC(hWnd, winDC);


            Directory.CreateDirectory(save_point);
            var path = Path.Combine(save_point, DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss-fff") + ".png");
            bmp.Save(path, System.Drawing.Imaging.ImageFormat.Png);


            if (screenshot_effect)
            {
                var hili = new HighLightForm(0); ;
                for (var i = 0; i < monitor_handles.Count(); ++i)
                {
                    if (monitor_handles[i] == hMonitor)
                    {
                        hili = new HighLightForm(i, winRect.Left - monitor_points[i].X,
                                                 winRect.Top - monitor_points[i].Y, bmp.Width, bmp.Height);
                        break;
                    }
                }

                hili.Show();
            }


            bmp.Dispose();
        }


        // 矩形取得

        private static bool CaptureRect(IntPtr hMonitor, IntPtr hdcMonitor, ref RECT lprcMonitor, IntPtr dwData)
        {
            // 指定のモニターID以外は無視する
            if (monitor_counter++ != selected_monitor_id) { return true; }

            uint tmp_x = 0;
            uint tmp_y = 0;
            GetDpiForMonitor(hMonitor, 0, ref tmp_x, ref tmp_y);

            IntPtr disDC = GetDC(IntPtr.Zero);

            //Bitmapの作成
            Bitmap bmp = new Bitmap(screenshot_position_Width, screenshot_position_Height);

            // 画像のDPIを指定しておく
            bmp.SetResolution(tmp_x, tmp_y);
            //Graphicsの作成
            Graphics g = Graphics.FromImage(bmp);
            // 背景を塗っておく
            var rect1 = new Rectangle(0, 0, bmp.Width, bmp.Height);
            g.FillRectangle(Brushes.Black, rect1);
            //Graphicsのデバイスコンテキストを取得
            IntPtr hDC = g.GetHdc();
            //Bitmapに画像をコピーする
            BitBlt(hDC, 0, 0, bmp.Width, bmp.Height, disDC,
                lprcMonitor.Left + screenshot_position_X, lprcMonitor.Top + screenshot_position_Y, SRCCOPY);
            //解放
            g.ReleaseHdc(hDC);
            g.Dispose();

            ReleaseDC(IntPtr.Zero, disDC);

            Directory.CreateDirectory(save_point);
            var path = Path.Combine(save_point, DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss-fff") + ".png");
            bmp.Save(path, System.Drawing.Imaging.ImageFormat.Png);


            if (screenshot_effect)
            {
                var hili = new HighLightForm(0); ;
                for (var i = 0; i < monitor_handles.Count(); ++i)
                {
                    if (monitor_handles[i] == hMonitor)
                    {
                        hili = new HighLightForm(i, screenshot_position_X, screenshot_position_Y,
                                                 bmp.Width, bmp.Height);
                        break;
                    }
                }

                hili.Show();
            }



            bmp.Dispose();

            return true;
        }

    }
}
