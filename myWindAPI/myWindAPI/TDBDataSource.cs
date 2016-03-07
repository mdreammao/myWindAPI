using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Diagnostics;
using TDBAPIImp;


namespace TDBAPI
{
    //错误代码
    public enum TDBErrNo
    {
        TDB_SUCCESS = 0,

        TDB_NETWORK_ERROR = -1,       //网络错误
        TDB_NETWORK_TIMEOUT = -2,     //网络超时
        TDB_NO_DATA = -3,           //没有数据
        TDB_OUT_OF_MEMORY = -4,       //内存耗尽
        TDB_LOGIN_FAILED = -5,      //登陆失败
        TDB_INVALID_PARAMS = -11,     //无效的参数
        TDB_INVALID_CODE_TYPE,      //无效的代码类型，比如向TDB_GetFuture传入非期货类型代码，返回之。

        TDB_WRONG_FORMULA = -50,      //指标公式错误        

        //Connect 返回值
        TDB_OPEN_FAILED = -100, //打开连接失败
    }
    
    //登陆结果
    public class TDBLoginResult : ICloneable
    {
        public string m_strInfo;     //登陆信息
        public string[] m_strMarkets; //授权市场
        public int[] m_nDynDates;    //动态日期

        public object Clone()
        {
            TDBLoginResult res = new TDBLoginResult();
            res.m_strInfo = m_strInfo;
            if (m_strMarkets.Length > 0)
            {
                res.m_strMarkets = new string[m_strMarkets.Length];
                System.Array.Copy(m_strMarkets, res.m_strMarkets, m_strMarkets.Length);
            }
            else
            {
                res.m_strMarkets = new string[0];
            }

            if (m_nDynDates.Length > 0)
            {
                res.m_nDynDates = new int[m_nDynDates.Length];
                System.Array.Copy(m_nDynDates, res.m_nDynDates, m_nDynDates.Length);
            }
            else
            {
                res.m_nDynDates = new int[0];
            }
            return res;
        }

        public void FromAPILoginResult(LibTDBWrap.TDBDefine_ResLogin loginRes)
        {
            m_strInfo = LibWrapHelper.AnsiArr2String(loginRes.szInfo, 0, loginRes.szInfo.Length);

            if (loginRes.nMarkets > 0)
            {
                m_nDynDates = new int[loginRes.nMarkets];
                Debug.Assert(loginRes.nDynDate.Length >= loginRes.nMarkets);

                System.Array.Copy(loginRes.nDynDate, m_nDynDates, loginRes.nMarkets);

                m_strMarkets = new string[loginRes.nMarkets];

                Debug.Assert(loginRes.szMarket.Length >= 8 * loginRes.nMarkets);
                for (int i = 0; i < loginRes.nMarkets; i++)
                {
                    m_strMarkets[i] = LibWrapHelper.AnsiArr2String(loginRes.szMarket, i * 8, 8);
                }
            }
            else
            {
                m_nDynDates = new int[0];
                m_strMarkets = new string[0];
            }
        }
    }

    //复权标志
    public enum TDBRefillFlag
    {
        REFILL_NONE = LibTDBWrap.REFILLFLAG.REFILL_NONE,   //不复权
        REFILL_BACKWARD = LibTDBWrap.REFILLFLAG.REFILL_BACKWARD, //向前复权
        REFILL_FORWARD = LibTDBWrap.REFILLFLAG.REFILL_FORWARD, //向后复权
    }
    //k线类别
    public enum TDBCycType
    {
        CYC_SECOND = LibTDBWrap.CYCTYPE.CYC_SECOND,    //秒线
        CYC_MINUTE = LibTDBWrap.CYCTYPE.CYC_MINUTE,   //分钟线
        CYC_DAY = LibTDBWrap.CYCTYPE.CYC_DAY,          //日线
        CYC_WEEK = LibTDBWrap.CYCTYPE.CYC_WEEK,         //周线
        CYC_MONTH = LibTDBWrap.CYCTYPE.CYC_MONTH,       //月线
        CYC_SEASON = LibTDBWrap.CYCTYPE.CYC_SEASON,     //季线
        CYC_HAFLYEAR = LibTDBWrap.CYCTYPE.CYC_HAFLYEAR, //半年
        CYC_YEAR = LibTDBWrap.CYCTYPE.CYC_YEAR          //年线
    }
    //请求K线
    public class TDBReqKLine
    {
        public string m_strWindCode; //证券万得代码(AG1312.SHF)
        public TDBRefillFlag m_nCQFlag;//除权标志：不复权，向前复权，向后复权
        public int m_nCQDate;       //复权日期(<=0:全程复权) 格式：YYMMDD，例如20130101表示2013年1月1日
        public int m_nQJFlag;   //全价标志(债券)(0:净价 1:全价)
        public TDBCycType m_nCycType;//数据周期：秒线、分钟、日线、周线、月线、季线、半年线、年线
        public int m_nCycDef; //周期数量：仅当nCycType取值：秒、分钟、日线、周线、月线时，这个字段有效。
        public int m_nAutoComplete;   //自动补齐：仅1秒钟线、1分钟线支持这个标志，（不为0：补齐；0：不补齐）
        public int m_nBeginDate;             //开始日期(交易日，<0:从上市日期开始； 0:从今天开始)
        public int m_nEndDate;              //结束日期(交易日，<=0:跟nBeginDate一样) 

        public LibTDBWrap.TDBDefine_ReqKLine ToAPIReqKLine()
        {
            LibTDBWrap.TDBDefine_ReqKLine reqAPIKLine = new LibTDBWrap.TDBDefine_ReqKLine();
            reqAPIKLine.chCode = LibWrapHelper.String2AnsiArr(m_strWindCode, 32);
            reqAPIKLine.nCQFlag = (int)m_nCQFlag;
            reqAPIKLine.nCQDate = m_nCQDate;
            reqAPIKLine.nQJFlag = m_nQJFlag;
            reqAPIKLine.nCycType = (int)m_nCycType;
            reqAPIKLine.nCycDef = m_nCycDef;
            reqAPIKLine.nAutoComplete = m_nAutoComplete;
            reqAPIKLine.nBeginDate = m_nBeginDate;
            reqAPIKLine.nBeginTime = 0;
            reqAPIKLine.nEndDate = m_nEndDate;
            reqAPIKLine.nEndTime = 0;
            return reqAPIKLine;
        }
    }
    //K线结果
    public class TDBKLine
    {
        public string m_strWindCode;        //万得代码(AG1312.SHF)
        public string m_strCode;            //交易所代码(ag1312)
        public int m_nDate;                 //日期（自然日）格式：YYMMDD，例如20130101表示2013年1月1日，0表示今天
        public int m_nTime;                 //时间（HHMMSSmmm）例如94500000 表示 9点45分00秒000毫秒
        public int m_nOpen;                 //开盘((a double number + 0.00005) *10000)
        public int m_nHigh;                 //最高((a double number + 0.00005) *10000)
        public int m_nLow;                  //最低((a double number + 0.00005) *10000)
        public int m_nClose;                //收盘((a double number + 0.00005) *10000)
        public Int64 m_iVolume;             //成交量
        public Int64 m_iTurover;            //成交额(元)
        public int m_nMatchItems;           //成交笔数
        public int m_nInterest;             //持仓量(期货)、IOPV(基金)、利息(债券)

