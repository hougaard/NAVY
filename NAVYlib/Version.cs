using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NAVYlib
{
    public class Version
    {
        public string name { get; set; }
        public string version { get; set; }
        public string description { get; set; }
        public int min_nav_buildno { get; set; }
        public int max_nav_buildno { get; set; }
        public bool ispublic {get; set; }
        public DateTime timestamp { get; set; }
        public byte[] package { get; set; }
        public static int Compare(string x, string y)
        {
            if (x == y) return 0;
            var version = new { First = GetVersion(x), Second = GetVersion(y) };
            int limit = Math.Max(version.First.Length, version.Second.Length);
            for (int i = 0; i < limit; i++)
            {
                int first = version.First.ElementAtOrDefault(i);
                int second = version.Second.ElementAtOrDefault(i);
                if (first > second) return 1;
                if (second > first) return -1;
            }
            return 0;
        }

        private static int[] GetVersion(string version)
        {
            return (from part in version.Split('.')
                    select Parse(part)).ToArray();
        }

        private static int Parse(string version)
        {
            int result;
            int.TryParse(version, out result);
            return result;
        }
    }
}
