using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

using Log;

namespace Core
{
    public interface IAlgo
    {
        Answer Solve();
    }

    public abstract class Algo : IAlgo
    {
        protected Model model;
        public List<int[]> criterion;

        #region Исходные параметры
        public int n; // Число команд

        public int nl; // Число команд лидеров
        public int[] s; // Номера зрелищных туров 

        public int d; // Число дней
        public int r; // Число дней в директивном сроке
        public int f; // Минимальное число игр за один игровой день
        public int g; // Максимальное число игр за один игровой день

        public int[] q; // Номера дней недели по дням
        public int[] w; // Номера недель по дням

        public V[][] v; // Номера приоритетных дней недели с числом в 4 тура для команд
        public int[][] t; // Номера прилоритетных часов
        #endregion  

        #region Варьируемые параметры
        public int[,,] x; // Номера команд в матчах [2 * (n - 1), n / 2, 2]
        public int[,] y; // Номера дней проведения матчей [2 * (n - 1), n / 2]
        public int[,] z; // Номера часов проведения матчей [2 * (n - 1), n / 2]

        protected Algo(int n, int nl, int[] s, int d, int r, int f, int g, int[] q, int[] w, V[][] v, int[][] t, int[,,] x, int[,] y, int[,] z)
        {
            this.n = n;
            this.nl = nl;
            this.s = s;
            this.d = d;
            this.r = r;
            this.f = f;
            this.g = g;
            this.q = q;
            this.w = w;
            this.v = v;
            this.t = t;
            this.x = x;
            this.y = y;
            this.z = z;
        }
        #endregion

        public Algo(Model model)
        {
            this.model = model;
            n = model.n;
            nl = model.nl;
            s = model.s;
            d = model.d;
            r = model.r;
            f = model.f;
            g = model.g;
            q = new int[d];
            w = new int[d];
            for (int i = 0; i < d; i++)
            {
                q[i] = model.q[i];
                w[i] = model.w[i];
            }
            v = model.v;
            t = model.t;

            criterion = new List<int[]>();
        }

        public abstract Answer Solve();
    }

    // Жадный алгоритм
    public class GreedyAlgo : Algo
    {
        public GreedyAlgo(Model model) : base(model) { }

        // Составление расписания матчей по турам
        public int[,,] GetTours()
        {
            int[,,] x = new int[2 * (n - 1), n / 2, 2];

            // Туры
            int[][,] a = new int[n - 1][,];
            for (int i = 0; i < n - 1; i++)
                a[i] = new int[n / 2, 2];

            {
                // Лента команд
                int[] b = new int[n];
                for (int i = 0; i < n; i++)
                    b[i] = i;

                // Заполнение туров в неустановленном порядке
                for (int i = 0; i < n - 1; i++)
                {
                    for (int j = 0; j < n / 2; j++)
                    {
                        a[i][j, 0] = b[j];
                        a[i][j, 1] = b[n - 1 - j];
                    }
                    if (i < n - 2)
                    {
                        int temp = b[n - 1];
                        for (int k = n - 1; k > 1; k--)
                            b[k] = b[k - 1];
                        b[1] = temp;
                    }
                }
            }

            {
                // Количества игр между командами лидерами в турах
                int[] b = new int[n - 1];
                for (int i = 0; i < n - 1; i++)
                    for (int j = 0; j < n / 2; j++)
                        if ((a[i][j, 0] < nl) && (a[i][j, 1] < nl))
                            b[i]++;

                // Сортировка туров по количеству игр между командами лидерами
                Array.Sort(b, a);
                Array.Reverse(a);
            }

            {
                // Распределение зрелищных туров по кругам
                List<int>[] b = { new List<int>(), new List<int>() };
                for (int i = 0; i < s.Length; i++)
                {
                    if (s[i] < n - 1)
                        b[0].Add(s[i]);
                    else
                        b[1].Add(s[i]);
                }

                // Заполнение расписания
                for (int i = 0; i < 2; i++)
                {
                    // Заполнение зрелищных туров в порядке сортировки
                    for (int j = 0; j < b[i].Count; j++)
                    {
                        for (int k = 0; k < n / 2; k++)
                            for (int l = 0; l < 2; l++)
                                x[b[i][j], k, l] = a[j][k, l];
                    }

                    // Заполнение оставшихся туров
                    int m = 0; // Номер свободного тура
                    for (int j = b[i].Count; j < n - 1; j++)
                    {
                        // Поиск свободного тура
                        for (; x[i * (n - 1) + m, 0, 0] != x[i * (n - 1) + m, 0, 1]; m++) ;

                        // Заполнение свободного тура
                        for (int k = 0; k < n / 2; k++)
                            for (int l = 0; l < 2; l++)
                                x[i * (n - 1) + m, k, l] = a[j][k, l];
                    }
                }
            }

            return x;
        }

