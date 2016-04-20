using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TDBAPI;
using System.Data.SqlClient;
using System.Data;

namespace myWindAPI
{
    /// <summary>
    /// 获取TDB数据的类。
    /// </summary>
    class WindTDBData
    {
        //一些全局变量的获取。主要是表的名称。
        private string connectString = Configuration.connectString;
        private string dataBaseName = Configuration.dataBaseName;
        private string tableOf50ETF = Configuration.tableOf50ETF;
        private string tableOfOptionAll = Configuration.tableOfOptionAll;
        private string tableOfIHFront = Configuration.tableOfIHFront;
        private string tableofIHNext = Configuration.tableofIHNext;

        /// <summary>
        /// TDB数据接口类。
        /// </summary>
        private TDBDataSource tdbSource;
        
        /// <summary>
        /// 需要获取数据的名称类型等。
        /// </summary>
        public TDBdataInformation dataInformation = new TDBdataInformation();
        
        /// <summary>
        /// TDB数据库连接信息。
        /// </summary>
        public TDBsource mySource = new TDBsource();
        
        /// <summary>
        /// 记录交易日信息的类。
        /// </summary>
        public TradeDays myTradeDays;

        /// <summary>
        /// 新建50etf数据表的函数。
        /// </summary>
        private void CreateTableOf50ETF()
        {
            using (SqlConnection conn = new SqlConnection(connectString))
            {
                conn.Open();//打开数据库  
                SqlCommand cmd = conn.CreateCommand();
                cmd.CommandText = "create table ["+dataBaseName+ "].[dbo].[" + tableOf50ETF + "] (Code int,Market char(4),[Date] int not null,[Time] int not null,[Tick] int not null,[Volume] float,[Turnover] float,[AccVolume] float,[AccTurnover] float,[Open] float,[High] float,[Low] float,[LastPrice] float,Ask1 float,Ask2 float,Ask3 float,Ask4 float,Ask5 float,Askv1 float,Askv2 float,Askv3 float,Askv4 float,Askv5 float,Bid1 float,Bid2 float,Bid3 float,Bid4 float,Bid5 float,Bidv1 float,Bidv2 float,Bidv3 float,Bidv4 float,Bidv5 float,[PreClose] float,primary key ([Date],[Time],[Tick]))";
                try
                {
                    cmd.ExecuteReader();
                }
                catch (Exception myerror)
                {
                    System.Console.WriteLine(myerror.Message);
                }
            }
                
        }

        /// <summary>
        /// 新建期权数据表。
        /// </summary>
        /// <param name="tableOfOption"></param>
        private void CreateTableOfOption(string tableOfOption)
        {
            using (SqlConnection conn = new SqlConnection(connectString))
            {
                conn.Open();//打开数据库  
                SqlCommand cmd = conn.CreateCommand();
                cmd.CommandText = "create table [" + dataBaseName + "].[dbo].[" + tableOfOption + "] ([Code] int not null,[OptionType] char(4),[Strike] float,[StartDate] int,[EndDate] int,[Date] int not null,[Time] int not null,[Tick] int not null,[Volume] float,[Turnover] float,[AccVolume] float,[AccTurnover] float,[Open] float,[High] float,[Low] float,[LastPrice] float,[Ask1] float,[Ask2] float,[Ask3] float,[Ask4] float,[Ask5] float,[Askv1] float,[Askv2] float,[Askv3] float,[Askv4] float,[Askv5] float,[Bid1] float,[Bid2] float,[Bid3] float,[Bid4] float,[Bid5] float,[Bidv1] float,[Bidv2] float,[Bidv3] float,[Bidv4] float,[Bidv5] float,Ask1Volatility float,Ask1Delta float,Ask2Volatility float,Ask2Delta float,Ask3Volatility float,Ask3Delta float,Ask4Volatility float,Ask4Delta float,Ask5Volatility float,Ask5Delta float,Bid1Volatility float,Bid1Delta float,Bid2Volatility float,Bid2Delta float,Bid3Volatility float,Bid3Delta float,Bid4Volatility float,Bid4Delta float,Bid5Volatility float,Bid5Delta float,MidVolatility float,MidDelta float,[PreClose] float,[PreSettle] float,[OpenMargin] float,primary key ([Date],[Time],[Tick]))";
                try
                {
                    cmd.ExecuteReader();
                }
                catch (Exception myerror)
                {
                    System.Console.WriteLine(myerror.Message);
                }
            }

        }

        private void CreateTableOfFinancialFuture(string tableOfFinancialFuture)
        {
            using (SqlConnection conn = new SqlConnection(connectString))
            {
                conn.Open();//打开数据库  
                SqlCommand cmd = conn.CreateCommand();
                cmd.CommandText = "create table [" + dataBaseName + "].[dbo].[" + tableOfFinancialFuture + "] ([Code] int not null,[Date] int not null,[Time] int not null,[Tick] int not null,[Volume] float,[Turnover] float,[AccVolume] float,[AccTurnover] float,[Open] float,[High] float,[Low] float,[LastPrice] float,[Ask1] float,[Ask2] float,[Ask3] float,[Ask4] float,[Ask5] float,[Askv1] float,[Askv2] float,[Askv3] float,[Askv4] float,[Askv5] float,[Bid1] float,[Bid2] float,[Bid3] float,[Bid4] float,[Bid5] float,[Bidv1] float,[Bidv2] float,[Bidv3] float,[Bidv4] float,[Bidv5] float,[PreClose] float,primary key ([Date],[Time],[Tick]))";
                try
                {
                    cmd.ExecuteReader();
                }
                catch (Exception myerror)
                {
                    System.Console.WriteLine(myerror.Message);
                }
            }
        }

        /// <summary>
        /// 50etf数据的计数函数。
        /// </summary>
        /// <param name="today">日期</param>
        /// <returns>数据的个数。</returns>
        private int CountDataOf50ETF(int today)
        {
            using (SqlConnection conn = new SqlConnection(Configuration.connectString))
            {
                conn.Open();//打开数据库  
                SqlCommand cmd = conn.CreateCommand();
                cmd.CommandText = "select COUNT(Date) from [" + dataBaseName + "].[dbo].[" + tableOf50ETF + "] where [Date]="+today.ToString();
                try
                {

                    int number = (int)cmd.ExecuteScalar();
                    return number;
                }
                catch (Exception myerror)
                {
                    System.Console.WriteLine(myerror.Message);
                }
            }
            return 0;
        }

        /// <summary>
        /// 构造函数。
        /// </summary>
        /// <param name="codeName">品种名称</param>
        /// <param name="startDate">开始时间</param>
        /// <param name="endDate">结束时间</param>
        /// <param name="type">品种类型</param>
        public WindTDBData(string market, int startDate, int endDate, string type)
        {
            dataInformation.market = market;
            dataInformation.startDate = startDate;
            dataInformation.endDate = endDate;
            dataInformation.type = type;
            if (type == "option" || type == "50etf" || type =="ih" || type=="if" || type=="ic" || type=="sh" || type=="50etfOrder")
            {
                mySource = Configuration.SHsource;
            }
            else if (type=="commodity")
            {
                mySource = Configuration.commoditySource;
            }
            //对接口类进行初始化。
            tdbSource = new TDBDataSource(mySource.IP, mySource.port, mySource.account, mySource.password);
            if (CheckConnection())
            {
                Console.WriteLine("Connect Success!");
                myTradeDays = new TradeDays(dataInformation.startDate, dataInformation.endDate);
                Console.WriteLine("Tradedays Collect!");
                switch (type)
                {
                    case "50etf":
                        Store50ETFData();
                        break;
                    case "option":
                        //在写入期权数据的时候，首先要有50etf的价格，这样才能计算波动率等希腊值。
                        Store50ETFData();
                        //核心函数，保存50etf期权的数据。
                        StoreOptionData(dataInformation.startDate, dataInformation.endDate);
                        break;
                    case "ih":
                        //StoreFinancialFuturesData(type, tableOfIHFront,tableofIHNext);
                        StoreFinancialFuturesData(type);
                        break;
                    case "50etfOrder":
                        //Store50ETFOrderData();
                        break;
                    default:
                        break;
                }
                switch (market)
                {
                    case "dce":
                        StoreDCECommodity(market);
                        break;
                    default:
                        break;
                }
            }
            else
            {
                Console.WriteLine("Please Input Valid Parameters!");
            }

            //工作完毕之后，关闭万德TDB数据库的连接。
            //关闭连接
            tdbSource.DisConnect();
        }

