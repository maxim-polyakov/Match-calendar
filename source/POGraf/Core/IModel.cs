using System;
using System.Collections.Generic;

namespace Core
{
    public class Model
    {
        public bool invalid; // Флаг корректности модели

        public int n
        {
            get { return teams.Length; }
        }
        public int r; // Число турнирных кругов
        public int d
        {
            get { return dates.Count; }
        }
        public int s
        {
            get { return times.Count; }
        }
        public List<DateTime> dates;
        public List<int> times;
        public List<Wish> wishes;
        public List<Criterion> criteria;
        public string[] teams;

        public Model(string[] teams, int rounds, List<DateTime> dates, List<int> times)
        {
            this.teams = (string[])teams.Clone();
            r = rounds;
            this.dates = new List<DateTime>(dates);
            this.times = new List<int>(times);
            wishes = new List<Wish>();
            criteria = new List<Criterion>();
            criteria.Add(new OrganizerCriterion(this));
            criteria.Add(new TeamsCriterion(this));
        }
    }

    public abstract class Wish
    {
        public int importancePercent;

        public Wish(int importancePercent)
        {
            this.importancePercent = importancePercent;
        }

        public abstract bool IsSuitable(int day, int slot, int tour, int gameInTour, Schedule schedule, Model model);
        public abstract int numErrors(Schedule schedule, Model model);
    }
    public abstract class TeamWish : Wish
    {
        public int team;

        public TeamWish(int importancePercent, int team) : base(importancePercent)
        {
            this.team = team;
        }
    }
    public abstract class RivalsWish : Wish
    {
        public RivalsWish(int importancePercent) : base(importancePercent)
        {
        }
    }
    public interface NeedHelp
    {
    }

    public class ToursInOrder : Wish, NeedHelp
    {
        public ToursInOrder(int importancePercent) : base(importancePercent)
        {
        }

        public override bool IsSuitable(int day, int slot, int tour, int gameInTour, Schedule schedule, Model model)
        {
            int a = day * model.s + slot;
            int b = model.d * model.s * tour / schedule.tours;
            int c = model.d * model.s * (tour + 1) / schedule.tours - 1;
            if (tour > 0)
            {
                int max = -1;
                int i;
                for (i = 0; i < schedule.games; i++)
                    if ((schedule.y[tour - 1, i].HasValue) && (schedule.z[tour - 1, i].HasValue))
                    {
                        int d = (int)schedule.y[tour - 1, i] * model.s + (int)schedule.z[tour - 1, i];
                        if (d > max)
                            max = d;
                    }
                if (max > -1)
                    b = (int)max;
            }
            if (tour < schedule.tours - 1)
            {
                int min = int.MaxValue;
                int i;
                for (i = 0; i < schedule.games; i++)
                    if ((schedule.y[tour + 1, i].HasValue) && (schedule.z[tour + 1, i].HasValue))
                    {
                        int d = (int)schedule.y[tour + 1, i] * model.s + (int)schedule.z[tour + 1, i];
                        if (d < min)
                            min = d;
                    }
                if (min < int.MaxValue)
                    c = min;
            }
            return ((a >= b) && (a <= c));
        }