        // Составление расписания матчей по дням
        public int[,] GetDays()
        {
            int[,] y = new int[2 * (n - 1), n / 2];

            // Присвоение расписанию неустановленных значений
            for (int i = 0; i < 2 * (n - 1); i++)
                for (int j = 0; j < n / 2; j++)
                    y[i, j] = -1;

            // Счетчик выполнения предпочтений
            int[][][] c = new int[n][][];
            for (int i = 0; i < n; i++)
            {
                c[i] = new int[v[i].Length][];
                for (int k = 0; k < v[i].Length; k++)
                {
                    c[i][k] = new int[2];
                    c[i][k][0] = v[i][k].d;
                }
            }

            int[] o = new int[d]; // Счетчик матчей в день
            int lastd = 0; // День последнего размещенного матча

            for (int i = 0; i < 2 * (n - 1); i++)
            {
                int a = lastd * g + o[lastd]; // Первый доступный слот для текущего тура
                int b = (d * g - a) / (2 * (n - 1) - i); // Число выделенных слотов для текущего тура

                // Сброс счетчика выполнения предпочтений каждые 4 тура
                if (i % 4 == 0)
                {
                    for (int j = 0; j < n; j++)
                        for (int k = 0; k < v[j].Length; k++)
                            c[j][k][1] = v[j][k].n;
                }

                // Размещение по предпочтениям
                for (int j = 0; j < n / 2; j++)
                {
                    for (int k = 0; k < 2; k++)
                    {
                        for (int l = 0; l < c[x[i, j, k]].Length; l++)
                        {
                            if (c[x[i, j, k]][l][1] > 0)
                            {
                                for (int e = a / g; e <= (a + b - 1) / g; e++)
                                {
                                    if ((q[e] == c[x[i, j, k]][l][0]) && (o[e] < g))
                                    {
                                        if (e == (a + b - 1) / g)
                                        {
                                            int ol = b % g;
                                            if (ol == 0)
                                                ol = g;
                                            if (o[e] >= ol)
                                                break;
                                        }
                                        y[i, j] = e;
                                        if (e > lastd)
                                            lastd = e;
                                        o[e]++;
                                        c[x[i, j, k]][l][1]--;
                                        if (k == 0)
                                            for (l = 0; l < c[x[i, j, 1]].Length; l++)
                                                if ((q[e] == c[x[i, j, 1]][l][0]) && (c[x[i, j, 1]][l][1] > 0))
                                                {
                                                    c[x[i, j, 1]][l][1]--;
                                                    break;
                                                }
                                        break;
                                    }
                                }
                                if (y[i, j] >= 0)
                                    break;
                            }
                        }
                        if (y[i, j] >= 0)
                            break;
                    }
                }

                // Размещение без предпочтений
                {
                    int j;
                    int e = a / g;

                    // Размещение в игровые дни с недостающим числом игр
                    for (j = 0; j < n / 2; j++)
                    {
                        if (y[i, j] < 0)
                        {
                            for (; e < lastd; e++)
                                if ((o[e] > 0) && (o[e] < f))
                                {
                                    y[i, j] = e;
                                    o[e]++;
                                    break;
                                }
                            if (e >= lastd)
                                break;
                        }
                    }

                    // Размещение в остальные дни
                    for (; j < n / 2; j++)
                    {
                        if (y[i, j] < 0)
                            for (e = a / g; e <= (a + b - 1) / g; e++)
                                if (o[e] < g)
                                {
                                    y[i, j] = e;
                                    if (e > lastd)
                                        lastd = e;
                                    o[e]++;
                                    break;
                                }
                    }
                }

                // Выполнение ограничения на минимальное число игр в игровой день
                for (int j = a / g; j < lastd; j++)
                {
                    while ((o[j] > 0) && (o[j] < f))
                        for (int k = 0; k < n / 2; k++)
                            if (y[i, k] == lastd)
                            {
                                y[i, k] = j;
                                o[lastd]--;
                                o[j]++;
                                if (o[lastd] == 0)
                                    for (int l = lastd - 1; ; l--)
                                        if (o[l] > 0)
                                        {
                                            lastd = l;
                                            break;
                                        }
                                break;
                            }
                }
            }

            return y;
        }

