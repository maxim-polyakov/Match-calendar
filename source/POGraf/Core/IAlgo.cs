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
        protected int[] q; // Номера дней недели по дням
        protected int[] w; // Номера недель по дням
        protected int[] m; // Номера месяцев по дням
        protected Wish[] wishes; // Пожелания

        protected int minGamesСonsid;

        public GreedyAlgo(Model model, int minGamesСonsid = 2)
        {
            this.model = model;
            n = model.n;
            r = model.r;
            s = model.s;
            d = model.d;
            q = model.q;
            w = model.w;
            m = model.m;
            t = r * (n - 1 + n % 2);
            g = n / 2 + n % 2;
            wishes = model.wishes;
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
                int minDemSuitSum = int.MaxValue;
                int? minDemSuitSumGame = null;
                for (int game = 0; game < currGames.Count; game++)
                {
                    int demSuitSum = d * s;
                    for (int day = 0; day < d; day++)
                        for (int slot = 0; slot < s; slot++)
                            demSuit[game, day, slot] = true;
                    for (int wish = 0; wish < wishes.Length; wish++)
                    {
                        if (wishes[wish].importancePercent == 100)
                            for (int day = 0; day < d; day++)
                                for (int slot = 0; slot < s; slot++)
                                    if (demSuit[game, day, slot] == true)
                                        if (wishes[wish].IsSuitable(day, slot, currGames[game][0], currGames[game][1], sch) == false)
                                        {
                                            demSuit[game, day, slot] = false;
                                            demSuitSum--;
                                        }
                    }
                    if ((demSuitSum < minDemSuitSum) && (demSuitSum != 0))
                    {
                        minDemSuitSum = demSuitSum;
                        minDemSuitSumGame = game;
                    }
                }

                if (minDemSuitSumGame.HasValue)
                {
                    int game = (int)minDemSuitSumGame;
                    int[,] wishRatio = new int[d, s];
                    for (int day = 0; day < d; day++)
                        for (int slot = 0; slot < s; slot++)
                            if (demSuit[game, day, slot] == true)
                                wishRatio[day, slot] = 10;
                            else
                                wishRatio[day, slot] = 0;
                    for (int wish = 0; wish < wishes.Length; wish++)
                        if (wishes[wish].importancePercent < 100)
                            for (int day = 0; day < d; day++)
                                for (int slot = 0; slot < s; slot++)
                                    if ((wishRatio[day, slot] != 0) &&
                                        (wishes[wish].IsSuitable(day, slot, currGames[game][0], currGames[game][1], sch)))
                                        wishRatio[day, slot] += wishes[wish].importancePercent;

                    int sum = 0;
                    for (int day = 0; day < d; day++)
                        for (int slot = 0; slot < s; slot++)
                            sum += wishRatio[day, slot];
                    Random random = new Random();
                    int a = random.Next(sum);
                    sum = 0;
                    int selDay = 0;
                    int selSlot = 0;
                    for (int i = 0; i < d; i++)
                        for (int j = 0; j < s; j++)
                        {
                            sum += wishRatio[i, j];
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
                else if (firstFreeTour < t)
                    minGamesСonsid++;
                else
                    break;
            }
            while ((firstFreeTour < t) || (currGames.Count > 0));

            if (currGames.Count == 0)
                sch.filled = true;

            criterion.Add(new int[1] { model.Criterion(sch) });
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
                    teams[i] = i;
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
                c[i] = new int[schedule.teams];
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
            int bestFilledCrit = 0;
            Schedule bestNotFilledSchedule = null;
            int bestNotFilledCrit = 0;
            for (int i = 0; i < iter; i++)
            {
                Schedule sch = algo.Solve(schedule);
                int crit = model.Criterion(sch);

                if (sch.filled)
                {
                    if (crit > bestFilledCrit)
                    {
                        bestFilledSchedule = sch;
                        bestFilledCrit = crit;
                        criterion.Add(new int[1] { crit });
                    }
                }
                else if (crit > bestNotFilledCrit)
                {
                    bestNotFilledSchedule = sch;
                    bestNotFilledCrit = crit;
                }
            }
            if (bestFilledSchedule != null)
                return bestFilledSchedule;
            else
                return bestNotFilledSchedule;
        }
    }
}
