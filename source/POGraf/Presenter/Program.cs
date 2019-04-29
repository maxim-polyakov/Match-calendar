using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core;
using System.Data;
using System.IO;

namespace Presenter
{
    class Program
    {
        static void Main(string[] args)
        {
            int n = 12;
            IPresenter p = new Presenter();
            Model mod = new Model();
            mod.n = n;
            IAlgo alg = new GreedyAlgo(mod);

            IAnswer ans = alg.Solve();
            p.SetAnswer(ans);
            p.SetModel(mod);


            var dataTable = new DataTable();

            for (int i = 0; i < (4 * (2 * (n - 1))); i++)
            {
                dataTable.Columns.Add(i.ToString());

            }
            for (int j = 0; j < n / 2; j++)
            {
                var row = dataTable.NewRow();
                for (int i = 0; i < (4 * (2 * (n - 1))); i += 4)
                {
                    row[i.ToString()] = "Соперник 1";

                }
                for (int i = 1; i < (4 * (2 * (n - 1))); i += 4)
                {
                    row[i.ToString()] = "Cоперник2";

                }
                for (int i = 2; i < (4 * (2 * (n - 1))); i += 4)
                {
                    row[i.ToString()] = (p.GetAnswer().GetInfo().Days[0, 0]).ToString();

                }
                for (int i = 3; i < (4 * (2 * (n - 1))); i += 4)
                {
                    row[i.ToString()] = (p.GetAnswer().GetInfo().Hours[0, 0]).ToString();



                }
                dataTable.Rows.Add(row);

            }
            dataTable.AcceptChanges();
            var html = p.GetHtml(dataTable);

            using (FileStream fstream = new FileStream(@"C:\Users\maxim\Desktop\СпецСеминар\H.html", FileMode.OpenOrCreate))
            {

                byte[] input = Encoding.Default.GetBytes(html);

                fstream.Write(input, 0, input.Length);

            }

        }
    }
}