        // Составление расписания матчей по часам
        public int[,] GetHours()
        {
            int[,] z = new int[2 * (n - 1), n / 2];

            // Присвоение расписанию неустановленных значений
            for (int i = 0; i < 2 * (n - 1); i++)
                for (int j = 0; j < n / 2; j++)
                    z[i, j] = -1;

            bool[,] a = new bool[d, g]; // Метки занятых часов

            for (int i = 0; i < 2 * (n - 1); i++)
            {
                // Первый день следующего тура
                int b = d;
                if (i < 2 * (n - 1) - 1)
                    for (int j = 0; j < n / 2; j++)
                        if (y[i + 1, j] < b)
                            b = y[i + 1, j];

                // Число игр следующего тура в первый день
                int c = 0;
                if (i < 2 * (n - 1) - 1)
                    for (int j = 0; j < n / 2; j++)
                        if (y[i + 1, j] == b)
                            c++;

                // Размещение по предпочтениям
                for (int j = 0; j < n / 2; j++)
                {
                    for (int k = 0; k < 2; k++)
                    {
                        for (int l = 0; l < t[x[i, j, k]].Length; l++)
                        {
                            if ((a[y[i, j], t[x[i, j, k]][l]] == false) && ((y[i, j] < b) || (t[x[i, j, k]][l] < g - c)))
                            {
                                z[i, j] = t[x[i, j, k]][l];
                                a[y[i, j], t[x[i, j, k]][l]] = true;
                                break;
                            }
                        }
                        if (z[i, j] >= 0)
                            break;
                    }
                }

                // Размещение без предпочтений
                for (int j = 0; j < n / 2; j++)
                {
                    if (z[i, j] == -1)
                        for (int k = 0; ; k++)
                        {
                            if (a[y[i, j], k] == false)
                            {
                                z[i, j] = k;
                                a[y[i, j], k] = true;
                                break;
                            }
                        }
                }
            }

            return z;
        }

        public override Answer Solve()
        {
            x = GetTours();
            y = GetDays();
            z = GetHours();
            Answer answer = new Answer(n, x, y, z);
            criterion.Add(model.Criterion(answer));
            return answer;
        }
    }

    // Алгоритм локального поиска
    public class LocalAlgo : GreedyAlgo
    {
        int numIter;

        public LocalAlgo(Model model, int numIter) : base(model)
        {
            this.numIter = numIter;
        }

