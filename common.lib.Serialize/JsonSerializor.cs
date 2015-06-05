using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Xml;

namespace common.lib.Serialize
{
    /**创建时间:20150525
     * @蒲贵洪
     * *
     * JSON 序列化 基础类
     * 
     * 功能描述:
     * -20150525
     * 1.ChangeDatasetToDataGridJson    将DataSet转换为DataGrid格式数据基础工具
     * 2.ChangeDataSetToComboxJson      将DataSet转换为Combobox格式数据基础工具
     * 3.ChangeDataSetToMenuJson        将DataSet转换为Menue格式数据基础工具
     * 4.ChangeDataTableToTreeJson      将DataTable转换为Tree格式数据基础工具
     * 5.ChangeDataTableToTreeGridJson  将DataTable转换为TreeGrid格式数据基础工具
     * 6.ChangeXmlToJson                将XML转换为JSON格式数据基础工具
     **实现流程
     *
     *
     */
    public class JsonSerializor
    {
        #region 私有构造函数
        public JsonSerializor() { }
        #endregion

        #region 字段
        private static readonly object _locker = new object();

        private static JsonSerializor _jsonSerializor;
        #endregion

        #region 属性
        public static JsonSerializor Instance
        {
            get
            {
                if (_jsonSerializor == null)
                {
                    lock (_locker)
                    {
                        if (_jsonSerializor == null)
                        {
                            _jsonSerializor = new JsonSerializor();
                        }
                    }
                }
                return _jsonSerializor;
            }
        }
        #endregion

        #region 方法
        /// <summary>
        /// 将DataSet 转换为 datagrid json数据格式
        /// </summary>
        /// <param name="ds"></param>
        /// <returns></returns>
        public string ChangeDatasetToDataGridJson(DataSet ds)
        {
            if (ds == null || ds.Tables.Count <= 0 || ds.Tables[0].Rows.Count <= 0)
            {
                //如果查询到的数据为空则返回标记ok:false
                return "{\"total\":" + ds.Tables[0].Rows.Count + "}";
            }
            StringBuilder sbu = new StringBuilder();
            sbu.Append("{\"total\":" + ds.Tables[0].Rows.Count + ",");
            foreach (DataTable dt in ds.Tables)
            {
                //sbu.Append(string.Format("\"{0}\":[", dt.TableName));
                sbu.Append(string.Format("\"rows\":["));

                foreach (DataRow dr in dt.Rows)
                {
                    sbu.Append("{");
                    for (int i = 0; i < dr.Table.Columns.Count; i++)
                    {
                        sbu.AppendFormat("\"{0}\":\"{1}\",", dr.Table.Columns[i].ColumnName.Replace("\"", "\\\"").Replace("\'", "\\\'"), ObjToStr(dr[i]).Replace("\"", "\\\"").Replace("\'", "\\\'")).Replace(Convert.ToString((char)13), "\\r\\n").Replace(Convert.ToString((char)10), "\\r\\n");
                    }
                    sbu.Remove(sbu.ToString().LastIndexOf(','), 1);
                    sbu.Append("},");
                }

                sbu.Remove(sbu.ToString().LastIndexOf(','), 1);
                sbu.Append("],");
            }
            sbu.Remove(sbu.ToString().LastIndexOf(','), 1);
            sbu.Append("}");
            return sbu.ToString();
        }

        /// <summary>
        /// 将DataSet 转换成Combox 格式数据
        /// </summary>
        /// <param name="ds"></param>
        /// <returns></returns>
        public string ChangeDataSetToComboxJson(DataSet ds)
        {
            StringBuilder _strSb = new StringBuilder();
            if (ds == null || ds.Tables.Count <= 0 || ds.Tables[0].Rows.Count <= 0)
            {
                //如果查询到的数据为空则返回total为0
                return "{\"total\":" + ds.Tables[0].Rows.Count + "}";
            }
            foreach (DataTable dt in ds.Tables)
            {
                _strSb.Append("[");

                foreach (DataRow dr in dt.Rows)
                {
                    _strSb.Append("{");
                    for (int i = 0; i < dr.Table.Columns.Count; i++)
                    {
                        _strSb.AppendFormat("\"{0}\":\"{1}\",", dr.Table.Columns[i].ColumnName.Replace("\"", "\\\"").Replace("\'", "\\\'"), ObjToStr(dr[i]).Replace("\"", "\\\"").Replace("\'", "\\\'")).Replace(Convert.ToString((char)13), "\\r\\n").Replace(Convert.ToString((char)10), "\\r\\n");
                    }
                    _strSb.Remove(_strSb.ToString().LastIndexOf(','), 1);
                    _strSb.Append("},");
                }

                _strSb.Remove(_strSb.ToString().LastIndexOf(','), 1);
                _strSb.Append("],");
            }
            _strSb.Remove(_strSb.ToString().LastIndexOf(','), 1);
            return _strSb.ToString();

        }


