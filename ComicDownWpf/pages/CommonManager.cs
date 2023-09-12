using ComicDownWpf.decoder;
using ComicDownWpf.pages;
using System.Collections.Generic;
using ComicPlugin;
using System.Windows.Controls;
using System.IO;
using System;
using Newtonsoft.Json;
using System.Windows.Forms;

namespace ComicDownWpf
{
    class CacheConfig
    { 
        public string RootPath { get; set; }
    }

    class CommonManager
    {
        public static ComicCollect comicCollectPage = new ComicCollect();
        public static ComicPage comicInfoPage = new ComicPage();
        public static DownLoadInfoPage downLoadPage = new DownLoadInfoPage();
        public static ComicItemPage mainPage = new ComicItemPage();
        public static HistoryPage historyPage = new HistoryPage();
        public static SearchPage searchPage = new SearchPage();
        public static DownCharpterPage downCharpterPage = new DownCharpterPage();
        public static string g_temp_path;
        public static string g_cache_path;
        public static string g_download_path;
        private static string config_path = "config.json";

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

        public static CacheConfig LoadConfig()
        { 
            string text =  File.ReadAllText(config_path);
            CacheConfig config = JsonConvert.DeserializeObject<CacheConfig>(text);
            return config;
        }

        public static void SaveConfig(CacheConfig config) 
        {
            string json = JsonConvert.SerializeObject(config);
            File.WriteAllText(config_path, json);
        }

        public static void Init()
        {
            CacheConfig config = null;

            if (File.Exists("config.json"))
            {
                config = LoadConfig();
            }
            else 
            {
                config = new CacheConfig();
                config.RootPath = Directory.GetCurrentDirectory();
                SaveConfig(config);
            }

            g_temp_path = config.RootPath;
            g_cache_path = g_temp_path + @"cache\";
            g_download_path = g_temp_path + @"download\";
            ParserManager.Init();
        }

        public static void Close()
        {
            ParserManager.Close();
        }

    }
}
