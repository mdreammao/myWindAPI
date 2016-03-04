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
}
