using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SQLite;
using System.Data;

namespace common.lib.DBUtility
{
    public abstract class DbHelperSQLite
    {
        //public static readonly string connectString = ConfigurationManager.AppSettings["SQliteConnString"];
        public static readonly string connectString = PubConstant.SqLiteConnection;

        public DbHelperSQLite()
        {
        }

        #region 公用方法
        /// <summary>
        /// 判断是否存在某表的某个字段
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="columnName"></param>
        /// <returns></returns>
        public static bool ColumnExists(string tableName, string columnName)
        {
            string sql = "select count(1) from syscolumns where [id] = object_id('" + tableName + "') and [name] =('" + columnName + ")'";
            Object res = GetSingle(sql);
            if (res == null)
            {
                return false;
            }
            return Convert.ToInt32(res) > 0;

        }
        /// <summary>
        /// 获取最大ID
        /// </summary>
        /// <param name="FieldName"></param>
        /// <param name="TableName"></param>
        /// <returns></returns>
        public static int GetMaxID(string FieldName, string TableName)
        {
            string strsql = "select max(" + FieldName + ")+1 from" + TableName;
            object obj = GetSingle(strsql);
            if (obj == null)
            {
                return 1;
            }
            else
            {
                return int.Parse(obj.ToString());
            }

        }
        /// <summary>
        /// 判断是否存在
        /// </summary>
        /// <param name="strSql"></param>
        /// <returns></returns>
        public static bool Exists(string strSql)
        {
            object obj = GetSingle(strSql);
            int cmdresult;
            if ((Object.Equals(obj, null)) || (Object.Equals(obj, System.DBNull.Value)))
            {
                cmdresult = 0;
            }
            else
            {
                cmdresult = int.Parse(obj.ToString());
            }
            if (cmdresult == 0)
            {
                return false;
            }
            else
            {
                return true;
            }
        }
        /// <summary>
        /// 判断是否存在表
        /// </summary>
        /// <param name="TableName"></param>
        /// <returns></returns>
        public static bool TabExists(string TableName)
        {
            string strsql = "select count(*) from sysobjects where id = object_id(N";
            object obj = GetSingle(strsql);
            int cmdresult;
            if ((Object.Equals(obj, null)) || (Object.Equals(obj, System.DBNull.Value)))
            {
                cmdresult = 0;
            }
            else
            {
                cmdresult = int.Parse(obj.ToString());
            }
            if (cmdresult == 0)
            {
                return false;
            }
            else
            {
                return true;
            }

        }
        /// <summary>
        /// 判断是否存在
        /// </summary>
        /// <param name="strSql"></param>
        /// <param name="cmdParms"></param>
        /// <returns></returns>
        public static bool Exists(string strSql, params SQLiteParameter[] cmdParms)
        {
            object obj = GetSingle(strSql, cmdParms);
            int cmdresult;
            if ((Object.Equals(obj, null)) || (Object.Equals(obj, System.DBNull.Value)))
            {
                cmdresult = 0;
            }
            else
            {
                cmdresult = int.Parse(obj.ToString());
            }
            if (cmdresult == 0)
            {
                return false;
            }
            else
            {
                return true;
            }
        }
        #endregion

        #region 执行简单SQL语句
        /// <summary>
        /// 执行SQL语句，返回影响的记录数
        /// </summary>
        /// <param name="sqlString">sql 语句</param>
        /// <returns>影响的记录数</returns>
        public static int ExecuteSql(string sqlString)
        {
            using (SQLiteConnection connection = new SQLiteConnection(connectString))
            {
                using (SQLiteCommand cmd = new SQLiteCommand(sqlString, connection))
                {
                    try
                    {
                        connection.Open();
                        int rows = cmd.ExecuteNonQuery();
                        return rows;
                    }
                    catch (System.Data.SQLite.SQLiteException e)
                    {
                        connection.Close();
                        throw e;
                    }
                    finally
                    {
                        if (connection != null) connection.Close();
                    }
                }
            }
        }

