namespace Fibertest.Dto;

public class NetAddress
{
    public string Ip4Address; // 172.35.98.128
    public string HostName; // domain.beltelecom.by 
    public int Port;

    public bool IsAddressSetAsIp;


    public NetAddress()
    {
        Ip4Address = @"0.0.0.0";
        HostName = "";
        Port = -1;
        IsAddressSetAsIp = true;
    }

    public NetAddress(string ip, int port)
    {
        Ip4Address = ip;
        HostName = "";
        Port = port;
        IsAddressSetAsIp = true;
    }

    public NetAddress(string ip, TcpPorts port)
    {
        Ip4Address = ip;
        HostName = "";
        Port = (int)port;
        IsAddressSetAsIp = true;
    }

    public bool Equals(NetAddress other)
    {
        if (IsAddressSetAsIp != other.IsAddressSetAsIp)
            return false;
        var isAddressEqual = IsAddressSetAsIp
            ? string.Equals(Ip4Address, other.Ip4Address)
            : string.Equals(HostName, other.HostName);
        return isAddressEqual && Port == other.Port;
    }

    public string GetAddress()
    {
        return IsAddressSetAsIp ? Ip4Address : HostName;
    }

    public string ToStringASpace => IsAddressSetAsIp ? $@"{Ip4Address} : {Port}" : $@"{HostName} : {Port}";
    public string ToStringA() => IsAddressSetAsIp ? $@"{Ip4Address}:{Port}" : $@"{HostName}:{Port}";

    public static bool IsValidIp4(string str)
    {
        var parts = str.Split('.');
        if (parts.Length != 4)
            return false;

        for (var i = 0; i < 3; i++)
        {
            int part;
            if (!int.TryParse(parts[i], out part))
                return false;
            if (part < 0 || part > 255)
                return false;
        }

        return true;
    }

    public bool HasValidIp4Address()
    {
        return IsValidIp4(Ip4Address);
    }

    public bool HasValidTcpPort()
    {
        return Port > 0 && Port < 65356;
    }

    public NetAddress Clone()
    {
        return (NetAddress)MemberwiseClone();
    }
}