using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.OleDb;
using System.Data;
//using System.Windows.Forms;

namespace 臻鼎科技OraDB
{
    public class OraDBBase
    {
        public OleDbConnection oledbConn = null;
        private string m_server_name;
        private string m_id;
        private string m_pwd;
        private string m_connect_string;
        private bool m_connect_state;

        public string strServerName
        {
            get { return m_server_name; }
            set { m_server_name = value; }
        }

        public string strID
        {
            get { return m_id; }
            set { m_id = value; }
        }

        public string strPWD
        {
            get { return m_pwd; }
            set { m_pwd = value; }
        }

        public string strConnectString
        {
            get { return m_connect_string; }
            set { m_connect_string = value; }
        }

        public bool bConnectState
        {
            get { return m_connect_state; }
            set { m_connect_state = value; }
        }

        public OraDBBase()
        {

        }

        public OraDBBase(string ServerName, string ID, string PWD)
        {
            m_server_name = ServerName;
            m_id = ID;
            m_pwd = PWD;
        }

        #region "公共方法"

        public static string[,] ConvertDataSet(DataSet d)
        {
            int r = d.Tables[0].Rows.Count;
            int c = d.Tables[0].Columns.Count;
            string[,] arr1 = new string[r, c];
            for (int i = 0; i < r; i++)
            {
                for (int j = 0; j < c; j++)
                {
                    arr1[i, j] = d.Tables[0].Rows[i][j].ToString();
                }
            }
            return arr1;
        }

        public static List<string[]> ConvertDataSet2(DataSet d)
        {
            List<string[]> map1 = new List<string[]>();
            int r = d.Tables[0].Rows.Count;
            int c = d.Tables[0].Columns.Count;
            for (int i = 0; i < r; i++)
            {
                string[] arr1 = new string[c];
                for (int j = 0; j < c; j++)
                {
                    arr1[j] = d.Tables[0].Rows[i][j].ToString();
                }
                map1.Add(arr1);
            }
            return map1;
        }

        public string getConnectString()
        {
            if (m_server_name == string.Empty)
            {
                throw new Exception("请设置服务名");
            }
            if (m_id == string.Empty)
            {
                throw new Exception("请设置ID");
            }
            if (m_pwd == string.Empty)
            {
                throw new Exception("请设置PWD");
            }

            strConnectString = "Provider=MSDAORA.1" +
                    ";Data Source=" + m_server_name +
                    ";User Id=" + m_id +
                    ";Password=" + m_pwd +
                    ";Persist Security Info=False";
            return strConnectString;
        }

        public bool getConnectState()
        {
            return m_connect_state;
        }

        public bool connect()
        {
            try
            {
                string str1 = getConnectString();
                if (str1 != null)
                {
                    if (oledbConn == null)
                    {
                        m_connect_state = false;
                        oledbConn = new OleDbConnection(str1);
                        oledbConn.Open();
                        if (oledbConn.State == ConnectionState.Open)
                        {
                            m_connect_state = true;
                        }
                    }
                    else
                    {
                        if (oledbConn.State != ConnectionState.Open)
                        {
                            oledbConn.Open();
                            if (oledbConn.State == ConnectionState.Open)
                            {
                                m_connect_state = true;
                            }
                        }
                    }
                }
                else
                {
                    throw new Exception("无效的连接字符串");
                }
            }
            catch (System.Exception ex)
            {
                throw new Exception(ex.Message);
            }
            return m_connect_state;
        }

        public void disconnect()
        {
            if (oledbConn != null)
            {
                oledbConn.Close();
                oledbConn.Dispose();
                oledbConn = null;
                m_connect_state = false;
            }
        }

        protected int executeNonQuery(string strSQL)
        {
            int res = 0;
            try
            {
                OleDbCommand com = new OleDbCommand(strSQL, oledbConn);
                res = com.ExecuteNonQuery();
                com.Dispose();
            }
            catch (System.Exception ex)
            {
                throw new Exception(ex.Message);
            }
            return res;
        }

        protected DataSet executeQuery(string strSQL)
        {
            DataSet da = new DataSet();
            try
            {
                OleDbDataAdapter sda = new OleDbDataAdapter(strSQL, oledbConn);
                int m = sda.Fill(da);
                sda.Dispose();
            }
            catch (System.Exception ex)
            {
                throw new Exception(ex.Message);
            }
            return da;
        }

