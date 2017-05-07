
using System;
using System.Windows.Forms;
using System.Net;
using System.IO;
using System.Xml;
using System.Collections;
using System.Runtime.InteropServices;
using System.Text;
using System.Collections.Generic;
using System.Xml.Linq;

public static class ExeUpdate
{
    //static IList<String> sectionList = new List<String>();   //本地文件名
    static Dictionary<string, string> localNameAndVersion = new Dictionary<string, string>();  //字典:本地文件名与版本
    static Dictionary<string, string[]> netNameAndVersion = new Dictionary<string, string[]>();   //字典:网络文件名与版本和下载链接，通过网络与本地版本对比，判断是否需要更新
    //static ArrayList downFileAdd = new ArrayList(); //“app更新升级软件” 网络下载地址。主程序只更新升级软件，主程序更新在升级软件里完成（update.exe）

    public static void getLocalVersion()
    {
        XmlNode node = ConfigXML.GetXmlNodeByXpath(imageInfo.XMLPath, @"config/version");  //获取版本节点集合
        foreach (XmlNode childNode in node)    //获取文件名和版本号
            localNameAndVersion.Add(childNode.Name, childNode.InnerText);   
    }
    
    public static void gewNetVersion()
    {
        try
        {
            WebClient wc = new WebClient();
            Stream stream = wc.OpenRead(imageInfo.appUpdateAdd);
            XmlDocument xmlDoc = new XmlDocument();
            //XDocument xmlDoc = XDocument.Load(imageInfo.appUpdateAdd);
           xmlDoc.Load(stream); MessageBox.Show(imageInfo.appUpdateAdd);
            XmlNode xmlNode = xmlDoc.SelectSingleNode("update");
            
            foreach (XmlNode childNode in xmlNode)
            {
                string name = childNode.Name;
                string[] versionAndDownLoad = new string[2];
                foreach (XmlNode info in childNode)
                {
                    MessageBox.Show(info.Name);
                    if (info.Name == "version")
                        versionAndDownLoad[0] = info.InnerText;
                    else if (info.Name == "download")
                        versionAndDownLoad[1] = info.InnerText;
                }
                MessageBox.Show(name + "  " + versionAndDownLoad.ToString());
                netNameAndVersion.Add(name, versionAndDownLoad);
            }
        }
        catch
        {
            MessageBox.Show("更新错误");
        }
    }

    public static Boolean checkUpdate(string url, string path)
    {
        int i = 0;
        string readLine;
        string XMLDown="";
        string headAdd = imageInfo.updateDirectory + @"\head.html";
        string XMLPath=imageInfo.updateDirectory+"\\"+imageInfo.MainUpdateFile;
        if (!Directory.Exists(imageInfo.updateDirectory))
            Directory.CreateDirectory(imageInfo.updateDirectory);

        //(如果域名商提供Forward with Masking，则下载包含下载XML地址的html文件，然后解析里面的xml下载地址。xml第二行为update)
        DownLoadFile(imageInfo.appUpdateAdd, headAdd); 

        if (!File.Exists(headAdd))  //文件不存在，下载失败
        {
            
            return false;
        }

        FileStream fs = new FileStream(headAdd, FileMode.Open);
        StreamReader sr = new StreamReader(fs);
        while ((readLine = sr.ReadLine()) != null)  //获取web端的XML文件下载地址
        {
            i++;
            if (readLine.Contains(imageInfo.MainUpdateFile))
            {
                int index = readLine.IndexOf(@"http://");
                int lastIndex = readLine.LastIndexOf(imageInfo.MainUpdateFile);
                XMLDown = readLine.Substring(index, lastIndex);
                break;
            }
            if (i==2 &&readLine.Contains("update"))
            {
                sr.Close();
                if (File.Exists(XMLPath))
                    File.Delete(XMLPath);
                File.Move(headAdd, XMLPath);  //重命名为MainUpdate.xml
                break;
            }
        }
        if (i != 2)
        {
            sr.Close();
            if (XMLDown == "")
                return false;  //未找到下载地址，文件下载失败
            DownLoadFile(XMLDown, XMLPath);  //下载配置文件
            if (!File.Exists(XMLPath))  //文件不存在，下载失败
                return false;
        }

        string NetAppVersion = ConfigXML.GetXmlNodeInnerTextByXpath(XMLPath, @"update/AppVersion/version");  //web端版本号
        string localAppVersion = ConfigXML.GetXmlNodeInnerTextByXpath(imageInfo.XMLPath, @"config/general/AppVersion");  //本地版本号
        if (localAppVersion.CompareTo(NetAppVersion) < 0)    //版本号比较
            return true;
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
        catch { }
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

    public static void downFile(string fileName)
    {
 
    } 
}

   