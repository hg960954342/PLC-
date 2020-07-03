using PLC.Entity;
using PLC.Enumerate;

namespace PLC.Commons
{
    /// <summary>
    /// 
    /// </summary>
    static class Common
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <param name="offset"></param>
        /// <returns></returns>
        public static short BytesToShort(byte[] src, int offset = 0)
        {
            var value = (short)((src[offset + 1] & 0xFF) | ((src[offset] & 0xFF) << 8));
            return value;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="src"></param>
        /// <param name="offset"></param>
        /// <returns></returns>
        public static int BytesToInt(byte[] src, int offset)
        {
            var value = (int)((src[offset+3] & 0xFF)
                     | ((src[offset + 2] & 0xFF) << 8)
                     | ((src[offset + 1] & 0xFF) << 16)
                     | ((src[offset ] & 0xFF) << 24));
            return value;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="src"></param>
        /// <param name="offset"></param>
        /// <returns></returns>
        public static long BytesToLong(byte[] src, int offset)
        {
            var value = (long)((src[offset + 7] & 0xFF)
                     | ((src[offset + 6] & 0xFF) << 8)
                     | ((src[offset + 5] & 0xFF) << 16)
                     | ((src[offset + 4] & 0xFF) << 24)
                     | ((src[offset + 3] & 0xFF) << 32)
                     | ((src[offset + 2] & 0xFF) << 40)
                     | ((src[offset + 1] & 0xFF) << 48)
                     | ((src[offset] & 0xFF) << 56));
            return value;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static byte[] ShortToBytes(short value)
        {
            var result = new byte[2];
            result[1] = (byte)value;
            result[0] = (byte)(value >> 8);
            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static byte[] IntToBytes(int value)
        {
            var result = new byte[4];
            result[3] = (byte)value;
            result[2] = (byte)(value >> 8);
            result[1] = (byte)(value >> 16);
            result[0] = (byte)(value >> 24);
            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static byte[] LongToBytes(long value)
        {
            var result = new byte[8];
            result[7] = (byte)value;
            result[6] = (byte)(value >> 8);
            result[5] = (byte)(value >> 16);
            result[4] = (byte)(value >> 24);
            result[3] = (byte)(value >> 32);
            result[2] = (byte)(value >> 40);
            result[1] = (byte)(value >> 48);
            result[0] = (byte)(value >> 56);
            return result;
        }

        /// <summary>
        /// 获取PLC地址信息
        /// </summary>
        /// <param name="addr"></param>
        /// <returns></returns>
        public static SiemensMessage GetAddress(string addr)
        {
            SiemensMessage msg = null;
            string address = addr.ToUpper();

            //地址不能为空，并且字符串长度不能少于2位
            if (!string.IsNullOrEmpty(address) && address.Length > 2)
            {
                address = address.Replace("DBX", "");  // DB1.DBX0.0  =>  DB1.0.0
                address = address.Replace("DB", "E");  // DB1.DBB0 =>  E1.EB0
                address = address.Replace("DW", "L");   // MDW0  => ML0    IDW0  =>  IL0

                var arr = address.Split('.');  //DB1.DBX0.0   M0.0   I0.0
                int arrLength = arr.Length;

                FnCode fnCode = GetFnCode(arr[0]);  //获取功能码
                DataType dataType = DataType.None;
                int addressStart = 0;
                int dbNumber = 0;
                bool isByte = true;


                if (arrLength == 1)
                {  //IB0  QW0   MD0
                    dataType = GetDataType(arr[0]);
                    addressStart = GetAddressStart(arr[0]);
                }
                else if (arrLength == 2)
                {  //I0.0  Q0.0  DB1.DBB0    DB1.DBB20
                    if (fnCode != FnCode.DB)
                    {
                        isByte = false;
                        dataType = DataType.Bit;
                        addressStart = GetBitStart(arr[0], arr[1]);
                    }
                    else
                    {
                        dataType = GetDataType(arr[1]);
                        addressStart = GetAddressStart(arr[1]);
                        dbNumber = GetDBNumber(arr[0]);
                    }
                }
                else if (arrLength == 3 && fnCode == FnCode.DB)
                {  //DB1.0.0
                    isByte = false;
                    dbNumber = GetDBNumber(arr[0]);
                    addressStart = GetBitStart(arr[1], arr[2]);  //有问题
                    dataType = DataType.Bit;
                }

                if (FnCode.None != fnCode && dataType != DataType.None && addressStart >= 0 && dbNumber >= 0)
                {
                    msg = new SiemensMessage
                    {
                        FnCode = fnCode,
                        DbBlockNo = dbNumber,
                        DataType = dataType,
                        DataAddress = addressStart,
                        IsByte = isByte
                    };
                }
            }
            return msg;
        }

        /// <summary>
        /// 获取功能码
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        private static FnCode GetFnCode(string address)
        {
            var fcCode = address.Substring(0, 1);
            FnCode temp = FnCode.None;

            switch (fcCode)
            {
                case "I":
                    temp = FnCode.I;
                    break;
                case "Q":
                    temp = FnCode.Q;
                    break;
                case "M":
                    temp = FnCode.M;
                    break;
                case "E":  //DB块
                    temp = FnCode.DB;
                    break;
                    //            case "S":  //S代表字符串
                    //                temp = DataType.String;
                    //                break;
            }
            return temp;
        }

        /// <summary>
        /// 获取地址类型
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        private static DataType GetDataType(string address)
        {

            var dataType = !char.IsNumber(address[1]) ? address.Substring(1, 1) : "b";
            DataType length = DataType.None;

            dataType = address.Substring(1, 1);
            switch (dataType)
            {
                case "b":
                case "X":
                    length = DataType.Bit;
                    break;
                case "B":
                    length = DataType.Byte;
                    break;
                case "W":
                    length = DataType.Word;
                    break;
                case "D":
                    length = DataType.Dint;
                    break;
                case "L":
                    length = DataType.Long;
                    break;
            }
            return length;
        }

        /// <summary>
        /// 获取DB块编号
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        private static int GetDBNumber(string address)
        {
            int temp = -1;
            try
            {
                temp = int.Parse(address.Substring(1));
            }
            catch
            {}
            return temp;
        }

        /// <summary>
        /// 获取bit起始地址
        /// </summary>
        /// <param name="addsByte"></param>
        /// <param name="addsBit"></param>
        /// <returns></returns>
        private static int GetBitStart(string addsByte, string addsBit)
        {
            int temp1 = -1;
            int temp2 = -1;
            try
            {
                int index = GetNumberIndex(addsByte);
                if (index >= 0)
                {
                    temp1 = int.Parse(addsByte.Substring(index));
                }
                temp2 = int.Parse(addsBit);
            }
            catch 
            {}
            return temp1 * 8 + temp2;
        }

        /// <summary>
        /// 获取起始地址
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        private static int GetAddressStart(string address)
        {
            int index = GetNumberIndex(address);
            int temp = -1;
            var str = "";

            if (index > 0)
            {
                try
                {
                    str = address.Substring(index);
                    temp = int.Parse(str);
                }
                catch 
                { }
            }
            return temp;
        }

        /// <summary>
        /// 获取当前字符串中第一个出现的数字所在的位置
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        private static int GetNumberIndex(string str)
        {
            for (int i = 0; i < str.Length; i++)
            {
                if (char.IsNumber(str[i]))
                {
                    return i;
                }
            }
            return -1;
        }
    }
}