        public override int numErrors(Schedule schedule, Model model)
        {
            List<int[]>[,] vs = new List<int[]>[schedule.tours, schedule.games];
            for (int i = 0; i < schedule.tours; i++)
                for (int j = 0; j < schedule.games; j++)
                    vs[i, j] = new List<int[]>();
            for (int tour = 0; tour < schedule.tours; tour++)
                for (int game = 0; game < schedule.games; game++)
                    if (schedule.y[tour, game].HasValue)
                        for (int i = tour + 1; i < schedule.tours; i++)
                            for (int j = 0; j < schedule.games; j++)
                                if (schedule.y[i, j].HasValue)
                                    if ((schedule.y[tour, game] > schedule.y[i, j])
                                        || ((schedule.y[tour, game] == schedule.y[i, j]) && (schedule.z[tour, game] > schedule.z[i, j])))
                                    {
                                        vs[tour, game].Add(new int[2] { i, j });
                                        vs[i, j].Add(new int[2] { tour, game });
                                    }
            int[] tg = new int[2];
            int value = 0;
            for (int max = 0; ; max = 0)
            {
                for (int i = 0; i < schedule.tours; i++)
                    for (int j = 0; j < schedule.games; j++)
                        if (vs[i, j].Count > max)
                        {
                            max = vs[i, j].Count;
                            tg[0] = i;
                            tg[1] = j;
                        }
                if (max > 0)
                {
                    value++;
                    for (int i = 0; i < vs[tg[0], tg[1]].Count; i++)
                    {
                        int[] a = vs[tg[0], tg[1]][i];
                        for (int k = 0; k < vs[a[0], a[1]].Count; k++)
                            if ((vs[a[0], a[1]][k][0] == tg[0]) && (vs[a[0], a[1]][k][1] == tg[1]))
                                vs[a[0], a[1]].RemoveAt(k);
                    }
                    vs[tg[0], tg[1]] = new List<int[]>();
                }
                else
                    break;
            }
            return value;
        }
    }
    public class DayOfWeekWish : Wish
    {
        DayOfWeek[] daysOfWeek;
        bool wish;
        public DayOfWeekWish(int importancePercent, DayOfWeek[] daysOfWeek, bool wish = true) : base(importancePercent)
        {
            this.daysOfWeek = daysOfWeek;
            this.wish = wish;
        }
        public override bool IsSuitable(int day, int slot, int tour, int gameInTour, Schedule schedule, Model model)
        {
            for (int i = 0; i < daysOfWeek.Length; i++)
                if (daysOfWeek[i] == model.dates[day].DayOfWeek)
                    return wish;
            return !wish;
        }

        public override int numErrors(Schedule schedule, Model model)
        {
            int sum = 0;
            for (int j = 0; j < schedule.tours; j++)
                for (int k = 0; k < schedule.games; k++)
                {
                    if (schedule.y[j, k].HasValue)
                    {
                        int[] temp = new int[] { (int)schedule.y[j, k], (int)schedule.z[j, k] };
                        schedule.y[j, k] = schedule.z[j, k] = null;
                        if (!IsSuitable(temp[0], temp[1], j, k, schedule, model))
                            sum++;
                        schedule.y[j, k] = temp[0];
                        schedule.z[j, k] = temp[1];
                    }
                    else
                        sum++;
                }
            return sum;
        }
    }
    public class MaxGamesPerWeek : Wish
    {
        int num;
        public MaxGamesPerWeek(int importancePercent, int num) : base(importancePercent)
        {
            this.num = num;
        }

        public override bool IsSuitable(int day, int slot, int tour, int gameInTour, Schedule schedule, Model model)
        {
            int dayOfWeek = (int)model.dates[day].DayOfWeek;
            if (dayOfWeek == 0)
                dayOfWeek = 7;
            dayOfWeek--;
            int[] sum = new int[2] { 0, 0 };
            for (int i = 0; i < schedule.tours; i++)
                for (int j = 0; j < schedule.games; j++)
                    for (int k = 0; k < 2; k++)
                        if ((schedule.x[i, j, 0] == schedule.x[tour, gameInTour, k]) || (schedule.x[i, j, 1] == schedule.x[tour, gameInTour, k]))
                            if (schedule.y[i, j].HasValue)
                            {
                                int a = dayOfWeek + (int)((model.dates[(int)schedule.y[i, j]] - (model.dates[day])).TotalDays);
                                if ((a >= 0) && (a <= 6))
                                    sum[k]++;
                            }
            for (int i = 0; i < 2; i++)
                if (sum[i] >= num)
                    return false;
            return true;
        }
        public override int numErrors(Schedule schedule, Model model)
        {
            int value = 0;
            int[,] numsGamesPerDay = new int[model.n, model.d];
            for (int i = 0; i < schedule.tours; i++)
                for (int j = 0; j < schedule.games; j++)
                    for (int k = 0; k < 2; k++)
                    {
                        if (schedule.y[i, j].HasValue)
                            numsGamesPerDay[(int)schedule.x[i, j, k], (int)schedule.y[i, j]]++;
                        else
                            value++;
                    }
            for (int i = 0; i < model.n; i++)
            {
                int sum = numsGamesPerDay[i, 0];
                for (int j = 1; j < model.d; j++)
                {
                    if ((model.dates[j - 1].DayOfWeek != DayOfWeek.Sunday)
                        && (((int)model.dates[j - 1].DayOfWeek + (model.dates[j] - model.dates[j - 1]).TotalDays) < 8))
                        sum += numsGamesPerDay[i, j];
                    else
                    {
                        if (sum > num)
                            value += sum - num;
                        sum = numsGamesPerDay[i, j];
                    }
                }
                if (sum > num)
                    value += sum - num;
            }
            return value;
        }
    }
    public class MaxGamesLeadersPerDay : Wish
    {
        int num;
        int[] teams;