        public void StoreDCECommodity(string market)
        {
            TDBCode[] codeArr;
            tdbSource.GetCodeTable(market, out codeArr);
            TDBReqFuture reqFuture = new TDBReqFuture("a.DCE", 20130101,20130531);
            TDBFutureAB[] futureABArr;
            reqFuture.m_nAutoComplete = 0;
            TDBErrNo nErrInner = tdbSource.GetFutureAB(reqFuture, out futureABArr);
        }

        /// <summary>
        /// 获取50ETF的逐笔成交数据
        /// </summary>
        public void Store50ETFOrderData()
        {
            //测试逐笔成交
            {
                TDBReq reqTransaction = new TDBReq("510050.SH",20160405);
                TDBTransaction[] transactionArr;
                TDBErrNo nErrInner = tdbSource.GetTransaction(reqTransaction, out transactionArr);
                //输出前10条记录
                Console.WriteLine("-----------{0}------------", transactionArr.Length);
                for (int i = 0; i < transactionArr.Length && i < 10; i++)
                {
                    Console.WriteLine("windcode:{0}, date:{1},time:{2}, price:{3},volume:{4},askorder:{5},bidorder:{6}",
                        transactionArr[i].m_strWindCode, transactionArr[i].m_nDate, transactionArr[i].m_nTime, transactionArr[i].m_nTradePrice,
                        transactionArr[i].m_nTradeVolume, transactionArr[i].m_nAskOrder, transactionArr[i].m_nBidOrder);
                }
            }
        }

        /// <summary>
        /// 判断TDB数据库是否连接成功。
        /// </summary>
        /// <returns>返回是否连接成功。</returns>
        public bool CheckConnection()
        {
            TDBLoginResult loginRes;
            TDBErrNo nErr = tdbSource.Connect(out loginRes);
            //输出登陆结果
            if (nErr == TDBErrNo.TDB_OPEN_FAILED)
            {
                Console.WriteLine("open failed, reason:{0}", loginRes.m_strInfo);
                Console.WriteLine();
                return false;
            }
            return true;
        }

        /// <summary>
        /// 存储50etf数据的函数。
        /// </summary>
        public void Store50ETFData()
        {
            if (SqlApplication.CheckExist(dataBaseName,tableOf50ETF))
            {
                Console.WriteLine("Table of 510050.SH exists!");

            }
            else
            {
                Console.WriteLine("Crating table of 510050.SH!");
                CreateTableOf50ETF();
            }
            using (SqlConnection conn = new SqlConnection(connectString))
            {
                conn.Open();//打开数据库  
                foreach (int date in myTradeDays.myTradeDays)
                {
                    int number = CountDataOf50ETF(date);
                    if (number > 0)
                    {
                        Console.WriteLine("510050: Date {0},numbers {1}", date, number);
                    }
                    else
                    {
                        
                        TDBReq reqTick = new TDBReq("510050.sh", date, date);
                        TDBTickAB[] tickArr;
                        TDBErrNo nErrInner = tdbSource.GetTickAB(reqTick, out tickArr);
                        Console.WriteLine("Insert 510050: Date {0},numbers {1}", date, tickArr.Length);
                        DataTable todayData = new DataTable();
                        #region DataTable的列名的建立
                        todayData.Columns.Add("Code", typeof(string));
                        todayData.Columns.Add("Market", typeof(string));
                        todayData.Columns.Add("Date", typeof(int));
                        todayData.Columns.Add("Time", typeof(int));
                        todayData.Columns.Add("Tick", typeof(int));
                        todayData.Columns.Add("Volume", typeof(double));
                        todayData.Columns.Add("Turnover", typeof(double));
                        todayData.Columns.Add("AccVolume", typeof(double));
                        todayData.Columns.Add("AccTurnover", typeof(double));
                        todayData.Columns.Add("Open", typeof(double));
                        todayData.Columns.Add("High", typeof(double));
                        todayData.Columns.Add("Low", typeof(double));
                        todayData.Columns.Add("LastPrice", typeof(double));
                        todayData.Columns.Add("Ask1", typeof(double));
                        todayData.Columns.Add("Ask2", typeof(double));
                        todayData.Columns.Add("Ask3", typeof(double));
                        todayData.Columns.Add("Ask4", typeof(double));
                        todayData.Columns.Add("Ask5", typeof(double));
                        todayData.Columns.Add("Askv1", typeof(double));
                        todayData.Columns.Add("Askv2", typeof(double));
                        todayData.Columns.Add("Askv3", typeof(double));
                        todayData.Columns.Add("Askv4", typeof(double));
                        todayData.Columns.Add("Askv5", typeof(double));
                        todayData.Columns.Add("Bid1", typeof(double));
                        todayData.Columns.Add("Bid2", typeof(double));
                        todayData.Columns.Add("Bid3", typeof(double));
                        todayData.Columns.Add("Bid4", typeof(double));
                        todayData.Columns.Add("Bid5", typeof(double));
                        todayData.Columns.Add("Bidv1", typeof(double));
                        todayData.Columns.Add("Bidv2", typeof(double));
                        todayData.Columns.Add("Bidv3", typeof(double));
                        todayData.Columns.Add("Bidv4", typeof(double));
                        todayData.Columns.Add("Bidv5", typeof(double));
                        todayData.Columns.Add("PreClose", typeof(double));
                        #endregion
                        int lastTick = 0;
                        int lastDate = 0;
                        int lastTime = 0;
                        for (int i = 0; i < tickArr.Length; i++)
                        {
                            TDBTickAB dataInformation = tickArr[i];
                            if (dataInformation.m_nDate == lastDate && dataInformation.m_nTime == lastTime)
                            {
                                lastTick += 1;
                            }
                            else
                            {
                                lastTick = 0;
                                lastDate = dataInformation.m_nDate;
                                lastTime = dataInformation.m_nTime;
                            }
                            #region 将数据写入每一行中。
                            DataRow r = todayData.NewRow();
                            r["Code"] = Convert.ToInt32(dataInformation.m_strWindCode.Substring(0, 6));
                            r["Market"] = dataInformation.m_strWindCode.Substring(7, 2);
                            r["Date"] = dataInformation.m_nDate;
                            r["Time"] = dataInformation.m_nTime;
                            r["Tick"] = lastTick;
                            r["Volume"] = dataInformation.m_iVolume;
                            r["Turnover"] = dataInformation.m_iTurover;
                            r["AccVolume"] = dataInformation.m_iAccVolume;
                            r["AccTurnover"] = dataInformation.m_iAccTurover;
                            r["Open"] = Convert.ToDouble(dataInformation.m_nOpen) / 10000;
                            r["High"] = Convert.ToDouble(dataInformation.m_nHigh) / 10000;
                            r["Low"] = Convert.ToDouble(dataInformation.m_nLow) / 10000;
                            r["LastPrice"] = Convert.ToDouble(dataInformation.m_nPrice) / 10000;
                            r["Ask1"] = Convert.ToDouble(dataInformation.m_nAskPrice[0]) / 10000;
                            r["Ask2"] = Convert.ToDouble(dataInformation.m_nAskPrice[1]) / 10000;
                            r["Ask3"] = Convert.ToDouble(dataInformation.m_nAskPrice[2]) / 10000;
                            r["Ask4"] = Convert.ToDouble(dataInformation.m_nAskPrice[3]) / 10000;
                            r["Ask5"] = Convert.ToDouble(dataInformation.m_nAskPrice[4]) / 10000;
                            r["Askv1"] = dataInformation.m_nAskVolume[0];
                            r["Askv2"] = dataInformation.m_nAskVolume[1];
                            r["Askv3"] = dataInformation.m_nAskVolume[2];
                            r["Askv4"] = dataInformation.m_nAskVolume[3];
                            r["Askv5"] = dataInformation.m_nAskVolume[4];
                            r["Bid1"] = Convert.ToDouble(dataInformation.m_nBidPrice[0]) / 10000;
                            r["Bid2"] = Convert.ToDouble(dataInformation.m_nBidPrice[1]) / 10000;
                            r["Bid3"] = Convert.ToDouble(dataInformation.m_nBidPrice[2]) / 10000;
                            r["Bid4"] = Convert.ToDouble(dataInformation.m_nBidPrice[3]) / 10000;
                            r["Bid5"] = Convert.ToDouble(dataInformation.m_nBidPrice[4]) / 10000;
                            r["Bidv1"] = dataInformation.m_nBidVolume[0];
                            r["Bidv2"] = dataInformation.m_nBidVolume[1];
                            r["Bidv3"] = dataInformation.m_nBidVolume[2];
                            r["Bidv4"] = dataInformation.m_nBidVolume[3];
                            r["Bidv5"] = dataInformation.m_nBidVolume[4];
                            r["PreClose"] = Convert.ToDouble(dataInformation.m_nPreClose) / 10000;
                            todayData.Rows.Add(r);
                            #endregion
                        }

                        using (SqlBulkCopy bulk = new SqlBulkCopy(connectString))
                        {
                            try
                            {
                                bulk.BatchSize = 100000;
                                bulk.DestinationTableName =tableOf50ETF;
                                #region 依次建立数据的映射。
                                bulk.ColumnMappings.Add("Code", "Code");
                                bulk.ColumnMappings.Add("Market","Market");
                                bulk.ColumnMappings.Add("Date", "Date");
                                bulk.ColumnMappings.Add("Time", "Time");
                                bulk.ColumnMappings.Add("Tick", "Tick");
                                bulk.ColumnMappings.Add("Volume", "Volume");
                                bulk.ColumnMappings.Add("Turnover", "Turnover");
                                bulk.ColumnMappings.Add("AccVolume", "AccVolume");
                                bulk.ColumnMappings.Add("AccTurnover", "AccTurnover");
                                bulk.ColumnMappings.Add("Open", "Open");
                                bulk.ColumnMappings.Add("High", "High");
                                bulk.ColumnMappings.Add("Low", "Low");
                                bulk.ColumnMappings.Add("LastPrice", "LastPrice");
                                bulk.ColumnMappings.Add("Ask1", "Ask1");
                                bulk.ColumnMappings.Add("Ask2", "Ask2");
                                bulk.ColumnMappings.Add("Ask3", "Ask3");
                                bulk.ColumnMappings.Add("Ask4", "Ask4");
                                bulk.ColumnMappings.Add("Ask5", "Ask5");
                                bulk.ColumnMappings.Add("Askv1", "Askv1");
                                bulk.ColumnMappings.Add("Askv2", "Askv2");
                                bulk.ColumnMappings.Add("Askv3", "Askv3");
                                bulk.ColumnMappings.Add("Askv4", "Askv4");
                                bulk.ColumnMappings.Add("Askv5", "Askv5");
                                bulk.ColumnMappings.Add("Bid1", "Bid1");
                                bulk.ColumnMappings.Add("Bid2", "Bid2");
                                bulk.ColumnMappings.Add("Bid3", "Bid3");
                                bulk.ColumnMappings.Add("Bid4", "Bid4");
                                bulk.ColumnMappings.Add("Bid5", "Bid5");
                                bulk.ColumnMappings.Add("Bidv1", "Bidv1");
                                bulk.ColumnMappings.Add("Bidv2", "Bidv2");
                                bulk.ColumnMappings.Add("Bidv3", "Bidv3");
                                bulk.ColumnMappings.Add("Bidv4", "Bidv4");
                                bulk.ColumnMappings.Add("Bidv5", "Bidv5");
                                bulk.ColumnMappings.Add("PreClose", "PreClose");
                                #endregion
                                bulk.WriteToServer(todayData);
                            }
                            catch (Exception myerror)
                            {
                                System.Console.WriteLine(myerror.Message);
                            }
                        }
                            
                    }
                }
                conn.Close();
            }
        }