        public void FromAPIKLine(ref LibTDBWrap.TDBDefine_KLine apiKLine)
        {
            m_strWindCode = LibWrapHelper.AnsiArr2String(apiKLine.chWindCode, 0, apiKLine.chWindCode.Length);
            m_strCode = LibWrapHelper.AnsiArr2String(apiKLine.chCode, 0, apiKLine.chCode.Length);
            m_nDate = apiKLine.nDate;
            m_nTime = apiKLine.nTime;
            m_nOpen = apiKLine.nOpen;
            m_nHigh = apiKLine.nHigh;
            m_nLow = apiKLine.nLow;
            m_nClose = apiKLine.nClose;
            m_iVolume = apiKLine.iVolume;
            m_iTurover = apiKLine.iTurover;
            m_nMatchItems = apiKLine.nMatchItems;
            m_nInterest = apiKLine.nInterest;
        }
    }

    //通用请求结构体
    public class TDBReq
    {
        public string m_strWindCode;//证券万得代码(AG1312.SHF)
        public int m_nBeginDate;       //开始日期（交易日）,为0则从今天，格式：YYMMDD，例如20130101表示2013年1月1日
        public int m_nEndDate;      //结束日期（交易日），为0则和nBeginDate一样

        public TDBReq(string strWindCode, int nBeginDate, int nEndDate = 0)
        {
            m_strWindCode = strWindCode;
            m_nBeginDate = nBeginDate;
            m_nEndDate = nEndDate;
        }

        public LibTDBWrap.TDBDefine_ReqTick ToAPIReqTick()
        {
            LibTDBWrap.TDBDefine_ReqTick reqAPITick = new LibTDBWrap.TDBDefine_ReqTick();
            reqAPITick.chCode = LibWrapHelper.String2AnsiArr(m_strWindCode, 32);
            reqAPITick.nBeginDate = m_nBeginDate;
            reqAPITick.nEndDate = m_nEndDate;
            reqAPITick.nBeginTime = 0;
            reqAPITick.nEndTime = 0;
            return reqAPITick;
        }
        public LibTDBWrap.TDBDefine_ReqTransaction ToAPIReqTransaction()
        {
            LibTDBWrap.TDBDefine_ReqTransaction reqTransaction = new LibTDBWrap.TDBDefine_ReqTransaction();
            reqTransaction.chCode = LibWrapHelper.String2AnsiArr(m_strWindCode, 32);
            reqTransaction.nBeginDate = m_nBeginDate;
            reqTransaction.nEndDate = m_nEndDate;
            reqTransaction.nBeginTime = 0;
            reqTransaction.nEndTime = 0;
            return reqTransaction;
        }
    }

    public class TDBTick
    {
        public string m_strWindCode;//万得代码(AG1312.SHF)
        public string m_strCode;//交易所代码(ag1312)
        public int m_nDate;     //日期（自然日）
        public int m_nTime;     //时间（HHMMSSmmm）例如94500000 表示 9点45分00秒000毫秒
        public int m_nPrice;                         //成交价((a double number + 0.00005) *10000)

        public long m_iVolume;                    //成交量
        public long m_iTurover;                //成交额(元)
        public int m_nMatchItems;                    //成交笔数
        public int m_nInterest;                      //IOPV(基金)、利息(债券)
        public char m_chTradeFlag;                   //成交标志
        public char m_chBSFlag;                      //BS标志
        public long m_iAccVolume;                 //当日累计成交量
        public long m_iAccTurover;             //当日成交额(元)
        public int m_nHigh;                          //最高((a double number + 0.00005) *10000)
        public int m_nLow;                           //最低((a double number + 0.00005) *10000)
        public int m_nOpen;                       //开盘((a double number + 0.00005) *10000)
        public int m_nPreClose;                   //前收盘((a double number + 0.00005) *10000)

        //下面的字段指数使用
        public int m_nIndex;                  //不加权指数
        public int m_nStocks;                 //品种总数
        public int m_nUps;                    //上涨品种数
        public int m_nDowns;                  //下跌品种数
        public int m_nHoldLines;              //持平品种数

        public void FromAPITick(ref LibTDBWrap.TDBDefine_Tick apiTick)
        {
            m_strWindCode = LibWrapHelper.AnsiArr2String(apiTick.chWindCode, 0, apiTick.chWindCode.Length);
            m_strCode = LibWrapHelper.AnsiArr2String(apiTick.chCode, 0, apiTick.chCode.Length);
            m_nDate = apiTick.nDate;
            m_nTime = apiTick.nTime;
            m_nPrice = apiTick.nPrice;
            m_iVolume = apiTick.iVolume;
            m_iTurover = apiTick.iTurover;
            m_nMatchItems = apiTick.nMatchItems;
            m_nInterest = apiTick.nInterest;
            m_chTradeFlag = (char)apiTick.chTradeFlag;
            m_chBSFlag = (char)apiTick.chBSFlag;
            m_iAccVolume = apiTick.iAccVolume;
            m_iAccTurover = apiTick.iAccTurover;
            m_nHigh = apiTick.nHigh;
            m_nLow = apiTick.nLow;
            m_nOpen = apiTick.nOpen;
            m_nPreClose = apiTick.nPreClose;

            m_nIndex = apiTick.nIndex;
            m_nStocks = apiTick.nStocks;
            m_nUps = apiTick.nUps;
            m_nDowns = apiTick.nDowns;
            m_nHoldLines = apiTick.nHoldLines;
        }
    }

    public class TDBTickAB
    {
        public string m_strWindCode;//万得代码(AG1312.SHF)
        public string m_strCode;//交易所代码(ag1312)
        public int m_nDate;     //日期（自然日）
        public int m_nTime;     //时间（HHMMSSmmm）例如94500000 表示 9点45分00秒000毫秒
        public int m_nPrice;                         //成交价((a double number + 0.00005) *10000)

        public long m_iVolume;                    //成交量
        public long m_iTurover;                //成交额(元)
        public int m_nMatchItems;                    //成交笔数
        public int m_nInterest;                      //IOPV(基金)、利息(债券)
        public char m_chTradeFlag;                   //成交标志
        public char m_chBSFlag;                      //BS标志
        public long m_iAccVolume;                 //当日累计成交量
        public long m_iAccTurover;             //当日成交额(元)
        public int m_nHigh;                          //最高((a double number + 0.00005) *10000)
        public int m_nLow;                           //最低((a double number + 0.00005) *10000)
        public int m_nOpen;                       //开盘((a double number + 0.00005) *10000)
        public int m_nPreClose;                   //前收盘((a double number + 0.00005) *10000)

        public int[] m_nAskPrice;               //叫卖价((a double number + 0.00005) *10000)
        public int[] m_nAskVolume;              //叫卖量
        public int[] m_nBidPrice;               //叫买价((a double number + 0.00005) *10000)
        public int[] m_nBidVolume;              //叫买量
        public int m_nAskAvPrice;               //加权平均叫卖价(上海L2)((a double number + 0.00005) *10000)
        public int m_nBidAvPrice;                 //加权平均叫买价(上海L2)((a double number + 0.00005) *10000)
        public long m_iTotalAskVolume;           //叫卖总量(上海L2)
        public long m_iTotalBidVolume;           //叫买总量(上海L2)

        //下面的字段指数使用
        public int m_nIndex;                  //不加权指数
        public int m_nStocks;                 //品种总数
        public int m_nUps;                    //上涨品种数
        public int m_nDowns;                  //下跌品种数
        public int m_nHoldLines;              //持平品种数

