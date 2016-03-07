using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;

namespace myWindAPI
{
    /// <summary>
    /// 记录一些重复使用的sql功能。
    /// </summary>
    class SqlApplication
    {
        /// <summary>
        /// 判断表是否存在。
        /// </summary>
        /// <param name="dataBaseName">数据库名</param>
        /// <param name="tableName">表名</param>
        /// <returns>返回是否存在表</returns>
        public static bool CheckExist(string dataBaseName,string tableName)
        {
            using (SqlConnection conn = new SqlConnection(Configuration.connectString))
            {
                conn.Open();//打开数据库  
                SqlCommand cmd = conn.CreateCommand();
                cmd.CommandText = "select COUNT(*) from [" + dataBaseName + "].sys.sysobjects where name = '" + tableName + "'";
                try
                {
                    
                    int number =(int)cmd.ExecuteScalar();
                    if (number>0)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                catch (Exception myerror)
                {
                    System.Console.WriteLine(myerror.Message);
                }
            }
                return false;
        }
    }
}
