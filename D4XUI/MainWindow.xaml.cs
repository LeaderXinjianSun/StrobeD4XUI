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
using System.Diagnostics;
using BingLibrary.hjb;
using 臻鼎科技OraDB;
using System.Data;
using MySql.Data.MySqlClient;
using System.Net;

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
        private string iniTimelyRainPath = System.Environment.CurrentDirectory + "\\TimelyRain.ini";
        string alarmExcelPath = System.Environment.CurrentDirectory + "\\D4X报警.xlsx";
        string alarmExcelPath1 = System.Environment.CurrentDirectory + "\\D4X条码.xlsx";
        List<AlarmData> AlarmList = new List<AlarmData>();
        string CurrentAlarmStr = "";
        string DangbanFirstProduct = "";
        string LastBanci = "";
        int timetick = 0;
        DateTime LasSam, NowSam;
        public static SampleWindow SampleWindow = null;
        double Yield = 0, _efficiency = 0, _variation = 0;
        #endregion
        public MainWindow()
        {
            InitializeComponent();

            dispatcherTimer.Tick += new EventHandler(DispatcherTimerTickUpdateUi);
            dispatcherTimer.Interval = new TimeSpan(0, 0, 0, 0, 100);//0.1s

        }
        private void MsgTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            MsgTextBox.ScrollToEnd();
        }
        private void DispatcherTimerTickUpdateUi(Object sender, EventArgs e)
        {
            timetick++;
            MsgTextBox.Text = MessageStr;
            PLCStatusEllipse.Fill = plcstate ? Brushes.Green : Brushes.Red;
            #region 大数据
            if (M10000 != null && plcstate)
            {
                for (int i = 0; i < AlarmList.Count; i++)
                {
                    if (M10000[i] != AlarmList[i].State)
                    {
                        AlarmList[i].State = M10000[i];
                        if (AlarmList[i].State)
                        {
                            if (CurrentAlarmStr != AlarmList[i].Content)
                            {
                                CurrentAlarmStr = AlarmList[i].Content;
                                AlarmList[i].Start = DateTime.Now;
                                AddMessage(AlarmList[i].Code + AlarmList[i].Content + "发生");
                                string _ip = GetIp();
                                string _class = DateTime.Now.Hour >= 8 && DateTime.Now.Hour < 20 ? "D" : "N";
                                string _faulttime = "0";
                                BigDataInsert(_ip, 治具编号.Text, 线体.Text, 测试料号.Text, _class, AlarmList[i].Content, AlarmList[i].Start.ToString(), _faulttime);
                            }
                        }
                        else
                        {
                            AlarmList[i].End = DateTime.Now;
                            AddMessage(AlarmList[i].Code + AlarmList[i].Content + "解除");
                            string _ip = GetIp();
                            string _class = DateTime.Now.Hour >= 8 && DateTime.Now.Hour < 20 ? "D" : "N";
                            string _faulttime = (AlarmList[i].End - AlarmList[i].Start).TotalMinutes.ToString("F0");
                            BigDataUpdate(_ip, AlarmList[i].Content, AlarmList[i].Start.ToString(), _class, _faulttime);
                        }
                    }
                }

            }
            #endregion
            #region 数据统计
            if (timetick > 10)
            {
                timetick = 0;

                if (HD200 != null && plcstate)
                {
                    #region 总直通率
                    if (HD200[0] == 0)
                    {
                        Yield = 0;
                    }
                    else
                    {
                        Yield = HD200[3] / HD200[0] * 100;
                    }
                    //总直通率
                    #endregion
                    #region 工作效率
                    DateTime _StartTime;
                    if (DateTime.Now.Hour >= 8 && DateTime.Now.Hour < 20)
                    {
                        _StartTime = Convert.ToDateTime("08:00:00");
                    }
                    else
                    {
                        if (DateTime.Now.Hour < 8)
                        {
                            _StartTime = Convert.ToDateTime("20:00:00").AddDays(-1);
                        }
                        else
                        {
                            _StartTime = Convert.ToDateTime("20:00:00");
                        }
                    }
                    double _totalmin = (DateTime.Now - _StartTime).TotalMinutes;
                    double _workmin = _totalmin - HD200[10] - HD200[11] - HD200[12] - HD200[13];
                    _efficiency = HD200[3] / _workmin / HD200[4] * 60;
                    //工作效率
                    #endregion
                    #region 影响比例
                    _variation = (HD200[10] + HD200[11] + HD200[12] + HD200[13]) / 10 / _totalmin;
                    //影响比例
                    #endregion
                }
            }
            #endregion
            #region 样本
            DateTime SamStartDatetime, SamDate, SamDateBigin;
            if (DateTime.Now.Hour >= 6 && DateTime.Now.Hour < 12)
            {
                //上午
                SamStartDatetime = Convert.ToDateTime("08:00:00");
                SamDate = Convert.ToDateTime("07:00:00");
                SamDateBigin = Convert.ToDateTime("06:00:00");
            }
            else
            {
                if (DateTime.Now.Hour >= 12 && DateTime.Now.Hour < 18)
                {
                    //下午
                    SamStartDatetime = Convert.ToDateTime("14:00:00");
                    SamDate = Convert.ToDateTime("13:00:00");
                    SamDateBigin = Convert.ToDateTime("12:00:00");
                }
                else
                {
                    if (DateTime.Now.Hour >= 18)
                    {
                        //前夜
                        SamStartDatetime = Convert.ToDateTime("20:00:00");
                        SamDate = Convert.ToDateTime("19:00:00");
                        SamDateBigin = Convert.ToDateTime("18:00:00");
                    }
                    else
                    {
                        //后夜
                        SamStartDatetime = Convert.ToDateTime("02:00:00");
                        SamDate = Convert.ToDateTime("01:00:00");
                        SamDateBigin = Convert.ToDateTime("00:00:00");
                    }
                }
            }
            if (M10000 != null && plcstate)
            {
                IsInSampleMode = M10000[110];
                SampleTestAbort = M10000[111];
                SampleTestFinished = M10000[112];
                SampleTestStart = M10000[113];
                if (IsInSampleMode && SampleTestAbort)
                {
                    AddMessage("样本测试中断");
                    Xinjie.SetM(11110, false);
                    IsInSampleMode = false;
                    SampleBarcode.Clear();
                }
                SampleGrid.Visibility = (DateTime.Now - SamDate).TotalSeconds > 0 && (SamDateBigin - LasSam).TotalSeconds > 0 && IsSample || (IsInSampleMode && !SampleTestAbort) ? Visibility.Visible : Visibility.Collapsed;
                SampleTextBlock.Text = IsInSampleMode ? "样本测试中" : "请测样本";
                if (!SampleTestAbort && !IsInSampleMode && (DateTime.Now - SamStartDatetime).TotalSeconds > 0 && IsSample && (SamDateBigin - LasSam).TotalSeconds > 0)
                {
                    Xinjie.SetM(11110, true);
                    Xinjie.SetM(11112, false);
                    SampleTestFinished = false;
                    SampleBarcode.Clear();
                    NowSam = DateTime.Now;
                    AddMessage("开始样本测试");

                }
                if (IsInSampleMode && SampleTestFinished)
                {
                    bool res = CheckSampleFromDt();
                    Xinjie.SetM(11114, !res);
                    Xinjie.SetM(11110, false);
                    if (res)
                    {
                        AddMessage("样本测试成功");
                        LasSam = DateTime.Now;
                        LastSampleTime.Text = LasSam.ToString();
                        Inifile.INIWriteValue(iniParameterPath, "Sample", "LastSample", LasSam.ToString());
                    }
                    else
                    {
                        NowSam = DateTime.Now;
                        AddMessage("样本测试失败");
                    }
                    Xinjie.SetM(11115, true);
                }
            }
            
            #endregion           
            #region 换班
            if (LastBanci != GetBanci())
            {
                LastBanci = GetBanci();
                Inifile.INIWriteValue(iniParameterPath, "Summary", "LastBanci", LastBanci);
                WriteMachineData();
                AddMessage(LastBanci + " 换班数据清零");
                Xinjie.SetM(11099, true);//通知PLC换班，计数清空
            }
            #endregion            
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
            ConnectDBTest();
            UDPInit();
            LoadParameter();
            Async.RunFuncAsync(PLCWork, null);

            LoadAlarmNames();
            AddMessage("加载完成");
            dispatcherTimer.Start();
            UDPWork();
            UDPWorkVPP();
            NowSam = DateTime.Now;
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
                            AlarmData ad = new AlarmData();
                            ad.Code = worksheet.Cells["A" + i.ToString()].Value == null ? "Null" : worksheet.Cells["A" + i.ToString()].Value.ToString();
                            ad.Content = worksheet.Cells["B" + i.ToString()].Value == null ? "Null" : worksheet.Cells["B" + i.ToString()].Value.ToString();
                            ad.Start = DateTime.Now;
                            ad.End = DateTime.Now;
                            ad.State = false;
                            AlarmList.Add(ad);
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
                        worksheet.Cells[1, 2].Value = "上料总数";
                        worksheet.Cells[1, 3].Value = "下料总数";
                        worksheet.Cells[1, 4].Value = "抛料数";
                        worksheet.Cells[1, 5].Value = "良品数";
                        worksheet.Cells[1, 6].Value = "UPH";
                        worksheet.Cells[1, 7].Value = "掉料数";
                        worksheet.Cells[1, 8].Value = "测试总数";
                        worksheet.Cells[1, 9].Value = "待料时间";
                        worksheet.Cells[1, 10].Value = "下空盘时间";
                        worksheet.Cells[1, 11].Value = "换胶带时间";
                        worksheet.Cells[1, 12].Value = "样本时间";
                        worksheet.Cells[1, 13].Value = "转盘信号超时时间";
                        worksheet.Cells[1, 14].Value = "灵敏度信号超时时间";
                        worksheet.Cells[1, 15].Value = "贴膜机信号超时时间";
                        worksheet.Cells[1, 16].Value = "转盘信号超时次数";
                        worksheet.Cells[1, 17].Value = "灵敏度信号超时次数";
                        worksheet.Cells[1, 18].Value = "贴膜机信号次数";
                        package.Save();
                    }
                }


                using (ExcelPackage package = new ExcelPackage(fileInfo))
                {
                    ExcelWorksheet worksheet = package.Workbook.Worksheets[1];
                    int newrow = worksheet.Dimension.End.Row + 1;
                    worksheet.Cells[newrow, 1].Value = System.DateTime.Now.ToString();
                    if (plcstate && HD200 != null)
                    {
                        worksheet.Cells[newrow, 2].Value = HD200[0];
                        worksheet.Cells[newrow, 3].Value = HD200[1];
                        worksheet.Cells[newrow, 4].Value = HD200[2];
                        worksheet.Cells[newrow, 5].Value = HD200[3];
                        worksheet.Cells[newrow, 6].Value = HD200[4];
                        worksheet.Cells[newrow, 7].Value = HD200[5];
                        worksheet.Cells[newrow, 8].Value = HD200[6];

                        worksheet.Cells[newrow, 9].Value = HD200[10] / 10;
                        worksheet.Cells[newrow, 10].Value = HD200[11] / 10;
                        worksheet.Cells[newrow, 11].Value = HD200[12] / 10;
                        worksheet.Cells[newrow, 12].Value = HD200[13] / 10;
                        worksheet.Cells[newrow, 13].Value = HD200[14] / 10;
                        worksheet.Cells[newrow, 14].Value = HD200[15] / 10;
                        worksheet.Cells[newrow, 15].Value = HD200[16] / 10;

                        worksheet.Cells[newrow, 16].Value = HD200[17];
                        worksheet.Cells[newrow, 17].Value = HD200[18];
                        worksheet.Cells[newrow, 18].Value = HD200[19];
                    }

                    package.Save();
                }
                AddMessage("保存机台生产数据完成");
            }
            catch (Exception ex)
            {
                AddMessage(ex.Message);
            }
        }

        private void LoadParameter()
        {
            治具编号.Text = Inifile.INIGetStringValue(iniParameterPath, "System", "治具编号", "null");
            线体.Text = Inifile.INIGetStringValue(iniParameterPath, "System", "线体", "null");
            测试料号.Text = Inifile.INIGetStringValue(iniParameterPath, "System", "测试料号", "null");
            DangbanFirstProduct = Inifile.INIGetStringValue(iniParameterPath, "Summary", "DangbanFirstProduct", "null");
            LastBanci = Inifile.INIGetStringValue(iniParameterPath, "Summary", "LastBanci", "null");
            ZhuanpanBarcode1.Text = Inifile.INIGetStringValue(iniParameterPath, "Barcode", "ZhuanpanBarcode1", "null");
            ZhuanpanBarcode2.Text = Inifile.INIGetStringValue(iniParameterPath, "Barcode", "ZhuanpanBarcode2", "null");
            SimoBarcode1.Text = Inifile.INIGetStringValue(iniParameterPath, "Barcode", "SimoBarcode1", "null");
            SimoBarcode2.Text = Inifile.INIGetStringValue(iniParameterPath, "Barcode", "SimoBarcode2", "null");
            LingminduBarcode1.Text = Inifile.INIGetStringValue(iniParameterPath, "Barcode", "LingminduBarcode1", "null");
            LingminduBarcode2.Text = Inifile.INIGetStringValue(iniParameterPath, "Barcode", "LingminduBarcode2", "null");
            LastSampleTime.Text = Inifile.INIGetStringValue(iniParameterPath, "Sample", "LastSample", "2019/1/1 00:00:00");
            try
            {
                LasSam = Convert.ToDateTime(LastSampleTime.Text);
            }
            catch
            {
                LastSampleTime.Text = Inifile.INIGetStringValue(iniParameterPath, "Sample", "LastSample", "2019/1/1 00:00:00");
            }

            string iniSamplePath = System.Environment.CurrentDirectory + "\\Sample.ini";
            try
            {
                IsSample = bool.Parse(Inifile.INIGetStringValue(iniSamplePath, "Sample", "IsSample", "True"));
            }
            catch
            {
                IsSample = true;
            }
            try
            {
                NGItemCount = int.Parse(Inifile.INIGetStringValue(iniSamplePath, "Sample", "NGItemCount", "9"));
            }
            catch
            {
                NGItemCount = 9;
            }
            for (int i = 0; i < 10; i++)
            {
                NGItems[i, 0] = Inifile.INIGetStringValue(iniSamplePath, "Sample", "NGItem" + (i + 1).ToString(), "Null");
                NGItems[i, 1] = Inifile.INIGetStringValue(iniSamplePath, "Sample", "NGItemClassify" + (i + 1).ToString(), "Null");
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
        bool M10140 = false, M10141 = false, M10142 = false, M10150 = false, M10151 = false, M10152 = false, M10153 = false, M10154 = false;



        private void ManulSampleButton_Click(object sender, RoutedEventArgs e)
        {
            if (!SampleTestAbort && !IsInSampleMode && IsSample && plcstate)
            {
                Xinjie.SetM(11110, true);
                Xinjie.SetM(11112, false);
                SampleTestFinished = false;
                SampleBarcode.Clear();
                NowSam = DateTime.Now;
                AddMessage("开始样本测试");
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (治具编号.IsReadOnly)
            {
                治具编号.IsReadOnly = false;
                线体.IsReadOnly = false;
                测试料号.IsReadOnly = false;
                SaveButton.Content = "Save";
            }
            else
            {
                if (治具编号.Text == "")
                {
                    治具编号.Text = "null";                    
                }
                Inifile.INIWriteValue(iniParameterPath, "System", "治具编号", 治具编号.Text);
                if (线体.Text == "")
                {
                    线体.Text = "null";
                }
                Inifile.INIWriteValue(iniParameterPath, "System", "线体", 线体.Text);
                if (测试料号.Text == "")
                {
                    测试料号.Text = "null";
                }
                Inifile.INIWriteValue(iniParameterPath, "System", "测试料号", 测试料号.Text);
                治具编号.IsReadOnly = true;
                线体.IsReadOnly = true;
                测试料号.IsReadOnly = true;
                SaveButton.Content = "Edit";
            }
        }

        private void FuncButton_Click(object sender, RoutedEventArgs e)
        {
            //SampleBarcode.Clear();
            //SampleBarcode.Add("G5Y796383C9LQ5919SAT");
            //SampleBarcode.Add("G5Y9321RAH5K7QC8V-G");
            //NowSam = Convert.ToDateTime("2019/8/16 18:45:16");
            //var a = CheckSampleFromDt();
            //if (!SampleTestAbort && !IsInSampleMode && IsSample && plcstate)
            //{
            //    Xinjie.SetM(11110, true);
            //    Xinjie.SetM(11112, false);
            //    SampleTestFinished = false;
            //    SampleBarcode.Clear();
            //    NowSam = DateTime.Now;
            //    AddMessage("开始样本测试");
            //}
        }

        double D1200;

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
        }


        public void PLCWork()
        {
            string COM = Inifile.INIGetStringValue(iniParameterPath, "PLC", "COM", "COM19");
            Random rd = new Random();
            while (true)
            {
                System.Threading.Thread.Sleep(50);
                plcstate = Xinjie.ReadSM(0);
                if (plcstate)
                {
                    M10000 = Xinjie.ReadMultiMCoil(11000);//读160个M
                    HD200 = Xinjie.readMultiHD(200);//读30个双字（32位）
                    D1200 = Xinjie.ReadW(1200);//读1个字（16位）
                    Xinjie.WriteW(1201, rd.Next(0, 99).ToString());
                    Xinjie.WriteW(400, (Yield * 10).ToString("F0"));
                    Xinjie.WriteW(403, (_efficiency * 100).ToString("F0"));
                    Xinjie.WriteW(404, (_variation * 1000).ToString("F0"));
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
        UDPClient udp2 = new UDPClient();
        void UDPInit()
        {
            string ip;
            int localport, targetport;
            ip = Inifile.INIGetStringValue(iniParameterPath, "转盘", "IP", "192.168.0.1");
            localport = int.Parse(Inifile.INIGetStringValue(iniParameterPath, "转盘", "LocalPort", "8001"));
            targetport = int.Parse(Inifile.INIGetStringValue(iniParameterPath, "转盘", "TargetPort", "5000"));
            udp1.Connect(localport, targetport, ip);
            ip = Inifile.INIGetStringValue(iniParameterPath, "灵敏度", "IP", "192.168.0.10");
            localport = int.Parse(Inifile.INIGetStringValue(iniParameterPath, "灵敏度", "LocalPort", "8002"));
            targetport = int.Parse(Inifile.INIGetStringValue(iniParameterPath, "灵敏度", "TargetPort", "5000"));
            udp2.Connect(localport, targetport, ip);

        }
        async void UDPWork()
        {
            
            
            while (true)
            {
                
                string rs = await udp1.ReceiveAsync();

                #region 从转盘接收条码
                if (rs != "error")
                {
                    RunLog("从转盘接收 " + rs);
                    AddMessage("从转盘接收 " + rs);
                    if (plcstate)
                    {
                        Xinjie.SetM(11148, true);
                    }
                    

                    string sends = "SNOK";
                    await udp1.SendAsync(sends);
                    AddMessage("向转盘发送 " + sends);
                    try
                    {
                        //SN1:G5Y9301RDD0K9037V-GF,P;SN2:G5Y9301RDCNK9037A-GF,P
                        //SN1:,;SN2:G5Y930432L2L65K5M-GF,P;49
                        string[] s1 = rs.Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries);
                        string[] s1_1 = s1[0].Split(new string[] { ":" }, StringSplitOptions.RemoveEmptyEntries);
                        if (s1_1[0] == "SN1" && s1_1.Length == 2)
                        {
                            try
                            {
                                string[] s1_1_1 = s1_1[1].Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
                                if (s1_1_1.Length >= 2)
                                {
                                    ZhuanpanBarcode1.Text = s1_1_1[0];
                                    if (SampleTestStart)
                                    {
                                        SampleBarcode.Add(s1_1_1[0]);
                                    }
                                    Inifile.INIWriteValue(iniParameterPath, "Barcode", "ZhuanpanBarcode1", ZhuanpanBarcode1.Text);

                                    if (s1_1_1[1] == "P")
                                    {
                                        ZhuanpanBarcode1.Background = Brushes.GreenYellow;
                                    }
                                    else
                                    {
                                        ZhuanpanBarcode1.Background = Brushes.Red;
                                    }
                                }
                                else
                                {
                                    ZhuanpanBarcode1.Text = "null";
                                    Inifile.INIWriteValue(iniParameterPath, "Barcode", "ZhuanpanBarcode1", ZhuanpanBarcode1.Text);
                                    ZhuanpanBarcode1.Background = Brushes.Gray;
                                }
                            }
                            catch (Exception ex)
                            {
                                AddMessage(ex.Message);

                            }
                            
                        }
                        string[] s1_2 = s1[1].Split(new string[] { ":" }, StringSplitOptions.RemoveEmptyEntries);
                        if (s1_2[0] == "SN2" && s1_2.Length == 2)
                        {
                            try
                            {
                                string[] s1_2_1 = s1_2[1].Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
                                if (s1_2_1.Length >= 2)
                                {
                                    ZhuanpanBarcode2.Text = s1_2_1[0];
                                    if (SampleTestStart)
                                    {
                                        SampleBarcode.Add(s1_2_1[0]);
                                    }
                                    Inifile.INIWriteValue(iniParameterPath, "Barcode", "ZhuanpanBarcode2", ZhuanpanBarcode2.Text);

                            
                                    if (s1_2_1[1] == "P")
                                    {
                                        ZhuanpanBarcode2.Background = Brushes.GreenYellow;
                                    }
                                    else
                                    {
                                        ZhuanpanBarcode2.Background = Brushes.Red;
                                    }
                                 

                                }
                                else
                                {
                                    ZhuanpanBarcode2.Text = "null";
                                    Inifile.INIWriteValue(iniParameterPath, "Barcode", "ZhuanpanBarcode2", ZhuanpanBarcode2.Text);

                                    ZhuanpanBarcode2.Background = Brushes.Gray;
                                }

                            }
                            catch (Exception ex)
                            {

                                AddMessage(ex.Message);
                            }
                            

                        }
                    }
                    catch (Exception ex)
                    {

                        AddMessage(ex.Message);
                    }


                }
                #endregion        
               
            }

        }

        async void UDPWorkVPP()
        {
            Stopwatch sw = new Stopwatch();
            bool first = true;
            while (true)
            {
                sw.Restart();
                await Task.Delay(100);
                #region 读取PLC信号
                try
                {
                    if (plcstate)
                    {
                        if (first)
                        {
                            await Task.Delay(100);
                            first = false;
                            M10140 = M10000[140];//条码移动到吸爪
                            M10141 = M10000[141];//条码移动到灵敏度
                            M10142 = M10000[142];//向灵敏度补发条码
                            M10150 = M10000[150];//清空灵敏度条码
                            M10151 = M10000[151];//灵敏度1PASS
                            M10152 = M10000[152];//灵敏度1NG
                            M10153 = M10000[153];//灵敏度2PASS
                            M10154 = M10000[154];//灵敏度2NG
                        }

                        if (M10140 != M10000[140])
                        {
                            M10140 = M10000[140];
                            if (M10140)
                            {
                                AddMessage("条码从撕膜到吸爪 " + ZhuanpanBarcode1.Text + "," + ZhuanpanBarcode2.Text);
                                RunLog("条码从撕膜到吸爪 " + ZhuanpanBarcode1.Text + "," + ZhuanpanBarcode2.Text);
                                SimoBarcode1.Text = ZhuanpanBarcode1.Text;
                                SimoBarcode1.Background = ZhuanpanBarcode1.Background;
                                Inifile.INIWriteValue(iniParameterPath, "Barcode", "SimoBarcode1", SimoBarcode1.Text);
                                ZhuanpanBarcode1.Text = "null";
                                ZhuanpanBarcode1.Background = Brushes.White;
                                Inifile.INIWriteValue(iniParameterPath, "Barcode", "ZhuanpanBarcode1", ZhuanpanBarcode1.Text);
                                SimoBarcode2.Text = ZhuanpanBarcode2.Text;
                                SimoBarcode2.Background = ZhuanpanBarcode2.Background;
                                Inifile.INIWriteValue(iniParameterPath, "Barcode", "SimoBarcode2", SimoBarcode2.Text);
                                ZhuanpanBarcode2.Text = "null";
                                ZhuanpanBarcode2.Background = Brushes.White;
                                Inifile.INIWriteValue(iniParameterPath, "Barcode", "ZhuanpanBarcode2", ZhuanpanBarcode2.Text);
                            }
                        }
                        if (M10141 != M10000[141])
                        {
                            M10141 = M10000[141];
                            if (M10141)
                            {
                                AddMessage("条码从吸爪到灵敏度" + SimoBarcode1.Text + "," + SimoBarcode2.Text);
                                RunLog("条码从吸爪到灵敏度" + SimoBarcode1.Text + "," + SimoBarcode2.Text);
                                LingminduBarcode1.Text = SimoBarcode1.Text;
                                LingminduBarcode1.Background = SimoBarcode1.Background;
                                Inifile.INIWriteValue(iniParameterPath, "Barcode", "LingminduBarcode1", LingminduBarcode1.Text);
                                SimoBarcode1.Text = "null";
                                SimoBarcode1.Background = Brushes.White;
                                Inifile.INIWriteValue(iniParameterPath, "Barcode", "SimoBarcode1", SimoBarcode1.Text);
                                LingminduBarcode2.Text = SimoBarcode2.Text;
                                LingminduBarcode2.Background = SimoBarcode2.Background;
                                Inifile.INIWriteValue(iniParameterPath, "Barcode", "LingminduBarcode2", LingminduBarcode2.Text);
                                SimoBarcode2.Text = "null";
                                SimoBarcode2.Background = Brushes.White;
                                Inifile.INIWriteValue(iniParameterPath, "Barcode", "SimoBarcode2", SimoBarcode2.Text);
                                string sends = "SN1:" + LingminduBarcode1.Text + ",P" + ";" + "SN2:" + LingminduBarcode2.Text + ",P" + "\r\n";
                                await udp2.SendAsync(sends);
                                AddMessage("向灵敏度发送 " + sends);
                                RunLog("向灵敏度发送 " + sends);
                            }
                        }
                        if (M10142 != M10000[142])
                        {
                            M10142 = M10000[142];
                            if (M10142)
                            {
                                string sends = "SN1:" + LingminduBarcode1.Text + ",P" + ";" + "SN2:" + LingminduBarcode2.Text + ",P" + "\r\n";
                                await udp2.SendAsync(sends);
                                AddMessage("向灵敏度补发 " + sends);
                                RunLog("向灵敏度补发 " + sends);
                            }
                        }
                        if (M10150 != M10000[150])
                        {
                            M10150 = M10000[150];
                            if (M10150)
                            {
                                AddMessage("灵敏度清空条码" + LingminduBarcode1.Text + "," + LingminduBarcode2.Text);
                                RunLog("灵敏度清空条码" + LingminduBarcode1.Text + "," + LingminduBarcode2.Text);
                                LingminduBarcode1.Text = "null";
                                LingminduBarcode1.Background = Brushes.White;
                                Inifile.INIWriteValue(iniParameterPath, "Barcode", "LingminduBarcode1", LingminduBarcode1.Text);
                                LingminduBarcode2.Text = "null";
                                LingminduBarcode2.Background = Brushes.White;
                                Inifile.INIWriteValue(iniParameterPath, "Barcode", "LingminduBarcode2", LingminduBarcode2.Text);
                            }
                        }
                        if (M10151 != M10000[151])
                        {
                            M10151 = M10000[151];
                            if (M10151)
                            {
                                LingminduJieGuo1.Background = Brushes.LightGreen;
                                SaveResult(LingminduBarcode1.Text, "OK", "1");
                            }
                            else
                            {
                                LingminduJieGuo1.Background = Brushes.Gray;
                            }
                        }
                        if (M10152 != M10000[152])
                        {
                            M10152 = M10000[152];
                            if (M10152)
                            {
                                LingminduJieGuo1.Background = Brushes.Red;
                                SaveResult(LingminduBarcode1.Text, "NG", "1");
                            }
                            else
                            {
                                LingminduJieGuo1.Background = Brushes.Gray;
                            }
                        }
                        if (M10153 != M10000[153])
                        {
                            M10153 = M10000[153];
                            if (M10153)
                            {
                                LingminduJieGuo2.Background = Brushes.LightGreen;
                                SaveResult(LingminduBarcode2.Text, "OK", "2");
                            }
                            else
                            {
                                LingminduJieGuo2.Background = Brushes.Gray;
                            }
                        }
                        if (M10154 != M10000[154])
                        {
                            M10154 = M10000[154];
                            if (M10154)
                            {
                                LingminduJieGuo2.Background = Brushes.Red;
                                SaveResult(LingminduBarcode2.Text, "NG", "2");
                            }
                            else
                            {
                                LingminduJieGuo2.Background = Brushes.Gray;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {

                    AddMessage(ex.Message);
                }
                
                CycleText.Text = sw.ElapsedMilliseconds.ToString() + "ms";
                #endregion
            }
        }
        #endregion
        #region 数据库
        private void ConnectDBTest()
        {
            try
            {
                OraDB oraDB = new OraDB("zdtdb", "ictdata", "ictdata*168");
                if (oraDB.isConnect())
                {
                    string dbtime = oraDB.sfc_getServerDateTime();
                    setLocalTime(dbtime);
                    AddMessage("数据库连接" + dbtime);
                }
                else
                {
                    AddMessage("数据库未连接");
                }
                oraDB.disconnect();
            }
            catch (Exception ex)
            {
                AddMessage(ex.Message);
            }
        }
        private void setLocalTime(string strDateTime)
        {
            DateTimeUtility.SYSTEMTIME st = new DateTimeUtility.SYSTEMTIME();
            DateTime dt = Convert.ToDateTime(strDateTime);
            st.FromDateTime(dt);
            DateTimeUtility.SetLocalTime(ref st);
        }
        #endregion
        #region 样本
        bool IsSample, IsInSampleMode = false,SampleTestAbort = false,SampleTestStart = false,SampleTestFinished = false;

        private async void AlarmButton_Click(object sender, RoutedEventArgs e)
        {
            AlarmButton.IsEnabled = false;
            await Task.Run(()=> {
                try
                {
                    if (!Directory.Exists("C:\\Debug\\" + DateTime.Now.ToString("yyyyMMdd")))
                    {
                        Directory.CreateDirectory(@"C:\\Debug\\" + DateTime.Now.ToString("yyyyMMdd"));
                    }
                    string path = "C:\\Debug\\" + DateTime.Now.ToString("yyyyMMdd") + "\\" + DateTime.Now.ToString("yyyyMMddHHmmss") + "AlarmSimple.csv";
                    Csvfile.savetocsv(path, new string[] { "Content", "Count", "Time(min)" });
                    string _class = DateTime.Now.Hour >= 8 && DateTime.Now.Hour < 20 ? "D" : "N";
                    string _ip = GetIp();
                    string _date;
                    if (DateTime.Now.Hour < 8)
                    {
                        _date = DateTime.Now.AddDays(-1).ToString("yyyyMMdd");
                    }
                    else
                    {
                        _date = DateTime.Now.ToString("yyyyMMdd");
                    }
                    
                    int alarmcount = 0; float alarmelapsed = 0;
                    foreach (var item in AlarmList)
                    {
                        MySqlConnection conn = null;
                        string StrMySQL = "Server=10.89.164.62;Database=dcdb;Uid=dcu;Pwd=dcudata;pooling=false;CharSet=utf8;port=3306";
                        conn = new MySqlConnection(StrMySQL);
                        conn.Open();
                        string stm;

                            stm = "SELECT * FROM TED_FAULT_DATA WHERE COMPUTERIP ='" + _ip + "' AND FAULTID = '" + item.Content +
        "' AND TDATE = '" + _date + "' AND CLASS = '" + _class + "' AND FL01 = '" + "OFF'";
                     

                        MySqlCommand cmd = new MySqlCommand(stm, conn);
                        MySqlDataReader rdr = cmd.ExecuteReader();
                        int i = 0;
                        float elapsed = 0;
                        while (rdr.Read())
                        {
                            try
                            {
                                elapsed += float.Parse(rdr.GetString("FAULTTIME"));
                            }
                            catch { }
                            i++;
                        }
                        if (i > 0)
                        {
                            alarmcount += i;
                            alarmelapsed += elapsed;
                            Csvfile.savetocsv(path, new string[] { item.Content, i.ToString(), elapsed.ToString("F1") });
                        }
                        conn.Close();
                        conn.Dispose();
                    }
                    if (plcstate)
                    {
                        Xinjie.WriteW(401, alarmcount.ToString());
                        Xinjie.WriteW(402, (alarmelapsed * 10).ToString("F0"));
                    }
                    Process process1 = new Process();
                    process1.StartInfo.FileName = path;
                    process1.StartInfo.Arguments = "";
                    process1.StartInfo.WindowStyle = ProcessWindowStyle.Maximized;
                    process1.Start();
                }
                catch (Exception ex)
                {
                    AddMessage(ex.Message);
                }
            });
            await Task.Run(() => {
                try
                {
                    string path = "C:\\Debug\\" + DateTime.Now.ToString("yyyyMMdd") + "\\" + DateTime.Now.ToString("yyyyMMddHHmmss") + "AlarmTotal.csv";
                    string _class = DateTime.Now.Hour >= 8 && DateTime.Now.Hour < 20 ? "D" : "N";
                    string _ip = GetIp();
                    string _date = DateTime.Now.ToString("yyyyMMdd");
                    MySqlConnection conn = null;
                    string StrMySQL = "Server=10.89.164.62;Database=dcdb;Uid=dcu;Pwd=dcudata;pooling=false;CharSet=utf8;port=3306";
                    conn = new MySqlConnection(StrMySQL);
                    conn.Open();
                    string stm;
                    if (DateTime.Now.Hour > 8)
                    {
                        stm = "SELECT * FROM TED_FAULT_DATA WHERE COMPUTERIP ='" + _ip +
                                "' AND TDATE = '" + _date + "' AND CLASS = '" + _class + "' AND FL01 = '" + "OFF'";
                    }
                    else
                    {
                        string _date1 = DateTime.Now.AddDays(-1).ToString("yyyyMMdd");
                        stm = "SELECT * FROM TED_FAULT_DATA WHERE COMPUTERIP ='" + _ip +
                                "' AND TDATE IN ('" + _date + "','" + _date1 + "') AND CLASS = '" + _class + "'AND FL01 = '" + "OFF'";
                    }
                    DataSet ds = new DataSet();
                    MySqlDataAdapter myadp = new MySqlDataAdapter(stm, conn); //适配器 
                    myadp.Fill(ds, "table0");
                    conn.Close();
                    conn.Dispose();
                    DataTable dt = ds.Tables["table0"];
                    if (dt.Rows.Count > 0)
                    {
                        string strHead = DateTime.Now.ToString("yyyyMMddHHmmss") + "AlarmTotal";
                        string strColumns = "";
                        for (int i = 0; i < dt.Columns.Count; i++)
                        {
                            strColumns += dt.Columns[i].ColumnName + ",";
                        }
                        strColumns = strColumns.Substring(0, strColumns.Length - 1);
                        Csvfile.dt2csv(dt, path, strHead, strColumns);

                        Process process1 = new Process();
                        process1.StartInfo.FileName = path;
                        process1.StartInfo.Arguments = "";
                        process1.StartInfo.WindowStyle = ProcessWindowStyle.Maximized;
                        process1.Start();
                    }
                }
                catch (Exception ex)
                {
                    AddMessage(ex.Message);
                }
            });
            AlarmButton.IsEnabled = true;

        }

        int NGItemCount; string[,] NGItems = new string[10,2];
        List<string> SampleBarcode = new List<string>();
        private void SampleButton_Click(object sender, RoutedEventArgs e)
        {
            if (SampleWindow != null)
            {
                if (SampleWindow.HasShow)
                    return;
            }
            SampleWindow = new SampleWindow();
            SampleWindow.Owner = Application.Current.MainWindow;
            SampleWindow.HasShow = true;
            SampleWindow.Show();

        }
        private bool CheckSampleFromDt()
        {
            //条码、时间=>表格
            //不良项目数量是否够？
            try
            {
                if (SampleBarcode.Count > 0)
                {
                    OraDB oraDB = new OraDB("zdtdb", "ictdata", "ictdata*168");
                    if (oraDB.isConnect())
                    {
                        //select* from barsamrec where barcode in ('G5Y796383C9LQ5919SAT','G5Y9321RAH5K7QC8V-G') and sdate > to_date('2019/8/16 18:45:16', 'yyyy/mm/dd hh24:mi:ss')
                        string selectSqlStr = "select * from barsamrec where barcode in （";
                        foreach (var item in SampleBarcode)
                        {
                            AddMessage(item);
                            selectSqlStr += "'" + item + "',";
                        }
                        selectSqlStr = selectSqlStr.Substring(0, selectSqlStr.Length - 1);
                        selectSqlStr += ") and sdate > to_date('" + NowSam.ToString() + "', 'yyyy/mm/dd hh24:mi:ss')";
                        DataSet s = oraDB.selectSQL2(selectSqlStr);
                        DataTable dt = s.Tables[0];
                        string Columns = "";
                        for (int i = 0; i < dt.Columns.Count - 1; i++)
                        {
                            Columns += dt.Columns[i].ColumnName + ",";
                        }
                        Columns += dt.Columns[dt.Columns.Count - 1].ColumnName;
                        Csvfile.dt2csv(dt, "C:\\Debug\\" + DateTime.Now.ToString("yyyyMMdd") + "\\" + DateTime.Now.ToString("yyyyMMddHHmmss") + "Sample.csv", "Sample", Columns);

                        try
                        {
                            Process process1 = new Process();
                            process1.StartInfo.FileName = "C:\\Debug\\" + DateTime.Now.ToString("yyyyMMdd") + "\\" + DateTime.Now.ToString("yyyyMMddHHmmss") + "Sample.csv";
                            process1.StartInfo.Arguments = "";
                            process1.StartInfo.WindowStyle = ProcessWindowStyle.Maximized;
                            process1.Start();
                        }
                        catch (Exception ex)
                        {
                            AddMessage(ex.Message);
                        }

                        //匹配不良项数量是否满足
                        int[] counts = new int[NGItemCount];
                        for (int i = 0; i < NGItemCount; i++)
                        {
                            for (int j = 0; j < dt.Rows.Count; j++)
                            {
                                if (((string)dt.Rows[j]["NGITEM"]).Contains(NGItems[i, 0]) && NGItems[i, 1] == (string)dt.Rows[j]["SITEM"])
                                {
                                    counts[i]++;
                                }
                            }
                        }
                        for (int i = 0; i < NGItemCount; i++)
                        {
                            if (counts[i] <= 0)
                            {
                                AddMessage("样本测试数量不足");
                                return false;
                            }
                        }
                        //匹配是否测试正确
                        for (int j = 0; j < dt.Rows.Count; j++)
                        {
                            if ((string)dt.Rows[j]["TRES"] != (string)dt.Rows[j]["NGITEM"])
                            {
                                AddMessage((string)dt.Rows[j]["BARCODE"] + "应该是" + (string)dt.Rows[j]["NGITEM"] + ",却测成了" + (string)dt.Rows[j]["TRES"]);
                            }
                        }
                        for (int j = 0; j < dt.Rows.Count; j++)
                        {
                            if ((string)dt.Rows[j]["TRES"] != (string)dt.Rows[j]["NGITEM"])
                            {
                                return false;
                            }
                        }
                        oraDB.disconnect();
                        return true;
                    }
                    else
                    {
                        AddMessage("数据库连接失败");
                        return false;
                    }
                }
                else
                {
                    AddMessage("条码数量为零");
                    return false;
                }
            }
            catch (Exception ex)
            {
                AddMessage(ex.Message);
                return false;
            }
            
        }
        #endregion
        #region 大数据上传
        /// <summary>
        /// 大数据上传
        /// </summary>
        /// <param name="COMPUTERIP">计算机IP</param>
        /// <param name="MACID">治具编号</param>
        /// <param name="LINEID">线体</param>
        /// <param name="PARTNUM">测试料号</param>
        /// <param name="CLASS">测试班别</param>
        /// <param name="FAULTID">故障名称</param>
        /// <param name="FAULTSTARTTIME">故障开始时间</param>
        /// <param name="FAULTTIME">故障时长(min)</param>
        /// <param name="FL01">预留字段/治具故障报警/OFF</param>
        /// <returns></returns>
        private async void BigDataInsert(string COMPUTERIP,string MACID,string LINEID,string PARTNUM,string CLASS,string FAULTID,string FAULTSTARTTIME,string FAULTTIME)
        {
            int result = await Task.Run<int>(() =>
            {
                MySqlConnection conn = null;
                try
                {
                    string StrMySQL = "Server=10.89.164.62;Database=dcdb;Uid=dcu;Pwd=dcudata;pooling=false;CharSet=utf8;port=3306";
                    conn = new MySqlConnection(StrMySQL);
                    conn.Open();
                    string _TDate;
                    if (DateTime.Now.Hour < 8)
                    {
                        _TDate = DateTime.Now.AddDays(-1).ToString("yyyyMMdd");
                    }
                    else
                    {
                        _TDate = DateTime.Now.ToString("yyyyMMdd");
                    }
                    string stm = "insert into TED_FAULT_DATA (WORKSTATION,COMPUTERIP,MACID,LINEID,PARTNUM,TDATE,TTIME,CLASS,FAULTID,FAULTSTARTTIME,FAULTTIME,REPAIRRESULT,REPAIRER,FL01) VALUES ('SLJ','"
                        + COMPUTERIP + "','" + MACID + "','" + LINEID + "','" + PARTNUM + "','" + _TDate + "','" + DateTime.Now.ToString("HHmmss") + "','"
                        + CLASS + "','" + FAULTID + "','" + FAULTSTARTTIME + "','" + FAULTTIME + "','NA','NA','ON')";
                    MySqlCommand cmd = new MySqlCommand(stm, conn);
                    int res = cmd.ExecuteNonQuery();
                    conn.Close();
                    conn.Dispose();
                    return res;
                }
                catch(Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    if (conn != null)
                    {
                        conn.Close();
                        conn.Dispose();
                    }
                    return -999;                    
                }
            });
            AddMessage("上传报警" + result.ToString());
        }
        private async void BigDataUpdate(string ip, string content,string starttime,string _class,string faulttime)
        {
            int result = await Task.Run<int>(() =>
            {
                MySqlConnection conn = null;
                try
                {
                    string StrMySQL = "Server=10.89.164.62;Database=dcdb;Uid=dcu;Pwd=dcudata;pooling=false;CharSet=utf8;port=3306";
                    conn = new MySqlConnection(StrMySQL);
                    conn.Open();

                    string stm = "update TED_FAULT_DATA SET CLASS = '" + _class + "',FAULTTIME = '" + faulttime + "',FL01 = 'OFF' WHERE COMPUTERIP = '" 
                    + ip + "' AND FAULTID = '" + content + "' AND FAULTSTARTTIME = '" + starttime + "'";
                    MySqlCommand cmd = new MySqlCommand(stm, conn);
                    int res = cmd.ExecuteNonQuery();
                    conn.Close();
                    conn.Dispose();
                    return res;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    if (conn != null)
                    {
                        conn.Close();
                        conn.Dispose();
                    }
                    return -999;
                }
            });
            AddMessage("更新报警" + result.ToString());
        }
        string GetIp()
        {
            string ipstring = "127.0.0.1";
            string hostName = Dns.GetHostName();
            System.Net.IPAddress[] addressList = Dns.GetHostAddresses(hostName);//会返回所有地址，包括IPv4和IPv6 
            foreach (var item in addressList)
            {
                ipstring = item.ToString();
                string[] ss = ipstring.Split(new string[] { "." }, StringSplitOptions.None);
                if (ss.Length == 4 && ss[0] == "10")
                {
                    return ipstring;
                }
            }
            return "127.0.0.1";
        }
        #endregion
        private void SaveResult(string bar,string rst,string index)
        {
            try
            {
                if (!Directory.Exists("C:\\Debug\\" + DateTime.Now.ToString("yyyyMMdd")))
                {
                    Directory.CreateDirectory(@"C:\\Debug\\" + DateTime.Now.ToString("yyyyMMdd"));
                }
                string path = "C:\\Debug\\" + DateTime.Now.ToString("yyyyMMdd") + "\\" + DateTime.Now.ToString("yyyyMMdd") + "Barcode.csv";
                Csvfile.savetocsv(path,new string[] { DateTime.Now.ToString(), bar , rst , index });
            }
            catch
            {

            }

        }
        //private void Button_Click(object sender, RoutedEventArgs e)
        //{

        //    //await udp1.SendAsync("test");
        //    //AddMessage(await udp1.ReceiveAsync());
        //    //await udp2.SendAsync("AEFAEWFA\r\n");
        //    //AddMessage("\"功能\"按钮仅供测试用，目前没作用");
        //

        public static void RunLog(string str)
        {
            try
            {
                string tempSaveFilee5 = System.AppDomain.CurrentDomain.BaseDirectory + @"RunLog";
                DateTime dtim = DateTime.Now;
                string DateNow = dtim.ToString("yyyy/MM/dd");
                string TimeNow = dtim.ToString("HH:mm:ss");

                if (!Directory.Exists(tempSaveFilee5))
                {
                    Directory.CreateDirectory(tempSaveFilee5);  //创建目录 
                }

                if (File.Exists(tempSaveFilee5 + "\\" + DateNow.Replace("/", "") + ".txt"))
                {
                    //第一种方法：
                    FileStream fs = new FileStream(tempSaveFilee5 + "\\" + DateNow.Replace("/", "") + ".txt", FileMode.Append);
                    StreamWriter sw = new StreamWriter(fs);
                    sw.WriteLine("TTIME：" + TimeNow + " 执行事件：" + str);
                    sw.Dispose();
                    fs.Dispose();
                    sw.Close();
                    fs.Close();
                }
                else
                {
                    //不存在就新建一个文本文件,并写入一些内容 
                    StreamWriter sw;
                    sw = File.CreateText(tempSaveFilee5 + "\\" + DateNow.Replace("/", "") + ".txt");
                    sw.WriteLine("TTIME：" + TimeNow + " 执行事件：" + str);
                    sw.Dispose();
                    sw.Close();
                }
            }
            catch { }
        }
    }
    class AlarmData
    {
        public string Code { set; get; }
        public string Content { set; get; }
        public DateTime Start { set; get; }
        public DateTime End { set; get; }
        public bool State { set; get; }
    }
}


