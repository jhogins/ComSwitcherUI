using System;
using System.IO.Ports;
using System.Threading;

public class SwitcherPort
{
    static string PortName = "COM3";
    static int BaudRate = 9600;
    static int DataBits = 8;
    static StopBits StopBits = StopBits.One;
    static Parity Parity = Parity.None;
    static Handshake Handshake = Handshake.None;

    SerialPort serialPort = new SerialPort();
    Thread readThread;
    private string lastMessage;

    public delegate void DgMessageReceived(string message);
    public event DgMessageReceived OnMessageReceived;

    public bool IsConnected => serialPort.IsOpen;
    
    public SwitcherPort()
    {
        serialPort.PortName = PortName;
        serialPort.BaudRate = BaudRate;
        serialPort.Parity = Parity;
        serialPort.DataBits = DataBits;
        serialPort.StopBits = StopBits;
        serialPort.Handshake = Handshake;

        readThread = new Thread(Read);
    }

    public void Open()
    {
        serialPort.Open();
        readThread.Start();
    }

    static string cmdSwitchToChannel = "{0}!";
    public void SwitchToChannel(int channel)
    {
        serialPort.Write(string.Format(cmdSwitchToChannel, channel));
    }

    private void Read()
    {
        while (serialPort.IsOpen)
        {
            try
            {
                lastMessage = serialPort.ReadLine();
                OnMessageReceived(lastMessage);
            }
            catch (TimeoutException) { }
        }
    }
}