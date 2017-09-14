using System;
using System.Windows.Forms;
using System.IO;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Net.NetworkInformation;
using System.Threading;
using System.Xml;
using System.Collections.Generic;

namespace BingWallpaper
{
    public partial class Form1 : Form
    {
        [DllImport("wininet")]
        //判断网络状况的方法,返回值true为连接，false为未连接  
        public extern static bool InternetGetConnectedState(out int conState, int reder); 

        Bitmap bitmap;
        string isAutoStart = null;

        public Form1(string arg)
        {
            InitializeComponent();
            if (Directory.Exists(imageInfo.imageSaveDirectory) == false)
                Directory.CreateDirectory(imageInfo.imageSaveDirectory);

            #region 控件透明设置
            pictureBox.Controls.Add(label1);  //控件透明
            label1.BackColor = Color.Transparent;
            pictureBox.Controls.Add(checkBox1);
            checkBox1.BackColor = Color.Transparent;
            pictureBox.Controls.Add(label2);
            label2.BackColor = Color.Transparent;
            pictureBox.Controls.Add(label3);
            label3.BackColor = Color.Transparent;
            //pictureBox.Controls.Add(textBox1);
            //textBox1.BackColor = Color.Transparent;
            #endregion

            programInit();  //获取程序初始值及初始化

            isAutoStart = arg;
            Thread thread = new Thread(wallpaperMainFun);
            thread.IsBackground = true;  //设置为后台线程
            thread.Start();
        }

        private void programInit()
        {
            if (SetFunction.linkExist()) //判断程序是否自启
                checkBox1.Checked = true;
            else
                checkBox1.Checked = false;

            bitmap = new Bitmap(imageInfo.appBackgroundImag);  //app背景
            pictureBox.Image = bitmap;

            if (!File.Exists(imageInfo.XMLPath))   //判断配置文件是否存在，不存在则创建
            {
                ConfigXML.CreateXmlDocument(imageInfo.XMLPath, "config", "1.0", "UTF-8", "yes");
                ConfigXML.CreateOrUpdateXmlNodeByXPath(imageInfo.XMLPath, @"config/general", "AppVersion", "1.0.0.20170422");
                ConfigXML.CreateOrUpdateXmlNodeByXPath(imageInfo.XMLPath, @"config/general", "delayMinute", "30");

            }
            //MessageBox.Show(ConfigXML.GetDelayTime(imageInfo.XMLPath, @"config/general/delayMinute", "30"));
           textBox1.Text = ConfigXML.GetDelayTime(imageInfo.XMLPath, @"config/general/delayMinute", "30"); //获取系统延时配置
        }

        private void wallpaperMainFun()
        {
            string p=imageInfo.imageSaveDirectory + DateTime.Now.ToLongDateString().ToString() + ".bmp";  //bmp文件路径
            if (!File.Exists(p))
            {
                if (CheckServeStatus()) //检查网络连接
                {
                    //bitmap = SetFunction.getImage();
                    SetFunction.getImage();

                    SetFunction.setWallpaper();
                    if (bitmap != null) //可能下载失败  
                    {
                        if ("a".Equals(isAutoStart))   //自启
                            appExit();
                    }
                    else return;
                }
                else return;
            }
            else
            {
                SetFunction.setWallpaper();
                if ("a".Equals(isAutoStart))
                    appExit();
            }
        }


