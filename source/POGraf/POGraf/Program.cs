using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Log;
using Loader;
using Core;
using Presenter;
using Solver;

namespace POGraf
{
    class Program
    {
        static void Main(string[] args)
        {   
            // Считать имя файла конфигурации
            Console.WriteLine("Config file: ");
            string configFile = Console.ReadLine();

            // Обработать файл конфигурации
            LoaderDummy loader = new LoaderDummy();
            ConfigParams parameters = loader.Initialize(configFile);
            string logName = parameters.logFile;

            // Подключить лог-файл
            LogGlobal.Join(Directory.GetCurrentDirectory()+"\\"+logName);
            LogGlobal.Start();
            LogGlobal.msg("");
            LogGlobal.msg("------------------------------------");
            LogGlobal.msg("Loader initialized " + configFile);
            //LogGlobal.msg(0, "Log file : " + parameters.logFile);
            LogGlobal.msg(0, "Choosen algorythm : " + parameters.algorythm);
            LogGlobal.msg(0, "Iterations num : " + parameters.iterations);

            // Считать имя файла с входными данными
            Console.WriteLine("Input file: ");
            string inputFile = Console.ReadLine();

            // Обработать xml-файл входных данных
            Model model = loader.Parse(inputFile, logName);

            // Произвести расчет
            ISolver solver = new SolverDummy();
            AnswerDummy ans = (AnswerDummy)solver.Solve(model, parameters, logName);
            LogGlobal.msg(0, DateTime.Now.TimeOfDay + " The answer is received");

            //var a = ans.GetInfo();
            //for (int i = 0; i < 2*(a.N-1); i++)
            //{
            //    for (int j = 0; j < a.N / 2; j++)
            //    {
            //        LogGlobal.msg(0, "Tour " + i + " Match " + j + " Teams: " + a.Tours[i,j,0] + " - " + a.Tours[i,j,1]);
            //    }
            //}                
            
            // Сформировать html-файл выходных данных
            Presenter.Presenter presenter = new Presenter.Presenter();
            presenter.ShowAnswer(model, ans, loader.mod, logName);

            LogGlobal.msg("");
            LogGlobal.msg("------------------------------------");
            LogGlobal.msg(0, DateTime.Now.TimeOfDay + " Work is over. Check answer.html");

            Console.WriteLine();
            Console.WriteLine("Work is over. Check the {0} and answer.html", logName);
            Console.ReadLine();
        }
    }
}
