using System.Net;

namespace TCPNetterServerGUI.Server.Params;

public class TcpServerParams
{
    public IPAddress ServerIP { get; set; }

    public int ServerPort { get; set; }

    public int Backlog { get; set; } = 100;
}