using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace myWindAPI
{
   
    /// <summary>
    /// 提供各种配置参数的类。含有各类静态函数。
    /// </summary>
    class Configuration
    {
        /// <summary>
        /// 数据库的名称。
        /// </summary>
        public static string dataBaseName = "Option";
        /// <summary>
        /// 记录50etf的表名称。
        /// </summary>
        public static string tableOf50ETF = "sh510050";
        /// <summary>
        /// 记录所有期权的表名称。
        /// </summary>
        public static string tableOfOptionAll = "optionAll";
        /// <summary>
        /// 保存交易日信息的数据表的名称。
        /// </summary>
        public static string tradeDaysTableName = "myTradeDays";
        /// <summary>
        /// 提供数据库sql连接字符串信息。
        /// </summary>
        public static string connectString= "server=(local);database=Option;Integrated Security=true;";
        /// <summary>
        /// 给定期权标的的名称。
        /// </summary>
        public static string underlyingAsset = "510050.SH";
        /// <summary>
        /// 保存期权合约基本信息的数据表的名称。
        /// </summary>
        public static string optionCodeTableName = "optionCodeInformation";
        /// <summary>
        /// 上交所及中金所连接信息。
        /// </summary>
        public static TDBsource SHsource = new TDBsource("114.80.154.34", "10061", "TD5928909016", "14158777");
        /// <summary>
        /// 商品期货连接信息。
        /// </summary>
        public static TDBsource commoditySource = new TDBsource("114.80.154.34", "10060", "TD5928909014", "13305104");
        /// <summary>
        /// 无风险收益率。
        /// </summary>
        public static double RiskFreeReturn = 0.05;
    }
}
