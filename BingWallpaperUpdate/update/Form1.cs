using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Threading;
using System.Net;
using System.Xml;
using System.Runtime.InteropServices;
using System.Net.NetworkInformation;

namespace update
{
    public partial class Form1 : Form
    {
        string pingAdd = "www.baidu.com";   //检测网络连接地址
        string updateDirectry = Application.StartupPath + "\\temp";// 配置文件目录
        string localXMLPath = Application.StartupPath + "\\config.xml";// 本地配置文件路径
        string netXMLPath = Application.StartupPath + "\\temp\\MainUpdate.xml";// 网络配置文件路径
        //web上的名字、版本、及下载地址
        Dictionary<string, string> netNameVersionDownload = new Dictionary<string, string>();
        //本地名字、版本
        Dictionary<string, string> localNameVersion = new Dictionary<string, string>();
        Thread thread;

        [DllImport("wininet")]
        //判断网络状况的方法,返回值true为连接，false为未连接  
        public extern static bool InternetGetConnectedState(out int conState, int reder);


        public Form1()
        {
            InitializeComponent();
            if (!Directory.Exists(Application.StartupPath + "\\ico"))
                Directory.CreateDirectory(Application.StartupPath + "\\ico");
            if (CheckServeStatus())  //网络连接检测
            {
                thread = new Thread(update);
                //thread.IsBackground = true;  //设置为后台线程
                thread.Start();
            }
            else
            {
                label1.Text = "网络连接失败";
                button1.Text = "退出";
            }
            
        }

