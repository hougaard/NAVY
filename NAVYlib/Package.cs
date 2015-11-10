using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NAVYlib
{
    [Serializable]
    public class OnlinePackage
    {
        public string name { get; set; }
        public string description { get; set; }
        public string owner { get; set; }
        public dynamic version { get; set; }
    }
}
