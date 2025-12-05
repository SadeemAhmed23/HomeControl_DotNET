bbssbsbbdndndndndndndnndusing System.Text;
using System.Windows.Forms;
using System.Xml;
using SuperSimpleTcp;

namespace DesktopServer
{
    public partial class Form1 : Form
    {
        string Power_ACA;
        string Power_ACB;
        bool ForceOn_ACA;
        bool ForceOn_ACB;
        int Timer_ACA;
        int Timer_ACB;

        string[] AcData_ACA;
        string[] AcData_ACB;

        string[] WaterData_Pump;
        string[] WaterData_Gyser;
        string[] WaterData_EGyser;

        private SimpleTcpServer server;
        string configFilePath = Path.Combine(Application.StartupPath, "config.txt");
        string AcControllerIP = "192.168.1.150";
        string WaterControllerIP = "192.168.1.151";
        public Form1()
        {
            InitializeComponent();
            string serverConfig = $"{txtHost.Text}:{txtPort.Text}";
            server = new SimpleTcpServer(serverConfig);
            server.Events.ClientConnected += ClientConnected;
            server.Events.ClientDisconnected += ClientDisconnected;
            server.Events.DataReceived += DataReceived;
            ReadConfig();
            server.Start();

        }

        private void ReadConfig()
        {
            string configFilePath = Path.Combine(Application.StartupPath, "config.txt");
            string content = File.ReadAllText(configFilePath);

            if (string.IsNullOrWhiteSpace(content))
            {
                MessageBox.Show("No Configuration Data in 'config.txt'");
                Application.Exit();
            }
            else if (!File.Exists(configFilePath))
            {
                File.Create(configFilePath);
            }
            else
            {
                string[] lines = File.ReadAllLines(configFilePath);
                AcControllerIP = lines[0];
                WaterControllerIP = lines[1];
               // MessageBox.Show($"{AcControllerIP} and  {WaterControllerIP}");
            }
        }
        private void ClientDisconnected(object? sender, ConnectionEventArgs e)
        {
            string clientIp = e.IpPort.Split(':')[0];
            AppendLog($"Client {clientIp} disconnected.");

            if (clientIp == AcControllerIP)
            {
                txtBox_AC.Invoke(new MethodInvoker(delegate
                {
                    txtBox_AC.BackColor = Color.Red;
                }));
            }
            if (clientIp == WaterControllerIP)
            {
                txtBox_Water.Invoke(new MethodInvoker(delegate
                {
                    txtBox_Water.BackColor = Color.Red;
                }));
            }
        }
        private void ClientConnected(object? sender, ConnectionEventArgs e)
        {
            string clientIp = e.IpPort.Split(':')[0];
            AppendLog($"Client {clientIp} connected.");

            if (clientIp == AcControllerIP)
            {
                txtBox_AC.Invoke(new MethodInvoker(delegate
                {
                    txtBox_AC.BackColor = Color.DarkBlue;
                }));
            }
            if (clientIp == WaterControllerIP)
            {
                txtBox_Water.Invoke(new MethodInvoker(delegate
                {
                    txtBox_Water.BackColor = Color.DarkBlue;
                }));
            }
        }
        private void DataReceived(object? sender, DataReceivedEventArgs e)
        {
            string receivedMessage = Encoding.UTF8.GetString(e.Data);
            AppendLog($"Received from {e.IpPort}: {receivedMessage}");

            string clientIp = e.IpPort.Split(':')[0];

            if (clientIp == AcControllerIP)
            {
                DisplayAcData(receivedMessage);
            }
            else if (clientIp == WaterControllerIP)
            {
                DisplayWaterData(receivedMessage);
            }
        }
        //==================================================================================================================
        //                                      Support Functions
        //==================================================================================================================
        private void SafeInvoke(Action action)
        {
            if (InvokeRequired)
            {
                Invoke(new MethodInvoker(action));
            }
            else
            {
                action();
            }
        }
        private void AppendLog(string message)
        {
            if (txtLogs.Items.Count > 50)
                SafeInvoke(() => txtLogs.Items.Clear());

            if (txtLogs.InvokeRequired)
            {
                txtLogs.Invoke(new MethodInvoker(delegate
                {
                    txtLogs.Items.Add(message);
                }));
            }
            else
            {
                txtLogs.Items.Add(message);
            }
        }
        private void AppendLog_AC(string message)
        {
            if (txtLogs_AC.Items.Count > 50)
                txtLogs_AC.Items.Clear();

            if (txtLogs_AC.InvokeRequired)
            {
                txtLogs_AC.Invoke(new MethodInvoker(delegate
                {
                    txtLogs_AC.Items.Add(message);
                }));
            }
            else
            {
                txtLogs_AC.Items.Add(message);
            }
        }
        private void AppendLog_Water(string message)
        {
            if (txtLogs_Water.Items.Count > 50)
                txtLogs_Water.Items.Clear();

            if (txtLogs_Water.InvokeRequired)
            {
                txtLogs_Water.Invoke(new MethodInvoker(delegate
                {
                    txtLogs_Water.Items.Add(message);
                }));
            }
            else
            {
                txtLogs_Water.Items.Add(message);
            }
        }
        //==================================================================================================================
        //                                      Update & Display AC Data
        //==================================================================================================================
        public void DisplayAcData(string msg)
        {
            string[] AcData = msg.Split('|');
            if (AcData.Length != 2)
                AppendLog("Invalid Format Recieved");

            AcData_ACA = AcData[0].Split(";");
            AcData_ACB = AcData[1].Split(";");

            SafeInvoke(() =>
            {
                if (AcData_ACA.Length == 4)
                {
                    txtPower_ACA.Text = AcData_ACA[0];
                    txtUptime_ACA.Text = AcData_ACA[1];
                    txtBigUptime_ACA.Text = AcData_ACA[1];
                    txtTimer_ACA.Text = AcData_ACA[2];

                    if (AcData_ACA[3] == "1")
                        ckForceOn_ACA.Checked = true;
                    else
                        ckForceOn_ACA.Checked = false;
                }
                if (AcData_ACB.Length == 4)
                {
                    txtPower_ACB.Text = AcData_ACB[0];
                    txtUptime_ACB.Text = AcData_ACB[1];
                    txtBigUptime_ACB.Text = AcData_ACB[1];
                    txtTimer_ACB.Text = AcData_ACB[2];

                    if (AcData_ACB[3] == "1")
                        ckForceOn_ACB.Checked = true;
                    else
                        ckForceOn_ACB.Checked = false;
                }

                if (AcData_ACA[0] == "ON")
                    txtBoxACA.BackColor = Color.LawnGreen;
                else
                    txtBoxACA.BackColor = Color.Red;

                if (AcData_ACB[0] == "ON")
                    txtBoxACB.BackColor = Color.LawnGreen;
                else
                    txtBoxACB.BackColor = Color.Red;
            });

        }
        void UpdateAcData(string msg)
        {
            //string msg = $"{Power_ACA};{Timer_ACA};{ForceOn_ACA}|{Power_ACB};{Timer_ACB};{ForceOn_ACB}";

            string targetClient = server.GetClients().FirstOrDefault(c => c.StartsWith(AcControllerIP));

            if (!string.IsNullOrEmpty(targetClient))
            {
                server.Send(targetClient, msg);
                AppendLog_AC($"Sent: {msg}");
            }
            else
            {
                AppendLog_AC($"ACEsp not found!");
            }
        }

