using System;
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
        void SetAnswer(IAnswer answer);

        Model GetModel();
        IAnswer GetAnswer();

        String GetHtml(DataTable dataTable);        
    }

    public class Presenter : DataTable, IPresenter
    {
        protected Model mod;
        protected IAnswer an;

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

        public void SetAnswer(IAnswer answer)
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

        public IAnswer GetAnswer()
        {
            return an;
        }

        public void ShowAnswer(Model mod, IAnswer ans, Loader.sсhedule model, string logName)
        {
            LogGlobal.Join(Directory.GetCurrentDirectory() + "\\" + logName);
            LogGlobal.msg("");
            LogGlobal.msg("------------------------------------");
            LogGlobal.msg(0, DateTime.Now.TimeOfDay + " Solver finished. Presenter start");

            this.SetAnswer(ans);
            this.SetModel(mod);            

            var dataTable = new DataTable();

            int columnsNum = 4 * (2 * (ans.GetInfo().N - 1)); // Число туров
            // Столбцы по числу туров            
            for (int i = 0; i < columnsNum; i++)
            {
                dataTable.Columns.Add(i.ToString());
            }

            int rowsNum = ans.GetInfo().N / 2; // Число матчей в туре
            // Строки по числу матчей            
            for (int j = 0; j < rowsNum; j++)
            {
                var row = dataTable.NewRow();

                //Соперник 1
                for (int i = 0; i < columnsNum; i += 4)
                {
                    //Название команды по её порядковому номеру в Answer.Tours
                    row[i.ToString()] = model.teams[ans.GetInfo().Tours[i/4,j,0]].name;
                }

                //Соперник 2
                for (int i = 1; i < columnsNum; i += 4)
                {
                    //Название команды по её порядковому номеру в Answer.Tours
                    row[i.ToString()] = model.teams[ans.GetInfo().Tours[i/4,j,1]].name;
                }

                //Дата матча
                for (int i = 2; i < columnsNum; i += 4)
                {
                    //Дата j-го матча в i-ом туре по её порядковому номеру в Answer.Days
                    row[i.ToString()] = mod.gameDates[ans.GetDays()[(i - 2) / 4, j]].ToShortDateString().ToString();
                }

                //Время матча
                for (int i = 3; i < columnsNum; i += 4)
                {
                    //Время j-го матча в i-ом туре по его порядковому номеру в Answer.Hours
                    row[i.ToString()] = model.stadium.time[(ans.GetHours()[(i - 3) / 4, j])];
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
    }
}
