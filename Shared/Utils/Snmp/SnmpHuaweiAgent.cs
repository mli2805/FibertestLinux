using SnmpSharpNet;

namespace Fibertest.Utils.Snmp
{
    public class SnmpHuaweiAgent
    {
        private readonly SnmpAgent _snmpAgent;
        private const string HuaweiOid = "1.3.6.1.4.1.2011.2.248";
        // MGTS huawei
        // private const string HuaweiOid2 = "1.3.6.1.4.1.2011.2.80.8";

        public SnmpHuaweiAgent(SnmpAgent snmpAgent)
        {
            _snmpAgent = snmpAgent;
        }

        public bool SendV2CPonTestTrap(DateTime systemStartTime, int slotPosition, int interfaceNumber, int trapNumber)
        {
            var trapData = CreateOltTrap("192.168.96.59", slotPosition, interfaceNumber, trapNumber);
            var upTime = (uint)(DateTime.Now - systemStartTime).TotalSeconds * 100; // Huawei OLT sends UpTime in 0,1sec,
            return _snmpAgent.SendSnmpV2CTrap(trapData, upTime, new Oid(HuaweiOid));
        }


        private VbCollection CreateOltTrap(string oltIp, int slotPosition, int interfaceNumber, int trapNumber)
        {
            var data = new List<Tuple<string, string, SnmpV2CDataType>>();

            data.Add(new Tuple<string, string, SnmpV2CDataType>(HuaweiOid, "0", SnmpV2CDataType.Integer32));
            data.Add(new Tuple<string, string, SnmpV2CDataType>(HuaweiOid, "4", SnmpV2CDataType.Integer32));
            data.Add(new Tuple<string, string, SnmpV2CDataType>(HuaweiOid, "3", SnmpV2CDataType.Integer32));
            data.Add(new Tuple<string, string, SnmpV2CDataType>(HuaweiOid, oltIp, SnmpV2CDataType.IpAddress));
            data.Add(new Tuple<string, string, SnmpV2CDataType>(HuaweiOid, "416", SnmpV2CDataType.Integer32));
            data.Add(new Tuple<string, string, SnmpV2CDataType>(HuaweiOid, "0", SnmpV2CDataType.Integer32));
            data.Add(new Tuple<string, string, SnmpV2CDataType>(HuaweiOid, slotPosition.ToString(), SnmpV2CDataType.Integer32));
            data.Add(new Tuple<string, string, SnmpV2CDataType>(HuaweiOid, interfaceNumber.ToString(), SnmpV2CDataType.Integer32));

            // data.Add(new Tuple<string, string, SnmpV2CDataType>(HuaweiOid, "0", SnmpV2CDataType.Integer32));
            data.Add(new Tuple<string, string, SnmpV2CDataType>(HuaweiOid, (trapNumber + 567000).ToString(), SnmpV2CDataType.Integer32));

            data.Add(new Tuple<string, string, SnmpV2CDataType>(HuaweiOid, "00000000", SnmpV2CDataType.OctetString));
            data.Add(new Tuple<string, string, SnmpV2CDataType>(HuaweiOid, "2", SnmpV2CDataType.Integer32));
            data.Add(new Tuple<string, string, SnmpV2CDataType>(HuaweiOid, "2", SnmpV2CDataType.Integer32));
            data.Add(new Tuple<string, string, SnmpV2CDataType>(HuaweiOid, DateTime.Now.ToString("O"), SnmpV2CDataType.OctetString));

            return new VbCollection(VbCollectionFactory.CreateCollection(data));
        }
    }
}