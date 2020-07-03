using PLC.Decoder;
using PLC.Encoder;
using PLC.Entity;
using PLC.Enumerate;
using System;
using System.Net;
using System.Net.Sockets;

namespace PLC.Commons
{
    /// <summary>
    /// 客户端
    /// </summary>
    class SocketClient
    {
        /// <summary>
        /// 返回数据超时时间（单位：秒）
        /// </summary>
        private const int timeOut = 2;

        /// <summary>
        /// 发送创建套接字
        /// </summary>
        private Socket tcpsend;

        /// <summary>
        /// 连接状态
        /// </summary>
        public bool Connected { get; private set; }

        /// <summary>
        /// 启动
        /// </summary>
        private bool start = false;

        /// <summary>
        /// 锁
        /// </summary>
        private object objLock = new object();

        /// <summary>
        /// 报文头
        /// </summary>
        private HeadData headData = new HeadData();

        /// <summary>
        /// plc连接端口
        /// </summary>
        private const int plcPort = 102;

        /// <summary>
        /// plc设备IP
        /// </summary>
        private string plcIP = "";

        /// <summary>
        /// plc类型
        /// </summary>
        private SiemensPLCS plcType;

        /// <summary>
        /// 接收数据缓存
        /// </summary>
        private byte[] receive_buff = new byte[1024 * 2];

        /// <summary>
        /// 编码
        /// </summary>
        private SiemensEncoder plcEncoder = new SiemensEncoder();

        /// <summary>
        /// 解码
        /// </summary>
        private SiemensDecoder plcDecoder = new SiemensDecoder();

        /// <summary>
        /// 
        /// </summary>
        private SiemensMessage response;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="msg"></param>
        internal delegate void OnErrorMessagehandle(string msg);

        /// <summary>
        /// 
        /// </summary>
        internal event OnErrorMessagehandle OnErrorMsg;

        /// <summary>
        /// 重新连接
        /// </summary>
        public void Reconnection()
        {
            if (!string.IsNullOrEmpty(plcIP))
                Connect(plcType, plcIP);
        }

        /// <summary>
        /// 连接
        /// </summary>
        /// <param name="type"></param>
        /// <param name="host"></param>
        public void Connect(SiemensPLCS type, string host)
        {
            if (start) return;
            plcType = type;
            plcIP = host;
            headData.SetPlcType(type);

            try
            {
                tcpsend = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)
                {
                    NoDelay = true
                };
                tcpsend.ReceiveTimeout = timeOut * 1000;
                tcpsend.SendTimeout = timeOut * 1000;
                var remotepoint = new IPEndPoint(IPAddress.Parse(host), plcPort);
                tcpsend.Connect(remotepoint);
                response = new SiemensMessage();
                start = true;
                Active();
            }
            catch (Exception ex)
            {
                Connected = false;
                start = false;
                ErrorMsg("Connect:" + ex.Message);
            }
        }

        /// <summary>
        /// 关闭连接
        /// </summary>
        public void Close()
        {
            start = false;
            Connected = false;
            if (tcpsend != null)
                tcpsend.Close();
        }

        /// <summary>
        /// 第一次连接
        /// </summary>
        private void Active()
        {
            var msg = new SiemensMessage
            {
                Type = CommandType.Head1,
                DataValue = headData.plcHead1
            };
            Send(msg);
        }

        /// <summary>
        /// 发送数据
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public SiemensMessage Send(SiemensMessage message)
        {
            lock (objLock)
            {
                response = new SiemensMessage
                {
                    Type = message.Type,
                    IsByte = message.IsByte,
                    DataValue = message.DataValue,
                    DataLength = message.DataLength,
                    DataAddress = message.DataAddress,
                    DataType = message.DataType,
                    DbBlockNo = message.DbBlockNo,
                    FnCode = message.FnCode
                };
                var data = plcEncoder.Encode(message);
                SendData(data);
                return response;
            }
        }

        /// <summary>
        /// 发送数据
        /// </summary>
        /// <param name="buffer"></param>
        private void SendData(byte[] buffer)
        {
            if (start)
            {
                try
                {
                    tcpsend.Send(buffer);
                    int bytesread = tcpsend.Receive(receive_buff);
                    var msg = plcDecoder.Decode(receive_buff, bytesread);
                    ProcessData(msg);
                }
                catch (Exception ex)
                {
                    Close();
                    ErrorMsg("Send Receive:" + ex.Message);
                }
            }
        }

        /// <summary>
        /// 处理数据
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="count"></param>
        /// <param name="msg"></param>
        private void ProcessData(SiemensMessage msg)
        {
            switch (msg.Type)
            {
                case CommandType.Head1:
                    response.Type = CommandType.Head2;
                    SendData(headData.plcHead2);
                    //Console.WriteLine("Head1");
                    break;
                case CommandType.Head2:
                    Connected = true;
                    //Console.WriteLine("Head2");
                    break;
                case CommandType.Read:
                case CommandType.Write:
                    response.Success = msg.Success;
                    response.Message = msg.Message;
                    response.DataValue = msg.DataValue;
                    //Console.WriteLine("Read | Write  " + Common.BytesToShort(msg.DataValue).ToString());
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// 错误信息
        /// </summary>
        /// <param name="v"></param>
        private void ErrorMsg(string v)
        {
            OnErrorMsg?.Invoke(v);
        }



    }
}
