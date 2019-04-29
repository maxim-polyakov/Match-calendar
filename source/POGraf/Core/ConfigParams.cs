using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Core
{
    public class ConfigParams
    {
        bool flag;
        public string logFile;
        public string algorythm;
        public int iterations;

        public ConfigParams(bool flag)
        {
            this.flag = false;
        }

        public static ConfigParams InvalidParams()
        {
            return new ConfigParams(false);
        }
    }
}
