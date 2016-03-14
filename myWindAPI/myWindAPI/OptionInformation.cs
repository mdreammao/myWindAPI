using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;
using WAPIWrapperCSharp;
using WindCommon;
using System.Data;

namespace myWindAPI
{
    /// <summary>
    /// 获取期权基本信息的类。按日期从万德上拉取期权基本信息。
    /// </summary>
    class OptionInformation
    {
        private string dataBaseName = Configuration.dataBaseName;
        private string connectString = Configuration.connectString;
        private string optionCodeTableName = Configuration.optionCodeTableName;

        /// <summary>
        /// 记录期权基本信息的静态哈希表。
        /// </summary>
        public static SortedDictionary<int,optionFormat> myOptionList = new SortedDictionary<int, optionFormat>();

        /// <summary>
        /// 构造函数。按日期记录期权合约的基本信息。
        /// </summary>
        /// <param name="startDate">开始日期</param>
        /// <param name="endDate">结束日期</param>
        public OptionInformation(int startDate,int endDate=0)
        {
            //对日期参数做一些勘误
            if (endDate == 0)
            {
                endDate = Convert.ToInt32(DateTime.Now.ToString("yyyyMMdd"));
            }
            if (endDate < startDate)
            {
                Console.WriteLine("Wrong trade Date!");
                startDate = endDate;
            }
            //按日期遍历，添加期权信息。
            GetOptionInformationList(startDate, endDate);
            //根据记录的提取的数据写入数据库。
            SaveOptionInformationList();

        }


        /// <summary>
        /// 查找当日存在合约的函数。
        /// </summary>
        /// <param name="date">日期</param>
        /// <returns>合约代码的数组。</returns>
        public int[] GetOptionNameByDate(int date)
        {
            List<int> nameList = new List<int>();
            foreach (var option in myOptionList)
            {
                if (option.Value.startDate<=date && option.Value.endDate>=date)
                {
                    nameList.Add(option.Key);
                }
            }
            return nameList.ToArray();
        }

        /// <summary>
        /// 从数据库中读取期权信息，并判断是否完整。
        /// </summary>
        /// <param name="startDate">开始时间</param>
        /// <param name="endDate">结束时间</param>
        /// <returns>是否完整</returns>
        private bool GetOptionListFromTable(int startDate,int endDate)
        {
            using (SqlConnection conn = new SqlConnection(connectString))
            {
                conn.Open();//打开数据库  
                SqlCommand cmd = conn.CreateCommand();
                cmd.CommandText = "select * from [" + dataBaseName + "].[dbo].[" + optionCodeTableName + "]";
                int theLastCode = 0;
                int theLastStartDate = 0;
                try
                {
                    //从数据库中读取数据流存入reader中  
                    SqlDataReader reader = cmd.ExecuteReader();
                    //从reader中读取下一行数据,如果没有数据,reader.Read()返回flase  
                    while (reader.Read())
                    {
                        optionFormat newOption = new optionFormat();
                        int optionCode = reader.GetInt32(reader.GetOrdinal("OptionCode"));
                        theLastCode = (optionCode > theLastCode) ? optionCode : theLastCode;
                        newOption.optionCode = optionCode;
                        newOption.optionName = reader.GetString(reader.GetOrdinal("OptionName")).Trim();
                        newOption.executeType = reader.GetString(reader.GetOrdinal("ExecuteType")).Trim();
                        newOption.strike = reader.GetDouble(reader.GetOrdinal("Strike"));
                        newOption.optionType = reader.GetString(reader.GetOrdinal("OptionType")).Trim();
                        newOption.startDate = reader.GetInt32(reader.GetOrdinal("StartDate"));
                        newOption.endDate= reader.GetInt32(reader.GetOrdinal("EndDate"));
                        newOption.market= reader.GetString(reader.GetOrdinal("Market")).Trim();
                        theLastStartDate = (theLastStartDate > newOption.startDate) ? theLastStartDate : newOption.startDate;
                        if (myOptionList.ContainsKey(optionCode)==false)
                        {
                            myOptionList.Add(optionCode, newOption);
                        }
                    }
                }
                catch (Exception myerror)
                {
                    System.Console.WriteLine(myerror.Message);
                }
                if (theLastStartDate>=endDate && theLastCode % 10000 == myOptionList.Count)
                {
                    return true;
                }
            }
            return false;
        }


