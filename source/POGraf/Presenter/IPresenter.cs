using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Windows.Forms;
using Excel = Microsoft.Office.Interop.Excel;


using Core;
using Loader;
using Log;
using System.Drawing;

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
            Core.Schedule Model = new Core.Schedule(ans.schedule);
            int columnsNum = ans.schedule.tours; // Число туров
            // Столбцы по числу туров            
            for (int i = 0; i < columnsNum; i++)
            {
                dataTable.Columns.Add(i.ToString());
            }
            DataGridView grid = new DataGridView();
            int rowsNum = ans.schedule.games; // Число матчей в туре
            // Строки по числу матчей            
            for (int j = 0; j < rowsNum; j++)
            {
                int k = 0;
                var row = dataTable.NewRow();

                //Соперник 1
                for (int i = 0; i < columnsNum; i += 4)
                {

                    if (mod.wishes[k].IsSuitable((int)ans.schedule.y[i, j], (int)ans.schedule.z[i, j], (int)ans.schedule.x[i, j, k], Model.games, Model, mod) && (k < mod.wishes.Count))
                    {
                        if (ans[i / 4, j].teams != null)
                            row[i.ToString()] = ans[i / 4, j].teams[0];
                        else
                            row[i.ToString()] = "";

                        k++;

                        grid.Rows[i].DefaultCellStyle.BackColor = Color.Green;
                    }
                    else
                    {

                        if (ans[i / 4, j].teams != null)
                            row[i.ToString()] = ans[i / 4, j].teams[0];
                        else
                            row[i.ToString()] = "";


                        k++;
                    }
                }
                //Соперник 2
                for (int i = 1; i < columnsNum; i += 4)
                {

                    if (mod.wishes[k].IsSuitable((int)ans.schedule.y[i, j], (int)ans.schedule.z[i, j], (int)ans.schedule.x[i, j, k], Model.games, Model, mod) && (k < mod.wishes.Count))
                    {
                        if (ans[i / 4, j].teams != null)
                            row[i.ToString()] = ans[i / 4, j].teams[1];
                        else
                            row[i.ToString()] = "";
                        grid.Rows[i].DefaultCellStyle.BackColor = Color.Green;
                        k++;
                    }
                    else
                    {
                        if (ans[i / 4, j].teams != null)
                            row[i.ToString()] = ans[i / 4, j].teams[1];
                        else
                            row[i.ToString()] = "";
                        k++;
                    }
                }


                //Дата матча
                for (int i = 2; i < columnsNum; i += 4)
                {
                    if (mod.wishes[k].IsSuitable((int)ans.schedule.y[i, j], (int)ans.schedule.z[i, j], (int)ans.schedule.x[i, j, k], Model.games, Model, mod) && (k < mod.wishes.Count))
                    {
                        if (ans[(i - 2) / 4, j].DateTime.HasValue)
                            row[i.ToString()] = ((DateTime)ans[(i - 2) / 4, j].DateTime).ToShortDateString().ToString();
                        else
                            row[i.ToString()] = "";

                        grid.Rows[i].DefaultCellStyle.BackColor = Color.Green;
                        k++;
                    }
                    else
                    {
                        if (ans[(i - 2) / 4, j].DateTime.HasValue)
                            row[i.ToString()] = ((DateTime)ans[(i - 2) / 4, j].DateTime).ToShortDateString().ToString();
                        else
                            row[i.ToString()] = "";
                        k++;
                    }
                    dataTable.Rows.Add(row);
                }

                //Время матча
                for (int i = 3; i < columnsNum; i += 4)
                {
                    

                    if (mod.wishes[k].IsSuitable((int)ans.schedule.y[i, j], (int)ans.schedule.z[i, j], (int)ans.schedule.x[i, j, k], Model.games, Model, mod) && (k < mod.wishes.Count))
                    {
                        if (ans[(i - 3) / 4, j].DateTime.HasValue)
                            row[i.ToString()] = model.stadium.time[(int)ans.schedule.z[(i - 3) / 4, j]];
                        else
                            row[i.ToString()] = "";
                        grid.Rows[i].DefaultCellStyle.BackColor = Color.Green;
                        k++;
                    }
                    else
                    {
                        if (ans[(i - 3) / 4, j].DateTime.HasValue)
                            row[i.ToString()] = model.stadium.time[(int)ans.schedule.z[(i - 3) / 4, j]];
                        else
                            row[i.ToString()] = "";
                        k++;
                    }


                }
                dataTable.AcceptChanges();
            }
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

        public void ShowAnswerEx(Model mod, Answer ans, Loader.sсhedule model, string logName)
        {
            LogGlobal.Join(Directory.GetCurrentDirectory() + "\\" + logName);
            LogGlobal.msg("");
            LogGlobal.msg("------------------------------------");
            LogGlobal.msg(0, DateTime.Now.TimeOfDay + " Solver finished. Presenter start");

            this.SetAnswer(ans);
            this.SetModel(mod);
           
            var dataTable = new DataTable();
            Core.Schedule Model = new Core.Schedule(ans.schedule);
            int columnsNum = ans.schedule.tours; // Число туров

            int rowsNum = ans.schedule.games; // Число матчей в туре


            Excel.Application ObjExcel = new Excel.Application();
            Excel.Workbook ObjWorkBook;
            Excel.Worksheet ObjWorkSheet;
            ObjWorkBook = ObjExcel.Workbooks.Add(System.Reflection.Missing.Value);
            ObjWorkBook = ObjExcel.Workbooks.Add(AppDomain.CurrentDomain.BaseDirectory);
            ObjWorkSheet = (Excel.Worksheet)ObjWorkBook.Sheets[1];



            for (int j = 0; j < rowsNum; j++)
            {
                var row = dataTable.NewRow();

                //Соперник 1
                int k = 0;
                for (int i = 0; i < columnsNum; i += 4)
                {

                    if (mod.wishes[k].IsSuitable((int)ans.schedule.y[i, j], (int)ans.schedule.z[i, j], (int)ans.schedule.x[i, j, k], Model.games, Model, mod) && (k < mod.wishes.Count))
                    {
                        if (ans[i / 4, j].teams != null)
                        {
                            ObjWorkSheet.Cells[j, i.ToString()] = ans[i / 4, j].teams[0];
                            ObjWorkSheet.get_Range(j, i).Font.Color = Excel.XlRgbColor.rgbGreen;
                        }
                        else
                            ObjWorkSheet.Cells[j, i.ToString()] = "";

                        k++;


                    }
                    else
                    {

                        if (ans[i / 4, j].teams != null)
                            ObjWorkSheet.Cells[j, i.ToString()] = ans[i / 4, j].teams[0];
                        else
                            ObjWorkSheet.Cells[j, i.ToString()] = "";


                        k++;
                    }
                }
                //Соперник 2
                for (int i = 1; i < columnsNum; i += 4)
                {

                    if (mod.wishes[k].IsSuitable((int)ans.schedule.y[i, j], (int)ans.schedule.z[i, j], (int)ans.schedule.x[i, j, k], Model.games, Model, mod)&&(k<mod.wishes.Count))
                    {
                        if (ans[i / 4, j].teams != null)
                        {
                            ObjWorkSheet.Cells[j, i.ToString()] = ans[i / 4, j].teams[1];
                            ObjWorkSheet.get_Range(j, i).Font.Color = Excel.XlRgbColor.rgbGreen;
                        }
                        else
                            ObjWorkSheet.Cells[j, i.ToString()] = "";
                        k++;
                    }
                    else
                    {

                        if (ans[i / 4, j].teams != null)
                        {
                            ObjWorkSheet.Cells[j, i.ToString()] = ans[i / 4, j].teams[1];
                        }
                        else

                            ObjWorkSheet.Cells[j, i.ToString()] = "";
                        k++;
                    }
                }


                //Дата матча
                for (int i = 2; i < columnsNum; i += 4)
                {
                    if (mod.wishes[k].IsSuitable((int)ans.schedule.y[i, j], (int)ans.schedule.z[i, j], (int)ans.schedule.x[i, j, k], Model.games, Model, mod) && (k < mod.wishes.Count))
                    {
                        if (ans[(i - 2) / 4, j].DateTime.HasValue)
                        {
                            ObjWorkSheet.Cells[j, i.ToString()] = ((DateTime)ans[(i - 2) / 4, j].DateTime).ToShortDateString().ToString();
                            ObjWorkSheet.get_Range(j, i).Font.Color = Excel.XlRgbColor.rgbGreen;
                        }
                        else
                            ObjWorkSheet.Cells[j, i.ToString()] = "";
                        k++;
                    }
                    else
                    {
                        if (ans[i / 4, j].teams != null)
                        {
                            ObjWorkSheet.Cells[j, i.ToString()] = ans[i / 4, j].teams[1];
                        }
                        else

                            ObjWorkSheet.Cells[j, i.ToString()] = "";
                        k++;
                    }
                }

                //Время матча
                for (int i = 3; i < columnsNum; i += 4)
                {
                    if (mod.wishes[k].IsSuitable((int)ans.schedule.y[i, j], (int)ans.schedule.z[i, j], (int)ans.schedule.x[i, j, k], Model.games, Model, mod) && (k < mod.wishes.Count))
                    {
                        if (ans[(i - 3) / 4, j].DateTime.HasValue)
                        {
                            ObjWorkSheet.Cells[j, i.ToString()] = model.stadium.time[(int)ans.schedule.z[(i - 3) / 4, j]];
                            ObjWorkSheet.get_Range(j, i).Font.Color = Excel.XlRgbColor.rgbGreen;
                        }
                        else
                            ObjWorkSheet.Cells[j, i.ToString()] = "";
                        k++;
                    }
                    else
                    {
                        if (ans[i / 4, j].teams != null)
                        {
                            ObjWorkSheet.Cells[j, i.ToString()] = ans[i / 4, j].teams[1];
                        }
                        else

                            ObjWorkSheet.Cells[j, i.ToString()] = "";
                        k++;
                    }
                }
                //если выводится фигня убрать 2ую индексацию
                ObjExcel.Visible = true;
                ObjExcel.UserControl = true;
            }
        }
        }
    }

