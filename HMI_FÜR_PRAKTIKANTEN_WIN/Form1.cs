using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using TwinCAT.Ads;

namespace HMI_FÜR_PRAKTIKANTEN_WIN
{
    public partial class Form1 : Form
    {
        Thread readThread;
        public static bool connected = false;
        public static TcAdsClient adsClient;

        //MainVoltage
        public static int mVoltage = 0;

        //Praktikant 1
        public static int In0_1 = 0;
        public static int In0_2 = 0;
        public static int In0_3 = 0;
        public static int In0_4 = 0;
        public static int In0_5 = 0;
        public static int In0_6 = 0;
        public static int In0_7 = 0;
        public static int In0_8 = 0;

        public static int Out0_1 = 0;
        public static int Out0_2 = 0;
        public static int Out0_3 = 0;
        public static int Out0_4 = 0;
        public static int Out0_5 = 0;
        public static int Out0_6 = 0;
        public static int Out0_7 = 0;
        public static int Out0_8 = 0;

        //Praktikant 2
        public static int In1_1 = 0;
        public static int In1_2 = 0;
        public static int In1_3 = 0;
        public static int In1_4 = 0;
        public static int In1_5 = 0;
        public static int In1_6 = 0;
        public static int In1_7 = 0;
        public static int In1_8 = 0;

        public static int Out1_1 = 0;
        public static int Out1_2 = 0;
        public static int Out1_3 = 0;
        public static int Out1_4 = 0;
        public static int Out1_5 = 0;
        public static int Out1_6 = 0;
        public static int Out1_7 = 0;
        public static int Out1_8 = 0;

        //Praktikant 3
        public static int In2_1 = 0;
        public static int In2_2 = 0;
        public static int In2_3 = 0;
        public static int In2_4 = 0;
        public static int In2_5 = 0;
        public static int In2_6 = 0;
        public static int In2_7 = 0;
        public static int In2_8 = 0;

        public static int Out2_1 = 0;
        public static int Out2_2 = 0;
        public static int Out2_3 = 0;
        public static int Out2_4 = 0;
        public static int Out2_5 = 0;
        public static int Out2_6 = 0;
        public static int Out2_7 = 0;
        public static int Out2_8 = 0;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            readThread = new Thread(DoRead);
            adsClient = new TcAdsClient();
            ConnectADS();
        }

        private void Form1_Closing(object sender, FormClosingEventArgs e)
        {
            try
            {
                DisconnectADS();
            }
            catch (Exception ex)
            {
                Environment.Exit(0);
            }
        }

        private void ButtonChangeText(Button btn, string text)
        {
            if (btn.InvokeRequired)
            {
                btn.Invoke((MethodInvoker)delegate { btn.Text = text; });
            }
            else
            {
                btn.Text = text;
            }
        }

        private void ConnectADS()
        {
            try
            {
                textboxText(statustxt, "Trying to Connect", "...");

                // Asynchronized access necessary for Console applications
                adsClient.Synchronize = false;

                // TwinCAT2 RTS1 Port = 801, TwinCAT3 RTS1 Port = 851
                adsClient.Connect(AmsNetId.Local, 801);

                // Read the identification and version number of the device
                DeviceInfo deviceInfo = adsClient.ReadDeviceInfo();
                Version version = deviceInfo.Version.ConvertToStandard();

                // Read the state of the device
                StateInfo stateInfo = adsClient.ReadState();
                AdsState state = stateInfo.AdsState;
                short deviceState = stateInfo.DeviceState;

                // Write ADS Commands (write state) to target
                // Set PLC to Run

                if (state == AdsState.Stop)
                {
                    StateInfo setState = new StateInfo(AdsState.Run, 0);
                    adsClient.WriteControl(setState);
                }
                Thread.Sleep(500);

                ReadEmptyVariables();

                readThread.Start();

                connected = true;
                textboxText(statustxt, "Connected", "...");
            }
            catch (Exception ex)
            {
                textboxText(statustxt, "Error While connecting", "...");
                MessageBox.Show(ex.Message);
            }
        }