        public override Answer Solve()
        {
            x = GetTours();
            y = GetDays();
            z = GetHours();

            {
                int[] cr = model.Criterion(new Answer(n, x, y, z));
                int crSave = 0;
                for (int i = 0; i < cr.Length; i++)
                    crSave += cr[i];
                crSave += cr[5];
                Console.WriteLine(crSave);
                criterion.Add(cr);
                for (int j = 0; j < cr.Length; j++)
                    Console.Write(cr[j] + " ");
                Console.WriteLine();

                int crNew;
                int[,,] xSave;
                int[,] ySave;
                int[,] zSave;

                for (int i = 0; i < numIter; i++)
                {
                    {
                        xSave = new int[2 * (n - 1), n / 2, 2];
                        ySave = new int[2 * (n - 1), n / 2];
                        zSave = new int[2 * (n - 1), n / 2];
                        for (int j = 0; j < 2 * (n - 1); j++)
                            for (int k = 0; k < n / 2; k++)
                            {
                                ySave[j, k] = y[j, k];
                                zSave[j, k] = z[j, k];
                                for (int l = 0; l < 2; l++)
                                    xSave[j, k, l] = x[j, k, l];
                            }
                    }
                    if (d * g - 2 * (n - 1) * (n / 2) >= g)
                    {
                        int[] a = new int[d];
                        int[] b = new int[d];
                        for (int j = 0; j < d; j++)
                        {
                            a[j] = СonflictsDay(j);
                            b[j] = j;
                        }
                        Array.Sort(a, b);
                        Random random = new Random();
                        int num = random.Next(1, (d * g - 2 * (n - 1) * (n / 2)) / g + 1);
                        int[] c = new int[num];
                        for (int j = 0; j < num; j++)
                        {
                            bool or;
                            do
                            {
                                or = true;
                                c[j] = random.Next(j, d);
                                for (int k = 0; k < j; k++)
                                    if (c[k] == c[j])
                                    {
                                        or = false;
                                        break;
                                    }
                            }
                            while (or == false);
                        }
                        for (int j = d - 1; j >= 0; j--)
                        {
                            for (int k = 0; k < num; k++)
                                if (j == b[c[k]])
                                {
                                    for (int l = j; l < d - 1; l++)
                                    {
                                        q[l] = q[l + 1];
                                        w[l] = w[l + 1];
                                    }
                                    d--;
                                    break;
                                }
                        }
                    }
                    cr = model.Criterion(new Answer(n, x, y, z));
                    crNew = 0;
                    for (int j = 0; j < cr.Length; j++)
                        crNew += cr[j];
                    crNew += cr[5];
                    if (crNew >= crSave)
                    {
                        x = xSave;
                        y = ySave;
                        z = zSave;
                        d = model.d;
                        for (int j = 0; j < d; j++)
                        {
                            q[j] = model.q[j];
                            w[j] = model.w[j];
                        }
                    }
                    else
                    {
                        crSave = crNew;
                        Console.WriteLine(crSave);
                        criterion.Add(cr);
                        for (int j = 0; j < 6; j++)
                            Console.Write(cr[j] + " ");
                        Console.WriteLine();
                    }

                    {
                        xSave = new int[2 * (n - 1), n / 2, 2];
                        ySave = new int[2 * (n - 1), n / 2];
                        zSave = new int[2 * (n - 1), n / 2];
                        for (int j = 0; j < 2 * (n - 1); j++)
                            for (int k = 0; k < n / 2; k++)
                            {
                                ySave[j, k] = y[j, k];
                                zSave[j, k] = z[j, k];
                                for (int l = 0; l < 2; l++)
                                    xSave[j, k, l] = x[j, k, l];
                            }
                    }
                    {
                        int[] a = new int[n];
                        for (int j = 0; j < n; j++)
                            a[j] = ConflictTeam(j);
                        int[] max = new int[2] { 0, 1 };
                        if (a[0] < a[1])
                        {
                            max[0] = 1;
                            max[1] = 0;
                        }
                        for (int j = 2; j < n; j++)
                        {
                            if (a[j] > max[0])
                            {
                                max[1] = max[0];
                                max[0] = j;
                            }
                            else if (a[j] > max[1])
                                max[1] = j;
                        }

                        Random random = new Random();
                        int maxR = random.Next(0, 2);
                        do { max[maxR] = random.Next(0, n); }
                        while (max[0] == max[1]);

                        for (int j = 0; j < 2 * (n - 1); j++)
                        {
                            for (int k = 0; k < n / 2; k++)
                            {
                                for (int l = 0; l < 2; l++)
                                {
                                    if (x[j, k, l] == max[0])
                                        x[j, k, l] = max[1];
                                    else if (x[j, k, l] == max[1])
                                        x[j, k, l] = max[0];
                                }
                            }
                        }

                        y = GetDays();
                        z = GetHours();
                    }
                    cr = model.Criterion(new Answer(n, x, y, z));
                    crNew = 0;
                    for (int j = 0; j < cr.Length; j++)
                        crNew += cr[j];
                    crNew += cr[5];
                    if (crNew >= crSave)
                    {
                        x = xSave;
                        y = ySave;
                        z = zSave;
                    }
                    else
                    {
                        crSave = crNew;
                        Console.WriteLine(crSave);
                        criterion.Add(cr);
                        for (int j = 0; j < 6; j++)
                            Console.Write(cr[j] + " ");
                        Console.WriteLine();
                    }

                    {
                        xSave = new int[2 * (n - 1), n / 2, 2];
                        ySave = new int[2 * (n - 1), n / 2];
                        zSave = new int[2 * (n - 1), n / 2];
                        for (int j = 0; j < 2 * (n - 1); j++)
                            for (int k = 0; k < n / 2; k++)
                            {
                                ySave[j, k] = y[j, k];
                                zSave[j, k] = z[j, k];
                                for (int l = 0; l < 2; l++)
                                    xSave[j, k, l] = x[j, k, l];
                            }
                    }
                    {
                        int round = i % 2;
                        int[] a = new int[n - 1];
                        for (int j = 0; j < n - 1; j++)
                            a[j] = ConflictTour(j + round * (n - 1));

                        int[] max = new int[2] { 0, 1 };
                        if (a[0] < a[1])
                        {
                            max[0] = 1;
                            max[1] = 0;
                        }
                        for (int j = 2; j < n - 1; j++)
                        {
                            if (a[j] > max[0])
                            {
                                max[1] = max[0];
                                max[0] = j;
                            }
                            else if (a[j] > max[1])
                                max[1] = j;
                        }

                        Random random = new Random();
                        int maxR = random.Next(0, 2);
                        do { max[maxR] = random.Next(0, n - 1); }
                        while (max[0] == max[1]);

                        max[0] += round * (n - 1);
                        max[1] += round * (n - 1);

                        for (int j = 0; j < n / 2; j++)
                            for (int k = 0; k < 2; k++)
                            {
                                int temp = x[max[0], j, k];
                                x[max[0], j, k] = x[max[1], j, k];
                                x[max[1], j, k] = temp;
                            }

                        x = GetTours();
                        y = GetDays();
                        z = GetHours();
                    }
                    cr = model.Criterion(new Answer(n, x, y, z));
                    crNew = 0;
                    for (int j = 0; j < cr.Length; j++)
                        crNew += cr[j];
                    crNew += cr[5];
                    if (crNew >= crSave)
                    {
                        x = xSave;
                        y = ySave;
                        z = zSave;
                    }
                    else
                    {
                        crSave = crNew;
                        Console.WriteLine(crSave);
                        criterion.Add(cr);
                        for (int j = 0; j < 6; j++)
                            Console.Write(cr[j] + " ");
                        Console.WriteLine();
                    }
                }
            }

            return new Answer(n, x, y, z);
        }