        public MaxGamesLeadersPerDay(int importancePercent, int num, int[] teams) : base(importancePercent)
        {
            this.num = num;
            this.teams = teams;
        }

        public override bool IsSuitable(int day, int slot, int tour, int gameInTour, Schedule schedule, Model model)
        {
            int sum = 0;
            for (int i = 0; i < schedule.tours; i++)
                for (int j = 0; j < schedule.games; j++)
                {
                    for (int k = 0; k < 2; k++)
                    {
                        int l;
                        for (l = 0; l < teams.Length; l++)
                            if (schedule.x[tour, gameInTour, k] == teams[l])
                                break;
                        if (l == teams.Length)
                            break;
                        else if ((k == 1) && (schedule.y[tour, gameInTour] == day))
                            sum++;
                    }
                }
            return (sum < num);
        }
        public override int numErrors(Schedule schedule, Model model)
        {
            int sum = 0;
            for (int j = 0; j < schedule.tours; j++)
                for (int k = 0; k < schedule.games; k++)
                {
                    if (schedule.y[j, k].HasValue)
                    {
                        int[] temp = new int[] { (int)schedule.y[j, k], (int)schedule.z[j, k] };
                        schedule.y[j, k] = schedule.z[j, k] = null;
                        if (!IsSuitable(temp[0], temp[1], j, k, schedule, model))
                            sum++;
                        schedule.y[j, k] = temp[0];
                        schedule.z[j, k] = temp[1];
                    }
                    else
                        sum++;
                }
            return sum;
        }
    }
    public class LeadersWish : RivalsWish
    {
        int[] teams;
        int[] tours;

        public LeadersWish(int importancePercent, int[] teams, int[] tours) : base(importancePercent)
        {
            this.teams = teams;
            this.tours = tours;
        }

        public override bool IsSuitable(int day, int slot, int tour, int gameInTour, Schedule schedule, Model model)
        {
            for (int i = 0; i < 2; i++)
            {
                int j;
                for (j = 0; j < teams.Length; j++)
                    if (schedule.x[tour, gameInTour, i] == teams[j])
                        break;
                if (j == teams.Length)
                    return true;
            }
            for (int i = 0; i < tours.Length; i++)
                if (tour == tours[i])
                    return true;
            return false;
        }
        public override int numErrors(Schedule schedule, Model model)
        {
            int sum = 0;
            for (int j = 0; j < schedule.tours; j++)
                for (int k = 0; k < schedule.games; k++)
                {
                    if (schedule.y[j, k].HasValue)
                    {
                        int[] temp = new int[] { (int)schedule.y[j, k], (int)schedule.z[j, k] };
                        schedule.y[j, k] = schedule.z[j, k] = null;
                        if (!IsSuitable(temp[0], temp[1], j, k, schedule, model))
                            sum++;
                        schedule.y[j, k] = temp[0];
                        schedule.z[j, k] = temp[1];
                    }
                    else
                        sum++;
                }
            return sum;
        }
    }
    public class MaxGamesPerHour : Wish
    {
        int num;
        public MaxGamesPerHour(int importancePercent, int num) : base(importancePercent)
        {
            this.num = num;
        }