        /// <summary>
        /// 将整理过的期权数据存入指定的表。
        /// </summary>
        /// <param name="optionList">期权数据</param>
        /// <param name="tableName">表名称</param>
        /// <param name="date">日期</param>
        /// <param name="code">代码名称</param>
        public bool InsertOptionData(List<optionDataFormat> optionList,string tableName,int date,int code)
        {
            bool success = false;
            using (SqlConnection conn=new SqlConnection(connectString))
            {
                conn.Open();
                DataTable todayData = new DataTable();
                #region DataTable的列名的建立
                todayData.Columns.Add("Code", typeof(int));
                todayData.Columns.Add("OptionType", typeof(string));
                todayData.Columns.Add("Strike", typeof(double));
                todayData.Columns.Add("StartDate", typeof(int));
                todayData.Columns.Add("EndDate", typeof(int));
                todayData.Columns.Add("Date", typeof(int));
                todayData.Columns.Add("Time", typeof(int));
                todayData.Columns.Add("Tick", typeof(int));
                todayData.Columns.Add("UnderlyingPrice", typeof(double));
                todayData.Columns.Add("Volume", typeof(double));
                todayData.Columns.Add("Turnover", typeof(double));
                todayData.Columns.Add("AccVolume", typeof(double));
                todayData.Columns.Add("AccTurnover", typeof(double));
                todayData.Columns.Add("Open", typeof(double));
                todayData.Columns.Add("High", typeof(double));
                todayData.Columns.Add("Low", typeof(double));
                todayData.Columns.Add("LastPrice", typeof(double));
                todayData.Columns.Add("Ask1", typeof(double));
                todayData.Columns.Add("Ask2", typeof(double));
                todayData.Columns.Add("Ask3", typeof(double));
                todayData.Columns.Add("Ask4", typeof(double));
                todayData.Columns.Add("Ask5", typeof(double));
                todayData.Columns.Add("Askv1", typeof(double));
                todayData.Columns.Add("Askv2", typeof(double));
                todayData.Columns.Add("Askv3", typeof(double));
                todayData.Columns.Add("Askv4", typeof(double));
                todayData.Columns.Add("Askv5", typeof(double));
                todayData.Columns.Add("Bid1", typeof(double));
                todayData.Columns.Add("Bid2", typeof(double));
                todayData.Columns.Add("Bid3", typeof(double));
                todayData.Columns.Add("Bid4", typeof(double));
                todayData.Columns.Add("Bid5", typeof(double));
                todayData.Columns.Add("Bidv1", typeof(double));
                todayData.Columns.Add("Bidv2", typeof(double));
                todayData.Columns.Add("Bidv3", typeof(double));
                todayData.Columns.Add("Bidv4", typeof(double));
                todayData.Columns.Add("Bidv5", typeof(double));
                todayData.Columns.Add("Ask1Volatility", typeof(double));
                todayData.Columns.Add("Ask2Volatility", typeof(double));
                todayData.Columns.Add("Ask3Volatility", typeof(double));
                todayData.Columns.Add("Ask4Volatility", typeof(double));
                todayData.Columns.Add("Ask5Volatility", typeof(double));
                todayData.Columns.Add("Ask1Delta", typeof(double));
                todayData.Columns.Add("Ask2Delta", typeof(double));
                todayData.Columns.Add("Ask3Delta", typeof(double));
                todayData.Columns.Add("Ask4Delta", typeof(double));
                todayData.Columns.Add("Ask5Delta", typeof(double));
                todayData.Columns.Add("Bid1Volatility", typeof(double));
                todayData.Columns.Add("Bid2Volatility", typeof(double));
                todayData.Columns.Add("Bid3Volatility", typeof(double));
                todayData.Columns.Add("Bid4Volatility", typeof(double));
                todayData.Columns.Add("Bid5Volatility", typeof(double));
                todayData.Columns.Add("Bid1Delta", typeof(double));
                todayData.Columns.Add("Bid2Delta", typeof(double));
                todayData.Columns.Add("Bid3Delta", typeof(double));
                todayData.Columns.Add("Bid4Delta", typeof(double));
                todayData.Columns.Add("Bid5Delta", typeof(double));
                todayData.Columns.Add("PreClose", typeof(double));
                todayData.Columns.Add("PreSettle", typeof(double));
                todayData.Columns.Add("OpenMargin", typeof(double));
                todayData.Columns.Add("MidVolatility", typeof(double));
                todayData.Columns.Add("MidDelta", typeof(double));
                #endregion
                foreach (optionDataFormat option in optionList)
                {
                    #region 将数据写入每一行中。
                    DataRow r = todayData.NewRow();
                    r["Code"] = option.optionCode;
                    r["OptionType"] = option.optionType;
                    r["Strike"] = option.strike;
                    r["StartDate"] = option.startDate;
                    r["EndDate"] = option.endDate;
                    r["Date"] = option.date;
                    r["Time"] = option.time;
                    r["Tick"] = option.tick;
                    r["UnderlyingPrice"] = option.underlyingPrice;
                    r["Volume"] = option.volumn;
                    r["Turnover"] = option.turnover;
                    r["AccVolume"] = option.accVolumn;
                    r["AccTurnover"] = option.accTurnover;
                    r["Open"] = option.open;
                    r["High"] = option.high;
                    r["Low"] = option.low;
                    r["LastPrice"] = option.lastPrice;
                    r["Ask1"] = option.ask[0];
                    r["Ask2"] = option.ask[1];
                    r["Ask3"] = option.ask[2];
                    r["Ask4"] = option.ask[3];
                    r["Ask5"] = option.ask[4];
                    r["Askv1"] = option.askv[0];
                    r["Askv2"] = option.askv[1];
                    r["Askv3"] = option.askv[2];
                    r["Askv4"] = option.askv[3];
                    r["Askv5"] = option.askv[4];
                    r["Bid1"] = option.bid[0];
                    r["Bid2"] = option.bid[1];
                    r["Bid3"] = option.bid[2];
                    r["Bid4"] = option.bid[3];
                    r["Bid5"] = option.bid[4];
                    r["Bidv1"] = option.bidv[0];
                    r["Bidv2"] = option.bidv[1];
                    r["Bidv3"] = option.bidv[2];
                    r["Bidv4"] = option.bidv[3];
                    r["Bidv5"] = option.bidv[4];
                    r["Ask1Volatility"] = option.askVolatility[0];
                    r["Ask2Volatility"] = option.askVolatility[1];
                    r["Ask3Volatility"] = option.askVolatility[2];
                    r["Ask4Volatility"] = option.askVolatility[3];
                    r["Ask5Volatility"] = option.askVolatility[4];
                    r["Ask1Delta"] = option.askDelta[0];
                    r["Ask2Delta"] = option.askDelta[1];
                    r["Ask3Delta"] = option.askDelta[2];
                    r["Ask4Delta"] = option.askDelta[3];
                    r["Ask5Delta"] = option.askDelta[4];
                    r["Bid1Volatility"] = option.bidVolatility[0];
                    r["Bid2Volatility"] = option.bidVolatility[1];
                    r["Bid3Volatility"] = option.bidVolatility[2];
                    r["Bid4Volatility"] = option.bidVolatility[3];
                    r["Bid5Volatility"] = option.bidVolatility[4];
                    r["Bid1Delta"] = option.bidDelta[0];
                    r["Bid2Delta"] = option.bidDelta[1];
                    r["Bid3Delta"] = option.bidDelta[2];
                    r["Bid4Delta"] = option.bidDelta[3];
                    r["Bid5Delta"] = option.bidDelta[4];
                    r["PreClose"] = option.preClose;
                    r["PreSettle"] = option.preSettle;
                    r["OpenMargin"] = option.openMargin;
                    r["MidVolatility"] = option.midVolatility;
                    r["MidDelta"] = option.midDelta;
                    todayData.Rows.Add(r);
                    #endregion
                }
                string csvname = @"G:\csvdata\"+tableName + ".csv";
                CsvApplication.SaveCSV(todayData, csvname);

                using (SqlBulkCopy bulk = new SqlBulkCopy(connectString))
                {
                    try
                    {
                        bulk.BatchSize = 100000;
                        bulk.DestinationTableName = tableName;
                        #region 依次建立数据的映射。
                        bulk.ColumnMappings.Add("Code", "Code");
                        bulk.ColumnMappings.Add("OptionType", "OptionType");
                        bulk.ColumnMappings.Add("Strike", "Strike");
                        bulk.ColumnMappings.Add("StartDate", "StartDate");
                        bulk.ColumnMappings.Add("EndDate", "EndDate");
                        bulk.ColumnMappings.Add("Date", "Date");
                        bulk.ColumnMappings.Add("Time", "Time");
                        bulk.ColumnMappings.Add("Tick", "Tick");
                        bulk.ColumnMappings.Add("UnderlyingPrice", "UnderlyingPrice");
                        bulk.ColumnMappings.Add("Volume", "Volume");
                        bulk.ColumnMappings.Add("Turnover", "Turnover");
                        bulk.ColumnMappings.Add("AccVolume", "AccVolume");
                        bulk.ColumnMappings.Add("AccTurnover", "AccTurnover");
                        bulk.ColumnMappings.Add("Open", "Open");
                        bulk.ColumnMappings.Add("High", "High");
                        bulk.ColumnMappings.Add("Low", "Low");
                        bulk.ColumnMappings.Add("LastPrice", "LastPrice");
                        bulk.ColumnMappings.Add("Ask1", "Ask1");
                        bulk.ColumnMappings.Add("Ask2", "Ask2");
                        bulk.ColumnMappings.Add("Ask3", "Ask3");
                        bulk.ColumnMappings.Add("Ask4", "Ask4");
                        bulk.ColumnMappings.Add("Ask5", "Ask5");
                        bulk.ColumnMappings.Add("Askv1", "Askv1");
                        bulk.ColumnMappings.Add("Askv2", "Askv2");
                        bulk.ColumnMappings.Add("Askv3", "Askv3");
                        bulk.ColumnMappings.Add("Askv4", "Askv4");
                        bulk.ColumnMappings.Add("Askv5", "Askv5");
                        bulk.ColumnMappings.Add("Bid1", "Bid1");
                        bulk.ColumnMappings.Add("Bid2", "Bid2");
                        bulk.ColumnMappings.Add("Bid3", "Bid3");
                        bulk.ColumnMappings.Add("Bid4", "Bid4");
                        bulk.ColumnMappings.Add("Bid5", "Bid5");
                        bulk.ColumnMappings.Add("Bidv1", "Bidv1");
                        bulk.ColumnMappings.Add("Bidv2", "Bidv2");
                        bulk.ColumnMappings.Add("Bidv3", "Bidv3");
                        bulk.ColumnMappings.Add("Bidv4", "Bidv4");
                        bulk.ColumnMappings.Add("Bidv5", "Bidv5");
                        bulk.ColumnMappings.Add("Ask1Volatility", "Ask1Volatility");
                        bulk.ColumnMappings.Add("Ask2Volatility", "Ask2Volatility");
                        bulk.ColumnMappings.Add("Ask3Volatility", "Ask3Volatility");
                        bulk.ColumnMappings.Add("Ask4Volatility", "Ask4Volatility");
                        bulk.ColumnMappings.Add("Ask5Volatility", "Ask5Volatility");
                        bulk.ColumnMappings.Add("Ask1Delta", "Ask1Delta");
                        bulk.ColumnMappings.Add("Ask2Delta", "Ask2Delta");
                        bulk.ColumnMappings.Add("Ask3Delta", "Ask3Delta");
                        bulk.ColumnMappings.Add("Ask4Delta", "Ask4Delta");
                        bulk.ColumnMappings.Add("Ask5Delta", "Ask5Delta");
                        bulk.ColumnMappings.Add("Bid1Volatility", "Bid1Volatility");
                        bulk.ColumnMappings.Add("Bid2Volatility", "Bid2Volatility");
                        bulk.ColumnMappings.Add("Bid3Volatility", "Bid3Volatility");
                        bulk.ColumnMappings.Add("Bid4Volatility", "Bid4Volatility");
                        bulk.ColumnMappings.Add("Bid5Volatility", "Bid5Volatility");
                        bulk.ColumnMappings.Add("Bid1Delta", "Bid1Delta");
                        bulk.ColumnMappings.Add("Bid2Delta", "Bid2Delta");
                        bulk.ColumnMappings.Add("Bid3Delta", "Bid3Delta");
                        bulk.ColumnMappings.Add("Bid4Delta", "Bid4Delta");
                        bulk.ColumnMappings.Add("Bid5Delta", "Bid5Delta");
                        bulk.ColumnMappings.Add("PreClose", "PreClose");
                        bulk.ColumnMappings.Add("PreSettle", "PreSettle");
                        bulk.ColumnMappings.Add("OpenMargin", "OpenMargin");
                        bulk.ColumnMappings.Add("MidVolatility", "MidVolatility");
                        bulk.ColumnMappings.Add("MidDelta", "MidDelta");
                        #endregion
                        bulk.WriteToServer(todayData);
                        Console.WriteLine("Date:{0}, Option: {1} insert data: {2} to {3}", date, code, optionList.Count,tableName);
                        
                        success = true;
                    }
                    catch (Exception myerror)
                    {
                        System.Console.WriteLine(myerror.Message);
                        Console.WriteLine("Date:{0}, Option: {1} insert data: {2} to {3} error!!", date, code, optionList.Count, tableName);
                        success = false;
                    }
                }
                conn.Close();
                return success;
            }
        }

