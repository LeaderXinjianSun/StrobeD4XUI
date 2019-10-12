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
using System.Windows.Shapes;
using BingLibrary.hjb.file;

namespace D4XUI
{
    /// <summary>
    /// SampleWindow.xaml 的交互逻辑
    /// </summary>
    public partial class SampleWindow : Window
    {
        public SampleWindow()
        {
            InitializeComponent();
        }
        public bool HasShow { get; set; }
        protected override void OnClosed(EventArgs e)
        {
            HasShow = false;
            base.OnClosed(e);
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            string iniSamplePath = System.Environment.CurrentDirectory + "\\Sample.ini";
            Inifile.INIWriteValue(iniSamplePath, "Sample", "IsSample", IsSampleCheck.IsChecked.ToString());
            Inifile.INIWriteValue(iniSamplePath, "Sample", "NGItemCount", NGItemCount.Text);
            Inifile.INIWriteValue(iniSamplePath, "Sample", "NGItem1", NGItem1.Text);
            Inifile.INIWriteValue(iniSamplePath, "Sample", "NGItem2", NGItem2.Text);
            Inifile.INIWriteValue(iniSamplePath, "Sample", "NGItem3", NGItem3.Text);
            Inifile.INIWriteValue(iniSamplePath, "Sample", "NGItem4", NGItem4.Text);
            Inifile.INIWriteValue(iniSamplePath, "Sample", "NGItem5", NGItem5.Text);
            Inifile.INIWriteValue(iniSamplePath, "Sample", "NGItem6", NGItem6.Text);
            Inifile.INIWriteValue(iniSamplePath, "Sample", "NGItem7", NGItem7.Text);
            Inifile.INIWriteValue(iniSamplePath, "Sample", "NGItem8", NGItem8.Text);
            Inifile.INIWriteValue(iniSamplePath, "Sample", "NGItem9", NGItem9.Text);
            Inifile.INIWriteValue(iniSamplePath, "Sample", "NGItem10", NGItem10.Text);

            Inifile.INIWriteValue(iniSamplePath, "Sample", "NGItemClassify1", NGItemClassify1.Text);
            Inifile.INIWriteValue(iniSamplePath, "Sample", "NGItemClassify2", NGItemClassify2.Text);
            Inifile.INIWriteValue(iniSamplePath, "Sample", "NGItemClassify3", NGItemClassify3.Text);
            Inifile.INIWriteValue(iniSamplePath, "Sample", "NGItemClassify4", NGItemClassify4.Text);
            Inifile.INIWriteValue(iniSamplePath, "Sample", "NGItemClassify5", NGItemClassify5.Text);
            Inifile.INIWriteValue(iniSamplePath, "Sample", "NGItemClassify6", NGItemClassify6.Text);
            Inifile.INIWriteValue(iniSamplePath, "Sample", "NGItemClassify7", NGItemClassify7.Text);
            Inifile.INIWriteValue(iniSamplePath, "Sample", "NGItemClassify8", NGItemClassify8.Text);
            Inifile.INIWriteValue(iniSamplePath, "Sample", "NGItemClassify9", NGItemClassify9.Text);
            Inifile.INIWriteValue(iniSamplePath, "Sample", "NGItemClassify10", NGItemClassify10.Text);

            
            Inifile.INIWriteValue(iniSamplePath, "Sample", "SamMode", SamMode.Text);

            Inifile.INIWriteValue(iniSamplePath, "Sample", "ZPMID", ZPMID.Text);
            Inifile.INIWriteValue(iniSamplePath, "Sample", "FCTMID", FCTMID.Text);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            string iniSamplePath = System.Environment.CurrentDirectory + "\\Sample.ini";            
            try
            {
                IsSampleCheck.IsChecked = bool.Parse(Inifile.INIGetStringValue(iniSamplePath, "Sample", "IsSample", "True"));
            }
            catch
            {
                IsSampleCheck.IsChecked = true;
            }
            try
            {
                int count = int.Parse(Inifile.INIGetStringValue(iniSamplePath, "Sample", "NGItemCount", "9"));
                NGItemCount.Text = count.ToString();
            }
            catch
            {
                NGItemCount.Text = "9";
            }
            NGItem1.Text = Inifile.INIGetStringValue(iniSamplePath, "Sample", "NGItem1", "Null");
            NGItem2.Text = Inifile.INIGetStringValue(iniSamplePath, "Sample", "NGItem2", "Null");
            NGItem3.Text = Inifile.INIGetStringValue(iniSamplePath, "Sample", "NGItem3", "Null");
            NGItem4.Text = Inifile.INIGetStringValue(iniSamplePath, "Sample", "NGItem4", "Null");
            NGItem5.Text = Inifile.INIGetStringValue(iniSamplePath, "Sample", "NGItem5", "Null");
            NGItem6.Text = Inifile.INIGetStringValue(iniSamplePath, "Sample", "NGItem6", "Null");
            NGItem7.Text = Inifile.INIGetStringValue(iniSamplePath, "Sample", "NGItem7", "Null");
            NGItem8.Text = Inifile.INIGetStringValue(iniSamplePath, "Sample", "NGItem8", "Null");
            NGItem9.Text = Inifile.INIGetStringValue(iniSamplePath, "Sample", "NGItem9", "Null");
            NGItem10.Text = Inifile.INIGetStringValue(iniSamplePath, "Sample", "NGItem10", "Null");

            NGItemClassify1.Text = Inifile.INIGetStringValue(iniSamplePath, "Sample", "NGItemClassify1", "Null") == "ZP" ? "ZP" : "FCT";
            NGItemClassify2.Text = Inifile.INIGetStringValue(iniSamplePath, "Sample", "NGItemClassify2", "Null") == "ZP" ? "ZP" : "FCT";
            NGItemClassify3.Text = Inifile.INIGetStringValue(iniSamplePath, "Sample", "NGItemClassify3", "Null") == "ZP" ? "ZP" : "FCT";
            NGItemClassify4.Text = Inifile.INIGetStringValue(iniSamplePath, "Sample", "NGItemClassify4", "Null") == "ZP" ? "ZP" : "FCT";
            NGItemClassify5.Text = Inifile.INIGetStringValue(iniSamplePath, "Sample", "NGItemClassify5", "Null") == "ZP" ? "ZP" : "FCT";
            NGItemClassify6.Text = Inifile.INIGetStringValue(iniSamplePath, "Sample", "NGItemClassify6", "Null") == "ZP" ? "ZP" : "FCT";
            NGItemClassify7.Text = Inifile.INIGetStringValue(iniSamplePath, "Sample", "NGItemClassify7", "Null") == "ZP" ? "ZP" : "FCT";
            NGItemClassify8.Text = Inifile.INIGetStringValue(iniSamplePath, "Sample", "NGItemClassify8", "Null") == "ZP" ? "ZP" : "FCT";
            NGItemClassify9.Text = Inifile.INIGetStringValue(iniSamplePath, "Sample", "NGItemClassify9", "Null") == "ZP" ? "ZP" : "FCT";
            NGItemClassify10.Text = Inifile.INIGetStringValue(iniSamplePath, "Sample", "NGItemClassify10", "Null") == "ZP" ? "ZP" : "FCT";

            SamMode.Text = Inifile.INIGetStringValue(iniSamplePath, "Sample", "SamMode", "Null") == "2h" ? "2h" : "6h";

            ZPMID.Text = Inifile.INIGetStringValue(iniSamplePath, "Sample", "ZPMID", "ATKC4-012");
            FCTMID.Text = Inifile.INIGetStringValue(iniSamplePath, "Sample", "FCTMID", "ATKC4-016");
        }

        private void TextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (PassWord.Text == GetPassWord())
                {                   
                    PasswordGrid.Visibility = Visibility.Collapsed;
                    ContentGrid.Visibility = Visibility.Visible;
                }
                PassWord.Text = "";
            }
        }
        string GetPassWord()
        {
            int day = System.DateTime.Now.Day;
            int month = System.DateTime.Now.Month;
            string ss = (day + month).ToString();
            string passwordstr = "";
            for (int i = 0; i < 4 - ss.Length; i++)
            {
                passwordstr += "0";
            }
            passwordstr += ss;
            return passwordstr;
        }
    }
}
