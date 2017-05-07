using System;
using System.Net.Http;
using Newtonsoft.Json;
using System.IO;
using System.Runtime.InteropServices;
using System.Drawing.Imaging;
using System.Drawing;
using System.Windows.Forms;
using System.Text;

public static class SetFunction
{
    #region 引用user32.dll包
    [DllImport("user32", EntryPoint = "SystemParametersInfo")]
    //设置桌面背景墙纸，SystemParametersInfo(20, True, 图片路径, 1)
    public static extern int SystemParametersInfo(int uAction, int uParam, string lpvParam, int fuWinIni); 
    #endregion


    static Bitmap bitmap;
    static string autoStartDirectory = Environment.GetFolderPath(System.Environment.SpecialFolder.Startup);   //获取自启文件夹路径
    static string ProgramName = Path.GetFileName(Application.ExecutablePath);    
    //public static string isAutoStart;

    public static int setWallpaper()   //设置壁纸
    {
       // string bmpPath = imageInfo.imageSaveBmpPath();
      //  string bmpFullPath =  bmpPath;
       // MessageBox.Show(imageInfo.BmpAdd);
        return SystemParametersInfo(20, 0, imageInfo.BmpAdd, 1);
    }

    public static Bitmap getImage()
    {
        HttpClient client = new HttpClient();
        client.BaseAddress = new Uri(imageInfo.domainBing);
        string json = client.GetStringAsync(imageInfo.url).Result;
        dynamic data = JsonConvert.DeserializeObject(json);
        string imageUrl = data.images[0].url;
        string imageName = Path.GetFileName(imageUrl);
        imageInfo.imageName = DateTime.Now.ToLongDateString().ToString() + imageName.Substring(imageName.Length - 4);    //获取年月日
        if (File.Exists(imageInfo.BmpAdd) == false)
        {
            if (File.Exists(imageInfo.imageSaveJpgPath()))
                File.Delete(imageInfo.imageSaveJpgPath());
            byte[] buffer = client.GetByteArrayAsync(imageUrl).Result;
            File.WriteAllBytes(imageInfo.imageSaveJpgPath(), buffer);
            if (jpgToBmp())  //成功转换
                return new Bitmap(imageInfo.BmpAdd);
        }
        return null;
    }

    public static Boolean jpgToBmp()
    {
        string jpgPath = imageInfo.imageSaveJpgPath();
        string bmpPath = imageInfo.BmpAdd;
        if (File.Exists(jpgPath))  //判断jpg是否已下载
        {
            bitmap = new Bitmap(jpgPath);
            bitmap.Save(bmpPath, ImageFormat.Bmp);   //转bmp格式
            bitmap.Dispose();
            File.Delete(imageInfo.imageSaveJpgPath());
            return true;
        }
        return false;
       
    }

    public static void creatLink(Boolean YON)   //true创建开机自启快捷方式, false删除开机自启快捷方式
    {
        

        if (!Directory.Exists(autoStartDirectory))   //自启目录不存在，创建
            Directory.CreateDirectory(autoStartDirectory);
        if (YON)
        {
            LinkCreate();
        }
        else
            LinkDelete();

        if (File.Exists(autoStartDirectory + "/" + ProgramName))
            return;

            //MessageBox.Show(autoStartDirectory + "/" + ProgramName);

    }

    //需要引入IWshRuntimeLibrary，搜索Windows Script Host Object Model
        /// 创建快捷方式
    /// <param name="autoStartDirectory">快捷方式所处的文件夹</param>
    /// <param name="ProgramName">快捷方式名称</param>
    /// <param name="targetPath">目标路径</param>
    /// <param name="description">描述</param>
    /// <param name="iconLocation">图标路径，格式为"可执行文件或DLL路径, 图标编号"，
    /// 例如System.Environment.SystemDirectory + "\\" + "shell32.dll, 165"</param>
    public static void LinkCreate()
    {
        string fullPath = autoStartDirectory + "/" + ProgramName;
        LinkDelete();//删除旧的快捷方式

        string targetPath = System.Reflection.Assembly.GetExecutingAssembly().Location; //程序路径
        string shortcutPath = Path.Combine(autoStartDirectory, string.Format("{0}.lnk", ProgramName));
        IWshRuntimeLibrary.WshShell shell = new IWshRuntimeLibrary.WshShell();
        IWshRuntimeLibrary.IWshShortcut shortcut = (IWshRuntimeLibrary.IWshShortcut)shell.CreateShortcut(shortcutPath);//创建快捷方式对象
        shortcut.TargetPath = targetPath;//指定程序路径
        shortcut.WorkingDirectory = Path.GetDirectoryName(targetPath);//设置起始位置
        shortcut.WindowStyle = 7;//目标应用程序的窗口状态分为普通、最大化、最小化【1,3,7】
        shortcut.Description = "";//设置备注
        shortcut.Arguments = "a";  //设置应用程序的启动参数
        shortcut.IconLocation = Application.StartupPath+"\\ico\\ico16X16.ico"; //设置图标路径
        //MessageBox.Show(Application.StartupPath);
        shortcut.Save();//保存快捷方式
       // MessageBox.Show(System.Reflection.Assembly.GetExecutingAssembly().Location);
    }

    public static void LinkDelete()
    {
        string linkPath = autoStartDirectory + "/" + ProgramName + ".lnk";
        if (linkExist())  //存在 
            File.Delete(linkPath);
    }

    public static Boolean linkExist()
    {
        string linkPath = autoStartDirectory + "/" + ProgramName + ".lnk";
        if (File.Exists(linkPath))  //存在 
            return true;

        return false;
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

    public static void DeleteDirectory(string path)
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

}
