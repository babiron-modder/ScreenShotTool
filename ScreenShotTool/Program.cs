using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ScreenShotTool
{
    internal static class Program
    {

        // 多重起動を禁止するミューテックス
        private static Mutex mutexObject;

        /// <summary>
        /// アプリケーションのメイン エントリ ポイントです。
        /// </summary>
        [STAThread]
        static void Main()
        {
            // 重複起動防止
            try
            {
                // ミューテックスを生成する
                mutexObject = new Mutex(false, @"Global\cfeb2fa8-b1fc-49cf-af64-129dc84df75c");
            }
            catch (ApplicationException e)
            {
                // 何もしない
                return;
            }

            // ミューテックスを取得する
            if (mutexObject.WaitOne(0, false))
            {
                // アプリケーションを実行
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new Form1());

                // ミューテックスを解放する
                mutexObject.ReleaseMutex();
            }
            else
            {
                // 何もしない
            }

            // ミューテックスを破棄する
            mutexObject.Close();


        }
    }
}
