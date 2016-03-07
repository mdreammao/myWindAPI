using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;
using WAPIWrapperCSharp;
using WindCommon;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;

namespace myWindAPI
{
    /// <summary>
    /// 获取交易日期信息的类。
    /// </summary>
    class TradeDays
    {
        private string connectString = Configuration.connectString;
        private string dataBaseName = Configuration.dataBaseName;
        private string tradeDaysTableName = Configuration.tradeDaysTableName;

        /// <summary>
        /// 存储历史的交易日信息。
        /// </summary>
        private static List<int> tradeDaysOfDataBase=new List<int>();


        /// <summary>
        /// 存储所有回测时期内的交易日信息。
        /// </summary>
        public List<int> myTradeDays { get; set; }


        /// <summary>
        /// 存储每日每个tick对应的时刻。
        /// </summary>
        public static int[] myTradeTicks { get; set; }


        /// <summary>
        /// 构造函数。从万德数据库中读取日期数据，并保持到本地数据库。
        /// </summary>
        /// <param name="startDate">交易日开始时间</param>
        /// <param name="endDate">交易日结束时间</param>
        public TradeDays(int startDate,int endDate=0)
        {
            //对给定的参数做一些勘误和修正。
            if (endDate == 0)
            {
                endDate = Convert.ToInt32(DateTime.Now.ToString("yyyyMMdd"));
            }
            if (endDate < startDate)
            {
                Console.WriteLine("Wrong trade Date!");
                startDate = endDate;
            }
            //从本地数据库中读取交易日信息。
            GetDataFromDataBase();
            //从万德数据库中读取交易日信息。但仅在数据库没有构造的时候进行读取。并保持到本地数据库。
            if (tradeDaysOfDataBase.Count == 0 || tradeDaysOfDataBase[tradeDaysOfDataBase.Count - 1] < 20161230)
            {
                GetDataFromWindDataBase();
                SaveTradeDaysData();
            }
            //根据给定的回测开始日期和结束日期，给出交易日列表。
            myTradeDays = new List<int>();
           
            foreach (int date in tradeDaysOfDataBase)
            {
                if (date>=startDate && date<=endDate)
                {
                    myTradeDays.Add(date);
                }
            }
            //生成每个tick对应的数组下标，便于后期的计算。
            if (myTradeTicks == null)
            {
                myTradeTicks = new int[28800];
            }
            for (int timeIndex = 0; timeIndex < 28800; timeIndex++)
            {
                myTradeTicks[timeIndex] = IndexToTime(timeIndex);
            }
        }


        /// <summary>
        /// 从本地数据库中读取交易日信息的函数。
        /// </summary>
        /// <returns></returns>
        private bool GetDataFromDataBase()
        {
            bool exist = false;
            int theLastDay = 0;
            if (tradeDaysOfDataBase.Count>0)
            {
                theLastDay = tradeDaysOfDataBase[tradeDaysOfDataBase.Count - 1];
            }
            //从数据库的表myTradeDays中读取交易日信息
            using (SqlConnection conn = new SqlConnection(connectString))
            {
                conn.Open();//打开数据库  
                            //  Console.WriteLine("数据库打开成功!");
                            //创建数据库命令  
                SqlCommand cmd = conn.CreateCommand();
                //创建查询语句  
                cmd.CommandText = "select [Date] from ["+dataBaseName+"].[dbo].["+tradeDaysTableName+"] order by[Date]";
                try
                {
                    SqlDataReader reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        int today = reader.GetInt32(reader.GetOrdinal("Date"));
                        if (today>theLastDay)
                        {
                            tradeDaysOfDataBase.Add(today);
                        }
                    }
                    reader.Close();
                }
                catch (Exception myError)
                {
                    System.Console.WriteLine(myError.Message);
                }
                finally
                {
                    conn.Close();
                    conn.Dispose();
                }
            }
            if (tradeDaysOfDataBase.Count>0)
            {
                exist = true;
            }
            return exist;
        }

        /// <summary>
        /// 从万德数据库中读取交易日信息数据。
        /// </summary>
        private void GetDataFromWindDataBase()
        {
            int theLastDay = 0;
            if (tradeDaysOfDataBase.Count > 0)
            {
                theLastDay = tradeDaysOfDataBase[tradeDaysOfDataBase.Count - 1];
            }
            //万德API接口的类。
            WindAPI w = new WindAPI();
            w.start();
            //从万德数据库中抓取交易日信息。
            WindData days = w.tdays("20100101", "20161231", "");
            //将万德中读取的数据转化成object数组的形式。
            object[] dayData = days.data as object[];
            foreach (object item in dayData)
            {
                DateTime today = (DateTime)item;
                int now = DateTimeToDays(today);
                if (now>theLastDay)
                {
                    tradeDaysOfDataBase.Add(now);
                }
            }
            w.stop();
        }

