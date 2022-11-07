using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;


namespace Beauty_Galery
{
    public partial class Form1 : Form
    {
        const int nl = 5;//колич. строк 1таб.
        const int nc = 9;//колич. столб. 1таб.
        const int nd = 7;//колич. дней недели.
        const int ntMax = 24;//максимально колич.талонов 
        bool[,,] isBusy = new bool[nl, nd, ntMax]; //массив знач. колонки запись для ячеек рассписания;
        string[,,] pib = new string[nl, nd, ntMax]; //массив имен клиента;
        string[,,] tel = new string[nl, nd, ntMax]; //массив контактного номера;
        int[,] cntBusy = new int[nl, nd]; //колич. занятих талонов.
        int[,] cntFree = new int[nl, nd];//колич. свобод. талонов.
        int[,] cntAll = new int[nl, nd];//всего талонов.
        int[] cntBusyT = new int[nl];//колич. занятих талонов на сегодня;
        int[] cntFreeT = new int[nl];//колич. свобод. талонов.на сегодня;
        int[] cntAllT = new int[nl];//всего талонов.на сегодня;
        bool[,] isAllBusy = new bool[nl, nd];//все талоны заняти
        bool[] isAllBusyT = new bool[nl];// все талоны заняти на сегодня
        bool[,] isDayOff = new bool[nl, nd];//выходной
        String[,] strLog = new String[nl, nd];//журнал событий
        struct talonLogLine//структура которая записывается во 2 файл(talonLog.txt)
        {
            public int indL;//индекс стороки 1 таблици(для виделеной ячейки)
            public int indC;//индекс столбца 1 таблици(для виделеной ячейки) 
            public String dateLog;//день и время записи в журнал событий
            public int action;//резервирование(1)/освобождение(0)талона
            //public int numberTalon;//номер талона
           // public String service;//вид услуги
           // public String masterName;//вид услуги
            public String dateTalon;//дата услуги
            public String timeTalon;//время услуги
            
