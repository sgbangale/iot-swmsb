using Microsoft.Extensions.Logging;
using SendGrid;
using SendGrid.Helpers.Mail;
using SWMSB.COMMON;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SWMSB.PROVIDERS
{
    public class EmailProvider
    {
        public EmailProvider(Config config, ILogger _logger)
        {
            log = _logger;
            SendGridClient = new SendGridClient(config.SENDGRID_API_KEY);
        }

        private readonly ILogger log;

        public SendGridClient SendGridClient { get; }

        public async Task<bool> SendEmail(EmailMsg emailMsg)
        {
            var validationResult = emailMsg.Validate();
            if (validationResult.Success)
            {
                try
                {

                    var msg = new SendGridMessage();

                    msg.SetFrom(new EmailAddress("sgbangale@gmail.com", "smartwatersystems"));

                    var recipients = new List<EmailAddress>
                    {
                      new EmailAddress(emailMsg.ReceiverEmail, emailMsg.ReceiverName)
                    };
                    msg.AddTos(recipients);

                    msg.SetSubject(emailMsg.Subject);

                    msg.AddContent(MimeType.Text, emailMsg.EmailBody);
                    var response = await SendGridClient.SendEmailAsync(msg);
                    if (response.StatusCode == System.Net.HttpStatusCode.Accepted)
                    {
                        return true;
                    }
                    return false;
                }
                catch (Exception ex)
                {
                    log.LogError($"{emailMsg}-{ex.Message}", ex, $"Error-{typeof(EmailProvider)}");
                    return false;
                }
            }
            else
            {
                log.LogError($"{typeof(EmailProvider)}-validation-failed-{validationResult.ErrorMessage}");
                return false;
            }

        }


    }
}
