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
using BingLibrary.Net.net;
using BingLibrary.hjb.file;
using BingLibrary.hjb.PLC;
using System.Collections.ObjectModel;
using BingLibrary.hjb.tools;
using OfficeOpenXml;
using System.IO;

namespace D4XUI
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        #region 主体
        #region 变量
        string MessageStr = "";
        DispatcherTimer dispatcherTimer = new DispatcherTimer();
        private string iniParameterPath = System.Environment.CurrentDirectory + "\\Parameter.ini";
        private string iniFClient = @"C:\FClient.ini";
        private string iniTimelyRainPath = System.Environment.CurrentDirectory + "\\TimelyRain.ini";
        string alarmExcelPath = System.Environment.CurrentDirectory + "\\D4X报警.xlsx";
        List<string> AlarmList = new List<string>();
        double downtime, zhuanpandowntime, lingmindudowntime, tiemojidowntime, waitzhuanpanforinput, waitlingminduforinput, waitTiemojiforinput, waitfortake, worktime, runtime;
        string DangbanFirstProduct = "";
        string LastBanci = "";
        Double UPH;
        #endregion
        public MainWindow()
        {
            InitializeComponent();

            dispatcherTimer.Tick += new EventHandler(DispatcherTimerTickUpdateUi);
            dispatcherTimer.Interval = new TimeSpan(0, 0, 0, 6);//6秒更新一次，即0.1分钟。

        }
        private void MsgTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            MsgTextBox.ScrollToEnd();
        }
        private void DispatcherTimerTickUpdateUi(Object sender, EventArgs e)
        {
            MsgTextBox.Text = MessageStr;
            PLCStatusEllipse.Fill = plcstate ? Brushes.Green : Brushes.Red;
            AlarmGrid.Visibility = Visibility.Collapsed;
            if (M10000 != null && plcstate)
            {
                for (int i = 0; i < AlarmList.Count; i++)
                {
                    if (M10000[i])
                    {
                        AlarmGrid.Visibility = Visibility.Visible;
                        AlarmTextBlock.Text = AlarmList[i];
                        break;
                    }
                }
            }
            if (AlarmGrid.Visibility == Visibility.Visible)
            {
                Inifile.INIWriteValue(iniFClient, "Alarm", "Name", AlarmTextBlock.Text);
            }
            else
            {
                Inifile.INIWriteValue(iniFClient, "Alarm", "Name", "NULL");
            }
            #region 及时雨
            if (M10000 != null && plcstate)
            {
                if (LastBanci != GetBanci())
                {
                    LastBanci = GetBanci();
                    Inifile.INIWriteValue(iniParameterPath, "Summary", "LastBanci", LastBanci);
                    AddMessage(LastBanci + " 换班数据清零");
                    WriteMachineData();
                    downtime = 0;
                    Inifile.INIWriteValue(iniTimelyRainPath, "TimelyRain", "Downtime", downtime.ToString());
                    zhuanpandowntime = 0;
                    Inifile.INIWriteValue(iniTimelyRainPath, "TimelyRain", "Zhuanpandowntime", zhuanpandowntime.ToString());
                    lingmindudowntime = 0;
                    Inifile.INIWriteValue(iniTimelyRainPath, "TimelyRain", "Lingmindudowntime", lingmindudowntime.ToString());
                    tiemojidowntime = 0;
                    Inifile.INIWriteValue(iniTimelyRainPath, "TimelyRain", "Tiemojidowntime", tiemojidowntime.ToString());
                    waitzhuanpanforinput = 0;
                    Inifile.INIWriteValue(iniTimelyRainPath, "TimelyRain", "Waitzhuanpanforinput", waitzhuanpanforinput.ToString());
                    waitlingminduforinput = 0;
                    Inifile.INIWriteValue(iniTimelyRainPath, "TimelyRain", "Waitlingminduforinput", waitlingminduforinput.ToString());
                    waitfortake = 0;
                    Inifile.INIWriteValue(iniTimelyRainPath, "TimelyRain", "Waitfortake", waitfortake.ToString());
                    worktime = 0;
                    Inifile.INIWriteValue(iniTimelyRainPath, "TimelyRain", "Worktime", worktime.ToString());
                    runtime = 0;
                    Inifile.INIWriteValue(iniTimelyRainPath, "TimelyRain", "Runtime", runtime.ToString());

                    Xinjie.SetM(10099, true);//通知PLC换班，计数清空
                }
                if (D1200 == 1 && DangbanFirstProduct != GetBanci())
                {
                    DangbanFirstProduct = GetBanci();
                    Inifile.INIWriteValue(iniParameterPath, "Summary", "DangbanFirstProduct", DangbanFirstProduct);
                    AddMessage(DangbanFirstProduct + " 开始生产");
                }
                if (M10000[100] && DangbanFirstProduct == GetBanci())
                {
                    downtime += 0.1;
                    Inifile.INIWriteValue(iniTimelyRainPath, "TimelyRain", "Downtime", downtime.ToString());
                }
                if (M10000[101] && DangbanFirstProduct == GetBanci())
                {
                    zhuanpandowntime += 0.1;
                    Inifile.INIWriteValue(iniTimelyRainPath, "TimelyRain", "Zhuanpandowntime", zhuanpandowntime.ToString());
                }
                if (M10000[102] && DangbanFirstProduct == GetBanci())
                {
                    lingmindudowntime += 0.1;
                    Inifile.INIWriteValue(iniTimelyRainPath, "TimelyRain", "Lingmindudowntime", lingmindudowntime.ToString());
                }
                if (M10000[103] && DangbanFirstProduct == GetBanci())
                {
                    tiemojidowntime += 0.1;
                    Inifile.INIWriteValue(iniTimelyRainPath, "TimelyRain", "Tiemojidowntime", tiemojidowntime.ToString());
                }
                if (M10000[104] && DangbanFirstProduct == GetBanci())
                {
                    waitzhuanpanforinput += 0.1;
                    Inifile.INIWriteValue(iniTimelyRainPath, "TimelyRain", "Waitzhuanpanforinput", waitzhuanpanforinput.ToString());
                }
                if (M10000[105] && DangbanFirstProduct == GetBanci())
                {
                    waitlingminduforinput += 0.1;
                    Inifile.INIWriteValue(iniTimelyRainPath, "TimelyRain", "Waitlingminduforinput", waitlingminduforinput.ToString());
                }
                if (M10000[106] && DangbanFirstProduct == GetBanci())
                {
                    waitTiemojiforinput += 0.1;
                    Inifile.INIWriteValue(iniTimelyRainPath, "TimelyRain", "WaitTiemojiforinput", waitTiemojiforinput.ToString());
                }
                if (M10000[107] && DangbanFirstProduct == GetBanci())
                {
                    waitfortake += 0.1;
                    Inifile.INIWriteValue(iniTimelyRainPath, "TimelyRain", "Waitfortake", waitfortake.ToString());
                }
                input.Text = HD200[0].ToString();
                Inifile.INIWriteValue(iniFClient, "DataList", "input", input.Text);
                output.Text = HD200[1].ToString();
                Inifile.INIWriteValue(iniFClient, "DataList", "output", output.Text);
                TestCount_Total.Text = HD200[2].ToString();
                Inifile.INIWriteValue(iniFClient, "DataList", "TestCount_Total", TestCount_Total.Text);
                PassCount_Total.Text = HD200[3].ToString();
                if (HD200[2] == 0)
                {
                    Yield_Total.Text = "0";
                }
                else
                {
                    Yield_Total.Text = (HD200[3] / HD200[2] * 100).ToString("F1");
                }
                Inifile.INIWriteValue(iniFClient, "DataList", "Yield_Total", Yield_Total.Text);
                TestCount_1.Text = HD200[4].ToString();
                Inifile.INIWriteValue(iniFClient, "DataList", "TestCount_1", TestCount_1.Text);
                PassCount_1.Text = HD200[5].ToString();
                if (HD200[4] == 0)
                {
                    Yield_1.Text = "0";
                }
                else
                {
                    Yield_1.Text = (HD200[5] / HD200[4] * 100).ToString("F1");
                }
                Inifile.INIWriteValue(iniFClient, "DataList", "Yield_1", Yield_1.Text);

                TestCount_2.Text = HD200[6].ToString();
                Inifile.INIWriteValue(iniFClient, "DataList", "TestCount_2", TestCount_2.Text);
                PassCount_2.Text = HD200[7].ToString();
                if (HD200[6] == 0)
                {
                    Yield_2.Text = "0";
                }
                else
                {
                    Yield_2.Text = (HD200[7] / HD200[6] * 100).ToString("F1");
                }
                Inifile.INIWriteValue(iniFClient, "DataList", "Yield_2", Yield_2.Text);
                AlarmCount.Text = HD200[8].ToString();
                Inifile.INIWriteValue(iniFClient, "Alarm", "count", AlarmCount.Text);
                Inifile.INIWriteValue(iniFClient, "state", "state", D1200.ToString());

                if (DangbanFirstProduct == GetBanci())
                {
                    worktime += 0.1;
                    Inifile.INIWriteValue(iniTimelyRainPath, "TimelyRain", "Worktime", worktime.ToString());
                }
                if (DangbanFirstProduct == GetBanci() && D1200 == 1)
                {
                    runtime += 0.1;
                    Inifile.INIWriteValue(iniTimelyRainPath, "TimelyRain", "Runtime", runtime.ToString());
                }
                if (runtime == 0 || UPH == 0)
                    AchievingRate.Text = "100";
                else
                    AchievingRate.Text = (HD200[1] / (UPH / 60 * runtime) * 100).ToString("F1");
                Inifile.INIWriteValue(iniFClient, "DataList", "AchievingRate", AchievingRate.Text);

            }
            Downtime.Text = downtime.ToString();
            Zhuanpandowntime.Text = zhuanpandowntime.ToString();
            Lingmindudowntime.Text = lingmindudowntime.ToString();
            Tiemojidowntime.Text = tiemojidowntime.ToString();
            Waitzhuanpanforinput.Text = waitzhuanpanforinput.ToString();
            Waitlingminduforinput.Text = waitlingminduforinput.ToString();
            WaitTiemojiforinput.Text = waitTiemojiforinput.ToString();
            Waitfortake.Text = waitfortake.ToString();
            Inifile.INIWriteValue(iniFClient, "DataList", "Downtime", downtime.ToString());
            Inifile.INIWriteValue(iniFClient, "DataList", "Zhuanpandowntime", zhuanpandowntime.ToString());
            Inifile.INIWriteValue(iniFClient, "DataList", "Lingmindudowntime", lingmindudowntime.ToString());
            Inifile.INIWriteValue(iniFClient, "DataList", "Tiemojidowntime", tiemojidowntime.ToString());
            Inifile.INIWriteValue(iniFClient, "DataList", "Waitzhuanpanforinput", waitzhuanpanforinput.ToString());
            Inifile.INIWriteValue(iniFClient, "DataList", "Waitlingminduforinput", waitlingminduforinput.ToString());
            Inifile.INIWriteValue(iniFClient, "DataList", "WaitTiemojiforinput", waitTiemojiforinput.ToString());
            Inifile.INIWriteValue(iniFClient, "DataList", "Waitfortake", waitfortake.ToString());

            if (worktime == 0)
            {
                ProperRate.Text = "0";
                ProperRate_AutoMation.Text = "0";
                ProperRate_Zhuanpan.Text = "0";
                ProperRate_Lingmindu.Text = "0";
                ProperRate_Tiemoji.Text = "0";
            }
            else
            {
                ProperRate.Text = ((1 - (downtime + zhuanpandowntime + lingmindudowntime + tiemojidowntime) / worktime) * 100).ToString("F1");
                ProperRate_AutoMation.Text = ((1 - downtime / worktime) * 100).ToString("F1");
                ProperRate_Zhuanpan.Text = ((1 - zhuanpandowntime / worktime) * 100).ToString("F1");
                ProperRate_Lingmindu.Text = ((1 - lingmindudowntime / worktime) * 100).ToString("F1");
                ProperRate_Tiemoji.Text = ((1 - tiemojidowntime / worktime) * 100).ToString("F1");
            }
            Inifile.INIWriteValue(iniFClient, "DataList", "ProperRate", ProperRate.Text);
            Inifile.INIWriteValue(iniFClient, "DataList", "ProperRate_AutoMation", ProperRate_AutoMation.Text);
            Inifile.INIWriteValue(iniFClient, "DataList", "ProperRate_Zhuanpan", ProperRate_Zhuanpan.Text);
            Inifile.INIWriteValue(iniFClient, "DataList", "ProperRate_Lingmindu", ProperRate_Lingmindu.Text);
            Inifile.INIWriteValue(iniFClient, "DataList", "ProperRate_Tiemoji", ProperRate_Tiemoji.Text);
            #endregion

        }
        private void WriteMachineData()
        {
            string excelpath = @"D:\D4XMachineData.xlsx";
            try
            {
                FileInfo fileInfo = new FileInfo(excelpath);
                if (!File.Exists(excelpath))
                {
                    using (ExcelPackage package = new ExcelPackage(fileInfo))
                    {
                        ExcelWorksheet worksheet = package.Workbook.Worksheets.Add("MachineData");
                        worksheet.Cells[1, 1].Value = "更新时间";
                        worksheet.Cells[1, 2].Value = "上料机故障时间";
                        worksheet.Cells[1, 3].Value = "转盘故障时间";
                        worksheet.Cells[1, 4].Value = "灵敏度故障时间";
                        worksheet.Cells[1, 5].Value = "贴膜机故障时间";
                        worksheet.Cells[1, 6].Value = "等待上料转盘时间";
                        worksheet.Cells[1, 7].Value = "等待上灵敏度时间";
                        worksheet.Cells[1, 8].Value = "等待下贴膜机时间";
                        worksheet.Cells[1, 9].Value = "上料/收盘等待时间";
                        worksheet.Cells[1, 10].Value = "上料机投入数量";
                        worksheet.Cells[1, 11].Value = "上料机产出数量";
                        worksheet.Cells[1, 12].Value = "总测试数量";
                        worksheet.Cells[1, 13].Value = "总PASS数量";
                        worksheet.Cells[1, 14].Value = "总直通率";
                        worksheet.Cells[1, 15].Value = "转盘测试数量";
                        worksheet.Cells[1, 16].Value = "转盘PASS数量";
                        worksheet.Cells[1, 17].Value = "转盘直通率";
                        worksheet.Cells[1, 18].Value = "灵敏度测试数量";
                        worksheet.Cells[1, 19].Value = "灵敏度PASS数量";
                        worksheet.Cells[1, 20].Value = "灵敏度直通率";
                        worksheet.Cells[1, 21].Value = "上料机报警数量";
                        worksheet.Cells[1, 22].Value = "达成率";
                        worksheet.Cells[1, 23].Value = "妥善率";
                        worksheet.Cells[1, 24].Value = "上料机妥善率";
                        worksheet.Cells[1, 25].Value = "转盘治具妥善率";
                        worksheet.Cells[1, 26].Value = "灵敏度治具妥善率";
                        worksheet.Cells[1, 27].Value = "贴膜机妥善率";
                        package.Save();
                    }
                }


                using (ExcelPackage package = new ExcelPackage(fileInfo))
                {
                    ExcelWorksheet worksheet = package.Workbook.Worksheets[1];
                    int newrow = worksheet.Dimension.End.Row + 1;
                    worksheet.Cells[newrow, 1].Value = System.DateTime.Now.ToString();
                    worksheet.Cells[newrow, 2].Value = Downtime.Text;
                    worksheet.Cells[newrow, 3].Value = Zhuanpandowntime.Text;
                    worksheet.Cells[newrow, 4].Value = Lingmindudowntime.Text;
                    worksheet.Cells[newrow, 5].Value = Tiemojidowntime.Text;
                    worksheet.Cells[newrow, 6].Value = Waitzhuanpanforinput.Text;
                    worksheet.Cells[newrow, 7].Value = Waitlingminduforinput.Text;
                    worksheet.Cells[newrow, 8].Value = WaitTiemojiforinput.Text;
                    worksheet.Cells[newrow, 9].Value = Waitfortake.Text;
                    worksheet.Cells[newrow, 10].Value = input.Text;
                    worksheet.Cells[newrow, 11].Value = output.Text;
                    worksheet.Cells[newrow, 12].Value = TestCount_Total.Text;
                    worksheet.Cells[newrow, 13].Value = PassCount_Total.Text;
                    worksheet.Cells[newrow, 14].Value = Yield_Total.Text;
                    worksheet.Cells[newrow, 15].Value = TestCount_1.Text;
                    worksheet.Cells[newrow, 16].Value = PassCount_1.Text;
                    worksheet.Cells[newrow, 17].Value = Yield_1.Text;
                    worksheet.Cells[newrow, 18].Value = TestCount_2.Text;
                    worksheet.Cells[newrow, 19].Value = PassCount_2.Text;
                    worksheet.Cells[newrow, 20].Value = Yield_2.Text;
                    worksheet.Cells[newrow, 21].Value = AlarmCount.Text;
                    worksheet.Cells[newrow, 22].Value = AchievingRate.Text;
                    worksheet.Cells[newrow, 23].Value = ProperRate.Text;
                    worksheet.Cells[newrow, 24].Value = ProperRate_AutoMation.Text;
                    worksheet.Cells[newrow, 25].Value = ProperRate_Zhuanpan.Text;
                    worksheet.Cells[newrow, 26].Value = ProperRate_Lingmindu.Text;
                    worksheet.Cells[newrow, 27].Value = ProperRate_Tiemoji.Text;
                    package.Save();
                }
                AddMessage("保存机台生产数据完成");
            }
            catch (Exception ex)
            {
                AddMessage(ex.Message);
            }
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
            UDPInit();
            Async.RunFuncAsync(PLCWork, null);
            LoadAlarmNames();
            LoadTimelyRain();
            AddMessage("加载完成");
            dispatcherTimer.Start();
        }
        private void LoadAlarmNames()
        {
            try
            {
                if (File.Exists(alarmExcelPath))
                {
                    FileInfo existingFile = new FileInfo(alarmExcelPath);
                    using (ExcelPackage package = new ExcelPackage(existingFile))
                    {
                        // get the first worksheet in the workbook
                        ExcelWorksheet worksheet = package.Workbook.Worksheets[1];
                        for (int i = 1; i <= worksheet.Dimension.End.Row; i++)
                        {
                            AlarmList.Add(worksheet.Cells["B" + i.ToString()].Value.ToString());
                        }
                    }
                }
                else
                {
                    AddMessage("D4X报警.xlsx 文件不存在");
                }
            }
            catch (Exception ex)
            {
                AddMessage(ex.Message);
            }
        }
        private void LoadTimelyRain()
        {
            try
            {
                downtime = double.Parse(Inifile.INIGetStringValue(iniTimelyRainPath, "TimelyRain", "Downtime", "0"));
            }
            catch
            {
                downtime = 0;
            }
            try
            {
                zhuanpandowntime = double.Parse(Inifile.INIGetStringValue(iniTimelyRainPath, "TimelyRain", "Zhuanpandowntime", "0"));
            }
            catch
            {
                zhuanpandowntime = 0;
            }
            try
            {
                lingmindudowntime = double.Parse(Inifile.INIGetStringValue(iniTimelyRainPath, "Lingmindudowntime", "Downtime", "0"));
            }
            catch
            {
                lingmindudowntime = 0;
            }

            try
            {
                tiemojidowntime = double.Parse(Inifile.INIGetStringValue(iniTimelyRainPath, "TimelyRain", "Tiemojidowntime", "0"));
            }
            catch
            {
                tiemojidowntime = 0;
            }

            try
            {
                waitzhuanpanforinput = double.Parse(Inifile.INIGetStringValue(iniTimelyRainPath, "TimelyRain", "Waitzhuanpanforinput", "0"));
            }
            catch
            {
                waitzhuanpanforinput = 0;
            }

            try
            {
                waitlingminduforinput = double.Parse(Inifile.INIGetStringValue(iniTimelyRainPath, "TimelyRain", "Waitlingminduforinput", "0"));
            }
            catch
            {
                waitlingminduforinput = 0;
            }

            try
            {
                waitTiemojiforinput = double.Parse(Inifile.INIGetStringValue(iniTimelyRainPath, "TimelyRain", "WaitTiemojiforinput", "0"));
            }
            catch
            {
                waitTiemojiforinput = 0;
            }
            try
            {
                waitfortake = double.Parse(Inifile.INIGetStringValue(iniTimelyRainPath, "TimelyRain", "Waitfortake", "0"));
            }
            catch
            {
                waitfortake = 0;
            }
            try
            {
                worktime = double.Parse(Inifile.INIGetStringValue(iniTimelyRainPath, "TimelyRain", "Worktime", "0"));
            }
            catch
            {
                worktime = 0;
            }
            try
            {
                runtime = double.Parse(Inifile.INIGetStringValue(iniTimelyRainPath, "TimelyRain", "Runtime", "0"));
            }
            catch
            {
                runtime = 0;
            }
        }
        private void LoadParameter()
        {
            DangbanFirstProduct = Inifile.INIGetStringValue(iniParameterPath, "Summary", "DangbanFirstProduct", "null");
            LastBanci = Inifile.INIGetStringValue(iniParameterPath, "Summary", "LastBanci", "null");
            try
            {
                UPH = Double.Parse(Inifile.INIGetStringValue(iniParameterPath, "Summary", "UPH", "300"));
            }
            catch
            {
                UPH = 300;
            }
        }
        private string GetBanci()
        {
            string rs = "";
            if (DateTime.Now.Hour >= 8 && DateTime.Now.Hour < 20)
            {
                rs += DateTime.Now.ToString("yyyyMMdd") + "Day";
            }
            else
            {
                if (DateTime.Now.Hour >= 0 && DateTime.Now.Hour < 8)
                {
                    rs += DateTime.Now.AddDays(-1).ToString("yyyyMMdd") + "Night";
                }
                else
                {
                    rs += DateTime.Now.ToString("yyyyMMdd") + "Night";
                }
            }
            return rs;
        }
        #endregion
        #region PLC
        ThingetPLC Xinjie = new ThingetPLC();
        bool plcstate = false;
        ObservableCollection<bool> M10000;
        ObservableCollection<double> HD200;
        double D1200;
        public void PLCWork()
        {
            string COM = Inifile.INIGetStringValue(iniParameterPath, "PLC", "COM", "COM12");
            while (true)
            {
                System.Threading.Thread.Sleep(10);
                plcstate = Xinjie.ReadSM(0);
                if (plcstate)
                {
                    M10000 = Xinjie.ReadMultiMCoil(10000);//读160个M
                    HD200 = Xinjie.readMultiHD(200);//读30个双字（32位）
                    D1200 = Xinjie.ReadW(1200);//读1个字（16位）
                }
                else
                {
                    System.Threading.Thread.Sleep(1000);
                    Xinjie.ModbusDisConnect();
                    Xinjie.ModbusInit(COM, 19200, System.IO.Ports.Parity.Even, 8, System.IO.Ports.StopBits.One);
                    Xinjie.ModbusConnect();
                }
            }
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
            
            await udp1.SendAsync("test");
            AddMessage(await udp1.ReceiveAsync());
           
        }
    }
}
