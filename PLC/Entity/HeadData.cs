using PLC.Enumerate;

namespace PLC.Entity
{
    /// <summary>
    /// 
    /// </summary>
    class HeadData
    {
        /// <summary>
        /// Head1
        /// </summary>
        public byte[] plcHead1 = new byte[] { 3, 0, 0, 0x16, 0x11, (byte)0xE0, 0, 0, 0, 1, 0, (byte)0xc0, 1, 10, (byte)0xc1, 2, 1, 2, (byte)0xc2, 2, 1, 0 };

        /// <summary>
        /// Head2
        /// </summary>
        public byte[] plcHead2 = new byte[] { 3, 0, 0, 0x19, 2, (byte)240, (byte)0x80, 50, 1, 0, 0, 4, 0, 0, 8, 0, 0, (byte)240, 0, 0, 1, 0, 1, 1, (byte)0xe0 };

        /// <summary>
        /// 
        /// </summary>
        private byte[] plcHead1_200 = new byte[] { 3, 0, 0, 0x16, 0x11, (byte)0xe0, 0, 0, 0, 1, 0, (byte)0xc1, 2, 0x4d, 0x57, (byte)0xc2, 2, 0x4d, 0x57, (byte)0xc0, 1, 9 };

        /// <summary>
        /// 
        /// </summary>
        private byte[] plcHead2_200 = new byte[] { 3, 0, 0, 0x19, 2, (byte)240, (byte)0x80, 50, 1, 0, 0, 0, 0, 0, 8, 0, 0, (byte)240, 0, 0, 1, 0, 1, 3, (byte)0xc0 };

        /// <summary>
        /// 
        /// </summary>
        private byte[] plcHead1_200smart = new byte[] { 3, 0, 0, 0x16, 0x11, (byte)0xe0, 0, 0, 0, 1, 0, (byte)0xc1, 2, 0x10, 0, (byte)0xc2, 2, 3, 0, (byte)0xc0, 1, 10 };

        /// <summary>
        /// 
        /// </summary>
        private byte[] plcHead2_200smart = new byte[] { 3, 0, 0, 0x19, 2, (byte)240, (byte)0x80, 50, 1, 0, 0, (byte)0xcc, (byte)0xc1, 0, 8, 0, 0, (byte)240, 0, 0, 1, 0, 1, 3, (byte)0xc0 };


        /// <summary>
        /// 设置
        /// </summary>
        /// <param name="siemens"></param>
        public void SetPlcType(SiemensPLCS siemens)
        {
            switch (siemens)
            {
                case SiemensPLCS.S200:
                    plcHead1 = plcHead1_200;
                    plcHead2 = plcHead2_200;
                    break;
                case SiemensPLCS.S300:
                    plcHead1[0x15] = 2;
                    break;
                case SiemensPLCS.S400:
                    plcHead1[0x11] = 0;
                    plcHead1[0x15] = 3;
                    break;
                case SiemensPLCS.S200Smart:
                    plcHead1 = plcHead1_200smart;
                    plcHead2 = plcHead2_200smart;
                    break;
                case SiemensPLCS.S1200:
                case SiemensPLCS.S1500:
                    plcHead1[0x15] = 0;
                    break;
            }
        }



    }
}