        /// <summary>
        /// 根据合约名称和表名称以及日期存储数据。
        /// </summary>
        /// <param name="contractName">合约名称</param>
        /// <param name="tableName">表名称</param>
        /// <param name="date">日期</param>
        /// <returns>是否存储成功</returns>
        public bool InsertFinancialFutureDate(string contractName,string tableName,int date)
        {
            TDBReqFuture reqFuture = new TDBReqFuture(contractName,date,date);
            TDBFutureAB[] futureABArr;
            reqFuture.m_nAutoComplete = 0;
            TDBErrNo nErrInner = tdbSource.GetFutureAB(reqFuture, out futureABArr);
            bool success = false;
            using (SqlConnection conn = new SqlConnection(connectString))
            {
                conn.Open();
                DataTable todayData = new DataTable();
                #region DataTable的列名的建立
                todayData.Columns.Add("Code", typeof(int));
                todayData.Columns.Add("Date", typeof(int));
                todayData.Columns.Add("Time", typeof(int));
                todayData.Columns.Add("Tick", typeof(int));
                todayData.Columns.Add("Volume", typeof(double));
                todayData.Columns.Add("Turnover", typeof(double));
                todayData.Columns.Add("AccVolume", typeof(double));
                todayData.Columns.Add("AccTurnover", typeof(double));
                todayData.Columns.Add("Open", typeof(double));
                todayData.Columns.Add("High", typeof(double));
                todayData.Columns.Add("Low", typeof(double));
                todayData.Columns.Add("LastPrice", typeof(double));
                todayData.Columns.Add("Ask1", typeof(double));
                todayData.Columns.Add("Ask2", typeof(double));
                todayData.Columns.Add("Ask3", typeof(double));
                todayData.Columns.Add("Ask4", typeof(double));
                todayData.Columns.Add("Ask5", typeof(double));
                todayData.Columns.Add("Askv1", typeof(double));
                todayData.Columns.Add("Askv2", typeof(double));
                todayData.Columns.Add("Askv3", typeof(double));
                todayData.Columns.Add("Askv4", typeof(double));
                todayData.Columns.Add("Askv5", typeof(double));
                todayData.Columns.Add("Bid1", typeof(double));
                todayData.Columns.Add("Bid2", typeof(double));
                todayData.Columns.Add("Bid3", typeof(double));
                todayData.Columns.Add("Bid4", typeof(double));
                todayData.Columns.Add("Bid5", typeof(double));
                todayData.Columns.Add("Bidv1", typeof(double));
                todayData.Columns.Add("Bidv2", typeof(double));
                todayData.Columns.Add("Bidv3", typeof(double));
                todayData.Columns.Add("Bidv4", typeof(double));
                todayData.Columns.Add("Bidv5", typeof(double));
                todayData.Columns.Add("PreClose", typeof(double));
                #endregion
                int tick = 0;
                int lastTime = 0;
                foreach (TDBFutureAB data in futureABArr)
                {
                    int time = data.m_nTime;
                    if (time > lastTime)
                    {
                        lastTime = time;
                        tick = 0;
                    }
                    else
                    {
                        tick += 1;
                    }
                    #region 将数据写入每一行中。将万德TDB格式转换为本地数据库格式。
                    DataRow r = todayData.NewRow();
                    r["Code"] =Convert.ToInt32(contractName.Substring(2, 4));
                    r["Date"] = date;
                    r["Time"] = time;
                    r["Tick"] = tick;
                    r["Volume"] = data.m_iVolume;
                    r["Turnover"] = data.m_iTurover;
                    r["AccVolume"] = data.m_iAccVolume;
                    r["AccTurnover"] = data.m_iAccTurover;
                    r["Open"] = Convert.ToDouble(data.m_nOpen) / 10000;
                    r["High"] = Convert.ToDouble(data.m_nHigh) / 10000;
                    r["Low"] = Convert.ToDouble(data.m_nLow) / 10000;
                    r["LastPrice"] = Convert.ToDouble(data.m_nPrice) / 10000;
                    r["Ask1"] = Convert.ToDouble(data.m_nAskPrice[0]) / 10000;
                    r["Ask2"] = Convert.ToDouble(data.m_nAskPrice[1]) / 10000;
                    r["Ask3"] = Convert.ToDouble(data.m_nAskPrice[2]) / 10000;
                    r["Ask4"] = Convert.ToDouble(data.m_nAskPrice[3]) / 10000;
                    r["Ask5"] = Convert.ToDouble(data.m_nAskPrice[4]) / 10000;
                    r["Askv1"] = data.m_nAskVolume[0];
                    r["Askv2"] = data.m_nAskVolume[1];
                    r["Askv3"] = data.m_nAskVolume[2];
                    r["Askv4"] = data.m_nAskVolume[3];
                    r["Askv5"] = data.m_nAskVolume[4];
                    r["Bid1"] = Convert.ToDouble(data.m_nBidPrice[0]) / 10000;
                    r["Bid2"] = Convert.ToDouble(data.m_nBidPrice[1]) / 10000;
                    r["Bid3"] = Convert.ToDouble(data.m_nBidPrice[2]) / 10000;
                    r["Bid4"] = Convert.ToDouble(data.m_nBidPrice[3]) / 10000;
                    r["Bid5"] = Convert.ToDouble(data.m_nBidPrice[4]) / 10000;
                    r["Bidv1"] = data.m_nBidVolume[0];
                    r["Bidv2"] = data.m_nBidVolume[1];
                    r["Bidv3"] = data.m_nBidVolume[2];
                    r["Bidv4"] = data.m_nBidVolume[3];
                    r["Bidv5"] = data.m_nBidVolume[4];
                    r["PreClose"] = Convert.ToDouble(data.m_nPreClose) / 10000;
                    if (Convert.ToDouble(data.m_nAskPrice[0]) * data.m_nAskVolume[0] * Convert.ToDouble(data.m_nBidPrice[0]) * data.m_nBidVolume[0] != 0)
                        //保证盘口价格有数据。
                    {
                        todayData.Rows.Add(r);
                    }
                    
                    #endregion
                }
                using (SqlBulkCopy bulk = new SqlBulkCopy(connectString))
                {
                    try
                    {
                        bulk.BatchSize = 100000;
                        bulk.DestinationTableName = tableName;
                        #region 依次建立数据的映射。
                        bulk.ColumnMappings.Add("Code", "Code");
                        bulk.ColumnMappings.Add("Date", "Date");
                        bulk.ColumnMappings.Add("Time", "Time");
                        bulk.ColumnMappings.Add("Tick", "Tick");
                        bulk.ColumnMappings.Add("Volume", "Volume");
                        bulk.ColumnMappings.Add("Turnover", "Turnover");
                        bulk.ColumnMappings.Add("AccVolume", "AccVolume");
                        bulk.ColumnMappings.Add("AccTurnover", "AccTurnover");
                        bulk.ColumnMappings.Add("Open", "Open");
                        bulk.ColumnMappings.Add("High", "High");
                        bulk.ColumnMappings.Add("Low", "Low");
                        bulk.ColumnMappings.Add("LastPrice", "LastPrice");
                        bulk.ColumnMappings.Add("Ask1", "Ask1");
                        bulk.ColumnMappings.Add("Ask2", "Ask2");
                        bulk.ColumnMappings.Add("Ask3", "Ask3");
                        bulk.ColumnMappings.Add("Ask4", "Ask4");
                        bulk.ColumnMappings.Add("Ask5", "Ask5");
                        bulk.ColumnMappings.Add("Askv1", "Askv1");
                        bulk.ColumnMappings.Add("Askv2", "Askv2");
                        bulk.ColumnMappings.Add("Askv3", "Askv3");
                        bulk.ColumnMappings.Add("Askv4", "Askv4");
                        bulk.ColumnMappings.Add("Askv5", "Askv5");
                        bulk.ColumnMappings.Add("Bid1", "Bid1");
                        bulk.ColumnMappings.Add("Bid2", "Bid2");
                        bulk.ColumnMappings.Add("Bid3", "Bid3");
                        bulk.ColumnMappings.Add("Bid4", "Bid4");
                        bulk.ColumnMappings.Add("Bid5", "Bid5");
                        bulk.ColumnMappings.Add("Bidv1", "Bidv1");
                        bulk.ColumnMappings.Add("Bidv2", "Bidv2");
                        bulk.ColumnMappings.Add("Bidv3", "Bidv3");
                        bulk.ColumnMappings.Add("Bidv4", "Bidv4");
                        bulk.ColumnMappings.Add("Bidv5", "Bidv5");
                        bulk.ColumnMappings.Add("PreClose", "PreClose");
                        #endregion
                        bulk.WriteToServer(todayData);
                        Console.WriteLine("Date:{0}, Future: {1} insert data: {2} to {3}", date, Convert.ToInt32(contractName.Substring(2, 4)), futureABArr.Length, tableName);
                        success = true;
                    }
                    catch (Exception myerror)
                    {
                        System.Console.WriteLine(myerror.Message);
                        success = false;
                    }
                }
                conn.Close();
                return success;
            }
        }

