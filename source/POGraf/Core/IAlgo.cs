using System;
using System.Collections.Generic;

namespace Core
{
    public abstract class Algo
    {
        public Model model;

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

        public GreedyAlgo(Model model, int minGamesСonsid = 1)
        {
            this.model = model;
            n = model.n;
            r = model.r;
            s = model.s;
            d = model.d;
            t = r * (n - 1 + n % 2);
            g = n / 2 + n % 2;
            wishes = model.wishes.ToArray();
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
            int minGames = minGamesСonsid;
            for (int i = 0; i < t; i++)
            {
                if (sch.x[i, 0, 0] == null)
                {
                    if (firstFreeTour == t)
                        firstFreeTour = i;
                }
                else
                    for (int j = 0; j < g; j++)
                        if (sch.y[i, j] == null)
                            currGames.Add(new int[2] { i, j });
            }

            do
            {
                while ((currGames.Count < minGames) && (firstFreeTour < t))
                {
                    SetNextTourRivals(sch, firstFreeTour);
                    for (int j = 0; j < g; j++)
                        currGames.Add(new int[2] { firstFreeTour, j });
                    for (firstFreeTour++; firstFreeTour < t; firstFreeTour++)
                        if (sch.x[firstFreeTour, 0, 0] == null)
                            break;
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
                        {
                            if (demSuitCoeff[i] == d * s + 1)
                                demSuitCoeff[i] = 0;
                            else
                                sum += demSuitCoeff[i];
                        }

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
                        Random random = new Random();
                        int game = (int)minDemSuitSumGame;
                        bool[,] wishRatio = new bool[d, s];
                        List<int[]> tru = new List<int[]>();
                        for (int i = 0; tru.Count == 0; i++)
                        {
                            for (int day = 0; day < d; day++)
                                for (int slot = 0; slot < s; slot++)
                                {
                                    wishRatio[day, slot] = demSuit[game, day, slot];
                                    for (int wish = 0; (wishRatio[day, slot]) && (wish < wishes.Length); wish++)
                                        if ((wishes[wish].importancePercent < 100) && (!(wishes[wish] is RivalsWish)))
                                            if (!wishes[wish].IsSuitable(day, slot, currGames[game][0], currGames[game][1], sch, model))
                                                wishRatio[day, slot] = (random.Next(100 + i) >= wishes[wish].importancePercent);
                                    if (wishRatio[day, slot])
                                        tru.Add(new int[2] { day, slot });
                                }
                        }

                        int a = random.Next(tru.Count);
                        sch.y[currGames[game][0], currGames[game][1]] = tru[a][0];
                        sch.z[currGames[game][0], currGames[game][1]] = tru[a][1];
                        currGames.RemoveAt(game);
                    }

                }
                else if (firstFreeTour < t)
                    minGames++;
                else
                    break;
            }
            while ((firstFreeTour < t) || (currGames.Count > 0));

            sch.filled = currGames.Count == 0;

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

        public IterAlgo(Algo algo, int par = 25)
        {
            model = algo.model;
            this.iter = par;
            this.algo = algo;
        }

        public override Schedule Solve(Schedule schedule = null)
        {
            Schedule bestSchedule = null;
            int bestCrit = int.MaxValue;
            Random random = new Random();
            for (int i = 0; i < iter; i++)
            {
                List<int[]> temp = new List<int[]>();
                for (int j = 0; j < algo.model.wishes.Count; j++)
                    if ((algo.model.wishes[j] is NeedHelp) && (algo.model.wishes[j].importancePercent < 100) && (random.Next(2) == 1))
                    {
                        temp.Add(new int[] { j, algo.model.wishes[j].importancePercent });
                        algo.model.wishes[j].importancePercent = 100;
                    }
                Schedule sch = algo.Solve(schedule);
                for (int j = 0; j < temp.Count; j++)
                    algo.model.wishes[temp[j][0]].importancePercent = temp[j][1];
                int[] crit = new int[model.criteria.Count];
                int sumCrit = 0;
                for (int j = 0; j < crit.Length; j++)
                {
                    crit[j] = model.criteria[j].Value(sch);
                    sumCrit += crit[j] * model.criteria[j].importancePercent;
                }
                sumCrit = (sumCrit + 100 - 1) / 100;
                if (sumCrit < bestCrit)
                {
                    bestSchedule = sch;
                    bestCrit = sumCrit;
                    i = 0;
                }
            }
            return bestSchedule;
        }
    }

    public class LocalAlgo : Algo
    {
        protected Algo algo;
        int iter;

        public LocalAlgo(Algo algo, int par = 5)
        {
            model = algo.model;
            this.algo = algo;
            this.iter = par;
        }

        public override Schedule Solve(Schedule schedule = null)
        {
            if (schedule == null)
                schedule = new Schedule(model.n, model.r, model.r * (model.n - 1 + model.n % 2), model.n / 2 + model.n % 2);
            Schedule currentSchedule = schedule;
            int currentCrit = int.MaxValue;
            Schedule nextSchedule;
            Random random = new Random();
            for (int i = 0; i < iter; i++)
            {
                List<int[]> suitable = new List<int[]>();
                nextSchedule = new Schedule(currentSchedule);
                for (int tour = 0; tour < currentSchedule.tours; tour++)
                    for (int gameInTour = 0; gameInTour < currentSchedule.games; gameInTour++)
                        if ((currentSchedule.y[tour, gameInTour].HasValue) && (!schedule.y[tour, gameInTour].HasValue))
                            for (int wish = 0; wish < model.wishes.Count; wish++)
                                if (!(model.wishes[wish] is RivalsWish))
                                {
                                    currentSchedule.y[tour, gameInTour] = currentSchedule.z[tour, gameInTour] = null;
                                    if (!model.wishes[wish].IsSuitable((int)nextSchedule.y[tour, gameInTour], (int)nextSchedule.z[tour, gameInTour], tour, gameInTour, currentSchedule, model))
                                    {
                                        currentSchedule.y[tour, gameInTour] = nextSchedule.y[tour, gameInTour];
                                        currentSchedule.z[tour, gameInTour] = nextSchedule.z[tour, gameInTour];
                                        nextSchedule.y[tour, gameInTour] = nextSchedule.z[tour, gameInTour] = null;
                                        break;
                                    }
                                    else
                                    {
                                        suitable.Add(new int[2] { tour, gameInTour });
                                        currentSchedule.y[tour, gameInTour] = nextSchedule.y[tour, gameInTour];
                                        currentSchedule.z[tour, gameInTour] = nextSchedule.z[tour, gameInTour];
                                    }
                                }
                for (int j = 0; (j < i) && (suitable.Count > 0); j++)
                {
                    int a = random.Next(suitable.Count);
                    nextSchedule.y[suitable[a][0], suitable[a][1]] = nextSchedule.z[suitable[a][0], suitable[a][1]] = null;
                    suitable.RemoveAt(a);
                }
                Schedule newSchedule = algo.Solve(nextSchedule);
                int[] crit = new int[model.criteria.Count];
                int sumCrit = 0;
                for (int j = 0; j < crit.Length; j++)
                {
                    crit[j] = model.criteria[j].Value(newSchedule);
                    sumCrit += crit[j] * model.criteria[j].importancePercent;
                }
                sumCrit = (sumCrit + 100 - 1) / 100;
                if (sumCrit < currentCrit)
                {
                    currentSchedule = newSchedule;
                    currentCrit = sumCrit;
                    i = 0;
                }
            }
            return currentSchedule;
        }
    }
}