        /// <summary>
        /// 将DataSet 转换成Menu 格式的json数据
        /// </summary>
        /// <param name="ds"></param>
        /// <returns></returns>
        public string ChangeDataSetToMenuJson(DataSet ds, int id, int pid)
        {
            /*
             * 具体实现步骤：
             * 1、接收datatable中的数据字段。
             * 2、遍历节点的子节点，如果子节点为 0 设置为首节点，子节点的结束。
             * 3、id列，父节点列。
             * 4、递归调用，循环的终点是该节点的父节点值是0,是否有子节点如果有子节点。
             * （终止条件，没有子节点）
             * 
             * 组成json数据
             * 
             * 
             */
            string _result = null;
            return _result;



        }

        /// <summary>
        /// 将obj转换为string ,公共方法
        /// </summary>
        /// <param name="ob"></param>
        /// <returns></returns>
        public string ObjToStr(object ob)
        {
            if (ob == null)
            {
                return string.Empty;
            }
            else
                return ob.ToString();
        }
        #endregion

        StringBuilder result = new StringBuilder();
        StringBuilder sb = new StringBuilder();
        public string ChangeDataTableToTreeJson(DataTable table, string idCol, string txtCol, string rela, object pId)
        {
            result.Clear();
            sb.Clear();
            GetTreeJsonByTable(table, idCol, txtCol, rela, pId);
            return result.ToString();
        }

        /// <summary>
        /// 获取JsonTree数据
        /// </summary>
        /// <param name="tabel">源数据</param>
        /// <param name="idCol">ID</param>
        /// <param name="txtCol">数据列</param>
        /// <param name="rela">关系字段</param>
        /// <param name="pId">父级ID</param>
        private void GetTreeJsonByTable(DataTable tabel, string idCol, string txtCol, string rela, object pId)
        {
            result.Append(sb.ToString());
            sb.Clear();
            if (tabel.Rows.Count > 0)
            {
                sb.Append("[");
                //string filer = string.Format("{0}='{1}'", rela, pId);
                string filer = string.Format("{0}={1}", rela, pId);
                DataRow[] rows = tabel.Select(filer);
                if (rows.Length > 0)
                {
                    foreach (DataRow row in rows)
                    {
                        sb.Append("{\"id\":\"" + row[idCol] + "\",\"text\":\"" + row[txtCol] + "\",\"state\":\"open\"");
                        if (tabel.Select(string.Format("{0}='{1}'", rela, row[idCol])).Length > 0)
                        {
                            sb.Append(",\"children\":");
                            GetTreeJsonByTable(tabel, idCol, txtCol, rela, row[idCol]);
                            result.Append(sb.ToString());
                            sb.Clear();
                        }
                        result.Append(sb.ToString());
                        sb.Clear();
                        sb.Append("},");
                    }
                    sb = sb.Remove(sb.Length - 1, 1);
                }
                sb.Append("]");
                result.Append(sb.ToString());
                sb.Clear();
            }
        }

        /// <summary>
        /// DataTable 转换成TreeGrid格式 数据
        /// </summary>
        /// <param name="table">原始数据表</param>
        /// <param name="idCol">主键</param>
        /// <param name="txtCol">列值</param>
        /// <param name="rela">关系列</param>
        /// <param name="pId">父节点列</param>
        /// <param name="val1"></param>
        /// <returns></returns>
        public string ChangeDataTableToTreeGridJson(DataTable table, string idCol, string txtCol, string rela, object pId, string val1)
        {
            result.Clear();
            sb.Clear();
            GetTreeGridJsonByTable(table, idCol, txtCol, rela, pId, val1);
            return result.ToString();
        }

        private void GetTreeGridJsonByTable(DataTable tabel, string idCol, string txtCol, string rela, object pId, string jylshCol1)
        {
            result.Append(sb.ToString());
            sb.Clear();
            if (tabel.Rows.Count > 0)
            {
                sb.Append("[");
                //string filer = string.Format("{0}='{1}'", rela, pId);
                string filer = string.Format("{0}={1}", rela, pId);
                DataRow[] rows = tabel.Select(filer);
                if (rows.Length > 0)
                {
                    foreach (DataRow row in rows)
                    {
                        sb.Append("{\"id\":\"" + row[idCol] + "\",\"text\":\"" + row[txtCol] + "\",\"state\":\"open\",\"jyjgbh\":\"" + row[jylshCol1] + "\"");
                        if (tabel.Select(string.Format("{0}='{1}'", rela, row[idCol])).Length > 0)
                        {
                            sb.Append(",\"children\":");
                            GetTreeGridJsonByTable(tabel, idCol, txtCol, rela, row[idCol], jylshCol1);
                            result.Append(sb.ToString());
                            sb.Clear();
                        }
                        result.Append(sb.ToString());
                        sb.Clear();
                        sb.Append("},");
                    }
                    sb = sb.Remove(sb.Length - 1, 1);
                }
                sb.Append("]");
                result.Append(sb.ToString());
                sb.Clear();
            }
        }

        /// <summary>
        /// Xml转换成Json  
        /// </summary>
        /// <param name="strXml"></param>
        /// <returns></returns>
        public string ChangeXmlToJson(string strXml)
        {
            try
            {
                XmlDocument doc = new XmlDocument();
                string _json_result;

                doc.LoadXml(strXml);
                _json_result = Newtonsoft.Json.JsonConvert.SerializeXmlNode(doc);
                return _json_result;
            }
            catch (Exception)
            {

                throw;
            }
        }

    }
}
