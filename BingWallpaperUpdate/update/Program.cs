using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace update
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
            if (args.Length != 0)    //判断是否由程序启动，程序启动传入一个参数
                Application.Run(new Form1());
        }
    }
}
