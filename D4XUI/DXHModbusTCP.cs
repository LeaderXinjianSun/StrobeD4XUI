using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Runtime.InteropServices;

namespace DXH.Robot
{
    public class DXHModbusTCP
    {
        #region 通用连接属性，外部可设置

        /// <summary>
        /// 连接用的套接字
        /// </summary>
        public Socket DXHSocket;
        /// <summary>
        /// 要连接的目标远程IP地址
        /// </summary>
        public string RemoteIPAddress = "LocalHost";
        /// <summary>
        /// 要连接的目标远程端口
        /// </summary>
        public int RemoteIPPort = 9000;
        /// <summary>
        /// 绑定的本地端口,0表示不绑定
        /// </summary>
        public int LocalIPPort = 0;
        /// <summary>
        /// 接受超时时间
        /// </summary>
        public int ReceiveTimeout = 1000;
        /// <summary>
        /// 发送超时时间
        /// </summary>
        public int SendTimeout = 1000;
        /// <summary>
        /// 异常断开后自动重连
        /// </summary>
        public bool ReConnect = true;
        /// <summary>
        /// 是否内部打印接受的数据(Console输出)
        /// </summary>
        public bool Print = true;

        #endregion

        #region 内部私有变量

        /// <summary>
        /// 锁定线程用，供TCPSend方法使用，防止收发数据之前互相错乱
        /// </summary>
        private System.Object TCPLock = new System.Object();
        /// <summary>
        /// 锁定线程用，防止判断连接状态时错乱
        /// </summary>
        private System.Object TCPRecLock = new System.Object();

        /// <summary>
        /// 接受缓冲区转成的字符串
        /// </summary>
        private string TCPRecStr = "";

        private string mConnectState = "Idle";
        /// <summary>
        /// 连接状态 Idle Connecting Connected Faulted Closing Closed
        /// </summary>
        private string _ConnectState
        {
            get { return mConnectState; }
            set
            {
                if (mConnectState != value)
                {
                    mConnectState = value;
                    if (ConnectStateChanged != null)
                        ConnectStateChanged(null, mConnectState);
                    if (mConnectState == "Faulted")
                    {//如果连接断开，重连
                        if (ReConnect)
                            StartTCPConnect();
                    }
                    else if (mConnectState == "Connected")
                    {//如果连接成功，接受
                        StartTCPReceive();
                    }
                }
            }
        }

        private bool mModbusState = false;
        /// <summary>
        /// 获取通信状态，true表示TCPSend有回应，false表示TCPSend无回应
        /// </summary>
        private bool _ModbusState
        {
            get { return mModbusState; }
            set
            {
                if (mModbusState != value)
                {
                    mModbusState = value;
                    if (ModbusStateChanged != null)
                        ModbusStateChanged(null, mModbusState);
                    if (mModbusState == false)
                    {

                    }
                }
            }
        }

        #endregion

        #region 事件

        /// <summary>
        /// 接受到一次数据的事件
        /// </summary>
        public event EventHandler<string> Received;

        /// <summary>
        /// 连接状态改变事件
        /// </summary>
        public event EventHandler<string> ConnectStateChanged;

        /// <summary>
        /// 通信状态改变事件
        /// </summary>
        public event EventHandler<bool> ModbusStateChanged;

        #endregion

        #region 外部可读状态

        /// <summary>
        /// 获取连接状态 Idle Connecting Connected Faulted Closing Closed 只读
        /// </summary>
        public string ConnectState
        {
            get { return mConnectState; }
        }
        /// <summary>
        /// 获取通信状态，true表示TCPSend有回应，false表示TCPSend无回应
        /// </summary>
        public bool ModbusState
        {
            get { return mModbusState; }
        }
        #endregion


        #region 功能函数

        /// <summary>
        /// Received事件的小封装
        /// </summary>
        /// <param name="str"></param>
        private void OnReceived(string str)
        {
            if (Received != null)
                Received(this, str);
        }

        /// <summary>
        /// 通过低级操作模式，关闭套接字的保持连接功能，让TCP在异常断开时短时间内尝试重连然后立刻断开，解决了拔网线等连接不断开的问题
        /// </summary>
        /// <param name="KeepAlive"></param>
        /// <param name="KeepAliveTime"></param>
        /// <param name="KeepAliveInterval"></param>
        private void SetKeepAlive(int KeepAlive, int KeepAliveTime, int KeepAliveInterval)
        {
            uint dummy = 0;
            byte[] inOptionValues = new byte[Marshal.SizeOf(dummy) * 3];
            BitConverter.GetBytes((uint)KeepAlive).CopyTo(inOptionValues, 0);
            BitConverter.GetBytes((uint)KeepAliveTime).CopyTo(inOptionValues, Marshal.SizeOf(dummy));
            BitConverter.GetBytes((uint)KeepAliveInterval).CopyTo(inOptionValues, Marshal.SizeOf(dummy) * 2);
            DXHSocket.IOControl(IOControlCode.KeepAliveValues, inOptionValues, null);
        }