            public talonLogLine(int iSel, int jSel, String str1, int act,  String str2, String str3)//конструктор структуры
            {
                indL = iSel;//выбраная строка ячейки 1 таб.
                indC = jSel;//выбраный столбец ячейки 1 таб.
                dateLog = str1;
                action = act;
                //numberTalon = nt;
               // service = str2;
               // masterName = str3;
                dateTalon =str2;
                timeTalon = str3;
               
            }
        };
        List<talonLogLine> tll = new List<talonLogLine>();
        String path = "C://work file/isBusy.txt";
        String pathClient = "C://work file/Client.txt";
        String pathLog = "C://work file/talonLog.txt";
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)//начальние действия
                                                           //по заполнению єлементов формы
                                                           //при запуске окна приложения
        {
            InitTbl1();
            label1.Text = "";
            label1.Visible = false;
            dataGridView2.Visible = false;
            InitMasFromFile();
        }
        private void InitTbl1()//инициализация строк таблицы(dataGridView1)
        {
            string[,] tblStr = new string[nl, nc] { { "Манікюр","Коваленко А.", "12:00-20:00", "14:00-20:00",
                    "Вихідний","14:00-20:00","12:00-18:00","12:00-18:00","Вихідний"},
            {"Манікюр/Педікюр","Майстренко В.", "9:00-17:00","14:00-18:00",
                    "12:00-20:00","Вихідний","9:00-17:00","9:00-23:00","Вихідний" },
            {"Брови/Вії","Шевченко К.","12:00-18:00", "Вихідний" ,
                    "12:00-18:00","12:00-18:00","Вихідний","10:00-14:00","10:00-14:00" },
            {"Масаж","Ткаченко Ю.", "10:00-18:00", "10:00-18:00",
                   "10:00-18:00","Вихідний","10:00-18:00","10:00-18:00","Вихідний" },
            { "Масаж","Стиценко Л.", "12:00-20:00", "Вихідний",
                   "12:00-18:00","12:00-18:00","12:00-18:00","Вихідний","10:00-15:00"} };
            for (int i = 0; i < nl; i++)
            {
                if(i < nl - 1)
                dataGridView1.Rows.Add();
                for (int j = 0; j < nc; j++)
                    dataGridView1[j, i].Value = tblStr[i, j];
            }
            dataGridView1.Sort(dataGridView1.Columns[0], ListSortDirection.Ascending);//сортируем по алфавиту
            InitStrLogForCols(0, nd);
            InitDayOff();
        }
        private void InitDayOff()//инициализация массива Выходных дней
        {
            for (int i = 0; i < nl; i++)
            {
                for (int j = 2; j < nc; j++)
                {
                    String str = Convert.ToString(dataGridView1[j, i].Value);
                    if (str.CompareTo("Вихідний") == 0)
                        isDayOff[i, j - 2] = true;
                    else
                        isDayOff[i, j - 2] = false;
                }
            }
        }
        private void InitMasFromFile()//чтение 3 файлов и инициализация из них массивов
        {
            bool isLogExist = InitListFromLogFile();
            try
            {
                StreamReader sr = new StreamReader(path);
                StreamReader sr1 = new StreamReader(pathClient);

                InitMas0();
                for (int i = 0; i < nl; i++)
                {
                    for (int j = 0; j < nd; j++)
                    {
                        String fline = sr.ReadLine();
                        String fline1 = sr1.ReadLine();
                        if (fline.Length > 0 && fline1.Length > 0)
                            InitMasFromFileLine(i, j, fline, fline1);
                        else
                            InitMasForCell(i, j);
                    }
                }
                int dow = DayOfWeekNow(); //сегодня
                int dow0 = Convert.ToInt32(sr.ReadLine());//dow0-день когда предыдущий раз запускали приложение
                if (dow0 == 0)
                    dow0 = nd;
                sr.Close();
                sr1.Close();
                if (dow0 < dow)
                {
                    InitMasForCols(dow0, dow - 1);
                    if (isLogExist)
                        DeleteFromList(dow0, dow - 1);
                }
                else if (dow0 > dow)
                {
                    InitMasForCols(0, dow - 1);
                    InitMasForCols(dow0, nd);
                    if (isLogExist)
                    {
                        DeleteFromList(0, dow - 1);
                        DeleteFromList(dow0, nd);
                    }
                }
                if (tll.Count > 0)
                    initStrLogFromList();
            }
            catch
            {
                InitMasForCols(0, nd);
                if (isLogExist)
                    tll.Clear();

            }
        }
        private bool InitListFromLogFile()//инициализация списка из файла talonLog.txt
        {
            bool retVal;//определяет существование файла talonLog.txt
            try
            {
                StreamReader srl = new StreamReader(pathLog);
                retVal = true;
                while (!srl.EndOfStream)
                {
                    String fLine = srl.ReadLine();
                    char sep = ',';
                    string[] parts = fLine.Split(sep);
                    int il = Convert.ToInt32(parts[0]);
                    int jC = Convert.ToInt32(parts[1]);
                    int act = Convert.ToInt32(parts[3]);
                   // int nt = Convert.ToInt32(parts[4]);
                    talonLogLine tl = new talonLogLine(il, jC,
                        parts[2], act, parts[4], parts[5]);
                    tll.Add(tl);
                }
                srl.Close();
            }
            catch
            {
                retVal = false;
            }
            return retVal;
        }
        private void DeleteFromList(int jB, int jE)//удаление из списка устаревших записей
        {
            for (int i = 0; i < tll.Count; i++)
            {
                if (tll[i].indC >= jB && tll[i].indC <= jE)
                    tll.RemoveAt(i--);
            }
        }
        private void InitMasFromFileLine(int i, int j, string fline, string fline1)//инициализация массивов и строки файла isBusy.txt,Client.txt
        {
            cntAll[i, j] = fline.Length;
            for (int k = 0; k < fline.Length; k++)
            {
                if (fline[k] == '1')
                {
                    isBusy[i, j, k] = true;
                    cntBusy[i, j]++;
                }
                else
                {
                    isBusy[i, j, k] = false;
                    cntFree[i, j]++;
                }
            }
            TestCellForBusy(i, j + 2);
            char[] sep = { ',' };
            string[] parts = fline1.Split(sep);
            int k1 = parts.Length/2;
            int k2 = 0;
            for (int k = 0; k <k1; k++)
            {
                if(parts[k2].Length>0)
                {
                    pib[i, j, k] = parts[k2];
                    tel[i, j, k] = parts[k2 + 1];
                }
                else
                {
                    pib[i, j, k] = "";
                    tel[i, j, k] = "";
                }
                k2 += 2;
               
            }
        }
        private void initStrLogFromList()//формирование массива строк журнала событий из списка
        {
            for (int i = 0; i < tll.Count; i++)
            {
                strLog[tll[i].indL, tll[i].indC] += tll[i].dateLog;
                String busyStr = "Місце зарезервоване";//талон
                String freeStr = "Місце вільне";//талон
                String dayU = Day_U(tll[i].indC + 2);
                String cab = Convert.ToString(dataGridView1[0, tll[i].indL].Value);
                String master = Convert.ToString(dataGridView1[1, tll[i].indL].Value);
                if (tll[i].action == 1)
                    strLog[tll[i].indL, tll[i].indC] += busyStr;
                else
                    strLog[tll[i].indL, tll[i].indC] += freeStr;
                //strLog[tll[i].indL, tll[i].indC] += " талон  N ";
                //strLog[tll[i].indL, tll[i].indC] += Convert.ToString(tll[i].numberTalon);
                strLog[tll[i].indL, tll[i].indC] += " на процедуру";
                strLog[tll[i].indL, tll[i].indC] += cab;
                strLog[tll[i].indL, tll[i].indC] += " до майстра ";
                strLog[tll[i].indL, tll[i].indC] += master;
                strLog[tll[i].indL, tll[i].indC] += " на " + dayU + " ";
                strLog[tll[i].indL, tll[i].indC] += tll[i].dateTalon;
                strLog[tll[i].indL, tll[i].indC] += " на  ";
                strLog[tll[i].indL, tll[i].indC] += tll[i].timeTalon;
                strLog[tll[i].indL, tll[i].indC] += ".\n";
            }
        }
        private void TwoTestCellForBusy(int i, int j)//двойное тестирование ячейки на занятось 
        {
            TestCellForBusy(i, j);
            String tm = Convert.ToString(dataGridView1[j, i].Value);
            if ((DayDifference(j) == 0) && TestInMiddleToday(tm))
                TestCellForBusyT(i, j);
        }
        private bool TestCellForBusy(int i, int j)//проверка ячейки на занятость всех талонов
        {
            bool retVal = false;
            if (cntBusy[i, j - 2] == cntAll[i, j - 2])
            {
                retVal = true;
                ChangeCellColor(i, j, Color.Red);
            }
            else if (isAllBusy[i, j - 2])
                ChangeCellColor(i, j, Color.Black);
            return retVal;
        }
        private bool TestCellForBusyT(int i, int j)//проверка ячейки на занятость всех талонов на сегодня
        {
            bool retVal = false;
            if (cntBusyT[i] == cntAllT[i])
            {
                retVal = true;
                ChangeCellColorT(i, j, Color.Red);
            }
            else if (isAllBusyT[i])
                ChangeCellColorT(i, j, Color.Black);
            return retVal;
        }
        private void ChangeCellColor(int i, int j, Color clr)//изменения цвета текста ячейки в зависимости от занятости талонов 
        {
            if (clr == Color.Red)
                isAllBusy[i, j - 2] = true;
            else
                isAllBusy[i, j - 2] = false;
            dataGridView1[j, i].Style.ForeColor = clr;
            dataGridView1[j, i].Style.SelectionForeColor = clr;
        }
        private void ChangeCellColorT(int i, int j, Color clr)//изменения цвета текста ячейки в зависимости от занятости талонов  на сегодня относительно текущего времени
        {
            if (clr == Color.Red)
                isAllBusyT[i] = true;
            else
                isAllBusyT[i] = false;
            ChangeCellColor(i, j, clr);
        }
        private void InitMas0()//начальная подготовка массивов к чтению файла IsBusy.txt 
        {
            for (int i = 0; i < nl; i++)
            {
                for (int j = 0; j < nd; j++)
                {
                    cntBusy[i, j] = 0;
                    cntFree[i, j] = 0;
                    cntAll[i, j] = 0;
                    isAllBusy[i, j] = false;
                    for ( int k = 0; k< ntMax; k++)
                    {
                        pib[i, j, k] = "";
                        tel[i, j, k] = "";
                    }
                }
            }
        }
        private static int DayOfWeekNow()//определение текущего дня недели
        {
            DateTime dt = DateTime.Now;
            int dw = Convert.ToInt32(dt.DayOfWeek);
            if (dw == 0)//dw-день недели(пон-1,вс-7)
                dw = nd;
            return dw;
        }
        private void InitMasForCols(int jB, int jE)//инициализация массивов для колонок 1 таб.
        {                                          //(jB-индекс начальной колонки;jE-индекс конечной колонки)
            for (int i = 0; i < nl; i++)
            {
                for (int j = jB; j < jE; j++)
                    InitMasForCell(i, j);
            }
        }
        private void InitStrLogForCols(int jB, int jE)//начальная инициализация жур.соб. пустой строкой
        {
            for (int i = 0; i < nl; i++)
            {
                for (int j = jB; j < jE; j++)
                    strLog[i, j] = "";
            }
        }
        private void InitMasForCell(int i, int j)//инициализация массивов для ячейки 1 таб.
        {
            cntBusy[i, j] = -1;//(-1-ячейка не определена)
            cntFree[i, j] = -1;
            cntAll[i, j] = -1;
            isAllBusy[i, j] = false;
            for (int k = 0; k < ntMax; k++)
            {
                isBusy[i, j, k] = false;
                pib[i, j, k] = "";
                tel[i, j, k] = "";
            }
                
        }