        public string[] List2Arr(List<string> list)
        {
            return list.ToArray();
        }

        public List<string> List2Arr(string[] arr)
        {
            return arr.ToList<string>();
        }

        public static string[,] List2Arr(List<string[]> list)
        {
            if (list.Count > 0)
            {
                string[,] arr = new string[list.Count, list[0].Length];
                for (int i = 0; i < list.Count; i++)
                {
                    for (int j = 0; j < list[0].Length; j++)
                    {
                        arr[i, j] = list[i][j];
                    }
                }
                return arr;
            }
            return null;
        }

        public static List<string[]> List2Arr(string[,] arr)
        {
            List<string[]> v = new List<string[]>();
            for (int i = 0; i < arr.GetLength(0); i++)
            {
                string[] arr1 = new string[arr.GetLength(1)];
                for (int j = 0; j < arr.GetLength(1); j++)
                {
                    arr1[j] = arr[i, j];
                }
                v.Add(arr1);
            }
            return v;
        }

        public bool checkSQL(string strTable, string[] arrField, string[] arrValue)
        {
            DataSet Da = selectSQL2(getSelectString(strTable, arrField, arrValue));
            return Da.Tables[0].Rows.Count > 0 ? true : false;
        }

        public bool checkSQL(string strTable, string[,] arrFieldAndValue)
        {
            DataSet Da = selectSQL2(getSelectString(strTable, arrFieldAndValue));
            return Da.Tables[0].Rows.Count > 0 ? true : false;
        }

        public bool checkSQL(string strSQL)
        {
            DataSet Da = selectSQL2(strSQL);
            return Da.Tables[0].Rows.Count > 0 ? true : false;
        }

        //绑定变量
        public bool checkSQL2(string strTable, string[,] arrFieldAndValue)
        {
            bool res = false;
            try
            {
                string str1 = "";
                for (int i = 0; i < arrFieldAndValue.GetLength(0); i++)
                {
                    str1 += string.Format("{0}=? and ", arrFieldAndValue[i, 0], i.ToString());
                }
                string strSQL = string.Format("select count(*) s1 from {0} where {1}",
                    strTable, str1.Substring(0, str1.Length - 5));
                OleDbCommand cmd = new OleDbCommand(strSQL, oledbConn);
                cmd.CommandType = CommandType.Text;
                for (int i = 0; i < arrFieldAndValue.GetLength(0); i++)
                {
                    cmd.Parameters.Add("?", arrFieldAndValue[i, 1]);
                }
                OleDbDataReader rea = cmd.ExecuteReader();
                res = rea.Read();
                rea.Close();
                rea.Dispose();
                cmd.Dispose();
            }
            catch (System.Exception ex)
            {
                throw new Exception(ex.Message);
            }
            return res;
        }

        public string getServerDateTime()
        {
            return selectSQL2("select to_char(sysdate,'yyyy-mm-dd hh:mi:ss') sd from dual").Tables[0].Rows[0][0].ToString();
        }

        #endregion

        #region "插入"

        public static string getInsertString(string strTable, string[,] arrFieldAndValue)
        {
            string strSQL = string.Empty;
            string str1 = "", str2 = "";
            strSQL = "insert into " + strTable + "(";
            for (int i = 0; i < arrFieldAndValue.GetLength(0); i++)
            {
                str1 += string.Format("{0},", arrFieldAndValue[i, 0]);
                str2 += string.Format("'{0}',", arrFieldAndValue[i, 1]);
            }
            strSQL += str1.Substring(0, str1.Length - 1) + ") values (";
            strSQL += str2.Substring(0, str2.Length - 1) + ")";
            return strSQL;
        }

