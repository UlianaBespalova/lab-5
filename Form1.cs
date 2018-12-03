using System;
using System.IO;
using System.Diagnostics;
using System.Collections.Generic;
//using EditDistanceProject;
using System.Windows.Forms;
using Liven;
using System.Threading.Tasks;
using System.Text;


namespace лаба4
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent(); //просто не будем это трогать.
        }

        /// <summary>
        /// Список слов
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        List<string> list = new List<string>();//создали пустой лист

        private void Form1_Load(object sender, EventArgs e)
        {

        }


        private void button1_Click(object sender, EventArgs e)
        {

            OpenFileDialog fileD = new OpenFileDialog();
            fileD.Filter = "текстовые файлы|*.txt"; //ограничиваемся текстовыми файлами

            if (fileD.ShowDialog() == DialogResult.OK)
            {
                Stopwatch t = new Stopwatch(); //штучка для измерения времени
                t.Start(); //начали отсчет

                string text = File.ReadAllText(fileD.FileName); //чтение файла в виде строки

                string[] textArray = text.Split(new char[] { ' ', '.', ',', '!', '?', '-', ';', ':', '/', '\t', '\n' }, StringSplitOptions.RemoveEmptyEntries); //разделяем строку на слова


                foreach (string i in textArray)
                {
                    string str = i.Trim(); //поудаляли пробелы перед и после слова
                    if (!list.Contains(str)) list.Add(str); //если в списке такого слова нет, добавляем его
                }

                t.Stop(); //остановили таймер

                this.label4.Text = (t.ElapsedTicks * 0.0001).ToString(); //посчитали время
                this.label1.Text = list.Count.ToString();  //посчитали количество записанных слов

            }

            else
            {
                MessageBox.Show("Нужно выбрать файл");
            }

        }




        private void buttonSeach_Click(object sender, EventArgs e)
        {
            string word = this.textBoxFind.Text.Trim(); //введенное слово для поиска

            if (!string.IsNullOrWhiteSpace(word) && list.Count > 0) //если слово не пустое
            {
                int kol;
                if (!int.TryParse(this.TextBoxKol.Text.Trim(), out kol))
                {
                    MessageBox.Show("Необходимо указать максимальное расстояние");
                    return;
                }

                if (kol < 1 || kol > 5)
                {
                    MessageBox.Show("Масксимальное расстояние должно быть больше 1 и меньше 5");
                    return;
                }

                int ThreadKol;
                if (!int.TryParse(this.textBoxPotok.Text.Trim(), out ThreadKol))
                {
                    MessageBox.Show("Необходимо указать количество потоков");
                    return;
                }



                Stopwatch t = new Stopwatch();
                t.Start(); //начали отсчет


                //----------параллельный поиск-----------

                List<ParallelSearchResult> Result = new List<ParallelSearchResult>();

                List<MinMax> arrayDilList = SubArrays.DivideSubArrays(0, list.Count, ThreadKol);//создали массив Минмаксов, первый элемент 0, последний = числу эл
                int count = arrayDilList.Count;//количество потоков=количество элементов в массиве

                Task<List<ParallelSearchResult>>[] tasks = new Task<List<ParallelSearchResult>>[count]; //массив классов с результатами (данными о каждом подмассиве)

                for (int i = 0; i < count; i++) //запускаем потоки
                {
                    List<string> tempTaskList = list.GetRange(arrayDilList[i].Min, arrayDilList[i].Max - arrayDilList[i].Min);//копируем значения из нужного диапазона
                    tasks[i] = new Task<List<ParallelSearchResult>>(
                        ThreadTaskk.ArrayThreadTask,
                        new ParallelSearchThreadParam()
                        {
                            tempList = tempTaskList,
                            maxDist = kol,
                            ThreadNumber = i,
                            wordPattern = word
                        });//разобрались с параметрами потока

                    tasks[i].Start();//запускаем поток
                }

                Task.WaitAll(tasks); //завершаем работу, только когда закончилась работа со всеми потоками
                t.Stop();

                for (int i = 0; i < count; i++)//объединение результатов
                {
                    Result.AddRange(tasks[i].Result);
                }

                //-----------------------

                this.labelTime2.Text = (t.ElapsedTicks * 0.0001).ToString(); //время поиска

                this.textBoxPotok.Text = count.ToString();

                this.listBoxResults.Items.Clear(); //очистили список (чтоб не показывал предыдущие результаты)
                this.listBoxResults.BeginUpdate();

                foreach (var i in Result)
                {
                    string ivan = i.word + " (расстояние = " + i.distance.ToString() + " поток = " + (i.ThreadNumber + 1).ToString() + ")";
                    this.listBoxResults.Items.Add(ivan);
                }

                this.listBoxResults.EndUpdate();

            }

            else
            {
                MessageBox.Show("Нужно ввести слово для поиска");
            }


        }

        private void label5_Click(object sender, EventArgs e)
        {

        }

        private void buttonReport_Click(object sender, EventArgs e)//вывод отчета
        {
            string TempReportFileName = "Report_" + DateTime.Now.ToString("dd_MM_yyyy_hhmmss"); //имя

            SaveFileDialog fd = new SaveFileDialog();
            fd.FileName = TempReportFileName;

            fd.DefaultExt = ".html";
            fd.Filter = "HTML Reports|*.html";

            if (fd.ShowDialog() == DialogResult.OK)
            {
                string ReportFileName = fd.FileName;
                StringBuilder b = new StringBuilder(); //ТЁМНАЯ МАГИЯ ЗАПИСИ ТАБЛИЦЫ НЕ ДАЙ БОГ КОМУ РАЗБИРАТЬСЯ
                b.AppendLine("<html>");
                b.AppendLine("<head>");
                b.AppendLine("<meta http-equiv='Content-Type' content='text/html; charset = UTF - 8'/>");
                b.AppendLine("<title>" + "Отчёт: " + ReportFileName + "</title>");
                b.AppendLine("</head>");
                b.AppendLine("<body>");
                b.AppendLine("<h1>" + "Отчёт: " + ReportFileName + "</h1>");
                b.AppendLine("<table border='1'>");
                b.AppendLine("<tr>");
                b.AppendLine("<td>Время чтения из файла</td>");
                b.AppendLine("<td>" + this.label4.Text + "</td>");
                b.AppendLine("</tr>");
                b.AppendLine("<tr>");
                b.AppendLine("<td>Количество уникальных слов в файле</td>");
                b.AppendLine("<td>" + this.label1.Text + "</td>");
                b.AppendLine("</tr>");
                b.AppendLine("<tr>");
                b.AppendLine("<td>Слово для поиска</td>");
                b.AppendLine("<td>" + this.textBoxFind.Text + "</td>");
                b.AppendLine("</tr>");
                b.AppendLine("<tr>");
                b.AppendLine("<td>Максимальное расстояние для нечеткого поиск а</td>");
                b.AppendLine("<td>" + this.TextBoxKol.Text + "</td>");
                b.AppendLine("</tr>");
                b.AppendLine("<tr>");
                b.AppendLine("<td>Время нечеткого поиска</td>");
                b.AppendLine("<td>" + this.label4.Text + "</td>");
                b.AppendLine("</tr>");
                b.AppendLine("<tr valign='top'>");
                b.AppendLine("<td>Результаты поиска</td>");
                b.AppendLine("<td>");
                b.AppendLine("<ul>");

                foreach (var x in this.listBoxResults.Items)
                {
                    b.AppendLine("<li>" + x.ToString() + "</li>");
                }

                b.AppendLine("</ul>");
                b.AppendLine("</td>");
                b.AppendLine("</tr>");
                b.AppendLine("</table>");
                b.AppendLine("</body>");
                b.AppendLine("</html>");

                File.AppendAllText(ReportFileName, b.ToString());
                MessageBox.Show("Отчет сформирован. Файл: " + ReportFileName);

            }
        }
    }
}
