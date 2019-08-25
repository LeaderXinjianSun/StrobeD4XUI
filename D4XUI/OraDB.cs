/*本类用于臻鼎科技的数据上传，基本全面*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Data.OleDb;
using System.Data;

namespace 臻鼎科技OraDB
{


    //上传数据
    public struct _SQL_DATA
    {
        public string strWork;          /*工号*/
        public string strOperator;      /*作业员*/
        public string strPart;          /*√料号*/
        public string strVer;           /*版本*/
        public string strFix;           /*√治具编号*/
        public string strMac;           /*机台编号*/
        public string strLine;          /*线体*/
        public string strOrder;         /*工令*/
        public string strDate;          /*√日期*/
        public string strTime;          /*√时间*/
        public string strInfo;          /*备注*/
        public string strBarcode;       /*√条码*/
        public string strResult;        /*√测试结果*/
        public string strVal01;         /*测试值1*/
        public string strVal02;         /*测试值2*/
        public string strVal03;         /*测试值3*/
        public string strVal04;         /*测试值4*/
        public string strVal05;         /*测试值5*/
        public string strVal06;         /*测试值6*/
        public string strVal07;         /*测试值7*/
        public string strVal08;         /*测试值8*/
        public string strSitem;         /*√测试类型*/
        public int Stnum;               /*√样品使用次数*/
        public int Unum;                /*√样品已使用次数*/
        public string S04;
    }

    public class OraDB : OraDBBase
    {
        public static string[] arrTableName = { "ICT_DATA", "UT_DATA", "FLUKE_DATA", "ICT_TAB",
                                              "UT_TABLE", "FLUKE_TABLE", "FAP207FCT_TABLE","CRDATA","FCT_DATA","BARSAMINFO","BARSAMREC" };
        public static string[] arrBarFields = { "BARCODE", "BARCODE", "BARCODE", "BAR_CODE", 
                                              "BARCODE", "BARCODE", "BARCODE","BARCODE","BARCODE" };

        private string m_server_name;
        private string m_id;
        private string m_pwd;
        private string m_table_name;

        public string strTableName
        {
            get { return m_table_name; }
            set { m_table_name = value; }
        }

        public OraDB()
            : base()
        {

        }

        public OraDB(string ServerName, string ID, string PWD)
            : base(ServerName, ID, PWD)
        {
            m_server_name = ServerName;
            m_id = ID;
            m_pwd = PWD;

            //
            base.connect();
        }

        #region "公共方法"

        public bool connectDB()
        {
            return base.connect();
        }

        public void disconnectDB()
        {
            base.disconnect();
        }

        public bool isConnect()
        {
            return base.getConnectState();
        }

        private string getSubStr(string str1, int len)
        {
            if (str1 == null)
            {
                return "";
            }
            int sz = len > str1.Length ? str1.Length : len;
            return str1.Substring(0, sz).ToUpper();
        }

        #endregion

        #region "SFC数据库"

        private string sfc_getLocalIP()
        {
            try
            {
                string hostname = Dns.GetHostName();
                IPAddress[] add = Dns.GetHostAddresses(hostname);
                foreach (IPAddress ip in add)
                {
                    //
                }
                return add[0].ToString();
            }
            catch (System.Exception ex)
            {
                return null;
            }
        }
        //"SELECT TO_CHAR(SYSDATE,'YYYY-MM-DD HH24:MI:SS') sDate FROM DUAL";
        public string sfc_getServerDateTime()
        {
            DataSet da = base.selectSQL2("select to_char(SYSDATE,'YYYY-MM-DD HH24:MI:SS') sDate FROM DUAL");
            return da.Tables[0].Rows[0][0].ToString();
        }

        public string sfc_getTableNameByIndex(int TableIndex)
        {
            return arrTableName[TableIndex];
        }

        public int sfc_getTableNameIndex(string TableName)
        {
            if (TableName != null)
            {
                for (int i = 0; i < arrTableName.Length; i++)
                {
                    if (arrTableName[i].ToUpper().Equals(TableName.Trim().ToUpper()))
                    {
                        return i;
                    }
                }
            }
            return 2;
        }

        public void sfc_getSQLSTR(int t, _SQL_DATA d, ref List<string> arrField, ref List<string> arrValue)
        {
            string strIP = sfc_getLocalIP();
            /*strTable=arrTableName[t];*/

            //长字符串换行前需加上@,但是会影响插入
            switch (t)
            {
                case 0:
                    arrField.AddRange(new string[] { "ICTNO","PARTNUM","WORKNO","LINEID","OPERTOR","BARCODE","TRESULT",
                    "TESTDATE","TESTTIME","ICT01","ICT02","ICT03","ICT04","ICT05","ICT06",
                    "ICT07","ICT08","ICT09","ICT10","SDATE","STIME","FPATH"});
                    arrValue.AddRange(new string[] { getSubStr(d.strFix, 19),
                                 getSubStr(d.strPart, 19),
                                 getSubStr(d.strOrder, 19),
                                 getSubStr(d.strLine, 19),
                                 getSubStr(d.strWork, 19),
                                 getSubStr(d.strBarcode, 49),
                                 getSubStr(d.strResult, 19),
                                 d.strDate,
                                 d.strTime,
                                 getSubStr(d.strInfo, 199),
                                 getSubStr(d.strVal01, 199),
                                 getSubStr(d.strVal02, 19),
                                 getSubStr(d.strVal03, 19),
                                 getSubStr(d.strVal04, 19),
                                 getSubStr(d.strVal05, 19),
                                 getSubStr(d.strVal06, 19),
                                 strIP,
                                 getSubStr(d.strVal07, 19),
                                 getSubStr(d.strVal08, 19),
                                 d.strDate,
                                 d.strTime,
                                 "" });

                    /*strTable = "ICT_DATA";*/
                    break;
                case 1:
                    arrField.AddRange(new string[] {"MACID","PARTNUM","WORKNO","LINEID","OPERTOR","BARCODE","TRESULT",
                                "TESTDATE","TESTTIME","UT01","UT02","UT03","UT04","UT05","UT06","UT07",
                                "UT08","UT09","UT10","SDATE","STIME","FPATH" });
                    arrValue.AddRange(new string[] { getSubStr(d.strFix, 19),
                                getSubStr(d.strPart, 19),
                                getSubStr(d.strOrder, 19),
                                getSubStr(d.strLine, 19),
                                getSubStr(d.strWork, 19),
                                getSubStr(d.strBarcode, 49),
                                d.strResult,
                                d.strDate,
                                d.strTime,
                                getSubStr(d.strInfo, 199),
                                getSubStr(d.strVal01, 199),
                                getSubStr(d.strVal02, 19),
                                getSubStr(d.strVal03, 19),
                                getSubStr(d.strVal04, 19),
                                getSubStr(d.strVal05, 19),
                                getSubStr(d.strVal06, 19),
                                strIP,
                                getSubStr(d.strVal07, 19),
                                getSubStr(d.strVal08, 19),
                                d.strDate,
                                d.strTime,
                                ""});
                    /*strTable = "UT_DATA";*/
                    break;
                case 2:
                    arrField.AddRange(new string[] {"MACID","PARTNUM","WORKNO","LINEID","OPERTOR","BARCODE","TRESULT",
                                "TESTDATE","TESTTIME","FL01","FL02","FL03","FL04","FL05","FL06","FL07",
                                "FL08","FL09","FL10","SDATE","STIME","FPATH" });
                    arrValue.AddRange(new string[] { getSubStr(d.strFix, 19),
                                getSubStr(d.strPart, 19),
                                getSubStr(d.strOrder, 19),
                                getSubStr(d.strLine, 19),
                                getSubStr(d.strWork, 19),
                                getSubStr(d.strBarcode, 49),
                                d.strResult,
                                d.strDate,
                                d.strTime,
                                getSubStr(d.strInfo, 199),
                                getSubStr(d.strVal01, 199),
                                getSubStr(d.strVal02, 19),
                                getSubStr(d.strVal03, 19),
                                getSubStr(d.strVal04, 19),
                                getSubStr(d.strVal05, 19),
                                getSubStr(d.strVal06, 19),
                                strIP,
                                getSubStr(d.strVal07, 19),
                                getSubStr(d.strVal08, 19),
                                d.strDate,
                                d.strTime,
                                ""});
                    /*strTable = "FLUKE_DATA";*/
                    break;
                case 3:
                    arrField.AddRange(new string[] { "ICT_NO", "BOARD_NAME", "BAR_CODE", "DDATE", "TTIME", "RESULT", 
                    "OP_ID", "SDATE", "STIME", "FPATH" });
                    arrValue.AddRange(new string[] { getSubStr(d.strFix, 19),
                                 getSubStr(d.strPart, 19),
                                 getSubStr(d.strBarcode, 24),
                                 d.strDate,
                                 d.strTime,
                                 getSubStr(d.strResult, 1),
                                 getSubStr(d.strWork, 19),
                                 d.strDate,
                                 d.strTime,
                                 ""});
                    /*strTable = "ICT_TAB";*/
                    break;
                case 4:
                    arrField.AddRange(new string[] { "PARTNUM","REVISION","WORK_ORDER","LINE_ID","OPERATOR,FIXTURE",
                                "INSTRUMENT","BARCODE","RESULT","TESTDATE","TESTTIME","FPATH"});
                    arrValue.AddRange(new string[] { getSubStr(d.strPart, 19),
                                getSubStr(d.strInfo, 19),
                                getSubStr(d.strOrder, 19),
                                getSubStr(d.strLine, 19),
                                getSubStr(d.strWork, 19),
                                getSubStr(d.strFix, 19),
                                getSubStr(d.strFix, 19),
                                getSubStr(d.strBarcode, 39),
                                d.strResult,
                                d.strDate,
                                d.strTime,
                                ""});
                    /*strTable = "UT_TABLE";*/
                    break;
                case 5:
                    arrField.AddRange(new string[] {"PARTNUM","REVISION","WORK_ORDER","LINE_ID","OPERATOR","NO","FIXTURE",
                                "INSTRUMENT","BARCODE","RESULT","TESTDATE","TESTTIME","FPATH" });
                    arrValue.AddRange(new string[] { getSubStr(d.strPart, 19),
                                getSubStr(d.strInfo, 9),
                                getSubStr(d.strOrder, 19),
                                getSubStr(d.strLine, 19),
                                getSubStr(d.strWork, 14),
                                getSubStr(d.strWork, 14),
                                getSubStr(d.strFix, 19),
                                getSubStr(d.strFix, 19),
                                getSubStr(d.strBarcode, 39),
                                d.strResult,
                                d.strDate,
                                d.strTime,
                                ""});
                    /*strTable = "FLUKE_TABLE";*/
                    break;
                case 6:
                    arrField.AddRange(new string[] {"PARTNUM","REVISION","WORK_ORDER","LINE_ID","NO",
                                "FIXTURE","BARCODE","RESULT","TESTDATE","TESTTIME","FPATH" });
                    arrValue.AddRange(new string[] {getSubStr(d.strPart, 19),
                                getSubStr(d.strInfo, 9),
                                getSubStr(d.strOrder, 19),
                                getSubStr(d.strLine, 9),
                                getSubStr(d.strWork, 19),
                                getSubStr(d.strFix, 19),
                                getSubStr(d.strBarcode, 29),
                                d.strResult,
                                d.strDate,
                                d.strTime,
                                "" });
                    /*strTable="FAP207FCT_TABLE";*/
                    break;
                case 9:
                    arrField.AddRange(new string[] {"PARTNUM","REVISION","SITEM","BARCODE","NGITEM",
                                "SLINE","SNUM","STNUM","UNUM","TIMEINT","ACTDATE","MNO","CDATE","CTIME","CUID",
                                "ISACT","S01","S02","S03","S04","S05"});
                    arrValue.AddRange(new string[] {getSubStr(d.strPart, 19),
                                "",
                                getSubStr(d.strSitem, 19),
                                getSubStr(d.strBarcode, 29),
                                "",
                                "",
                                "",
                                d.Stnum.ToString(),
                                d.Unum.ToString(),
                                "",

                                "",
                                "",
                                d.strDate,
                                d.strTime,
                                "",

                                "",
                                "",
                                "",
                                "",
                                d.S04,
                                ""});
                    /*strTable="BARSAMINFO_TABLE";*/
                    break;
                case 10:
                    arrField.AddRange(new string[] {"PARTNUM","REVISION","SITEM","BARCODE","NGITEM",
                                    "TRES","MNO","CDATE","CTIME","CLINE","CUID",
                                    "SR01","SR02","SR03","SR04","SR05"});
                    arrValue.AddRange(new string[] {getSubStr(d.strPart, 19),
                                    "",
                                    getSubStr(d.strSitem, 19),
                                    getSubStr(d.strBarcode, 29),
                                    "",
                                    d.strResult,
                                    getSubStr(d.strFix, 19),
                                    d.strDate,
                                    d.strTime,
                                    "",
                                    "",
                                    "",
                                    "",
                                    "",
                                    d.S04,
                                    ""});
                    /*strTable="BARSAMINFO_TABLE";*/
                    break;
                default:
                    break;
            }
        }

        public string sfc_getDeleteSQLSTR(int t, _SQL_DATA d)
        {
            List<string> arr1 = new List<string>();
            List<string> arr2 = new List<string>();
            sfc_getSQLSTR(t, d, ref arr1, ref arr2);
            return OraDB.getDeleteString(arrTableName[t], arr1.ToArray(), arr2.ToArray());
        }

        public string sfc_getInsertSQLSTR(int t, _SQL_DATA d)
        {
            List<string> arr1 = new List<string>();
            List<string> arr2 = new List<string>();
            sfc_getSQLSTR(t, d, ref arr1, ref arr2);
            return OraDB.getInsertString(arrTableName[t], arr1.ToArray(), arr2.ToArray());
        }

        public string sfc_getInsertSQLSTR(string strTable, _SQL_DATA d)
        {
            int i = 0;
            for (; i < arrTableName.Length; i++)
            {
                if (strTable.ToUpper().Trim() == arrTableName[i].ToUpper().Trim())
                {
                    break;
                }
            }
            return sfc_getInsertSQLSTR(i, d);
        }

        #endregion

        #region "信息串联机"

        public bool auto_checkPartAndBarcode(string strPart, string strBarcode)
        {
            return base.checkSQL2("sfcdata.barautbind",
                new string[,] { { "scpartnum", strPart }, { "scbarcode", strBarcode } });
        }

        public string auto_getPart(string strBarcode)
        {
            string str1 = OraDBBase.getSelectString("sfcdata.barautbind",
                new string[] { "scpartnum" },
                new string[,] { { "scbarcode", strBarcode } });

            str1 += " order by scdate,sctime desc";
            return base.selectSQL2(str1).Tables[0].Rows[0][0].ToString();
        }

        public string auto_getPNLBarcode(string strBarcode)
        {
            string str1 = OraDBBase.getSelectString("sfcdata.barautbind",
                new string[] { "scpnlbar" },
                new string[,] { { "scbarcode", strBarcode } });

            str1 += " order by scdate,sctime desc";
            return base.selectSQL2(str1).Tables[0].Rows[0][0].ToString();
        }

        public string[,] auto_getSubBarcode(string strPNLBarcode)
        {
            string str1 = OraDBBase.getSelectString(/*"sfcdata.barautbind"*/"barautbind",
                new string[] { "scbarcode", "pcsser" },
                new string[,] { { "scpnlbar", strPNLBarcode } });
            str1 += " order by to_number(pcsser) asc";
            return OraDB.ConvertDataSet(base.selectSQL2(str1));
        }

        public string[,] auto_getSubBarcode2(string strSubBarcode)
        {
            string str1 = auto_getPNLBarcode(strSubBarcode);
            return auto_getSubBarcode(str1);
        }

        #endregion

        #region "CR测试检查"

        public bool checkCR(string strBarcode, bool bPASS = true)
        {
            int m = 0;
            string[] arr1;
            if (bPASS)
            {
                arr1 = new string[] { "P", "PASS", "Pass", "pass" };
            }
            else
            {
                arr1 = new string[] { "F", "FAIL", "Fail", "fail" };
            }
            for (int i = 0; i < arr1.Length; i++)
            {
                string str1 = OraDBBase.getSelectString("crdata",
                   new string[] { "barcode", "tres" },
                   new string[] { strBarcode, arr1[i] });
                str1 += " order by sdate,stime desc";
                m += base.checkSQL(str1) ? 1 : 0;
            }
            if (m > 0)
            {
                return true;
            }
            return false;
        }

        #endregion

        #region "ICT测试检查"

        public bool checkICTDATA(string strBarcode, bool bPASS = true)
        {
            int m = 0;
            string[] arr1 = bPASS ? new string[] { "P", "PASS", "Pass", "pass" } :
                new string[] { "F", "FAIL", "Fail", "fail" };
            for (int i = 0; i < arr1.Length; i++)
            {
                string str1 = OraDBBase.getSelectString("ict_data",
                   new string[] { "barcode", "tresult" },
                   new string[] { strBarcode, arr1[i] });
                str1 += " order by itsdate,itstime desc";
                m += base.checkSQL(str1) ? 1 : 0;
            }
            if (m > 0)
            {
                return true;
            }
            return false;
        }

        public bool checkICTTAB(string strBarcode, bool bPASS = true)
        {
            int m = 0;
            string[] arr1 = bPASS ? new string[] { "P", "PASS", "Pass", "pass" } :
                new string[] { "F", "FAIL", "Fail", "fail" };
            for (int i = 0; i < arr1.Length; i++)
            {
                string str1 = OraDBBase.getSelectString("ict_tab",
                   new string[] { "bar_code", "result" },
                   new string[] { strBarcode, arr1[i] });
                str1 += " order by itsdate,itstime desc";
                m += base.checkSQL(str1) ? 1 : 0;
            }
            if (m > 0)
            {
                return true;
            }
            return false;
        }

        #endregion
    }
}