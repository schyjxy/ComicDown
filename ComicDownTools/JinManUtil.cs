using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Media.Imaging;
using System.IO;

namespace ComicDownTools
{
    class JinManUtil
    {
        public static string md5(string str)
        {
            try
            {
                MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();
                byte[] bytValue, bytHash;
                bytValue = System.Text.Encoding.UTF8.GetBytes(str);
                bytHash = md5.ComputeHash(bytValue);
                md5.Clear();
                string sTemp = "";

                for (int i = 0; i < bytHash.Length; i++)
                {
                    sTemp += bytHash[i].ToString("X").PadLeft(2, '0');
                }
                str = sTemp.ToLower();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            return str;
        }

        private static void WebpToJpeg(string path)
        {
            using (Stream ms = new MemoryStream(File.ReadAllBytes(path)))
            {
                BitmapImage bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.StreamSource = ms;
                bitmap.EndInit();
                bitmap.Freeze();
             
                ms.Dispose();
                ms.Close();

                BitmapEncoder encoder = new JpegBitmapEncoder();                
                encoder.Frames.Add(BitmapFrame.Create(bitmap));
                FileStream fileStream = new FileStream(@"C:\Users\hang\Desktop\00002.jpg", FileMode.Create);
                encoder.Save(fileStream);
            }
        }

        public static void ScrambleImage(int aid, int scrambleId, string imgHtml)
        {
            var count = GetNum(aid, "00001");
            var path = @"C:\Users\hang\Desktop\00002.webp";
            WebpToJpeg(path);

            Image sourceBmp = Image.FromFile(@"C:\Users\hang\Desktop\00002.jpg");
            Bitmap newBmp = new Bitmap(sourceBmp.Width, sourceBmp.Height);
            Graphics graphics = Graphics.FromImage(newBmp);

            int picHeight = sourceBmp.Height;
            int leftHeight = sourceBmp.Height % count;

            RectangleF srcRect = new RectangleF();
            RectangleF destRect = new RectangleF();

            for (int i = 0; i < count; i++)
            {
                var spliceHeight = (int)Math.Floor(sourceBmp.Height * 1.0 / count);
                var srcY = picHeight - spliceHeight * (i + 1) - leftHeight;
                var destY = spliceHeight * i;

                if (i == 0)
                {
                    spliceHeight += leftHeight;
                    destY += leftHeight;
                }

                srcRect.X = 0;
                srcRect.Y = srcY;
                srcRect.Height = spliceHeight;
                srcRect.Width = sourceBmp.Width;

                destRect.X = 0;
                destRect.Y = destY;
                destRect.Height = spliceHeight;
                destRect.Width = sourceBmp.Width;

                graphics.DrawImage(sourceBmp, srcRect, destRect, GraphicsUnit.Pixel);
            }

            graphics.Dispose();
            newBmp.Save("test.jpg");
            sourceBmp.Dispose();
            newBmp.Dispose();
        }

        //输入27001，“00001”， return 20
        public static int GetNum(int e, string t)
        {
            var a = 10;
            if (e >= 268850)
            {
                var n = e.ToString() + t;
                var temp = md5(n);
                temp = temp.Substring(temp.Length - 1);
                byte[] number = Encoding.UTF8.GetBytes(temp);

                switch (number[0] %= 10)
                {
                    case 0:
                        a = 2;
                        break;
                    case 1:
                        a = 4;
                        break;
                    case 2:
                        a = 6;
                        break;
                    case 3:
                        a = 8;
                        break;
                    case 4:
                        a = 10;
                        break;
                    case 5:
                        a = 12;
                        break;
                    case 6:
                        a = 14;
                        break;
                    case 7:
                        a = 16;
                        break;
                    case 8:
                        a = 18;
                        break;
                    case 9:
                        a = 20; break;
                }
            }
            return a;
        }
    }
}