        public override bool IsSuitable(int day, int slot, int tour, int gameInTour, Schedule schedule, Model model)
        {
            int sum = 0;
            for (int i = 0; i < schedule.tours; i++)
                for (int j = 0; j < schedule.games; j++)
                    if ((schedule.y[i, j] == day) && (schedule.z[i, j] == slot))
                        sum++;
            return (sum < num);
        }
        public override int numErrors(Schedule schedule, Model model)
        {
            int sum = 0;
            for (int j = 0; j < schedule.tours; j++)
                for (int k = 0; k < schedule.games; k++)
                {
                    if (schedule.y[j, k].HasValue)
                    {
                        int[] temp = new int[] { (int)schedule.y[j, k], (int)schedule.z[j, k] };
                        schedule.y[j, k] = schedule.z[j, k] = null;
                        if (!IsSuitable(temp[0], temp[1], j, k, schedule, model))
                            sum++;
                        schedule.y[j, k] = temp[0];
                        schedule.z[j, k] = temp[1];
                    }
                    else
                        sum++;
                }
            return sum;
        }
    }
    public class Evenly : Wish
    {
        public Evenly(int importancePercent) : base(importancePercent)
        {
        }

        public override bool IsSuitable(int day, int slot, int tour, int gameInTour, Schedule schedule, Model model)
        {
            if ((day >= (model.d * tour / schedule.tours)) && (day < (model.d * (tour + 1) / schedule.tours)))
                return true;
            else
                return false;
        }
        public override int numErrors(Schedule schedule, Model model)
        {
            int sum = 0;
            for (int j = 0; j < schedule.tours; j++)
                for (int k = 0; k < schedule.games; k++)
                {
                    if (schedule.y[j, k].HasValue)
                    {
                        int[] temp = new int[] { (int)schedule.y[j, k], (int)schedule.z[j, k] };
                        schedule.y[j, k] = schedule.z[j, k] = null;
                        if (!IsSuitable(temp[0], temp[1], j, k, schedule, model))
                            sum++;
                        schedule.y[j, k] = temp[0];
                        schedule.z[j, k] = temp[1];
                    }
                    else
                        sum++;
                }
            return sum;
        }
    }
    public class MinMaxGamesPerDay : Wish
    {
        int min;
        int max;
        public MinMaxGamesPerDay(int importancePercent, int min, int max) : base(importancePercent)
        {
            this.min = min;
            this.max = max;
        }

