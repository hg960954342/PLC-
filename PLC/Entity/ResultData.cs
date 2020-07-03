namespace PLC.Entity
{
    /// <summary>
    /// 结果
    /// </summary>
    public class ResultData
    {
        /// <summary>
        /// 值
        /// </summary>
        public object Value { get; set; }

        /// <summary>
        /// 信息
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// 是否成功
        /// </summary>
        public bool  Success { get; set; }

        /// <summary>
        /// 是否连接
        /// </summary>
        public bool Connected { get; set; }
    }
}
