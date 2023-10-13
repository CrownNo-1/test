using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TCP
{
    /// <summary>
    /// 服务端与客户端创建，接受与发送
    /// </summary>
    public class TCP_Create
    {
        #region 公共对象
        /// <summary>
        /// 客户端对象
        /// </summary>
        private static Socket socket_client;

        /// <summary>
        /// 服务端对象
        /// </summary>
        private static Socket socket_server;
        /// <summary>
        /// 客户端委托
        /// </summary>
        /// <param name="data"></param>
        public delegate void Data(string data);
        /// <summary>
        /// 服务端委托
        /// </summary>
        /// <param name="data">返回的内容</param>
        /// <param name="ip">发送内容的用户信息</param>
        public delegate void Data_Server(string data, string ip);
        private static Data server_data;
        private static Data_Server server_data_server;

        /// <summary>
        /// 服务端(存储连接成功的客户端)
        /// </summary>
        private static List<Socket> socket_server_list=new List<Socket>();
        private static List<string> sockets_client = new List<string>();

        /// <summary>
        /// 判断是否连接成功，或者判断是否以断开
        /// </summary>
        private static bool judgment=false;
        #endregion

        #region 服务端
        /// <summary>
        /// 创建服务端
        /// </summary>
        /// <param name="IP">创建服务端的IP地址</param>
        /// <param name="port">端口号</param>
        /// /// <param name="method">端口号</param>
        public static void TCP_Server_Create(string IP,int port, Data_Server method =null) {
            try
            {
                if (socket_server==null)
                {
                    socket_server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    EndPoint en = new IPEndPoint(IPAddress.Parse(IP), Convert.ToInt32(port));
                    socket_server.Bind(en);
                    socket_server.Listen(10);
                    judgment = true;
                    Task.Run(() => {
                        while (judgment)
                        {
                            socket_server_list.Add(socket_server.Accept());
                            IPEndPoint ipend = socket_server_list[socket_server_list.Count - 1].RemoteEndPoint as IPEndPoint;
                            sockets_client.Add(ipend.Port.ToString());
                            server_data_server += method;
                            TCP_Server_Listening(ipend.Port.ToString());

                        }
                    });
                }
                else
                {
                    throw new Exception("服务端以创建");
                }
               
            }
            catch (Exception)
            {

                throw;
            }
        }
        /// <summary>
        /// 监听服务端发来的消息
        /// </summary>
        /// <param name="ip">内部会处理一个用户的下标</param>
        private static void TCP_Server_Listening(string ip) {
            Task.Run(() => {
                try
                {
                    byte[] data = new byte[1024];
                    int count;
                    IPEndPoint ipend = socket_server_list[sockets_client.IndexOf(ip)].RemoteEndPoint as IPEndPoint;
                    while (socket_server!=null)
                    {
                        count = socket_server_list[sockets_client.IndexOf(ip)].Receive(data);
                        if (count==0)
                        {
                            socket_server_list.RemoveAt(sockets_client.IndexOf(ip));
                            sockets_client.RemoveAt(sockets_client.IndexOf(ip));
                            return;
                        }
                        server_data_server.Invoke(Encoding.Default.GetString(data,0, count),  ipend.Port.ToString());
                    }
                }
                catch (Exception)
                {
                    throw;
                }
            });
            
        }
        /// <summary>
        /// 服务端的信息发送
        /// </summary>
        /// <param name="Information">发送的信息</param>
        /// <param name="index">等于-1的时候就是全部发送，单个发送的时候输入对方的端口号就能进行发送</param>
        public static void TCP_Server_Send(string Information,string index) {
            try
            {
                if (socket_server != null)
                {
                    index=index.Trim();
                    if (index.Equals("-1"))
                    {
                        for (int i = 0; i < socket_server_list.Count - 1; i++)
                        {
                            socket_server_list[i].Send(Encoding.Default.GetBytes(Information));
                        }
                    }
                    else
                    {
                        socket_server_list[sockets_client.IndexOf(index)].Send(Encoding.Default.GetBytes(Information));
                    }
                }
                else { throw new Exception("服务端未创建"); }
                
            }
            catch (Exception)
            {

                throw;
            }
            
        }
        #endregion

        #region 客户端

        /// <summary>
        /// 创建服务端并会自动监听，服务端发来的消息
        /// </summary>
        /// <param name="IP">连接服务器的ip</param>
        /// <param name="port">连接服务器的端口号</param>
        /// <param name="method">一个方法用来做接受</param>
        public static void TCP_Client_Create(string IP, int port, Data method = null) {
            try
            {
                if (socket_client == null)
                {
                    socket_client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    socket_client.Connect(IPAddress.Parse(IP), Convert.ToInt32(port));
                    server_data += method;
                    Task.Run(() => { TCP_Client_Listening(); });
                }
                else { throw new Exception("客户端以创建"); }
                
                
            }
            catch (Exception)
            {

                throw;
            }
            
        }
        /// <summary>
        /// 监听服务器发来的消息
        /// 注意:假设服务端断开那么会返回已断开
        /// </summary>
        private static void TCP_Client_Listening() {
            
            try
            {
                int count;
                byte[] data = new byte[1024];
                while (true)
                {
                    Thread.Sleep(90);
                    count = socket_client.Receive(data);
                    if (count == 0)
                    {
                        server_data.Invoke("已断开");
                        return;
                    }
                    server_data.Invoke(Encoding.Default.GetString(data,0,count));
                }
            }
            catch (Exception)
            {

                throw;
            }
        }
        /// <summary>
        /// 客户端的发送信息
        /// </summary>
        /// <param name="Information">需要发送的内容</param>
        public static void TCP_Client_Send(string Information) {
            try
            {
                if (socket_client != null)
                {
                    socket_client.Send(Encoding.Default.GetBytes(Information));
                }
                else { throw new Exception("客户端未创建"); }
                
            }
            catch (Exception)
            {

                throw;
            }
            
        }
        #endregion
    }
}