        public override bool IsSuitable(int day, int slot, int tour, int gameInTour, Schedule schedule, Model model)
        {
            int[] sum = new int[model.d];
            for (int i = 0; i < day; i++)
                sum[i] = 0;
            int nul = 0;
            for (int i = 0; i < schedule.tours; i++)
                for (int j = 0; j < schedule.games; j++)
                {
                    if (schedule.y[i, j].HasValue)
                        sum[(int)schedule.y[i, j]]++;
                    else
                        nul++;
                }
            if (sum[day] >= max)
                return false;
            else if (sum[day] >= min)
            {
                int a = 0;
                for (int i = 0; i < model.d; i++)
                    if ((sum[i] > 0) && (sum[i] < min))
                        a += min - sum[i];
                if (nul > a)
                    return true;
                else
                    return false;
            }
            else if (sum[day] > 0)
                return true;
            else
            {
                int a = 0;
                for (int i = 0; i < model.d; i++)
                    if ((sum[i] > 0) && (sum[i] < min))
                        a += min - sum[i];
                a += min;
                if (nul >= a)
                    return true;
                else
                    return false;
            }
        }
        public override int numErrors(Schedule schedule, Model model)
        {
            int sum = 0;
            for (int j = 0; j < schedule.tours; j++)
                for (int k = 0; k < schedule.games; k++)
                {
                    if (schedule.y[j, k].HasValue)
                    {
                        int[] temp = new int[] { (int)schedule.y[j, k], (int)schedule.z[j, k] };
                        schedule.y[j, k] = schedule.z[j, k] = null;
                        if (!IsSuitable(temp[0], temp[1], j, k, schedule, model))
                            sum++;
                        schedule.y[j, k] = temp[0];
                        schedule.z[j, k] = temp[1];
                    }
                    else
                        sum++;
                }
            return sum;
        }
    }
    public class DayTeamWish : TeamWish
    {
        int[] days;
        bool wish;
        public DayTeamWish(int importancePercent, int team, int[] days, bool wish = true) : base(importancePercent, team)
        {
            this.days = days;
            this.wish = wish;
        }
        public override bool IsSuitable(int day, int slot, int tour, int gameInTour, Schedule schedule, Model model)
        {
            if ((schedule.x[tour, gameInTour, 0] == team) || (schedule.x[tour, gameInTour, 1] == team))
            {
                for (int i = 0; i < days.Length; i++)
                    if (days[i] == day)
                        if (wish)
                            return true;
                        else
                            return false;
                if (wish)
                    return false;
                else
                    return true;
            }
            else
                return true;
        }
        public override int numErrors(Schedule schedule, Model model)
        {
            int sum = 0;
            for (int j = 0; j < schedule.tours; j++)
                for (int k = 0; k < schedule.games; k++)
                {
                    if (schedule.y[j, k].HasValue)
                    {
                        int[] temp = new int[] { (int)schedule.y[j, k], (int)schedule.z[j, k] };
                        schedule.y[j, k] = schedule.z[j, k] = null;
                        if (!IsSuitable(temp[0], temp[1], j, k, schedule, model))
                            sum++;
                        schedule.y[j, k] = temp[0];
                        schedule.z[j, k] = temp[1];
                    }
                    else
                        sum++;
                }
            return sum;
        }
    }
    public class DayOfWeekTeamWish : TeamWish
    {
        DayOfWeek[] daysOfWeek;
        bool wish;
        public DayOfWeekTeamWish(int importancePercent, int team, DayOfWeek[] daysOfWeek, bool wish = true) : base(importancePercent, team)
        {
            this.daysOfWeek = daysOfWeek;
            this.wish = wish;
        }
        public override bool IsSuitable(int day, int slot, int tour, int gameInTour, Schedule schedule, Model model)
        {
            if ((schedule.x[tour, gameInTour, 0] == team) || (schedule.x[tour, gameInTour, 1] == team))
            {
                for (int i = 0; i < daysOfWeek.Length; i++)
                    if (daysOfWeek[i] == model.dates[day].DayOfWeek)
                        return wish;
                return !wish;
            }
            else
                return true;
        }
        public override int numErrors(Schedule schedule, Model model)
        {
            int sum = 0;
            for (int j = 0; j < schedule.tours; j++)
                for (int k = 0; k < schedule.games; k++)
                {
                    if (schedule.y[j, k].HasValue)
                    {
                        int[] temp = new int[] { (int)schedule.y[j, k], (int)schedule.z[j, k] };
                        schedule.y[j, k] = schedule.z[j, k] = null;
                        if (!IsSuitable(temp[0], temp[1], j, k, schedule, model))
                            sum++;
                        schedule.y[j, k] = temp[0];
                        schedule.z[j, k] = temp[1];
                    }
                    else
                        sum++;
                }
            return sum;
        }
    }
    public class TimeSlotTeamWish : TeamWish
    {
        int[] slots;
        bool wish;
        public TimeSlotTeamWish(int importancePercent, int team, int[] slots, bool wish = true) : base(importancePercent, team)
        {
            this.slots = slots;
            this.wish = wish;
        }
        public override bool IsSuitable(int day, int slot, int tour, int gameInTour, Schedule schedule, Model model)
        {
            if ((schedule.x[tour, gameInTour, 0] == team) || (schedule.x[tour, gameInTour, 1] == team))
            {
                for (int i = 0; i < slots.Length; i++)
                    if (slots[i] == slot)
                        if (wish)
                            return true;
                        else
                            return false;
                if (wish)
                    return false;
                else
                    return true;
            }
            else
                return true;
        }
        public override int numErrors(Schedule schedule, Model model)
        {
            int sum = 0;
            for (int j = 0; j < schedule.tours; j++)
                for (int k = 0; k < schedule.games; k++)
                {
                    if (schedule.y[j, k].HasValue)
                    {
                        int[] temp = new int[] { (int)schedule.y[j, k], (int)schedule.z[j, k] };
                        schedule.y[j, k] = schedule.z[j, k] = null;
                        if (!IsSuitable(temp[0], temp[1], j, k, schedule, model))
                            sum++;
                        schedule.y[j, k] = temp[0];
                        schedule.z[j, k] = temp[1];
                    }
                    else
                        sum++;
                }
            return sum;
        }
    }
    public class DayTimeSlotTeamWish : TeamWish
    {
        int[][] daysSlots;
        bool wish;

