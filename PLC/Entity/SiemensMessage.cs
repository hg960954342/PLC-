using PLC.Enumerate;

namespace PLC.Entity
{
    /// <summary>
    /// 
    /// </summary>
    class SiemensMessage
    {
        /// <summary>
        /// 命令类型  1、2、3 读、4 写
        /// </summary>
        public CommandType Type { get; set; }

        /// <summary>
        /// 功能码   DB 84
        /// </summary>
        public FnCode FnCode { get; set; }

        /// <summary>
        /// 数据类型长度
        /// </summary>
        public DataType DataType { get; set; }

        /// <summary>
        /// 数据类型长度
        /// </summary>
        public int DataLength { get; set; }

        /// <summary>
        /// 数据类型   1为bool   2为读取Byte
        /// </summary>
        public bool IsByte { get; set; }

        /// <summary>
        /// 数据类型起始地址
        /// </summary>
        public int DataAddress { get; set; }

        /// <summary>
        /// db块地址编号
        /// </summary>
        public int DbBlockNo { get; set; }

        /// <summary>
        /// 数据值
        /// </summary>
        public byte[] DataValue { get; set; }

        /// <summary>
        /// 信息
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// 成功
        /// </summary>
        public bool Success { get; set; }
    }

}
