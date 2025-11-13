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
            string filePath = "DATA\\VSP_SVID.txt";

            if (!File.Exists(filePath))
                throw new FileNotFoundException("SVID 파일을 찾을 수 없습니다.", filePath);

            var lines = File.ReadAllLines(filePath);

            foreach (var line in lines)
            {
                // 빈 줄 / 주석 스킵
                if (string.IsNullOrWhiteSpace(line))
                    continue;

                if (line.StartsWith("#") || line.StartsWith("//"))
                    continue;

                // 공백 기준 분리
                var parts = line.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length < 3)
                    continue; // 잘못된 형식

                long svid = long.Parse(parts[0]);
                string name = parts[1];
                string fmt = parts[2];

                // unit 정보가 파일에 없으므로 기본 빈 문자열
                _gemApi.AddSvid(svid, name, fmt, "");
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            string filePath = "DATA\\VSP_CEID.txt";

            if (!File.Exists(filePath))
                throw new FileNotFoundException("SVID 파일을 찾을 수 없습니다.", filePath);

            var lines = File.ReadAllLines(filePath);

            foreach (var line in lines)
            {
                // 빈 줄 / 주석 스킵
                if (string.IsNullOrWhiteSpace(line))
                    continue;

                if (line.StartsWith("#") || line.StartsWith("//"))
                    continue;

                // 공백 기준 분리
                var parts = line.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length < 2)
                    continue; // 잘못된 형식

                int ceid = int.Parse(parts[0]);
                string name = parts[1];

                _gemApi.AddCeid(ceid, name);
            }
        }

        private void button6_Click(object sender, EventArgs e)
        {
            int value;
            if (int.TryParse(textBox1.Text, out value))
            {
                _gemApi.SendEventReport(value);
            }                
        }
    }
}