        public static string getInsertString(string strTable, string[] arrField, string[] arrValue)
        {
            string strSQL = string.Empty;
            if (arrField.Length != arrValue.Length)
            {
                throw new Exception("字段的个数和值的个数不一致");
            }
            string str1 = "", str2 = "";
            strSQL = "insert into " + strTable + "(";
            for (int i = 0; i < arrField.Length; i++)
            {
                str1 += string.Format("{0},", arrField[i]);
                str2 += string.Format("'{0}',", arrValue[i]);                
            }
            strSQL += str1.Substring(0, str1.Length - 1) + ") values (";
            strSQL += str2.Substring(0, str2.Length - 1) + ")";
            return strSQL;
        }
        public static string getInsertString1(string strTable, string[] arrField, string[] arrValue)
        {
            string strSQL = string.Empty;
            if (arrField.Length != arrValue.Length)
            {
                throw new Exception("字段的个数和值的个数不一致");
            }
            string str1 = "", str2 = "";
            strSQL = "insert into " + strTable + "(";
            for (int i = 0; i < arrField.Length; i++)
            {

                str1 += string.Format("{0},", arrField[i]);
                if (arrField[i] == "STNUM" || arrField[i] == "UNUM")
                {
                    str2 += string.Format("{0},", arrValue[i]);
                }
                else
                {
                    str2 += string.Format("'{0}',", arrValue[i]);
                }

            }
            strSQL += str1.Substring(0, str1.Length - 1) + ") values (";
            strSQL += str2.Substring(0, str2.Length - 1) + ")";
            return strSQL;
        }
        public string getInsertString(string strTable, string[] arrValue)
        {
            string strSQL = string.Empty;
            string[] arrField = getTableColumns(strTable).ToArray();
            if (arrField.Length != arrValue.Length)
            {
                throw new Exception("字段的个数和值的个数不一致");
            }
            string str1 = "", str2 = "";
            strSQL = "insert into " + strTable + "(";
            for (int i = 0; i < arrField.Length; i++)
            {
                str1 += string.Format("{0},", arrField[i]);
                str2 += string.Format("'{0}',", arrValue[i]);
            }
            strSQL += str1.Substring(0, str1.Length - 1) + ") values (";
            strSQL += str2.Substring(0, str2.Length - 1) + ")";
            return strSQL;
        }

        public static string getInsertString2(string strTable, string[] arrValue)
        {
            string strSQL = string.Empty;
            string str2 = "";
            strSQL = "insert into " + strTable + " values (";
            for (int i = 0; i < arrValue.Length; i++)
            {
                str2 += string.Format("'{0}',", arrValue[i]);
            }
            strSQL += str2.Substring(0, str2.Length - 1) + ")";
            return strSQL;
        }

        public int insertSQL(string strTable, string[,] arrFieldAndValue)
        {
            return executeNonQuery(getInsertString(strTable, arrFieldAndValue));
        }

        public int insertSQL(string strTable, string[] arrField, string[] arrValue)
        {
            return executeNonQuery(getInsertString(strTable, arrField, arrValue));
        }
        public int insertSQL1(string strTable, string[] arrField, string[] arrValue)
        {
            return executeNonQuery(getInsertString1(strTable, arrField, arrValue));
        }

        public int insertSQL(string strTable, string[] arrValue)
        {
            return executeNonQuery(getInsertString2(strTable, arrValue));
        }

        public int insertSQL(string strSQL)
        {
            return executeNonQuery(strSQL);
        }

        #endregion

        #region "查询"

        public static string getSelectString(string strTable, string[] arrField, string[,] arrFieldAndValue)
        {
            string strSQL = string.Empty;
            string str1 = "";
            string str2 = "";
            strSQL = "select ";
            for (int i = 0; i < arrField.Length; i++)
            {
                str2 += string.Format("{0},", arrField[i]);
            }
            strSQL += string.Format("{0} from {1} where ", str2.Substring(0, str2.Length - 1), strTable);
            for (int i = 0; i < arrFieldAndValue.GetLength(0); i++)
            {
                str1 += string.Format("{0}='{1}' and ", arrFieldAndValue[i, 0], arrFieldAndValue[i, 1]);
            }
            strSQL += str1.Substring(0, str1.Length - 5);
            return strSQL;
        }

        public static string getSelectString(string strTable, string[,] arrFieldAndValue)
        {
            string strSQL = string.Empty;
            strSQL = string.Format("select * from {0} where ", strTable);
            string str1 = "";
            for (int i = 0; i < arrFieldAndValue.GetLength(0); i++)
            {
                str1 += string.Format("{0}='{1}' and ", arrFieldAndValue[i, 0], arrFieldAndValue[i, 1]);
            }
            strSQL += str1.Substring(0, str1.Length - 5);
            return strSQL;
        }