        private void dataGridView1_MouseClick(object sender, MouseEventArgs e)
        {

            int i;//индекс строки 2 таб.
            int j;//номер талона.
            int nl2;//количество строк 2 таб.
            DeleteTbl2();
            String tms = "";
            richTextBox1.Text = "";
            bool isFinishToday = false;
            bool isInMiddleToday = false;
            Point curInd = dataGridView1.CurrentCellAddress;
            if (curInd.X > 1 && curInd.Y < nl)
            {
                if (!isDayOff[curInd.Y, curInd.X - 2])
                {
                    if (strLog[curInd.Y, curInd.X - 2].Length > 0)
                        richTextBox1.AppendText(strLog[curInd.Y, curInd.X - 2]);
                    FormLabel1Text(curInd);
                    String tm = Convert.ToString(dataGridView1.CurrentCell.Value);
                    int hb = GetHourBegin(tm);
                    int mb = GetMinuteBegin(tm);
                    int he = GetHourEnd(tm);
                    int me = GetMinuteEnd(tm);
                    int tmmb = GetBeginInMinutes(tm);
                    int tmme = GetEndInMinutes(tm);
                    int curhb = hb;
                    int curmb = mb;
                    int curtmm = tmmb;
                    nl2 = (tmme - tmmb) / 60;
                    if (cntAll[curInd.Y, curInd.X - 2] != nl2)
                    {
                        cntAll[curInd.Y, curInd.X - 2] = nl2;
                        cntBusy[curInd.Y, curInd.X - 2] = 0;
                        cntFree[curInd.Y, curInd.X - 2] = nl2;
                    }
                    i = 0;
                    j = 1;
                    if (DayDifference(curInd.X) == 0)
                    {
                        isFinishToday = TestFinishToday(tm);
                        if (isFinishToday)
                        {
                            curhb = he;
                            curmb = me;
                            curtmm = tmme;
                        }
                        else
                        {
                            isInMiddleToday = TestInMiddleToday(tm);
                            if (isInMiddleToday)
                            {
                                curhb = DateTime.Now.Hour;
                                curmb = DateTime.Now.Minute;
                                ShiftBeginTimeToday(out curhb, out curmb, mb);
                                curtmm = 60 * curhb + curmb;
                                nl2 = (tmme - curtmm) / 60;
                                cntAllT[curInd.Y] = nl2;
                                cntBusyT[curInd.Y] = 0;
                                cntFreeT[curInd.Y] = 0;
                                j = (tmme - tmmb) / 60 - (tmme - curtmm) / 60 + 1;
                            }
                        }
                    }
                    while (curtmm < tmme)
                    {
                        int curhe = curhb;
                        int curme = curmb + 60;
                        if (curme == 60)
                        {
                            curme = 0;
                            curhe += 1;
                        }
                        tms = TalonTime(curhb, curmb, curhe, curme);
                        if (i < nl2 - 1)
                            dataGridView2.Rows.Add();
                        dataGridView2[0, i].Value = j;
                        dataGridView2[1, i].Value = tms;
                        if (isBusy[curInd.Y, curInd.X - 2, j - 1])
                        {
                            dataGridView2[2, i].Value = pib[curInd.Y, curInd.X - 2, j - 1];
                            dataGridView2[3, i].Value = tel[curInd.Y, curInd.X - 2, j - 1]; 
                            dataGridView2[4, i].Value = 1;
                            if (isInMiddleToday)
                                cntBusyT[curInd.Y]++;
                        }
                        else
                        {
                            dataGridView2[2, i].Value = "";
                            dataGridView2[3, i].Value = "";
                            dataGridView2[4, i].Value = 0;
                            if (isInMiddleToday)
                                cntFreeT[curInd.Y]++;
                        }
                        i++;
                        j++;
                        curhb = curhe;
                        curmb = curme;
                        curtmm = 60 * curhb + curmb;
                    }
                    if (!isFinishToday)
                    {
                        dataGridView2.Visible = true;
                        if ((isInMiddleToday && TestCellForBusyT(curInd.Y, curInd.X)) ||
                            TestCellForBusy(curInd.Y, curInd.X))
                            FormMessage(curInd);
                    }
                }
            }
        }
        private static int GetPart(String tm, int ind)
        {
            char[] sep = { '-', ':' };
            string[] parts = tm.Split(sep);
            return (Convert.ToInt32(parts[ind]));
        }
        private static int GetHourBegin(String tm)
        {
            return (GetPart(tm, 0));
        }
        private static int GetMinuteBegin(String tm)
        {
            return (GetPart(tm, 1));
        }
        private static int GetHourEnd(String tm)
        {
            return (GetPart(tm, 2));
        }
        private static int GetMinuteEnd(String tm)
        {
            return (GetPart(tm, 3));
        }
        private static int GetBeginInMinutes(String tm)
        {
            return (60 * GetHourBegin(tm) + GetMinuteBegin(tm));
        }
        private static int GetEndInMinutes(String tm)
        {
            return (60 * GetHourEnd(tm) + GetMinuteEnd(tm));
        }
        private static int GetNowInMinutes()
        {
            int hnow = DateTime.Now.Hour;
            int mnow = DateTime.Now.Minute;
            return (60 * hnow + mnow);
        }
        private static bool TestFinishToday(String tm)
        {
            return (GetNowInMinutes() > GetEndInMinutes(tm) - 15);
        }
        private static bool TestInMiddleToday(String tm)
        {
            return (GetNowInMinutes() > GetBeginInMinutes(tm));
        }
        private void WriteLabel1(String str)
        {
            label1.Text = str;
            label1.Visible = true;
        }
        private void FormLabel1Text(Point curInd)
        {
            bool isFinishToday = false;
            String str = "";
            String tm = Convert.ToString(dataGridView1.CurrentCell.Value);
            int hdt = DayDifference(curInd.X);
            if (hdt == 0)
                isFinishToday = TestFinishToday(tm);
            if (isFinishToday)
                str += " На сьогодні прийом завершено";
            else
            {
                String cab = Convert.ToString(dataGridView1[0, curInd.Y].Value);
                String master= Convert.ToString(dataGridView1[1, curInd.Y].Value);
                String day = Convert.ToString(dataGridView1.Columns[curInd.X].HeaderText);
                DateTime dt1 = DateTime.Today;
                DateTime dt2 = dt1.AddDays(hdt);
                String dt2s = dt2.ToString();
                str += "Список вільних місць на процедуру " + cab + " до майстра "+ master +
                    ".\n" + day + ". " + dt2s.Substring(0, 10) + ". ";
                str += tm;
            }
            WriteLabel1(str);
        }
        private void FormMessage(Point curInd)
        {
            bool isBusyAllWeek = true;
            String str2 = " Вільних місць на ";
            if (DayDifference(curInd.X) == 0)
                str2 += "Сьогодні";
            else
                str2 += Day_U(curInd.X);
            str2 += " Немає ";
            String nextDow = "";
            nextDow = SearchNextFreeDay(curInd.X - 1, nd, curInd.Y);
            if (nextDow.Length != 0)
                isBusyAllWeek = false;
            if (isBusyAllWeek)
            {
                nextDow = SearchNextFreeDay(0, curInd.X - 2, curInd.Y);
                if (nextDow.Length != 0)
                    isBusyAllWeek = false;
            }
            if (isBusyAllWeek)
                str2 = "Вільних місць на найближчий тиждень немає.\n Спробуйте завтра";
            else
            {
                str2 += "Найближче вільне місце на ";
                str2 += nextDow;
            }
            MessageBox.Show(str2);
        }
        private static int DayDifference(int colSel)
        {
            int dwt = DayOfWeekNow();   //day of week today
            int dws = colSel - 1;      //day of week selected
            int hdt = dws - dwt;       //difference of days
            if (hdt < 0)
                hdt += nd;
            return hdt;
        }
        private String SearchNextFreeDay(int colB, int colE, int lineSel)//поиск ближайшего свободного талона
        {
            String nextDow = "";
            for (int wd = colB; wd < colE; wd++)
            {
                if (!isDayOff[lineSel, wd] && !isAllBusy[lineSel, wd])
                {
                    if (wd + 1 != DayOfWeekNow())
                        nextDow = Day_U(wd + 2);
                    break;
                }
            }
            return nextDow;
        }
        private string Day_U(int colSel)
        {
            String dayu = dataGridView1.Columns[colSel].HeaderText;
            if (colSel - 1 == 3 || colSel - 1 == 6)
            {
                dayu = dayu.Substring(0, dayu.Length - 1);
                dayu += "у";
            }
            else if (colSel - 1 == 5 || colSel - 1 == 7)
            {
                dayu = dayu.Substring(0, dayu.Length - 1);
                dayu += "ю";
            }
            return dayu;
        }
        private static void ShiftBeginTimeToday(out int curhb, out int curmb, int mb)
        {
            int hnow = DateTime.Now.Hour;
            int mnow = DateTime.Now.Minute;
            curhb = hnow;
            curmb = mnow;
            if (mnow != mb)
            {
                curmb = mb;
                if (mnow > mb)
                    curhb++;

                
            }
           /* if (mnow > 0 && mnow < 15)
                curmb = 15;
            else if (mnow > 15 && mnow < 30)
                curmb = 30;
            else if (mnow > 30 && mnow < 45)
                curmb = 45;
            else if (mnow > 45)
            {
                curmb = 0;
                curhb++;
            }
           */
        }
        private void DeleteTbl2()
        {
            label1.Text = "";
            label1.Visible = false;
            while (dataGridView2.RowCount > 1)
                dataGridView2.Rows.Remove(dataGridView2.Rows[0]);
            dataGridView2.Visible = false;
        }
        private static string TalonTime(int hb, int mb, int he, int me)// время начала и конца талона
        {
            string tmStr = TimeToString(hb, mb);
            tmStr += "-";
            tmStr += TimeToString(he, me);
            return tmStr;
        }
        private static string TimeToString(int h, int m)
        {
            string tms = "";
            if (h < 10)
                tms += "0";
            tms += Convert.ToString(h) + ":" + Convert.ToString(m);
            if (m == 0)
                tms += "0";
            return tms;
        }
        private void dataGridView2_CellMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            Point curInd2 = dataGridView2.CurrentCellAddress;
            bool isClick = true;
            if (curInd2.X == 2 || curInd2.X == 3)
                Refresh();
            else if (curInd2.X == 4)
            {
                dataGridView2.Visible = false;
                Point curInd1 = dataGridView1.CurrentCellAddress;
                int k = Convert.ToInt32(dataGridView2[0, curInd2.Y].Value) - 1;
                pib[curInd1.Y, curInd1.X - 2, k] = Convert.ToString(dataGridView2[2, curInd2.Y].Value);
                tel[curInd1.Y, curInd1.X - 2, k] = Convert.ToString(dataGridView2[3, curInd2.Y].Value);
                if (Convert.ToInt32(dataGridView2[4, curInd2.Y].Value) == 1)
                {
                    dataGridView2[4, curInd2.Y].Value = 0;
                    pib[curInd1.Y, curInd1.X - 2, k] = "";
                    tel[curInd1.Y, curInd1.X - 2, k] = "";
                    dataGridView2[2, curInd2.Y].Value = "";
                    dataGridView2[3, curInd2.Y].Value = "";
                    isBusy[curInd1.Y, curInd1.X - 2, k] = false;
                    cntFree[curInd1.Y, curInd1.X - 2]++;
                    cntBusy[curInd1.Y, curInd1.X - 2]--;
                    if (DayDifference(curInd1.X) == 0)
                    {
                        cntFreeT[curInd1.Y]++;
                        cntBusyT[curInd1.Y]--;
                    }
                }
                else
                {
                    if (pib[curInd1.Y, curInd1.X-2,k].Length == 0 || tel[curInd1.Y, curInd1.X - 2, k].Length == 0)
                    {
                        dataGridView2.Visible = true;
                        Refresh();
                        isClick = false;
                        MessageBox.Show("Додайте ПІБ та номер телефону");
                    }
                    else
                    {
                        dataGridView2[4, curInd2.Y].Value = 1;
                        isBusy[curInd1.Y, curInd1.X - 2, k] = true;
                        cntFree[curInd1.Y, curInd1.X - 2]--;
                        cntBusy[curInd1.Y, curInd1.X - 2]++;
                        if (DayDifference(curInd1.X) == 0)
                        {
                            cntFreeT[curInd1.Y]--;
                            cntBusyT[curInd1.Y]++;
                        }
                    }
                }
                
                if(isClick)
                {
                    dataGridView2.Visible = true;
                    Refresh();
                    TwoTestCellForBusy(curInd1.Y, curInd1.X);
                    FormRichBoxTextLineOnline(curInd1, curInd2);
                }
                
            }
        }
        private void FormRichBoxTextLineOnline(Point curInd1, Point curInd2)//формирование ж.с.
        {
            int act;
            String strLine = textBox1.Text;
            String busyStr = "Місце зарезервоване ";
            String freeStr = " Місце вільне ";
            String indTalon = Convert.ToString(dataGridView2[0, curInd2.Y].Value);
            //nt = Convert.ToInt32(indTalon);
            String tmTalon = Convert.ToString(dataGridView2[1, curInd2.Y].Value);
            String cab = Convert.ToString(dataGridView1[0, curInd1.Y].Value);
            String master = Convert.ToString(dataGridView1[1, curInd1.Y].Value);
            String dayU = Day_U(curInd1.X);
            DateTime dt1 = DateTime.Today;
            int hdt = DayDifference(curInd1.X);
            DateTime dt2 = dt1.AddDays(hdt);
            String dt2s = dt2.ToString();
            if (Convert.ToInt32(dataGridView2[4, curInd2.Y].Value) == 1)
            {
                strLine += busyStr;
                act = 1;
            }
            else
            {
                strLine += freeStr;
                act = 0;
            }
            //strLine += " талон  N ";
            //strLine += indTalon;
            strLine += "  на процедуру ";
            strLine += cab;
            strLine += " до майстра ";
            strLine += master;
            strLine += " на " + dayU + " " + dt2s.Substring(0, 10);
            strLine += " на час: ";
            strLine += tmTalon;
            strLine += ".\n";
            strLog[curInd1.Y, curInd1.X - 2] += strLine;
            richTextBox1.AppendText(strLine);
            talonLogLine tl = new talonLogLine(curInd1.Y, curInd1.X - 2, textBox1.Text,
              act, dt2s.Substring(0, 10), tmTalon);
            tll.Add(tl);
        }
        private void timer1_Tick(object sender, EventArgs e)
        {
            DateTime dt = DateTime.Now;
            textBox1.Text = dt.ToString();
            int dow = DayOfWeekNow();
            textBox2.Text = dataGridView1.Columns[dow + 1].HeaderText;
        }
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            StreamWriter sw = new StreamWriter(path, false);//запись данних в файли перед закрытием
            StreamWriter sw1 = new StreamWriter(pathClient, false);//запись данних в файли перед закрытием
            for (int i = 0; i < nl; i++)
            {
                for (int j = 0; j < nd; j++)
                {
                    if (cntAll[i, j] > 0)
                    {
                        for (int k = 0; k < cntAll[i, j]; k++)
                        {
                            if (isBusy[i, j, k])
                            {
                                sw.Write(1);
                                sw1.Write(pib[i,j,k]);
                                sw1.Write(',');
                                sw1.Write(tel[i,j,k]);
                                if (k<cntAll[i,j]-1)
                                   sw1.Write(',');
                            }
                                
                            else
                            {
                                sw.Write(0);
                                sw1.Write(',');
                                if (k < cntAll[i, j] - 1)
                                    sw1.Write(',');
                            }
                                
                        }
                    }
                    sw.WriteLine();
                    sw1.WriteLine();
                }
            }
            int dow = DayOfWeekNow();
            sw.WriteLine(dow);
            sw.Close();
            sw1.Close();
            StreamWriter swl = new StreamWriter(pathLog, false);
            for (int i = 0; i < tll.Count; i++)
            {
                swl.WriteLine(Convert.ToString(tll[i].indL) + ',' + Convert.ToString(tll[i].indC) +
                    ',' + tll[i].dateLog + ',' + Convert.ToString(tll[i].action) + ',' +
                    +',' + tll[i].dateTalon + ',' + tll[i].timeTalon);
                    
            }
            swl.Close();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}

