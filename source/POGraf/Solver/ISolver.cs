using Core;
using Log;
using System;
using System.IO;

namespace Solver
{
    public interface ISolver
    {
        Answer Solve(Model model, ConfigParams parameters, string logName);
        void SetModel(Model model);
        void SetAlgo(Algo algo);
    }

    public class SolverDummy : ISolver
    {
        Model mod;
        Algo alg;
        Answer an;

        public void SetAlgo(Algo algo)
        {
            alg = algo;
        }

        public void SetModel(Model model)
        {
            mod = model;
        }

        public Answer Solve(Model model, ConfigParams parameters, string logName)
        {
            LogGlobal.Join(Directory.GetCurrentDirectory() + "\\" + logName);
            LogGlobal.msg("");
            LogGlobal.msg("------------------------------------");
            LogGlobal.msg(0, DateTime.Now.TimeOfDay + " Loader finished. Solver start");

            Answer ans;
            switch (parameters.algorythm)
            {
                case "greedy":
                    {
                        Algo algo = new IterAlgo(new GreedyAlgo(model, 2), 2);

                        LogGlobal.Join(Directory.GetCurrentDirectory() + "\\" + logName);
                        LogGlobal.msg(0, DateTime.Now.TimeOfDay + " Algorythm start");

                        ans = new Answer(algo.Solve(), model);

                        LogGlobal.msg(0, DateTime.Now.TimeOfDay + " The answer is ready");

                        break;
                    }
                case "local":
                    {
                        Algo algo = new LocalAlgo(new IterAlgo(new GreedyAlgo(model), 2), 2);

                        LogGlobal.Join(Directory.GetCurrentDirectory() + "\\" + logName);
                        LogGlobal.msg(0, DateTime.Now.TimeOfDay + " Algorythm start");

                        ans = new Answer(algo.Solve(), model);

                        LogGlobal.msg(0, DateTime.Now.TimeOfDay + " The answer is ready");

                        break;
                    }
                default:
                    {
                        ans = null;
                        break;
                    }

            }
            return ans;
        }


    }
}