        public void FromAPITick(ref LibTDBWrap.TDBDefine_TickAB apiTick)
        {
            m_strWindCode = LibWrapHelper.AnsiArr2String(apiTick.chWindCode, 0, apiTick.chWindCode.Length);
            m_strCode = LibWrapHelper.AnsiArr2String(apiTick.chCode, 0, apiTick.chCode.Length);
            m_nDate = apiTick.nDate;
            m_nTime = apiTick.nTime;
            m_nPrice = apiTick.nPrice;
            m_iVolume = apiTick.iVolume;
            m_iTurover = apiTick.iTurover;
            m_nMatchItems = apiTick.nMatchItems;
            m_nInterest = apiTick.nInterest;
            m_chTradeFlag = (char)apiTick.chTradeFlag;
            m_chBSFlag = (char)apiTick.chBSFlag;
            m_iAccVolume = apiTick.iAccVolume;
            m_iAccTurover = apiTick.iAccTurover;
            m_nHigh = apiTick.nHigh;
            m_nLow = apiTick.nLow;
            m_nOpen = apiTick.nOpen;
            m_nPreClose = apiTick.nPreClose;

            m_nAskPrice = LibWrapHelper.CopyIntArr(apiTick.nAskPrice);
            m_nAskVolume = LibWrapHelper.CopyIntArr(apiTick.nAskVolume);
            m_nBidPrice = LibWrapHelper.CopyIntArr(apiTick.nBidPrice);
            m_nBidVolume = LibWrapHelper.CopyIntArr(apiTick.nBidVolume);
            m_nAskAvPrice = apiTick.nAskAvPrice;
            m_nBidAvPrice = apiTick.nBidAvPrice;
            m_iTotalAskVolume = apiTick.iTotalAskVolume;
            m_iTotalBidVolume = apiTick.iTotalBidVolume;

            m_nIndex = apiTick.nIndex;
            m_nStocks = apiTick.nStocks;
            m_nUps = apiTick.nUps;
            m_nDowns = apiTick.nDowns;
            m_nHoldLines = apiTick.nHoldLines;
        }
    }

    //请求期货结构体
    public class TDBReqFuture
    {
        public string m_strWindCode; //证券万得代码(AG1312.SHF)
        public int m_nBeginDate;     //开始日期（交易日）,为0则从今天，格式：YYMMDD，例如20130101表示2013年1月1日
        public int m_nEndDate;      //结束日期（交易日），为0则和nBeginDate一样

        public int m_nAutoComplete;  //自动补齐标志:( 0：不自动补齐，1:自动补齐）

        public TDBReqFuture(string strWindCode, int nBeginDate, int nEndDate = 0, int nAutoComplete = 0)
        {
            m_strWindCode = strWindCode;
            m_nBeginDate = nBeginDate;
            m_nEndDate = nEndDate;
            m_nAutoComplete = nAutoComplete;
        }

        public LibTDBWrap.TDBDefine_ReqFuture ToAPIReqFuture()
        {
            LibTDBWrap.TDBDefine_ReqFuture reqAPIFuture = new LibTDBWrap.TDBDefine_ReqFuture();
            reqAPIFuture.chCode = LibWrapHelper.String2AnsiArr(m_strWindCode, 32);
            reqAPIFuture.nBeginDate = m_nBeginDate;
            reqAPIFuture.nEndDate = m_nEndDate;
            reqAPIFuture.nBeginTime = 0;
            reqAPIFuture.nEndTime = 0;
            reqAPIFuture.nAutoComplete = m_nAutoComplete;
            return reqAPIFuture;
        }
    }

    public class TDBFuture
    {
        public string m_strWindCode;               //万得代码(AG1312.SHF)
        public string m_strCode;                   //交易所代码(ag1312)

        public int m_nDate;                         //日期（自然日）格式：YYMMDD
        public int m_nTime;                         //时间，格式：HHMMSSmmm

        public long m_iVolume;                   //成交量
        public long m_iTurover;               //成交额(元)

        public int m_nSettle;                       //结算价((a double number + 0.00005) *10000)
        public int m_nPosition;                     //持仓量
        public int m_nCurDelta;                     //虚实度

        public char m_chTradeFlag;                  //成交标志（港股有值）

        public long m_iAccVolume;                //当日累计成交量
        public long m_iAccTurover;            //当日成交额(元)

        public int m_nHigh;                         //最高((a double number + 0.00005) *10000)
        public int m_nLow;                          //最低((a double number + 0.00005) *10000)
        public int m_nOpen;                         //开盘((a double number + 0.00005) *10000)
        public int m_nPrice;                        //成交价((a double number + 0.00005) *10000)

        public int m_nPreClose;                     //前收盘((a double number + 0.00005) *10000)
        public int m_nPreSettle;                    //昨结算((a double number + 0.00005) *10000)
        public int m_nPrePosition;                  //昨持仓

        public void FromAPIFuture(ref LibTDBWrap.TDBDefine_Future apiFuture)
        {
            m_strWindCode = LibWrapHelper.AnsiArr2String(apiFuture.chWindCode, 0, apiFuture.chWindCode.Length);
            m_strCode = LibWrapHelper.AnsiArr2String(apiFuture.chCode, 0, apiFuture.chCode.Length);
            m_nDate = apiFuture.nDate;
            m_nTime = apiFuture.nTime;
            m_iVolume = apiFuture.iVolume;
            m_iTurover = apiFuture.iTurover;

            m_nSettle = apiFuture.nSettle;
            m_nPosition = apiFuture.nPosition;
            m_nCurDelta = apiFuture.nCurDelta;

            m_chTradeFlag = (char)apiFuture.chTradeFlag;

            m_iAccVolume = apiFuture.iAccVolume;
            m_iAccTurover = apiFuture.iAccTurover;

            m_nHigh = apiFuture.nHigh;
            m_nLow = apiFuture.nLow;
            m_nOpen = apiFuture.nOpen;
            m_nPrice = apiFuture.nPrice;

            m_nPreClose = apiFuture.nPreClose;
            m_nPreSettle = apiFuture.nPreSettle;
            m_nPrePosition = apiFuture.nPrePosition;
        }
    }

    public class TDBFutureAB
    {
        public string m_strWindCode;               //万得代码(AG1312.SHF)
        public string m_strCode;                   //交易所代码(ag1312)

        public int m_nDate;                         //日期（自然日）格式：YYMMDD
        public int m_nTime;                         //时间，格式：HHMMSSmmm

        public long m_iVolume;                   //成交量
        public long m_iTurover;                 //成交额(元)

        public int m_nSettle;                       //结算价((a double number + 0.00005) *10000)
        public int m_nPosition;                     //持仓量
        public int m_nCurDelta;                     //虚实度

        public char m_chTradeFlag;                  //成交标志（港股有值）

        public long m_iAccVolume;                //当日累计成交量
        public long m_iAccTurover;            //当日成交额(元)

        public int m_nHigh;                         //最高((a double number + 0.00005) *10000)
        public int m_nLow;                          //最低((a double number + 0.00005) *10000)
        public int m_nOpen;                         //开盘((a double number + 0.00005) *10000)
        public int m_nPrice;                        //成交价((a double number + 0.00005) *10000)

        public int[] m_nAskPrice;               //叫卖价((a double number + 0.00005) *10000)
        public int[] m_nAskVolume;               //叫卖量
        public int[] m_nBidPrice;               //叫买价((a double number + 0.00005) *10000)
        public int[] m_nBidVolume;               //叫买量

