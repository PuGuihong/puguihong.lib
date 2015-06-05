using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace common.lib.Serialize
{
    /**创建时间:20150525
     * @蒲贵洪
     * *
     * XML 序列化 基础类
     * 
     * 功能描述:
     * -20150525
     * 1.
     * 
     **实现流程
     *
     *
     */
    public class XmlSerializor
    {
        #region 构造函数
        private XmlSerializor() { }
        #endregion

        #region 字段
        private static readonly object _locker = new object();
        private static XmlSerializor _xmlSerializor;
        #endregion

        #region 属性
        public static XmlSerializor Instance
        {
            get
            {
                if (_xmlSerializor == null)
                {
                    lock (_locker)
                    {
                        if (_xmlSerializor == null)
                        {
                            _xmlSerializor = new XmlSerializor();
                        }
                    }

                }
                return _xmlSerializor;
            }
        }
        #endregion

        #region
        /// <summary>
        /// Xml转换成Json  
        /// </summary>
        /// <param name="strXml"></param>
        /// <returns></returns>
        public string XmlToJson(string strXml)
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
        #endregion
    }
}
