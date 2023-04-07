using Autofac;
using Caliburn.Micro;
using Fibertest.Dto;
using Fibertest.Graph;
using Fibertest.GrpcClientLib;
using Fibertest.Utils;
using Fibertest.WpfCommonViews;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;

namespace Fibertest.WpfClient
{
    public sealed class AutofacClient : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            ILoggerFactory loggerFactory = LoggerFactory.Create(b =>
            {
                b.ClearProviders();
                b.AddDebug();  // Debug.WriteLine() - see Output window
                b.AddConsole(); // in WPF does not work !!!
                b.AddSerilog(LoggerConfigurationFactory
                    .Configure(LogEventLevel.Debug) // here is my configuration of log files
                    .CreateLogger());
            });

            Microsoft.Extensions.Logging.ILogger logger = loggerFactory.CreateLogger<ShellViewModel>();
            builder.RegisterInstance(logger).As<Microsoft.Extensions.Logging.ILogger>();

            builder.RegisterType<ShellViewModel>().As<IShell>();
            builder.RegisterType<WindowManager>().As<IWindowManager>().InstancePerLifetimeScope();
            builder.RegisterType<Model>().InstancePerLifetimeScope();

            builder.RegisterType<WaitCursor>().As<IWaitCursor>();

            builder.RegisterType<GrpcC2DService>().InstancePerLifetimeScope();
            builder.RegisterType<GrpcC2RService>().InstancePerLifetimeScope();

            builder.RegisterType<GrpcInClientProcessor>().InstancePerLifetimeScope();
            builder.RegisterType<ClientGrpcServiceHost>().As<IGrpcHost>().InstancePerLifetimeScope();

            builder.RegisterType<WcfServiceInSuperClient>().As<IWcfServiceInSuperClient>().InstancePerLifetimeScope();
            // builder.RegisterType<WcfServiceInClient>().InstancePerLifetimeScope();

            builder.RegisterType<CurrentClientConfiguration>().InstancePerLifetimeScope();
            builder.RegisterType<CurrentUser>().InstancePerLifetimeScope();
            builder.RegisterType<CurrentGis>().InstancePerLifetimeScope();
            builder.RegisterType<DataCenterConfig>().InstancePerLifetimeScope();
            builder.RegisterType<SystemState>().InstancePerLifetimeScope();
            builder.RegisterType<ConfigurationViewModel>().InstancePerLifetimeScope();
            builder.RegisterType<EventToLogLineParser>().InstancePerLifetimeScope();
            builder.RegisterType<EventLogComposer>().InstancePerLifetimeScope();
            builder.RegisterType<LogOperationsViewModel>().InstancePerLifetimeScope();
            builder.RegisterType<EventLogViewModel>().InstancePerLifetimeScope();
            builder.RegisterType<AboutViewModel>().InstancePerLifetimeScope();
            builder.RegisterType<WaitViewModel>().InstancePerLifetimeScope();
            builder.RegisterType<LicenseFromFileDecoder>().InstancePerLifetimeScope();
            builder.RegisterType<LicenseFileChooser>().As<ILicenseFileChooser>().InstancePerLifetimeScope();
            builder.RegisterType<GraphGpsCalculator>().InstancePerLifetimeScope();

            builder.RegisterType<RtuHolder>().As<IRtuHolder>().InstancePerLifetimeScope();
            builder.RegisterType<ReflectogramManager>().InstancePerLifetimeScope();
            builder.RegisterType<TraceStateViewsManager>().InstancePerLifetimeScope();

            builder.RegisterType<OpticalEventsViewModel>();
            builder.RegisterType<OpticalEventsDoubleViewModel>().InstancePerLifetimeScope();
            builder.RegisterType<OpticalEventsExecutor>().InstancePerLifetimeScope();

            builder.RegisterType<NetworkEventsViewModel>();
            builder.RegisterType<NetworkEventsDoubleViewModel>().InstancePerLifetimeScope();

            builder.RegisterType<BopNetworkEventsViewModel>();
            builder.RegisterType<BopNetworkEventsDoubleViewModel>().InstancePerLifetimeScope();

            builder.RegisterType<TabulatorViewModel>().InstancePerLifetimeScope();
            builder.RegisterType<CommonStatusBarViewModel>().InstancePerLifetimeScope();

