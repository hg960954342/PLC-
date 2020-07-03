using PLC;
using PLC.Enumerate;
using System;
using System.Windows.Forms;

namespace Test
{
    public partial class Form1 : Form
    {
        SiemensNet plc = new SiemensNet();


        private static short[] Getvaleu() {
            short aa = 12;
            byte[] bb =BitConverter.GetBytes(aa);
            var value =new short[1];
            for (int i = 0; i < value.Length; i++)
            {
                value[i] = Datas(bb,i*2);
            }
            return value;
        }
        private static short Datas(byte[] buffers , int indexs)
        {
            return (short)(buffers[indexs] << 8 | buffers[indexs + 1]);
        }

        public Form1()
        {
            Getvaleu();
            plc.OnErrorMessage += Plc_OnErrorMessage;
            InitializeComponent();
        }

        private void Plc_OnErrorMessage(string msg)
        {
            Console.WriteLine(msg);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                plc.Connect(SiemensPLCS.S1200, txtIP.Text);
                button1.Enabled = false;
                button3.Enabled = true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            var value = bool.Parse(w_value.Text);
            var r = plc.Write(w_addr.Text, value);
            Console.WriteLine("Value:" + r.Value + "  Success:" + r.Success + "  Connecd:" + r.Connected + "  msg:" + r.Message);
        }

        private void button4_Click(object sender, EventArgs e)
        {
            var value = byte.Parse(w_value.Text);
            var r = plc.Write(w_addr.Text, value);
            Console.WriteLine("Value:" + r.Value + "  Success:" + r.Success + "  Connecd:" + r.Connected + "  msg:" + r.Message);
        }

        private void button5_Click(object sender, EventArgs e)
        {
            var value = short.Parse(w_value.Text);
            var r = plc.Write(w_addr.Text, value);
            Console.WriteLine("Value:" + r.Value + "  Success:" + r.Success + "  Connecd:" + r.Connected + "  msg:" + r.Message);
        }

        private void button6_Click(object sender, EventArgs e)
        {
            var value = int.Parse(w_value.Text);
            var r = plc.Write(w_addr.Text, value);
            Console.WriteLine("Value:" + r.Value + "  Success:" + r.Success + "  Connecd:" + r.Connected + "  msg:" + r.Message);
        }

        private void button7_Click(object sender, EventArgs e)
        {
            var value = long.Parse(w_value.Text);
            var r = plc.Write(w_addr.Text, value);
            Console.WriteLine("Value:" + r.Value + "  Success:" + r.Success + "  Connecd:" + r.Connected + "  msg:" + r.Message);
        }

        private void button10_Click(object sender, EventArgs e)
        {
            var r = plc.Read(r_addr.Text);
            if (r.Success)
                r_value.Text = r.Value.ToString();
            Console.WriteLine("Value:" + r.Value + "  Success:" + r.Success + "  Connecd:" + r.Connected + "  msg:" + r.Message);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            plc.Close();
            button3.Enabled = false;
            button1.Enabled = true;
        }
    }
}
