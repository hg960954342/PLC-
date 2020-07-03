using PLC.Commons;
using PLC.Entity;
using PLC.Enumerate;
using System;

namespace PLC.Decoder
{
    /// <summary>
    /// 解码器
    /// </summary>
    class SiemensDecoder
    {
        /// <summary>
        /// 解码
        /// </summary>
        /// <param name="buffer">报文</param>
        /// <param name="length">报文长度</param>
        /// <returns></returns>
        public SiemensMessage Decode(byte[] buffer, int length)
        {
            SiemensMessage msg = null;
            if (buffer[0] == 3 && buffer[1] ==0)
            {
                short messageLength = Common.BytesToShort(buffer, 2);      //2-3 是报文长度
                if (messageLength == length)
                {
                    int type = buffer[11];
                    msg = new SiemensMessage();
                    switch (type)
                    {
                        case 0xC0:
                            msg.Type = CommandType.Head1;
                            break;
                        case 0x04:
                            msg.Type = CommandType.Head2;
                            break;
                        default:
                            ReadOrWrite(buffer, msg);
                            break;
                    }
                }
            }
            return msg;
        }

        /// <summary>
        /// 读写数据
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="msg"></param>
        private void ReadOrWrite(byte[] buffer, SiemensMessage msg)
        {
            int endLength = buffer[16];//获取结尾报文长度
            int type = buffer[19];//结果类型  05写入返回   04 读取返回
            int code = buffer[21];//返回结果   0xFF 代表无错

            byte[] value = null;
            var errorMsg = type == 4 ? "读取数据失败！" : "写入数据失败！";

            if (code == 0xFF)
            {
                //0x05 是写入返回   0x04 是读取返回
                switch (type)
                {
                    case 0x04:
                        msg.Type = CommandType.Read;
                        int bType = buffer[22];
                        int dataLength = Common.BytesToShort(buffer, 23);    //结果数据长度
                        if (bType == 4)                                      //03 代表返回是bit     04代表byte
                            dataLength = dataLength / 8;                     //结果数据长度 / 8 bit

                        byte[] buf = new byte[dataLength];
                        Array.Copy(buffer, 25, buf, 0, dataLength);
                        value = buf;
                        break;
                    case 0x05:
                        msg.Type = CommandType.Write;
                        break;
                    default:
                        errorMsg = "发现错误读写类型：" + type;
                        break;
                }
                errorMsg = null;
            }

            msg.Success = string.IsNullOrEmpty(errorMsg);
            msg.Message = errorMsg;
            msg.DataValue = value;
        }

    }
}