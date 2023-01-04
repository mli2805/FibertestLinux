using SnmpSharpNet;

namespace Fibertest.Utils.Snmp
{
    public static class VbCollectionFactory
    {
        public static Vb CreateVb(Tuple<string, string, SnmpV2CDataType> tuple)
        {
            switch (tuple.Item3)
            {
                case SnmpV2CDataType.Oid:
                    return new Vb(new Oid(tuple.Item1), new Oid(tuple.Item2));
                case SnmpV2CDataType.IpAddress:
                    return new Vb(new Oid(tuple.Item1), new IpAddress(tuple.Item2));
                case SnmpV2CDataType.TimeTicks:
                    return new Vb(new Oid(tuple.Item1), new TimeTicks(tuple.Item2));
                case SnmpV2CDataType.OctetString:
                    return new Vb(new Oid(tuple.Item1), new OctetString(tuple.Item2));
                case SnmpV2CDataType.Counter32:
                    return new Vb(new Oid(tuple.Item1), new Counter32(tuple.Item2));
                // ReSharper disable once RedundantCaseLabel
                case SnmpV2CDataType.Integer32:
                default: 
                    return new Vb(new Oid(tuple.Item1), new Integer32(tuple.Item2));
            }
        }

        public static IEnumerable<Vb> CreateCollection(List<Tuple<string, string, SnmpV2CDataType>> data)
        {
            return data.Select(CreateVb);
        }
    }
}