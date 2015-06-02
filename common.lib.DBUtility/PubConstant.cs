using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;

namespace common.lib.DBUtility
{

    public class PubConstant
    {
        /// <summary>
        /// 获取连接字符串
        /// </summary>
        public static string ConnectionString
        {
            get
            {
                string _connectionString = ConfigurationManager.AppSettings["ConnectionStr"].ToString();
                string ConStringEncrypt = ConfigurationManager.AppSettings["ConStringEncrypt"];

                if (ConStringEncrypt == "true")
                {
                    _connectionString = DESEncrypt.Decrypt(_connectionString);
                }

                return _connectionString;
            }
        }

        /// <summary>
        /// 得到web.config里配置项的数据库连接字符串。
        /// </summary>
        /// <param name="configName"></param>
        /// <returns></returns>
        public static string GetConnectionString(string configName)
        {
            string connectionString = ConfigurationManager.AppSettings[configName];
            string ConStringEncrypt = ConfigurationManager.AppSettings["ConStringEncrypt"];
            if (ConStringEncrypt == "true")
            {
                connectionString = DESEncrypt.Decrypt(connectionString);
            }
            return connectionString;
        }

        public static string MySqlStr
        {
            get
            {
                string _connectionString = ConfigurationManager.ConnectionStrings["MySqlStr"].ConnectionString;
                string ConStringEncrypt = ConfigurationManager.AppSettings["ConStringEncrypt"];
                if (ConStringEncrypt == "true")
                {
                    _connectionString = DESEncrypt.Decrypt(_connectionString);
                }
                return _connectionString;
            }
        }


        public static string ConnectionStringYccy
        {
            get
            {
                string _connectionString = ConfigurationManager.AppSettings["ConnectionStringYCCY"];
                string ConStringEncrypt = ConfigurationManager.AppSettings["ConStringEncrypt"];
                if (ConStringEncrypt == "true")
                {
                    _connectionString = DESEncrypt.Decrypt(_connectionString);
                }
                return _connectionString;
            }
        }

        public static string ConnectionStringYccy2
        {
            get
            {
                string _connectionString = ConfigurationManager.AppSettings["ConnectionStringYCCY2"];
                string ConStringEncrypt = ConfigurationManager.AppSettings["ConStringEncrypt"];
                if (ConStringEncrypt == "true")
                {
                    _connectionString = DESEncrypt.Decrypt(_connectionString);
                }
                return _connectionString;
            }
        }

        /// <summary>
        /// 获取车辆的登记照片
        /// </summary>
        public static string ConnectionPicture
        {
            get
            {
                string _connectionString = ConfigurationManager.AppSettings["ConnectionPicture"];
                string ConStringEncrypt = ConfigurationManager.AppSettings["ConStringEncrypt"];
                if (ConStringEncrypt == "true")
                {
                    _connectionString = DESEncrypt.Decrypt(_connectionString);
                }
                return _connectionString;
            }
        }

        public static string SqLiteConnection
        {
            get
            {
                string _connectionString = ConfigurationManager.ConnectionStrings["SQliteConnString"].ToString();
                //string ConStringEncrypt = ConfigurationManager.AppSettings["ConStringEncrypt"];
                //if (ConStringEncrypt == "true")
                //{
                //    _connectionString = DESEncrypt.Decrypt(_connectionString);
                //}
                return _connectionString;
            }
        }
    }
}