        public int m_nPreClose;                     //前收盘((a double number + 0.00005) *10000)
        public int m_nPreSettle;                    //昨结算((a double number + 0.00005) *10000)
        public int m_nPrePosition;                  //昨持仓

        public void FromAPIFutureAB(ref LibTDBWrap.TDBDefine_FutureAB apiFutureAB)
        {
            m_strWindCode = LibWrapHelper.AnsiArr2String(apiFutureAB.chWindCode, 0, apiFutureAB.chWindCode.Length);
            m_strCode = LibWrapHelper.AnsiArr2String(apiFutureAB.chCode, 0, apiFutureAB.chCode.Length);
            m_nDate = apiFutureAB.nDate;
            m_nTime = apiFutureAB.nTime;
            m_iVolume = apiFutureAB.iVolume;
            m_iTurover = apiFutureAB.iTurover;

            m_nSettle = apiFutureAB.nSettle;
            m_nPosition = apiFutureAB.nPosition;
            m_nCurDelta = apiFutureAB.nCurDelta;

            m_chTradeFlag = (char)apiFutureAB.chTradeFlag;

            m_iAccVolume = apiFutureAB.iAccVolume;
            m_iAccTurover = apiFutureAB.iAccTurover;

            m_nHigh = apiFutureAB.nHigh;
            m_nLow = apiFutureAB.nLow;
            m_nOpen = apiFutureAB.nOpen;
            m_nPrice = apiFutureAB.nPrice;

            m_nAskPrice = LibWrapHelper.CopyIntArr(apiFutureAB.nAskPrice);
            m_nAskVolume = LibWrapHelper.CopyIntArr(apiFutureAB.nAskVolume);
            m_nBidPrice = LibWrapHelper.CopyIntArr(apiFutureAB.nBidPrice);
            m_nBidVolume = LibWrapHelper.CopyIntArr(apiFutureAB.nBidVolume);

            m_nPreClose = apiFutureAB.nPreClose;
            m_nPreSettle = apiFutureAB.nPreSettle;
            m_nPrePosition = apiFutureAB.nPrePosition;
        }
    }

    public class TDBTransaction
    {
        public string m_strWindCode;     //万得代码(AG1312.SHF)
        public string m_strCode;         //交易所代码(ag1312)
        public int m_nDate;              //日期（自然日）格式:YYMMDD
        public int m_nTime;              //成交时间(精确到毫秒HHMMSSmmm)
        public int m_nIndex;             //成交编号(从1开始，递增1)
        public char m_chFunctionCode;     //成交代码: 'C', 0
        public char m_chOrderKind;        //委托类别
        public char m_chBSFlag;           //BS标志
        public int m_nTradePrice;        //成交价格((a double number + 0.00005) *10000)
        public int m_nTradeVolume;       //成交数量
        public int m_nAskOrder;          //叫卖序号
        public int m_nBidOrder;          //叫买序号

        public void FromAPITransaction(ref LibTDBWrap.TDBDefine_Transaction apiTransaction)
        {
            m_strWindCode = LibWrapHelper.AnsiArr2String(apiTransaction.chWindCode, 0, apiTransaction.chWindCode.Length);
            m_strCode = LibWrapHelper.AnsiArr2String(apiTransaction.chCode, 0, apiTransaction.chCode.Length);
            m_nDate = apiTransaction.nDate;
            m_nTime = apiTransaction.nTime;
            m_nIndex = apiTransaction.nIndex;
            m_chFunctionCode = (char)apiTransaction.chFunctionCode;
            m_chOrderKind = (char)apiTransaction.chOrderKind;
            m_chBSFlag = (char)apiTransaction.chBSFlag;
            m_nTradePrice = apiTransaction.nTradePrice;
            m_nTradeVolume = apiTransaction.nTradeVolume;
            m_nAskOrder = apiTransaction.nAskOrder;
            m_nBidOrder = apiTransaction.nBidOrder;
        }
    }
    public class TDBOrder
    {
        public string m_strWindCode;        //万得代码(AG1312.SHF)
        public string m_strCode;            //交易所代码(ag1312)
        public int m_nDate;                 //日期（自然日）格式YYMMDD
        public int m_nTime;                 //委托时间（精确到毫秒HHMMSSmmm）
        public int m_nIndex;                //委托编号，从1开始，递增1
        public int m_nOrder;                //交易所委托号
        public char m_chOrderKind;           //委托类别
        public char m_chFunctionCode;        //委托代码, B, S, C
        public int m_nOrderPrice;           //委托价格((a double number + 0.00005) *10000)
        public int m_nOrderVolume;          //委托数量

        public void FromAPIOrder(ref LibTDBWrap.TDBDefine_Order apiOrder)
        {
            m_strWindCode = LibWrapHelper.AnsiArr2String(apiOrder.chWindCode, 0, apiOrder.chWindCode.Length);
            m_strCode = LibWrapHelper.AnsiArr2String(apiOrder.chCode, 0, apiOrder.chCode.Length);
            m_nDate = apiOrder.nDate;
            m_nTime = apiOrder.nTime;
            m_nIndex = apiOrder.nIndex;
            m_nOrder = apiOrder.nOrder;
            m_chOrderKind = (char)apiOrder.chOrderKind;
            m_chFunctionCode = (char)apiOrder.chFunctionCode;
            m_nOrderPrice = apiOrder.nOrderPrice;
            m_nOrderVolume = apiOrder.nOrderVolume;
        }
    }
    public class TDBOrderQueue
    {
        public string m_strWindCode;         //万得代码(AG1312.SHF)
        public string m_strCode;             //交易所代码(ag1312)
        public int m_nDate;                  //日期（自然日）格式YYMMDD
        public int m_nTime;                  //订单时间(精确到毫秒HHMMSSmmm)
        public int m_nSide;                  //买卖方向('B':Bid 'A':Ask)
        public int m_nPrice;                 //成交价格((a double number + 0.00005) *10000)
        public int m_nOrderItems;            //订单数量
        public int m_nABItems;               //明细个数
        public int[] m_nABVolume;          //订单明细

        public void FromAPIOrderQueue(ref LibTDBWrap.TDBDefine_OrderQueue apiOrderQueue)
        {
            m_strWindCode = LibWrapHelper.AnsiArr2String(apiOrderQueue.chWindCode, 0, apiOrderQueue.chWindCode.Length);
            m_strCode = LibWrapHelper.AnsiArr2String(apiOrderQueue.chCode, 0, apiOrderQueue.chCode.Length);
            m_nDate = apiOrderQueue.nDate;
            m_nTime = apiOrderQueue.nTime;
            m_nSide = apiOrderQueue.nSide;
            m_nPrice = apiOrderQueue.nPrice;
            m_nOrderItems = apiOrderQueue.nOrderItems;
            m_nABItems = apiOrderQueue.nABItems;
            m_nABVolume = LibWrapHelper.CopyIntArr(apiOrderQueue.nABVolume);
        }
    }

    public class TDBCode
    {
        public string m_strWindCode;//万得代码(AG1312.SHF)
        public string m_strCode;//交易所代码(ag1312)
        public string m_strMarket;//市场代码(SHF)
        public string m_strCNName;//证券中文名称
        public string m_strENName;//证券英文名称
        public int m_nType;//证券类型，参考tdf 3.0文档中的证券索引表nType 说明

