using System.Collections.Generic;
using System.Net.NetworkInformation;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using static System.Windows.Forms.DataFormats;

namespace Network_Monitoring
{
    public partial class Form1 : Form
    {
        private static List<Form1> openForms = new List<Form1>();
        private static int formCounter = 1;
        private static int tabSpacing = 50;
        public Form1()
        {
            InitializeComponent();
            openForms.Add(this);
            CalculatePosition();
            this.TopMost = true;
            this.Text = "Ping Status Monitoring System Tab-" + formCounter;
            formCounter++;
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
            int sent = 0;
            int received = 0;
            int lost = 0;
            long totalRoundtripTime = 0;
            long minRoundtripTime = long.MaxValue;
            long maxRoundtripTime = 0;

            while (isPinging)
            {
                try
                {
                    Ping ping = new Ping();
                    PingReply reply = await ping.SendPingAsync(host);

                    // Update statistics
                    sent++;
                    if (reply.Status == IPStatus.Success)
                    {
                        received++;
                        totalRoundtripTime += reply.RoundtripTime;
                        minRoundtripTime = Math.Min(minRoundtripTime, reply.RoundtripTime);
                        maxRoundtripTime = Math.Max(maxRoundtripTime, reply.RoundtripTime);
                    }
                    else
                    {
                        lost++;
                    }

                    DisplayPingResult(reply);

                    // Ping interval, you can adjust as needed
                    await Task.Delay(1000);
                }
                catch (PingException ex)
                {
                    DisplayPingError(ex.Message);
                }
            }

            // Display statistics
            DisplayPingStatistics(sent, received, lost, totalRoundtripTime, minRoundtripTime, maxRoundtripTime);
        }



        private void DisplayPingStatistics(int sent, int received, int lost, long totalRoundtripTime, long minRoundtripTime, long maxRoundtripTime)
        {
            double averageRoundtripTime = received > 0 ? totalRoundtripTime / received : 0;

            Invoke((MethodInvoker)delegate
            {
                listBox1.Items.Add("\n");
                listBox1.Items.Add($"Packets: Sent = {sent}, Received = {received}, Lost = {lost} ({(lost / (double)sent) * 100}% loss)");
                listBox1.Items.Add($"Approximate round trip times in milli-seconds:");
                listBox1.Items.Add($"Minimum = {minRoundtripTime}ms, Maximum = {maxRoundtripTime}ms, Average = {averageRoundtripTime}ms");
                listBox1.TopIndex = listBox1.Items.Count - 1;
                listBox1.Items.Add("\n");
            });
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

        private void button3_Click(object sender, EventArgs e)
        {
            if (openForms.Count >= 3)
            {
                MessageBox.Show("Maximum of 2 windows are allowed.");
                return;
            }
            Form1 newForm = new Form1();
            openForms.Add(newForm);
            CalculatePosition();
            newForm.StartPosition = FormStartPosition.Manual;


            newForm.Location = new System.Drawing.Point(this.Location.X + this.Width, this.Location.Y);

            newForm.Show();

        }

        private void CalculatePosition()
        {
            int totalWidth = 0;
            foreach (var form in openForms)
            {
                totalWidth += form.Width;
            }

            // Center all forms if total width is less than the screen width
            if (totalWidth < Screen.PrimaryScreen.Bounds.Width)
            {
                int screenWidth = Screen.PrimaryScreen.Bounds.Width;
                int screenHeight = Screen.PrimaryScreen.Bounds.Height;

                int x = (screenWidth - totalWidth) / 2;
                int y = (screenHeight - this.Height) / 2;

                foreach (var form in openForms)
                {
                    form.Location = new System.Drawing.Point(x, y);
                    x += form.Width;
                }
            }
            else // Adjust positions of existing tabs and keep the new tab to the right side of the last tab
            {
                int x = openForms[openForms.Count - 1].Location.X + openForms[openForms.Count - 1].Width + tabSpacing;
                int y = openForms[0].Location.Y; // Keep y coordinate same for all tabs

                foreach (var form in openForms)
                {
                    form.Location = new System.Drawing.Point(x, y);
                    x += form.Width + tabSpacing;
                }
            }
        }

        private System.Drawing.Point CalculateCenteredPosition(int index)
        {
            int screenWidth = Screen.PrimaryScreen.Bounds.Width;
            int screenHeight = Screen.PrimaryScreen.Bounds.Height;

            int x = (screenWidth - this.Width) / 2 + (index - 1) * tabSpacing;
            int y = (screenHeight - this.Height) / 2;

            return new System.Drawing.Point(x, y);
        }

    }
}