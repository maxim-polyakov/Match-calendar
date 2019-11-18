using Core;
using Log;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Xml.Serialization;

namespace Loader
{
    public interface ILoader
    {
        ConfigParams Initialize(string configFile);
        Model Parse(string inputFile, string logName);
    }

    //public interface IBuilder
    //{
    //    void SetSimpleParams(Loader.sсhedule model);
    //    void SetS(Loader.sсhedule model);
    //    void SetDR(Loader.sсhedule model);
    //    void SetQ(Loader.sсhedule model);
    //    void SetV(Loader.sсhedule model);
    //    void SetT(Loader.sсhedule model);
    //    Model CreateModel(Loader.sсhedule model);  
    //}

    //public class ModelBuilder : IBuilder
    //{
    //    public Model trueModel;

    //    public ModelBuilder(string logName)
    //    {
    //        trueModel = new Model();
    //        LogGlobal.Join(Directory.GetCurrentDirectory() + "\\" + logName);

    //    }

    //    public Model CreateModel(Loader.sсhedule model)
    //    {
    //        this.SetSimpleParams(model);
    //        this.SetDR(model);
    //        this.SetQ(model);
    //        this.SetS(model);
    //        this.SetT(model);
    //        this.SetV(model);

    //        return trueModel;
    //    }

    //    public void SetSimpleParams(Loader.sсhedule model)
    //    {
    //        trueModel.n = model.teams.Length;

    //        int leaders = 0;
    //        for (int i = 0; i < model.teams.Length; i++)
    //        {
    //            if (model.teams[i].leader == true)
    //                leaders += 1;
    //        }
    //        trueModel.nl = leaders;
    //        LogGlobal.msg(0, "Teams : " + trueModel.n + " Leaders : " + trueModel.nl);

    //        trueModel.f = Convert.ToInt32(model.stadium.minGame);
    //        trueModel.g = Convert.ToInt32(model.stadium.maxGame);
    //        LogGlobal.msg(0, "Min game : " + trueModel.f + " Max game : " + trueModel.g);
    //    }

    //    public void SetS(Loader.sсhedule model)
    //    {
    //        trueModel.s = GetFavouriteTours(model);
    //    }

    //    public void SetDR(Loader.sсhedule model)
    //    {
    //        List<DateTime> days = GetGameDays(model);
    //        trueModel.gameDates = days;
    //        trueModel.d = days.Count;

    //        trueModel.r = days.Count - model.championship.reserv.Length;
    //        LogGlobal.msg(0, "Days : " + trueModel.d + " Days without reserv : " + trueModel.r);
    //    }
    //    public void SetT(Loader.sсhedule model)
    //    {
    //        trueModel.t = GetTimes(model);
    //    }

    //    public static List<DateTime> GetGameDays(Loader.sсhedule model)
    //    {
    //        List<DateTime> duration = new List<DateTime>();

    //        for (DateTime i = model.championship.start; i <= model.championship.end; i = i.AddDays(1))
    //        {
    //            for (int j = 0; j < model.championship.days.Length; j++)
    //            {
    //                if (i.DayOfWeek.ToString() == model.championship.days[j])
    //                {
    //                    duration.Add(i);
    //                    Console.WriteLine(i);
    //                    break;
    //                }
    //            }
    //        }

    //        List<DateTime> dur = duration;

    //        for (int k = 0; k < model.championship.decrees.Length; k++)
    //        {
    //            Console.WriteLine("Decree " + k + " : " + model.championship.decrees[k]);
    //            for (int l = 0; l < duration.Count; l++)
    //            {
    //                if (duration[l] == Convert.ToDateTime(model.championship.decrees[k]))
    //                {
    //                    dur.Remove(duration[l]);
    //                }
    //            }
    //        }

    //        for (int m = 0; m < model.championship.reserv.Length; m++)
    //        {
    //            dur.Add(model.championship.reserv[m]);
    //        }

    //        return dur;
    //    }

    //    public static List<int> GetDaysNumbers(List<DateTime> days)
    //    {
    //        List<int> daysNumbers = new List<int>();
    //        Days name;

