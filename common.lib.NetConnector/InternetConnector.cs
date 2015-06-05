using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Net.Sockets;

namespace common.lib.NetConnector
{
    /**创建时间:20150525
     * @蒲贵洪
     * *
     * InternetConnector 网络连接 基础类
     * 
     * 功能描述:
     * -20150525
     * 1.
     **实现流程
     *
     *
     */
    public class InternetConnector
    {
        #region 构造函数
        public InternetConnector() { }
        #endregion

        #region 字段
        private static readonly object _locker = new object();

        private static InternetConnector _jsonSerializor;

        private static int NETWORK_ALIVE_LAN = 0x00000001;
        private static int NETWORK_ALIVE_WAN = 0x00000002;
        private static int NETWORK_ALIVE_AOL = 0x00000004;

        [DllImport("sensapi.dll")]
        private extern static bool IsNetworkAlive(ref int flags);
        [DllImport("sensapi.dll")]
        private extern static bool IsDestinationReachable(string dest, IntPtr ptr);

        [DllImport("wininet.dll")]
        private extern static bool InternetGetConnectedState(out int connectionDescription, int reservedValue);

        #endregion

        #region 属性
        public static InternetConnector Instance
        {
            get
            {
                if (_jsonSerializor == null)
                {
                    lock (_locker)
                    {
                        if (_jsonSerializor == null)
                        {
                            _jsonSerializor = new InternetConnector();
                        }
                    }
                }
                return _jsonSerializor;
            }
        }
        #endregion

        #region 方法

        /// <summary>
        /// 判断网络连接状态
        /// </summary>
        /// <returns></returns>
        public static bool IsConnected()
        {
            int desc = 0;
            bool state = InternetGetConnectedState(out desc, 0);
            return state;
        }

        /// <summary>
        /// 判断局域网连接状态
        /// </summary>
        /// <returns></returns>
        public static bool IsLanAlive()
        {
            return IsNetworkAlive(ref NETWORK_ALIVE_LAN);
        }

        /// <summary>
        /// 判断广域网连接状态
        /// </summary>
        /// <returns></returns>
        public static bool IsWanAlive()
        {
            return IsNetworkAlive(ref NETWORK_ALIVE_WAN);
        }

        /// <summary>
        /// 拨号适配器连接状态
        /// </summary>
        /// <returns></returns>
        public static bool IsAOLAlive()
        {
            return IsNetworkAlive(ref NETWORK_ALIVE_AOL);
        }

        /// <summary>
        /// 目标地址存活状态
        /// </summary>
        /// <param name="Destination"></param>
        /// <returns></returns>
        public static bool IsDestinationAlive(string Destination)
        {
            return (IsDestinationReachable(Destination, IntPtr.Zero));
        }

        /// <summary>
        /// 在指定时间内尝试连接指定主机上的指定端口。 （默认端口：80,默认链接超时：2000毫秒）
        /// </summary>
        /// <param name="HostNameOrIp">主机名称或者IP地址</param>
        /// <param name="port">端口</param>
        /// <param name="timeOut">超时时间</param>
        /// <returns>返回布尔类型</returns>
        public static bool IsHostAlive(string HostNameOrIp, int? port, int? timeOut)
        {
            TcpClient tc = new TcpClient();
            tc.SendTimeout = timeOut ?? 2000;
            tc.ReceiveTimeout = timeOut ?? 2000;

            bool isAlive;
            try
            {
                tc.Connect(HostNameOrIp, port ?? 80);
                isAlive = true;
            }
            catch
            {
                isAlive = false;
            }
            finally
            {
                tc.Close();
            }
            return isAlive;
        }

        #endregion

    }
}
