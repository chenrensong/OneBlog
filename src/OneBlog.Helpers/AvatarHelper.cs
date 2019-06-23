using SS.Toolkit.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OneBlog.Helpers
{
    public class AvatarHelper
    {
        private static IList<string> Defalut = new List<string>();

        static AvatarHelper()
        {
            for (int i = 1; i <= 10; i++)
            {
                Defalut.Add(string.Format("http://cdn.datiancun.com/static/avatar/{0}.png", i));
            }
        }

        /// <summary>
        /// 随机头像
        /// </summary>
        /// <returns></returns>
        public static string GetRandomAvatar()
        {
            Random r = new Random(GuidHelper.Gen().GetHashCode());
            var number = r.Next(0, 9);
            return Defalut[number];
        }



    }
}
