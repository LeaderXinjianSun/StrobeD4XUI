using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using DXH.Robot;
using BingLibrary.Net.net;
using BingLibrary.hjb.file;

namespace D4XUI
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        #region 主体
        string MessageStr = "";
        DispatcherTimer dispatcherTimer = new DispatcherTimer();
        string iniParameterPath = System.Environment.CurrentDirectory + "\\Parameter.ini";
        public MainWindow()
        {
            InitializeComponent();

            dispatcherTimer.Tick += new EventHandler(DispatcherTimerTickUpdateUi);
            dispatcherTimer.Interval = new TimeSpan(0, 0, 0, 0, 100);
            dispatcherTimer.Start();
        }
        private void MsgTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            MsgTextBox.ScrollToEnd();
        }
        private void DispatcherTimerTickUpdateUi(Object sender, EventArgs e)
        {
            MsgTextBox.Text = MessageStr;
            PLCStatusEllipse.Fill = plcstate ? Brushes.Green : Brushes.Red;
        }
        public void AddMessage(string str)
        {
            string[] s = MessageStr.Split('\n');
            if (s.Length > 1000)
            {
                MessageStr = "";
            }
            if (MessageStr != "")
            {
                MessageStr += "\n";
            }
            MessageStr += System.DateTime.Now.ToString("HH:mm:ss") + " " + str;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            PLCInit();
            UDPInit();
            AddMessage("加载完成");
        }
        #endregion
        #region PLC
        DXHModbusTCP Xinjie = new DXHModbusTCP();
        bool plcstate = false;
        void PLCInit()
        {            
            Xinjie.RemoteIPAddress = Inifile.INIGetStringValue(iniParameterPath, "PLC", "IP", "192.168.0.103");
            Xinjie.RemoteIPPort = int.Parse(Inifile.INIGetStringValue(iniParameterPath, "PLC", "Port", "502"));
            Xinjie.StartTCPConnect();
        }
        #endregion
        #region UDP
        UDPClient udp1 = new UDPClient();
        void UDPInit()
        {
            string ip;
            int localport, targetport;
            ip = Inifile.INIGetStringValue(iniParameterPath, "转盘", "IP", "192.168.0.100");
            localport = int.Parse(Inifile.INIGetStringValue(iniParameterPath, "转盘", "LocalPort", "3000"));
            targetport = int.Parse(Inifile.INIGetStringValue(iniParameterPath, "转盘", "TargetPort", "5000"));
            udp1.Connect(localport, targetport, ip);
        }
        #endregion

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            int[] mCurStatusPLC = Xinjie.ModbusTCPRead(1, 1, 200, 16);//读状态 M200}
            await udp1.SendAsync("test");
            AddMessage(await udp1.ReceiveAsync());
        }
    }
}