        public static string getSelectString(string strTable, string[] arrField, string[] arrValue)
        {
            string strSQL = string.Empty;
            if (arrField.Length != arrValue.Length)
            {
                throw new Exception("字段的个数和值的个数不一致");
            }
            strSQL = string.Format("select * from {0} where ", strTable);
            string str1 = "";
            for (int i = 0; i < arrField.Length; i++)
            {
                str1 += string.Format("{0}='{1}' and ", arrField[i], arrValue[i]);
            }
            strSQL += str1.Substring(0, str1.Length - 5);
            return strSQL;
        }
        public static string getSelectStringwithOrder(string strTable, string[] arrField, string[] arrValue)
        {
            string strSQL = string.Empty;
            if (arrField.Length != arrValue.Length)
            {
                throw new Exception("字段的个数和值的个数不一致");
            }
            strSQL = string.Format("select * from {0} where ", strTable);
            string str1 = "";
            for (int i = 0; i < arrField.Length; i++)
            {
                str1 += string.Format("{0}='{1}' and ", arrField[i], arrValue[i]);
            }
            strSQL += str1.Substring(0, str1.Length - 5);
            strSQL += " ORDER BY SDATE DESC, STIME DESC";
            return strSQL;
        }

        public static string getSelectString(string strTable, string[] arrField)
        {
            string strSQL = string.Empty;
            string str1 = "";
            strSQL = "select ";
            for (int i = 0; i < arrField.Length; i++)
            {
                str1 += string.Format("{0},", arrField[i]);
            }
            strSQL += str1.Substring(0, str1.Length - 1) + " from " + strTable;
            return strSQL;
        }

        public string getSelectString(string strTable)
        {
            return string.Format("select * from {0}", strTable);
        }

        public DataSet selectSQL(string strTable, string[] arrField, string[,] arrFieldAndValue)
        {
            return executeQuery(getSelectString(strTable, arrField, arrFieldAndValue));
        }

        public DataSet selectSQL(string strTable, string[,] arrFieldAndValue)
        {
            return executeQuery(getSelectString(strTable, arrFieldAndValue));
        }

        public DataSet selectSQL(string strTable, string[] arrField, string[] arrValue)
        {
            return executeQuery(getSelectString(strTable, arrField, arrValue));
        }
        public DataSet selectSQLwithOrder(string strTable, string[] arrField, string[] arrValue)
        {
            return executeQuery(getSelectStringwithOrder(strTable, arrField, arrValue));
        }

        public DataSet selectSQL(string strTable, string[] arrField)
        {
            return executeQuery(getSelectString(strTable, arrField));
        }

        public DataSet selectSQL(string strTable)
        {
            return executeQuery(getSelectString(strTable));
        }

        public DataSet selectSQL2(string strSQL)
        {
            return executeQuery(strSQL);
        }
        #endregion

        #region "更新"

