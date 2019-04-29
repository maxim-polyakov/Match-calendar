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
    public class AnswerTests
    {
        [TestMethod()]
        public void SortTest()
        {
            Model model = Sample.Generate();
            GreedyAlgo algo = new GreedyAlgo(model);
            Answer answer = algo.Solve();
            answer.Sort();
            for (int i = 0; i < 2 * (model.n - 1); i++)
            {
                for (int j = 1; j < model.n / 2; j++)
                    if ((answer.Days[i, j - 1] > answer.Days[i, j]) || ((answer.Days[i, j - 1] == answer.Days[i, j]) && (answer.Hours[i, j - 1] > answer.Hours[i, j])))
                        Assert.Fail();
            }
        }
    }
}
