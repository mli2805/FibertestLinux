using System.Net;
using System.Net.Mail;
using Fibertest.Dto;
using Fibertest.Graph;
using Fibertest.Graph.TraceStateReport;
using Fibertest.Utils;
using Microsoft.Extensions.Options;

namespace Fibertest.DataCenter
{
    public class SmtpNotifier
    {
        private readonly IWritableOptions<SmtpConfig> _config;
        private readonly IOptions<DataCenterConfig> _fullConfig;
        private readonly ILogger<SmtpNotifier> _logger;
        private readonly Model _writeModel;

        private const string TestEmailSubj = @"Test email - Тестовое сообщение";
        private const string TestEmailMessage =
            @"You received this letter because you are included in Fibertest alarm subscription. - Вы получили данное сообщение так как включены в  список рассылки Fibertest'a";

        public SmtpNotifier(IWritableOptions<SmtpConfig> config, IOptions<DataCenterConfig> fullConfig, 
            ILogger<SmtpNotifier> logger, Model writeModel)
        {
            _config = config;
            _fullConfig = fullConfig;
            _logger = logger;
            _writeModel = writeModel;
        }

        public void SaveSmtpSettings(SmtpConfig dto)
        {
            _config.Update(c=>c.FillIn(dto));
        }

        public async Task<bool> SendTest(string address)
        {
            var mailTo = new List<string> { address };
            return await SendEmail(TestEmailSubj, TestEmailMessage, null, mailTo);
        }

        public async Task<bool> SendOpticalEvent(MonitoringResultDto dto, AddMeasurement addMeasurement)
        {
            var mailTo = _writeModel.GetEmailsToSendMonitoringResult(dto);
            _logger.LLog(Logs.DataCenter.ToInt(), $"There are {mailTo.Count} addresses to send e-mail");
            if (mailTo.Count == 0) return true;

            var subj = _writeModel.GetShortMessageForMonitoringResult(dto);
            if (subj == null) return false;
            var attachmentFilename = PreparePdfAttachment(addMeasurement);
            return await SendEmail(subj, subj, attachmentFilename, mailTo);
        }

        private string? PreparePdfAttachment(AddMeasurement addMeasurement)
        {
            try
            {
                var folder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\Reports");
                if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);

                string filename = Path.Combine(folder, $@"TraceStateReport{DateTime.Now:yyyy-MM-dd-hh-mm-ss}.pdf");
                var trace = _writeModel.Traces.First(t=>t.TraceId == addMeasurement.TraceId);
                var rtu = _writeModel.Rtus.First(r => r.Id == addMeasurement.RtuId);
                var reportModel = new TraceReportModel()
                {
                    TraceTitle = trace.Title,
                    TraceState = trace.State.ToLocalizedString(),
                    RtuTitle = rtu.Title,
                    RtuSoftwareVersion = rtu.Version,
                    PortTitle = trace.OtauPort == null 
                        ? "trace not connected to port"
                        : trace.OtauPort.IsPortOnMainCharon
                            ? trace.OtauPort.OpticalPort.ToString()
                            : $@"{trace.OtauPort.Serial}-{trace.OtauPort.OpticalPort}",
                    MeasurementTimestamp = $@"{addMeasurement.MeasurementTimestamp:G}",
                    RegistrationTimestamp = $@"{addMeasurement.EventRegistrationTimestamp:G}",
                
                    Accidents = ConvertAccidents(addMeasurement.Accidents).ToList(),
                };
                new TraceStateReportProvider().Create(reportModel, _fullConfig.Value);
                    //.Save(filename);
                return filename;
            }
            catch (Exception e)
            {
                _logger.Log(LogLevel.Error, Logs.DataCenter.ToInt(), @"ShowReport: " + e.Message);
                return null;
            }
        }

        private IEnumerable<AccidentLineModel> ConvertAccidents(List<AccidentOnTraceV2> list)
        {
            var number = 0;
            var isGisOn = !_fullConfig.Value.General.IsWithoutMapMode;
            var factory = new AccidentLineModelFactory();
            foreach (var accidentOnTraceV2 in list)
            {
                yield return factory.Create(accidentOnTraceV2, ++number, isGisOn);
            }
        }

        public async Task<bool> SendNetworkEvent(Guid rtuId, bool isMainChannel, bool isOk)
        {
            var mailTo = _writeModel.GetEmailsToSendNetworkEvent(rtuId);
            _logger.LLog(Logs.DataCenter.ToInt(), $"There are {mailTo.Count} addresses to send e-mail");
            if (mailTo.Count == 0) return true;

            var subj = _writeModel.GetShortMessageForNetworkEvent(rtuId, isMainChannel, isOk);
            return await SendEmail(subj, subj, null, mailTo);
        }
        public async Task<bool> SendBopState(AddBopNetworkEvent cmd)
        {
            var mailTo = _writeModel.GetEmailsToSendBopNetworkEvent(cmd);
            _logger.LLog(Logs.DataCenter.ToInt(), $"There are {mailTo.Count} addresses to send e-mail");
            if (mailTo.Count == 0) return true;

            var subj = EventReport.GetShortMessageForBopState(cmd);
            return await SendEmail(subj, subj, null, mailTo);
        }

        // userId - if empty - all users who have email
        private async Task<bool> SendEmail(string subject, string body, string? attachmentFilename, List<string> addresses)
        {
            try
            {
                var mailFrom = _config.Value.MailFrom;
                using (SmtpClient smtpClient = GetSmtpClient(mailFrom))
                {
                    var mail = new MailMessage
                    {
                        From = new MailAddress(mailFrom),
                        Subject = subject,
                        Body = body
                    };
                    foreach (var address in addresses)
                        mail.To.Add(address);

                    if (attachmentFilename != null)
                        mail.Attachments.Add(new Attachment(attachmentFilename));

                    await smtpClient.SendMailAsync(mail);
                    return true;
                }
            }
            catch (Exception e)
            {
                _logger.Log(LogLevel.Error, Logs.DataCenter.ToInt(), e.Message);
                return false;
            }
        }

        private SmtpClient GetSmtpClient(string mailFrom)
        {
            
            SmtpClient smtpClient = new SmtpClient(_config.Value.SmtpHost, _config.Value.SmtpPort)
            {
                EnableSsl = true,
                Timeout = _config.Value.SmtpTimeoutMs,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(mailFrom, _config.Value.MailFromPassword)
            };

            return smtpClient;
        }
    }
}
