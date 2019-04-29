using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core
{
    public class Answer
    {
        public int N { get; private set; }
        public int[, ,] Tours { get; private set; }
        public int[,] Days { get; private set; }
        public int[,] Hours { get; private set; }

        public Answer(int n, int[, ,] tours, int[,] days, int[,] hours)
        {
            N = n;
            Tours = tours;
            Days = days;
            Hours = hours;
        }      
        
        public void Sort()
        {
            for (int i = 0; i < 2 * (N - 1); i++)
            {
                for (int j = 0; j < N / 2; j++)
                {
                    for (int k = j + 1; k < N / 2; k++)
                    {
                        if ((Days[i, j] > Days[i, k]) || ((Days[i, j] == Days[i, k]) && (Hours[i, j] > Hours[i, k])))
                        {
                            int copy = Days[i, j];
                            Days[i, j] = Days[i, k];
                            Days[i, k] = copy;
                            copy = Hours[i, j];
                            Hours[i, j] = Hours[i, k];
                            Hours[i, k] = copy;
                            for (int l = 0; l < 2; l++)
                            {
                                copy = Tours[i, j, l];
                                Tours[i, j, l] = Tours[i, k, l];
                                Tours[i, k, l] = copy;
                            }
                        }
                    }
                }
            }
        }
    }

    public interface IAnswer
    {
        Answer GetInfo();
        void Sort();
        int[,] GetDays();
        int[,] GetHours();
    }

    public class AnswerDummy : IAnswer
    {
        Answer an;

        public AnswerDummy() { }

        public AnswerDummy(Answer answer)
        {
            an = answer;
        }
        public AnswerDummy(int n, int[, ,] tours, int[,] days, int[,] hours)
        {
            an = new Answer(n, tours, days, hours);
        }
        public Answer GetInfo()
        {
            return an;
        }
        public void Sort()
        {
            an.Sort();
        }
        public int[,] GetDays()
        {
            return this.an.Days;
        }

        public int[,] GetHours()
        {
            return this.an.Hours;
        }
    }
}
