using Fibertest.Dto;

namespace Fibertest.Graph
{
    public static class TceTypeStructExt
    {
        public static TceS CreateTce(this TceTypeStruct tceTypeStruct)
        {
            var tce = new TceS() { TceTypeStruct = tceTypeStruct, ProcessSnmpTraps = true };
            foreach (var slot in tceTypeStruct.SlotPositions)
            {
                tce.Slots.Add(new TceSlot() { Position = slot, GponInterfaceCount = 0 });
            }
            return tce;
        }

        public static IEnumerable<TceTypeStruct> Generate()
        {
            // ===== HUAWEI =================================


            yield return new TceTypeStruct()
            {
                Id = 101,
                IsVisible = true,
                Maker = TceMaker.Huawei,
                Model = @"MA5600T",
                SoftwareVersion = @"MA5600V800R016C10",
                Code = @"Huawei_MA5600T_R016", // investigated 2022-Apr-15
                SlotPositions = new[] { 1, 2, 3, 4, 5, 6, 9, 10, 11, 12, 13, 14, 15, 16 },
                GponInterfaceNumerationFrom = 0,
                Comment = "",
            };

            yield return new TceTypeStruct()
            {
                Id = 102,
                IsVisible = true,
                Maker = TceMaker.Huawei,
                Model = @"MA5600T",
                SoftwareVersion = @"MA5600V800R018C10",
                Code = @"Huawei_MA5600T_R018", // investigated 2022-Apr-12
                SlotPositions = new[] { 1, 2, 3, 4, 5, 6, 9, 10, 11, 12, 13, 14, 15, 16 },
                GponInterfaceNumerationFrom = 0,
                Comment = "",
            };

            yield return new TceTypeStruct()
            {
                Id = 103,
                IsVisible = true,
                Maker = TceMaker.Huawei,
                Model = @"MA5800",
                SoftwareVersion = @"MA5800V100R020C10",  // the same as @"MA5600V800R018C10"
                Code = @"Huawei_MA5600T_R018", // investigated 2022-Nov-15
                SlotPositions = new[] { 1, 2, 3, 4, 5, 6, 9, 10, 11, 12, 13, 14, 15, 16 },
                GponInterfaceNumerationFrom = 0,
                Comment = "",
            };

            yield return new TceTypeStruct()
            {
                Id = 100,
                IsVisible = true,
                Maker = TceMaker.Huawei,
                Model = @"MA5608T",
                SoftwareVersion = @"MA5600V800R013C00",
                Code = @"Huawei_MA5608T_R013", // investigated 2021
                SlotPositions = new[] { 0, 1 },
                GponInterfaceNumerationFrom = 0,
                Comment = "",
            };

            // ===== ZTE ====================================

            yield return new TceTypeStruct()
            {
                Id = 201,
                IsVisible = true,
                Maker = TceMaker.ZTE,
                Model = @"C300 (19″)",
                SoftwareVersion = @"V1.2.5P3",
                Code = @"ZTE_C300_v1", // investigated 2022-Apr-12
                SlotPositions = new[] { 2, 3, 4, 5, 6, 7, 8, 9, 12, 13, 14, 15, 16, 17 },
                GponInterfaceNumerationFrom = 1,
                Comment = "",
            };

            yield return new TceTypeStruct()
            {
                Id = 2011,
                IsVisible = true,
                Maker = TceMaker.ZTE,
                Model = @"C300 (19″)",
                SoftwareVersion = @"V2.1.0", // the same as @"V1.2.5P3"
                Code = @"ZTE_C300_v1", // investigated 2022-Nov-15
                SlotPositions = new[] { 2, 3, 4, 5, 6, 7, 8, 9, 12, 13, 14, 15, 16, 17 },
                GponInterfaceNumerationFrom = 1,
                Comment = "",
            };

            yield return new TceTypeStruct()
            {
                Id = 202,
                IsVisible = true,
                Maker = TceMaker.ZTE,
                Model = @"C300M (19″)",
                SoftwareVersion = @"V4.0.2P2",
                Code = @"ZTE_C300M_v4", // investigated 2022-Apr-12
                SlotPositions = new[] { 2, 3, 4, 5, 6, 7, 8, 9, 12, 13, 14, 15, 16, 17 },
                GponInterfaceNumerationFrom = 1,
                Comment = "",
            };

            yield return new TceTypeStruct()
            {
                Id = 2021,
                IsVisible = true,
                Maker = TceMaker.ZTE,
                Model = @"C300M (19″)",
                SoftwareVersion = @"V4.3P7",
                Code = @"ZTE_C300M_v43", // investigated 2022-Nov-15
                SlotPositions = new[] { 2, 3, 4, 5, 6, 7, 8, 9, 12, 13, 14, 15, 16, 17 },
                GponInterfaceNumerationFrom = 1,
                Comment = "",
            };

            yield return new TceTypeStruct()
            {
                Id = 200,
                IsVisible = true,
                Maker = TceMaker.ZTE,
                Model = @"C320",
                SoftwareVersion = @"V1.2.5P2",
                Code = @"ZTE_C320_v1", // investigated 2021
                SlotPositions = new[] { 1, 2 },
                GponInterfaceNumerationFrom = 1,
                Comment = "",
            };

        }
    }
}
