using System.Net.Sockets;
using System.Text;

namespace Fibertest.Utils.Recovery;

//https://wiki.mikrotik.com/wiki/API_in_C_Sharp
public class Mikrotik
{
    readonly Stream? _stream;
    readonly TcpClient _tcpClient;

    public bool IsAvailable => _stream != null;

        
    public Mikrotik(string ip, int connectionTimeout)
    {
        _tcpClient = new TcpClient();
        var connection = _tcpClient.BeginConnect(ip, 8728, null, null);
        var success = connection.AsyncWaitHandle.WaitOne(TimeSpan.FromSeconds(connectionTimeout));
        _stream = success ? _tcpClient.GetStream() : null;
    }

    public void Close()
    {
        _stream?.Close();
        _tcpClient.Close();
    }
    public bool Login(string username, string password)
    {
        Send("/login", true);
        string hash = Read()[0].Split(new[] { "ret=" }, StringSplitOptions.None)[1];
        Send("/login");
        Send("=name=" + username);
        Send("=response=00" + EncodePassword(password, hash), true);
        return Read()[0] == "!done";
    }

    public void Send(string co, bool endsentence = false)
    {
        byte[] bajty = Encoding.ASCII.GetBytes(co.ToCharArray());
        byte[] velikost = EncodeLength(bajty.Length);

        _stream!.Write(velikost, 0, velikost.Length);
        _stream.Write(bajty, 0, bajty.Length);

        if (endsentence)
            _stream.WriteByte(0);
    }

    public List<string> Read()
    {
        List<string> output = new List<string>();
        string o = "";
        byte[] tmp = new byte[4];
        while (true)
        {
            tmp[3] = (byte)_stream!.ReadByte();
            //if(tmp[3] == 220) tmp[3] = (byte)connection.ReadByte(); it sometimes happend to me that 
            //mikrotik send 220 as some kind of "bonus" between words, this fixed things, not sure about it though
            long count;
            if (tmp[3] == 0)
            {
                output.Add(o);
                if (o.Substring(0, 5) == "!done")
                {
                    break;
                }
                else
                {
                    o = "";
                    continue;
                }
            }
            else
            {
                if (tmp[3] < 0x80)
                {
                    count = tmp[3];
                }
                else
                {
                    if (tmp[3] < 0xC0)
                    {
                        int tmpi = BitConverter.ToInt32(new byte[] { (byte)_stream.ReadByte(), tmp[3], 0, 0 }, 0);
                        count = tmpi ^ 0x8000;
                    }
                    else
                    {
                        if (tmp[3] < 0xE0)
                        {
                            tmp[2] = (byte)_stream.ReadByte();
                            int tmpi = BitConverter.ToInt32(new byte[] { (byte)_stream.ReadByte(), tmp[2], tmp[3], 0 }, 0);
                            count = tmpi ^ 0xC00000;
                        }
                        else
                        {
                            if (tmp[3] < 0xF0)
                            {
                                tmp[2] = (byte)_stream.ReadByte();
                                tmp[1] = (byte)_stream.ReadByte();
                                int tmpi = BitConverter.ToInt32(new[] { (byte)_stream.ReadByte(), tmp[1], tmp[2], tmp[3] }, 0);
                                count = tmpi ^ 0xE0000000;
                            }
                            else
                            {
                                if (tmp[3] == 0xF0)
                                {
                                    tmp[3] = (byte)_stream.ReadByte();
                                    tmp[2] = (byte)_stream.ReadByte();
                                    tmp[1] = (byte)_stream.ReadByte();
                                    tmp[0] = (byte)_stream.ReadByte();
                                    count = BitConverter.ToInt32(tmp, 0);
                                }
                                else
                                {
                                    //Error in packet reception, unknown length
                                    break;
                                }
                            }
                        }
                    }
                }
            }

            for (int i = 0; i < count; i++)
            {
                o += (Char)_stream.ReadByte();
            }
        }
        return output;
    }

    byte[] EncodeLength(int delka)
    {
        if (delka < 0x80)
        {
            byte[] tmp = BitConverter.GetBytes(delka);
            return new[] { tmp[0] };
        }
        if (delka < 0x4000)
        {
            byte[] tmp = BitConverter.GetBytes(delka | 0x8000);
            return new[] { tmp[1], tmp[0] };
        }
        if (delka < 0x200000)
        {
            byte[] tmp = BitConverter.GetBytes(delka | 0xC00000);
            return new[] { tmp[2], tmp[1], tmp[0] };
        }
        if (delka < 0x10000000)
        {
            byte[] tmp = BitConverter.GetBytes(delka | 0xE0000000);
            return new[] { tmp[3], tmp[2], tmp[1], tmp[0] };
        }
        else
        {
            byte[] tmp = BitConverter.GetBytes(delka);
            return new byte[] { 0xF0, tmp[3], tmp[2], tmp[1], tmp[0] };
        }
    }

    private string EncodePassword(string password, string hash)
    {
        byte[] hashByte = new byte[hash.Length / 2];
        for (int i = 0; i <= hash.Length - 2; i += 2)
        {
            hashByte[i / 2] = Byte.Parse(hash.Substring(i, 2), System.Globalization.NumberStyles.HexNumber);
        }
        byte[] heslo = new byte[1 + password.Length + hashByte.Length];
        heslo[0] = 0;
        Encoding.ASCII.GetBytes(password.ToCharArray()).CopyTo(heslo, 1);
        hashByte.CopyTo(heslo, 1 + password.Length);

        var md5 = System.Security.Cryptography.MD5.Create();

        var hotovo = md5.ComputeHash(heslo);

        //Convert encoded bytes back to a 'readable' string
        string navrat = "";
        foreach (byte h in hotovo)
        {
            navrat += h.ToString("x2");
        }
        return navrat;
    }
}