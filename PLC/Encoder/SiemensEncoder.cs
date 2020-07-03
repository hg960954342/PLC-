using PLC.Entity;
using PLC.Enumerate;
using System.Collections.Generic;

namespace PLC.Encoder
{
    /// <summary>
    /// 编码器
    /// </summary>
    class SiemensEncoder
    {
        /// <summary>
        /// 发送缓存
        /// </summary>
        private List<byte> sendData = new List<byte>();

        /// <summary>
        /// 编码
        /// </summary>
        /// <param name="msg"></param>
        /// <returns></returns>
        public byte[] Encode(SiemensMessage msg)
        {
            int dataAddress = 0;
            sendData.Clear();
            switch (msg.Type)
            {
                case CommandType.Head1:
                case CommandType.Head2:
                    sendData.AddRange(msg.DataValue);
                    break;
                case CommandType.Read:                    
                    sendData.Add(3);
                    sendData.Add(0);
                    sendData.Add(0);
                    sendData.Add(0x1F);  //长度
                    sendData.Add(0x02);
                    sendData.Add(0xF0);
                    sendData.Add(0x80);
                    sendData.Add(0x32);
                    sendData.Add(0x01);
                    sendData.Add(0);
                    sendData.Add(0);
                    sendData.Add(0);
                    sendData.Add(0x01);
                    sendData.Add(0);
                    sendData.Add(0x0E);
                    sendData.Add(0);
                    sendData.Add(0);
                    sendData.Add(0x04);
                    sendData.Add(0x01);
                    sendData.Add(0x12);
                    sendData.Add(0x0A);
                    sendData.Add(0x10);
                    //---------------------------
                    sendData.Add(msg.IsByte ? (byte)2 : (byte)1);       //1 为bool 2为byte
                    //读取类型长度  byte =1 , shot = 2  int = 4;  string 先读前2位，获取字符串长度，然后在读
                    sendData.Add((byte)(msg.DataLength >> 8 & 0xFF));
                    sendData.Add((byte)(msg.DataLength & 0xFF));
                    //DB块编号
                    sendData.Add((byte)(msg.DbBlockNo >> 8 & 0xFF));
                    sendData.Add((byte)(msg.DbBlockNo & 0xFF));
                    //I/Q/M/DB  功能代码
                    sendData.Add((byte)msg.FnCode);

                    if (msg.DataType == DataType.Bit)
                        dataAddress = msg.DataAddress;
                    else
                        dataAddress = msg.DataAddress * 8; //长度   byte * 8
                    sendData.Add((byte)(dataAddress >> 16 & 0xFF));
                    sendData.Add((byte)(dataAddress >> 8 & 0xFF));
                    sendData.Add((byte)(dataAddress & 0xFF));
                    break;
                case CommandType.Write:
                    int dataLength = msg.DataLength;
                    sendData.Add(3);
                    sendData.Add(0);
                    //数据长度
                    sendData.Add((byte)(0x23 + dataLength >> 8 & 0xFF));
                    sendData.Add((byte)(0x23 + dataLength & 0xFF));
                    sendData.Add(0x02);
                    sendData.Add(0xF0);
                    sendData.Add(0x80);
                    sendData.Add(0x32);
                    sendData.Add(0x01);
                    sendData.Add(0);
                    sendData.Add(0);
                    sendData.Add(0);
                    sendData.Add(0x01);
                    sendData.Add(0);
                    sendData.Add(0x0E);
                    sendData.Add(0);
                    sendData.Add((byte)(4 + dataLength));
                    sendData.Add(0x05);
                    sendData.Add(0x01);
                    sendData.Add(0x12);
                    sendData.Add(0x0A);
                    sendData.Add(0x10);
                    //---------------------------
                    sendData.Add(msg.IsByte ? (byte)2 : (byte)1);    //1 为bool 2为byte
                    //数据长度
                    sendData.Add((byte)(dataLength >> 8 & 0xFF));
                    sendData.Add((byte)(dataLength & 0xFF));
                    //DB块编号
                    sendData.Add((byte)(msg.DbBlockNo >> 8 & 0xFF));
                    sendData.Add((byte)(msg.DbBlockNo & 0xFF));
                    //I/Q/M/DB  功能代码
                    sendData.Add((byte)msg.FnCode);

                    if (msg.DataType == DataType.Bit)
                        dataAddress = msg.DataAddress;
                    else
                        dataAddress = msg.DataAddress * 8; //长度   byte * 8

                    sendData.Add((byte)(dataAddress >> 16 & 0xFF));
                    sendData.Add((byte)(dataAddress >> 8 & 0xFF));
                    sendData.Add((byte)(dataAddress & 0xFF));

                    sendData.Add(0);
                    sendData.Add(msg.IsByte ? (byte)4 : (byte)3);        //bit 为3    byte 为 4
                    //类型长度
                    var dtl = msg.DataType == DataType.Bit ? 1 : dataLength * 8;
                    sendData.Add((byte)(dtl >> 8 & 0xFF));
                    sendData.Add((byte)(dtl & 0xFF));
                    sendData.AddRange(msg.DataValue);  // 写入数据
                    break;
                default:
                    break;
            }
            return sendData.ToArray();
        }
    }
}