        /// <summary>
        /// 按日期来保存期权数据，并作简单的计算得到波动率保证金等。
        /// </summary>
        /// <param name="startDate">开始时间</param>
        /// <param name="endDate">结束时间</param>
        public void StoreOptionData(int startDate,int endDate)
        {
            //构造期权合约信息的类。
            OptionInformation myOptionInfo = new OptionInformation(startDate);
            //预处理获取50etf前收盘价。
            Dictionary<int, double> etfPreCloseList = GetETFPreCloseList(startDate, endDate);
            //记录每个期权合约最大的记录日期，只能在最大记录日期后一日插入。
            Dictionary<int, int> optionMaxDate = new Dictionary<int, int>();
            //按日期进行遍历。
            foreach (int today in myTradeDays.myTradeDays)
            {
                //首先读取对应日期的50etf的成交价格。
                double[] etfPrice = GetETFData(today);
                //接下来获取对应日期的50etf期权的合约编号。
                int[] optionList = myOptionInfo.GetOptionNameByDate(today);
                //最后遍历所有的option进行处理以及存储。注意会涉及到波动率和delta的计算。耗时较长。
                foreach (int optionCode in optionList)
                {
                    string tableOfOption = "sh" + optionCode.ToString();
                    //如果表不存在就建立表格并且将最后日期置为0，否者就查询表格确定最后日期。
                    if (SqlApplication.CheckExist(dataBaseName, tableOfOption)==false)
                    {
                        optionMaxDate.Add(optionCode, 0);
                        Console.WriteLine("Crating table of {0}", tableOfOption);
                        CreateTableOfOption(tableOfOption);
                    }
                    else
                    {
                        if (optionMaxDate.ContainsKey(optionCode)==false)
                        {
                            optionMaxDate.Add(optionCode, MaxDateOfOption(tableOfOption, optionCode));
                            Console.WriteLine("Date: {0} Code: {1} MaxDate: {2}", today, optionCode, optionMaxDate[optionCode]);
                        }
                    }
                    if (today !=TradeDays.GetNextTradeDay(optionMaxDate[optionCode]) && optionMaxDate[optionCode]>0)
                    {
                        continue;
                    }
                    //从TDB数据库中获取原始的期权数据。不包括希腊值保证金等数据。
                    List<optionDataFormat> optionData = GetOptionDataFromTDB(optionCode, today);
                    //计算希腊值等。并对不合理的数据进行进一步的处理。例如处理集合竞价的部分。处理开盘收盘前后的数据。
                    List<optionDataFormat> optionDataModified = ModifyOptionDataFromTDB(today, etfPreCloseList[today], optionData, etfPrice);
                    Console.WriteLine("Date:{0}, Option: {1} TDB exists data: {2}", today, optionCode, optionDataModified.Count);
                    //按单独的表存储。
                    bool success=InsertOptionData(optionDataModified, tableOfOption, today, optionCode);
                    if (success)
                    {
                        if (optionMaxDate.ContainsKey(optionCode) == false)
                        {
                            optionMaxDate.Add(optionCode, today);
                        }
                        else
                        {
                            optionMaxDate[optionCode] = today;
                        }
                    }
                    
                }
            }
        }