        public static string getUpdateString(string strTable, string[,] arrFieldAndNewValue, string[,] arrFieldAndOldValue)
        {
            string strSQL = string.Empty;
            string str1 = "", str2 = "";
            str1 = "update " + strTable + " set ";
            for (int i = 0; i < arrFieldAndNewValue.GetLength(0); i++)
            {
                str1 += string.Format("{0}= '{1}',", arrFieldAndNewValue[i, 0], arrFieldAndNewValue[i, 1]);
            }

            str2 = " where ";
            for (int i = 0; i < arrFieldAndOldValue.GetLength(0); i++)
            {
                str2 += string.Format("{0}= '{1}' and ", arrFieldAndOldValue[i, 0], arrFieldAndOldValue[i, 1]);
            }

            strSQL += str1.Substring(0, str1.Length - 1);
            strSQL += str2.Substring(0, str2.Length - 5);
            return strSQL;
        }
        public static string getUpdateString1(string strTable, string[,] arrFieldAndNewValue, string[,] arrFieldAndOldValue)
        {
            string strSQL = string.Empty;
            string str1 = "", str2 = "";
            str1 = "update " + strTable + " set ";
            for (int i = 0; i < arrFieldAndNewValue.GetLength(0); i++)
            {
                if (arrFieldAndNewValue[i, 0] == "STNUM" || arrFieldAndNewValue[i, 0] == "UNUM")
                {
                    str1 += string.Format("{0}= {1},", arrFieldAndNewValue[i, 0], arrFieldAndNewValue[i, 1]);
                }
                else
                {
                    str1 += string.Format("{0}= '{1}',", arrFieldAndNewValue[i, 0], arrFieldAndNewValue[i, 1]);
                }
                
            }

            str2 = " where ";
            for (int i = 0; i < arrFieldAndOldValue.GetLength(0); i++)
            {
                str2 += string.Format("{0}= '{1}' and ", arrFieldAndOldValue[i, 0], arrFieldAndOldValue[i, 1]);
            }

            strSQL += str1.Substring(0, str1.Length - 1);
            strSQL += str2.Substring(0, str2.Length - 5);
            return strSQL;
        }
        public static string getUpdateString2(string strTable, string[,] arrFieldAndNewValue, string[,] arrFieldAndOldValue)
        {
            string strSQL = string.Empty;
            string str1 = "", str2 = "";
            str1 = "update " + strTable + " set ";
            for (int i = 0; i < arrFieldAndNewValue.GetLength(0); i++)
            {
                if (arrFieldAndNewValue[i, 0] == "BLDATE")
                {
                    str1 += string.Format("{0}= {1},", arrFieldAndNewValue[i, 0], arrFieldAndNewValue[i, 1]);
                }
                else
                {
                    str1 += string.Format("{0}= '{1}',", arrFieldAndNewValue[i, 0], arrFieldAndNewValue[i, 1]);
                }
                
            }

            str2 = " where ";
            for (int i = 0; i < arrFieldAndOldValue.GetLength(0); i++)
            {
                str2 += string.Format("{0}= '{1}' and ", arrFieldAndOldValue[i, 0], arrFieldAndOldValue[i, 1]);
            }

            strSQL += str1.Substring(0, str1.Length - 1);
            strSQL += str2.Substring(0, str2.Length - 5);
            return strSQL;
        }

        public static string getUpdateString(string strTable, string[,] arrFieldAndNewValue)
        {
            string strSQL = string.Empty;
            string str1 = "", str2 = "";
            str1 = "update " + strTable + " set ";
            for (int i = 0; i < arrFieldAndNewValue.GetLength(0); i++)
            {
                str1 += string.Format("{0}= '{1}',", arrFieldAndNewValue[i, 0], arrFieldAndNewValue[i, 1]);
            }
            strSQL += str1.Substring(0, str1.Length - 1);
            return strSQL;
        }

        public static string getUpdateString(string strTable, string strField, string strValue)
        {
            return string.Format("update {0} set {1}='{2}'", strTable, strField, strValue);
        }

        public int updateSQL(string strTable, string strField, string strValue)
        {
            return executeNonQuery(getUpdateString(strTable, strField, strValue));
        }

        public int updateSQL(string strTable, string[,] arrFieldAndNewValue, string[,] arrFieldAndOldValue)
        {
            return executeNonQuery(getUpdateString(strTable, arrFieldAndNewValue, arrFieldAndOldValue));
        }
        public int updateSQL1(string strTable, string[,] arrFieldAndNewValue, string[,] arrFieldAndOldValue)
        {
            return executeNonQuery(getUpdateString1(strTable, arrFieldAndNewValue, arrFieldAndOldValue));
        }
        public int updateSQL2(string strTable, string[,] arrFieldAndNewValue, string[,] arrFieldAndOldValue)
        {
            return executeNonQuery(getUpdateString2(strTable, arrFieldAndNewValue, arrFieldAndOldValue));
        }

        public int updateSQL(string strTable, string[,] arrFieldAndNewValue)
        {
            return executeNonQuery(getUpdateString(strTable, arrFieldAndNewValue));
        }

        public int updateSQL(string strSQL)
        {
            return executeNonQuery(strSQL);
        }

        #endregion

        #region "删除"

        public static string getDeleteString(string strTable, string strField, string strValue)
        {
            return string.Format("delete from {0} where {1}='{2}'", strTable, strField, strValue);
        }

