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
        string alarmExcelPath1 = System.Environment.CurrentDirectory + "\\D4X条码.xlsx";
        List<string> AlarmList = new List<string>();
        //List<string> BarcodeList = new List<string>();
        double downtime, zhuanpandowntime, lingmindudowntime, tiemojidowntime, waitzhuanpanforinput, waitlingminduforinput, waitTiemojiforinput, waitfortake, worktime, runtime;
        string DangbanFirstProduct = "";
        string LastBanci = "";
        string ZhuanpanJieGuo1 = "";
        string ZhuanpanJieGuo2 = "";
        string SimoJieGuo1 = "";
        string SimoJieGuo2 = "";
        string S_LingminduJieGuo1 = "";
        string S_LingminduJieGuo2 = "";
        Double UPH;
        int tick = 0;
        #endregion
        public MainWindow()
        {
            InitializeComponent();

            dispatcherTimer.Tick += new EventHandler(DispatcherTimerTickUpdateUi);
            dispatcherTimer.Interval = new TimeSpan(0, 0, 0, 0, 100);//6秒更新一次，即0.1分钟。

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
            if (++tick >= 60)
            {
                tick = 0;
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
                        Inifile.INIWriteValue(iniTimelyRainPath, "TimelyRain", "Downtime", downtime.ToString("F1"));
                        zhuanpandowntime = 0;
                        Inifile.INIWriteValue(iniTimelyRainPath, "TimelyRain", "Zhuanpandowntime", zhuanpandowntime.ToString("F1"));
                        lingmindudowntime = 0;
                        Inifile.INIWriteValue(iniTimelyRainPath, "TimelyRain", "Lingmindudowntime", lingmindudowntime.ToString("F1"));
                        tiemojidowntime = 0;
                        Inifile.INIWriteValue(iniTimelyRainPath, "TimelyRain", "Tiemojidowntime", tiemojidowntime.ToString("F1"));
                        waitzhuanpanforinput = 0;
                        //Inifile.INIWriteValue(iniTimelyRainPath, "TimelyRain", "Waitzhuanpanforinput", waitzhuanpanforinput.ToString("F1"));
                        //waitlingminduforinput = 0;
                        //Inifile.INIWriteValue(iniTimelyRainPath, "TimelyRain", "Waitlingminduforinput", waitlingminduforinput.ToString("F1"));
                        //WaitTiemojiforinput = 0;
                        //Inifile.INIWriteValue(iniTimelyRainPath, "TimelyRain", "WaitTiemojiforinput", WaitTiemojiforinput.ToString("F1"));
                        waitfortake = 0;
                        Inifile.INIWriteValue(iniTimelyRainPath, "TimelyRain", "Waitfortake", waitfortake.ToString("F1"));
                        //worktime = 0;
                        //Inifile.INIWriteValue(iniTimelyRainPath, "TimelyRain", "Worktime", worktime.ToString("F1"));
                        //runtime = 0;
                        //Inifile.INIWriteValue(iniTimelyRainPath, "TimelyRain", "Runtime", runtime.ToString("F1"));

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
                        Inifile.INIWriteValue(iniTimelyRainPath, "TimelyRain", "Downtime", downtime.ToString("F1"));
                    }
                    if (M10000[101] && DangbanFirstProduct == GetBanci())
                    {
                        zhuanpandowntime += 0.1;
                        Inifile.INIWriteValue(iniTimelyRainPath, "TimelyRain", "Zhuanpandowntime", zhuanpandowntime.ToString("F1"));
                    }
                    if (M10000[102] && DangbanFirstProduct == GetBanci())
                    {
                        lingmindudowntime += 0.1;
                        Inifile.INIWriteValue(iniTimelyRainPath, "TimelyRain", "Lingmindudowntime", lingmindudowntime.ToString("F1"));
                    }
                    if (M10000[103] && DangbanFirstProduct == GetBanci())
                    {
                        tiemojidowntime += 0.1;
                        Inifile.INIWriteValue(iniTimelyRainPath, "TimelyRain", "Tiemojidowntime", tiemojidowntime.ToString("F1"));
                    }
                    //if (M10000[104] && DangbanFirstProduct == GetBanci())
                    //{
                    //    waitzhuanpanforinput += 0.1;
                    //    Inifile.INIWriteValue(iniTimelyRainPath, "TimelyRain", "Waitzhuanpanforinput", waitzhuanpanforinput.ToString("F1"));
                    //}
                    //if (M10000[105] && DangbanFirstProduct == GetBanci())
                    //{
                    //    waitlingminduforinput += 0.1;
                    //    Inifile.INIWriteValue(iniTimelyRainPath, "TimelyRain", "Waitlingminduforinput", waitlingminduforinput.ToString("F1"));
                    //}
                    //if (M10000[106] && DangbanFirstProduct == GetBanci())
                    //{
                    //    waitTiemojiforinput += 0.1;
                    //    Inifile.INIWriteValue(iniTimelyRainPath, "TimelyRain", "WaitTiemojiforinput", waitTiemojiforinput.ToString("F1"));
                    //}
                    if (M10000[107] && DangbanFirstProduct == GetBanci())
                    {
                        waitfortake += 0.1;
                        Inifile.INIWriteValue(iniTimelyRainPath, "TimelyRain", "Waitfortake", waitfortake.ToString("F1"));
                    }
                    //    input.Text = HD200[0].ToString();
                    //    Inifile.INIWriteValue(iniFClient, "DataList", "input", input.Text);
                    //    output.Text = HD200[1].ToString();
                    //    Inifile.INIWriteValue(iniFClient, "DataList", "output", output.Text);
                    //    TestCount_Total.Text = HD200[2].ToString();
                    //    Inifile.INIWriteValue(iniFClient, "DataList", "TestCount_Total", TestCount_Total.Text);
                    //    PassCount_Total.Text = HD200[3].ToString();
                    //    if (HD200[2] == 0)
                    //    {
                    //        Yield_Total.Text = "0";
                    //    }
                    //    else
                    //    {
                    //        Yield_Total.Text = (HD200[3] / HD200[2] * 100).ToString("F1");
                    //    }
                    //    Inifile.INIWriteValue(iniFClient, "DataList", "Yield_Total", Yield_Total.Text);
                    //    TestCount_1.Text = HD200[4].ToString();
                    //    Inifile.INIWriteValue(iniFClient, "DataList", "TestCount_1", TestCount_1.Text);
                    //    PassCount_1.Text = HD200[5].ToString();
                    //    if (HD200[4] == 0)
                    //    {
                    //        Yield_1.Text = "0";
                    //    }
                    //    else
                    //    {
                    //        Yield_1.Text = (HD200[5] / HD200[4] * 100).ToString("F1");
                    //    }
                    //    Inifile.INIWriteValue(iniFClient, "DataList", "Yield_1", Yield_1.Text);

                    TestCount_2.Text = HD200[6].ToString();
                    Inifile.INIWriteValue(iniFClient, "DataList", "TestCount_2", TestCount_2.Text);
                    PassCount_2.Text = HD200[3].ToString();
                    if (HD200[6] == 0)
                    {
                        Yield_2.Text = "0";
                    }
                    else
                    {
                        Yield_2.Text = (HD200[3] / HD200[6] * 100).ToString("F1");
                    }
                    Inifile.INIWriteValue(iniFClient, "DataList", "Yield_2", Yield_2.Text);
                    //    AlarmCount.Text = HD200[8].ToString();
                    //    Inifile.INIWriteValue(iniFClient, "Alarm", "count", AlarmCount.Text);
                    //    Inifile.INIWriteValue(iniFClient, "state", "state", D1200.ToString());

                    //    if (DangbanFirstProduct == GetBanci())
                    //    {
                    //        worktime += 0.1;
                    //        Inifile.INIWriteValue(iniTimelyRainPath, "TimelyRain", "Worktime", worktime.ToString());
                    //    }
                    //    if (DangbanFirstProduct == GetBanci() && D1200 == 1)
                    //    {
                    //        runtime += 0.1;
                    //        Inifile.INIWriteValue(iniTimelyRainPath, "TimelyRain", "Runtime", runtime.ToString());
                    //    }
                    //    if (runtime == 0 || UPH == 0)
                    //        AchievingRate.Text = "100";
                    //    else
                    //        AchievingRate.Text = (HD200[1] / (UPH / 60 * runtime) * 100).ToString("F1");
                    //    Inifile.INIWriteValue(iniFClient, "DataList", "AchievingRate", AchievingRate.Text);

                    //}
                    Downtime.Text = downtime.ToString("F1");
                    Zhuanpandowntime.Text = zhuanpandowntime.ToString("F1");
                    Lingmindudowntime.Text = lingmindudowntime.ToString("F1");
                    Tiemojidowntime.Text = tiemojidowntime.ToString("F1");
                    //Waitzhuanpanforinput.Text = waitzhuanpanforinput.ToString("F1");
                    //Waitlingminduforinput.Text = waitlingminduforinput.ToString("F1");
                    //WaitTiemojiforinput.Text = waitTiemojiforinput.ToString("F1");
                    Waitfortake.Text = waitfortake.ToString("F1");
                    Inifile.INIWriteValue(iniFClient, "DataList", "Downtime", downtime.ToString("F1"));
                    Inifile.INIWriteValue(iniFClient, "DataList", "Zhuanpandowntime", zhuanpandowntime.ToString("F1"));
                    Inifile.INIWriteValue(iniFClient, "DataList", "Lingmindudowntime", lingmindudowntime.ToString("F1"));
                    Inifile.INIWriteValue(iniFClient, "DataList", "Tiemojidowntime", tiemojidowntime.ToString("F1"));
                    //Inifile.INIWriteValue(iniFClient, "DataList", "Waitzhuanpanforinput", waitzhuanpanforinput.ToString("F1"));
                    //Inifile.INIWriteValue(iniFClient, "DataList", "Waitlingminduforinput", waitlingminduforinput.ToString("F1"));
                    //Inifile.INIWriteValue(iniFClient, "DataList", "WaitTiemojiforinput", waitTiemojiforinput.ToString("F1"));
                    Inifile.INIWriteValue(iniFClient, "DataList", "Waitfortake", waitfortake.ToString("F1"));

                    //if (worktime == 0)
                    //{
                    //    ProperRate.Text = "0";
                    //    ProperRate_AutoMation.Text = "0";
                    //    ProperRate_Zhuanpan.Text = "0";
                    //    ProperRate_Lingmindu.Text = "0";
                    //    ProperRate_Tiemoji.Text = "0";
                    //}
                    //else
                    //{
                    //    ProperRate.Text = ((1 - (downtime + zhuanpandowntime + lingmindudowntime + tiemojidowntime) / worktime) * 100).ToString("F1");
                    //    ProperRate_AutoMation.Text = ((1 - downtime / worktime) * 100).ToString("F1");
                    //    ProperRate_Zhuanpan.Text = ((1 - zhuanpandowntime / worktime) * 100).ToString("F1");
                    //    ProperRate_Lingmindu.Text = ((1 - lingmindudowntime / worktime) * 100).ToString("F1");
                    //    ProperRate_Tiemoji.Text = ((1 - tiemojidowntime / worktime) * 100).ToString("F1");
                    //}
                    //Inifile.INIWriteValue(iniFClient, "DataList", "ProperRate", ProperRate.Text);
                    //Inifile.INIWriteValue(iniFClient, "DataList", "ProperRate_AutoMation", ProperRate_AutoMation.Text);
                    //Inifile.INIWriteValue(iniFClient, "DataList", "ProperRate_Zhuanpan", ProperRate_Zhuanpan.Text);
                    //Inifile.INIWriteValue(iniFClient, "DataList", "ProperRate_Lingmindu", ProperRate_Lingmindu.Text);
                    //Inifile.INIWriteValue(iniFClient, "DataList", "ProperRate_Tiemoji", ProperRate_Tiemoji.Text);
                    #endregion
                }
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
                    //worksheet.Cells[newrow, 6].Value = Waitzhuanpanforinput.Text;
                    //worksheet.Cells[newrow, 7].Value = Waitlingminduforinput.Text;
                    //worksheet.Cells[newrow, 8].Value = WaitTiemojiforinput.Text;
                    worksheet.Cells[newrow, 9].Value = Waitfortake.Text;
                    //worksheet.Cells[newrow, 10].Value = input.Text;
                    //worksheet.Cells[newrow, 11].Value = output.Text;
                    //worksheet.Cells[newrow, 12].Value = TestCount_Total.Text;
                    //worksheet.Cells[newrow, 13].Value = PassCount_Total.Text;
                    //worksheet.Cells[newrow, 14].Value = Yield_Total.Text;
                    //worksheet.Cells[newrow, 15].Value = TestCount_1.Text;
                    //worksheet.Cells[newrow, 16].Value = PassCount_1.Text;
                    //worksheet.Cells[newrow, 17].Value = Yield_1.Text;
                    //worksheet.Cells[newrow, 18].Value = TestCount_2.Text;
                    //worksheet.Cells[newrow, 19].Value = PassCount_2.Text;
                    //worksheet.Cells[newrow, 20].Value = Yield_2.Text;
                    //worksheet.Cells[newrow, 21].Value = AlarmCount.Text;
                    //worksheet.Cells[newrow, 22].Value = AchievingRate.Text;
                    //worksheet.Cells[newrow, 23].Value = ProperRate.Text;
                    //worksheet.Cells[newrow, 24].Value = ProperRate_AutoMation.Text;
                    //worksheet.Cells[newrow, 25].Value = ProperRate_Zhuanpan.Text;
                    //worksheet.Cells[newrow, 26].Value = ProperRate_Lingmindu.Text;
                    //worksheet.Cells[newrow, 27].Value = ProperRate_Tiemoji.Text;
                    package.Save();
                }
                AddMessage("保存机台生产数据完成");
            }
            catch (Exception ex)
            {
                AddMessage(ex.Message);
            }
        }
        //private void WriteBarcodeData()
        //{
        //    string excelpath1 = @"D:\D4XBarcodeData.xlsx";
        //    try
        //    {
        //        FileInfo fileInfo1 = new FileInfo(excelpath1);
        //        if (!File.Exists(excelpath1))
        //        {
        //            using (ExcelPackage package1 = new ExcelPackage(fileInfo1))
        //            {
        //                ExcelWorksheet worksheet1 = package1.Workbook.Worksheets.Add("BarcodeData");
        //                worksheet1.Cells[1, 1].Value = "更新时间";
        //                worksheet1.Cells[1, 2].Value = "撕膜平台条码";
        //                worksheet1.Cells[1, 3].Value = "吸抓吸盘条码";
        //                worksheet1.Cells[1, 4].Value = "灵敏度治具条码";
        //                package1.Save();
        //            }
        //        }
        //        using (ExcelPackage package1 = new ExcelPackage(fileInfo1))
        //        {
        //            ExcelWorksheet worksheet1 = package1.Workbook.Worksheets[1];
        //            int newrow = worksheet1.Dimension.End.Row + 1;
        //            worksheet1.Cells[newrow, 1].Value = System.DateTime.Now.ToString();
        //            worksheet1.Cells[newrow, 2].Value = ZhuanpanBarcode1.Text + ZhuanpanBarcode2.Text;
        //            worksheet1.Cells[newrow, 3].Value = SimoBarcode1.Text + SimoBarcode2.Text;
        //            worksheet1.Cells[newrow, 4].Value = LingminduBarcode1.Text+ LingminduBarcode2.Text;




        //            package1.Save();
        //        }
        //        AddMessage("保存条码数据完成");
        //    }

        //    catch (Exception ex)
        //    {
        //        AddMessage(ex.Message);
        //    }
        //}







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
            LoadParameter();
            Async.RunFuncAsync(PLCWork, null);

            LoadAlarmNames();
            LoadTimelyRain();
            //LoadBarcode();
            AddMessage("加载完成");
            dispatcherTimer.Start();
            UDPWork();
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
        //private void LoadBarcode()
        //{
        //    try
        //    {
        //        if (File.Exists(alarmExcelPath1))
        //        {
        //            FileInfo existingFile1 = new FileInfo(alarmExcelPath1);
        //            using (ExcelPackage package1 = new ExcelPackage(existingFile1))
        //            {
        //                // get the first worksheet in the workbook
        //                ExcelWorksheet worksheet1 = package1.Workbook.Worksheets[1];
        //                for (int i = 1; i <= worksheet1.Dimension.End.Row; i++)
        //                {
        //                    BarcodeList.Add(worksheet1.Cells["B" + i.ToString()].Value.ToString());
        //                }

        //            }
        //        }
        //        else
        //        {
        //            AddMessage("D4X条码.xlsx 文件不存在");
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        AddMessage(ex.Message);
        //    }
        //}

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
            ZhuanpanBarcode1.Text = Inifile.INIGetStringValue(iniParameterPath, "Barcode", "ZhuanpanBarcode1", "null");
            ZhuanpanBarcode2.Text = Inifile.INIGetStringValue(iniParameterPath, "Barcode", "ZhuanpanBarcode2", "null");
            SimoBarcode1.Text = Inifile.INIGetStringValue(iniParameterPath, "Barcode", "SimoBarcode1", "null");
            SimoBarcode2.Text = Inifile.INIGetStringValue(iniParameterPath, "Barcode", "SimoBarcode2", "null");
            LingminduBarcode1.Text = Inifile.INIGetStringValue(iniParameterPath, "Barcode", "LingminduBarcode1", "null");
            LingminduBarcode2.Text = Inifile.INIGetStringValue(iniParameterPath, "Barcode", "LingminduBarcode2", "null");
            //LingminduJieGuo1.Text = Inifile.INIGetStringValue(iniParameterPath, "JieGuo", "LingminduJieGuo1", "null");
            //LingminduJieGuo2.Text = Inifile.INIGetStringValue(iniParameterPath, "JieGuo", "LingminduJieGuo2", "null");
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
        bool M10140 = false, M10141 = false, M10150 = false, M10151 = false, M10152 = false, M10153 = false, M10154 = false;
        bool M10142 = false, M10143 = false;
        bool M10144 = false, M10145 = false;
        bool M10146 = false, M10147 = false;
        bool M10148 = false, M10149 = false;

        double D1200;

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
        }


        public void PLCWork()
        {
            string COM = Inifile.INIGetStringValue(iniParameterPath, "PLC", "COM", "COM19");
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
            bool first = true;
            Stopwatch sw = new Stopwatch();
            while (true)
            {
                sw.Restart();
                string rs = await udp1.ReceiveAsync();

                await Task.Delay(2);
                #region 从转盘接收条码

                //string rs = await udp1.ReceiveAsync();
                if (rs != "error")
                {
                    //string tiaomafenpeicishu = rs;
                    //if (tiaomafenpeicishu.Contains("\r"))
                    //{
                    //    tiaomafenpeicishu.Replace("\r","");
                    //}
                    //if (tiaomafenpeicishu.Contains("\n"))
                    //{
                    //    tiaomafenpeicishu.Replace("\n", "");
                    //}
                    RunLog("从转盘接收 " + rs);
                    AddMessage("从转盘接收 " + rs);

                    Xinjie.SetM(10148, true);

                    string sends = "SNOK";
                    await udp1.SendAsync(sends);
                    AddMessage("向转盘发送 " + sends);
                    //if (M10000[142])
                    //{
                    //    ZhuanpanBarcode1.Text = "null";
                    //    Inifile.INIWriteValue(iniParameterPath, "Barcode", "ZhuanpanBarcode1", ZhuanpanBarcode1.Text);
                    //}
                    //if (M10000[143])
                    //{
                    //    ZhuanpanBarcode2.Text = "null";
                    //    Inifile.INIWriteValue(iniParameterPath, "Barcode", "ZhuanpanBarcode2", ZhuanpanBarcode2.Text);
                    //}
                    try
                    {
                        string[] s1 = rs.Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries);
                        string[] s1_1 = s1[0].Split(new string[] { ":" }, StringSplitOptions.RemoveEmptyEntries);
                        if (s1_1[0] == "SN1" && s1_1.Length == 2)
                        {
                            //SN1:G5Y9301RDD0K9037V-GF,P;SN2:G5Y9301RDCNK9037A-GF,P
                            string[] s1_1_1 = s1_1[1].Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
                            ZhuanpanBarcode1.Text = s1_1_1[0];
                            Inifile.INIWriteValue(iniParameterPath, "Barcode", "ZhuanpanBarcode1", ZhuanpanBarcode1.Text);
                            if (s1_1_1.Length == 2)
                            {
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
                                ZhuanpanBarcode1.Background = Brushes.Gray;
                            }
                        }
                        string[] s1_2 = s1[1].Split(new string[] { ":" }, StringSplitOptions.RemoveEmptyEntries);
                        if (s1_2[0] == "SN2" && s1_2.Length == 2)
                        {
                            string[] s1_2_1 = s1_2[1].Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
                            ZhuanpanBarcode2.Text = s1_2_1[0];
                            Inifile.INIWriteValue(iniParameterPath, "Barcode", "ZhuanpanBarcode2", ZhuanpanBarcode2.Text);

                            if (s1_2_1.Length == 2)
                            {
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
                                ZhuanpanBarcode2.Background = Brushes.Gray;
                            }

                        }
                    }
                    catch (Exception ex)
                    {

                        AddMessage(ex.Message);
                    }


                }
                #endregion
                #region 从灵敏度接收结果

                //string rs1 = await udp2.ReceiveAsync();
                //if (rs1 != "error")
                //{
                //    AddMessage("从灵敏度接收 " + rs1);
                //    RunLog("从灵敏度接收 " + rs1);
                //    try
                //    {
                //        string[] s2 = rs1.Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries);
                //        string[] s2_1 = s2[0].Split(new string[] { ":" }, StringSplitOptions.RemoveEmptyEntries);
                //        if (s2_1[0] == "SN1")
                //        {
                //            string[] s2_1_1 = s2_1[1].Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
                //            LingminduJieGuo1.Text = s2_1_1[1];
                //            Inifile.INIWriteValue(iniParameterPath, "JieGuo", "LingminduJieGuo1", LingminduJieGuo1.Text);
                //            if (s2_1_1[1] == "OK")
                //                Xinjie.SetM(10144, true);
                //            else
                //                Xinjie.SetM(10144, false);
                //            if (s2_1_1[1] == "NG")
                //                Xinjie.SetM(10145, true);
                //            else
                //                Xinjie.SetM(10145, false);

                //        }
                //        string[] s2_2 = s2[1].Split(new string[] { ":" }, StringSplitOptions.RemoveEmptyEntries);
                //        if (s2_2[0] == "SN2")
                //        {
                //            string[] s2_2_1 = s2_2[1].Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
                //            LingminduJieGuo2.Text = s2_2_1[1];
                //            Inifile.INIWriteValue(iniParameterPath, "JieGuo", "LingminduJieGuo2", LingminduJieGuo2.Text);
                //            string[] s2_2_1_1 = s2_2_1[1].Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries);

                //            if (s2_2_1_1[0] == "OK")
                //                Xinjie.SetM(10146, true);
                //            else
                //                Xinjie.SetM(10146, false);
                //            if (s2_2_1_1[0] == "NG")
                //                Xinjie.SetM(10147, true);
                //            else
                //                Xinjie.SetM(10147, false);

                //        }

                //    }

                //    catch (Exception ex)
                //    {

                //        AddMessage(ex.Message);
                //    }



                //}
                #endregion
                #region 读取PLC信号

                if (plcstate)
                {
                    if (first)
                    {
                        first = false;
                        M10140 = M10000[140];//条码移动到吸爪
                        M10141 = M10000[141];//条码移动到灵敏度
                        M10142 = M10000[142];//左穴空,清空左穴条码
                        M10143 = M10000[143];//右穴空,清空右穴条码
                        M10150 = M10000[150];
                        M10151 = M10000[151];
                        M10152 = M10000[152];
                        M10153 = M10000[153];
                        M10154 = M10000[154];
                    }
                    if (M10140 != M10000[140])
                    {
                        M10140 = M10000[140];
                        if (M10140)
                        {

                            //if (M10000[142])
                            //    ZhuanpanBarcode1.Text = "null";
                            //Inifile.INIWriteValue(iniParameterPath, "Barcode", "ZhuanpanBarcode1", ZhuanpanBarcode1.Text);
                            //if (M10000[143])
                            //    ZhuanpanBarcode2.Text = "null";
                            //Inifile.INIWriteValue(iniParameterPath, "Barcode", "ZhuanpanBarcode2", ZhuanpanBarcode2.Text);
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
                            //SimoJieGuo1 = ZhuanpanJieGuo1;
                            //SimoJieGuo2 = ZhuanpanJieGuo2;
                            //Xinjie.SetM(10142, false);
                            //Xinjie.SetM(10143, false);
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
                            //S_LingminduJieGuo1 = SimoJieGuo1;
                            //S_LingminduJieGuo2 = SimoJieGuo2;
                            string sends = "SN1:" + LingminduBarcode1.Text + ",P" + ";" + "SN2:" + LingminduBarcode2.Text + ",P" + "\r\n";
                            await udp2.SendAsync(sends);
                            AddMessage("向灵敏度发送 " + sends);
                            RunLog("向灵敏度发送 " + sends);
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
                        }
                        else
                        {
                            LingminduJieGuo2.Background = Brushes.Gray;
                        }
                    }
                }
                CycleText.Text = sw.ElapsedMilliseconds.ToString() + "ms";
            }
            #endregion
        }


        #endregion
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
}