        public static int ExecuteSqlByTime(string sqlString, int times)
        {
            using (SQLiteConnection connection = new SQLiteConnection(connectString))
            {
                using (SQLiteCommand cmd = new SQLiteCommand(sqlString, connection))
                {
                    try
                    {
                        connection.Open();
                        cmd.CommandTimeout = times;
                        int rows = cmd.ExecuteNonQuery();
                        return rows;
                    }
                    catch (System.Data.SQLite.SQLiteException e)
                    {
                        connection.Close();
                        throw e;
                    }
                    finally
                    {
                        if (connection != null) connection.Close();
                    }
                }
            }
        }
        public static object GetSingle(string sqlString, params SQLiteParameter[] cmdParms)
        {
            using (SQLiteConnection connection = new SQLiteConnection(connectString))
            {
                using (SQLiteCommand cmd = new SQLiteCommand())
                {
                    try
                    {
                        PrepareCommand(cmd, connection, null, sqlString, cmdParms);
                        object obj = cmd.ExecuteScalar();
                        cmd.Parameters.Clear();
                        if ((Object.Equals(obj, null)) || (Object.Equals(obj, System.DBNull.Value)))
                        {
                            return null;
                        }
                        else
                        {
                            return obj;
                        }

                    }
                    catch (System.Data.SQLite.SQLiteException e)
                    {

                        throw e;
                    }
                    finally
                    {
                        if (connection != null) connection.Close();
                    }
                }
            }
        }
        /// <summary>
        /// 执行查询语句，返回SqliteDataReader ( 注意：调用该方法后，一定要对SqlDataReader进行Close )
        /// </summary>
        /// <param name="strSql"></param>
        /// <returns></returns>
        public static SQLiteDataReader ExecuteReader(string strSql)
        {
            SQLiteConnection connection = new SQLiteConnection(connectString);
            SQLiteCommand cmd = new SQLiteCommand(strSql, connection);
            try
            {
                connection.Open();
                SQLiteDataReader myReader = cmd.ExecuteReader(CommandBehavior.CloseConnection);
                return myReader;
            }
            catch (System.Data.SQLite.SQLiteException e)
            {

                throw e;
            }
            finally
            {
                if (connection != null) connection.Close();
            }
        }
        /// <summary>
        /// 执行查询语句，返回DataSet
        /// </summary>
        /// <param name="strSql"></param>
        /// <returns></returns>
        public static DataSet Query(string strSql)
        {
            using (SQLiteConnection connection = new SQLiteConnection(connectString))
            {
                DataSet ds = new DataSet();
                try
                {
                    connection.Open();
                    SQLiteDataAdapter command = new SQLiteDataAdapter(strSql, connection);
                    command.Fill(ds, "rows");
                }
                catch (System.Data.SqlClient.SqlException e)
                {

                    throw new Exception(e.Message);
                }
                finally
                {
                    if (connection != null) connection.Close();
                }
                return ds;
            }
        }
        public static DataSet Query(string strSql, int times)
        {
            using (SQLiteConnection connection = new SQLiteConnection(connectString))
            {
                DataSet ds = new DataSet();
                try
                {
                    connection.Open();
                    SQLiteDataAdapter command = new SQLiteDataAdapter(strSql, connection);
                    command.SelectCommand.CommandTimeout = times;
                    command.Fill(ds, "ds");
                }
                catch (System.Data.SqlClient.SqlException e)
                {

                    throw new Exception(e.Message);
                }
                finally
                {
                    if (connection != null) connection.Close();
                }
                return ds;
            }
        }
        public static object GetSingle(string sqlString)
        {
            using (SQLiteConnection connection = new SQLiteConnection(connectString))
            {
                using (SQLiteCommand cmd = new SQLiteCommand(sqlString, connection))
                {
                    try
                    {
                        connection.Open();
                        object obj = cmd.ExecuteScalar();
                        if ((Object.Equals(obj, null)) || (Object.Equals(obj, System.DBNull.Value)))
                        {
                            return null;
                        }
                        else
                        {
                            return obj;
                        }

                    }
                    catch (System.Data.SQLite.SQLiteException e)
                    {
                        connection.Close();
                        throw e;
                    }
                    finally
                    {
                        if (connection != null) connection.Close();
                    }
                }

            }
        }

        public static object GetSingle(string sqlString, int times)
        {
            using (SQLiteConnection connection = new SQLiteConnection(connectString))
            {
                using (SQLiteCommand cmd = new SQLiteCommand(sqlString, connection))
                {
                    try
                    {
                        connection.Open();
                        cmd.CommandTimeout = times;
                        object obj = cmd.ExecuteScalar();
                        if ((Object.Equals(obj, null)) || (Object.Equals(obj, System.DBNull.Value)))
                        {
                            return null;
                        }
                        else
                        {
                            return obj;
                        }
                    }
                    catch (System.Data.SQLite.SQLiteException e)
                    {
                        connection.Close();
                        throw e;
                    }
                    finally
                    {
                        if (connection != null) connection.Close();
                    }
                }

            }
        }

        private static void PrepareCommand(SQLiteCommand cmd, SQLiteConnection conn, SQLiteTransaction trans, string cmdText, params SQLiteParameter[] cmdParms)
        {
            if (conn.State != System.Data.ConnectionState.Open)
                conn.Open();
            cmd.Connection = conn;
            cmd.CommandText = cmdText;
            if (trans != null)
                cmd.Transaction = trans;
            cmd.CommandType = CommandType.Text;
            if (cmdParms != null)
            {
                foreach (SQLiteParameter parameter in cmdParms)
                {
                    if ((parameter.Direction == ParameterDirection.InputOutput || parameter.Direction == ParameterDirection.Input) &&
                    (parameter.Value == null))
                    {
                        parameter.Value = DBNull.Value;
                    }
                    cmd.Parameters.Add(parameter);
                }
            }
        }
        #endregion
    }
}