        //==================================================================================================================
        //                                      Server & AC Panel Button Events
        //==================================================================================================================
        private void BtnStartServer_Click(object sender, EventArgs e)
        {
            try
            {
                server.Start();
                AppendLog("Server started on port 8080...");
            }
            catch (Exception ex)
            {
                AppendLog("Error: " + ex.Message);
            }
        }

        private void BtnStopServer_Click(object sender, EventArgs e)
        {
            try
            {
                server.Stop();
                AppendLog("Server stopped.");
            }
            catch (Exception ex)
            {
                AppendLog("Error: " + ex.Message);
            }
        }
        private void btnSettingShowHide_Click(object sender, EventArgs e)
        {
            pnlSetting.Visible = !pnlSetting.Visible;

            if (pnlSetting.Visible)
                btnSettingShowHide.Text = "<<";
            else
                btnSettingShowHide.Text = ">>";
        }

        private void BtnON_ACA_Click(object sender, EventArgs e)
        {
            //Power_ACA = "ON";
            UpdateAcData("PowerACA_ON");

        }

        private void BtnOFF_ACA_Click(object sender, EventArgs e)
        {
            //Power_ACA = "OFF";
            UpdateAcData("PowerACA_OFF");
        }

        private void BtnON_ACB_Click(object sender, EventArgs e)
        {
            //Power_ACB = "ON";
            UpdateAcData("PowerACB_ON");
        }

        private void BtnOFF_ACB_Click(object sender, EventArgs e)
        {
            //Power_ACB = "OFF";
            UpdateAcData("PowerACB_OFF");
        }

        private void ckForceOn_ACA_CheckedChanged(object sender, EventArgs e)
        {
            if (ckForceOn_ACA.Checked)
            {
                UpdateAcData("ForceOnACA_Checked");
                ckForceOn_ACA.BackColor = Color.DodgerBlue;
                ckForceOn_ACA.ForeColor = Color.FloralWhite;
            }
            else
            {
                UpdateAcData("ForceOnACA_NotChecked");
                ckForceOn_ACA.BackColor = Color.WhiteSmoke;
                ckForceOn_ACA.ForeColor = Color.Black;
            }
        }

