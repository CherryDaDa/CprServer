using System;
using System.Diagnostics;
using System.IO.Ports;
using System.Threading;
using SensorSever;

class Pro {





    public static void Main(String[] args)
    {
        // Pro pro = new Pro();
        // pro.Start();
        // pro.ReceiveThreadfunction();
        try
        {
            ServerConnect serverConnect = new ServerConnect();
            serverConnect.StartServer();
            // 你的主程序逻辑
        }
        catch (Exception ex)
        {
            File.WriteAllText("error_log.txt", ex.ToString());
            throw;
        }

    }
    private SerialPort sp;
    private Thread receiveThread;//用于接收消息的线程
    public string portName = "COM5";//串口名，根据Arduino中显示的串口写
    public int baudRate = 9600;//波特率 基本都是9600
    public Parity parity = Parity.None;//校验位
    public int dateBits = 8;//数据位
    public StopBits stopBits = StopBits.One;//停止位

    void Start()
    {
        OpenPort();
        receiveThread = new Thread(ReceiveThreadfunction);
        receiveThread.IsBackground = true;
        receiveThread.Start();
    }

    public void OpenPort()
    {
        //创建串口
        sp = new SerialPort(portName, baudRate, parity, dateBits, stopBits);
        sp.ReadTimeout = 400;
        try
        {
            sp.Open();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }

    public void OnApplicationQuit()
    {
        //关闭串口
        ClosePort();
    }

    public void ClosePort()
    {
        try
        {
            sp.Close();
            receiveThread.Abort();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }


    public void ReceiveThreadfunction()
    {
        //Console.WriteLine("HelloWorld");//打印
        while (true)
        {
            if (this.sp != null && this.sp.IsOpen)//检测端口是否开启
            {
                try
                {
                    String strRec = sp.ReadLine();
                    //Debug.Log("The number is :" + strRec);
                    if (strRec != null)
                    {
                        int index = Convert.ToInt32(strRec);
                        Console.WriteLine(index);
                        //if (index == 90)//温湿度传感器
                        //{
                        //    AirSlake.Awake = true;
                        //}
                        //if (index == 0)//机械按键
                        //{
                        //    Button.Button0 = true;
                        //}
                        //if (index == 1)
                        //{
                        //    Button.Button1 = true;
                        //    Button.Button2 = true;
                        //}
                    }
                }
                catch
                {

                }
            }
        }
    }
}