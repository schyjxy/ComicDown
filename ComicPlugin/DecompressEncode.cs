using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Security;

namespace ComicPlugin
{
    class DecompressEncode
    {
        
        public static string DealAll(string host, Stream inputStream, string contentEncoding, Encoding encode = null)
        {
            var retString = "";
            if (encode == null)
            {
                encode = Encoding.UTF8;
            }

            switch (contentEncoding)
            {
                case "gzip": retString = DecompressGzip(inputStream, encode); break;
                case "deflate": retString = DecompressDeflate(inputStream, encode); break;
                default: retString = NoCompress(inputStream, encode); break;
            }

            return retString;
        }

        public static string DecompressGzip(Stream inputStream, Encoding encode)
        {
            string retStr = "";
            int pos = 0;
            byte[] buffer = new byte[1024];
            MemoryStream memoryStream = new MemoryStream();
            GZipStream stream = new GZipStream(inputStream, CompressionMode.Decompress);
            pos = stream.Read(buffer, 0, buffer.Length);

            while (pos > 0)
            {
                memoryStream.Write(buffer, 0, pos);
                pos = stream.Read(buffer, 0, buffer.Length);
            }

            retStr = encode.GetString(memoryStream.ToArray());
            memoryStream.Dispose();
            memoryStream.Close();
            stream.Close();
            return retStr;
        }

        public static Stream DecompressGzipStream(Stream inputStream)
        {
            GZipStream stream = new GZipStream(inputStream, CompressionMode.Decompress);
            return stream;
        }

        public static Stream DecompressDeflateStream(Stream inputStream)
        {
            DeflateStream stream = new DeflateStream(inputStream, CompressionMode.Decompress);
            return stream;
        }

        public static string DecompressDeflate(Stream inputStream, Encoding encode)
        {
            string retStr = "";
            int pos = 0;
            byte[] buffer = new byte[1024];
            MemoryStream memoryStream = new MemoryStream();
            DeflateStream stream = new DeflateStream(inputStream, CompressionMode.Decompress);
            pos = stream.Read(buffer, 0, buffer.Length);

            while (pos > 0)
            {
                memoryStream.Write(buffer, 0, pos);
                pos = stream.Read(buffer, 0, buffer.Length);
            }

            retStr = encode.GetString(memoryStream.ToArray());
            memoryStream.Dispose();
            memoryStream.Close();          
            stream.Dispose();
            stream.Close();
            return retStr;
        }

        public static string NoCompress(Stream stream, Encoding encode)
        {
            StreamReader reader = new StreamReader(stream, encode);
            return reader.ReadToEnd();
        }

    }
}
