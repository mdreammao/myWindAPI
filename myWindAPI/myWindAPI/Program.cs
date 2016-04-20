/**
#######################################################
期权期货数据整理和存储程序。
利用万德API以及万德TDB数据库，提取期权和期货盘口数据。
并且在这基础上，计算期权盘口价格对应的波动率和delta。
作者：毛衡
时间：2016-03-04
版本：v1.0.0
#######################################################
1、更新了存储50etf数据的部分。
2、修改了tradeDays的读取部分，防止重复从万德数据库中读取
数据。
3、修改了optionInformation的读取部分，防止重复从万德数据
库中读取数据。
作者：毛衡
时间：2016-03-07
版本：v1.0.1
#######################################################
1、从TDB数据库中获取期权数据。
2、根据50etf数据以及期权数据，计算期权对应的希腊值和开仓保证金。
作者：毛衡
时间：2016-03-08
版本：v1.0.2
#######################################################
1、将整理好的TDB期权数据存入本地数据库。
2、修正了duration的数据格式，是的其更贴近现实。
作者：毛衡
时间：2016-03-09
版本：v1.0.3
#######################################################
1、处理了计算波动率的bug，比如遇到期权价格为0或者期权快到期
时候的异常处理。
2、将整理好的IH期货数据存入本地数据库。按合约代码存储，主要
记录当月的数据和下月的数据。
作者：毛衡
时间：2016-03-14
版本：v1.0.4
#######################################################
1、对期权记录数的结构进行更新，添加了标的价格这个字段。
作者：毛衡
时间：2016-04-13
版本：v1.0.5
#######################################################
**/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WAPIWrapperCSharp;
using WindCommon;
using TDBAPI;

namespace myWindAPI
{
    class Program
    {
        static void Main(string[] args)
        {
            //WindTDBData myTDBData = new WindTDBData("sh", 20160301, 20160331, "50etf");
            //WindTDBData myTDBData2 = new WindTDBData("sh", 20160101, 20160331, "option");
            //WindTDBData myTDBData3 = new WindTDBData("CFE", 20160101, 20150331, "ih");
            //WindTDBData myTDBData4 = new WindTDBData("sh", 20160301, 20160331, "50etfOrder");
            WindTDBData myTDBData5 = new WindTDBData("dce", 20100501, 20151231, "commodity");
        }
    }
}
