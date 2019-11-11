using System;
using System.Collections.Generic;

namespace Core
{
    public abstract class Algo
    {
        public Model model;
        public List<int[]> criterion;

        public abstract Schedule Solve(Schedule schedule = null);
    }

    public class GreedyAlgo : Algo
    {
        protected int n; // Число команд
        protected int r; // Число турнирных кругов
        protected int t; // Число туров
        protected int g; // Число игр в туре
        protected int s; // Число временных слотов в день
        protected int d; // Число дней
        protected Wish[] wishes; // Пожелания

        public int minGamesСonsid;
        public int coeffRandom;

        public GreedyAlgo(Model model, int coeffRandom = 3, int minGamesСonsid = 1)
        {
            this.model = model;
            n = model.n;
            r = model.r;
            s = model.s;
            d = model.d;
            t = r * (n - 1 + n % 2);
            g = n / 2 + n % 2;
            wishes = model.wishes.ToArray();
            this.coeffRandom = coeffRandom;
            this.minGamesСonsid = minGamesСonsid;
        }

        public override Schedule Solve(Schedule schedule = null)
        {
            Schedule sch;
            if (schedule == null)
                sch = new Schedule(n, r, t, g);
            else
                sch = new Schedule(schedule);

            int firstFreeTour = t;
            List<int[]> currGames = new List<int[]>(); // рассматриваемые неразмещенные матчи (тур, матч)
            for (int i = 0; i < t; i++)
            {
                if (sch.x[i, 0, 0] == null)
                {
                    firstFreeTour = i;
                    break;
                }
                else
                    for (int j = 0; j < g; j++)
                        if (sch.y[i, j] == null)
                            currGames.Add(new int[2] { i, j });
            }

            do
            {
                while ((currGames.Count < minGamesСonsid) && (firstFreeTour < t))
                {
                    SetNextTourRivals(sch, firstFreeTour);
                    for (int j = 0; j < g; j++)
                        currGames.Add(new int[2] { firstFreeTour, j });
                    firstFreeTour++;
                }

                bool[,,] demSuit = new bool[currGames.Count, d, s];
                int[] demSuitCoeff = new int[currGames.Count];
                int? minDemSuitSumGame = null;
                for (int game = 0; game < currGames.Count; game++)
                {
                    for (int day = 0; day < d; day++)
                        for (int slot = 0; slot < s; slot++)
                        {
                            demSuit[game, day, slot] = true;
                            demSuitCoeff[game] = 1;
                        }
                    for (int wish = 0; wish < wishes.Length; wish++)
                    {
                        if ((wishes[wish].importancePercent == 100) && (!(wishes[wish] is RivalsWish)))
                            for (int day = 0; day < d; day++)
                                for (int slot = 0; slot < s; slot++)
                                    if (demSuit[game, day, slot] == true)
                                        if (wishes[wish].IsSuitable(day, slot, currGames[game][0], currGames[game][1], sch, model) == false)
                                        {
                                            demSuit[game, day, slot] = false;
                                            demSuitCoeff[game]++;
                                        }
                    }
                    if ((minDemSuitSumGame == null) && (demSuitCoeff[game] <= d * s))
                        minDemSuitSumGame = -1;
                }

                if (minDemSuitSumGame.HasValue)
                {
                    {
                        Random random = new Random();
                        int sum = 0;
                        for (int i = 0; i < currGames.Count; i++)
                            sum += demSuitCoeff[i];
                        int a = random.Next(sum);
                        for (int i = 0; i < currGames.Count; i++)
                        {
                            a -= demSuitCoeff[i];
                            if (a < 0)
                            {
                                minDemSuitSumGame = i;
                                break;
                            }
                        }
                    }

                    {
                        int game = (int)minDemSuitSumGame;
                        ulong[,] wishRatio = new ulong[d, s];
                        for (int day = 0; day < d; day++)
                            for (int slot = 0; slot < s; slot++)
                                if (demSuit[game, day, slot] == true)
                                    wishRatio[day, slot] = 1;
                                else
                                    wishRatio[day, slot] = 0;
                        bool ulongExcess = false;
                        for (int wish = 0; wish < wishes.Length; wish++)
                            if ((wishes[wish].importancePercent < 100) && (!(wishes[wish] is RivalsWish)))
                            {
                                for (int day = 0; day < d; day++)
                                    for (int slot = 0; slot < s; slot++)
                                    {
                                        if ((wishRatio[day, slot] != 0) &&
                                            (wishes[wish].IsSuitable(day, slot, currGames[game][0], currGames[game][1], sch, model)))
                                        {
                                            wishRatio[day, slot] *= (ulong)wishes[wish].importancePercent * ((ulong)101 - (ulong)coeffRandom) / (ulong)10;
                                            if (wishRatio[day, slot] > ulong.MaxValue / 1000)
                                                ulongExcess = true;
                                        }
                                    }
                                if (ulongExcess)
                                {
                                    for (int day = 0; day < d; day++)
                                        for (int slot = 0; slot < s; slot++)
                                            wishRatio[day, slot] = (wishRatio[day, slot] + 1000 - 1) / 1000;
                                    ulongExcess = false;
                                }
                            }
                        ulong usum = 0;
                        for (int day = 0; day < d; day++)
                            for (int slot = 0; slot < s; slot++)
                                usum += wishRatio[day, slot];
                        while (usum > int.MaxValue)
                        {
                            ulong b = 10;
                            usum = 0;
                            for (int day = 0; day < d; day++)
                                for (int slot = 0; slot < s; slot++)
                                {
                                    wishRatio[day, slot] = (wishRatio[day, slot] + b - 1) / b;
                                    usum += wishRatio[day, slot];
                                }
                        }

                        Random random = new Random();
                        int a = random.Next((int)usum);
                        int sum = 0;
                        int selDay = 0;
                        int selSlot = 0;
                        for (int i = 0; i < d; i++)
                            for (int j = 0; j < s; j++)
                            {
                                sum += (int)wishRatio[i, j];
                                if (sum > a)
                                {
                                    selDay = i;
                                    selSlot = j;
                                    i = d;
                                    break;
                                }
                            }
                        sch.y[currGames[game][0], currGames[game][1]] = selDay;
                        sch.z[currGames[game][0], currGames[game][1]] = selSlot;
                        currGames.RemoveAt(game);
                    }

                }
                else if (firstFreeTour < t)
                    minGamesСonsid++;
                else
                    break;
            }
            while ((firstFreeTour < t) || (currGames.Count > 0));

            if (currGames.Count == 0)
                sch.filled = true;

            criterion = new List<int[]>();
            criterion.Add(new int[model.criteria.Count]);
            for (int i = 0; i < model.criteria.Count; i++)
                criterion[0][i] = model.criteria[i].Value(sch);
            return sch;
        }

