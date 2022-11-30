using Fibertest.Dto;
using Fibertest.Utils;

namespace Fibertest.Rtu
{
    public partial class Charon
    {
        public bool DetachOtauFromPort(int fromOpticalPort)
        {
            _logger.Log(LogLevel.Information, Logs.RtuManager.ToInt(), $"Detach from port {fromOpticalPort} requested...");
            var extPorts = GetExtendedPorts();
            if (extPorts == null)
                return false;
            if (LastAnswer.Substring(0, 15) == "ERROR_COMMAND\r\n")
            {
                return true;
            }
            if (!extPorts.ContainsKey(fromOpticalPort))
            {
                LastErrorMessage = "There is no such extended port. Nothing to do.";
                _logger.Log(LogLevel.Information, Logs.RtuManager.ToInt(), LastErrorMessage, 2);
                return true;
            }

            extPorts.Remove(fromOpticalPort);
            var content = DictionaryToContent(extPorts);
            SendWriteIniCommand(content);

            if (IsLastCommandSuccessful)
            {
                var child = Children[fromOpticalPort];
                FullPortCount -= child.FullPortCount;
                Children.Remove(fromOpticalPort);
            }
            return IsLastCommandSuccessful;
        }

        public void RewriteIni(Dictionary<int, NetAddress> extPorts)
        {
            var content = DictionaryToContent(extPorts);
            SendWriteIniCommand(content);
        }

        public Charon? AttachOtauToPort(NetAddress additionalOtauAddress, int toOpticalPort)
        {
            _logger.Log(LogLevel.Information, Logs.RtuManager.ToInt(), $"Attach {additionalOtauAddress.ToStringA()} to port {toOpticalPort} requested...");
            if (!ValidateAttachCommand(additionalOtauAddress, toOpticalPort))
                return null;
            var extPorts = GetExtendedPorts();
            if (extPorts == null) // read charon ini file error
            {
                return null;
            }
            if (LastAnswer.Substring(0, 15) == "ERROR_COMMAND\r\n")
            {
                return null;
            }
            if (extPorts.ContainsKey(toOpticalPort))
            {
                LastErrorMessage = "This is extended port already. Denied.";
                _logger.Log(LogLevel.Information, Logs.RtuManager.ToInt(), LastErrorMessage, 2);
                return null;
            }

            _logger.Log(LogLevel.Information, Logs.RtuManager.ToInt(), $"Check connection with OTAU {additionalOtauAddress.ToStringA()}");
            var child = new Charon(additionalOtauAddress, false, _config, _logger, _serialPort);
            if (child.InitializeOtauRecursively() != null)
            {
                return null;
            }

            extPorts.Add(toOpticalPort, additionalOtauAddress);
            var content = DictionaryToContent(extPorts);
            SendWriteIniCommand(content);

            if (IsLastCommandSuccessful)
            {
                Children.Add(toOpticalPort, child);
                FullPortCount += child.FullPortCount;
            }

            return child;
        }

        private bool ValidateAttachCommand(NetAddress additionalOtauAddress, int toOpticalPort)
        {
            if (toOpticalPort < 1 || toOpticalPort > OwnPortCount)
            {
                LastErrorMessage = $"Optical port number should be from 1 to {OwnPortCount}";
                _logger.Log(LogLevel.Information, Logs.RtuManager.ToInt(), LastErrorMessage, 2);
                IsLastCommandSuccessful = false;
                return false;
            }

            if (!additionalOtauAddress.HasValidTcpPort())
            {
                LastErrorMessage = "Tcp port number should be from 1 to 65355";
                _logger.Log(LogLevel.Information, Logs.RtuManager.ToInt(), LastErrorMessage, 2);
                IsLastCommandSuccessful = false;
                return false;
            }

            if (!additionalOtauAddress.HasValidIp4Address())
            {
                LastErrorMessage = "Invalid ip address";
                _logger.Log(LogLevel.Information, Logs.RtuManager.ToInt(), LastErrorMessage, 2);
                IsLastCommandSuccessful = false;
                return false;
            }

            return true;
        }
    }
}