using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core
{
    public interface IModel
    {
        int[] Criterion(Answer answer);
    }

    public class Model : IModel
    {
        public bool invalid; // Флаг корректности модели
        public List<DateTime> gameDates; // Список дат игровых дней

        public int n; // Число команд
        public int nl; // Число команд лидеров
        public int[] s; // Номера зрелищных туров 
        public int d; // Число игровых дней
        public int r; // Число дней в директивном сроке (за вычетом резервных дней)
        public int f; // Минимальное число игр за один игровой день
        public int g; // Максимальное число игр за один игровой день
        public int[] q; // Номера дней недели игровых дней
        public int[] w; // Номера недель по дням
        public V[][] v; // Номера приоритетных дней недели с числом в 4 тура для команд  
        public int[][] t; // Номера приоритетных часов

        public Model InvalidModel()
        {
            Model model = new Model();
            model.invalid = true;
            return model;
        }

        public int[] Criterion(Answer answer)
        {
            int[,,] x = answer.Tours;
            int[,] y = answer.Days;
            int[,] z = answer.Hours;

            int[] cr = new int[6];

            // Расписание укладывается в директивный срок
            {
                int maxDay = 0;
                int lastTour = 2 * (n - 1) - 1;
                for (int i = 0; i < n / 2; i++)
                {
                    if (y[lastTour, i] > maxDay)
                        maxDay = y[lastTour, i];
                }

                if (maxDay >= r)
                    cr[0] = maxDay - (r - 1);
            }

            // Каждая команда играет один матч в неделю
            {
                int[,] a = new int[d, n];
                for (int i = 0; i < 2 * (n - 1); i++)
                    for (int j = 0; j < n / 2; j++)
                    {
                        a[y[i, j], x[i, j, 0]]++;
                        a[y[i, j], x[i, j, 1]]++;
                    }
                int week = w[0];
                int[] b = new int[n];
                for (int i = 0; i < d; i++)
                {
                    for (int j = 0; j < n; j++)
                        b[j] += a[i, j];
                    if ((i == d - 1) || (w[i + 1] > week))
                    {
                        for (int j = 0; j < n; j++)
                        {
                            if (b[j] > 1)
                                cr[1] += b[j] - 1;
                            b[j] = 0;
                        }
                        week = w[i];
                    }
                }
            }

            // Матчи между лидерами проходят во время зрелищных туров
            {
                for (int i = 0; i < s.Length; i++)
                    for (int j = 0; j < n / 2; j++)
                        if ((x[s[i], j, 0] < nl) && (x[s[i], j, 1] < nl))
                            cr[2]--;
                cr[2] += nl * 2 * (n - 1);
            }

            // За один день не более 2 матчей между лидерами
            {
                int[] a = new int[d];
                for (int i = 0; i < 2 * (n - 1); i++)
                    for (int j = 0; j < n / 2; j++)
                        if ((x[i, j, 0] < nl) && (x[i, j, 1] < nl))
                            a[y[i, j]]++;
                for (int i = 0; i < d; i++)
                    if (a[i] > 2)
                        cr[3] += a[i] - 2;
            }

            // Пожелания команд по времени
            {
                for (int i = 0; i < 2 * (n - 1); i++)
                    for (int j = 0; j < n / 2; j++)
                        for (int k = 0; k < 2; k++)
                            if (t[x[i, j, k]].Length > 0)
                            {
                                cr[4]++;
                                for (int l = 0; l < t[x[i, j, k]].Length; l++)
                                    if (t[x[i, j, k]][l] == z[i, j])
                                    {
                                        cr[4]--;
                                        break;
                                    }
                            }
            }

            // Пожелания команд по дням недели для одного матча раз в 4 тура
            {
                int[][] a = new int[n][];
                for (int i = 0; i < n; i++)
                    a[i] = new int[v[i].Length];
                for (int i = 0; i < 2 * (n - 1); i++)
                {
                    if (i % 4 == 0)
                    {
                        for (int j = 0; j < n; j++)
                            for (int k = 0; k < v[j].Length; k++)
                                a[j][k] = v[j][k].n;
                    }
                    for (int j = 0; j < n / 2; j++)
                    {
                        for (int k = 0; k < 2; k++)
                        {
                            if (v[x[i, j, k]].Length > 0)
                            {
                                for (int l = 0; l < v[x[i, j, k]].Length; l++)
                                {
                                    if (q[y[i, j]] == v[x[i, j, k]][l].d)
                                    {
                                        if (a[x[i, j, k]][l] > 0)
                                        {
                                            cr[5]--;
                                            a[x[i, j, k]][l]--;
                                        }
                                        break;
                                    }
                                }

                            }
                        }
                    }
                }
                int sum = 0;
                for (int i = 0; i < n; i++)
                {
                    for (int j = 0; j < v[i].Length; j++)
                        sum += v[i][j].n;
                }
                cr[5] += sum * 2 * (n - 1) / 4;
                if (2 * (n - 1) % 4 != 0)
                {
                    for (int i = 0; i < n; i++)
                    {
                        sum = 0;
                        for (int j = 0; j < v[i].Length; j++)
                            sum += v[i][j].n;
                        if (sum == 4)
                            cr[5] += 2;
                        else if (sum == 3)
                            cr[5] += 1;
                    }
                }
            }

            return cr;
        }
    }

    // Номера приоритетных дней недели с числом повторений в четыре тура
    public struct V 
    {
        public int d; // Приоритеный день недели
        public int n; // Число повторений в четыре тура

        public V(int d, int n) 
        { 
            this.d = d; this.n = n; 
        } 
    }

}