        private void SetNextTourRivals(Schedule schedule, int tour)
        {
            Random random = new Random();
            int toursInRound = schedule.teams - 1 + schedule.teams % 2;
            int round = tour / toursInRound;
            if (tour % toursInRound == 0)
            {
                List<int> teams = new List<int>(schedule.teams);
                for (int i = 0; i < schedule.teams; i++)
                    teams.Add(i);
                for (int i = 0; i < schedule.teams; i++)
                {
                    int a = random.Next(teams.Count);
                    schedule.roundsTeams[round, i] = teams[a];
                    teams.RemoveAt(a);
                }
            }
            List<int[]> c = new List<int[]>(toursInRound);
            for (int i = 0; i < toursInRound; i++)
            {
                c.Add(new int[schedule.teams]);
                for (int j = 0; j < schedule.teams; j++)
                    c[i][(j + i) % schedule.teams] = schedule.roundsTeams[round, j];
            }
            for (int i = tour - tour % toursInRound; i < tour; i++)
            {
                for (int j = 0; j < c.Count; j++)
                    for (int k = 0; k < schedule.games; k++)
                        if (((schedule.x[i, k, 0] == c[j][0]) && (schedule.x[i, k, 1] == c[j][schedule.teams - 1])) ||
                            ((schedule.x[i, k, 1] == c[j][0]) && (schedule.x[i, k, 0] == c[j][schedule.teams - 1])))
                        {
                            c.RemoveAt(j);
                            j = c.Count;
                            break;
                        }
            }
            int e = random.Next(c.Count);
            for (int i = 0; i < schedule.games; i++)
            {
                schedule.x[tour, i, 0] = c[e][i];
                schedule.x[tour, i, 1] = c[e][schedule.teams - 1 - i];
            }
        }
    }

    public class IterAlgo : Algo
    {
        protected int iter;
        protected Algo algo;

        public IterAlgo(Algo algo, int iter = 100)
        {
            model = algo.model;
            this.iter = iter;
            this.algo = algo;
        }

        public override Schedule Solve(Schedule schedule = null)
        {
            Schedule bestFilledSchedule = null;
            int bestFilledCrit = int.MaxValue;
            Schedule bestNotFilledSchedule = null;
            int bestNotFilledCrit = 0;
            criterion = new List<int[]>();
            for (int i = 0; i < iter; i++)
            {
                Schedule sch = algo.Solve(schedule);
                int[] crit = algo.criterion[algo.criterion.Count - 1];
                int sumCrit = 0;
                for (int j = 0; j < crit.Length; j++)
                    sumCrit += crit[j] * model.criteria[j].importancePercent;
                sumCrit = (sumCrit + 100 - 1) / 100;
                if (sch.filled)
                {
                    if (sumCrit < bestFilledCrit)
                    {
                        bestFilledSchedule = sch;
                        bestFilledCrit = sumCrit;
                        criterion.Add(new int[model.criteria.Count]);
                        for (int i = 0; i < model.criteria.Count; i++)
                            criterion[Count - 1][i] = crit[i];
                    }
                }
                else if (sumCrit < bestNotFilledCrit)
                {
                    bestNotFilledSchedule = sch;
                    bestNotFilledCrit = sumCrit;
                    criterion.Add(new int[model.criteria.Count]);
                    for (int i = 0; i < model.criteria.Count; i++)
                        criterion[Count - 1][i] = crit[i];
                }
            }
            if (bestFilledSchedule != null)
                return bestFilledSchedule;
            else
                return bestNotFilledSchedule;
        }
    }
}