        bool HasStartTCPConnect = false;
        /// <summary>
        /// 开始连接，直到连接成功，设置ReConnect会在异常断开连接时主动重连
        /// </summary>
        public async void StartTCPConnect()
        {
            if (!HasStartTCPConnect)
                HasStartTCPConnect = true;
            else
                return;//防止重复执行

            DXHSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);//实例化Socket

            DXHSocket.ReceiveTimeout = ReceiveTimeout;//设置Socket的一些属性
            DXHSocket.SendTimeout = SendTimeout;
            SetKeepAlive(1, 1000, 100);//缩短KeepAlive的时间，让连接异常断开后1秒关闭
            int mTime = 100;//重连的间隔时间，目的是间隔越来越长
            bool TempConnected = false;//临时变量，目的是异步操作后再更新状态，防止一些线程问题 
            while (_ConnectState != "Connected" && _ConnectState != "Closing" && HasStartTCPConnect)//如果已连接或在断开流程中，就退出连接循环
            {
                _ConnectState = "Connecting";//在异步操作之前设置状态为Connecting
                Task Task_Connect = Task.Run(() =>
                {//异步方法
                    try
                    {
                        if (LocalIPPort != 0 && DXHSocket.IsBound == false)//如果本地端口设置不为0，并且Socket端口未绑定，说明需要绑定本地端口
                        {
                            //设置Socket关闭时立即关闭，才会不占用端口，否则关闭连接时需要等待Socket自动关闭，造成一段时间内重连该端口会被上一个套接字占用端口
                            DXHSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.DontLinger, false);//关闭套接字时不占用端口
                            DXHSocket.Bind(new IPEndPoint(System.Net.IPAddress.Any, LocalIPPort));//Socket绑定端口，IP地址也可以设置但意义不大
                        }
                        DXHSocket.Connect(RemoteIPAddress, RemoteIPPort);//开始连接远程服务器，连接失败会直接到异常处理
                        TempConnected = true;//置临时变量连接成功
                        Console.WriteLine("连接成功！");
                    }
                    catch (Exception ex)
                    {//连接失败延迟一会再连接
                        Console.WriteLine("ReConnect:" + ex.Message + ",将在" + mTime + "ms后重试！");
                        Thread.Sleep(mTime);
                        mTime = mTime < 1000 ? mTime + 50 : 1000;
                    }
                });
                await Task_Connect;
                if (TempConnected)
                {//通过临时变量，在异步之后设置状态为Connected
                    _ConnectState = "Connected";
                }
            }

            HasStartTCPConnect = false;