        public static string getDeleteString(string strTable, string[] arrField, string[] arrValue)
        {
            if (arrField.Length != arrValue.Length)
            {
                throw new Exception("字段的个数和值的个数不一致");
            }
            string strSQL = string.Empty;
            string str1 = "";
            str1 = "delete from " + strTable + " where ";
            for (int i = 0; i < arrField.Length; i++)
            {
                str1 += string.Format("{0}= '{1}' and ", arrField[i], arrValue[i]);
            }
            strSQL += str1.Substring(0, str1.Length - 5);
            return strSQL;
        }

        public static string getDeleteString(string strTable, string[,] arrFieldAndValue)
        {
            string strSQL = string.Empty;
            string str1 = "";
            str1 = "delete from " + strTable + " where ";
            for (int i = 0; i < arrFieldAndValue.GetLength(0); i++)
            {
                str1 += string.Format("{0}= '{1}' and ", arrFieldAndValue[i, 0], arrFieldAndValue[i, 1]);
            }
            strSQL += str1.Substring(0, str1.Length - 5);
            return strSQL;
        }

        public string getDeleteString(string strTable)
        {
            return string.Format("delete from {0}", strTable);
        }

        public int deleteSQL(string strTable, string strField, string strValue)
        {
            return executeNonQuery(getDeleteString(strTable, strField, strValue));
        }

        public int deleteSQL(string strTable, string[] arrField, string[] arrValue)
        {
            return executeNonQuery(getDeleteString(strTable, arrField, arrValue));
        }

        public int deleteSQL(string strTable, string[,] arrFieldAndValue)
        {
            return executeNonQuery(getDeleteString(strTable, arrFieldAndValue));
        }

        public int deleteSQL(string strTable)
        {
            return executeNonQuery(getDeleteString(strTable));
        }

        #endregion

        #region "创建表"

        public static string getCreateString(string strTable, string[,] FieldAndType)
        {
            string strSQL = string.Empty;
            string str1 = "";
            strSQL = "create table " + strTable + "(";
            for (int i = 0; i < FieldAndType.GetLength(0); i++)
            {
                str1 += string.Format("{0} {1},", FieldAndType[i, 0], FieldAndType[i, 1]);
            }
            strSQL += str1.Substring(0, str1.Length - 1) + ")";
            return strSQL;
        }

        public void createSQL(string strTable, string[,] arrFieldAndType)
        {
            executeNonQuery(getCreateString(strTable, arrFieldAndType));
        }

        public void createSQL(string strSQL)
        {
            executeNonQuery(strSQL);
        }

        #endregion

        #region "删除表"

        public string getDropString(string strTable)
        {
            return string.Format("drop table {0}", strTable);
        }

        public void dropSQL(string strTable)
        {
            executeNonQuery(getDropString(strTable));
        }

        #endregion

        #region "表名和字段"

        public List<string> getTables()
        {
            List<string> arr1 = new List<string>();
            if (oledbConn != null)
            {
                OleDbCommand cmd1 = new OleDbCommand("select TABLE_NAME from USER_TABLES order by TABLE_NAME", oledbConn);
                OleDbDataReader rd1 = cmd1.ExecuteReader();
                while (rd1.Read())
                {
                    for (int i = 0; i < rd1.FieldCount; i++)
                    {
                        arr1.Add(rd1[i].ToString());
                    }
                }
                rd1.Close();
                rd1.Dispose();
                cmd1.Dispose();
            }
            return arr1;
        }

        public List<string> getTableColumns(string strTable)
        {
            List<string> arr1 = new List<string>();
            try
            {
                if (oledbConn != null)
                {
                    OleDbCommand cmd1 = new OleDbCommand(string.Format("select t.COLUMN_NAME from USER_TAB_COLUMNS t where t.TABLE_NAME='{0}'",
                        strTable.ToUpper()), oledbConn);
                    OleDbDataReader rd1 = cmd1.ExecuteReader();
                    while (rd1.Read())
                    {
                        for (int i = 0; i < rd1.FieldCount; i++)
                        {
                            Console.WriteLine(rd1[i].ToString());
                            arr1.Add(rd1[i].ToString());
                        }
                    }
                    rd1.Close();
                    rd1.Dispose();
                    cmd1.Dispose();
                }
            }
            catch (System.Exception ex)
            {

            }
            finally
            {
            }
            return arr1;
        }

        #endregion

    }
}