        /// <summary>
        /// 从TDB数据库中读取原始的数据。等待进一步的处理。
        /// </summary>
        /// <param name="optionCode">期权合约代码。</param>
        /// <param name="date">日期。</param>
        /// <returns>原始的数据。</returns>
        private List<optionDataFormat> GetOptionDataFromTDB(int optionCode,int date)
        {
            List<optionDataFormat> optionDataList = new List<optionDataFormat>();
            //从TDB数据库中读取数据。
            TDBReqFuture reqFuture = new TDBReqFuture(optionCode.ToString() + "."+dataInformation.market, date, date);
            TDBFutureAB[] futureABArr;
            reqFuture.m_nAutoComplete = 0; //是否按tick补齐。
            TDBErrNo nErrInner = tdbSource.GetFutureAB(reqFuture, out futureABArr);
            int tick = 0;
            int lastTime = 0;
            for (int i = 0; i < futureABArr.Length; i++)
            {
                //初始化存储期权数据的容器。
                optionDataFormat optionData = new optionDataFormat();
                optionData.ask = new double[5];
                optionData.askv = new double[5];
                optionData.bid = new double[5];
                optionData.bidv = new double[5];
                optionData.askVolatility = new double[5];
                optionData.askDelta = new double[5];
                optionData.bidVolatility = new double[5];
                optionData.bidDelta = new double[5];
                //选定期权数据。
                TDBFutureAB data = futureABArr[i];
                int time = data.m_nTime;
                if (time>lastTime)
                {
                    lastTime = time;
                    tick = 0;
                }
                else
                {
                    tick += 1;
                }
                //数据格式的转换。
                #region 将万德数据转化为本地的数据结构。
                optionData.optionCode = optionCode;
                optionData.optionType = OptionInformation.myOptionList[optionCode].optionType;
                optionData.strike = OptionInformation.myOptionList[optionCode].strike;
                optionData.startDate = OptionInformation.myOptionList[optionCode].startDate;
                optionData.endDate = OptionInformation.myOptionList[optionCode].endDate;
                optionData.date = date;
                optionData.time = time;
                optionData.tick = tick;
                optionData.volumn = data.m_iVolume;
                optionData.turnover = data.m_iTurover;
                optionData.accVolumn = data.m_iAccVolume;
                optionData.accTurnover = data.m_iAccTurover;
                optionData.open = Convert.ToDouble(data.m_nOpen) / 10000;
                optionData.high = Convert.ToDouble(data.m_nHigh) / 10000;
                optionData.low = Convert.ToDouble(data.m_nLow) / 10000;
                optionData.lastPrice = Convert.ToDouble(data.m_nPrice) / 10000;
                optionData.ask[0] = Convert.ToDouble(data.m_nAskPrice[0]) / 10000;
                optionData.ask[1] = Convert.ToDouble(data.m_nAskPrice[1]) / 10000;
                optionData.ask[2] = Convert.ToDouble(data.m_nAskPrice[2]) / 10000;
                optionData.ask[3] = Convert.ToDouble(data.m_nAskPrice[3]) / 10000;
                optionData.ask[4] = Convert.ToDouble(data.m_nAskPrice[4]) / 10000;
                optionData.askv[0] = data.m_nAskVolume[0];
                optionData.askv[1] = data.m_nAskVolume[1];
                optionData.askv[2] = data.m_nAskVolume[2];
                optionData.askv[3] = data.m_nAskVolume[3];
                optionData.askv[4] = data.m_nAskVolume[4];
                optionData.bid[0] = Convert.ToDouble(data.m_nBidPrice[0]) / 10000;
                optionData.bid[1] = Convert.ToDouble(data.m_nBidPrice[1]) / 10000;
                optionData.bid[2] = Convert.ToDouble(data.m_nBidPrice[2]) / 10000;
                optionData.bid[3] = Convert.ToDouble(data.m_nBidPrice[3]) / 10000;
                optionData.bid[4] = Convert.ToDouble(data.m_nBidPrice[4]) / 10000;
                optionData.bidv[0] = data.m_nBidVolume[0];
                optionData.bidv[1] = data.m_nBidVolume[1];
                optionData.bidv[2] = data.m_nBidVolume[2];
                optionData.bidv[3] = data.m_nBidVolume[3];
                optionData.bidv[4] = data.m_nBidVolume[4];
                optionData.preClose = Convert.ToDouble(data.m_nPreClose) / 10000;
                optionData.preSettle = Convert.ToDouble(data.m_nPreSettle) / 10000;
                #endregion
                //在列表中插入数据。
                optionDataList.Add(optionData);
            }
            return optionDataList;
        }

