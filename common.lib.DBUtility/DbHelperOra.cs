using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.OracleClient;
using System.Collections;
using System.Data;

namespace common.lib.DBUtility
{
    /// <summary>
    /// 数据访问基础类(基于Oracle) Copyright (C) Maticsoft
    /// 可以用户可以修改满足自己项目的需要。
    /// </summary>
    public abstract class DbHelperOra
    {
        //数据库连接字符串(web.config来配置)，可以动态更改connectionString支持多数据库.	
        public static string connectionString = PubConstant.ConnectionString;
        public static string connectionStringYccy = PubConstant.ConnectionStringYccy;
        public static string connectionStringYccy2 = PubConstant.ConnectionStringYccy2;
        public static string ConnectionPicture = PubConstant.ConnectionPicture;

        public DbHelperOra()
        {
        }

        #region 公用方法

        /// <summary>
        /// 得到指定列名的最大数
        /// </summary>
        /// <param name="FieldName"></param>
        /// <param name="TableName"></param>
        /// <returns></returns>
        public static int GetMaxID(string FieldName, string TableName)
        {
            string strsql = "select max(" + FieldName + ")+1 from " + TableName;
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
        /// 判断指定记录是否存在
        /// </summary>
        /// <param name="strSql"></param>
        /// <returns></returns>
        public static bool Exists(string strSql)
        {
            object obj = GetSingle(strSql);

            int cmdresult;

            if (obj == null) return false;

            cmdresult = int.Parse(obj.ToString());

            if (cmdresult == 0) return false;

            return true;

        }

        /// <summary>
        /// 判断指定记录是否存在
        /// </summary>
        /// <param name="strSql"></param>
        /// <param name="cmdParms"></param>
        /// <returns></returns>
        public static bool Exists(string strSql, params OracleParameter[] cmdParms)
        {
            object obj = GetSingle(strSql, cmdParms);
            int cmdresult;


            if (obj == null) return false;

            cmdresult = int.Parse(obj.ToString());

            if (cmdresult == 0) return false;

            return true;
        }


        #endregion

        /// <summary>
        /// 获取Connection连接对象
        /// </summary>
        /// <returns></returns>
        public static OracleConnection getOracleConnetion()
        {
            OracleConnection conn = new OracleConnection();
            conn.ConnectionString = connectionString;
            if (conn.State == ConnectionState.Closed) conn.Open();
            return conn;
        }

        /// <summary>
        /// 获取Connection连接对象
        /// </summary>
        /// <param name="strConnection">数据库连接字符串</param>
        /// <returns></returns>
        public static OracleConnection getOracleConnetion(string strConnection)
        {
            OracleConnection conn = new OracleConnection();
            conn.ConnectionString = strConnection;
            if (conn.State == ConnectionState.Closed) conn.Open();
            return conn;
        }

        #region  执行简单SQL语句

        /// <summary>
        /// 执行SQL语句，返回影响的记录数
        /// </summary>
        /// <param name="SQLString">SQL语句</param>
        /// <returns>影响的记录数</returns>
        public static int ExecuteSql(string SQLString)
        {
            using (OracleConnection connection = new OracleConnection(connectionString))
            {
                using (OracleCommand cmd = new OracleCommand(SQLString, connection))
                {
                    try
                    {
                        connection.Open();

                        int rows = cmd.ExecuteNonQuery();

                        return rows;
                    }
                    catch (Exception E)
                    {

                        throw new Exception(E.Message);
                    }
                    finally
                    {
                        cmd.Dispose();
                        if (connection != null) connection.Close();
                    }

                }
            }
        }
        /// <summary>
        /// 使用存储过程插入并输出ID
        /// </summary>
        /// <param name="id"></param>
        /// <param name="pName">存储过程名字</param>
        /// <param name="tableName">表名</param>
        /// <param name="commandParameters">存储过程参数</param>
        /// <param name="outParamList">输出参数集合</param>
        /// <param name="returnValueList">返回值集合</param>
        /// <returns></returns>
        public static int ExecuteInsertByP(OracleCommand cmd, string pName, string seqName, List<OracleParameter> commandParameters, Dictionary<string, OracleType> outParamList, Dictionary<string, string> returnValueList)
        {
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = pName;
            //添加输入参数
            foreach (OracleParameter parm in commandParameters)
            {
                cmd.Parameters.Add(parm);
            }
            //输出参数集合
            Dictionary<string, OracleParameter> outParamDic = new Dictionary<string, OracleParameter>();
            //添加输出参数到字典
            foreach (string outParam in outParamList.Keys)
            {
                OracleParameter outParamObj = null;

                if (OracleType.NVarChar == outParamList[outParam])
                {
                    outParamObj = new OracleParameter(outParam, outParamList[outParam], 2000);
                }
                else if (OracleType.VarChar == outParamList[outParam])
                {
                    outParamObj = new OracleParameter(outParam, outParamList[outParam], 2000);
                }
                else
                {
                    outParamObj = new OracleParameter(outParam, outParamList[outParam]);
                }

                outParamObj.Direction = ParameterDirection.Output;//输出参数配置
                cmd.Parameters.Add(outParamObj);
                outParamDic.Add(outParam, outParamObj);
            }

            ////////////////////////////////////
            //需要封装到 Dictionary<string, string>
            ///////////////////////////////
            int val = cmd.ExecuteNonQuery();
            //添加返回值到字典
            foreach (string outParam in outParamDic.Keys)
            {
                returnValueList.Add(outParam, outParamDic[outParam].Value == null ? string.Empty : outParamDic[outParam].Value.ToString());//添加返回值
            }
            cmd.Parameters.Clear();//清除参数

            //待定义输出参数
            return val;
        }
        /// <summary>
        /// 执行多条SQL语句，实现数据库事务。
        /// </summary>
        /// <param name="SQLStringList">多条SQL语句</param>		
        public static void ExecuteSqlTran(ArrayList SQLStringList)
        {
            using (OracleConnection conn = new OracleConnection(connectionString))
            {
                conn.Open();
                OracleCommand cmd = new OracleCommand();
                cmd.Connection = conn;
                OracleTransaction tx = conn.BeginTransaction();
                cmd.Transaction = tx;
                try
                {
                    for (int n = 0; n < SQLStringList.Count; n++)
                    {
                        string strsql = SQLStringList[n].ToString();
                        if (strsql.Trim().Length > 1)
                        {
                            cmd.CommandText = strsql;
                            cmd.ExecuteNonQuery();
                        }
                    }
                    tx.Commit();
                }
                catch (System.Data.OracleClient.OracleException E)
                {
                    tx.Rollback();
                    throw new Exception(E.Message);
                }
                finally
                {
                    if (conn != null) conn.Close();  //关闭数据库连接 何锐 2014.01.21 
                }
            }
        }


        /// <summary>
        /// 执行多条SQL语句，实现数据库事务,并返回受影响的行数
        /// </summary>
        /// <param name="SQLStringList">多条SQL语句</param>		
        public static int ExecuteSqlTran2(ArrayList SQLStringList)
        {
            int R = 0;
            using (OracleConnection conn = new OracleConnection(connectionString))
            {
                conn.Open();
                OracleCommand cmd = new OracleCommand();
                cmd.Connection = conn;
                OracleTransaction tx = conn.BeginTransaction();
                cmd.Transaction = tx;
                try
                {
                    for (int n = 0; n < SQLStringList.Count; n++)
                    {
                        string strsql = SQLStringList[n].ToString();
                        if (strsql.Trim().Length > 1)
                        {
                            cmd.CommandText = strsql;
                            R += cmd.ExecuteNonQuery();
                        }
                    }
                    tx.Commit();
                }
                catch (System.Data.OracleClient.OracleException E)
                {
                    R = 0;
                    tx.Rollback();
                    throw new Exception(E.Message);
                }
                finally
                {
                    if (conn != null) conn.Close();  //关闭数据库连接 何锐 2014.01.21 
                }
            }
            return R;
        }
        /// <summary>
        /// 执行带一个存储过程参数的的SQL语句。
        /// </summary>
        /// <param name="SQLString">SQL语句</param>
        /// <param name="content">参数内容,比如一个字段是格式复杂的文章，有特殊符号，可以通过这个方式添加</param>
        /// <returns>影响的记录数</returns>
        public static int ExecuteSql(string SQLString, string content)
        {
            using (OracleConnection connection = new OracleConnection(connectionString))
            {
                OracleCommand cmd = new OracleCommand(SQLString, connection);
                System.Data.OracleClient.OracleParameter myParameter = new System.Data.OracleClient.OracleParameter("@content", OracleType.NVarChar);
                myParameter.Value = content;
                cmd.Parameters.Add(myParameter);
                try
                {
                    connection.Open();
                    int rows = cmd.ExecuteNonQuery();
                    return rows;
                }
                catch (System.Data.OracleClient.OracleException E)
                {
                    throw new Exception(E.Message);
                }
                finally
                {
                    if (cmd != null) cmd.Dispose();
                    if (connection != null) connection.Close();
                }
            }
        }
        /// <summary>
        /// 向数据库里插入图像格式的字段(和上面情况类似的另一种实例)
        /// </summary>
        /// <param name="strSQL">SQL语句</param>
        /// <param name="fs">图像字节,数据库的字段类型为image的情况</param>
        /// <returns>影响的记录数</returns>
        public static int ExecuteSqlInsertImg(string strSQL, byte[] fs)
        {
            using (OracleConnection connection = new OracleConnection(connectionString))
            {
                OracleCommand cmd = new OracleCommand(strSQL, connection);
                System.Data.OracleClient.OracleParameter myParameter = new System.Data.OracleClient.OracleParameter("@fs", OracleType.LongRaw);
                myParameter.Value = fs;
                cmd.Parameters.Add(myParameter);
                try
                {
                    connection.Open();
                    int rows = cmd.ExecuteNonQuery();
                    return rows;
                }
                catch (System.Data.OracleClient.OracleException E)
                {
                    throw new Exception(E.Message);
                }
                finally
                {
                    if (cmd != null) cmd.Dispose();
                    if (connection != null) connection.Close();
                }
            }
        }

        /// <summary>
        /// 执行一条计算查询结果语句，返回查询结果（object）。
        /// </summary>
        /// <param name="SQLString">计算查询结果语句</param>
        /// <returns>查询结果（object）</returns>
        public static object GetSingle(string SQLString)
        {
            using (OracleConnection connection = new OracleConnection(connectionString))
            {
                using (OracleCommand cmd = new OracleCommand(SQLString, connection))
                {
                    try
                    {
                        connection.Open();
                        object obj = cmd.ExecuteScalar();
                        if ((Object.Equals(obj, null)) || (Object.Equals(obj, System.DBNull.Value)))
                        {
                            connection.Close();
                            return null;
                        }
                        else
                        {
                            connection.Close();
                            return obj;
                        }
                    }
                    catch (System.Data.OracleClient.OracleException e)
                    {
                        throw new Exception(e.Message);
                    }
                    finally
                    {
                        if (connection != null) connection.Close();
                    }
                }
            }
        }


        /// <summary>
        /// 执行一条计算查询结果语句，返回查询结果（object）。
        /// </summary>
        /// <param name="SQLString">计算查询结果语句</param>
        /// <returns>查询结果（object）</returns>
        public static object GetNum(string SQLString)
        {
            using (OracleConnection connection = new OracleConnection(connectionStringYccy))
            {
                using (OracleCommand cmd = new OracleCommand(SQLString, connection))
                {
                    try
                    {
                        connection.Open();
                        object obj = cmd.ExecuteScalar();
                        if ((Object.Equals(obj, null)) || (Object.Equals(obj, System.DBNull.Value)))
                        {
                            connection.Close();
                            return null;
                        }
                        else
                        {
                            connection.Close();
                            return obj;
                        }
                    }
                    catch (System.Data.OracleClient.OracleException e)
                    {
                        throw new Exception(e.Message);
                    }
                    finally
                    {
                        if (connection != null) connection.Close();
                    }
                }
            }
        }


        /// <summary>
        /// 执行查询语句，返回OracleDataReader ( 注意：调用该方法后，一定要对SqlDataReader进行Close )
        /// </summary>
        /// <param name="strSQL">查询语句</param>
        /// <returns>OracleDataReader</returns>
        public static OracleDataReader ExecuteReader(string strSQL)
        {
            OracleConnection connection = new OracleConnection(connectionString);
            OracleCommand cmd = new OracleCommand(strSQL, connection);
            try
            {
                connection.Open();
                OracleDataReader myReader = cmd.ExecuteReader(CommandBehavior.CloseConnection);
                return myReader;
            }
            catch (System.Data.OracleClient.OracleException e)
            {
                throw new Exception(e.Message);
            }

        }
        /// <summary>
        /// 执行查询语句，返回DataSet
        /// </summary>
        /// <param name="SQLString">查询语句</param>
        /// <returns>DataSet</returns>
        public static DataSet Query(string SQLString)
        {
            using (OracleConnection connection = new OracleConnection(connectionString))
            {
                DataSet ds = new DataSet();
                try
                {
                    connection.Open();
                    OracleDataAdapter command = new OracleDataAdapter(SQLString, connection);
                    command.Fill(ds, "ds");
                }
                catch (System.Data.OracleClient.OracleException ex)
                {
                    throw new Exception(ex.Message);
                }
                finally
                {
                    if (connection != null) connection.Close();
                }
                return ds;
            }
        }

        /// <summary>
        /// 执行远程查验查询语句，返回DataSet
        /// </summary>
        /// <param name="SQLString">查询语句</param>
        /// <returns>DataSet</returns>
        public static DataSet QueryYCCY(string SQLString)
        {
            using (OracleConnection connection = new OracleConnection(connectionStringYccy))
            {
                DataSet ds = new DataSet();
                try
                {
                    connection.Open();
                    OracleDataAdapter command = new OracleDataAdapter(SQLString, connection);
                    command.Fill(ds, "ds");
                    connection.Close();
                }
                catch (System.Data.OracleClient.OracleException ex)
                {
                    connection.Close();
                    throw new Exception(ex.Message);
                }
                return ds;
            }
        }

        #endregion

        #region 执行带参数的SQL语句

        /// <summary>
        /// 执行SQL语句，返回影响的记录数
        /// </summary>
        /// <param name="SQLString">SQL语句</param>
        /// <returns>影响的记录数</returns>
        public static int ExecuteSql(string SQLString, params OracleParameter[] cmdParms)
        {
            using (OracleConnection connection = new OracleConnection(connectionString))
            {
                using (OracleCommand cmd = new OracleCommand())
                {
                    try
                    {
                        PrepareCommand(cmd, connection, null, SQLString, cmdParms);
                        int rows = cmd.ExecuteNonQuery();
                        cmd.Parameters.Clear();
                        connection.Close();
                        return rows;
                    }
                    catch (System.Data.OracleClient.OracleException E)
                    {

                        throw new Exception(E.Message);
                    }
                    finally
                    {
                        if (connection != null) connection.Close();
                    }
                }
            }
        }


        /// <summary>
        /// 执行SQL语句，返回影响的记录数
        /// </summary>
        /// <param name="SQLString">SQL语句</param>
        /// <returns>影响的记录数</returns>
        public static bool ExecuteNonQueryCount(string SQLString)
        {
            OracleConnection connection = new OracleConnection(connectionString);
            try
            {
                OracleCommand cmd = new OracleCommand(SQLString, connection);
                connection.Open();
                int rows = cmd.ExecuteNonQuery();
                cmd.Parameters.Clear();
                if (rows != 0)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (System.Data.OracleClient.OracleException E)
            {
                throw new Exception(E.Message);

            }
            finally
            {
                if (connection != null) connection.Close();
            }
        }


        /// <summary>
        /// 执行SQL语句，返回一个实际条数
        /// </summary>
        /// <param name="SQLString">SQL语句</param>
        /// <returns>影响的记录数</returns>
        public static int ExecuteSqltruecount(string SQLString)
        {
            OracleConnection connection = new OracleConnection(connectionString);
            try
            {
                OracleCommand cmd = new OracleCommand(SQLString, connection);
                connection.Open();
                int rows = Convert.ToInt32(cmd.ExecuteScalar());
                cmd.Parameters.Clear();
                return rows;
            }
            catch (System.Data.OracleClient.OracleException E)
            {
                throw new Exception(E.Message);
            }
            finally
            {
                if (connection != null) connection.Close();
            }


        }


        /// <summary>
        /// 执行SQL语句，返回一个OracleDataReader
        /// </summary>
        /// <param name="SQLString">SQL语句</param>
        /// <returns>影响的记录数</returns>
        public static OracleDataReader ExecuteSqlReder(string SQLString)
        {
            OracleConnection connection = new OracleConnection(connectionString);
            try
            {
                OracleCommand cmd = new OracleCommand(SQLString, connection);
                connection.Open();
                OracleDataReader rows = cmd.ExecuteReader(); ;
                cmd.Parameters.Clear();
                return rows;
            }
            catch (System.Data.OracleClient.OracleException E)
            {
                throw new Exception(E.Message);
            }
        }




        /// <summary>
        /// 执行多条SQL语句，实现数据库事务。
        /// </summary>
        /// <param name="SQLStringList">SQL语句的哈希表（key为sql语句，value是该语句的OracleParameter[]）</param>
        public static int ExecuteSqlTran(Hashtable SQLStringList)
        {
            int ret = 0;

            using (OracleConnection conn = new OracleConnection(connectionString))
            {
                conn.Open();

                using (OracleTransaction trans = conn.BeginTransaction())
                {
                    OracleCommand cmd = new OracleCommand();
                    try
                    {
                        //循环
                        foreach (DictionaryEntry myDE in SQLStringList)
                        {
                            string cmdText = myDE.Key.ToString();
                            cmd.CommandText = cmdText;
                            OracleParameter[] cmdParms = null;
                            if (myDE.Value.ToString() != "") cmdParms = (OracleParameter[])myDE.Value;

                            PrepareCommand(cmd, conn, trans, cmdText, cmdParms);


                            int rows = cmd.ExecuteNonQuery();

                            cmd.Parameters.Clear();

                        }

                        trans.Commit();
                        ret = 1;
                    }
                    catch (Exception ex)
                    {

                        trans.Rollback();


                        ret = 0;

                    }
                    finally
                    {

                        //关闭连接
                        //if (conn.State == ConnectionState.Open) conn.Close();   //此句不能关闭数据库连接 何锐 2014.01.21 
                        if (conn != null) conn.Close();
                    }
                }

            }

            return ret;
        }

        /// <summary>
        /// 执行多条SQL语句，实现数据库事务。
        /// </summary>
        /// <param name="SQLStringList"></param>
        public static int ExecuteSqlTran(List<string> SQLStringList)
        {
            using (OracleConnection conn = new OracleConnection(connectionString))
            {
                conn.Open();
                using (OracleTransaction trans = conn.BeginTransaction())
                {
                    OracleCommand cmd = new OracleCommand();
                    try
                    {
                        //循环
                        foreach (string myDE in SQLStringList)
                        {
                            string cmdText = myDE.ToString();
                            OracleParameter[] cmdParms = null;

                            PrepareCommand(cmd, conn, trans, cmdText, cmdParms);
                            int rows = cmd.ExecuteNonQuery();
                            cmd.Parameters.Clear();
                            // if (rows < 1) throw new Exception("执行失败");
                        }
                        trans.Commit();
                        return 1;
                    }
                    catch
                    {
                        if (trans != null) trans.Rollback();
                        //throw;
                        return 0;
                    }
                    finally
                    {
                        if (conn != null) conn.Close();
                    }
                }
            }
        }



        /// <summary>
        /// 执行一条计算查询结果语句，返回查询结果（object）。
        /// </summary>
        /// <param name="SQLString">计算查询结果语句</param>
        /// <returns>查询结果（object）</returns>
        public static object GetSingle(string SQLString, params OracleParameter[] cmdParms)
        {
            using (OracleConnection connection = new OracleConnection(connectionString))
            {
                using (OracleCommand cmd = new OracleCommand())
                {
                    try
                    {
                        PrepareCommand(cmd, connection, null, SQLString, cmdParms);
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
                    catch (System.Data.OracleClient.OracleException e)
                    {
                        throw new Exception(e.Message);
                    }
                    finally
                    {
                        if (connection != null) connection.Close();

                    }
                }
            }
        }

        /// <summary>
        /// 执行查询语句，返回OracleDataReader ( 注意：调用该方法后，一定要对SqlDataReader进行Close )
        /// </summary>
        /// <param name="strSQL">查询语句</param>
        /// <returns>OracleDataReader</returns>
        public static OracleDataReader ExecuteReader(string SQLString, params OracleParameter[] cmdParms)
        {
            OracleConnection connection = new OracleConnection(connectionString);
            OracleCommand cmd = new OracleCommand();
            try
            {
                PrepareCommand(cmd, connection, null, SQLString, cmdParms);
                OracleDataReader myReader = cmd.ExecuteReader(CommandBehavior.CloseConnection);
                cmd.Parameters.Clear();
                return myReader;
            }
            catch (System.Data.OracleClient.OracleException e)
            {
                throw new Exception(e.Message);
            }

        }

        /// <summary>
        /// 执行查询语句，返回DataSet
        /// </summary>
        /// <param name="SQLString">查询语句</param>
        /// <returns>DataSet</returns>
        public static DataSet Query(string SQLString, params OracleParameter[] cmdParms)
        {
            using (OracleConnection connection = new OracleConnection(connectionString))
            {
                OracleCommand cmd = new OracleCommand();
                PrepareCommand(cmd, connection, null, SQLString, cmdParms);
                using (OracleDataAdapter da = new OracleDataAdapter(cmd))
                {
                    DataSet ds = new DataSet();
                    try
                    {
                        da.Fill(ds, "ds");
                        cmd.Parameters.Clear();
                    }
                    catch (System.Data.OracleClient.OracleException ex)
                    {
                        throw new Exception(ex.Message);
                    }
                    finally
                    {
                        if (connection != null) connection.Close();
                    }

                    return ds;
                }
            }
        }


        private static void PrepareCommand(OracleCommand cmd, OracleConnection conn, OracleTransaction trans, string cmdText, OracleParameter[] cmdParms)
        {
            if (conn.State != ConnectionState.Open)
                conn.Open();
            cmd.Connection = conn;
            cmd.CommandText = cmdText;
            if (trans != null)
                cmd.Transaction = trans;
            cmd.CommandType = CommandType.Text;//cmdType;
            if (cmdParms != null)
            {
                foreach (OracleParameter parm in cmdParms)
                    cmd.Parameters.Add(parm);
            }
        }

        #endregion

        #region 存储过程操作

        /// <summary>
        /// 执行存储过程 返回SqlDataReader ( 注意：调用该方法后，一定要对SqlDataReader进行Close )
        /// </summary>
        /// <param name="storedProcName">存储过程名</param>
        /// <param name="parameters">存储过程参数</param>
        /// <returns>OracleDataReader</returns>
        public static OracleDataReader RunProcedure(string storedProcName, IDataParameter[] parameters)
        {
            OracleConnection connection = new OracleConnection(connectionString);
            OracleDataReader returnReader;
            connection.Open();
            OracleCommand command = BuildQueryCommand(connection, storedProcName, parameters);
            command.CommandType = CommandType.StoredProcedure;
            returnReader = command.ExecuteReader(CommandBehavior.CloseConnection);
            return returnReader;
        }


        /// <summary>
        /// 执行存储过程
        /// </summary>
        /// <param name="storedProcName">存储过程名</param>
        /// <param name="parameters">存储过程参数</param>
        /// <param name="tableName">DataSet结果中的表名</param>
        /// <returns>DataSet</returns>
        public static DataSet RunProcedure(string storedProcName, IDataParameter[] parameters, string tableName)
        {
            using (OracleConnection connection = new OracleConnection(connectionString))
            {

                DataSet dataSet = new DataSet();
                try
                {
                    connection.Open();
                    OracleDataAdapter sqlDA = new OracleDataAdapter();
                    sqlDA.SelectCommand = BuildQueryCommand(connection, storedProcName, parameters);
                    sqlDA.Fill(dataSet, tableName);
                    //connection.Close();
                }
                catch (Exception ex)
                {
                    throw ex;
                }
                finally
                {
                    if (connection != null) connection.Close();
                }

                return dataSet;
            }
        }


        /// <summary>
        /// 构建 OracleCommand 对象(用来返回一个结果集，而不是一个整数值)
        /// </summary>
        /// <param name="connection">数据库连接</param>
        /// <param name="storedProcName">存储过程名</param>
        /// <param name="parameters">存储过程参数</param>
        /// <returns>OracleCommand</returns>
        private static OracleCommand BuildQueryCommand(OracleConnection connection, string storedProcName, IDataParameter[] parameters)
        {
            OracleCommand command = new OracleCommand(storedProcName, connection);
            command.CommandType = CommandType.StoredProcedure;
            foreach (OracleParameter parameter in parameters)
            {
                command.Parameters.Add(parameter);
            }
            return command;
        }

        /// <summary>
        /// 执行存储过程，返回影响的行数		
        /// </summary>
        /// <param name="storedProcName">存储过程名</param>
        /// <param name="parameters">存储过程参数</param>
        /// <param name="rowsAffected">影响的行数</param>
        /// <returns></returns>
        public static int RunProcedure(string storedProcName, IDataParameter[] parameters, out int rowsAffected)
        {
            using (OracleConnection connection = new OracleConnection(connectionString))
            {
                int result = 0;
                try
                {
                    connection.Open();
                    OracleCommand command = BuildIntCommand(connection, storedProcName, parameters);
                    rowsAffected = command.ExecuteNonQuery();
                    result = (int)command.Parameters["ReturnValue"].Value;
                    //Connection.Close();
                }
                catch (Exception ex)
                {
                    throw ex;
                }
                finally
                {
                    if (connection != null) connection.Close();

                }
                return result;
            }
        }

        /// <summary>
        /// 创建 OracleCommand 对象实例(用来返回一个整数值)	
        /// </summary>
        /// <param name="storedProcName">存储过程名</param>
        /// <param name="parameters">存储过程参数</param>
        /// <returns>OracleCommand 对象实例</returns>
        private static OracleCommand BuildIntCommand(OracleConnection connection, string storedProcName, IDataParameter[] parameters)
        {
            OracleCommand command = BuildQueryCommand(connection, storedProcName, parameters);
            command.Parameters.Add(new OracleParameter("ReturnValue",
                OracleType.Int32, 4, ParameterDirection.ReturnValue,
                false, 0, 0, string.Empty, DataRowVersion.Default, null));
            return command;
        }



        /// <summary>
        /// 执行存储过程，返回执行结果   
        /// </summary>
        /// <param name="storedProcName">存储过程名</param>
        /// <param name="parameters">存储过程参数</param>
        /// <returns></returns>
        public static bool ExcuteCommandProc(string storedProcName, IDataParameter[] parameters, int num)
        {
            bool rs = true;
            OracleConnection conn = new OracleConnection(connectionString);
            try
            {
                conn.Open();//打开
                OracleCommand cmd = conn.CreateCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = storedProcName;//存储过程名
                cmd.Parameters.AddRange(parameters);//参数

                cmd.ExecuteNonQuery();//执行

                int rows;
                rows = Convert.ToInt32(parameters[num].Value.ToString());//返回值，“1”为成功，“0”为失败

                // Common.Logger.Info("“1”为成功，“0”为失败" + rows);
                string message = parameters[num + 1].Value.ToString();

            }
            catch (Exception ex)
            {
                //Common.Logger.Error("新增角色失败：" + ex);

                rs = false;
            }
            finally
            {
                if (conn != null) conn.Close();//关闭连接
            }

            return rs;
        }


        /// <summary>
        /// 执行存储过程，返回执行结果   
        /// </summary>
        /// <param name="storedProcName">存储过程名</param>
        /// <param name="parameters">存储过程参数</param>
        /// <returns></returns>
        public static int ExcuteJclwProc(string storedProcName, IDataParameter[] parameters, int num)
        {
            int rs = 0;
            OracleConnection conn = new OracleConnection(connectionString);
            try
            {
                conn.Open();//打开
                OracleCommand cmd = conn.CreateCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = storedProcName;//存储过程名
                cmd.Parameters.AddRange(parameters);//参数

                cmd.ExecuteNonQuery();//执行

                rs = Convert.ToInt32(parameters[num].Value.ToString());//返回值，“1”为成功，“0”为失败

                // Common.Logger.Info("“1”为成功，“0”为失败" + rows);
                string message = parameters[num + 1].Value.ToString();

            }
            catch (Exception ex)
            {
                //Common.Logger.Error("新增角色失败：" + ex);

                rs = 0;
            }
            finally
            {
                if (conn != null) conn.Close();//关闭连接
            }

            return rs;
        }

        /// <summary>
        /// 执行存储过程，返回执行结果   删除Orcl数据库
        /// </summary>
        /// <param name="storedProcName">存储过程名</param>
        /// <param name="parameters">存储过程参数</param>
        /// <returns></returns>
        public static bool ExcuteOrclProc(string storedProcName, IDataParameter[] parameters, int num)
        {
            bool rs = true;
            OracleConnection conn = new OracleConnection(connectionStringYccy);
            try
            {
                conn.Open();//打开
                OracleCommand cmd = conn.CreateCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = storedProcName;//存储过程名
                cmd.Parameters.AddRange(parameters);//参数

                cmd.ExecuteNonQuery();//执行

                int rows;
                rows = Convert.ToInt32(parameters[num].Value.ToString());//返回值，“1”为成功，“0”为失败

                // Common.Logger.Info("“1”为成功，“0”为失败" + rows);
                string message = parameters[num + 1].Value.ToString();

            }
            catch (Exception ex)
            {
                //Common.Logger.Error("新增角色失败：" + ex);

                rs = false;
            }
            finally
            {
                if (conn != null) conn.Close();//关闭连接
            }

            return rs;
        }


        /// <summary>
        /// 执行存储过程，判断车辆是否可以撤销
        /// </summary>
        /// <param name="storedProcName"></param>
        /// <param name="parameters"></param>
        /// <param name="num"></param>
        /// <returns></returns>
        public static string ExcuteOrclCX(string storedProcName, OracleParameter[] parameters, int num, out int rs)
        {
            //int rs = 0;
            string mes = "";
            OracleConnection conn = new OracleConnection(connectionStringYccy);
            try
            {
                conn.Open();//打开
                OracleCommand cmd = conn.CreateCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = storedProcName;//存储过程名
                cmd.Parameters.AddRange(parameters);//参数

                cmd.ExecuteNonQuery();//执行

                mes = parameters[num + 1].Value.ToString();

                rs = Convert.ToInt32(parameters[num].Value.ToString());


            }
            catch (Exception ex)
            {
                rs = -1;
            }
            finally
            {
                if (conn != null) conn.Close();//关闭连接
            }

            return mes;
        }

        /// <summary>
        /// 执行存储过程，检验是否ORCL数据库照片是否相等
        /// </summary>
        /// <param name="storedProcName"></param>
        /// <param name="parameters"></param>
        /// <param name="num"></param>
        /// <returns></returns>
        public static bool ExcuteOrclSelect(string storedProcName, OracleParameter[] parameters, int num, out int rs)
        {
            bool bs = true;

            rs = 0;

            string mes = "";

            OracleConnection conn = new OracleConnection(connectionStringYccy);
            try
            {
                conn.Open();//打开
                OracleCommand cmd = conn.CreateCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = storedProcName;//存储过程名
                cmd.Parameters.AddRange(parameters);//参数

                cmd.ExecuteNonQuery();//执行

                mes = parameters[num + 1].Value.ToString();

                rs = Convert.ToInt32(parameters[num].Value.ToString());

            }
            catch (Exception ex)
            {
                bs = false;
            }
            finally
            {
                if (conn != null) conn.Close();//关闭连接
            }

            return bs;
        }

        public static DataSet ExcuteQueryOrcl(string sql)
        {
            using (OracleConnection connection = new OracleConnection(connectionStringYccy))
            {
                DataSet ds = new DataSet();
                try
                {
                    connection.Open();
                    OracleDataAdapter command = new OracleDataAdapter(sql, connection);
                    command.Fill(ds, "ds");
                }
                catch (System.Data.OracleClient.OracleException ex)
                {
                    throw new Exception(ex.Message);
                }
                finally
                {
                    if (connection != null) connection.Close();
                }
                return ds;
            }

        }

        /// <summary>
        /// 获取车辆的登记照片
        /// </summary>
        /// <param name="sql"></param>
        /// <returns></returns>
        public static DataTable ExcuteQueryPicture(string sql)
        {
            using (OracleConnection connection = new OracleConnection(ConnectionPicture))
            {
                DataSet ds = new DataSet();
                try
                {
                    connection.Open();
                    OracleDataAdapter command = new OracleDataAdapter(sql, connection);
                    command.Fill(ds, "ds");
                }
                catch (System.Data.OracleClient.OracleException ex)
                {
                    throw new Exception(ex.Message);
                }
                finally
                {
                    if (connection != null) connection.Close();
                }
                return ds.Tables[0];
            }

        }
        #endregion

    }
}