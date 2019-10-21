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
            return 0;
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
            // доделать ограничение сверху
            if (tour > 0)
            {
                int maxDay = 0;
                int maxSlot = 0;
                for (int i = 0; i < schedule.games; i++)
                    if (schedule.y[tour - 1, i].HasValue)
                    {
                        if (schedule.y[tour - 1, i] > maxDay)
                        {
                            maxDay = (int)schedule.y[tour - 1, i];
                            maxSlot = (int)schedule.z[tour - 1, i];
                        }
                        else if ((schedule.y[tour - 1, i] == maxDay) && (schedule.z[tour - 1, i] > maxSlot))
                            maxSlot = (int)schedule.z[tour - 1, i];
                    }
                if ((day > maxDay) || ((day == maxDay) && (slot > maxSlot)))
                    return true;
                else
                    return false;
            }
            else
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
