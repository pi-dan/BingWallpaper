using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace BingWallpaper
{
    static class Program
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            if (args.Length != 0)    //判断是否自启动，自启动有参数 a
                Application.Run(new Form1(args[0]));
            else
                Application.Run(new Form1(null));
        }
    }
}