        public  Boolean CheckServeStatus()
        {
            int errCount = 0;//ping时连接失败个数
            System.Int32 dwFlag = new Int32();
            string urls = pingAdd;

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
                    // MessageBox.Show("网络不稳定");
                    return true;
                }
            }
            else
            {
                // MessageBox.Show("网络正常");
                return true;
            }
        }

        public  bool MyPing(string urls, out int errorCount)
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

		public void update()
		{

            Boolean flag = false;
            string download;
            try
            {
                getAllVersion();
               
                int len = netNameVersionDownload.Count;  //文件个数
                int i = 0; 
                foreach (string key in netNameVersionDownload.Keys)   //遍历web端的版本信息
                {

                    i++; 
                    
                    //本地不包含web端的文件  或者 本地版本号小于web端
                    if (!localNameVersion.Keys.Contains(key) ||
                        (localNameVersion.Keys.Contains(key) && (localNameVersion[key].CompareTo(netNameVersionDownload[key].Split(' ')[0]) < 0)))
                    {
                        if (key == "image")   //程序图标等
                        {
                            string[] downs = netNameVersionDownload[key].Split(' ');  //下载地址数组
                            for (int num = 1; num < downs.Length; num++)   //第0个是版本号
                            {
                                if (downs[num].Trim() != string.Empty)  //去掉空格干扰，分割后可能会出现一个空格
                                {
                                    DownLoadFile(downs[num], updateDirectry + "\\" + Path.GetFileName(downs[num]));
                                    if (!File.Exists(updateDirectry + "\\" + Path.GetFileName(downs[num])))
                                    {
                                        MessageBox.Show(downs.Length + Path.GetFileName(downs[num]).Length + "1下载失败", "失败");
                                        Application.Exit();
                                    }
                                    else  //移动文件
                                        moveFile(updateDirectry + "\\" + Path.GetFileName(downs[num]), Application.StartupPath + "\\ico\\" + Path.GetFileName(downs[num]));//"\\ico");
                                }
                            }
                        }
                        else
                        {
                            download = netNameVersionDownload[key].Split(' ')[1];  //获取下载地址
                            if (File.Exists(updateDirectry + "\\" + key))  //下载前文件存在就删除
                                File.Delete(updateDirectry + "\\" + key);

                            DownLoadFile(download, updateDirectry + "\\" + key);  //下载文件
                            //MessageBox.Show(netNameVersionDownload[key]);
                            //MessageBox.Show(download);
                            if (!File.Exists(updateDirectry + "\\" + key))   //文件下载失败
                            {
                                MessageBox.Show(key + "2下载失败", "失败");
                                Application.Exit();
                            }
                        }
                    }
                    if ((Double)i / len * 100 <= 100)  //进度条
                    {
                        setLabel1Text(Math.Round(((Double)i / len * 100), 0) + "%");
                        setProgressBar1Text(Math.Round(((Double)i / len * 100),0) + "");
                    }
                }
                setLabel1Text(100 + "%");
                setProgressBar1Text(100 + "");
                
                flag = true;

            }
            finally
            {
                if (flag)
                {
                    foreach (string key in netNameVersionDownload.Keys)   //遍历web端的版本信息
                    {
                       moveFile(updateDirectry + "\\" + key, Application.StartupPath + "\\" + key);
                       //MessageBox.Show(tt + "");
                        //更新版本信息
                        ConfigXML.CreateOrUpdateXmlNodeByXPath(localXMLPath, @"config/version", key, netNameVersionDownload[key].Split(' ')[0]);
                    }
                    //更新主版本号
                    string NetAppVersion = ConfigXML.GetXmlNodeInnerTextByXpath(netXMLPath, @"update/AppVersion/version");
                    ConfigXML.CreateOrUpdateXmlNodeByXPath(localXMLPath, @"config/general", "AppVersion", NetAppVersion);
                    DeleteDirectory(updateDirectry); //删除temp文件夹
                    setButton1Text("退出");
                    setLabel1Text("更新完成！");
                }
                
            }
		}

        delegate void SetTextCallback(string data);
        private void setLabel1Text(string data)
        {
            if (true == label1.InvokeRequired)
            {
                SetTextCallback d = new SetTextCallback(setLabel1Text);
                this.Invoke(d, data);
            }
            else
            {
                label1.Text = data;
            }
        }
        private void setProgressBar1Text(string data)
        {
            if (true == label1.InvokeRequired)
            {
                SetTextCallback d = new SetTextCallback(setProgressBar1Text);
                this.Invoke(d, data);
            }
            else
            {
                progressBar1.Value = Convert.ToInt32(data);
            }
        }
        private void setButton1Text(string data)
        {
            if (true == label1.InvokeRequired)
            {
                SetTextCallback d = new SetTextCallback(setButton1Text);
                this.Invoke(d, data);
            }
            else
            {
                button1.Text = data;
            }
        }

        public static void DeleteDirectory(string path)
        {
            try
            {
                DirectoryInfo dir = new DirectoryInfo(path);
                if (dir.Exists)
                {
                    DirectoryInfo[] childs = dir.GetDirectories();
                    foreach (DirectoryInfo child in childs)
                    {
                        child.Delete(true);
                    }
                    dir.Delete(true);
                }
            }
            catch { }
        }

        public static Boolean moveFile(string fileSource, string fileDestination)
        {
            if (File.Exists(fileSource))
            {
                if (File.Exists(fileDestination))
                    File.Delete(fileDestination);
                if (!Directory.Exists(Path.GetDirectoryName(fileDestination)))
                    Directory.CreateDirectory(Path.GetDirectoryName(fileDestination));
                File.Move(fileSource, fileDestination);
                return true;
            }
            else
                return false;
        }

        public static void DownLoadFile(string url, string path)
        {
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                WebResponse response = request.GetResponse();
                Stream stream = response.GetResponseStream();

                byte[] buffer = new byte[1024];
                Stream outStream = CreateFile(path);
                Stream inStream = response.GetResponseStream();

                int l;
                do
                {
                    l = inStream.Read(buffer, 0, buffer.Length);
                    if (l > 0)
                        outStream.Write(buffer, 0, l);
                }
                while (l > 0);

                outStream.Close();
                inStream.Close();
            }
            catch {  }
        }

        public static FileStream CreateFile(string filePath)
        {
            string fileDirectory = Path.GetDirectoryName(filePath);
            if (!Directory.Exists(fileDirectory))
                Directory.CreateDirectory(fileDirectory);
            if (File.Exists(filePath))
                File.Delete(filePath);
            return File.Create(filePath);
        }

        private Boolean getAllVersion()  //读取本地及web文件的信息
        {
            //MessageBox.Show(localXMLPath);
            if (!(File.Exists(localXMLPath) || File.Exists(netXMLPath)))  //文件不存在
                return false;
            
            XmlNode localVersionNode = ConfigXML.GetXmlNodeByXpath(localXMLPath, @"config/version");
            if (localVersionNode == null) return false;  //无内容
            foreach (XmlNode child in localVersionNode)
            {
                if (!(child.Name == "update.exe") ) //排除已经升级过的程序
                    localNameVersion.Add(child.Name, child.InnerText);
            }
            //foreach (string key in localNameVersion.Keys)
             //   MessageBox.Show(key + " " + localNameVersion[key]);
            XmlNode netVersionNode = ConfigXML.GetXmlNodeByXpath(netXMLPath, "update");
            if (netVersionNode == null) return false; //无内容
            foreach (XmlNode appName in netVersionNode)
            {
                string versionDownload =string.Empty;   //version 空格 download
                if (appName.Name == "AppVersion" || appName.Name=="update.exe" )
                    continue;
                foreach (XmlNode child in appName)
                {
                   // MessageBox.Show(child.InnerText);
                    //if (child.Name == "version")
                        versionDownload += (child.InnerText+" ");
                    //else if (child.Name == "download")
                      //  versionDownload += (child.InnerText+" ");
                }
                netNameVersionDownload.Add(appName.Name, versionDownload);
            }
          //  foreach (string key in netNameVersionDownload.Keys)
           //     MessageBox.Show(key + " " + netNameVersionDownload[key]);

            return true;
        }


        private void button1_Click(object sender, EventArgs e)
        {
            if (button1.Text == "取消" && MessageBox.Show("取消更新？",
                                                "提示", MessageBoxButtons.YesNo, MessageBoxIcon.Asterisk) == DialogResult.Yes)
            {
                thread.Abort();
                Application.Exit();
            }
            else if (button1.Text == "退出")
            {
                Application.Exit();
            }
        }


	}
}