        public class FIleLastTimeComparer : IComparer<FileInfo>
        {
            public int Compare(FileInfo x, FileInfo y)
            {
                return y.LastWriteTime.CompareTo(x.LastWriteTime);//递减
                //return x.LastWriteTime.CompareTo(y.LastWriteTime);//递增
            }
        }
        public  void appExit()   //退出程序
        {

            //删除多余的图片文件
            DirectoryInfo theFolder = new DirectoryInfo(imageInfo.imageSaveDirectory);
            FileInfo[] fileInfo = theFolder.GetFiles();//目录中文件信息
            if (fileInfo.Length >= 30)   //图片文件个数大于三十
            {
                Array.Sort(fileInfo, new FIleLastTimeComparer());//按文件时间修改顺序排序
                string[] fileNames = new string[fileInfo.Length];
                for (int i = 0; i < fileInfo.Length; i++)
                {
                    fileNames[i] = fileInfo[i].Name;
                }
                for (int i = 30; i < fileNames.Length; i++)
                {
                    File.Delete(imageInfo.imageSaveDirectory + "\\" + fileNames[i]);
                }
            }


            if(!CheckServeStatus())   //网络未连接
                Application.Exit();
            
            Boolean isUpdate = ExeUpdate.checkUpdate(imageInfo.appUpdateAdd, imageInfo.updateDirectory + "\\" + imageInfo.MainUpdateFile);
            //MessageBox.Show(isUpdate+"SSS");
            string XMLPath = imageInfo.updateDirectory + "\\" + imageInfo.MainUpdateFile;

            try
            {
                if (isUpdate)  //检测到升级
                {
                   
                    string NetUpdateVersion = ConfigXML.GetXmlNodeInnerTextByXpath(XMLPath, @"update/update.exe/version");  //update.exe的web端版本号
                    string localUpdateVersion = ConfigXML.GetXmlNodeInnerTextByXpath(imageInfo.XMLPath, @"config/version/update.exe");  //本地版本号
                    string NetAppVersion = ConfigXML.GetXmlNodeInnerTextByXpath(XMLPath, @"update/AppVersion/version");
                    Boolean updateIsStart = checkUpdateIsStart();  //判断是否启动update.exe文件
                    bool updateState = false;  //update文件是否启动
                    if (localUpdateVersion.CompareTo(NetUpdateVersion) < 0)  //update.exe文件升级
                    {
                        string updateAdd = ConfigXML.GetXmlNodeInnerTextByXpath(XMLPath, @"update/update.exe/download");
                        ExeUpdate.DownLoadFile(updateAdd, imageInfo.updateDirectory + "\\" + "update.exe");   //下载更新文件
                        Boolean move1 = SetFunction.moveFile(imageInfo.updateDirectory + "\\" + "update.exe", Application.StartupPath + "\\" + "update.exe");
                        //MessageBox.Show(updateAdd);
                        if (move1 )
                        {//更新升级文件版本号
                            ConfigXML.CreateOrUpdateXmlNodeByXPath(imageInfo.XMLPath, @"config/version", "update.exe", NetUpdateVersion);
                            updateState = true;
                        }
                        

                        if (!updateIsStart)
                        {
                            SetFunction.DeleteDirectory(Application.StartupPath + "\\" + "temp");
                            //更新主版本号
                            ConfigXML.CreateOrUpdateXmlNodeByXPath(imageInfo.XMLPath, @"config/general", "AppVersion", NetAppVersion);
                        }
                    }
                    if ((updateIsStart  && updateState))
                    {
                        //提醒用户更新 
                        if (MessageBox.Show("检测到新版本，是否升级？",
                                                "更新提示", MessageBoxButtons.YesNo, MessageBoxIcon.Asterisk) == DialogResult.Yes)
                        {
                            System.Diagnostics.Process.Start(Application.StartupPath + "\\update.exe","start");   //传入的参数任意，update.exe只判断是否有参数传入
                            //////打开升级软件
                        }
                    }
                }
                else   //没有升级 ，删除临时文件
                    SetFunction.DeleteDirectory(Application.StartupPath + "\\" + "temp");
            }
            catch
            { }
            Application.Exit();
        }

        public static Boolean checkUpdateIsStart()
        {
            Dictionary<string, string> localNameVersion = new Dictionary<string, string>();
            Dictionary<string, string> netNameVersion = new Dictionary<string, string>();
            string netXMLPath = imageInfo.updateDirectory + "\\" + imageInfo.MainUpdateFile;
            if (!(File.Exists(imageInfo.XMLPath) || File.Exists(netXMLPath)))  //文件不存在
                return false;
            XmlNode localVersionNode = ConfigXML.GetXmlNodeByXpath(imageInfo.XMLPath, @"config/version");
            if (!(localVersionNode == null))
                foreach (XmlNode child in localVersionNode)
                {
                    if (!(child.Name == "update.exe")) //排除本程序升级的程序
                        localNameVersion.Add(child.Name, child.InnerText);
                }

            XmlNode netVersionNode = ConfigXML.GetXmlNodeByXpath(netXMLPath, "update");
            if (netVersionNode == null) return false; //无内容
            foreach (XmlNode appName in netVersionNode)  //遍历web上版本信息
            {
                if (appName.Name == "AppVersion" || appName.Name == "update.exe")
                    continue;
                foreach (XmlNode child in appName)
                {
                    if (child.Name == "version")
                        netNameVersion.Add(appName.Name, child.InnerText);
                }
            }

            foreach(string key in  netNameVersion.Keys)
            {
                if (localNameVersion.ContainsKey(key) && (localNameVersion[key].Equals(netNameVersion[key])))
                    continue;
                else
                    return true;
            }
            
            return false;
        }

