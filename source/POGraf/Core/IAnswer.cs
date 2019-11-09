using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core
{
    public class Answer
    {
        public Schedule schedule;
        protected string[] teams;
        protected DateTime[,] timeSlots;

        public Answer(Schedule schedule, DateTime[,] timeSlots, string[] teams)
        {
            this.schedule = schedule;
            this.teams = teams;
            this.timeSlots = timeSlots;
        }

        public Game this[int i, int j]
        {
            get
            {
                if ((i < schedule.tours) && (j < schedule.games))
                {
                    Game game = new Game();
                    if (schedule.x[i, j, 0] != null)
                        game.teams = new string[2] { teams[(int)schedule.x[i, j, 0]], teams[(int)schedule.x[i, j, 1]] };
                    else
                        game.teams = null;
                    if (schedule.y[i, j] != null)
                        game.DateTime = timeSlots[(int)schedule.y[i, j], (int)schedule.z[i, j]];
                    else
                        game.DateTime = null;
                    return game;
                }
                else
                    return null;
            }
        }
    }

    public class Game
    {
        public string[] teams;
        public DateTime? DateTime;
    }

    public class Schedule
    {
        public int teams;
        public int rounds;
        public int tours;
        public int games;
        public int?[,,] x;
        public int?[,] y;
        public int?[,] z;
        public bool filled;
        public int[,] roundsTeams;

        public Schedule(int teams, int rounds, int tours, int games)
        {
            this.teams = teams;
            this.rounds = rounds;
            this.tours = tours;
            this.games = games;
            x = new int?[tours, games, 2];
            y = new int?[tours, games];
            z = new int?[tours, games];
            filled = false;
            for (int i = 0; i < tours; i++)
                for (int j = 0; j < games; j++)
                    x[i, j, 0] = x[i, j, 1] = y[i, j] = z[i, j] = null;
            roundsTeams = new int[rounds, teams];
        }
        public Schedule(Schedule schedule)
        {
            teams = schedule.teams;
            rounds = schedule.rounds;
            tours = schedule.tours;
            games = schedule.games;
            x = new int?[tours, games, 2];
            y = new int?[tours, games];
            z = new int?[tours, games];
            filled = schedule.filled;
            for (int i = 0; i < tours; i++)
                for (int j = 0; j < games; j++)
                {
                    x[i, j, 0] = schedule.x[i, j, 0];
                    x[i, j, 1] = schedule.x[i, j, 1];
                    y[i, j] = schedule.y[i, j];
                    z[i, j] = schedule.z[i, j];
                }
            roundsTeams = new int[rounds, teams];
            for (int i = 0; i < rounds; i++)
                for (int j = 0; j < teams; j++)
                    roundsTeams[i, j] = schedule.roundsTeams[i, j];
        }
    }
}
