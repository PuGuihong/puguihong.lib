using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

namespace common.lib.DataConvertHelper
{
    /**创建时间:20150525
     * @蒲贵洪
     * *
     * DataTable 帮助基础类
     * 功能描述:
     * 1.MergeDataTable 数据比较、合并
     * 
     **实现流程
     *
     *
     */
    public class DataTableHelper
    {
        #region 构造函数
        public DataTableHelper() { }
        #endregion

        #region 字段
        private static readonly object _locker = new object();

        private static DataTableHelper _dataTableTool;
        #endregion

        #region 属性
        public static DataTableHelper Instance
        {
            get
            {
                if (_dataTableTool == null)
                {
                    lock (_locker)
                    {
                        if (_dataTableTool == null)
                        {
                            _dataTableTool = new DataTableHelper();
                        }
                    }
                }
                return _dataTableTool;
            }
        }
        #endregion

        #region 方法
        /// <summary>
        /// 合并两个datatable 公用方法
        /// </summary>
        /// <param name="dt1"></param>
        /// <param name="dt2"></param>
        /// <param name="dt2name"></param>
        /// <param name="dtname"></param>
        /// <returns></returns>
        public DataSet MergeDataTable(DataTable dt1, DataTable dt2, string dt2name, string dtname)
        {
            DataTable dt3 = dt1.Clone();
            for (int i = 0; i < dt2.Columns.Count; i++)
            {
                dt3.Columns.Add(dt2.Columns[i].ColumnName + "_" + dt2name);
            }
            object[] obj = new object[dt3.Columns.Count];
            for (int j = 0; j < dt1.Rows.Count; j++)
            {
                dt1.Rows[j].ItemArray.CopyTo(obj, 0);
                dt3.Rows.Add(obj);
            }

            if (dt1.Rows.Count >= dt2.Rows.Count)
            {
                for (int i = 0; i < dt2.Rows.Count; i++)
                {
                    for (int j = 0; j < dt2.Columns.Count; j++)
                    {
                        dt3.Rows[i][j + dt1.Columns.Count] = dt2.Rows[i][j].ToString();
                    }
                }

            }
            else
            {
                DataRow dr3;
                for (int i = 0; i < dt2.Rows.Count - dt1.Rows.Count; i++)
                {
                    dr3 = dt3.NewRow();
                    dt3.Rows.Add(dr3);
                }
                for (int i = 0; i < dt2.Rows.Count; i++)
                {
                    for (int j = 0; j < dt2.Columns.Count; j++)
                    {
                        dt3.Rows[i][j + dt1.Columns.Count] = dt2.Rows[i][j].ToString();
                    }
                }
            }
            dt3.TableName = dtname;
            DataSet _ds = new DataSet();
            _ds.Tables.Add(dt3);
            return _ds;
        }
    }
        #endregion
}
