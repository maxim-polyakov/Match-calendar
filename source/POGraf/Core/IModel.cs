using System;
using System.Collections.Generic;

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
        public int r; // Число турнирных кругов
        public int s; // Число временных слотов в день
        public int d; // Число дней
        public int[] q; // Номера дней недели по дням
        public int[] w; // Номера недель по дням
        public int[] m; // Номера месяцев по дням
        public Wish[] wishes; // Пожелания

        public Model InvalidModel()
        {
            Model model = new Model();
            model.invalid = true;
            return model;
        }

        public int[] Criterion(Answer answer)
        {
            return new int[1] { Criterion(answer.schedule) };
        }

        public int Criterion(Schedule schedule)
        {
            Schedule sch = new Schedule(schedule);
            int sum = 0;
            for (int i = 0; i < wishes.Length; i++)
            {
                int plus = 0;
                int minus = 0;
                for (int j = 0; j < schedule.tours; j++)
                    for (int k = 0; k < schedule.games; k++)
                    {
                        if (schedule.y[j, k].HasValue)
                        {
                            sch.y[j, k] = sch.z[j, k] = null;
                            if (wishes[i].IsSuitable((int)schedule.y[j, k], (int)schedule.z[j, k], j, k, sch))
                                plus++;
                            else
                                minus++;
                            sch.y[j, k] = schedule.y[j, k];
                            sch.z[j, k] = schedule.z[j, k];
                        }
                        else
                            minus++; ;
                    }
                sum += wishes[i].importancePercent * plus / (plus + minus);
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

        public abstract bool IsSuitable(int day, int slot, int tour, int gameInTour, Schedule schedule);
    }
    public abstract class TeamWish : Wish
    {
        public int team;

        public TeamWish(int importancePercent, int team) : base(importancePercent)
        {
            this.team = team;
        }
    }

    public class ToursInOrderWish : Wish
    {
        public ToursInOrderWish(int importancePercent) : base(importancePercent)
        {
        }

        public override bool IsSuitable(int day, int slot, int tour, int gameInTour, Schedule schedule)
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

        public override bool IsSuitable(int day, int slot, int tour, int gameInTour, Schedule schedule)
        {
            for (int i = 0; i < schedule.tours; i++)
                for (int j = 0; j < schedule.games; j++)
                    if ((schedule.y[i, j] == day) && (schedule.z[i, j] == slot))
                        return false;
            return true;
        }
    }
}
