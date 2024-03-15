using System.Net.NetworkInformation;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace Network_Monitoring
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            listBox1.Hide();
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            textBox1.KeyDown += button1_KeyDown;
        }

        private bool isPinging = false;
        private void button1_Click(object sender, EventArgs e)
        {
            string ip = textBox1.Text.Trim();


            if (!string.IsNullOrEmpty(ip))
            {
                listBox1.Show();

                if (!isPinging)
                {
                    isPinging = true;
                    button1.Text = "Stop";
                    textBox1.Enabled = false;
                    button1.BackColor = Color.IndianRed;
                    PingHost(ip);
                }
                else
                {
                    isPinging = false;
                    button1.Text = "Start";
                    textBox1.Enabled = true;
                    button1.BackColor = Color.LimeGreen;
                }
            }
            else
            {
                MessageBox.Show("Please enter a valid IP address.");
            }
        }

        private async void PingHost(string host)
        {
            while (isPinging)
            {
                try
                {
                    Ping ping = new Ping();
                    PingReply reply = await ping.SendPingAsync(host);
                    DisplayPingResult(reply);
                }
                catch (PingException ex)
                {
                    DisplayPingError(ex.Message);
                }

                // Ping interval, you can adjust as needed
                await Task.Delay(1000);
            }
        }

        private void DisplayPingResult(PingReply reply)
        {
            if (reply.Status == IPStatus.Success)
            {
                Invoke((MethodInvoker)delegate
                {
                    listBox1.Items.Add($"Reply from {reply.Address}: Bytes={reply.Buffer.Length} time={reply.RoundtripTime}ms  {DateTime.Now.ToLongTimeString()}");
                });

                // Scroll down to the last added item
                listBox1.TopIndex = listBox1.Items.Count - 1;
            }
            else
            {
                Invoke((MethodInvoker)delegate
                {
                    listBox1.Items.Add($"Ping failed: {reply.Status}  {DateTime.Now.ToLongTimeString()}");
                    listBox1.TopIndex = listBox1.Items.Count - 1;
                });
            }
        }

        private void DisplayPingError(string error)
        {

            Invoke((MethodInvoker)delegate
            {
                listBox1.Items.Add($"Ping failed: {DateTime.Now.ToLongTimeString()}");
                listBox1.TopIndex = listBox1.Items.Count - 1;
            });
        }

        private void button2_Click(object sender, EventArgs e)
        {
            listBox1.Items.Clear();
            //textBox1.Enabled = true;
        }

        private void button1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                button1.PerformClick();
            }
        }
    }
}