        public void FromAPICode(ref LibTDBWrap.TDBDefine_Code apiCode)
        {
            m_strWindCode = LibWrapHelper.AnsiArr2String(apiCode.chWindCode, 0, apiCode.chWindCode.Length);
            m_strCode = LibWrapHelper.AnsiArr2String(apiCode.chCode, 0, apiCode.chCode.Length);
            m_strMarket = LibWrapHelper.AnsiArr2String(apiCode.chMarket, 0, apiCode.chMarket.Length);
            m_strCNName = LibWrapHelper.AnsiArr2String(apiCode.chCNName, 0, apiCode.chCNName.Length);
            m_strENName = LibWrapHelper.AnsiArr2String(apiCode.chENName, 0, apiCode.chENName.Length);
            m_nType = apiCode.nType;
        }
    }

    class TDBDataSource
    {
        public TDBDataSource(string strIP="", string strPort="", string strUser="", string strPassword="", int nTimeOutVal = 30, int nRetryCount=1, int nRetryGap=1)
        {
            m_strIP = strIP;
            m_strPort = strPort;
            m_strUser = strUser;
            m_strPassword = strPassword;
            m_nTimeOutVal = nTimeOutVal;
            m_nRetryCount = nRetryCount;
            m_nRetryGap = nRetryGap;

            m_hTdb = (IntPtr)0;
        }
        ~TDBDataSource()
        {
            DisConnect();
        }

        //同步连接到数据源，返回值TDB_SUCCESS表示成功
        public TDBErrNo Connect(out TDBLoginResult tdbLoginResult)
        {
            if ((UInt64)m_hTdb != 0)
            {
                tdbLoginResult = (TDBLoginResult)m_loginResult.Clone();
                return  TDBErrNo.TDB_SUCCESS;
            }

            LibTDBWrap.OPEN_SETTINGS openSettings = new LibTDBWrap.OPEN_SETTINGS();
            openSettings.szIP = LibWrapHelper.String2AnsiArr(m_strIP, 24);
            openSettings.szPort = LibWrapHelper.String2AnsiArr(m_strPort, 8);
            openSettings.szUser = LibWrapHelper.String2AnsiArr(m_strUser, 32);
            openSettings.szPassword = LibWrapHelper.String2AnsiArr(m_strPassword, 32);

            openSettings.nTimeOutVal = m_nTimeOutVal ;
            openSettings.nRetryCount = m_nRetryCount;
            openSettings.nRetryGap = m_nRetryGap;

            IntPtr pUnmanagedOpenSettings = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(LibTDBWrap.OPEN_SETTINGS)));
            Marshal.StructureToPtr(openSettings, pUnmanagedOpenSettings, false);