        /// <summary>
        /// 将交易日信息存储到本地数据库中。
        /// </summary>
        /// <returns>返回存储是否成功</returns>
        private bool SaveTradeDaysData()
        {
            bool success = false;
            using (SqlConnection conn = new SqlConnection(connectString))
            {
                conn.Open();//打开数据库  
                SqlCommand cmd = conn.CreateCommand();
                cmd.CommandText = "select max([Date]) as [Date] from [" + dataBaseName + "].[dbo].[" + tradeDaysTableName + "]";
                //判断数据表是否存在。
                bool exist = false;
                int theLastDate = 0;
                try
                {
                    theLastDate = (int)cmd.ExecuteScalar();
                    exist = true;
                }
                catch (Exception myerror)
                {
                    System.Console.WriteLine(myerror.Message);
                }
                //若数据表不存在就创建新表。
                if (exist == false)
                {
                    System.Console.WriteLine("Creating new database of tradeDays");
                    cmd.CommandText = "create table [" + dataBaseName + "].[dbo].[" + tradeDaysTableName + "] ([Date] int not null,primary key ([Date]))";
                    try
                    {
                        cmd.ExecuteReader();
                    }
                    catch (Exception myerror)
                    {
                        System.Console.WriteLine(myerror.Message);
                    }
                }
                //如果表中的最大日期小于tradeDaysOfDataBase中的最大日期就更新本表。
                if (tradeDaysOfDataBase.Count>0 && theLastDate < tradeDaysOfDataBase[tradeDaysOfDataBase.Count-1])
                {
                    //利用DateTable格式存入数据。
                    DataTable myDataTable = new DataTable();
                    myDataTable.Columns.Add("Date", typeof(int));
                    foreach (int today in tradeDaysOfDataBase)
                    {
                        if (today>theLastDate)
                        {
                            DataRow r = myDataTable.NewRow();
                            r["Date"] = today;
                            myDataTable.Rows.Add(r);
                        }
                    }
                    //利用sqlbulkcopy写入数据
                    using (SqlBulkCopy bulk = new SqlBulkCopy(connectString))
                    {
                        try
                        {
                            bulk.DestinationTableName = tradeDaysTableName;
                            bulk.ColumnMappings.Add("Date", "Date");
                            bulk.WriteToServer(myDataTable);
                            success = true;
                        }
                        catch (Exception myerror)
                        {
                            System.Console.WriteLine(myerror.Message);
                        }
                    }
                }
                conn.Close();
            }
            return success;
        }


        /// <summary>
        /// 将DateTime格式的日期转化成为int类型的日期。
        /// </summary>
        /// <param name="time">DateTime类型的日期</param>
        /// <returns>Int类型的日期</returns>
        public static int DateTimeToDays(DateTime time)
        {
            return time.Year * 10000 + time.Month * 100 + time.Day;
        }


        /// <summary>
        /// 静态函数。将数组下标转化为具体时刻。
        /// </summary>
        /// <param name="Index">下标</param>
        /// <returns>时刻</returns>
        public static int IndexToTime(int index)
        {
            int time0 = index * 500;
            int hour = time0 / 3600000;
            time0 = time0 % 3600000;
            int minute = time0 / 60000;
            time0 = time0 % 60000;
            int second = time0;
            if (hour < 2)
            {
                hour += 9;
                minute += 30;
                if (minute >= 60)
                {
                    minute -= 60;
                    hour += 1;
                }
            }
            else
            {
                hour += 11;
            }
            return hour * 10000000 + minute * 100000 + second;
        }


        /// <summary>
        /// 静态函数。给出下一交易日。
        /// </summary>
        /// <param name="today">当前交易日</param>
        /// <returns>下一交易日</returns>
        public static int GetNextTradeDay(int today)
        {
            int nextIndex = tradeDaysOfDataBase.FindIndex(delegate (int i) { return i == today; }) + 1;
            if (nextIndex >= tradeDaysOfDataBase.Count)
            {
                return 0;
            }
            else
            {
                return tradeDaysOfDataBase[nextIndex];
            }
        }


        /// <summary>
        /// 静态函数。给出前一交易日。
        /// </summary>
        /// <param name="today">当前交易日</param>
        /// <returns>返回前一交易日</returns>
        public static int GetPreviousTradeDay(int today)
        {
            int preIndex = tradeDaysOfDataBase.FindIndex(delegate (int i) { return i == today; }) - 1;
            if (preIndex < 0)
            {
                return 0;
            }
            else
            {
                return tradeDaysOfDataBase[preIndex];
            }
        }


        /// <summary>
        /// 静态函数。获取交易日间隔天数。
        /// </summary>
        /// <param name="firstday">开始日期</param>
        /// <param name="lastday">结束日期</param>
        /// <returns>间隔天数</returns>
        public static int GetTimeSpan(int firstday, int lastday)
        {
            if (firstday >= tradeDaysOfDataBase[0] && lastday <= tradeDaysOfDataBase[tradeDaysOfDataBase.Count - 1] && lastday >= firstday)
            {
                int startIndex = -1, endIndex = -1;
                for (int i = 0; i < tradeDaysOfDataBase.Count; i++)
                {
                    if (tradeDaysOfDataBase[i] == firstday)
                    {
                        startIndex = i;
                    }
                    if (tradeDaysOfDataBase[i] > firstday && tradeDaysOfDataBase[i - 1] < firstday)
                    {
                        startIndex = i;
                    }
                    if (tradeDaysOfDataBase[i] == lastday)
                    {
                        endIndex = i;
                    }
                    if (tradeDaysOfDataBase[i] > lastday && tradeDaysOfDataBase[i - 1] < lastday)
                    {
                        endIndex = i - 1;
                    }
                }
                if (startIndex != -1 && endIndex != -1)
                {
                    return endIndex - startIndex + 1;
                }
                else
                {
                    return 0;
                }
            }
            else
            {
                return 0;
            }
        }

        /// <summary>
        /// 判断今日是否是期权行权日。每月第四个星期三。
        /// </summary>
        /// <param name="day">日期</param>
        /// <returns>是否是行权日</returns>
        public static bool isExpiryDate (int day)
        {
            DateTimeFormatInfo format = new DateTimeFormatInfo();
            string dayString = DateTime.ParseExact(day.ToString(), "yyyyMMdd", null).ToString();
            DateTime today = Convert.ToDateTime(dayString);
            if (today.AddDays(-21).Month==today.Month && today.AddDays(-28).Month != today.Month && today.DayOfWeek.ToString()=="Wednesday")
            {
                return true;
            }
            return false;
        }
    }
}
