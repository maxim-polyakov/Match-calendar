using System;
using System.Collections.Generic;

namespace Core
{
    public interface IModel
    {
        int[] Criterion(IAnswer answer);
    }

    public class Model : IModel
    {
        public bool invalid; // Флаг корректности модели

        public int n; // Число команд
        public int r; // Число турнирных кругов
        public int s; // Число временных слотов в день
        public int d; // Число дней
        public List<DateTime> dates;
        public List<Wish> wishes;
        public List<Criterion> criteria;

        public Model InvalidModel()
        {
            Model model = new Model();
            model.invalid = true;
            return model;
        }

        public Model(int teams, int rounds, int slotsPerDay, List<DateTime> dates)
        {
            n = teams;
            r = rounds;
            s = slotsPerDay;
            this.dates = dates;
            d = dates.Count;
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
    }
    public abstract class TeamWish : Wish
    {
        public int team;

        public TeamWish(int importancePercent, int team) : base(importancePercent)
        {
            this.team = team;
        }
    }

    public class ToursInOrder : Wish
    {
        public ToursInOrder(int importancePercent) : base(importancePercent)
        {
        }

        public override bool IsSuitable(int day, int slot, int tour, int gameInTour, Schedule schedule, Model model)
        {
            if (tour > 0)
            {
                for (int i = 0; i < schedule.games; i++)
                    if (schedule.y[tour - 1, i].HasValue)
                    {
                        if ((schedule.y[tour - 1, i] > day) ||
                            ((schedule.y[tour - 1, i] == day) && (schedule.z[tour - 1, i] >= slot)))
                            return false;
                    }
                    else
                        return false;
            }
            return true;
        }
    }
    public class NotMoreOneGameSlot : Wish
    {
        public NotMoreOneGameSlot(int importancePercent) : base(importancePercent)
        {
        }

        public override bool IsSuitable(int day, int slot, int tour, int gameInTour, Schedule schedule, Model model)
        {
            for (int i = 0; i < schedule.tours; i++)
                for (int j = 0; j < schedule.games; j++)
                    if ((schedule.y[i, j] == day) && (schedule.z[i, j] == slot))
                        return false;
            return true;
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
    }
    public class MinMaxGamesDay : Wish
    {
        int min;
        int max;
        public MinMaxGamesDay(int importancePercent, int min, int max) : base(importancePercent)
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
            {
                if ((model.wishes[i].importancePercent < 100) && (!(model.wishes[i] is TeamWish)))
                {
                    int numNotSuitable = 0;
                    for (int j = 0; j < schedule.tours; j++)
                        for (int k = 0; k < schedule.games; k++)
                        {
                            if (schedule.y[j, k].HasValue)
                            {
                                sch.y[j, k] = sch.z[j, k] = null;
                                if (!model.wishes[i].IsSuitable((int)schedule.y[j, k], (int)schedule.z[j, k], j, k, sch, model))
                                    numNotSuitable++;
                                sch.y[j, k] = schedule.y[j, k];
                                sch.z[j, k] = schedule.z[j, k];
                            }
                            else
                                numNotSuitable++;
                        }
                    value += model.wishes[i].importancePercent * numNotSuitable;
                }
            }
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
                    int numNotSuitable = 0;
                    for (int j = 0; j < schedule.tours; j++)
                        for (int k = 0; k < schedule.games; k++)
                        {
                            if (schedule.y[j, k].HasValue)
                            {
                                sch.y[j, k] = sch.z[j, k] = null;
                                if (!model.wishes[i].IsSuitable((int)schedule.y[j, k], (int)schedule.z[j, k], j, k, sch, model))
                                    numNotSuitable++;
                                sch.y[j, k] = schedule.y[j, k];
                                sch.z[j, k] = schedule.z[j, k];
                            }
                            else
                                numNotSuitable++;
                        }
                    cfFailsTeams[((TeamWish)(model.wishes[i])).team] += model.wishes[i].importancePercent * numNotSuitable;
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
