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
        public List<DateTime> gameDates; // Список дат игровых дней

        public int n; // Число команд
        public int r; // Число турнирных кругов
        public int s; // Число временных слотов в день
        public int d; // Число дней
        public int[] q; // Номера дней недели по дням
        public int[] w; // Номера недель по дням
        public int[] m; // Номера месяцев по дням
        public List<Wish> wishes; // Пожелания

        public Model InvalidModel()
        {
            Model model = new Model();
            model.invalid = true;
            return model;
        }

        public int[] Criterion(IAnswer answer)
        {
            return new int[1] { Criterion(answer.schedule) };
        }

        public int Criterion(Schedule schedule)
        {
            Schedule sch = new Schedule(schedule);
            int sum = 0;
            for (int i = 0; i < wishes.Count; i++)
            {
                if (wishes[i].importancePercent < 100)
                {
                    int plus = 0;
                    int minus = 0;
                    for (int j = 0; j < schedule.tours; j++)
                        for (int k = 0; k < schedule.games; k++)
                        {
                            if (schedule.y[j, k].HasValue)
                            {
                                sch.y[j, k] = sch.z[j, k] = null;
                                if (wishes[i].IsSuitable((int)schedule.y[j, k], (int)schedule.z[j, k], j, k, sch, this))
                                    plus++;
                                else
                                    minus++;
                                sch.y[j, k] = schedule.y[j, k];
                                sch.z[j, k] = schedule.z[j, k];
                            }
                            else
                                minus++;
                        }
                    sum += wishes[i].importancePercent * plus / (plus + minus);
                }
            }
            return sum;
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
        public DayTeamWish(int importancePercent, int team, int[] days) : base(importancePercent, team)
        {
            this.days = days;
        }
        public override bool IsSuitable(int day, int slot, int tour, int gameInTour, Schedule schedule, Model model)
        {
            if ((schedule.x[tour, gameInTour, 0] == team) || (schedule.x[tour, gameInTour, 1] == team))
            {
                for (int i = 0; i < days.Length; i++)
                    if (days[i] == day)
                        return true;
                return false;
            }
            else
                return true;
        }
    }
    public class TimeSlotTeamWish : TeamWish
    {
        int[] slots;
        public TimeSlotTeamWish(int importancePercent, int team, int[] slots) : base(importancePercent, team)
        {
            this.slots = slots;
        }
        public override bool IsSuitable(int day, int slot, int tour, int gameInTour, Schedule schedule, Model model)
        {
            if ((schedule.x[tour, gameInTour, 0] == team) || (schedule.x[tour, gameInTour, 1] == team))
            {
                for (int i = 0; i < slots.Length; i++)
                    if (slots[i] == slot)
                        return true;
                return false;
            }
            else
                return true;
        }
    }
    public class DayTimeSlotTeamWish : TeamWish
    {
        int[][] daysSlots;

        public DayTimeSlotTeamWish(int importancePercent, int team, int[][] daysSlots) : base(importancePercent, team)
        {
            this.daysSlots = daysSlots;
        }
        public override bool IsSuitable(int day, int slot, int tour, int gameInTour, Schedule schedule, Model model)
        {
            if ((schedule.x[tour, gameInTour, 0] == team) || (schedule.x[tour, gameInTour, 1] == team))
            {
                for (int i = 0; i < daysSlots.Length; i++)
                    if ((daysSlots[i][0] == day) && (daysSlots[i][1] == slot))
                        return true;
                return false;
            }
            else
                return true;
        }
    }
}
