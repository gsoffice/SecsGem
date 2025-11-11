using SecsGemLib;
using SecsGemLib.Utils;
using System.Text;

namespace SecsGemTester
{
    public partial class Form1 : Form
    {
        private GemApi _gemApi;

        public Form1()
        {
            InitializeComponent();
            Logger.EventHandler += Logger_LogWritten;
        }

        private void Logger_LogWritten(string log)
        {
            // ?? UI 스레드 아닌 경우 Invoke 필요
            if (InvokeRequired)
            {
                Invoke(new Action(() => richTextBox1.AppendText(log + Environment.NewLine)));
            }
            else
            {
                richTextBox1.AppendText(log + Environment.NewLine);
            }
        }

        private async void button1_Click(object sender, EventArgs e)
        {
            _gemApi = new();
            _gemApi.Connected += () => Console.WriteLine("Connected!");
            _gemApi.Disconnected += () => Console.WriteLine("Disconnected!");
            _gemApi.MessageReceived += OnMessageReceived;
            bool ok = await _gemApi.ConnectAsync("127.0.0.1", 5000, true);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            _gemApi.Disconnect();
        }

        private async void button3_Click(object sender, EventArgs e)
        {
            await _gemApi.SendMsg(1, 13);
        }

        private void OnMessageReceived(string msg)
        {
            Console.WriteLine(msg);
        }

        private void button4_Click(object sender, EventArgs e)
        {
            _gemApi.AddSvid(0, "SVID1", "A");
            _gemApi.AddSvid(0, "SVID2", "U4");
            _gemApi.AddSvid(0, "SVID3", "L");            
        }
    }
}
