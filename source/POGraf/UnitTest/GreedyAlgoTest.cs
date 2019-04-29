using Microsoft.VisualStudio.TestTools.UnitTesting;
using Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnitTest
{
    [TestClass()]
    public class GreedyAlgoTests
    {
        [TestMethod()]
        public void GetToursTest()
        {
            Model model = Sample.Generate();
            GreedyAlgo algo = new GreedyAlgo(model);

            int[,,] x = algo.GetTours();

            for (int i = 0; i < 2 * (model.n - 1); i++)
            {
                for (int j = 0; j < model.n; j++)
                {
                    int num = 0;
                    for (int k = 0; k < model.n / 2; k++)
                        for (int l = 0; l < 2; l++)
                            if (x[i, k, l] == j)
                                num++;
                    if (num != 1)
                        Assert.Fail("Не все команды играют один матч за тур");
                }
            }

            for (int i = 0; i < 2; i++)
                for (int j = 0; j < model.n - 1; j++)
                    for (int k = j + 1; k < model.n; k++)
                    {
                        int num = 0;
                        for (int l = i * (model.n - 1); l < (i + 1) * (model.n - 1); l++)
                            for (int a = 0; a < model.n / 2; a++)
                                if (((x[l, a, 0] == j) || (x[l, a, 1] == j)) && ((x[l, a, 0] == k) || (x[l, a, 1] == k)))
                                    num++;
                        if (num != 1)
                            Assert.Fail("Соперники повторяются в круге" + i);
                    }
        }

        [TestMethod()]
        public void GetDaysTest()
        {
            Model model = Sample.Generate();
            GreedyAlgo algo = new GreedyAlgo(model);

            algo.x = algo.GetTours();
            int[,] y = algo.GetDays();

            for (int i = 0; i < 2 * (model.n - 1); i++)
            {
                int prev = 0;
                int curr = 0;
                for (int j = 0; j < model.n / 2; j++)
                {
                    if (y[i, j] < prev)
                        Assert.Fail("Туры не проходят последовательно");
                    else if (y[i, j] > curr)
                        curr = y[i, j];
                }
                prev = curr;
            }

            int[] a = new int[model.d];
            for (int i = 0; i < 2 * (model.n - 1); i++)
                for (int j = 0; j < model.n / 2; j++)
                    a[y[i, j]]++;
            int max;
            for (int i = model.d - 1; ; i--)
                if (a[i] > 0)
                {
                    max = i;
                    break;
                }
            for (int i = 0; i < max; i++)
            {
                if (a[i] > 0)
                {
                    if (a[i] < model.f)
                        Assert.Fail("Недостаточное число игр в игровом дне");
                    else if (a[i] > model.g)
                        Assert.Fail("Превышено число игр в игровом дне");
                }
            }
            if (a[max] > model.g)
                Assert.Fail("Превышено число игр в игровом дне");
        }
    }
}
