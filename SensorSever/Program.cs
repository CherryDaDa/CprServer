using System;
using System.Diagnostics;
using System.IO.Ports;
using System.Threading;
using SensorSever;

class Pro {

    private SerialPort sp;
    public string portName = "COM5"; // 串口名，根据Arduino中显示的串口写
    public int baudRate = 9600; // 波特率 基本都是9600
    public Parity parity = Parity.None; // 校验位
    public int dataBits = 8; // 数据位
    public StopBits stopBits = StopBits.One; // 停止位
    public ServerConnect serverConnect;


    public static void Main(String[] args)
    {
        try
        {
            Pro pro = new Pro();
            pro.Start();
            pro.serverConnect = new ServerConnect();
            pro.serverConnect.StartServer();
            // 你的主程序逻辑
            
          
            Console.ReadLine(); // 保持程序运行
        }
        catch (Exception ex)
        {
            File.WriteAllText("error_log.txt", ex.ToString());
            throw;
        }

    }

    void Start()
    {
        OpenPort();
    }

    public void OpenPort()
    {
        // 创建串口
        sp = new SerialPort(portName, baudRate, parity, dataBits, stopBits);
        sp.ReadTimeout = 100; // 减少读取超时时间
        sp.DataReceived += new SerialDataReceivedEventHandler(DataReceivedHandler); // 注册数据接收事件
        try
        {
            sp.Open();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }

    //public void OnApplicationQuit()
    //{
    //    //关闭串口
    //    ClosePort();
    //}

    //public void ClosePort()
    //{
    //    try
    //    {
    //        sp.Close();
    //    }
    //    catch (Exception ex)
    //    {
    //        Console.WriteLine(ex.Message);
    //    }
    //}


    private void DataReceivedHandler(object sender, SerialDataReceivedEventArgs e)
    {
        SerialPort sp = (SerialPort)sender;
        try
        {
            String strRec = sp.ReadLine();
            if (strRec != null)
            {
                int index = Convert.ToInt32(strRec.Trim());
                    Console.WriteLine($"{index}");
                    serverConnect.SensorReceiveData($"{index}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }
}