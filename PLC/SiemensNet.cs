using PLC.Commons;
using PLC.Entity;
using PLC.Enumerate;
using System;

/// <summary>
/// 
/// </summary>
namespace PLC
{
    /// <summary>
    /// 
    /// </summary>
    public class SiemensNet
    {
        /// <summary>
        /// 
        /// </summary>
        private SocketClient plc = new SocketClient();

        /// <summary>
        /// 是否连接
        /// </summary>
        public bool Connected { get { return plc.Connected; } }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="msg"></param>
        public delegate void ErrorMessageHandle(string msg);

        /// <summary>
        /// 
        /// </summary>
        public event ErrorMessageHandle OnErrorMessage;

        /// <summary>
        /// 
        /// </summary>
        public SiemensNet()
        {
            plc.OnErrorMsg += Plc_OnErrorMsg;
        }

        /// <summary>
        /// 连接
        /// </summary>
        /// <param name="type"></param>
        /// <param name="host"></param>
        public void Connect(SiemensPLCS type, string host)
        {
            plc.Connect(type, host);
        }

        /// <summary>
        /// 重新连接
        /// </summary>
        public void Reconnection()
        {
            plc.Reconnection();
        }

        /// <summary>
        /// 关闭连接
        /// </summary>
        public void Close()
        {
            plc.Close();
        }

        #region 读取
        /// <summary>
        /// 读取PLC数据
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        public ResultData Read(string address)
        {
            return ReadBytes(address, 0);
        }

        /// <summary>
        /// 读取PLC数据
        /// </summary>
        /// <param name="address"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public ResultData ReadBytes(string address, int count)
        {
            ResultData rd = new ResultData
            {
                Connected = false,
                Message = "当前与PLC通信断开！",
                Success = false
            };

            if (this.Connected)
            {
                SiemensMessage msg = Common.GetAddress(address);
                if (msg == null)
                {
                    rd.Message = "请输入正确的PLC地址！";
                    return rd;
                }

                try
                {
                    msg.Type = CommandType.Read;
                    if (count > 0)
                    {
                        if (msg.DataType != DataType.Byte)
                        {
                            rd.Success = false;
                            rd.Message = "对不起，请输入Byte类型地址！";
                            return rd;
                        }
                        msg.DataType = DataType.Bytes;
                        msg.DataLength = count;
                    }
                    else
                        msg.DataLength = (int)msg.DataType;

                    SiemensMessage result = plc.Send(msg);
                    if (result.Success)
                    {
                        var value = GetValue(result.DataValue, result.DataType, result.DataLength);
                        rd.Value = value;
                    }
                    rd.Success = result.Success;
                    if (Connected)
                        rd.Message = result.Message;
                    rd.Connected = Connected;
                }
                catch (Exception e)
                {
                    rd.Message = e.Message;
                }
            }
            return rd;
        }

        /// <summary>
        /// 获取对象值
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="type"></param>
        /// <param name="dataLength"></param>
        /// <returns></returns>
        private object GetValue(byte[] buffer, DataType type, int dataLength)
        {
            object value = null;

            switch (type)
            {
                case DataType.Bit:
                    value = buffer[0] == 1 ? true : false;
                    break;
                case DataType.Byte:
                    value = buffer[0];
                    break;
                case DataType.Word:
                    value = Common.BytesToShort(buffer, 0);
                    break;
                case DataType.Dint:
                    value = Common.BytesToInt(buffer, 0);
                    break;
                case DataType.Long:
                    value = Common.BytesToLong(buffer, 0);
                    break;
                case DataType.Bytes:
                    value = buffer;
                    break;
            }
            return value;
        }

        #endregion

        #region 写入
        /// <summary>
        /// 写入数据
        /// </summary>
        /// <param name="address"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public ResultData Write(string address, bool value)
        {
            return Write(address, value ? (byte)1 : (byte)0);
        }

        /// <summary>
        /// 写入数据
        /// </summary>
        /// <param name="address"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public ResultData Write(string address, byte value)
        {
            byte[] buf = new byte[] { value };
            return Write(address, buf);
        }

        /// <summary>
        /// 写入数据
        /// </summary>
        /// <param name="address"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public ResultData Write(string address, short value)
        {
            byte[] buf = Common.ShortToBytes(value);
            return Write(address, buf);
        }

        /// <summary>
        /// 写入数据
        /// </summary>
        /// <param name="address"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public ResultData Write(string address, int value)
        {
            byte[] buf = Common.IntToBytes(value);
            return Write(address, buf);
        }

        /// <summary>
        /// 写入数据
        /// </summary>
        /// <param name="address"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public ResultData Write(string address, long value)
        {
            byte[] buf = Common.LongToBytes(value);
            return Write(address, buf);
        }

        /// <summary>
        /// 写入数据
        /// </summary>
        /// <param name="address"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public ResultData Write(string address, byte[] value)
        {
            ResultData rd = new ResultData
            {
                Connected = false,
                Success = false,
                Message = "当前与PLC通信断开！"
            };

            if (this.Connected)
            {
                rd.Connected = true;
                SiemensMessage msg = Common.GetAddress(address);

                if (msg == null)
                {
                    rd.Message = "当前PLC地址不正确！";
                    return rd;
                }

                msg.Type = CommandType.Write;
                msg.DataValue = value;
                msg.DataLength = value.Length;

                try
                {
                    SiemensMessage result = plc.Send(msg);
                    if(Connected)
                    rd.Message = result.Message;
                    rd.Success = result.Success;
                    rd.Value = result.DataValue;
                    rd.Connected = Connected;
                }
                catch (Exception e)
                {
                    rd.Message = e.Message;
                }
            }
            return rd;
        }
        #endregion

        #region 信息
        /// <summary>
        /// 错误信息
        /// </summary>
        /// <param name="msg"></param>
        private void Plc_OnErrorMsg(string msg)
        {
            ErrorMsg(msg);
        }

        /// <summary>
        /// 错误信息
        /// </summary>
        /// <param name="msg"></param>
        private void ErrorMsg(string msg)
        {
            //Console.WriteLine(msg);
            OnErrorMessage?.Invoke(msg);
        }
        #endregion
    }
}
