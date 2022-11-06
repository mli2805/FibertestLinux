namespace Fibertest.Dto
{
    public enum GetOtdrInfo
    {
        ServiceCmdGetOtdrInfoMfid    =  1,
        ServiceCmdGetOtdrInfoMfsn    =  2,
        ServiceCmdGetOtdrInfoOmsn    =  3,
        ServiceCmdGetOtdrInfoOmid    =  4,
    }

    public enum ServiceFunctionCommand
    {
        IitServiceXxx = 700,
        Monitor = 701, //mean the same as ..._POINTS
        GetParam = 702,
        ShowParam = 703,
        SetBase = 704,
        SetParam = 705,
        MonitorPoints = 701, //monitor by points comparison
        MonitorEvents = 701, //monitor by events comparison
        GetBase = 707,
        SetParamFromSor = 708,
        ShowParamLng = 709,
        Auto = 710,
        GetOtdrInfo = 711,
        GetAutoParam = 712,
        SetAutoParam = 713,

        AutoAnalyze = 714,
        DevInformation = 715,
        ApplyParam = 716,
        AutoParamFind = 717,

        SetParamDefaults = 718,
        GetYScale = 719,
        SetYScaleFlag = 720,
        AutoEof = 721,
        RangeView = 722,
        AutoMeasPrm = 723,
        SaveParams = 724,
        LoadParams = 725,
        GetPower = 726,
        ReloadContext = 727,

        GetModulesCount = 728,
        GetModulesName = 729,
        GetModulesVersion = 730,

        GetMeasStepsCount = 731,
        ApplyFilter = 732,
        GetBaseBuffer = 733,
        SetBaseBuffer = 734,
        MonitorBuffer = 735,
        SetParamFromSorBuffer = 736,
        AutoBuffer = 737,
        ApplyFilterBuffer = 738,

        GetParamForLaser = 739,
        Reserved1 = 740,
        LsControl = 741,
        LsPwrTest = 742,
        LsGetParams = 743,
        SetIntermediateSorPointsNum = 744,
        ParamMeasLmaxGet = 745,
        ParamMeasLmaxSet = 746,
        ParamMeasConqGet = 747,
        UnitGet = 748,
        MeasConnParamsAndLmax = 749,
        ObtainLinkScanParams = 750,
        ApplyLinkScanParamI = 751,
        SetPonParamsForAutoParams = 752,
        ParamDevicePortCommand = 753,

        InitializeTemperatureTemplate = 754,
        InitializeTemperatureCurveGenerator = 755,
        GenerateTemperatureCurve = 756,
        CollectTemperatureData = 757,
        InitializeTemperatureAnalyzer = 758,
        TemperatureAnalysis = 759,
        EstimateNumberOfFastAverages = 760,
        EstimateNumberOfDataPoints = 761,
        WaitForHardwareToGetReady = 762,
        SetFastMeasCustomResolution = 763,
        ParamDwdmLaserHeating = 764,
        OtdrUnitTemperature = 765,

        SetParamList                          =766,
        PaidOptionCheck                      =767,
        SorSetLangCode                      =768,
        GetIdLib                             =769,
        GetMeasSteppingMode                 =770,
        SetMeasSteppingMode                 =771,
        LinkScanGetParam                      =772,
        ObtainLinkScanParamsEx              =773,
        PresetSorField                       =774,
        ParamOutSwitchControl               =775,
        LinkScanSetParam                      =776,
        SorSetAutoPrmFlag                   =777,
        TweakLinkScanParams = 778,
        InsertIitEvents = 779,
        Auto2 = 780,
    }
}