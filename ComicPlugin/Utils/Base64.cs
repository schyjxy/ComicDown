using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ComicPlugin.Utils
{
    public class Base64
    {
        public static string DecodeBase64(Encoding encoding, string code, bool isUriSafe = false)
        {
            string decode = "";
            if(isUriSafe)
            {
                code = code.Replace("-", "+").Replace("_", "/");
            }

            byte[] bytes = Convert.FromBase64String(code);
            try
            {
                decode = encoding.GetString(bytes);
            }
            catch
            {
                decode = code;
            }
            return decode;
        }
    }
}
