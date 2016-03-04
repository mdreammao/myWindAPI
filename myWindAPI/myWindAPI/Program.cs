/**
#######################################################
期权期货数据整理和存储程序。
利用万德API以及万德TDB数据库，提取期权和期货盘口数据。
并且在这基础上，计算期权盘口价格对应的波动率和delta。
作者：毛衡
时间：2016-03-04
版本：v1.0.0
#######################################################

#######################################################
**/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WAPIWrapperCSharp;
using WindCommon;

namespace myWindAPI
{
    class Program
    {
        static void Main(string[] args)
        {
            //获取50etf期权信息。
            OptionInformation myOptionInfo = new OptionInformation(20160101);
        }
        static void DoAPISameple()
        {
            WindAPI w = new WindAPI();
            w.start();

            //wset取沪深300指数成分
            //WindData wd = w.wset("IndexConstituent", "date=20141215;windcode=000300.SH");
            //OutputWindData(wd, "wset");

            //WindData wd = w.wsd("600000.SH,600004.SH", "open", "2014-10-16", "2014-12-16", "");
            //OutputWindData(wd, "wsd");
            WindData d = w.tdays("20150101", "20201213", "");
            object t=d.getDataByFunc("tdays", true);
            WindData optionInformation = w.wset("OptionChain", "date=20150209;us_code=510050.SH;option_var=;month=全部;call_put=全部");
            object[] tt = optionInformation.data as object[];
            Console.WriteLine(optionInformation.data.GetType());
            Console.WriteLine(tt.GetType());
            foreach (var item in tt)
            {
                Console.WriteLine(item.ToString());
                Console.WriteLine(item.GetType());
            }
           
            w.stop();
        }

        static void OutputWindData(WindData wd, string strFuncName)
        {
            string s = WindDataMethod.WindDataToString(wd, strFuncName);
            Console.Write(s);
        }
    }
}
