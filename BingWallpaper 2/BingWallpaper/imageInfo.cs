using System;
using System.Windows.Forms;

public static class imageInfo
{
    public static string url = "HPImageArchive.aspx?format=js&idx=0&n=1";    //图片地址
    public static string domainBing = "http://www.bing.com";       //bing主页
    public static string pingAdd = "www.baidu.com";  //检测网络连接地址（ping)
    public static string appUpdateAdd = @"http://www.lovecatcat.com/BingWallpaper/update/MainUpdate.xml";  //app更新地址
    public static string imageSaveDirectory = Application.StartupPath + "\\壁纸\\";
    public static string updateDirectory = Application.StartupPath + "\\temp";
    public static string MainUpdateFile = "MainUpdate.xml";
    public static string appBackgroundImag = Application.StartupPath + "\\ico\\background.bmp";
    public static string BmpAdd = imageInfo.imageSaveDirectory + DateTime.Now.ToLongDateString().ToString() + ".bmp";  //bmp文件路径"
    public static string imageName = "";
    public static string XMLPath = Application.StartupPath + "\\config.xml";// 配置文件路径

    public static string imageSaveJpgPath()
    {
        return imageSaveDirectory + imageName;
    }
    //public static string imageSaveBmpPath()
    //{
    //    string imagePath = imageSaveJpgPath();
    //    string newBmp = imagePath.Substring(0, imagePath.Length - 3) + "bmp";
    //    return newBmp;
    //}
}