    //        foreach (DateTime day in days)
    //        {
    //            for (name = Days.Monday; name <= Days.Sunday; name++)
    //            {
    //                if (day.DayOfWeek.ToString() == Enum.GetName(typeof(Days), name))
    //                {
    //                    daysNumbers.Add((int)name);
    //                }
    //            }
    //        }

    //        return daysNumbers;
    //    }
    //    public static int[][] GetTimes(Loader.sсhedule model)
    //    {
    //        int[][] time = new int[model.teams.Length][];
    //        for (int i = 0; i < model.teams.Length; i++)
    //        {
    //            if (model.teams[i].slots != null)
    //            {
    //                string[] timeSlots = new string[model.teams[i].slots.Length];
    //                for (int j = 0; j < model.teams[i].slots.Length; j++)
    //                    timeSlots[j] = model.teams[i].slots[j].time;

    //                string[] uniqueSlots = timeSlots.Distinct().ToArray();
    //                List<int> teamSlots = new List<int>();
    //                for (int k = 0; k < model.stadium.time.Length; k++)
    //                {
    //                    for (int l = 0; l < uniqueSlots.Length; l++)
    //                    {
    //                        if (uniqueSlots[l] == model.stadium.time[k])
    //                            teamSlots.Add(k);
    //                    }
    //                }
    //                time[i] = teamSlots.ToArray();
    //                timeSlots = null;
    //                uniqueSlots = null;
    //                teamSlots.Clear();
    //            }
    //            else
    //            {
    //                time[i] = new int[0];
    //            }

    //        }
    //        return time;
    //    }
    //}

    public class LoaderDummy : ILoader
    {
        public Loader.sсhedule mod;

        public ConfigParams Initialize(string configFile)
        {
            if (File.Exists(Directory.GetCurrentDirectory() + "\\" + configFile) == false)
            {
                LogGlobal.msg(1, "File : " + configFile + " not found");
                Console.WriteLine("Файл {0} не найден", configFile);
                Console.ReadLine();
            }
            try
            {
                INIManager manager = new INIManager(Directory.GetCurrentDirectory() + "\\" + configFile);
                ConfigParams parameters = new ConfigParams(true);
                parameters.logFile = manager.GetPrivateString("log", "logFile");
                parameters.algorythm = manager.GetPrivateString("algorythm", "alg");
                parameters.iterations = Convert.ToInt32(manager.GetPrivateString("other", "iterations"));
                return parameters;
            }
            catch (Exception e)
            {
                LogGlobal.msg(1, "Exception : " + e.Message);
                return new ConfigParams(false);
            }

        }

        public static List<DateTime> GetGameDays(Loader.sсhedule model)
        {
            List<DateTime> duration = new List<DateTime>();

            for (DateTime i = model.championship.start; i <= model.championship.end; i = i.AddDays(1))
            {
                for (int j = 0; j < model.championship.days.Length; j++)
                {
                    if (i.DayOfWeek.ToString() == model.championship.days[j])
                    {
                        duration.Add(i);
                        //Console.WriteLine(i);
                        break;
                    }
                }
            }

            List<DateTime> dur = duration;

            for (int k = 0; k < model.championship.decrees.Length; k++)
            {
                //Console.WriteLine("Decree " + k + " : " + model.championship.decrees[k]);
                for (int l = 0; l < duration.Count; l++)
                {
                    if (duration[l] == Convert.ToDateTime(model.championship.decrees[k]))
                    {
                        dur.Remove(duration[l]);
                    }
                }
            }

            for (int m = 0; m < model.championship.reserv.Length; m++)
            {
                dur.Add(model.championship.reserv[m]);
            }

            return dur;
        }

