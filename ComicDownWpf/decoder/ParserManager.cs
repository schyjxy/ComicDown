using ComicDownWpf.Engine;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using ComicPlugin;
using System.Linq;

namespace ComicDownWpf.decoder
{
    class ParserManager
    {
        static Dictionary<string, IComicDecoder> parseDict;
        private static void LoadPlugins()
        {
            string path = "plugins";

            if (Directory.Exists(path))
            {
                string[] files = Directory.GetFiles(path);

                foreach (string file in files)
                {
                    if (file.EndsWith(".dll"))
                    {
                        Assembly.LoadFile(System.IO.Path.GetFullPath(file));
                    }
                }
            }

            Type interfaceType = typeof(IComicDecoder);
            try
            {
                Type[] types = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(a => a.GetTypes())
                .Where(p => interfaceType.IsAssignableFrom(p) && p.IsClass)
                .ToArray();

                foreach (Type type in types)
                {
                    if (type.Name != "CommonParser" && parseDict.ContainsKey(type.Name) == false)
                    {
                        IComicDecoder decoder = (IComicDecoder)Activator.CreateInstance(type);
                        parseDict.Add(decoder.CodeName, decoder);
                    }
                }
            }
            catch (Exception ex)
            {              
                Console.WriteLine("载入解析器失败:" + ex.Message);
            }
      
        }

        public static void Init()
        {
            parseDict = new Dictionary<string, IComicDecoder>();
            LoadPlugins();
        }

        private static IComicDecoder FindParser(string codeName)
        {
            return parseDict[codeName];
        }

        public static IComicDecoder FindParserByCodeName(string parseName)
        {
            IComicDecoder parser = parseDict[parseName];
            return parser;
        }

        public static string GetHotComicUrl(string parseName)
        {
            var parse = parseDict[parseName];

            if(parse == null)
            {
                return null;
            }
             
            return parse.HotComicUrl;
        }

        public static List<IComicDecoder> GetParserList()
        {
           var parserList = new List<IComicDecoder>();
           foreach(var i in parseDict)
           {
                parserList.Add(i.Value);
           }

           return parserList;
        }

        public static async Task<List<ComicDetailInfo>> GetHotComic(string codeName, string url)
        {
            var list = FindParser(codeName)?.GetHotComic();
            return list;
        }

        public static ComicDetailInfo GetComicInfo(string codeName, string url)
        {
            var list = FindParser(codeName)?.GetComicInfo(url);
            return list;
        }

        public static SearchResult SearchComic(string codecName, string keyWord)
        {
            SearchResult result = new SearchResult();

            try
            {
                IComicDecoder parser = parseDict[codecName];
                result = parser?.SearchComic(keyWord);
                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine("SearchComic ：{0}", ex.Message);
            }

            return result;
        }

        public static bool DownLoadImage(string codecName, string path, string url)
        {
            try
            {
                IComicDecoder parser = parseDict[codecName];
                bool result = (bool)parser?.DownLoadImage(url, path);
                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine("ParseManager DownLoadImage: {0}", ex.Message);
            }
            
            return false;
        }

        public static List<string> GetImageList(string codeName, string url, GetOneImgHandler handler)
        {

            List<string> list = new List<string>();

            try
            {
                list = FindParser(codeName)?.GetComicImage(url, handler);
                return list;
            }
            catch (Exception ex)
            {
                Console.WriteLine("GetImageList,  {0}", ex.Message);
            }

            return list;
        }

        public static List<ComicDetailInfo> GoToPage(string url)
        {
            var list = FindParser(url)?.GoToPage(url);
            return list;
        }

        public static SearchResult GetMenu(string pareserName)
        {
            IComicDecoder decoder = parseDict[pareserName];
            var list = decoder.GetComicMenu();
            return list;
        }

        public static void Close()
        {
           
        }
    }
}