            if (_ConnectState == "Closing")//如果状态在Closing中，设置状态为Closed
                _ConnectState = "Closed";
        }
        /// <summary>
        /// 关闭套接字，断开连接，不会主动重连
        /// </summary>
        public void Close()
        {
            try
            {
                _ConnectState = "Closing";//设置状态为Closing，Socket关闭后会结束一些循环

                DXHSocket.Close();//关闭Socket
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
        bool HasStartTCPReceive = false;
        /// <summary>
        /// 开始结束数据，连接成功后自动开始
        /// </summary>
        private async void StartTCPReceive()
        {
            if (!HasStartTCPReceive)
                HasStartTCPReceive = true;
            else
                return;//防止重复执行

            while (DXHSocket.Connected)//Socket已连接时
            {
                bool TempConnected = true;
                Task TaskRec = Task.Run(() =>
                {
                    lock (TCPRecLock)//锁定线程，防止多个线程同时调用，导致收发顺序错乱
                    {
                        if (DXHSocket.Poll(-1, SelectMode.SelectRead) && DXHSocket.Connected)//等待数据读取
                        {
                            //byte[] Receivedbytes = new byte[1024];
                            try
                            {
                                if (DXHSocket.Available == 0)
                                {//数据长度为0，说明连接断开
                                    TempConnected = false;
                                    Console.WriteLine("bytesRec == 0");
                                }
                            }
                            catch (Exception ex)
                            {//连接异常断开时
                                Console.WriteLine("TCPRecEX:" + ex.Message);
                                TempConnected = false;
                            }
                        }
                        else
                        {//Socket被关闭时
                            Console.WriteLine("POLLFalse");
                            TempConnected = false;
                        }
                    }
                });
                await TaskRec;
                if (TempConnected == false)
                {//连接断开
                    if (_ConnectState != "Closing")//不是自己关闭Socket
                    {
                        DXHSocket.Close();//关闭Socket并置状态为Faulted，会自动尝试重连
                        _ConnectState = "Faulted";
                    }
                    else
                    {//自己关闭Socket，置状态位Closed，不为自动重连
                        _ConnectState = "Closed";
                    }
                }
            }
            HasStartTCPReceive = false;
        }

        int Modbus_Index = 0;

        /// <summary>
        /// ModbusTCP读取
        /// </summary>
        /// <param name="mDevIndex">站号</param>
        /// <param name="mFunction">功能码(读线圈:1,读寄存器:3)</param>
        /// <param name="mDevAdd">地址</param>
        /// <param name="mDataToRead">读取数</param>
        /// <returns></returns>
        public int[] ModbusTCPRead(int mDevIndex, int mFunction, int mDevAdd, int mDataToRead = 1)
        {
            if (_ConnectState != "Connected")
            {
                _ModbusState = false;
                return null;
            }
            lock (TCPLock)
            {
                byte[] mByteToSend = new byte[12];

                //事务元标识符
                Modbus_Index++;
                if (Modbus_Index > 65535)
                    Modbus_Index = 0;
                mByteToSend[0] = Convert.ToByte(Modbus_Index >> 8 & 0xFF);
                mByteToSend[1] = Convert.ToByte(Modbus_Index & 0xFF);

                //协议标识符
                mByteToSend[2] = 0;
                mByteToSend[3] = 0;

                //数据长度
                mByteToSend[4] = 0;
                mByteToSend[5] = 6;
                //站号
                mByteToSend[6] = Convert.ToByte(mDevIndex & 0xFF);
                //功能码
                mByteToSend[7] = (byte)mFunction;
                //起始地址
                mByteToSend[8] = Convert.ToByte(mDevAdd >> 8 & 0xFF);
                mByteToSend[9] = Convert.ToByte(mDevAdd & 0xFF);

                //读取个数
                mByteToSend[10] = Convert.ToByte(mDataToRead >> 8 & 0xFF);
                mByteToSend[11] = Convert.ToByte(mDataToRead & 0xFF);

                int mRecLen = 0;
                if (mFunction == 1)
                {
                    int len = 0;
                    if (mDataToRead % 8 != 0)
                        len = mDataToRead / 8 + 1;
                    else
                        len = mDataToRead / 8;

                    mRecLen = 9 + len;
                }
                else
                    mRecLen = 9 + mDataToRead * 2;

                int mBefore = DXHSocket.Available;
                if (mBefore > 0)
                {
                    byte[] ByteBefore = new byte[mBefore];

                    lock (TCPRecLock)
                    {
                        DXHSocket.Receive(ByteBefore);
                    }
                }

                DXHSocket.Send(mByteToSend);

                int mCount = DXHSocket.Available;
                int mtiemout = 0;
                while (mCount < mRecLen)
                {
                    Thread.Sleep(1);
                    mCount = DXHSocket.Available;
                    mtiemout++;
                    if (mtiemout > 1000 && mCount < mRecLen)
                    {
                        if (mCount > 0)
                        {
                            byte[] ByteEnd = new byte[mCount];

                            lock (TCPRecLock)
                            {
                                DXHSocket.Receive(ByteEnd);
                            }
                        }
                        Debug.Print("RobotRead:" + "Robot没有回应！");
                        _ModbusState = false;
                        return null;
                    }
                }

                byte[] mByteToRead = new byte[mRecLen];
                lock (TCPRecLock)
                {
                    DXHSocket.Receive(mByteToRead);
                }

                if (mByteToRead[0] == Convert.ToByte(Modbus_Index >> 8 & 0xFF) && mByteToRead[1] == Convert.ToByte(Modbus_Index & 0xFF))
                {
                    int[] mData = new int[mDataToRead];
                    for (int i = 0; i < mDataToRead; i++)
                    {
                        if (mFunction == 1)
                        {
                            mData[i] = (mByteToRead[9 + i / 8] >> (i % 8)) & 1;
                        }
                        else
                        {
                            mData[i] = mByteToRead[9 + i * 2] * 256 + mByteToRead[10 + i * 2];
                        }
                    }
                    _ModbusState = true;
                    return mData;
                }
                else
                {
                    _ModbusState = false;
                    return null;
                }



            }
        }
        /// <summary>
        /// Modbus写入
        /// </summary>
        /// <param name="mDevIndex">站号</param>
        /// <param name="mFunction">功能码(写线圈:15,写寄存器:16)</param>
        /// <param name="mDevAdd">地址</param>
        /// <param name="mDataToWrite">写入的数据集合</param>
        /// <returns></returns>
        public bool ModbusTCPWrite(int mDevIndex, int mFunction, int mDevAdd, int[] mDataToWrite)
        {
            if (_ConnectState != "Connected")
            {
                _ModbusState = false;
                return false;
            }
            if (mDataToWrite == null)
            {
                return false;
            }
            lock (TCPLock)
            {

                int len = 0;
                if (mFunction == 0x0F)
                {
                    if (mDataToWrite.Length % 8 != 0)
                        len = mDataToWrite.Length / 8 + 1;
                    else
                        len = mDataToWrite.Length / 8;
                }
                else
                    len = mDataToWrite.Length * 2;

                byte[] mByteToSend = new byte[13 + len];

                //事务元标识符
                Modbus_Index++;
                if (Modbus_Index > 65535)
                    Modbus_Index = 0;
                mByteToSend[0] = Convert.ToByte(Modbus_Index >> 8 & 0xFF);
                mByteToSend[1] = Convert.ToByte(Modbus_Index & 0xFF);

                //协议标识符
                mByteToSend[2] = 0;
                mByteToSend[3] = 0;

                //数据长度
                int mSendLen = 7;

                mSendLen = len + 7;
                mByteToSend[4] = Convert.ToByte(mSendLen >> 8 & 0xFF);
                mByteToSend[5] = Convert.ToByte(mSendLen & 0xFF);
                //站号
                mByteToSend[6] = Convert.ToByte(mDevIndex & 0xFF);
                //功能码
                mByteToSend[7] = (byte)mFunction;
                //起始地址
                mByteToSend[8] = Convert.ToByte(mDevAdd >> 8 & 0xFF);
                mByteToSend[9] = Convert.ToByte(mDevAdd & 0xFF);

                //读取个数
                mByteToSend[10] = Convert.ToByte(mDataToWrite.Length >> 8 & 0xFF);
                mByteToSend[11] = Convert.ToByte(mDataToWrite.Length & 0xFF);

                mByteToSend[12] = Convert.ToByte(len & 0xFF);

                if (mFunction == 0x0F)
                {
                    string mDevData = "";
                    for (int j = 0; j * 8 < mDataToWrite.Length; j++)
                    {
                        string s = "";
                        for (int i = 0; i < 8; i++)
                        {
                            if (i + j * 8 < mDataToWrite.Length)
                            {
                                if (mDataToWrite[i + j * 8] == 0)
                                {
                                    s = "0" + s;
                                }
                                else
                                {
                                    s = "1" + s;
                                }
                            }
                            else
                                s = "0" + s;
                        }
                        mByteToSend[13 + j] = Convert.ToByte(s, 2);
                    }

                }
                else
                {
                    for (int i = 0; i < mDataToWrite.Length; i++)
                    {
                        mByteToSend[2 * i + 13] = Convert.ToByte(mDataToWrite[i] >> 8 & 0xFF);
                        mByteToSend[2 * i + 14] = Convert.ToByte(mDataToWrite[i] & 0xFF);
                    }
                }

                int mRecLen = 12;

                int mBefore = DXHSocket.Available;
                if (mBefore > 0)
                {
                    byte[] ByteBefore = new byte[mBefore];

                    lock (TCPRecLock)
                    {
                        DXHSocket.Receive(ByteBefore);
                    }
                }

                DXHSocket.Send(mByteToSend);

                int mCount = DXHSocket.Available;
                int mtiemout = 0;
                while (mCount < mRecLen)
                {
                    Thread.Sleep(1);
                    mCount = DXHSocket.Available;
                    mtiemout++;
                    if (mtiemout > 1000 && mCount < mRecLen)
                    {
                        if (mCount > 0)
                        {
                            byte[] ByteEnd = new byte[mCount];
                            lock (TCPRecLock)
                            {
                                DXHSocket.Receive(ByteEnd);
                            }

                        }
                        Debug.Print("RobotWrite:" + "Robot没有回应！");
                        _ModbusState = false;
                        return false;
                    }
                }
                byte[] mByteToRead = new byte[mRecLen];
                lock (TCPRecLock)
                {
                    DXHSocket.Receive(mByteToRead);
                }

                if (mByteToRead[0] == Convert.ToByte(Modbus_Index >> 8 & 0xFF) && mByteToRead[1] == Convert.ToByte(Modbus_Index & 0xFF) && mByteToRead[2] == 0 && mByteToRead[3] == 0)
                {
                    _ModbusState = true;
                    return true;
                }
                else
                {
                    _ModbusState = false;
                    return false;
                }
            }
        }

        #endregion
    }
}
