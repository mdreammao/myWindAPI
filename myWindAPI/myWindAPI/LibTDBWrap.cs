using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace TDBAPIImp
{
    public class LibWrapHelper
    {
        //nDstLen指定目标字符串的长度，当<=0时，为str.Length;
        public static byte[] String2AnsiArr(string str, int nDstLen)
        {
            Encoding coderMBCS = Encoding.GetEncoding(936);
            byte[] src = coderMBCS.GetBytes(str);
            if (nDstLen <= 0)
            {
                nDstLen = str.Length;
            }
            byte[] dst = new byte[nDstLen];
            src.CopyTo(dst, 0);
            return dst;
        }

        public static string AnsiArr2String(byte[] btArr, int nStart, int nLen)
        {
            if (nLen<=0)
            {
                return "";
            }

            int nLenExcludeZero = 0;
            for (int i = 0; i < nLen; i++ )
            {
                if (btArr[i] > 0)
                {
                    nLenExcludeZero++;
                }
                else
                {
                    break;
                }
            }
            byte[] dstArr = new byte[nLenExcludeZero];
            System.Array.Copy(btArr, nStart, dstArr, 0, nLenExcludeZero);
            Encoding coderMBCS = Encoding.GetEncoding(936);
            return coderMBCS.GetString(dstArr);
        }

        
        public static IntPtr CopyStructToGlobalMem(object obj, System.Type typeInfo)
        {
            IntPtr pRet = Marshal.AllocHGlobal(Marshal.SizeOf(typeInfo));
            Marshal.StructureToPtr(obj, pRet, false);
            return pRet;
        }

        public static int[] CopyIntArr(object intArray)
        {
            int[] nums = (int[])intArray;
            int[] nRet = new int[nums.Length];
            System.Array.Copy(nums, nRet, nums.Length);
            return nRet;
        }
    }


    public class LibTDBWrap
    {
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
        public struct OPEN_SETTINGS
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 24)]
            public byte[] szIP;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
            public byte[] szPort;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
            public byte[] szUser;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
            public byte[] szPassword;

            public Int32 nTimeOutVal;
            public Int32 nRetryCount;
            public Int32 nRetryGap;
        }

        public enum TDBProxyType
        {
            TDB_PROXY_SOCK4,
            TDB_PROXY_SOCK4A,
            TDB_PROXY_SOCK5,
            TDB_PROXY_HTTP11,
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
        public struct TDB_OPEN_SETTINGS
        {
            public Int32 nProxyType;    //enum TDBProxyType

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 64)]
            public byte[] szProxyHostIp;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
            public byte[] szProxyPort;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
            public byte[] szProxyUser;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
            public byte[] szProxyPwd;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
        public struct TDBDefine_ResLogin
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 64)]
            public byte[] szInfo;

            public Int32 nMarkets;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 256*8)]
            public byte[] szMarket;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 256)]
            public Int32[] nDynDate;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
        public struct TDBDefine_Code
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
            public byte[] chWindCode;        //万得代码(AG1312.SHF)

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
            public byte[] chCode;            //交易所代码(ag1312)

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
            public byte[] chMarket;           //市场代码(SHF)

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
            public byte[] chCNName;          //证券中文名称

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
            public byte[] chENName;          //证券英文名称

            public Int32  nType;                 //证券类型
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
        public struct TDBDefine_ReqKLine
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
            public byte[] chCode;//证券万得代码(AG1312.SHF)

            public Int32 nCQFlag; //除权标志：不复权，向前复权，向后复权
            public Int32 nCQDate; //复权日期(<=0:全程复权) 格式：YYMMDD，例如20130101表示2013年1月1日
            public Int32 nQJFlag; //全价标志(债券)(0:净价 1:全价)

            public Int32 nCycType;//数据周期：秒线、分钟、日线、周线、月线、季线、半年线、年线
            public Int32 nCycDef; //周期数量：仅当nCycType取值：秒、分钟、日线、周线、月线时，这个字段有效。

            public Int32 nAutoComplete;   //自动补齐：仅1秒钟线、1分钟线支持这个标志，（不为0：补齐；0：不补齐）
            public Int32 nBeginDate;             //开始日期(交易日，<0:从上市日期开始； 0:从今天开始)
            public Int32 nEndDate;               //结束日期(交易日，<=0:跟nBeginDate一样) 
            public Int32 nBeginTime;             //开始时间，<=0表示从开始，格式：（HHMMSSmmm）例如94500000 表示 9点45分00秒000毫秒
            public Int32 nEndTime;               //结束时间，<=0表示到结束，格式：（HHMMSSmmm）例如94500000 表示 9点45分00秒000毫秒
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
        public struct TDBDefine_KLine
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
            public byte[] chWindCode;            //万得代码(AG1312.SHF)
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
            public byte[] chCode;                //交易所代码(ag1312)

            public Int32 nDate;                   //日期（自然日）格式：YYMMDD，例如20130101表示2013年1月1日，0表示今天
            public Int32 nTime;                   //时间（HHMMSSmmm）例如94500000 表示 9点45分00秒000毫秒
            public Int32 nOpen;                   //开盘((a double number + 0.00005) *10000)
            public Int32 nHigh;                   //最高((a double number + 0.00005) *10000)
            public Int32 nLow;                    //最低((a double number + 0.00005) *10000)
            public Int32 nClose;                  //收盘((a double number + 0.00005) *10000)

            public Int64 iVolume;             //成交量
            public Int64 iTurover;            //成交额(元)

            public Int32 nMatchItems;         //成交笔数
            public Int32 nInterest;           //持仓量(期货)、IOPV(基金)、利息(债券)
        }

        public enum REFILLFLAG
        {
            REFILL_NONE = 0,            //不复权
            REFILL_BACKWARD = 1,      //全程向前复权（从现在向过去）
            REFILL_FORWARD = 2,       //全程向后复权（从过去向现在）
        }
        public enum CYCTYPE
        {
            CYC_SECOND=0,
            CYC_MINUTE,
            CYC_DAY,
            CYC_WEEK,
            CYC_MONTH,
            CYC_SEASON,
            CYC_HAFLYEAR,
            CYC_YEAR,
            CYC_TICKBAR,
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
        public struct TDBDefine_ReqTick
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
            public byte[] chCode; //证券万得代码(AG1312.SHF)
            public Int32 nBeginDate;    //开始日期（交易日）,为0则从今天，格式：YYMMDD，例如20130101表示2013年1月1日
            public Int32 nEndDate;      //结束日期（交易日），为0则和nBeginDate一样
            public Int32 nBeginTime;    //开始时间：若<=0则从头，格式：（HHMMSSmmm）例如94500000 表示 9点45分00秒000毫秒
            public Int32 nEndTime;      //结束时间：若<=0则至最后
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
        public struct TDBDefine_Tick
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
            public byte[] chWindCode;                //万得代码(AG1312.SHF)

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
            public byte[] chCode;                    //交易所代码(ag1312)
            public Int32 nDate;                          //日期（自然日）
            public Int32 nTime;                          //时间（HHMMSSmmm）例如94500000 表示 9点45分00秒000毫秒
            public Int32 nPrice;                         //成交价((a double number + 0.00005) *10000)
            public Int64 iVolume;                    //成交量
            public Int64 iTurover;                //成交额(元)
            public Int32 nMatchItems;                    //成交笔数
            public Int32 nInterest;                      //IOPV(基金)、利息(债券)
            public byte chTradeFlag;                   //成交标志
            public byte chBSFlag;                      //BS标志
            public Int64 iAccVolume;                 //当日累计成交量
            public Int64 iAccTurover;             //当日成交额(元)
            public Int32 nHigh;                          //最高((a double number + 0.00005) *10000)
            public Int32 nLow;                           //最低((a double number + 0.00005) *10000)
            public Int32 nOpen;                       //开盘((a double number + 0.00005) *10000)
            public Int32 nPreClose;                   //前收盘((a double number + 0.00005) *10000)

            //下面的字段指数使用
            public Int32 nIndex;                  //不加权指数
            public Int32 nStocks;                 //品种总数
            public Int32 nUps;                    //上涨品种数
            public Int32 nDowns;                  //下跌品种数
            public Int32 nHoldLines;              //持平品种数
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
        public struct TDBDefine_TickAB
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
            public byte[] chWindCode;                //万得代码(AG1312.SHF)

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
            public byte[] chCode;                    //交易所代码(ag1312)
            public Int32 nDate;                          //日期（自然日）
            public Int32 nTime;                          //时间（HHMMSSmmm）例如94500000 表示 9点45分00秒000毫秒
            public Int32 nPrice;                         //成交价((a double number + 0.00005) *10000)
            public Int64 iVolume;                    //成交量
            public Int64 iTurover;                //成交额(元)
            public Int32 nMatchItems;                    //成交笔数
            public Int32 nInterest;                      //IOPV(基金)、利息(债券)
            public byte chTradeFlag;                   //成交标志
            public byte chBSFlag;                      //BS标志
            public Int64 iAccVolume;                 //当日累计成交量
            public Int64 iAccTurover;             //当日成交额(元)
            public Int32 nHigh;                          //最高((a double number + 0.00005) *10000)
            public Int32 nLow;                           //最低((a double number + 0.00005) *10000)
            public Int32 nOpen;                       //开盘((a double number + 0.00005) *10000)
            public Int32 nPreClose;                   //前收盘((a double number + 0.00005) *10000)

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 10)]
            public Int32[] nAskPrice;               //叫卖价((a double number + 0.00005) *10000)

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 10)]
            public Int32[]  nAskVolume;            //叫卖量

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 10)]
            public Int32[]  nBidPrice;               //叫买价((a double number + 0.00005) *10000)

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 10)]
            public Int32[]  nBidVolume;            //叫买量
            
            public Int32    nAskAvPrice;                 //加权平均叫卖价(上海L2)((a double number + 0.00005) *10000)
            public Int32 nBidAvPrice;                 //加权平均叫买价(上海L2)((a double number + 0.00005) *10000)
            public Int64 iTotalAskVolume;           //叫卖总量(上海L2)
            public Int64 iTotalBidVolume;           //叫买总量(上海L2)

            //下面的字段指数使用
            public Int32 nIndex;                  //不加权指数
            public Int32 nStocks;                 //品种总数
            public Int32 nUps;                    //上涨品种数
            public Int32 nDowns;                  //下跌品种数
            public Int32 nHoldLines;              //持平品种数
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
        public struct TDBDefine_ReqFuture
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
            public byte[] chCode; //证券万得代码(AG1312.SHF)
            public Int32 nBeginDate;    //开始日期（交易日）,为0则从今天，格式：YYMMDD，例如20130101表示2013年1月1日
            public Int32 nEndDate;      //结束日期（交易日），为0则和nBeginDate一样
            public Int32 nBeginTime;    //开始时间：若<=0则从头，格式：（HHMMSSmmm）例如94500000 表示 9点45分00秒000毫秒
            public Int32 nEndTime;      //结束时间：若<=0则至最后

            public Int32 nAutoComplete;  //自动补齐标志:( 0：不自动补齐，1:自动补齐）
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
        public struct TDBDefine_Future
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
            public byte[] chWindCode;               //万得代码(AG1312.SHF)

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
            public byte[] chCode;                   //交易所代码(ag1312)
            
            public Int32 nDate;                         //日期（自然日）格式：YYMMDD
            public Int32 nTime;                         //时间，格式：HHMMSSmmm
    
            public Int64 iVolume;                   //成交量
            public Int64 iTurover;               //成交额(元)

            public Int32 nSettle;                       //结算价((a double number + 0.00005) *10000)
            public Int32 nPosition;                     //持仓量
            public Int32 nCurDelta;                     //虚实度

            public byte chTradeFlag;                  //成交标志（港股有值）

            public Int64 iAccVolume;                //当日累计成交量
            public Int64 iAccTurover;            //当日成交额(元)

            public Int32 nHigh;                         //最高((a double number + 0.00005) *10000)
            public Int32 nLow;                          //最低((a double number + 0.00005) *10000)
            public Int32 nOpen;                         //开盘((a double number + 0.00005) *10000)
            public Int32 nPrice;                        //成交价((a double number + 0.00005) *10000)

            public Int32 nPreClose;                     //前收盘((a double number + 0.00005) *10000)
            public Int32 nPreSettle;                    //昨结算((a double number + 0.00005) *10000)
            public Int32 nPrePosition;                  //昨持仓
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
        public struct TDBDefine_FutureAB
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
            public byte[] chWindCode;               //万得代码(AG1312.SHF)

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
            public byte[] chCode;                   //交易所代码(ag1312)

            public Int32 nDate;                         //日期（自然日）格式：YYMMDD
            public Int32 nTime;                         //时间，格式：HHMMSSmmm

            public Int64 iVolume;                   //成交量
            public Int64 iTurover;               //成交额(元)

            public Int32 nSettle;                       //结算价((a double number + 0.00005) *10000)
            public Int32 nPosition;                     //持仓量
            public Int32 nCurDelta;                     //虚实度

            public byte chTradeFlag;                  //成交标志（港股有值）

            public Int64 iAccVolume;                //当日累计成交量
            public Int64 iAccTurover;            //当日成交额(元)

            public Int32 nOpen;                         //开盘((a double number + 0.00005) *10000)
            public Int32 nHigh;                          //最高((a double number + 0.00005) *10000)
            public Int32 nLow;                         //最低((a double number + 0.00005) *10000)
            public Int32 nPrice;                        //成交价((a double number + 0.00005) *10000)

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 5)]
            public Int32[] nAskPrice;               //叫卖价((a double number + 0.00005) *10000)
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 5)]
            public Int32[] nAskVolume;               //叫卖量
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 5)]
            public Int32[] nBidPrice;               //叫买价((a double number + 0.00005) *10000)
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 5)]
            public Int32[] nBidVolume;               //叫买量

            public Int32 nPreClose;                     //前收盘((a double number + 0.00005) *10000)
            public Int32 nPreSettle;                    //昨结算((a double number + 0.00005) *10000)
            public Int32 nPrePosition;                  //昨持仓
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
        public struct TDBDefine_ReqTransaction
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
            public byte[] chCode;               //证券万得代码(AG1312.SHF)
            public Int32  nBeginDate;            //开始日期（交易日），格式YYMMDD
            public Int32  nEndDate;              //数据日期（交易日）小于等于0和nBeginDate相同
            public Int32  nBeginTime;            //开始时间:<=0表示从0开始，格式：HHMMSSmmm
            public Int32  nEndTime;              //结束时间：<=0表示到最后
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
        public struct TDBDefine_Order
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
            public byte[] chWindCode;        //万得代码(AG1312.SHF)
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
            public byte[] chCode;            //交易所代码(ag1312)
            public Int32  nDate;                 //日期（自然日）格式YYMMDD
            public Int32  nTime;                 //委托时间（精确到毫秒HHMMSSmmm）
            public Int32  nIndex;                //委托编号，从1开始，递增1
            public Int32  nOrder;                //交易所委托号
            public byte chOrderKind;           //委托类别
            public byte chFunctionCode;        //委托代码, B, S, C
            public Int32  nOrderPrice;           //委托价格((a double number + 0.00005) *10000)
            public Int32  nOrderVolume;          //委托数量
        }
        
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
        public struct TDBDefine_Transaction
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
            public byte[]    chWindCode;     //万得代码(AG1312.SHF)
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
            public byte[]    chCode;         //交易所代码(ag1312)
            public Int32     nDate;              //日期（自然日）格式:YYMMDD
            public Int32     nTime;              //成交时间(精确到毫秒HHMMSSmmm)
            public Int32     nIndex;             //成交编号(从1开始，递增1)
            public byte    chFunctionCode;     //成交代码: 'C', 0
            public byte    chOrderKind;        //委托类别
            public byte    chBSFlag;           //BS标志
            public Int32     nTradePrice;        //成交价格((a double number + 0.00005) *10000)
            public Int32     nTradeVolume;       //成交数量
            public Int32     nAskOrder;          //叫卖序号
            public Int32     nBidOrder;          //叫买序号
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
        public struct TDBDefine_OrderQueue
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
            public byte[]    chWindCode;         //万得代码(AG1312.SHF)
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
            public byte[]    chCode;             //交易所代码(ag1312)
            public Int32     nDate;                  //日期（自然日）格式YYMMDD
            public Int32     nTime;                  //订单时间(精确到毫秒HHMMSSmmm)
            public Int32     nSide;                  //买卖方向('B':Bid 'A':Ask)
            public Int32     nPrice;                 //成交价格((a double number + 0.00005) *10000)
            public Int32     nOrderItems;            //订单数量
            public Int32     nABItems;               //明细个数
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 50)]
            public Int32[]     nABVolume;          //订单明细
        }

        [DllImport(
             "TDB_API_Windows_v2.dll",
             EntryPoint = "TDB_Open",
             CallingConvention = CallingConvention.Cdecl,
             ExactSpelling = true
             )]
        public static extern IntPtr TDB_Open(IntPtr pSetting, IntPtr pLoginRes);

        [DllImport(
             "TDB_API_Windows_v2.dll",
             EntryPoint = "TDB_OpenProxy",
             CallingConvention = CallingConvention.Cdecl,
             ExactSpelling = true
             )]
        public static extern IntPtr TDB_Open(IntPtr pSetting, IntPtr pProxySetting, IntPtr pLoginRes);

        [DllImport(
             "TDB_API_Windows_v2.dll",
             EntryPoint = "TDB_Close",
             CallingConvention = CallingConvention.Cdecl,
             ExactSpelling = true
             )]
        public static extern Int32 TDB_Close(IntPtr hTdb);

        [DllImport(
             "TDB_API_Windows_v2.dll",
             EntryPoint = "TDB_GetCodeTable",
             CallingConvention = CallingConvention.Cdecl,
             ExactSpelling = true
             )]
        public static extern Int32 TDB_GetCodeTable(IntPtr hTdb, IntPtr szMarket, IntPtr ppCodeTable, IntPtr pCount);

        [DllImport(
             "TDB_API_Windows_v2.dll",
             EntryPoint = "TDB_Free",
             CallingConvention = CallingConvention.Cdecl,
             ExactSpelling = true
             )]
        public static extern void TDB_Free(IntPtr pArr);

        [DllImport(
             "TDB_API_Windows_v2.dll",
             EntryPoint = "TDB_GetKLine",
             CallingConvention = CallingConvention.Cdecl,
             ExactSpelling = true
             )]
        public static extern Int32 TDB_GetKLine(IntPtr hTdb, IntPtr pReq, IntPtr pData, IntPtr pCount);

        [DllImport(
             "TDB_API_Windows_v2.dll",
             EntryPoint = "TDB_GetTick",
             CallingConvention = CallingConvention.Cdecl,
             ExactSpelling = true
             )]
        public static extern Int32 TDB_GetTick(IntPtr hTdb, IntPtr pReq, IntPtr pData, IntPtr pCount);

        [DllImport(
             "TDB_API_Windows_v2.dll",
             EntryPoint = "TDB_GetTickAB",
             CallingConvention = CallingConvention.Cdecl,
             ExactSpelling = true
             )]
        public static extern Int32 TDB_GetTickAB(IntPtr hTdb, IntPtr pReq, IntPtr pData, IntPtr pCount);

        [DllImport(
             "TDB_API_Windows_v2.dll",
             EntryPoint = "TDB_GetFuture",
             CallingConvention = CallingConvention.Cdecl,
             ExactSpelling = true
             )]
        public static extern Int32 TDB_GetFuture(IntPtr hTdb, IntPtr pReq, IntPtr pData, IntPtr pCount);

        [DllImport(
             "TDB_API_Windows_v2.dll",
             EntryPoint = "TDB_GetFutureAB",
             CallingConvention = CallingConvention.Cdecl,
             ExactSpelling = true
             )]
        public static extern Int32 TDB_GetFutureAB(IntPtr hTdb, IntPtr pReq, IntPtr pData, IntPtr pCount);

        [DllImport(
             "TDB_API_Windows_v2.dll",
             EntryPoint = "TDB_GetTransaction",
             CallingConvention = CallingConvention.Cdecl,
             ExactSpelling = true
             )]
        public static extern Int32 TDB_GetTransaction(IntPtr hTdb, IntPtr pReq, IntPtr pData, IntPtr pCount);

        [DllImport(
             "TDB_API_Windows_v2.dll",
             EntryPoint = "TDB_GetOrderQueue",
             CallingConvention = CallingConvention.Cdecl,
             ExactSpelling = true
             )]
        public static extern Int32 TDB_GetOrderQueue(IntPtr hTdb, IntPtr pReq, IntPtr pData, IntPtr pCount);

        [DllImport(
             "TDB_API_Windows_v2.dll",
             EntryPoint = "TDB_GetOrder",
             CallingConvention = CallingConvention.Cdecl,
             ExactSpelling = true
             )]
        public static extern Int32 TDB_GetOrder(IntPtr hTdb, IntPtr pReq, IntPtr pData, IntPtr pCount);


        [DllImport(
             "TDB_API_Windows_v2.dll",
             EntryPoint = "TDB_GetCodeInfo",
             CallingConvention = CallingConvention.Cdecl,
             ExactSpelling = true
             )]
        public static extern IntPtr TDB_GetCodeInfo(IntPtr hTdb, IntPtr pWindCode);
        
    }
}
