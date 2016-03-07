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
        private string connectString = Configuration.connectString;
        private string dataBaseName = Configuration.dataBaseName;
        private string tableOf50ETF = Configuration.tableOf50ETF;

        /// <summary>
        /// 需要获取数据的名称类型等。
        /// </summary>
        public TDBdataInformation myData = new TDBdataInformation();
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
                cmd.CommandText = "create table ["+dataBaseName+ "].[dbo].[" + tableOf50ETF + "] (Code int,Market char(4),[Date] int not null,[Time] int not null, [Tick] int not null,[Volume] float,[Turnover] float,[AccVolume] float,[AccTurnover] float,[Open] float,[High] float,[Low] float,[LastPrice] float,Ask1 float,Ask2 float,Ask3 float,Ask4 float,Ask5 float,Askv1 float,Askv2 float,Askv3 float,Askv4 float,Askv5 float,Bid1 float,Bid2 float,Bid3 float,Bid4 float,Bid5 float,Bidv1 float,Bidv2 float,Bidv3 float,Bidv4 float,Bidv5 float,[PreClose] float,primary key ([Date],[Time],[Tick]))";
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
        public WindTDBData(string codeName, int startDate, int endDate, string type)
        {
            myData.codeName = codeName;
            myData.startDate = startDate;
            myData.endDate = endDate;
            myData.type = type;
            if (type == "option" || type == "50etf" || type =="ih" || type=="if" || type=="ic" || type=="sh")
            {
                mySource = Configuration.SHsource;
            }
            else if (type=="commodity")
            {
                mySource = Configuration.commoditySource;
            }
            if (CheckConnection())
            {
                Console.WriteLine("Connect Success!");
                myTradeDays = new TradeDays(myData.startDate, myData.endDate);
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
                        StoreOptionData(myData.startDate, myData.endDate);
                        break;
                    default:
                        break;
                }
            }
            else
            {
                Console.WriteLine("Please Input Valid Parameters!");
            }
        }


        /// <summary>
        /// 判断TDB数据库是否连接成功。
        /// </summary>
        /// <returns>返回是否连接成功。</returns>
        public bool CheckConnection()
        {
            TDBDataSource tdbSource = new TDBDataSource(mySource.IP,mySource.port,mySource.account,mySource.password);
            TDBLoginResult loginRes;
            TDBErrNo nErr = tdbSource.Connect(out loginRes);
            //输出登陆结果
            if (nErr == TDBErrNo.TDB_OPEN_FAILED)
            {
                Console.WriteLine("open failed, reason:{0}", loginRes.m_strInfo);
                Console.WriteLine();
                return false;
            }
            //关闭连接
            tdbSource.DisConnect();
            return true;
        }

        /// <summary>
        /// 存储50etf数据的函数。
        /// </summary>
        public void Store50ETFData()
        {
            TDBDataSource tdbSource = new TDBDataSource(mySource.IP, mySource.port, mySource.account, mySource.password);
            TDBLoginResult loginRes;
            TDBErrNo nErr = tdbSource.Connect(out loginRes);
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
                            TDBTickAB mydata = tickArr[i];
                            if (mydata.m_nDate == lastDate && mydata.m_nTime == lastTime)
                            {
                                lastTick += 1;
                            }
                            else
                            {
                                lastTick = 0;
                                lastDate = mydata.m_nDate;
                                lastTime = mydata.m_nTime;
                            }
                            #region 将数据写入每一行中。
                            DataRow r = todayData.NewRow();
                            r["Code"] = Convert.ToInt32(mydata.m_strWindCode.Substring(0, 6));
                            r["Market"] = mydata.m_strWindCode.Substring(7, 2);
                            r["Date"] = mydata.m_nDate;
                            r["Time"] = mydata.m_nTime;
                            r["Tick"] = lastTick;
                            r["Volume"] = mydata.m_iVolume;
                            r["Turnover"] = mydata.m_iTurover;
                            r["AccVolume"] = mydata.m_iAccVolume;
                            r["AccTurnover"] = mydata.m_iAccTurover;
                            r["Open"] = Convert.ToDouble(mydata.m_nOpen) / 10000;
                            r["High"] = Convert.ToDouble(mydata.m_nHigh) / 10000;
                            r["Low"] = Convert.ToDouble(mydata.m_nLow) / 10000;
                            r["LastPrice"] = Convert.ToDouble(mydata.m_nPrice) / 10000;
                            r["Ask1"] = Convert.ToDouble(mydata.m_nAskPrice[0]) / 10000;
                            r["Ask2"] = Convert.ToDouble(mydata.m_nAskPrice[1]) / 10000;
                            r["Ask3"] = Convert.ToDouble(mydata.m_nAskPrice[2]) / 10000;
                            r["Ask4"] = Convert.ToDouble(mydata.m_nAskPrice[3]) / 10000;
                            r["Ask5"] = Convert.ToDouble(mydata.m_nAskPrice[4]) / 10000;
                            r["Askv1"] = mydata.m_nAskVolume[0];
                            r["Askv2"] = mydata.m_nAskVolume[1];
                            r["Askv3"] = mydata.m_nAskVolume[2];
                            r["Askv4"] = mydata.m_nAskVolume[3];
                            r["Askv5"] = mydata.m_nAskVolume[4];
                            r["Bid1"] = Convert.ToDouble(mydata.m_nBidPrice[0]) / 10000;
                            r["Bid2"] = Convert.ToDouble(mydata.m_nBidPrice[1]) / 10000;
                            r["Bid3"] = Convert.ToDouble(mydata.m_nBidPrice[2]) / 10000;
                            r["Bid4"] = Convert.ToDouble(mydata.m_nBidPrice[3]) / 10000;
                            r["Bid5"] = Convert.ToDouble(mydata.m_nBidPrice[4]) / 10000;
                            r["Bidv1"] = mydata.m_nBidVolume[0];
                            r["Bidv2"] = mydata.m_nBidVolume[1];
                            r["Bidv3"] = mydata.m_nBidVolume[2];
                            r["Bidv4"] = mydata.m_nBidVolume[3];
                            r["Bidv5"] = mydata.m_nBidVolume[4];
                            r["PreClose"] = Convert.ToDouble(mydata.m_nPreClose) / 10000;
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
                
            //关闭连接
            tdbSource.DisConnect();

        }

        public void StoreOptionData(int startDate,int endDate)
        {
            OptionInformation myOptionInfo = new OptionInformation(startDate, endDate);
            foreach (int today in myTradeDays.myTradeDays)
            {
                //未完待续，需要存储50etf期权数据以及其对应的希腊字母。
                //====================================================
                //====================================================
                //====================================================
                //====================================================
                //====================================================
            }
        }
        /// <summary>
        /// 获取期权数据的函数。
        /// </summary>
        public void GetOptionData()
        {
            //获取50etf期权信息。
            OptionInformation myOptionInfo = new OptionInformation(myData.startDate,myData.endDate);
            

        }


        /// <summary>
        /// 获取IH期货数据的函数。
        /// </summary>
        public void GetIHData()
        {

        }

    }
}