        int СonflictsDay(int a)
        {
            int sum = 0;

            int sum0 = 0;
            int sum1 = 0;
            int sum2 = 0;
            for (int i = 0; i < 2 * (n - 1); i++)
            {
                for (int j = 0; j < n / 2; j++)
                    if (y[i, j] == a)
                    {
                        if ((x[i, j, 0] < nl) && (x[i, j, 1] < nl))
                            sum0++;
                        for (int k = 0; k < 2; k++)
                        {
                            if (t[x[i, j, k]].Length > 0)
                            {
                                sum1++;
                                for (int l = 0; l < t[x[i, j, k]].Length; l++)
                                    if (z[i, j] == t[x[i, j, k]][l])
                                    {
                                        sum1--;
                                        break;
                                    }
                            }
                            if (v[x[i, j, k]].Length > 0)
                            {
                                V[] b = new V[v[x[i, j, k]].Length];
                                for (int l = 0; l < b.Length; l++)
                                {
                                    b[l].d = v[x[i, j, k]][l].d;
                                    b[l].n = v[x[i, j, k]][l].n;
                                }

                                for (int l = i - i % 4; (l < i - i % 4 + 4) && (l < 2 * (n - 1)); l++)
                                {
                                    if (l != i)
                                        for (int o = 0; o < n / 2; o++)
                                        {
                                            if ((x[l, o, 0] == x[i, j, k]) || (x[l, o, 1] == x[i, j, k]))
                                                for (int c = 0; c < b.Length; c++)
                                                    if (b[c].d == q[y[l, o]])
                                                    {
                                                        if (b[c].n > 0)
                                                            b[c].n--;
                                                        break;
                                                    }
                                        }
                                }

                                int temp = 0;
                                for (int l = 0; l < b.Length; l++)
                                    temp += b[l].n;
                                if (i < (2 * (n - 1) / 4) * 4)
                                {
                                    if (temp > 0)
                                    {
                                        sum2++;
                                        for (int l = 0; l < b.Length; l++)
                                            if (q[a] == b[l].d)
                                            {
                                                if (b[l].n > 0)
                                                    sum2--;
                                                break;
                                            }
                                    }
                                }
                                else if (temp > 2)
                                {
                                    sum2++;
                                    for (int l = 0; l < b.Length; l++)
                                        if (q[a] == b[l].d)
                                        {
                                            if (b[l].n > 0)
                                                sum2--;
                                            break;
                                        }
                                }
                            }
                        }
                    }
            }
            if (sum0 > 2)
                sum += (sum0 - 2);
            sum += sum1;
            sum += sum2;

            return sum;
        }