            builder.RegisterType<BopStateViewModel>();
            builder.RegisterType<RtuStateViewModel>();
            builder.RegisterType<TraceStateViewModel>();
            builder.RegisterType<BaseRefModelFactory>();
            builder.RegisterType<TraceStatisticsViewModel>();
            
            builder.RegisterType<LicenseCommandFactory>().InstancePerLifetimeScope();
            builder.RegisterType<LicenseSender>().InstancePerLifetimeScope();
            builder.RegisterType<LicenseViewModel>().InstancePerLifetimeScope();
            builder.RegisterType<NoLicenseAppliedViewModel>().InstancePerLifetimeScope();
            builder.RegisterType<MainMenuViewModel>().InstancePerLifetimeScope();
            builder.RegisterType<ShellViewModel>().As<IShell>();
            builder.RegisterType<OtauToAttachViewModel>().InstancePerLifetimeScope();
            builder.RegisterType<TraceToAttachViewModel>().InstancePerLifetimeScope();
            builder.RegisterType<RtuUpdateViewModel>();
            builder.RegisterType<UserViewModel>();
            builder.RegisterType<UserListViewModel>();
            builder.RegisterType<ZoneViewModel>();
            builder.RegisterType<ZonesViewModel>();
            builder.RegisterType<ObjectsAsTreeToZonesViewModel>();
            builder.RegisterType<GisSettingsViewModel>();
            builder.RegisterType<GraphVisibilitySettingsViewModel>();
            builder.RegisterType<SmtpSettingsViewModel>();
            builder.RegisterType<SmsSettingsViewModel>();
            builder.RegisterType<SnmpSettingsViewModel>();
            builder.RegisterType<DbOptimizationViewModel>().InstancePerLifetimeScope();
            builder.RegisterType<GraphOptimizationViewModel>().InstancePerLifetimeScope();
            builder.RegisterType<ChangePasswordViewModel>();

            builder.RegisterType<LandmarksIntoBaseSetter>().InstancePerLifetimeScope();
            builder.RegisterType<BaseRefsCheckerOnServer>().InstancePerLifetimeScope();
            builder.RegisterType<BaseRefMessages>().InstancePerLifetimeScope();
            builder.RegisterType<TraceModelBuilder>().InstancePerLifetimeScope();
            builder.RegisterType<BaseRefLandmarksTool>().InstancePerLifetimeScope();
            builder.RegisterType<BaseRefMeasParamsChecker>().InstancePerLifetimeScope();
            builder.RegisterType<BaseRefLandmarksChecker>().InstancePerLifetimeScope();
            builder.RegisterType<BaseRefsAssignViewModel>();
            
            builder.RegisterType<SoundManager>().InstancePerLifetimeScope();
            builder.RegisterType<RtuRemover>().InstancePerLifetimeScope();
            builder.RegisterType<RtuFilterViewModel>().InstancePerLifetimeScope();

            builder.RegisterType<RtuStateModelFactory>().InstancePerLifetimeScope();
            builder.RegisterType<BopStateViewsManager>().InstancePerLifetimeScope();
            builder.RegisterType<RtuChannelViewModel>();
            builder.RegisterType<RtuChannelViewsManager>().InstancePerLifetimeScope();
            builder.RegisterType<RtuStateViewsManager>().InstancePerLifetimeScope();
            builder.RegisterType<AccidentLineModelFactory>().InstancePerLifetimeScope();
            builder.RegisterType<TraceStateModelFactory>().InstancePerLifetimeScope();
            builder.RegisterType<TraceStateViewsManager>().InstancePerLifetimeScope();
            builder.RegisterType<TraceStatisticsViewsManager>().InstancePerLifetimeScope();
            builder.RegisterType<TraceInfoViewModel>();
            builder.RegisterType<StepChoiceViewModel>();
            builder.RegisterType<TraceStepByStepViewModel>().InstancePerLifetimeScope();
            
            builder.RegisterType<GrmEquipmentRequests>().InstancePerLifetimeScope();
            builder.RegisterType<GrmNodeRequests>().InstancePerLifetimeScope();
            builder.RegisterType<GrmFiberWithNodesRequest>().InstancePerLifetimeScope();
            builder.RegisterType<GrmFiberRequests>().InstancePerLifetimeScope();
            builder.RegisterType<GrmRtuRequests>().InstancePerLifetimeScope();