        /// <summary>
        /// 从本地数据库中获取50etf的前收盘价。
        /// </summary>
        /// <param name="startDate">开始日期</param>
        /// <param name="endDate">结束日期</param>
        /// <returns>前收盘价列表。</returns>
        public Dictionary<int,double> GetETFPreCloseList(int startDate,int endDate)
        {
            Dictionary<int, double> myList = new Dictionary<int, double>();
            using (SqlConnection conn = new SqlConnection(connectString))
            {
                conn.Open();
                SqlCommand cmd = conn.CreateCommand();
                cmd.CommandText= "select distinct[Date],[PreClose] from [" + dataBaseName + "].[dbo].[" + tableOf50ETF + "] where [Date]>="+startDate.ToString()+" and [Date]<="+endDate.ToString()+" order by [Date]";
                try
                {
                    SqlDataReader reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        int date = reader.GetInt32(reader.GetOrdinal("Date"));
                        double close = reader.GetDouble(reader.GetOrdinal("PreClose"));
                        if (myList.ContainsKey(date)==true)
                        {
                            if (close>0)
                            {
                                myList[date] = close;
                            }
                        }
                        else
                        {
                            myList.Add(date, close);
                        }
                    }
                }
                catch (Exception)
                {
                    
                    throw;
                }
                conn.Close();
            }
            return myList;
        }