            IntPtr pUnmanagedLoginRes = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(LibTDBWrap.TDBDefine_ResLogin)));

            m_hTdb = LibTDBWrap.TDB_Open(pUnmanagedOpenSettings, pUnmanagedLoginRes);
            LibTDBWrap.TDBDefine_ResLogin loginRes = (LibTDBWrap.TDBDefine_ResLogin)Marshal.PtrToStructure(pUnmanagedLoginRes, typeof(LibTDBWrap.TDBDefine_ResLogin));

            Marshal.FreeHGlobal(pUnmanagedOpenSettings);
            Marshal.FreeHGlobal(pUnmanagedLoginRes);

            m_loginResult = new TDBLoginResult();
            m_loginResult.FromAPILoginResult(loginRes);

            tdbLoginResult = (TDBLoginResult)m_loginResult.Clone();

            if ((UInt64)m_hTdb != 0)
            {
                return TDBErrNo.TDB_SUCCESS;
            }
            else
            {
                return TDBErrNo.TDB_OPEN_FAILED;
            }
        }
        //断开到数据源的连接
        public void DisConnect()
        {
            if ((UInt64)m_hTdb != 0)
            {
                LibTDBWrap.TDB_Close(m_hTdb);
                m_hTdb = (IntPtr)0;
            }
        }

        //获取某个市场或者全部市场的代码表。strMarket取值: "SH"、"SZ"、"CF"、"SHF"、"CZC"、"DCE"，全部市场：""
        public TDBErrNo GetCodeTable(string strMarket, out TDBCode[] codeArr)
        {
            TDBErrNo nVerifyRet = SimpleVerifyReqInput(strMarket);
            codeArr = new TDBCode[0];
            if (nVerifyRet != TDBErrNo.TDB_SUCCESS)
            {
                return nVerifyRet;
            }
            int nArrLen = 128; 
            byte[] btMarketArr = LibWrapHelper.String2AnsiArr(strMarket, nArrLen);
            IntPtr pUnmanagedMarket = Marshal.AllocHGlobal(btMarketArr.Length);
            Marshal.Copy(btMarketArr, 0, pUnmanagedMarket, nArrLen);
            IntPtr ppCodeTable = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(IntPtr)));
            IntPtr pCount = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(Int32)));

            int nRet = LibTDBWrap.TDB_GetCodeTable(m_hTdb, pUnmanagedMarket, ppCodeTable, pCount);
            IntPtr pCodeTable = (IntPtr)Marshal.PtrToStructure(ppCodeTable, typeof(IntPtr));
            int nCount = (Int32)Marshal.PtrToStructure(pCount, typeof(Int32));

            if (nRet == 0 && (UInt64)pCodeTable != 0 && nCount>0)
            {
                codeArr = new TDBCode[nCount];
                int nElemLen = Marshal.SizeOf(typeof(LibTDBWrap.TDBDefine_Code));
                for (int i = 0; i < nCount; i++ )
                {
                    LibTDBWrap.TDBDefine_Code apiCode = (LibTDBWrap.TDBDefine_Code)Marshal.PtrToStructure((IntPtr)((UInt64)pCodeTable + (UInt64)(nElemLen*i)), typeof(LibTDBWrap.TDBDefine_Code));
                    codeArr[i] = new TDBCode();
                    codeArr[i].FromAPICode(ref apiCode);
                }
            }
            else
            {
                //如果网络连接断掉，则关闭连接
                if (nRet == (int)TDBErrNo.TDB_NETWORK_ERROR)
                {
                    DisConnect();
                }
            }

            if ((UInt16)pCodeTable!=0)
            {
                LibTDBWrap.TDB_Free(pCodeTable);
            }

            Marshal.FreeHGlobal(pUnmanagedMarket);
            Marshal.FreeHGlobal(ppCodeTable);
            Marshal.FreeHGlobal(pCount);
            return (TDBErrNo)nRet;
        }

        //获取K线
        public TDBErrNo GetKLine(TDBReqKLine reqKLine, out TDBKLine[] tdbKLine)
        {
            TDBErrNo nVerifyRet = SimpleVerifyReqInput(reqKLine);
            tdbKLine = new TDBKLine[0];
            if (nVerifyRet != TDBErrNo.TDB_SUCCESS)
            {
                return nVerifyRet;
            }

            LibTDBWrap.TDBDefine_ReqKLine reqKLineInner = reqKLine.ToAPIReqKLine();
            IntPtr pUnmanagedAPIReqK = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(LibTDBWrap.TDBDefine_ReqKLine)));
            Marshal.StructureToPtr(reqKLineInner, pUnmanagedAPIReqK, false);

            IntPtr ppKLine = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(IntPtr)));
            IntPtr pCount = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(Int32)));
            int nRet = LibTDBWrap.TDB_GetKLine(m_hTdb, pUnmanagedAPIReqK, ppKLine, pCount);
            IntPtr PKLine = (IntPtr)Marshal.PtrToStructure(ppKLine, typeof(IntPtr));
            int nCount = (int)Marshal.PtrToStructure(pCount, typeof(Int32));
            if ((UInt64)PKLine != 0 && nRet == 0 && nCount > 0)
            {
                tdbKLine = new TDBKLine[nCount];
                int nElemLen = Marshal.SizeOf(typeof(LibTDBWrap.TDBDefine_KLine));
                for (int i = 0; i < nCount; i++ )
                {
                    LibTDBWrap.TDBDefine_KLine apiKLine = (LibTDBWrap.TDBDefine_KLine)Marshal.PtrToStructure((IntPtr)((UInt64)PKLine + (UInt64)(nElemLen*i)), typeof(LibTDBWrap.TDBDefine_KLine));
                    tdbKLine[i] = new TDBKLine();
                    tdbKLine[i].FromAPIKLine(ref apiKLine);
                }

            }
            else
            {
                //如果网络连接断掉，则关闭连接
                if (nRet == (int)TDBErrNo.TDB_NETWORK_ERROR)
                {
                    DisConnect();
                }
            }

            if ((UInt64)PKLine != 0)
            {
                LibTDBWrap.TDB_Free(PKLine);
            }

            Marshal.FreeHGlobal(pUnmanagedAPIReqK);
            Marshal.FreeHGlobal(ppKLine);
            Marshal.FreeHGlobal(pCount);
            
            return (TDBErrNo)nRet;
        }

        //获取普通股票的行情数据(不带买卖盘)，本接口不支持期货，对于期货（CF市场和商品期货），需要调用GetFuture或GetFutureAB
        public TDBErrNo GetTick(TDBReq reqTick, out TDBTick[] tdbTick)
        {
            TDBErrNo nVerifyRet = SimpleVerifyReqInput(reqTick);
            tdbTick = new TDBTick[0];
            if (nVerifyRet != TDBErrNo.TDB_SUCCESS)
            {
                return nVerifyRet;
            }

            LibTDBWrap.TDBDefine_ReqTick reqAPITick = reqTick.ToAPIReqTick();
            IntPtr pUnmanagedReqAPITick = LibWrapHelper.CopyStructToGlobalMem(reqAPITick, typeof(LibTDBWrap.TDBDefine_ReqTick));
            IntPtr ppTick = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(IntPtr)));
            IntPtr pCount = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(Int32)));
            int nRet = LibTDBWrap.TDB_GetTick(m_hTdb, pUnmanagedReqAPITick, ppTick, pCount);
            IntPtr pTick = (IntPtr)Marshal.PtrToStructure(ppTick, typeof(IntPtr));
            int nCount = (Int32)Marshal.PtrToStructure(pCount, typeof(Int32));
            if ((UInt64)pTick!=0 && nCount>0 && nRet==0)
            {
                tdbTick = new TDBTick[nCount];
                int nElemLen = Marshal.SizeOf(typeof(LibTDBWrap.TDBDefine_Tick));
                for (int i = 0; i < nCount; i++ )
                {
                    IntPtr pCurRecord = (IntPtr)((UInt64)pTick + (UInt64)(nElemLen * i));
                    LibTDBWrap.TDBDefine_Tick apiTick = (LibTDBWrap.TDBDefine_Tick)Marshal.PtrToStructure(pCurRecord, typeof(LibTDBWrap.TDBDefine_Tick));
                    tdbTick[i] = new TDBTick();
                    tdbTick[i].FromAPITick(ref apiTick);
                }
            }
            else
            {
                //如果网络连接断掉，则关闭连接
                if (nRet == (int)TDBErrNo.TDB_NETWORK_ERROR)
                {
                    DisConnect();
                }
            }

            if ((UInt64)pTick != 0)
            {
                LibTDBWrap.TDB_Free(pTick);
            }
            
            Marshal.FreeHGlobal(pUnmanagedReqAPITick);
            Marshal.FreeHGlobal(ppTick);
            Marshal.FreeHGlobal(pCount);

            return (TDBErrNo)nRet;
        }


        //获取普通股票的行情数据(带买卖盘)，本接口不支持期货，对于期货（CF市场和商品期货），需要调用GetFuture或GetFutureAB
        public TDBErrNo GetTickAB(TDBReq reqTick, out TDBTickAB[] tdbTickAB)
        {
            TDBErrNo nVerifyRet = SimpleVerifyReqInput(reqTick);
            tdbTickAB = new TDBTickAB[0];
            if (nVerifyRet != TDBErrNo.TDB_SUCCESS)
            {
                return nVerifyRet;
            }

            LibTDBWrap.TDBDefine_ReqTick reqAPITick = reqTick.ToAPIReqTick();
            IntPtr pUnmanagedReqAPITick = LibWrapHelper.CopyStructToGlobalMem(reqAPITick, typeof(LibTDBWrap.TDBDefine_ReqTick));
            IntPtr ppTickAB = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(IntPtr)));
            IntPtr pCount = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(Int32)));
            int nRet = LibTDBWrap.TDB_GetTickAB(m_hTdb, pUnmanagedReqAPITick, ppTickAB, pCount);
            IntPtr pTickAB = (IntPtr)Marshal.PtrToStructure(ppTickAB, typeof(IntPtr));
            int nCount = (Int32)Marshal.PtrToStructure(pCount, typeof(Int32));
            if ((UInt64)pTickAB != 0 && nCount > 0 && nRet == 0)
            {
                tdbTickAB = new TDBTickAB[nCount];
                int nElemLen = Marshal.SizeOf(typeof(LibTDBWrap.TDBDefine_TickAB));
                for (int i = 0; i < nCount; i++)
                {
                    IntPtr pCurRecord = (IntPtr)((UInt64)pTickAB + (UInt64)(nElemLen * i));
                    LibTDBWrap.TDBDefine_TickAB apiTickAB = (LibTDBWrap.TDBDefine_TickAB)Marshal.PtrToStructure(pCurRecord, typeof(LibTDBWrap.TDBDefine_TickAB));
                    tdbTickAB[i] = new TDBTickAB();
                    tdbTickAB[i].FromAPITick(ref apiTickAB);
                }
            }
            else
            {
                //如果网络连接断掉，则关闭连接
                if (nRet == (int)TDBErrNo.TDB_NETWORK_ERROR)
                {
                    DisConnect();
                }
            }

            if ((UInt64)pTickAB != 0)
            {
                LibTDBWrap.TDB_Free(pTickAB);
            }

            Marshal.FreeHGlobal(pUnmanagedReqAPITick);
            Marshal.FreeHGlobal(ppTickAB);
            Marshal.FreeHGlobal(pCount);

            return (TDBErrNo)nRet;
        }


        //获取期货行情数据(不带买卖盘)，本接口不支持普通股票代码，对于普通股票（SH 和SZ市场），需要调用GetTick或GetTickAB
        public TDBErrNo GetFuture(TDBReqFuture reqFuture, out TDBFuture[] tdbFuture)
        {
            TDBErrNo nVerifyRet = SimpleVerifyReqInput(reqFuture);
            tdbFuture = new TDBFuture[0];
            if (nVerifyRet != TDBErrNo.TDB_SUCCESS)
            {
                return nVerifyRet;
            }

            LibTDBWrap.TDBDefine_ReqFuture reqAPIFuture = reqFuture.ToAPIReqFuture();
            IntPtr pUnmanagedReqAPIFuture = LibWrapHelper.CopyStructToGlobalMem(reqAPIFuture, typeof(LibTDBWrap.TDBDefine_ReqFuture));
            IntPtr ppFuture = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(IntPtr)));
            IntPtr pCount = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(Int32)));
            int nRet = LibTDBWrap.TDB_GetFuture(m_hTdb, pUnmanagedReqAPIFuture, ppFuture, pCount);
            IntPtr pFuture = (IntPtr)Marshal.PtrToStructure(ppFuture, typeof(IntPtr));
            int nCount = (Int32)Marshal.PtrToStructure(pCount, typeof(Int32));
            if ((UInt64)pFuture != 0 && nCount > 0 && nRet == 0)
            {
                tdbFuture = new TDBFuture[nCount];
                int nElemLen = Marshal.SizeOf(typeof(LibTDBWrap.TDBDefine_Future));
                for (int i = 0; i < nCount; i++)
                {
                    IntPtr pCurRecord = (IntPtr)((UInt64)pFuture + (UInt64)(nElemLen * i));
                    LibTDBWrap.TDBDefine_Future apiFuture = (LibTDBWrap.TDBDefine_Future)Marshal.PtrToStructure(pCurRecord, typeof(LibTDBWrap.TDBDefine_Future));
                    tdbFuture[i] = new TDBFuture();
                    tdbFuture[i].FromAPIFuture(ref apiFuture);
                }
            }
            else
            {
                //如果网络连接断掉，则关闭连接
                if (nRet == (int)TDBErrNo.TDB_NETWORK_ERROR)
                {
                    DisConnect();
                }
            }

            if ((UInt64)pFuture != 0)
            {
                LibTDBWrap.TDB_Free(pFuture);
            }

            Marshal.FreeHGlobal(pUnmanagedReqAPIFuture);
            Marshal.FreeHGlobal(ppFuture);
            Marshal.FreeHGlobal(pCount);

            return (TDBErrNo)nRet;
        }

        //获取期货行情数据(带买卖盘)，本接口不支持普通股票代码，对于普通股票（SH 和SZ市场），需要调用GetTick或GetTickAB
        public TDBErrNo GetFutureAB(TDBReqFuture reqFuture, out TDBFutureAB[] tdbFutureAB)
        {
            TDBErrNo nVerifyRet = SimpleVerifyReqInput(reqFuture);
            tdbFutureAB = new TDBFutureAB[0];
            if (nVerifyRet != TDBErrNo.TDB_SUCCESS)
            {
                return nVerifyRet;
            }

            LibTDBWrap.TDBDefine_ReqFuture reqAPIFuture = reqFuture.ToAPIReqFuture();
            IntPtr pUnmanagedReqAPIFuture = LibWrapHelper.CopyStructToGlobalMem(reqAPIFuture, typeof(LibTDBWrap.TDBDefine_ReqFuture));
            IntPtr ppFutureAB = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(IntPtr)));
            IntPtr pCount = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(Int32)));
            int nRet = LibTDBWrap.TDB_GetFutureAB(m_hTdb, pUnmanagedReqAPIFuture, ppFutureAB, pCount);
            IntPtr pFutureAB = (IntPtr)Marshal.PtrToStructure(ppFutureAB, typeof(IntPtr));
            int nCount = (Int32)Marshal.PtrToStructure(pCount, typeof(Int32));
            if ((UInt64)pFutureAB != 0 && nCount > 0 && nRet == 0)
            {
                tdbFutureAB = new TDBFutureAB[nCount];
                int nElemLen = Marshal.SizeOf(typeof(LibTDBWrap.TDBDefine_FutureAB));
                for (int i = 0; i < nCount; i++)
                {
                    IntPtr pCurRecord = (IntPtr)((UInt64)pFutureAB + (UInt64)(nElemLen * i));
                    LibTDBWrap.TDBDefine_FutureAB apiFuture = (LibTDBWrap.TDBDefine_FutureAB)Marshal.PtrToStructure(pCurRecord, typeof(LibTDBWrap.TDBDefine_FutureAB));
                    tdbFutureAB[i] = new TDBFutureAB();
                    tdbFutureAB[i].FromAPIFutureAB(ref apiFuture);
                }
            }
            else
            {
                //如果网络连接断掉，则关闭连接
                if (nRet == (int)TDBErrNo.TDB_NETWORK_ERROR)
                {
                    DisConnect();
                }
            }

            if ((UInt64)pFutureAB != 0)
            {
                LibTDBWrap.TDB_Free(pFutureAB);
            }

            Marshal.FreeHGlobal(pUnmanagedReqAPIFuture);
            Marshal.FreeHGlobal(ppFutureAB);
            Marshal.FreeHGlobal(pCount);

            return (TDBErrNo)nRet;
        }

        //获取逐笔成交
        public TDBErrNo GetTransaction(TDBReq reqTransaction, out TDBTransaction[] tdbTransaction)
        {
            TDBErrNo nVerifyRet = SimpleVerifyReqInput(reqTransaction);
            tdbTransaction = new TDBTransaction[0];
            if (nVerifyRet != TDBErrNo.TDB_SUCCESS)
            {
                return nVerifyRet;
            }

            LibTDBWrap.TDBDefine_ReqTransaction reqAPITransaction = reqTransaction.ToAPIReqTransaction();
            IntPtr pUnmanagedReqAPITransaction = LibWrapHelper.CopyStructToGlobalMem(reqAPITransaction, typeof(LibTDBWrap.TDBDefine_ReqTransaction));
            IntPtr ppTransaction = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(IntPtr)));
            IntPtr pCount = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(Int32)));
            int nRet = LibTDBWrap.TDB_GetTransaction(m_hTdb, pUnmanagedReqAPITransaction, ppTransaction, pCount);
            IntPtr pTransaction = (IntPtr)Marshal.PtrToStructure(ppTransaction, typeof(IntPtr));
            int nCount = (Int32)Marshal.PtrToStructure(pCount, typeof(Int32));
            if ((UInt64)pTransaction != 0 && nCount > 0 && nRet == 0)
            {
                tdbTransaction = new TDBTransaction[nCount];
                int nElemLen = Marshal.SizeOf(typeof(LibTDBWrap.TDBDefine_Transaction));
                for (int i = 0; i < nCount; i++)
                {
                    IntPtr pCurRecord = (IntPtr)((UInt64)pTransaction + (UInt64)(nElemLen * i));
                    LibTDBWrap.TDBDefine_Transaction apiFuture = (LibTDBWrap.TDBDefine_Transaction)Marshal.PtrToStructure(pCurRecord, typeof(LibTDBWrap.TDBDefine_Transaction));
                    tdbTransaction[i] = new TDBTransaction();
                    tdbTransaction[i].FromAPITransaction(ref apiFuture);
                }
            }
            else
            {
                //如果网络连接断掉，则关闭连接
                if (nRet == (int)TDBErrNo.TDB_NETWORK_ERROR)
                {
                    DisConnect();
                }
            }

            if ((UInt64)pTransaction != 0)
            {
                LibTDBWrap.TDB_Free(pTransaction);
            }

            Marshal.FreeHGlobal(pUnmanagedReqAPITransaction);
            Marshal.FreeHGlobal(ppTransaction);
            Marshal.FreeHGlobal(pCount);

            return (TDBErrNo)nRet;
        }

        //委托队列
        public TDBErrNo GetOrderQueue(TDBReq reqOrderQueue, out TDBOrderQueue[] tdbOrderQueue)
        {
            TDBErrNo nVerifyRet = SimpleVerifyReqInput(reqOrderQueue);
            tdbOrderQueue = new TDBOrderQueue[0];
            if (nVerifyRet != TDBErrNo.TDB_SUCCESS)
            {
                return nVerifyRet;
            }

            LibTDBWrap.TDBDefine_ReqTransaction reqAPIOrderQueue = reqOrderQueue.ToAPIReqTransaction();
            IntPtr pUnmanagedReqAPIOrderQueue = LibWrapHelper.CopyStructToGlobalMem(reqAPIOrderQueue, typeof(LibTDBWrap.TDBDefine_ReqTransaction));
            IntPtr ppOrderQueue = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(IntPtr)));
            IntPtr pCount = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(Int32)));
            int nRet = LibTDBWrap.TDB_GetOrderQueue(m_hTdb, pUnmanagedReqAPIOrderQueue, ppOrderQueue, pCount);
            IntPtr pOrderQueue = (IntPtr)Marshal.PtrToStructure(ppOrderQueue, typeof(IntPtr));
            int nCount = (Int32)Marshal.PtrToStructure(pCount, typeof(Int32));
            if ((UInt64)pOrderQueue != 0 && nCount > 0 && nRet == 0)
            {
                tdbOrderQueue = new TDBOrderQueue[nCount];
                int nElemLen = Marshal.SizeOf(typeof(LibTDBWrap.TDBDefine_OrderQueue));
                for (int i = 0; i < nCount; i++)
                {
                    IntPtr pCurRecord = (IntPtr)((UInt64)pOrderQueue + (UInt64)(nElemLen * i));
                    LibTDBWrap.TDBDefine_OrderQueue apiOrderQueue = (LibTDBWrap.TDBDefine_OrderQueue)Marshal.PtrToStructure(pCurRecord, typeof(LibTDBWrap.TDBDefine_OrderQueue));
                    tdbOrderQueue[i] = new TDBOrderQueue();
                    tdbOrderQueue[i].FromAPIOrderQueue(ref apiOrderQueue);
                }
            }
            else
            {
                //如果网络连接断掉，则关闭连接
                if (nRet == (int)TDBErrNo.TDB_NETWORK_ERROR)
                {
                    DisConnect();
                }
            }

            if ((UInt64)pOrderQueue != 0)
            {
                LibTDBWrap.TDB_Free(pOrderQueue);
            }

            Marshal.FreeHGlobal(pUnmanagedReqAPIOrderQueue);
            Marshal.FreeHGlobal(ppOrderQueue);
            Marshal.FreeHGlobal(pCount);

            return (TDBErrNo)nRet;
        }

        //逐笔委托
        public TDBErrNo GetOrder(TDBReq reqOrder, out TDBOrder[] tdbOrder)
        {
            TDBErrNo nVerifyRet = SimpleVerifyReqInput(reqOrder);
            tdbOrder = new TDBOrder[0];
            if (nVerifyRet != TDBErrNo.TDB_SUCCESS)
            {
                return nVerifyRet;
            }

            LibTDBWrap.TDBDefine_ReqTransaction reqAPIOrder = reqOrder.ToAPIReqTransaction();
            IntPtr pUnmanagedReqAPIOrder = LibWrapHelper.CopyStructToGlobalMem(reqAPIOrder, typeof(LibTDBWrap.TDBDefine_ReqTransaction));
            IntPtr ppOrder = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(IntPtr)));
            IntPtr pCount = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(Int32)));
            int nRet = LibTDBWrap.TDB_GetOrder(m_hTdb, pUnmanagedReqAPIOrder, ppOrder, pCount);
            IntPtr pOrder = (IntPtr)Marshal.PtrToStructure(ppOrder, typeof(IntPtr));
            int nCount = (Int32)Marshal.PtrToStructure(pCount, typeof(Int32));
            if ((UInt64)pOrder != 0 && nCount > 0 && nRet == 0)
            {
                tdbOrder = new TDBOrder[nCount];
                int nElemLen = Marshal.SizeOf(typeof(LibTDBWrap.TDBDefine_Order));
                for (int i = 0; i < nCount; i++)
                {
                    IntPtr pCurRecord = (IntPtr)((UInt64)pOrder + (UInt64)(nElemLen * i));
                    LibTDBWrap.TDBDefine_Order apiOrder = (LibTDBWrap.TDBDefine_Order)Marshal.PtrToStructure(pCurRecord, typeof(LibTDBWrap.TDBDefine_Order));
                    tdbOrder[i] = new TDBOrder();
                    tdbOrder[i].FromAPIOrder(ref apiOrder);
                }
            }
            else
            {
                //如果网络连接断掉，则关闭连接
                if (nRet == (int)TDBErrNo.TDB_NETWORK_ERROR)
                {
                    DisConnect();
                }
            }

            if ((UInt64)pOrder != 0)
            {
                LibTDBWrap.TDB_Free(pOrder);
            }

            Marshal.FreeHGlobal(pUnmanagedReqAPIOrder);
            Marshal.FreeHGlobal(ppOrder);
            Marshal.FreeHGlobal(pCount);

            return (TDBErrNo)nRet;
        }

        //如果查询的代码不存在，连接已经断掉、未连接，则返回null
        public TDBCode GetCodeInfo(string strWindCode)
        {
            TDBErrNo nVerifyRet = SimpleVerifyReqInput(strWindCode);
            if (nVerifyRet != TDBErrNo.TDB_SUCCESS)
            {
                return null;
            }
            int nMaxWindCodeLen = 64;
            IntPtr pszWindCode = Marshal.AllocHGlobal(nMaxWindCodeLen);
            byte[] btWindCode = LibWrapHelper.String2AnsiArr(strWindCode, nMaxWindCodeLen);
            btWindCode[btWindCode.Length - 1] = 0;
            Marshal.Copy(btWindCode, 0, pszWindCode, btWindCode.Length);
            IntPtr pCode = LibTDBWrap.TDB_GetCodeInfo(m_hTdb, pszWindCode);
            Marshal.FreeHGlobal(pszWindCode);
            if ((UInt64)pCode != 0)
            {
                LibTDBWrap.TDBDefine_Code apiCode = (LibTDBWrap.TDBDefine_Code)Marshal.PtrToStructure(pCode, typeof(LibTDBWrap.TDBDefine_Code));
                TDBCode tdbCode = new TDBCode();
                tdbCode.FromAPICode(ref apiCode);
                return tdbCode;
            }
            else
            {
                return null;
            }
        }


        //////////////////////////
        private TDBErrNo SimpleVerifyReqInput(object reqObj)
        {
            if (reqObj == null)
            {
                return TDBErrNo.TDB_INVALID_PARAMS;
            }
            
            if ((UInt64)m_hTdb == 0)
            {
                return TDBErrNo.TDB_NETWORK_ERROR;
            }

            return TDBErrNo.TDB_SUCCESS;
        }
        private string m_strIP;
        private string m_strPort;
        private string m_strUser;
        private string m_strPassword;
        private int m_nTimeOutVal;
        private int m_nRetryCount;
        private int m_nRetryGap;

        private IntPtr m_hTdb;
        private TDBLoginResult m_loginResult;
    }
}
