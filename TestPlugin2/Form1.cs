using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TestPlugin2
{
    public partial class Form1 : Form
    {
        public TestPluginClass plug;
        public PluginSystem.DataInfo info;
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                byte[] input = HexStringToByteArray(textBox1.Text.Replace(" ", ""));
                if (input.Length != 0x14)
                {
                    MessageBox.Show("Not a valid SHA1 length!");
                    return;
                }
                byte[] buff = plug.Host.getDataBySha1(input);
                if (buff != null)
                {
                    SaveFileDialog d = new SaveFileDialog();
                    d.Filter = "*.bin|*.bin";
                    if (d.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    {
                        File.WriteAllBytes(d.FileName, buff);
                        MessageBox.Show("Done.");
                    }
                }
            }
            catch { }
        }

        public static byte[] HexStringToByteArray(string hex)
        {
            return Enumerable.Range(0, hex.Length)
                             .Where(x => x % 2 == 0)
                             .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
                             .ToArray();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            try
            {
                byte[] input = HexStringToByteArray(textBox1.Text.Replace(" ", ""));
                if (input.Length != 0x14)
                {
                    MessageBox.Show("Not a valid SHA1 length!");
                    return;
                }
                OpenFileDialog d = new OpenFileDialog();
                d.Filter = "*.bin|*.bin";
                if (d.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    byte[] buff = File.ReadAllBytes(d.FileName);
                    int count = plug.Host.setDataBySha1(buff, input, null);
                    MessageBox.Show("Done with " + count + " replacement(s).");
                }
            }
            catch { }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            try
            {
                byte[] input = HexStringToByteArray(textBox1.Text.Replace(" ", ""));
                OpenFileDialog d = new OpenFileDialog();
                d.Filter = "*.bin|*.bin";
                if (d.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    byte[] buff = File.ReadAllBytes(d.FileName);
                    MemoryStream m = new MemoryStream();
                    m.Write(info.sha1, 0, 0x14);
                    List<string> list = plug.Host.getTOCFileLabels();
                    string tocname = null;
                    foreach(string toc in list)
                        if (info.toc.EndsWith(toc))
                        {
                            tocname = toc;
                            break;
                        }
                    Helpers.WriteNullString(m, tocname);
                    m.Write(buff, 0, buff.Length);
                    plug.Host.AddModJob(plug.Name, "Replacement by SHA1", m.ToArray());
                    MessageBox.Show("Done.");
                }
            }
            catch { }
        }

    }

    public static class Helpers
    {
        public static void WriteInt(Stream s, int i)
        {
            s.Write(BitConverter.GetBytes(i), 0, 4);
        }

        public static void WriteUInt(Stream s, uint i)
        {
            s.Write(BitConverter.GetBytes(i), 0, 4);
        }

        public static void WriteShort(Stream s, short i)
        {
            s.Write(BitConverter.GetBytes(i), 0, 2);
        }

        public static void WriteUShort(Stream s, ushort i)
        {
            s.Write(BitConverter.GetBytes(i), 0, 2);
        }

        public static void WriteLEInt(Stream s, int i)
        {
            List<byte> t = new List<byte>(BitConverter.GetBytes(i));
            t.Reverse();
            s.Write(t.ToArray(), 0, 4);
        }

        public static void WriteLEUInt(Stream s, uint i)
        {
            List<byte> t = new List<byte>(BitConverter.GetBytes(i));
            t.Reverse();
            s.Write(t.ToArray(), 0, 4);
        }

        public static void WriteLEUShort(Stream s, ushort u)
        {
            byte[] buff = BitConverter.GetBytes(u);
            buff = buff.Reverse().ToArray();
            s.Write(buff, 0, 2);
        }

        public static int ReadInt(Stream s)
        {
            byte[] buff = new byte[4];
            s.Read(buff, 0, 4);
            return BitConverter.ToInt32(buff, 0);
        }

        public static uint ReadUInt(Stream s)
        {
            byte[] buff = new byte[4];
            s.Read(buff, 0, 4);
            return BitConverter.ToUInt32(buff, 0);
        }

        public static short ReadShort(Stream s)
        {
            byte[] buff = new byte[2];
            s.Read(buff, 0, 2);
            return BitConverter.ToInt16(buff, 0);
        }

        public static ushort ReadUShort(Stream s)
        {
            byte[] buff = new byte[2];
            s.Read(buff, 0, 2);
            return BitConverter.ToUInt16(buff, 0);
        }

        public static long ReadLong(Stream s)
        {
            byte[] buff = new byte[8];
            s.Read(buff, 0, 8);
            return BitConverter.ToInt64(buff, 0);
        }

        public static ulong ReadULong(Stream s)
        {
            byte[] buff = new byte[8];
            s.Read(buff, 0, 8);
            return BitConverter.ToUInt64(buff, 0);
        }

        public static float ReadFloat(Stream s)
        {
            byte[] buff = new byte[4];
            s.Read(buff, 0, 4);
            return BitConverter.ToSingle(buff, 0);
        }

        public static int ReadLEInt(Stream s)
        {
            byte[] buff = new byte[4];
            s.Read(buff, 0, 4);
            buff = buff.Reverse().ToArray();
            return BitConverter.ToInt32(buff, 0);
        }

        public static uint ReadLEUInt(Stream s)
        {
            byte[] buff = new byte[4];
            s.Read(buff, 0, 4);
            buff = buff.Reverse().ToArray();
            return BitConverter.ToUInt32(buff, 0);
        }

        public static short ReadLEShort(Stream s)
        {
            byte[] buff = new byte[2];
            s.Read(buff, 0, 2);
            buff = buff.Reverse().ToArray();
            return BitConverter.ToInt16(buff, 0);
        }

        public static ushort ReadLEUShort(Stream s)
        {
            byte[] buff = new byte[2];
            s.Read(buff, 0, 2);
            buff = buff.Reverse().ToArray();
            return BitConverter.ToUInt16(buff, 0);
        }

        public static string ReadNullString(Stream s)
        {
            string res = "";
            byte b;
            while ((b = (byte)s.ReadByte()) > 0 && s.Position < s.Length) res += (char)b;
            return res;
        }

        public static void WriteNullString(Stream s, string t)
        {
            foreach (char c in t)
                s.WriteByte((byte)c);
            s.WriteByte(0);
        }
    }
}