        /// <summary>
        /// 获取50etf数据的函数。
        /// </summary>
        public double[] GetETFData(int date)
        {
            double[] myPrice = new double[28800];
            using (SqlConnection conn = new SqlConnection(connectString))
            {
                conn.Open();
                SqlCommand cmd = conn.CreateCommand();
                cmd.CommandText = "select [Time],[Tick],[lastPrice] from ["+dataBaseName+"].[dbo].["+tableOf50ETF+"] where [Date]="+date.ToString()+" and [Time]>=92500000";
                try
                {
                    SqlDataReader reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        int time = reader.GetInt32(reader.GetOrdinal("Time")) + reader.GetInt32(reader.GetOrdinal("Tick")) * 500;
                        double price = reader.GetDouble(reader.GetOrdinal("LastPrice"));
                        int index =Math.Max(0, TradeDays.TimeToIndex(time));
                        if (index>=0 && index<28800)
                        {
                            myPrice[index] = price;
                        }
                    }
                }
                catch (Exception)
                {
                    throw;
                }
                conn.Close();
            }
            double lastPrice = myPrice[0];
            for (int i = 1; i < myPrice.Length; i++)
            {
                if (myPrice[i]==0)
                {
                    myPrice[i] = lastPrice;
                }
                else
                {
                    lastPrice = myPrice[i];
                }
            }
            return myPrice;
        }

        /// <summary>
        /// 修正从TDB数据库中获取的期权数据。
        /// </summary>
        /// <param name="today">日期</param>
        /// <param name="etfPreClose">etf前收盘价</param>
        /// <param name="optionList">原始的期权数据</param>
        /// <param name="etfPrice">当日etf价格</param>
        /// <returns>修正过的期权数据</returns>
        private List<optionDataFormat> ModifyOptionDataFromTDB(int today,double etfPreClose, List<optionDataFormat> optionList,double[] etfPrice)
        {
            List<optionDataFormat> modifiedList = new List<optionDataFormat>();
            double strike = optionList[0].strike;
            double preSettle = optionList[0].preSettle;
            int duration = TradeDays.GetTimeSpan(today, optionList[0].endDate);
            if (preSettle==0)
            {
                Console.WriteLine("preSettle wrong!");
            }
            for (int i = 0; i < optionList.Count;i++)
            {
                optionDataFormat option = optionList[i];
                int index = TradeDays.TimeToIndex(option.time+option.tick*500);
                //排除集合竞价的情况，其他需要排除的情况也可以添加。
                if (option.ask[0]==option.bid[0] || (option.askv[0]==0 || option.bidv[0]==0) || index<0 || index>=28800)
                {
                    continue;
                }
                //按每日分成28800个tick计算时间。
                double tickDuration = (double)duration + 1- index / 28800;
                //开始计算希腊值以及开仓保证金。
                option.midVolatility = Impv.sigma(etfPrice[index], (option.bid[0] + option.ask[0]) / 2, strike, tickDuration, Configuration.RiskFreeReturn, option.optionType);
                option.midDelta = Impv.optionDelta(etfPrice[index], option.midVolatility, strike, tickDuration, Configuration.RiskFreeReturn, option.optionType);
                option.openMargin = Impv.Margin(etfPreClose, preSettle, strike, option.optionType);
                option.underlyingPrice = etfPrice[index];
                for (int j = 0; j < 5; j++)
                {
                    option.askVolatility[j]= Impv.sigma(etfPrice[index], option.ask[j], strike, tickDuration, Configuration.RiskFreeReturn, option.optionType);
                    option.bidVolatility[j] = Impv.sigma(etfPrice[index], option.bid[j], strike, tickDuration, Configuration.RiskFreeReturn, option.optionType);
                    option.askDelta[j] = Impv.optionDelta(etfPrice[index], option.askVolatility[j], strike, tickDuration, Configuration.RiskFreeReturn, option.optionType);
                    option.bidDelta[j] = Impv.optionDelta(etfPrice[index], option.bidVolatility[j], strike, tickDuration, Configuration.RiskFreeReturn, option.optionType);
                }
                modifiedList.Add(option);
            }
            return modifiedList;
        }

        /// <summary>
        /// 读取期权特定日期的记录数。
        /// </summary>
        /// <param name="tableOfOption">期权表名称</param>
        /// <param name="code">期权代码</param>
        /// <param name="date">日期</param>
        /// <returns>当日记录条数</returns>
        public int CountDataOfOption(string tableOfOption,int code,int date)
        {
            using (SqlConnection conn=new SqlConnection(connectString))
            {
                conn.Open();//打开数据库  
                SqlCommand cmd = conn.CreateCommand();
                cmd.CommandText = "select COUNT(Date) from [" + dataBaseName + "].[dbo].[" + tableOfOption + "] where [Date]=" + date.ToString();
                try
                {

                    int number = (int)cmd.ExecuteScalar();
                    return number;
                }
                catch (Exception myerror)
                {
                    System.Console.WriteLine(myerror.Message);
                }
            }
            return 0;
        }
       
        
        /// <summary>
        /// 读取期权数据最后日期的函数。
        /// </summary>
        /// <param name="tableOfOption">表名称</param>
        /// <param name="code">期权代码</param>
        /// <returns>最后日期</returns>
        public int MaxDateOfOption(string tableOfOption, int code)
        {
            using (SqlConnection conn = new SqlConnection(connectString))
            {
                conn.Open();//打开数据库  
                SqlCommand cmd = conn.CreateCommand();
                cmd.CommandText = "select Max(Date) from [" + dataBaseName + "].[dbo].[" + tableOfOption + "]";
                try
                {
                    int number = (int)cmd.ExecuteScalar();
                    return number;
                }
                catch (Exception myerror)
                {
                    System.Console.WriteLine(myerror.Message);
                }
            }
            return 0;
        }

        /// <summary>
        /// 根据金融期货的名称记录当月合约和下月合约
        /// </summary>
        /// <param name="futureName">品种名称</param>
        /// <param name="tableFront">当月合约的表</param>
        /// <param name="tableNext">下月合约的表</param>
        public void StoreFinancialFuturesData(string futureName,string tableFront,string tableNext)
        {
            //判断当月以及下月的表是否存在，如果不存在建立表格。
            if (SqlApplication.CheckExist(dataBaseName, tableFront) == false)
            {
                Console.WriteLine("Crating table of {0}", tableFront);
                CreateTableOfFinancialFuture(tableFront);
            }
            if (SqlApplication.CheckExist(dataBaseName, tableNext) == false)
            {
                Console.WriteLine("Crating table of {0}", tableFront);
                CreateTableOfFinancialFuture(tableNext);
            }
            string frontFuture;
            string nextFuture;
            foreach (int today in myTradeDays.myTradeDays)
            {
                //必要的日期辨认，根据当日的日期得到对应的当月合约和下月合约。
                DateTime thisMonth = TradeDays.IntToDateTime(today);
                DateTime nextMonth = DateTime.Parse(thisMonth.ToString("yyyy-MM-01")).AddMonths(1);
                DateTime nextTwoMonth=DateTime.Parse(nextMonth.ToString("yyyy-MM-01")).AddMonths(1);
                if (today<=TradeDays.ThirdFridayList[thisMonth.Year*100+thisMonth.Month] && (futureName=="if" || today>=20150501)) 
                    //对IH,IF来说，201504的当月合约才IH1505,下月合约为IH1506
                {
                    frontFuture = futureName + ((thisMonth.Year % 100) * 100 + thisMonth.Month).ToString() + ".CF";
                    nextFuture = futureName + ((nextMonth.Year % 100) * 100 + nextMonth.Month).ToString() + ".CF";
                }
                else
                {
                    frontFuture = futureName + ((nextMonth.Year % 100)*100 + nextMonth.Month).ToString() + ".CF";
                    nextFuture = futureName + ((nextTwoMonth.Year % 100)*100 + nextTwoMonth.Month).ToString() + ".CF";
                }
                //Console.WriteLine("Insert Date:{0}, Date:{1},{2}", today, frontFuture, nextFuture);
                //记录当月合约
                InsertFinancialFutureDate(frontFuture, tableFront, today);
                //记录下月合约
                InsertFinancialFutureDate(nextFuture, tableNext, today);

            }
        }

        /// <summary>
        /// 记录金融期货数据到本地。
        /// </summary>
        /// <param name="futureName">期货的合约代码</param>
        public void StoreFinancialFuturesData(string futureName)
        {
            string frontFuture;
            string nextFuture;
            string frontTable;
            string nextTable;
            foreach (int today in myTradeDays.myTradeDays)
            {
                //必要的日期辨认，根据当日的日期得到对应的当月合约和下月合约。
                DateTime thisMonth = TradeDays.IntToDateTime(today);
                DateTime nextMonth = DateTime.Parse(thisMonth.ToString("yyyy-MM-01")).AddMonths(1);
                DateTime nextTwoMonth = DateTime.Parse(nextMonth.ToString("yyyy-MM-01")).AddMonths(1);
                if (today <= TradeDays.ThirdFridayList[thisMonth.Year * 100 + thisMonth.Month] && (futureName == "if" || today >= 20150501))
                //对IH,IF来说，201504的当月合约才IH1505,下月合约为IH1506
                {
                    frontFuture = futureName + ((thisMonth.Year % 100) * 100 + thisMonth.Month).ToString() + ".CF";
                    nextFuture = futureName + ((nextMonth.Year % 100) * 100 + nextMonth.Month).ToString() + ".CF";
                }
                else
                {
                    frontFuture = futureName + ((nextMonth.Year % 100) * 100 + nextMonth.Month).ToString() + ".CF";
                    nextFuture = futureName + ((nextTwoMonth.Year % 100) * 100 + nextTwoMonth.Month).ToString() + ".CF";
                }
                frontTable = frontFuture.Substring(0, 6);
                nextTable = nextFuture.Substring(0, 6);
                //判断当月以及下月的表是否存在，如果不存在建立表格。
                if (SqlApplication.CheckExist(dataBaseName, frontTable) == false)
                {
                    Console.WriteLine("Crating table of {0}", frontTable);
                    CreateTableOfFinancialFuture(frontTable);
                }
                if (SqlApplication.CheckExist(dataBaseName, nextTable) == false)
                {
                    Console.WriteLine("Crating table of {0}", nextTable);
                    CreateTableOfFinancialFuture(nextTable);
                }
                //记录当月合约
                InsertFinancialFutureDate(frontFuture, frontTable, today);
                //记录下月合约
                InsertFinancialFutureDate(nextFuture, nextTable, today);

            }
        }

    }
}