        private void DisconnectADS()
        {
            try
            {
                textboxText(statustxt, "Trying to Disconnect", "...");
                Thread.Sleep(1000);
                textboxText(statustxt, "Trying to stop Thread", "...");
                readThread.Abort();
            }
            catch (Exception ex)
            {
                textboxText(statustxt, "Error while stopping Thread", "...");
            }

            try
            {
                textboxText(statustxt, "Trying to stop ADS", "...");
                adsClient.WriteControl(new StateInfo(AdsState.Stop, adsClient.ReadState().DeviceState));
                ADSDispose();
                connected = false;
                textboxText(statustxt, "Disconnected", "...");
                ReadEmptyVariables();
            }
            catch (Exception ex)
            {
                textboxText(statustxt, "Error while disconnecting", "...");
            }
        }

        public void ADSDispose()
        {
            try
            {
                if (InvokeRequired)
                {
                    Invoke(new DoADSDisposeDelegate(DoADSDispose));
                    return;
                }
                if (!adsClient.Disposed)
                {
                    adsClient.Dispose();
                    adsClient.Disconnect();
                }
            }
            catch (Exception ex)
            {
                textboxText(statustxt, "Error while ADSDispose", "...");
            }
        }

        private delegate void DoADSDisposeDelegate();
        private void DoADSDispose()
        {
            try
            {
                if (!adsClient.Disposed)
                {
                    adsClient.Dispose();
                    adsClient.Disconnect();

                }
            }
            catch (Exception ex)
            {
                textboxText(statustxt, "Error while ADSDispose", "...");
            }
        }

        void textboxText(TextBox tb, string Text, string value)
        {
            tb.Invoke(new Action(() =>
            {
                if (value == "0" || value == "False")
                {
                    tb.BackColor = Color.Red;
                }
                else if (value == "1" || value == "True")
                {
                    tb.BackColor = Color.Green;
                }
                else
                {
                    tb.BackColor = Color.White;
                }
                tb.Text = (Text + value);
            }));
        }

