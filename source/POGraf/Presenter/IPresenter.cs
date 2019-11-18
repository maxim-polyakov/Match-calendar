using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text;
using System.Web.UI;
using System.Web.UI.HtmlControls;

using Core;
using Loader;
using Log;

namespace Presenter
{
    public interface IPresenter
    {
        void SetModel(Model model);
        void SetAnswer(Answer answer);

        Model GetModel();
        Answer GetAnswer();

        String GetHtml(DataTable dataTable);
    }

    public class Presenter : DataTable, IPresenter
    {
        protected Model mod;
        protected Answer an;

        public String GetHtml(DataTable dataTable)
        {
            StringBuilder sbControlHtml = new StringBuilder();

            using (StringWriter stringWriter = new StringWriter())
            {
                using (HtmlTextWriter htmlWriter = new HtmlTextWriter(stringWriter))
                {
                    using (var htmlTable = new HtmlTable())
                    {
                        htmlTable.Border = 3;
                        using (var headerRow = new HtmlTableRow())
                        {
                            foreach (DataColumn dataColumn in dataTable.Columns)
                            {
                                using (var htmlColumn = new HtmlTableCell())
                                {
                                    htmlColumn.InnerText = dataColumn.ColumnName;
                                    headerRow.Cells.Add(htmlColumn);
                                }
                            }
                            htmlTable.Rows.Add(headerRow);
                        }

                        // Add data rows  
                        foreach (DataRow row in dataTable.Rows)
                        {
                            using (var htmlRow = new HtmlTableRow())
                            {
                                foreach (DataColumn column in dataTable.Columns)
                                {
                                    using (var htmlColumn = new HtmlTableCell())
                                    {
                                        htmlColumn.InnerText = row[column].ToString();
                                        htmlRow.Cells.Add(htmlColumn);
                                    }
                                }
                                htmlTable.Rows.Add(htmlRow);
                            }
                        }
                        htmlTable.RenderControl(htmlWriter);
                        sbControlHtml.Append(stringWriter.ToString());
                    }
                }
            }
            return sbControlHtml.ToString();
        }

        public void SetAnswer(Answer answer)
        {
            an = answer;
        }

        public void SetModel(Model model)
        {
            mod = model;
        }

        public Model GetModel()
        {
            return mod;
        }

        public Answer GetAnswer()
        {
            return an;
        }

        public void ShowAnswer(Model mod, Answer ans, Loader.sсhedule model, string logName)
        {
            LogGlobal.Join(Directory.GetCurrentDirectory() + "\\" + logName);
            LogGlobal.msg("");
            LogGlobal.msg("------------------------------------");
            LogGlobal.msg(0, DateTime.Now.TimeOfDay + " Solver finished. Presenter start");

            this.SetAnswer(ans);
            this.SetModel(mod);

            var dataTable = new DataTable();

            int columnsNum = ans.schedule.tours; // Число туров
            // Столбцы по числу туров            
            for (int i = 0; i < columnsNum; i++)
            {
                dataTable.Columns.Add(i.ToString());
            }

            int rowsNum = ans.schedule.games; // Число матчей в туре
            // Строки по числу матчей            
            for (int j = 0; j < rowsNum; j++)
            {
                var row = dataTable.NewRow();

                //Соперник 1
                for (int i = 0; i < columnsNum; i += 4)
                {
                    if (ans[i / 4, j].teams != null)
                        row[i.ToString()] = ans[i / 4, j].teams[0];
                    else
                        row[i.ToString()] = "";
                }

                //Соперник 2
                for (int i = 1; i < columnsNum; i += 4)
                {
                    if (ans[i / 4, j].teams != null)
                        row[i.ToString()] = ans[i / 4, j].teams[1];
                    else
                        row[i.ToString()] = "";
                }

                //Дата матча
                for (int i = 2; i < columnsNum; i += 4)
                {
                    if (ans[(i - 2) / 4, j].DateTime.HasValue)
                        row[i.ToString()] = ((DateTime)ans[(i - 2) / 4, j].DateTime).ToShortDateString().ToString();
                    else
                        row[i.ToString()] = "";
                }

                //Время матча
                for (int i = 3; i < columnsNum; i += 4)
                {
                    if (ans[(i - 3) / 4, j].DateTime.HasValue)
                        row[i.ToString()] = model.stadium.time[(int)ans.schedule.z[(i - 3) / 4, j]];
                    else
                        row[i.ToString()] = "";
                }

                dataTable.Rows.Add(row);
            }

            dataTable.AcceptChanges();
            var html = this.GetHtml(dataTable);

            using (FileStream fstream = new FileStream(Directory.GetCurrentDirectory() + "\\answer.html", FileMode.OpenOrCreate))
            {
                byte[] input = Encoding.Default.GetBytes(html);

                fstream.Write(input, 0, input.Length);
                LogGlobal.msg(0, DateTime.Now.TimeOfDay + " Presenter finished");
            }
        }

        public void ShowCrit(List<int[]> l)
        {
            var dataTable = new DataTable();

            for (int i = 0; i < l[i].Length; i++)
            {
                dataTable.Columns.Add(i.ToString());
            }
            for (int j = 0; j < l.Count; j++)
            {
                var row = dataTable.NewRow();


                for (int i = 0; i < l[i].Length; i += 1)
                {

                    row[i.ToString()] = l[i][j].ToString();
                }



                dataTable.Rows.Add(row);
            }
            dataTable.AcceptChanges();
            var html = this.GetHtml(dataTable);

            using (FileStream fstream = new FileStream(Directory.GetCurrentDirectory() + "\\Crit.html", FileMode.OpenOrCreate))
            {
                byte[] input = Encoding.Default.GetBytes(html);

                fstream.Write(input, 0, input.Length);

            }

        }
    }
}
