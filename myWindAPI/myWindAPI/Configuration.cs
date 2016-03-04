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
    }
}
