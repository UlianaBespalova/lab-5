using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Liven
{
    public class EditDistance //посчитать расстояние
    {
        public static int Distance(string str1, string str2)
        {
            if ((str1==null)||(str2==null)) return -1; //если обе строки пустые, конец

            int len1 = str1.Length;//длины слов
            int len2 = str2.Length;

            if ((len1 == 0) && (len2 == 0)) return 0; //если есть нулевые строки, все просто
            if (len1 == 0) return len2;
            if (len2 == 0) return len1;

            string strUp1 = str1.ToUpper();
            string strUp2 = str2.ToUpper(); //приводим к верхнему регистру

            int[,] matrix = new int[len1 + 1, len2 + 1];//создали матрицу

            for (int i = 0; i < len1; i++) matrix[i, 0] = i;
            for (int j = 0; j < len2; j++) matrix[0, j] = j; //пронумеровали столбики и строчки


            for (int i=1; i<len1+1; i++)
            {
                for (int j=1; j<len2+1; j++)
                {
                    int sEqual = ((strUp1.Substring(i - 1, 1) == strUp2.Substring(j - 1, 1)) ? 0 : 1);
                    //если соответствующие символы равны, присваиваем 0, иначе 1
                    int oI = matrix[i, j - 1] + 1; //insert
                    int oD = matrix[i - 1, j] + 1; //delete
                    int oR = matrix[i - 1, j - 1] + sEqual; //replace

                    matrix[i, j] = Math.Min(Math.Min(oI, oD), oR); //минимум

                    if ((i>1)&&(j>1)&&  //если элемент не первый (не номер столбика)
                        (strUp1.Substring(i-1,1) == strUp2.Substring(j-2, 1))&&  //если при перестановке символы равны
                        (strUp1.Substring(i - 2, 1) == strUp2.Substring(j - 1, 1)))
                    {
                        matrix[i, j] = Math.Min(matrix[i, j], matrix[i - 2, j - 2] + sEqual);
                    }
                }
            }
            return matrix[len1, len2]; //вернули нижний правый
        }

    }




    public class MinMax //разделение на подмассивы
    {
        public int Min { get; set; }
        public int Max { get; set; }

        public MinMax (int m1, int m2)
        {
            this.Min = m1;
            this.Max = m2;
        }
    }


    public static class SubArrays //деление массива на последовательности
    {
        public static List<MinMax> DivideSubArrays (int Index1, int Index2, int subArrayCount)
        {
            List<MinMax> result = new List<MinMax>(); //список пар и индексами подмассивов
            if ((Index2-Index1)<=subArrayCount) //если число элементов недостаточно для деления,
            {
                result.Add(new MinMax(0, (Index2 - Index1)));//возвращаем целиком
            }

            else
            {
                int delta = (Index2 - Index1) / subArrayCount;//нашли, сколько должно быть в подмассиве элементов
                int currentBegin = Index1; 
                while ((Index2-currentBegin)>=2*delta)//пока размер массива укладывается в оставшуюся последовательность
                {
                    result.Add(new MinMax(currentBegin, currentBegin + delta));//делаем подмассив
                    currentBegin += delta;//сдвигаем точку отсчета
                }
                result.Add(new MinMax(currentBegin, Index2));//оставшаяся часть массива
            }
            return result;
        }
        
    }



    public class ParallelSearchResult //хранение результатов
    {
        public string word { get; set; } //найденное слово
        public int distance { get; set; } //расстояние
        public int ThreadNumber { get; set; } //номер потока
    }


    class ParallelSearchThreadParam //класс с параметрами для поиска
    {
        public List<string> tempList { get; set; } //массив
        public string wordPattern { get; set; } //слово
        public int maxDist { get; set; } //максимальное расстояние
        public int ThreadNumber { get; set; } //номер потока
    }


    public class ThreadTaskk //поиск слов в потоке
    {
        public static List<ParallelSearchResult> ArrayThreadTask (object paramObj)//сам поиск
        {
            ParallelSearchThreadParam param = (ParallelSearchThreadParam)paramObj; //создали объект с указ параметрами

            string wordUp = param.wordPattern.Trim().ToUpper(); //привели к верхнему регистру

            List<ParallelSearchResult> Result = new List<ParallelSearchResult>();//новый массив для хранения результатов

            foreach (string str in param.tempList) //проходимся по словам в подмассиве
            {
                int dist = EditDistance.Distance(str.ToUpper(), wordUp); //посчитали расстояние Ливенштейна
                if (dist <= param.maxDist)//если норм, 
                {
                    ParallelSearchResult temp = new ParallelSearchResult()//записываем результаты
                    {
                        word = str,
                        distance = dist,
                        ThreadNumber = param.ThreadNumber
                    };
                    Result.Add(temp);
                }
                
            }

            return Result; // и выводим результат

        }
    }


}
