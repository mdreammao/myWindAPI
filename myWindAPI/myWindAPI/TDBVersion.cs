using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TDBAPI
{
    public class TDBVersion
    {
        public static string GetVersion()
        {
        	  //version: 2352.20140114.1
        	  //version: 2509.20140121.1，加入x64平台。
            //svn版本号.日期.次数
            return "2352.20140114.1";
        }
    }
}