        public DayTimeSlotTeamWish(int importancePercent, int team, int[][] daysSlots, bool wish = true) : base(importancePercent, team)
        {
            this.daysSlots = daysSlots;
            this.wish = wish;
        }
        public override bool IsSuitable(int day, int slot, int tour, int gameInTour, Schedule schedule, Model model)
        {
            if ((schedule.x[tour, gameInTour, 0] == team) || (schedule.x[tour, gameInTour, 1] == team))
            {
                for (int i = 0; i < daysSlots.Length; i++)
                    if ((daysSlots[i][0] == day) && (daysSlots[i][1] == slot))
                    {
                        if (wish)
                            return true;
                        else
                            return false;
                    }
                if (wish)
                    return false;
                else
                    return true;
            }
            else
                return true;
        }
        public override int numErrors(Schedule schedule, Model model)
        {
            int sum = 0;
            for (int j = 0; j < schedule.tours; j++)
                for (int k = 0; k < schedule.games; k++)
                {
                    if (schedule.y[j, k].HasValue)
                    {
                        int[] temp = new int[] { (int)schedule.y[j, k], (int)schedule.z[j, k] };
                        schedule.y[j, k] = schedule.z[j, k] = null;
                        if (!IsSuitable(temp[0], temp[1], j, k, schedule, model))
                            sum++;
                        schedule.y[j, k] = temp[0];
                        schedule.z[j, k] = temp[1];
                    }
                    else
                        sum++;
                }
            return sum;
        }
    }

    public abstract class Criterion
    {
        public int importancePercent;
        protected Model model;
        public Criterion(Model model, int importancePercent)
        {
            this.model = model;
            this.importancePercent = importancePercent;
        }
        public abstract int Value(Schedule schedule);
    }
    public class OrganizerCriterion : Criterion
    {
        public OrganizerCriterion(Model model, int importancePercent = 100) : base(model, importancePercent)
        {
        }
        public override int Value(Schedule schedule)
        {
            Schedule sch = new Schedule(schedule);
            int value = 0;
            for (int i = 0; i < model.wishes.Count; i++)
                if ((model.wishes[i].importancePercent < 100) && (!(model.wishes[i] is TeamWish)))
                    value += model.wishes[i].importancePercent * model.wishes[i].numErrors(sch, model);
            value = (value + 99 - 1) / 99;
            return value;
        }
    }
    public class TeamsCriterion : Criterion
    {
        public TeamsCriterion(Model model, int importancePercent = 100) : base(model, importancePercent)
        {
        }
        public override int Value(Schedule schedule)
        {
            Schedule sch = new Schedule(schedule);
            int value = 0;
            int[] cfFailsTeams = new int[model.n];
            int[] numWishesTeam = new int[model.n];
            for (int i = 0; i < model.wishes.Count; i++)
            {
                if ((model.wishes[i].importancePercent < 100) && (model.wishes[i] is TeamWish))
                {
                    cfFailsTeams[((TeamWish)(model.wishes[i])).team] += model.wishes[i].importancePercent * model.wishes[i].numErrors(sch, model);
                    numWishesTeam[((TeamWish)(model.wishes[i])).team] += 1;
                }
            }
            for (int i = 0; i < model.n; i++)
                if (numWishesTeam[i] > 0)
                    value += cfFailsTeams[i] / numWishesTeam[i];
            value = (value + 99 - 1) / 99;
            return value;
        }
    }
}