        public static Boolean CheckServeStatus()
        {
            int errCount = 0;//ping时连接失败个数
            System.Int32 dwFlag = new Int32();
            string urls = imageInfo.pingAdd;

            if (!InternetGetConnectedState(out dwFlag, 0))
            { 
                return false; //未连网!
            }
            else if (!MyPing(urls, out errCount))
            {
                if ((double)errCount / urls.Length >= 0.3)
                {
                    //MessageBox.Show("网络异常~连接多次无响应");
                    return false;
                }
                else
                {
                    //MessageBox.Show("网络不稳定");
                    return false;
                }
            }
            else
            {
               // MessageBox.Show("网络正常");
                return true;
            }
        }

        public static bool MyPing(string urls, out int errorCount)
        {
            bool isconn = true;
            Ping ping = new Ping();
            errorCount = 0;
            try
            {
                PingReply pr;
                pr = ping.Send(urls);
                if (pr.Status != IPStatus.Success)
                {
                    isconn = false;
                    errorCount++;
                }
            }
            catch
            {
                isconn = false;
                errorCount = urls.Length;
            }
            //if (errorCount > 0 && errorCount < 3)
            //  isconn = true;
            return isconn;
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if(checkBox1.Checked)
                SetFunction.creatLink(true);
            else
                SetFunction.creatLink(false);
        }

        int timeCount = 0;
        private void timer1_Tick(object sender, EventArgs e)
        {
            timeCount++;
            int delayMinute = Convert.ToInt32(textBox1.Text);
            if (timeCount >= delayMinute && isAutoStart.Equals("a"))
                appExit();
            Thread threadTime = new Thread(wallpaperMainFun);
            threadTime.IsBackground = true;  //设置为后台线程
            threadTime.Start();
        }

        private void Form1_Resize(object sender, EventArgs e)
        {
            if (this.WindowState == FormWindowState.Minimized)  //判断是否点击最小化窗口
            {
                notifyIcon1.Visible = true;   //显示在托盘区
                this.ShowInTaskbar = false;   //隐藏任务栏图标
            }

        }

        private void notifyIcon1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            this.WindowState = FormWindowState.Normal;  //还原窗体显示 
            this.Activate();  //激活窗体并给予它焦点 
            this.ShowInTaskbar = true;    //显示任务栏图标
            notifyIcon1.Visible = false;   //隐藏托盘图标
        }

        private void 主界面ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Normal;  //还原窗体显示 
            this.Activate();  //激活窗体并给予它焦点 
            this.ShowInTaskbar = true;    //显示任务栏图标
            notifyIcon1.Visible = false;   //隐藏托盘图标
        }

        private void 退出ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            appExit();
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            int minute;
            try
            {
                minute = Convert.ToInt32(textBox1.Text);
                textBox1.BackColor = Color.White;
                //SetFunction.IniWrite("general", "delayMinute", Convert.ToInt32(textBox1.Text).ToString(), imageInfo.iniPath);
                ConfigXML.CreateOrUpdateXmlNodeByXPath(imageInfo.XMLPath, @"config/general", "delayMinute", Convert.ToInt32(textBox1.Text).ToString());
            }
            catch
            {
                textBox1.BackColor = Color.Red;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            //ExeUpdate.checkUpdate(imageInfo.appUpdateAdd, imageInfo.updateDirectory + "\\" + imageInfo.MainUpdateFile);
            //MessageBox.Show( Path.GetFileName("http://74.121.151.188/catcatzone.com/BingWallpaper/update/ico/ico16X16.ico"));
            button1.Text = "检查中...";
            button1.Enabled = false;
            Thread up = new Thread(appExit);
            up.IsBackground = true;  //设置为后台线程
            up.Start();
            //appExit();
           
        }






    }
}
