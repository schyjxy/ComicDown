using ComicDownWpf.decoder;
using ComicDownWpf.pages;
using System.Collections.Generic;
using ComicPlugin;
using System.Windows.Controls;
using System.IO;
using System;

namespace ComicDownWpf
{
    class CommonManager
    {
        public static ComicCollect comicCollectPage = new ComicCollect();
        public static ComicPage comicInfoPage = new ComicPage();
        public static DownLoadInfoPage downLoadPage = new DownLoadInfoPage();
        public static ComicItemPage mainPage = new ComicItemPage();
        public static HistoryPage historyPage = new HistoryPage();
        public static SearchPage searchPage = new SearchPage();
        public static DownCharpterPage downCharpterPage = new DownCharpterPage();
        public static string g_temp_path = @"E:\漫画\";
        public static string g_cache_path = g_temp_path + @"cache\";
        public static string g_download_path = g_temp_path + @"download\";

        public static string CacheImage(string url, string subPath)
        {
            if(File.Exists(url))
            {
                return url;
            }
            HttpUtils httpUtils = new HttpUtils();
            string path = CommonManager.g_cache_path + subPath;
            string fileName = DateTime.Now.ToString("yyyy_mm_dd_HH_mm_ss") + ".jpg";

            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            string fullPath = string.Format(@"{0}\{1}", path, fileName);

            if (!httpUtils.DownLoadImage(url, fullPath))
            {
                return url;
            }
            return fullPath;
        }


        public static void Init()
        {
            ParserManager.Init();
        }

        public static void Close()
        {
            ParserManager.Close();
        }

    }
}
