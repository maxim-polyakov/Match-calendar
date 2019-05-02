using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

using Core;
using Log;

namespace Solver
{
    public interface ISolver
    {
        IAnswer Solve(Model model, ConfigParams parameters, string logName);
        void SetModel(IModel model);
        void SetAlgo(IAlgo algo);
    }

    public class SolverDummy : ISolver
    {
        IModel mod;
        IAlgo alg;
        IAnswer an;

        public void SetAlgo(IAlgo algo)
        {
            alg = algo;
        }

        public void SetModel(IModel model)
        {
            mod = model;
        }

        public IAnswer Solve(Model model, ConfigParams parameters, string logName)
        {
            LogGlobal.Join(Directory.GetCurrentDirectory() + "\\" + logName);
            LogGlobal.msg("");
            LogGlobal.msg("------------------------------------");
            LogGlobal.msg(0, DateTime.Now.TimeOfDay + " Loader finished. Solver start");

            IAnswer ans = new AnswerDummy();
            switch (parameters.algorythm)
            {
                case "greedy":
                    {
                        IAlgo greedy = new GreedyAlgo(model);

                        LogGlobal.Join(Directory.GetCurrentDirectory() + "\\" + logName);
                        LogGlobal.msg(0, DateTime.Now.TimeOfDay + " Algorythm start");

                        ans = new AnswerDummy(greedy.Solve());

                        LogGlobal.msg(0, DateTime.Now.TimeOfDay + " The answer is ready");

                        break;
                    }
                case "local":
                    {
                        IAlgo local = new LocalAlgo(model, parameters.iterations);

                        LogGlobal.Join(Directory.GetCurrentDirectory() + "\\" + logName);
                        LogGlobal.msg(0, DateTime.Now.TimeOfDay + " Algorythm start");

                        ans = new AnswerDummy(local.Solve());

                        LogGlobal.msg(0, DateTime.Now.TimeOfDay + " The answer is ready");

                        break;
                    }
                default:
                    {
                        break;
                    }

            }

            ans.Sort();
            return ans;
        }

      
        }
    }