        private void ckForceOn_ACB_CheckedChanged(object sender, EventArgs e)
        {
            if (ckForceOn_ACB.Checked)
            {
                UpdateAcData("ForceOnACB_Checked");
                ckForceOn_ACB.BackColor = Color.DodgerBlue;
                ckForceOn_ACB.ForeColor = Color.FloralWhite;
            }
            else
            {
                UpdateAcData("ForceOnACB_NotChecked");
                ckForceOn_ACB.BackColor = Color.WhiteSmoke;
                ckForceOn_ACB.ForeColor = Color.Black;
            }
        }

        private void BtnSetTimer_ACA_Click(object sender, EventArgs e)
        {
            decimal seconds = (timerHour_ACA.Value * 3600) + (timerMin_ACA.Value * 60);
            UpdateAcData($"ACA;{seconds}");
        }
        private void btnSetTimer_ACB_Click(object sender, EventArgs e)
        {
            decimal seconds = (timerHour_ACB.Value * 3600) + (timerMin_ACB.Value * 60);
            UpdateAcData($"ACB;{seconds}");
        }
        //==================================================================================================================
        //                                      Update & Display Water Data
        //==================================================================================================================
        private void DisplayWaterData(string msg)
        {
            string[] WaterData = msg.Split('|');

            if (WaterData.Length != 3)
                AppendLog("Invalid Format Recieved");

            WaterData_Pump = WaterData[0].Split(";");
            WaterData_Gyser = WaterData[1].Split(";");
            WaterData_EGyser = WaterData[2].Split(";");

            SafeInvoke(() =>
            {
                if (WaterData_Pump.Length == 3)
                {
                    txtPower_Pump.Text  = WaterData_Pump[0];
                    txtUptime_Pump.Text = WaterData_Pump[1];
                    txtTimer_Pump.Text  = WaterData_Pump[2];
                }
                if (WaterData_Gyser.Length == 3)
                {
                    txtPower_Gyser.Text  = WaterData_Gyser[0];
                    txtUptime_Gyser.Text = WaterData_Gyser[1];
                    txtTimer_Gyser.Text  = WaterData_Gyser[2];
                }
                if (WaterData_EGyser.Length == 3)
                {
                    txtPower_EGyser.Text  = WaterData_EGyser[0];
                    txtUptime_EGyser.Text = WaterData_EGyser[1];
                    txtTimer_EGyser.Text  = WaterData_EGyser[2];
                }

                if (WaterData_Pump[0] == "ON") 
                  txtBoxPump.BackColor = Color.LawnGreen;
               else
                  txtBoxPump.BackColor = Color.Red;
                
                if (WaterData_EGyser[0] == "ON")
                    txtBoxEGyser.BackColor = Color.LawnGreen;
                else
                    txtBoxEGyser.BackColor = Color.Red;

                if (WaterData_Gyser[0] == "ON")
                    txtBoxGyser.BackColor = Color.LawnGreen;
                else
                    txtBoxGyser.BackColor = Color.Red;
            });

        }
        void UpdateWaterData(string msg)
        {
            string targetClient = server.GetClients().FirstOrDefault(c => c.StartsWith(WaterControllerIP));

            if (!string.IsNullOrEmpty(targetClient))
            {
                server.Send(targetClient, msg);
                AppendLog_Water($"Sent: {msg}");
            }
            else
            {
                AppendLog_Water($"Water Esp not found!");
            }
        }
        //==================================================================================================================
        //                                      Water Panel Button Events
        //==================================================================================================================
        private void BtnON_Pump_Click(object sender, EventArgs e)
        {
            UpdateWaterData("PowerPump_ON");
        }
        private void BtnOFF_Pump_Click(object sender, EventArgs e)
        {
            UpdateWaterData("PowerPump_OFF");
        }

        private void BtnON_EGyser_Click(object sender, EventArgs e)
        {
            UpdateWaterData("PowerEGyser_ON");
        }

        private void BtnOFF_EGyser_Click(object sender, EventArgs e)
        {
            UpdateWaterData("PowerEGyser_OFF");
        }

        private void BtnON_Gyser_Click(object sender, EventArgs e)
        {
            UpdateWaterData("PowerGyser_ON");
        }

        private void BtnOFF_Gyser_Click(object sender, EventArgs e)
        {
            UpdateWaterData("PowerGyser_OFF");
        }

        private void BtnSetTimer_Pump_Click(object sender, EventArgs e)
        {
            decimal seconds = (timerHour_Pump.Value * 3600) + (timerMin_Pump.Value * 60);
            UpdateWaterData($"Pump;{seconds}");
        }

        private void BtnSetTimer_EGyser_Click(object sender, EventArgs e)
        {
            decimal seconds = (timerHour_EGyser.Value * 3600) + (timerMin_EGyser.Value * 60);
            UpdateWaterData($"EGyser;{seconds}");
        }

        private void BtnSetTimer_Gyser_Click(object sender, EventArgs e)
        {
            decimal seconds = (timerHour_Gyser.Value * 3600) + (timerMin_Gyser.Value * 60);
            UpdateWaterData($"Gyser;{seconds}");
        }


    }
}