        void WriteBtnForceColor(Button btn, int varHandle)
        {
            try
            {
                if (adsClient.ReadAny(varHandle, typeof(Boolean)).ToString() == "False")
                {
                    btn.Text = "Fore";
                    btn.BackColor = Color.Red;
                }
                else
                {
                    btn.Text = "Release";
                    btn.BackColor = Color.Green;
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        void ReadEmptyVariables()
        {
            //MainVoltage
            textboxText(tBMainVoltage, "", mVoltage.ToString());

            //Praktikant 1
            textboxText(tBIn0_1, "In1:", In0_1.ToString());
            textboxText(tBIn0_2, "In2:", In0_2.ToString());
            textboxText(tBIn0_3, "In3:", In0_3.ToString());
            textboxText(tBIn0_4, "In4:", In0_4.ToString());
            textboxText(tBIn0_5, "In5:", In0_5.ToString());
            textboxText(tBIn0_6, "In6:", In0_6.ToString());
            textboxText(tBIn0_7, "In7:", In0_7.ToString());
            textboxText(tBIn0_8, "In8:", In0_8.ToString());

            textboxText(tBOut0_1, "Out1:", Out0_1.ToString());
            textboxText(tBOut0_2, "Out2:", Out0_2.ToString());
            textboxText(tBOut0_3, "Out3:", Out0_3.ToString());
            textboxText(tBOut0_4, "Out4:", Out0_4.ToString());
            textboxText(tBOut0_5, "Out5:", Out0_5.ToString());
            textboxText(tBOut0_6, "Out6:", Out0_6.ToString());
            textboxText(tBOut0_7, "Out7:", Out0_7.ToString());
            textboxText(tBOut0_8, "Out8:", Out0_8.ToString());

            //Praktikant 2
            textboxText(tBIn1_1, "In1:", In1_1.ToString());
            textboxText(tBIn1_2, "In2:", In1_2.ToString());
            textboxText(tBIn1_3, "In3:", In1_3.ToString());
            textboxText(tBIn1_4, "In4:", In1_4.ToString());
            textboxText(tBIn1_5, "In5:", In1_5.ToString());
            textboxText(tBIn1_6, "In6:", In1_6.ToString());
            textboxText(tBIn1_7, "In7:", In1_7.ToString());
            textboxText(tBIn1_8, "In8:", In1_8.ToString());

            textboxText(tBOut1_1, "Out1:", Out1_1.ToString());
            textboxText(tBOut1_2, "Out2:", Out1_2.ToString());
            textboxText(tBOut1_3, "Out3:", Out1_3.ToString());
            textboxText(tBOut1_4, "Out4:", Out1_4.ToString());
            textboxText(tBOut1_5, "Out5:", Out1_5.ToString());
            textboxText(tBOut1_6, "Out6:", Out1_6.ToString());
            textboxText(tBOut1_7, "Out7:", Out1_7.ToString());
            textboxText(tBOut1_8, "Out8:", Out1_8.ToString());

            //Praktikant 3
            textboxText(tBIn2_1, "In1:", In2_1.ToString());
            textboxText(tBIn2_2, "In2:", In2_2.ToString());
            textboxText(tBIn2_3, "In3:", In2_3.ToString());
            textboxText(tBIn2_4, "In4:", In2_4.ToString());
            textboxText(tBIn2_5, "In5:", In2_5.ToString());
            textboxText(tBIn2_6, "In6:", In2_6.ToString());
            textboxText(tBIn2_7, "In7:", In2_7.ToString());
            textboxText(tBIn2_8, "In8:", In2_8.ToString());

            textboxText(tBOut2_1, "Out1:", Out2_1.ToString());
            textboxText(tBOut2_2, "Out2:", Out2_2.ToString());
            textboxText(tBOut2_3, "Out3:", Out2_3.ToString());
            textboxText(tBOut2_4, "Out4:", Out2_4.ToString());
            textboxText(tBOut2_5, "Out5:", Out2_5.ToString());
            textboxText(tBOut2_6, "Out6:", Out2_6.ToString());
            textboxText(tBOut2_7, "Out7:", Out2_7.ToString());
            textboxText(tBOut2_8, "Out8:", Out2_8.ToString());
        }

        void ReadVariables()
        {
            try
            {
                //MainVoltage
                textboxText(tBMainVoltage, "", adsClient.ReadAny(mVoltage, typeof(Boolean)).ToString());
                WriteBtnForceColor(BtnMainVoltage, mVoltage);

                //Praktikant 1
                textboxText(tBIn0_1, "", adsClient.ReadAny(In0_1, typeof(Boolean)).ToString());
                textboxText(tBIn0_2, "", adsClient.ReadAny(In0_2, typeof(Boolean)).ToString());
                textboxText(tBIn0_3, "", adsClient.ReadAny(In0_3, typeof(Boolean)).ToString());
                textboxText(tBIn0_4, "", adsClient.ReadAny(In0_4, typeof(Boolean)).ToString());
                textboxText(tBIn0_5, "", adsClient.ReadAny(In0_5, typeof(Boolean)).ToString());
                textboxText(tBIn0_6, "", adsClient.ReadAny(In0_6, typeof(Boolean)).ToString());
                textboxText(tBIn0_7, "", adsClient.ReadAny(In0_7, typeof(Boolean)).ToString());
                textboxText(tBIn0_8, "", adsClient.ReadAny(In0_8, typeof(Boolean)).ToString());

                textboxText(tBOut0_1, "", adsClient.ReadAny(Out0_1, typeof(Boolean)).ToString());
                textboxText(tBOut0_2, "", adsClient.ReadAny(Out0_2, typeof(Boolean)).ToString());
                textboxText(tBOut0_3, "", adsClient.ReadAny(Out0_3, typeof(Boolean)).ToString());
                textboxText(tBOut0_4, "", adsClient.ReadAny(Out0_4, typeof(Boolean)).ToString());
                textboxText(tBOut0_5, "", adsClient.ReadAny(Out0_5, typeof(Boolean)).ToString());
                textboxText(tBOut0_6, "", adsClient.ReadAny(Out0_6, typeof(Boolean)).ToString());
                textboxText(tBOut0_7, "", adsClient.ReadAny(Out0_7, typeof(Boolean)).ToString());
                textboxText(tBOut0_8, "", adsClient.ReadAny(Out0_8, typeof(Boolean)).ToString());

                WriteBtnForceColor(BtnForceOut0_1, Out0_1);
                WriteBtnForceColor(BtnForceOut0_2, Out0_2);
                WriteBtnForceColor(BtnForceOut0_3, Out0_3);
                WriteBtnForceColor(BtnForceOut0_4, Out0_4);
                WriteBtnForceColor(BtnForceOut0_5, Out0_5);
                WriteBtnForceColor(BtnForceOut0_6, Out0_6);
                WriteBtnForceColor(BtnForceOut0_7, Out0_7);
                WriteBtnForceColor(BtnForceOut0_8, Out0_8);

                //Praktikant 2
                textboxText(tBIn1_1, "", adsClient.ReadAny(In1_1, typeof(Boolean)).ToString());
                textboxText(tBIn1_2, "", adsClient.ReadAny(In1_2, typeof(Boolean)).ToString());
                textboxText(tBIn1_3, "", adsClient.ReadAny(In1_3, typeof(Boolean)).ToString());
                textboxText(tBIn1_4, "", adsClient.ReadAny(In1_4, typeof(Boolean)).ToString());
                textboxText(tBIn1_5, "", adsClient.ReadAny(In1_5, typeof(Boolean)).ToString());
                textboxText(tBIn1_6, "", adsClient.ReadAny(In1_6, typeof(Boolean)).ToString());
                textboxText(tBIn1_7, "", adsClient.ReadAny(In1_7, typeof(Boolean)).ToString());
                textboxText(tBIn1_8, "", adsClient.ReadAny(In1_8, typeof(Boolean)).ToString());

                textboxText(tBOut1_1, "", adsClient.ReadAny(Out1_1, typeof(Boolean)).ToString());
                textboxText(tBOut1_2, "", adsClient.ReadAny(Out1_2, typeof(Boolean)).ToString());
                textboxText(tBOut1_3, "", adsClient.ReadAny(Out1_3, typeof(Boolean)).ToString());
                textboxText(tBOut1_4, "", adsClient.ReadAny(Out1_4, typeof(Boolean)).ToString());
                textboxText(tBOut1_5, "", adsClient.ReadAny(Out1_5, typeof(Boolean)).ToString());
                textboxText(tBOut1_6, "", adsClient.ReadAny(Out1_6, typeof(Boolean)).ToString());
                textboxText(tBOut1_7, "", adsClient.ReadAny(Out1_7, typeof(Boolean)).ToString());
                textboxText(tBOut1_8, "", adsClient.ReadAny(Out1_8, typeof(Boolean)).ToString());

                WriteBtnForceColor(BtnForceOut1_1, Out1_1);
                WriteBtnForceColor(BtnForceOut1_2, Out1_2);
                WriteBtnForceColor(BtnForceOut1_3, Out1_3);
                WriteBtnForceColor(BtnForceOut1_4, Out1_4);
                WriteBtnForceColor(BtnForceOut1_5, Out1_5);
                WriteBtnForceColor(BtnForceOut1_6, Out1_6);
                WriteBtnForceColor(BtnForceOut1_7, Out1_7);
                WriteBtnForceColor(BtnForceOut1_8, Out1_8);

                //Praktikant 3
                textboxText(tBIn2_1, "", adsClient.ReadAny(In2_1, typeof(Boolean)).ToString());
                textboxText(tBIn2_2, "", adsClient.ReadAny(In2_2, typeof(Boolean)).ToString());
                textboxText(tBIn2_3, "", adsClient.ReadAny(In2_3, typeof(Boolean)).ToString());
                textboxText(tBIn2_4, "", adsClient.ReadAny(In2_4, typeof(Boolean)).ToString());
                textboxText(tBIn2_5, "", adsClient.ReadAny(In2_5, typeof(Boolean)).ToString());
                textboxText(tBIn2_6, "", adsClient.ReadAny(In2_6, typeof(Boolean)).ToString());
                textboxText(tBIn2_7, "", adsClient.ReadAny(In2_7, typeof(Boolean)).ToString());
                textboxText(tBIn2_8, "", adsClient.ReadAny(In2_8, typeof(Boolean)).ToString());

                textboxText(tBOut2_1, "", adsClient.ReadAny(Out2_1, typeof(Boolean)).ToString());
                textboxText(tBOut2_2, "", adsClient.ReadAny(Out2_2, typeof(Boolean)).ToString());
                textboxText(tBOut2_3, "", adsClient.ReadAny(Out2_3, typeof(Boolean)).ToString());
                textboxText(tBOut2_4, "", adsClient.ReadAny(Out2_4, typeof(Boolean)).ToString());
                textboxText(tBOut2_5, "", adsClient.ReadAny(Out2_5, typeof(Boolean)).ToString());
                textboxText(tBOut2_6, "", adsClient.ReadAny(Out2_6, typeof(Boolean)).ToString());
                textboxText(tBOut2_7, "", adsClient.ReadAny(Out2_7, typeof(Boolean)).ToString());
                textboxText(tBOut2_8, "", adsClient.ReadAny(Out2_8, typeof(Boolean)).ToString());

                WriteBtnForceColor(BtnForceOut2_1, Out2_1);
                WriteBtnForceColor(BtnForceOut2_2, Out2_2);
                WriteBtnForceColor(BtnForceOut2_3, Out2_3);
                WriteBtnForceColor(BtnForceOut2_4, Out2_4);
                WriteBtnForceColor(BtnForceOut2_5, Out2_5);
                WriteBtnForceColor(BtnForceOut2_6, Out2_6);
                WriteBtnForceColor(BtnForceOut2_7, Out2_7);
                WriteBtnForceColor(BtnForceOut2_8, Out2_8);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        void CreateVariableHandle() {
            //MainVoltage
            mVoltage = adsClient.CreateVariableHandle("MAIN.OutMainVolt");


            //Praktikant 1
            In0_1 = adsClient.CreateVariableHandle("MAIN.In0_1");
            In0_2 = adsClient.CreateVariableHandle("MAIN.In0_2");
            In0_3 = adsClient.CreateVariableHandle("MAIN.In0_3");
            In0_4 = adsClient.CreateVariableHandle("MAIN.In0_4");
            In0_5 = adsClient.CreateVariableHandle("MAIN.In0_5");
            In0_6 = adsClient.CreateVariableHandle("MAIN.In0_6");
            In0_7 = adsClient.CreateVariableHandle("MAIN.In0_7");
            In0_8 = adsClient.CreateVariableHandle("MAIN.In0_8");

            Out0_1 = adsClient.CreateVariableHandle("MAIN.Out0_1");
            Out0_2 = adsClient.CreateVariableHandle("MAIN.Out0_2");
            Out0_3 = adsClient.CreateVariableHandle("MAIN.Out0_3");
            Out0_4 = adsClient.CreateVariableHandle("MAIN.Out0_4");
            Out0_5 = adsClient.CreateVariableHandle("MAIN.Out0_5");
            Out0_6 = adsClient.CreateVariableHandle("MAIN.Out0_6");
            Out0_7 = adsClient.CreateVariableHandle("MAIN.Out0_7");
            Out0_8 = adsClient.CreateVariableHandle("MAIN.Out0_8");

            //Praktikant 2
            In1_1 = adsClient.CreateVariableHandle("MAIN.In1_1");
            In1_2 = adsClient.CreateVariableHandle("MAIN.In1_2");
            In1_3 = adsClient.CreateVariableHandle("MAIN.In1_3");
            In1_4 = adsClient.CreateVariableHandle("MAIN.In1_4");
            In1_5 = adsClient.CreateVariableHandle("MAIN.In1_5");
            In1_6 = adsClient.CreateVariableHandle("MAIN.In1_6");
            In1_7 = adsClient.CreateVariableHandle("MAIN.In1_7");
            In1_8 = adsClient.CreateVariableHandle("MAIN.In1_8");

            Out1_1 = adsClient.CreateVariableHandle("MAIN.Out1_1");
            Out1_2 = adsClient.CreateVariableHandle("MAIN.Out1_2");
            Out1_3 = adsClient.CreateVariableHandle("MAIN.Out1_3");
            Out1_4 = adsClient.CreateVariableHandle("MAIN.Out1_4");
            Out1_5 = adsClient.CreateVariableHandle("MAIN.Out1_5");
            Out1_6 = adsClient.CreateVariableHandle("MAIN.Out1_6");
            Out1_7 = adsClient.CreateVariableHandle("MAIN.Out1_7");
            Out1_8 = adsClient.CreateVariableHandle("MAIN.Out1_8");

            //Praktikant 2
            In2_1 = adsClient.CreateVariableHandle("MAIN.In2_1");
            In2_2 = adsClient.CreateVariableHandle("MAIN.In2_2");
            In2_3 = adsClient.CreateVariableHandle("MAIN.In2_3");
            In2_4 = adsClient.CreateVariableHandle("MAIN.In2_4");
            In2_5 = adsClient.CreateVariableHandle("MAIN.In2_5");
            In2_6 = adsClient.CreateVariableHandle("MAIN.In2_6");
            In2_7 = adsClient.CreateVariableHandle("MAIN.In2_7");
            In2_8 = adsClient.CreateVariableHandle("MAIN.In2_8");

            Out2_1 = adsClient.CreateVariableHandle("MAIN.Out2_1");
            Out2_2 = adsClient.CreateVariableHandle("MAIN.Out2_2");
            Out2_3 = adsClient.CreateVariableHandle("MAIN.Out2_3");
            Out2_4 = adsClient.CreateVariableHandle("MAIN.Out2_4");
            Out2_5 = adsClient.CreateVariableHandle("MAIN.Out2_5");
            Out2_6 = adsClient.CreateVariableHandle("MAIN.Out2_6");
            Out2_7 = adsClient.CreateVariableHandle("MAIN.Out2_7");
            Out2_8 = adsClient.CreateVariableHandle("MAIN.Out2_8");
        }

        void DoRead()
        {
            while (connected == true)
            {
                try
                {
                    CreateVariableHandle();
                    Thread.Sleep(500);
                    ReadVariables();

                    Thread.Sleep(1000);

                }
                catch (Exception ex)
                {
                    DisconnectADS();
                    MessageBox.Show(ex.Message);
                }
            }
        }

        private void label39_Click(object sender, EventArgs e)
        {

        }

        void forceBool(int varHandle, Button th)
        {
            try
            {
                if (adsClient.ReadAny(varHandle, typeof(Boolean)).ToString() == "False")
                {
                    adsClient.WriteAny(varHandle, Boolean.Parse("True"));
                    th.Text = "Release";
                    th.BackColor = Color.Green;
                }
                else
                {
                    adsClient.WriteAny(varHandle, Boolean.Parse("False"));
                    th.Text = "Force";
                    th.BackColor = Color.Red;
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        //MainVoltage Button
        private void BtnMainVoltage_Click(object sender, EventArgs e)
        {
            forceBool(mVoltage, BtnMainVoltage);
        }


        //Button for Praktikant1
        private void BtnForceOut0_1_Click(object sender, EventArgs e)
        {
            forceBool(Out0_1, BtnForceOut0_1);
        }

        private void BtnForceOut0_2_Click(object sender, EventArgs e)
        {
            forceBool(Out0_2, BtnForceOut0_2);
        }

        private void BtnForceOut0_3_Click(object sender, EventArgs e)
        {
            forceBool(Out0_3, BtnForceOut0_3);
        }

        private void BtnForceOut0_4_Click(object sender, EventArgs e)
        {
            forceBool(Out0_4, BtnForceOut0_4);
        }

        private void BtnForceOut0_5_Click(object sender, EventArgs e)
        {
            forceBool(Out0_5, BtnForceOut0_5);
        }

        private void BtnForceOut0_6_Click(object sender, EventArgs e)
        {
            forceBool(Out0_6, BtnForceOut0_6);
        }

        private void BtnForceOut0_7_Click(object sender, EventArgs e)
        {
            forceBool(Out0_7, BtnForceOut0_7);
        }

        private void BtnForceOut0_8_Click(object sender, EventArgs e)
        {
            forceBool(Out0_8, BtnForceOut0_8);
        }


        //Button for Praktikant2

        private void BtnForceOut1_1_Click(object sender, EventArgs e)
        {
            forceBool(Out1_1, BtnForceOut1_1);
        }

        private void BtnForceOut1_2_Click(object sender, EventArgs e)
        {
            forceBool(Out1_2, BtnForceOut1_2);
        }

        private void BtnForceOut1_3_Click(object sender, EventArgs e)
        {
            forceBool(Out1_3, BtnForceOut1_3);
        }

        private void BtnForceOut1_4_Click(object sender, EventArgs e)
        {
            forceBool(Out1_4, BtnForceOut1_4);
        }

        private void BtnForceOut1_5_Click(object sender, EventArgs e)
        {
            forceBool(Out1_5, BtnForceOut1_5);
        }

        private void BtnForceOut1_6_Click(object sender, EventArgs e)
        {
            forceBool(Out1_6, BtnForceOut1_6);
        }

        private void BtnForceOut1_7_Click(object sender, EventArgs e)
        {
            forceBool(Out1_7, BtnForceOut1_7);
        }

        private void BtnForceOut1_8_Click(object sender, EventArgs e)
        {
            forceBool(Out1_8, BtnForceOut1_8);
        }


        //Button for Praktikant3
        private void BtnForceOut2_1_Click(object sender, EventArgs e)
        {
            forceBool(Out2_1, BtnForceOut2_1);
        }

        private void BtnForceOut2_2_Click(object sender, EventArgs e)
        {
            forceBool(Out2_2, BtnForceOut2_2);
        }

        private void BtnForceOut2_3_Click(object sender, EventArgs e)
        {
            forceBool(Out2_3, BtnForceOut2_3);
        }

        private void BtnForceOut2_4_Click(object sender, EventArgs e)
        {
            forceBool(Out2_4, BtnForceOut2_4);
        }

        private void BtnForceOut2_5_Click(object sender, EventArgs e)
        {
            forceBool(Out2_5, BtnForceOut2_5);
        }

        private void BtnForceOut2_6_Click(object sender, EventArgs e)
        {
            forceBool(Out2_6, BtnForceOut2_6);
        }

        private void BtnForceOut2_7_Click(object sender, EventArgs e)
        {
            forceBool(Out2_7, BtnForceOut2_7);
        }

        private void BtnForceOut2_8_Click(object sender, EventArgs e)
        {
            forceBool(Out2_8, BtnForceOut2_8);

        }
    }
}
