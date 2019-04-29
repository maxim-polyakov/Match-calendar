using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Log
{
    public interface ILog
    {
        void LogInfo(string msg);
        void LogWarn(string msg);
        void LogError(string msg);
    }

    public class LogDummy : ILog
    {
        public void LogError(string msg)
        {
            Console.WriteLine("Error");
        }

        public void LogInfo(string msg)
        {
            Console.WriteLine("Info");
        }

        public void LogWarn(string msg)
        {
            Console.WriteLine("Warn");
        }

        public LogDummy(string path)
        {
            using (StreamWriter sw = new StreamWriter(path, false, System.Text.Encoding.Default))
            {
                sw.WriteLine(DateTime.Now + "Match-calendar start");
                sw.WriteLine("Loader: model is done");
                sw.WriteLine("Solver: answer is ready");

            }
        }
    }
}