            builder.RegisterType<SorDataParsingReporter>().InstancePerLifetimeScope();
            builder.RegisterType<AccidentsFromSorExtractor>().InstancePerLifetimeScope();
            builder.RegisterType<AccidentPlaceLocator>().InstancePerLifetimeScope();
            builder.RegisterType<NodeEventsOnGraphExecutor>().InstancePerLifetimeScope();
            builder.RegisterType<FiberEventsOnGraphExecutor>().InstancePerLifetimeScope();
            builder.RegisterType<EquipmentEventsOnGraphExecutor>().InstancePerLifetimeScope();
            builder.RegisterType<TraceEventsOnGraphExecutor>().InstancePerLifetimeScope();
            builder.RegisterType<RtuEventsOnGraphExecutor>().InstancePerLifetimeScope();
            builder.RegisterType<AccidentEventsOnGraphExecutor>().InstancePerLifetimeScope();
            builder.RegisterType<ResponsibilityEventsOnGraphExecutor>().InstancePerLifetimeScope();
            builder.RegisterType<EventsOnGraphExecutor>().InstancePerLifetimeScope();
            builder.RegisterType<GraphReadModel>().InstancePerLifetimeScope();

            builder.RegisterType<EchoEventsOnTreeExecutor>().InstancePerLifetimeScope();
            builder.RegisterType<TraceEventsOnTreeExecutor>().InstancePerLifetimeScope();
            builder.RegisterType<InitializeRtuEventOnTreeExecutor>().InstancePerLifetimeScope();
            builder.RegisterType<RtuEventsOnTreeExecutor>().InstancePerLifetimeScope();
            builder.RegisterType<ZoneEventsOnTreeExecutor>().InstancePerLifetimeScope();
            builder.RegisterType<EventsOnTreeExecutor>().InstancePerLifetimeScope();
            
            builder.RegisterType<TreeOfRtuModel>().InstancePerLifetimeScope();
            builder.RegisterType<TreeOfRtuViewModel>().InstancePerLifetimeScope();
            builder.RegisterType<FreePorts>().InstancePerLifetimeScope();
            builder.RegisterType<ChildrenViews>().InstancePerLifetimeScope();
            builder.RegisterType<PortLeaf>();
            builder.RegisterType<OtauLeaf>();
            builder.RegisterType<RtuLeaf>();
            builder.RegisterType<TraceLeaf>();
            builder.RegisterType<RtuLeafActions>().InstancePerLifetimeScope();
            builder.RegisterType<RtuLeafActionsPermissions>().InstancePerLifetimeScope();
            builder.RegisterType<RtuLeafContextMenuProvider>().InstancePerLifetimeScope();
            builder.RegisterType<TraceLeafActions>().InstancePerLifetimeScope();
            builder.RegisterType<TraceLeafActionsPermissions>().InstancePerLifetimeScope();
            builder.RegisterType<TraceLeafContextMenuProvider>().InstancePerLifetimeScope();
            builder.RegisterType<PortLeafActions>().InstancePerLifetimeScope();
            builder.RegisterType<PortLeafContextMenuProvider>().InstancePerLifetimeScope();
            builder.RegisterType<CommonActions>().InstancePerLifetimeScope();

            builder.RegisterType<TraceContentChoiceViewModel>();
            builder.RegisterType<EquipmentOfChoiceModelFactory>().InstancePerLifetimeScope();
            
            builder.RegisterType<UiDispatcherProvider>().As<IDispatcherProvider>().InstancePerLifetimeScope();
            builder.RegisterType<EventArrivalNotifier>().InstancePerLifetimeScope();
            builder.RegisterType<Heartbeater>().InstancePerLifetimeScope();
            builder.RegisterType<ClientPoller>().InstancePerLifetimeScope();

            builder.RegisterType<AddEquipmentIntoNodeBuilder>();
            builder.RegisterType<GpsInputViewModel>();
            builder.RegisterType<GpsInputSmallViewModel>();
            builder.RegisterType<LandmarksGraphParser>();
            builder.RegisterType<LandmarksBaseParser>();
            builder.RegisterType<OneLandmarkViewModel>();
            builder.RegisterType<TraceChoiceViewModel>().InstancePerLifetimeScope();
            builder.RegisterType<LandmarksViewModel>();
            builder.RegisterType<LandmarksViewsManager>().InstancePerLifetimeScope();
            builder.RegisterType<NodeUpdateViewModel>();
            builder.RegisterType<FiberUpdateViewModel>();
            builder.RegisterType<EquipmentInfoViewModel>();