        int ConflictTeam(int a)
        {
            int sum = 0;

            if (t[a].Length > 0)
            {
                for (int i = 0; i < 2 * (n - 1); i++)
                {
                    for (int j = 0; j < n / 2; j++)
                    {
                        if ((x[i, j, 0] == a) || (x[i, j, 1] == a))
                        {
                            sum++;
                            for (int k = 0; k < t[a].Length; k++)
                            {
                                if (z[i, j] == t[a][k])
                                {
                                    sum--;
                                    break;
                                }
                            }
                            break;
                        }
                    }
                }
            }

            if (v[a].Length > 0)
            {
                V[] b = new V[v[a].Length];

                for (int i = 0; i < 2 * (n - 1); i++)
                {
                    if (i % 4 == 0)
                    {
                        for (int j = 0; j < v[a].Length; j++)
                        {
                            b[j].d = v[a][j].d;
                            b[j].n = v[a][j].n;
                        }
                    }
                    for (int j = 0; j < n / 2; j++)
                    {
                        if ((x[i, j, 0] == a) || (x[i, j, 1] == a))
                        {
                            for (int k = 0; k < b.Length; k++)
                            {
                                if (q[b[k].d] == y[i, j])
                                {
                                    if (b[k].n > 0)
                                        b[k].n--;
                                    break;
                                }
                            }
                            break;
                        }
                    }
                    if (i % 4 == 3)
                    {
                        for (int j = 0; j < b.Length; j++)
                        {
                            sum += b[j].n;
                        }
                    }
                    else if (i == 2 * (n - 1) - 1)
                    {
                        int temp = 0;
                        for (int j = 0; j < b.Length; j++)
                        {
                            temp += b[j].n;
                        }
                        temp -= 2;
                        if (temp > 0)
                            sum += temp;
                    }
                }
            }

            return sum;
        }

        int ConflictTour(int a)
        {
            int sum0 = 0;
            int sum1 = 0;
            int sum2 = 0;

            {
                bool spect = false;
                for (int i = 0; i < s.Length; i++)
                {
                    if (s[i] == a)
                    {
                        spect = true;
                        break;
                    }
                }
                if (spect == false)
                {
                    for (int j = 0; j < n / 2; j++)
                        if ((x[a, j, 0] < nl) && (x[a, j, 1] < nl))
                            sum0++;
                }
            }

            {
                for (int i = 0; i < n / 2; i++)
                    for (int j = 0; j < 2; j++)
                        if (t[x[a, i, j]].Length > 0)
                        {
                            sum1++;
                            for (int k = 0; k < t[x[a, i, j]].Length; k++)
                                if (t[x[a, i, j]][k] == z[a, i])
                                {
                                    sum1--;
                                    break;
                                }
                        }
            }

            {
                for (int i = 0; i < n / 2; i++)
                {
                    for (int j = 0; j < 2; j++)
                    {
                        if (v[x[a, i, j]].Length > 0)
                        {
                            V[] b = new V[v[x[a, i, j]].Length];
                            for (int k = 0; k < b.Length; k++)
                            {
                                b[k].d = v[x[a, i, j]][k].d;
                                b[k].n = v[x[a, i, j]][k].n;
                            }

                            for (int k = a - a % 4; (k < a - a % 4 + 4) && (k < 2 * (n - 1)); k++)
                            {
                                if (k != a)
                                    for (int l = 0; l < n / 2; l++)
                                    {
                                        if ((x[k, l, 0] == x[a, i, j]) || (x[k, l, 1] == x[a, i, j]))
                                            for (int c = 0; c < b.Length; c++)
                                                if (b[c].d == q[y[k, l]])
                                                {
                                                    if (b[c].n > 0)
                                                        b[c].n--;
                                                    break;
                                                }
                                    }
                            }

                            int temp = 0;
                            for (int k = 0; k < b.Length; k++)
                                temp += b[k].n;
                            if (a < (2 * (n - 1) / 4) * 4)
                            {
                                if (temp > 0)
                                {
                                    sum2++;
                                    for (int k = 0; k < b.Length; k++)
                                        if (q[y[a, i]] == b[k].d)
                                        {
                                            if (b[k].n > 0)
                                                sum2--;
                                            break;
                                        }
                                }
                            }
                            else if (temp > 2)
                            {
                                sum2++;
                                for (int k = 0; k < b.Length; k++)
                                    if (q[y[a, i]] == b[k].d)
                                    {
                                        if (b[k].n > 0)
                                            sum2--;
                                        break;
                                    }
                            }
                        }
                    }
                }
            }

            return (sum0 + sum1 + sum2);
        }
    }
}