        /// <summary>
        /// 按日期遍历，添加期权信息。写入静态哈希表myOptionList。
        /// </summary>
        /// <param name="startDate">开始日期</param>
        /// <param name="endDate">结束日期</param>
        private void GetOptionInformationList(int startDate, int endDate)
        {
            //定义交易日期的类。
            TradeDays myTradeDays = new TradeDays(startDate, endDate);
            //从数据库表中获取信息，如果已经是完整信息就不需要连接万德数据库。
            if (GetOptionListFromTable(startDate,endDate)==true)
            {
                return;
            }
            //按日期遍历，添加期权信息。
            WindAPI w = new WindAPI();
            w.start();
            foreach (int today in myTradeDays.myTradeDays)
            {
                if (TradeDays.IsOptionExerciseDate(today) || today == myTradeDays.myTradeDays[myTradeDays.myTradeDays.Count - 1])
                {
                    WindData optionToday = w.wset("OptionChain", "date=" + today.ToString() + ";us_code=" + Configuration.underlyingAsset + ";option_var=;month=全部;call_put=全部");
                    object[] optionList = optionToday.data as object[];
                    int num = optionList.Length / 13;
                    for (int i = 0; i < num; i++)
                    {
                        optionFormat option = new optionFormat();
                        string codeString = (string)optionList[i * 13 + 4 - 1];
                        option.optionCode = Convert.ToInt32(codeString.Substring(0, 8));
                        option.market = codeString.Substring(9, 2);
                        option.optionName = (string)optionList[i * 13 + 5 - 1];
                        option.executeType = (string)optionList[i * 13 + 6 - 1];
                        option.strike = (double)optionList[i * 13 + 7 - 1];
                        option.optionType = (string)optionList[i * 13 + 9 - 1];
                        option.startDate = TradeDays.DateTimeToInt((DateTime)optionList[i * 13 + 10 - 1]);
                        option.endDate = TradeDays.DateTimeToInt((DateTime)optionList[i * 13 + 11 - 1]);
                        if (myOptionList.ContainsKey(option.optionCode) == false)
                        {
                            myOptionList.Add(option.optionCode, option);
                        }
                    }
                }
            }
            w.stop();
        }


        /// <summary>
        /// 将获取的期权信息数据存入到数据库中。
        /// </summary>
        /// <returns>返回是否存储成功的判断。</returns>
        private bool SaveOptionInformationList()
        {
            bool success = false;
            using (SqlConnection conn = new SqlConnection(connectString))
            {
                HashSet<int> optionListFromTable = new HashSet<int>();
                conn.Open();//打开数据库  
                SqlCommand cmd = conn.CreateCommand();
                cmd.CommandText = "select [OptionCode] from [" + dataBaseName + "].[dbo].[" + optionCodeTableName + "]";
                //尝试读取所有合约代码。判断数据表是否存在。
                int theLastCode = 0;
                try
                {
                    //从数据库中读取数据流存入reader中  
                    SqlDataReader reader = cmd.ExecuteReader();
                    //从reader中读取下一行数据,如果没有数据,reader.Read()返回flase  
                    while (reader.Read())
                    {
                        int optionCode = reader.GetInt32(reader.GetOrdinal("OptionCode"));
                        theLastCode = (optionCode > theLastCode) ? optionCode : theLastCode;
                        optionListFromTable.Add(optionCode);
                    }
                }
                catch (Exception myerror)
                {
                    System.Console.WriteLine(myerror.Message);
                }
                //若数据表不存在就创建新表。
                if (theLastCode==0)
                {
                    System.Console.WriteLine("Creating new database of optionCode");
                    cmd.CommandText = "create table [" + dataBaseName + "].[dbo].[" + optionCodeTableName + "] ([OptionCode] int not null,[OptionName] char(24),[ExecuteType] char(4),[Strike] float,[OptionType] char(4),[StartDate] int,[EndDate] int,[Market] char(4),primary key ([OptionCode]))";
                    try
                    {
                        cmd.ExecuteReader();
                    }
                    catch (Exception myerror)
                    {
                        System.Console.WriteLine(myerror.Message);
                    }
                }
                //如果表中的最大日期小于myOptionList中的最大日期就更新本表。
                
                if (myOptionList.Count > 0)
                {
                    //利用DateTable格式存入数据。
                    DataTable myDataTable = new DataTable();
                    myDataTable.Columns.Add("OptionCode", typeof(int));
                    myDataTable.Columns.Add("OptionName", typeof(string));
                    myDataTable.Columns.Add("ExecuteType", typeof(string));
                    myDataTable.Columns.Add("Strike", typeof(double));
                    myDataTable.Columns.Add("OptionType", typeof(string));
                    myDataTable.Columns.Add("StartDate", typeof(int));
                    myDataTable.Columns.Add("EndDate", typeof(int));
                    myDataTable.Columns.Add("Market", typeof(string));
                    foreach (var item in myOptionList)
                    {
                        if (optionListFromTable.Contains(item.Key)==false)
                        {
                            DataRow r = myDataTable.NewRow();
                            r["OptionCode"] = item.Value.optionCode; 
                            r["OptionName"]   = item.Value.optionName;
                            r["ExecuteType"] = item.Value.executeType;
                            r["Strike"] = item.Value.strike;
                            r["OptionType"] = item.Value.optionType;
                            r["StartDate"] = item.Value.startDate;
                            r["EndDate"] = item.Value.endDate;
                            r["Market"] = item.Value.market;
                            myDataTable.Rows.Add(r);
                        }
                    }
                    //利用sqlbulkcopy写入数据
                    using (SqlBulkCopy bulk = new SqlBulkCopy(connectString))
                    {
                        try
                        {
                            bulk.DestinationTableName = optionCodeTableName;
                            bulk.ColumnMappings.Add("OptionCode", "OptionCode");
                            bulk.ColumnMappings.Add("OptionName", "OptionName");
                            bulk.ColumnMappings.Add("ExecuteType", "ExecuteType");
                            bulk.ColumnMappings.Add("Strike", "Strike");
                            bulk.ColumnMappings.Add("OptionType", "OptionType");
                            bulk.ColumnMappings.Add("StartDate", "StartDate");
                            bulk.ColumnMappings.Add("EndDate", "EndDate");
                            bulk.ColumnMappings.Add("Market", "Market");
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
    }
}