            builder.RegisterType<MonitoringSettingsModelFactory>();
            builder.RegisterType<MonitoringSettingsViewModel>();

            builder.RegisterType<MachineKeyProvider>().As<IMachineKeyProvider>();
            builder.RegisterType<LoginViewModel>();
            builder.RegisterType<ServersConnectViewModel>();
            builder.RegisterType<SecurityAdminConfirmationViewModel>();
            builder.RegisterType<ServerConnectionLostViewModel>();
            builder.RegisterType<NetAddressTestViewModel>();
            builder.RegisterType<NetAddressInputViewModel>();
            builder.RegisterType<RtuAskSerialViewModel>();
            builder.RegisterType<RtuInitializeModel>();
            builder.RegisterType<RtuInitializeViewModel>();
            builder.RegisterType<MeasurementInterrupter>().InstancePerLifetimeScope();
            builder.RegisterType<OtdrParametersThroughServerSetterViewModel>().InstancePerLifetimeScope();
            builder.RegisterType<ClientMeasurementViewModel>().InstancePerLifetimeScope();
            builder.RegisterType<OutOfTurnPreciseMeasurementViewModel>().InstancePerLifetimeScope();
            builder.RegisterType<MeasurementAsBaseAssigner>().InstancePerLifetimeScope();
            builder.RegisterType<AutoAnalysisParamsViewModel>();
            builder.RegisterType<VeexMeasurementTool>().InstancePerLifetimeScope();
            builder.RegisterType<OneIitMeasurementExecutor>().InstancePerLifetimeScope();
            builder.RegisterType<OneVeexMeasurementExecutor>().InstancePerLifetimeScope();
            builder.RegisterType<AutoBaseViewModel>().InstancePerLifetimeScope();
            builder.RegisterType<FailedAutoBasePdfProvider>().InstancePerLifetimeScope();
            builder.RegisterType<WholeIitRtuMeasurementsExecutor>().InstancePerLifetimeScope();
            builder.RegisterType<WholeVeexRtuMeasurementsExecutor>().InstancePerLifetimeScope();
            builder.RegisterType<RtuAutoBaseViewModel>().InstancePerLifetimeScope();

            builder.RegisterType<NodeVmActions>().InstancePerLifetimeScope();
            builder.RegisterType<CommonVmActions>().InstancePerLifetimeScope();
            builder.RegisterType<NodeVmPermissions>().InstancePerLifetimeScope();
            builder.RegisterType<NodeVmContextMenuProvider>().InstancePerLifetimeScope();
            builder.RegisterType<RtuVmActions>().InstancePerLifetimeScope();
            builder.RegisterType<RtuVmPermissions>().InstancePerLifetimeScope();
            builder.RegisterType<RtuVmContextMenuProvider>().InstancePerLifetimeScope();
            builder.RegisterType<MapActions>().InstancePerLifetimeScope();
            builder.RegisterType<MapContextMenuProvider>().InstancePerLifetimeScope();

            builder.RegisterType<ComponentsReportProvider>().InstancePerLifetimeScope();
            builder.RegisterType<ComponentsReportViewModel>().InstancePerLifetimeScope();
            builder.RegisterType<ActualOpticalEventsReportProvider>().InstancePerLifetimeScope();
            builder.RegisterType<AllOpticalEventsReportProvider>().InstancePerLifetimeScope();
            builder.RegisterType<OpticalEventsReportViewModel>().InstancePerLifetimeScope();

            builder.RegisterType<TraceStateReportProvider>().InstancePerLifetimeScope();
            builder.RegisterType<ModelLoader>().InstancePerLifetimeScope();

            builder.RegisterType<TceTypeViewModel>();
            builder.RegisterType<OneTceViewModel>();
            builder.RegisterType<TcesViewModel>().InstancePerLifetimeScope();
            builder.RegisterType<TceReportProvider>().InstancePerLifetimeScope();
        }
    }
}
