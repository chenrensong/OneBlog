using System;
using System.DrawingCore;
using System.DrawingCore.Drawing2D;
using System.DrawingCore.Imaging;

namespace OneBlog.Helpers
{
    public class ValidateHelper
    {
        private static byte[] randb = new byte[4];
        private static readonly Random rand = new Random();
        private static Matrix m = new Matrix();
        private static Bitmap charbmp = new Bitmap(25, 25);

        private static Font[] fonts = {
                                        new Font(new FontFamily("Times New Roman"), 16 + Next(3), FontStyle.Regular),
                                        new Font(new FontFamily("Georgia"), 16 + Next(3), FontStyle.Regular),
                                        new Font(new FontFamily("Arial"), 16 + Next(3), FontStyle.Regular),
                                        new Font(new FontFamily("Comic Sans MS"), 16 + Next(3), FontStyle.Regular)
                                     };

        /// <summary>
        /// 获得下一个随机数
        /// </summary>
        /// <param name="max">最大值</param>
        /// <returns></returns>
        private static int Next(int max)
        {
            rand.NextBytes(randb);
            int value = BitConverter.ToInt32(randb, 0);
            value = value % (max + 1);
            if (value < 0)
                value = -value;
            return value;
        }


        /// <summary>
        /// 获得下一个随机数
        /// </summary>
        /// <param name="min">最小值</param>
        /// <param name="max">最大值</param>
        /// <returns></returns>
        private static int Next(int min, int max)
        {
            int value = Next(max - min) + min;
            return value;
        }

        /// <summary>
        /// 生成验证码
        /// </summary>
        /// <param name="length">指定验证码的长度</param>
        /// <returns></returns>
        public static string CreateValidateCode(int length)
        {
            int[] randMembers = new int[length];
            int[] validateNums = new int[length];
            string validateNumberStr = "";
            //生成起始序列值
            int seekSeek = unchecked((int)DateTime.Now.Ticks);
            Random seekRand = new Random(seekSeek);
            int beginSeek = (int)seekRand.Next(0, Int32.MaxValue - length * 10000);
            int[] seeks = new int[length];
            for (int i = 0; i < length; i++)
            {
                beginSeek += 10000;
                seeks[i] = beginSeek;
            }
            //生成随机数字
            for (int i = 0; i < length; i++)
            {
                Random rand = new Random(seeks[i]);
                int pownum = 1 * (int)Math.Pow(10, length);
                randMembers[i] = rand.Next(pownum, Int32.MaxValue);
            }
            //抽取随机数字
            for (int i = 0; i < length; i++)
            {
                string numStr = randMembers[i].ToString();
                int numLength = numStr.Length;
                Random rand = new Random();
                int numPosition = rand.Next(0, numLength - 1);
                validateNums[i] = Int32.Parse(numStr.Substring(numPosition, 1));
            }
            //生成验证码
            for (int i = 0; i < length; i++)
            {
                validateNumberStr += validateNums[i].ToString();
            }
            return validateNumberStr;
        }

