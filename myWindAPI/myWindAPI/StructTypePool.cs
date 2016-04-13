using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace myWindAPI
{
    /// <summary>
    /// 存储期权基本信息的结构体。
    /// </summary>
    struct optionFormat
    {
        public int optionCode;
        public string optionName;
        public int startDate;
        public int endDate;
        public string optionType;
        public string executeType;
        public double strike;
        public string market;
    }

    /// <summary>
    /// 存储期权数据的结构体。包含30多个字段。
    /// </summary>
    struct optionDataFormat
    {
        public int optionCode;
        public string optionType;
        public double strike;
        public int startDate;
        public int endDate;
        public int date;
        public int time;
        public int tick;
        public double underlyingPrice;
        public double volumn;
        public double turnover;
        public double accVolumn;
        public double accTurnover;
        public double open;
        public double high;
        public double low;
        public double lastPrice;
        public double preSettle;
        public double preClose;
        public double[] ask, askv, askVolatility,askDelta,bid, bidv,bidVolatility,bidDelta;
        public double openMargin;
        public double midVolatility,midDelta;
    }

    /// <summary>
    /// 需要提取TDB数据的品种信息。
    /// </summary>
    struct TDBdataInformation
    {
        public string market;
        public int startDate;
        public int endDate;
        public string type;
    }
    /// <summary>
    /// 记录TDB连接信息的结构体。
    /// </summary>
    struct TDBsource
    {
        public string IP;
        public string port;
        public string account;
        public string password;
        public TDBsource(string IP, string port, string account, string password)
        {
            this.IP = IP;
            this.port = port;
            this.account = account;
            this.password = password;
        }
    }
}