        public static int[][] GetTimes(Loader.sсhedule model)
        {
            int[][] time = new int[model.teams.Length][];
            for (int i = 0; i < model.teams.Length; i++)
            {
                if (model.teams[i].slots != null)
                {
                    string[] timeSlots = new string[model.teams[i].slots.Length];
                    for (int j = 0; j < model.teams[i].slots.Length; j++)
                        timeSlots[j] = model.teams[i].slots[j].time;

                    string[] uniqueSlots = timeSlots.Distinct().ToArray();
                    List<int> teamSlots = new List<int>();
                    for (int k = 0; k < model.stadium.time.Length; k++)
                    {
                        for (int l = 0; l < uniqueSlots.Length; l++)
                        {
                            if (uniqueSlots[l] == model.stadium.time[k])
                                teamSlots.Add(k);
                        }
                    }
                    time[i] = teamSlots.ToArray();
                    timeSlots = null;
                    uniqueSlots = null;
                    teamSlots.Clear();
                }
                else
                {
                    time[i] = new int[0];
                }

            }
            return time;
        }

        public Model Parse(string inputFile, string logName)
        {
            LogGlobal.Join(Directory.GetCurrentDirectory() + "\\" + logName);
            LogGlobal.msg("");
            LogGlobal.msg("------------------------------------");

            string xmlFile = Directory.GetCurrentDirectory() + "\\" + inputFile;
            XmlSerializer serializer = new XmlSerializer(typeof(Loader.sсhedule));
            if (File.Exists(xmlFile) == false)
            {
                LogGlobal.msg(1, "File : " + xmlFile + " not found");
                Console.WriteLine("Файл {0} не найден", xmlFile);
                Console.ReadLine();
                return null;
            }

            FileStream input = File.OpenRead(xmlFile);
            sсhedule model;

            LogGlobal.msg(0, DateTime.Now.TimeOfDay + " Loader start to parse " + xmlFile);

            try
            {
                model = (sсhedule)serializer.Deserialize(input);
                input.Close();
            }
            catch (Exception e)
            {
                input.Close();
                LogGlobal.msg(1, "Serializer exception" + e.Message);
                //Console.WriteLine("serializer: {0}", e.Message);
                //Console.WriteLine(e.GetBaseException());
                //Console.ReadLine();
                return null;
            }

            mod = model;
            LogGlobal.msg(0, DateTime.Now.TimeOfDay + " Done with XML, creating model");

            Model trueModel = new Model(null, 2, GetGameDays(model),null);

            LogGlobal.msg(0, "Teams : " + trueModel.n);
            LogGlobal.msg(0, "Days : " + trueModel.d);

            LogGlobal.msg(0, DateTime.Now.TimeOfDay + " Model is done");

            return trueModel;
        }
    }

    //Класс для чтения/записи INI-файлов
    public class INIManager
    {
        //Конструктор, принимающий путь к INI-файлу
        public INIManager(string aPath)
        {
            path = aPath;
        }

        //Конструктор без аргументов (путь к INI-файлу нужно будет задать отдельно)
        public INIManager() : this("") { }

        //Возвращает значение из INI-файла (по указанным секции и ключу) 
        public string GetPrivateString(string aSection, string aKey)
        {
            //Для получения значения
            StringBuilder buffer = new StringBuilder(SIZE);

            //Получить значение в buffer
            GetPrivateString(aSection, aKey, null, buffer, SIZE, path);

            //Вернуть полученное значение
            return buffer.ToString();
        }

        //Пишет значение в INI-файл (по указанным секции и ключу) 
        public void WritePrivateString(string aSection, string aKey, string aValue)
        {
            //Записать значение в INI-файл
            WritePrivateString(aSection, aKey, aValue, path);
        }

        //Возвращает или устанавливает путь к INI файлу
        public string Path { get { return path; } set { path = value; } }

        //Поля класса
        private const int SIZE = 1024; //Максимальный размер (для чтения значения из файла)
        private string path = null; //Для хранения пути к INI-файлу

        //Импорт функции GetPrivateProfileString (для чтения значений) из библиотеки kernel32.dll
        [DllImport("kernel32.dll", EntryPoint = "GetPrivateProfileString")]
        private static extern int GetPrivateString(string section, string key, string def, StringBuilder buffer, int size, string path);

        //Импорт функции WritePrivateProfileString (для записи значений) из библиотеки kernel32.dll
        [DllImport("kernel32.dll", EntryPoint = "WritePrivateProfileString")]
        private static extern int WritePrivateString(string section, string key, string str, string path);
    }
}