        /// <summary>
        /// 创建验证码的图片
        /// </summary>
        /// <param name="validateNum">验证码</param>
        public static byte[] CreateValidateGraphic(string validateCode)
        {

            if (string.IsNullOrEmpty(validateCode)) return null;

            var image = new Bitmap((int)Math.Ceiling((validateCode.Length * 22.5)), 40);
            Graphics g = Graphics.FromImage(image);

            g.SmoothingMode = SmoothingMode.HighSpeed;
            g.Clear(Color.White);
            try
            {
                Random random = new Random();       //生成随机生成器
                g.Clear(Color.White);               //清空图片背景色
                for (int i = 0; i < 2; i++)             //画图片的背景噪音线
                {
                    int x1 = random.Next(image.Width);
                    int x2 = random.Next(image.Width);
                    int y1 = random.Next(image.Height);
                    int y2 = random.Next(image.Height);
                    g.DrawLine(new Pen(Color.Black), x1, y1, x2, y2);
                }

                //输出文字
                //Font font = new System.Drawing.Font("Arial", 12, (System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Italic));
                //System.Drawing.Drawing2D.LinearGradientBrush brush = new System.Drawing.Drawing2D.LinearGradientBrush(new Rectangle(0, 0, image.Width, image.Height), Color.Gray, Color.Blue, 1.2f, true);
                //g.DrawString(checkCode, font, brush, 2, 2);

                Graphics charg = Graphics.FromImage(charbmp);

                SolidBrush drawBrush = new SolidBrush(Color.FromArgb(Next(100), Next(100), Next(100)));
                float charx = -18;
                for (int i = 0; i < validateCode.Length; i++)
                {
                    m.Reset();
                    m.RotateAt(Next(30) - 25, new PointF(Next(3) + 7, Next(3) + 7));

                    charg.Clear(Color.Transparent);
                    charg.Transform = m;
                    //定义前景色为黑色
                    drawBrush.Color = Color.Black;

                    charx = charx + 20 + Next(2);
                    PointF drawPoint = new PointF(charx, 0.1F);
                    charg.DrawString(validateCode[i].ToString(), fonts[Next(fonts.Length - 1)], drawBrush, new PointF(0, 0));

                    charg.ResetTransform();

                    g.DrawImage(charbmp, drawPoint);
                }


                //画图片的前景噪音点
                for (int i = 0; i < 25; i++)
                {
                    int x = random.Next(image.Width);
                    int y = random.Next(image.Height);

                    image.SetPixel(x, y, Color.FromArgb(random.Next()));
                }

                //画图片的边框线
                g.DrawRectangle(new Pen(Color.Silver), 0, 0, image.Width - 1, image.Height - 1);

                //输出
                System.IO.MemoryStream ms = new System.IO.MemoryStream();
                image.Save(ms, ImageFormat.Jpeg);
                return ms.ToArray();
            }
            finally
            {
                g.Dispose();
                image.Dispose();
            }
            //Bitmap image = new Bitmap((int)Math.Ceiling(validateCode.Length * 12.0), 22);
            //Graphics g = Graphics.FromImage(image);
            //try
            //{
            //    //生成随机生成器
            //    Random random = new Random();
            //    //清空图片背景色
            //    g.Clear(Color.White);
            //    //画图片的干扰线
            //    for (int i = 0; i < 25; i++)
            //    {
            //        int x1 = random.Next(image.Width);
            //        int x2 = random.Next(image.Width);
            //        int y1 = random.Next(image.Height);
            //        int y2 = random.Next(image.Height);
            //        g.DrawLine(new Pen(Color.Silver), x1, y1, x2, y2);
            //    }
            //    Font font = new Font("Arial", 12, (FontStyle.Bold | FontStyle.Italic));
            //    LinearGradientBrush brush = new LinearGradientBrush(new Rectangle(0, 0, image.Width, image.Height),
            //     Color.Blue, Color.DarkRed, 1.2f, true);
            //    g.DrawString(validateCode, font, brush, 3, 2);
            //    //画图片的前景干扰点
            //    for (int i = 0; i < 100; i++)
            //    {
            //        int x = random.Next(image.Width);
            //        int y = random.Next(image.Height);
            //        image.SetPixel(x, y, Color.FromArgb(random.Next()));
            //    }
            //    //画图片的边框线
            //    g.DrawRectangle(new Pen(Color.Silver), 0, 0, image.Width - 1, image.Height - 1);
            //    //保存图片数据
            //    MemoryStream stream = new MemoryStream();
            //    image.Save(stream, ImageFormat.Jpeg);
            //    //输出图片流
            //    return stream.ToArray();
            //}
            //finally
            //{
            //    g.Dispose();
            //    image.Dispose();
            //}
        }


        /// <summary>
        /// 得到验证码图片的长度
        /// </summary>
        /// <param name="validateNumLength">验证码的长度</param>
        /// <returns></returns>
        public static int GetImageWidth(int validateNumLength)
        {
            return (int)(validateNumLength * 12.0);
        }


    }
}
