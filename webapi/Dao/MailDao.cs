using System.ComponentModel;
using System.Net.Mail;
using System.Net.Mime;
using System.Text;
using webapi.Utils;

namespace webapi.Dao
{
     public class MailDao
     {
          private MailConfig? config;
          public MailDao(IConfiguration configuration)
          {
               config = configuration.GetValue<MailConfig>("MailConfig");
               config?.SetBlankStringToNull();
          }

          public Task<bool> SendHTMLBodyMail(string toEmail, string subject, string message)
          {

               try
               {
                    if (config == null)
                    {
                         return Task.FromResult(false);
                    }

                    if (!config.Port.HasValue)
                    {
                         return Task.FromResult(false);
                    }

                    if (string.IsNullOrWhiteSpace(config.FromEmail))
                    {
                         return Task.FromResult(false);
                    }

                    if (string.IsNullOrWhiteSpace(config.Hostname))
                    {
                         return Task.FromResult(false);
                    }

                    if (string.IsNullOrWhiteSpace(config.SecretString))
                    {
                         return Task.FromResult(false);
                    }

                    if (config?.UseSSL == null)
                    {
                         return Task.FromResult(false);
                    }

                    TaskCompletionSource<bool> completion = new TaskCompletionSource<bool>();
                    SMTPListener sMTPListenerlistener = new SMTPListener(completion);

                    MailMessage smtpMessage = new MailMessage();
                    smtpMessage.From = new MailAddress(config!.FromEmail!, config!.DisplayName!);
                    smtpMessage.To.Add(toEmail);
                    smtpMessage.Subject = subject;
                    smtpMessage.Body = message;
                    smtpMessage.IsBodyHtml = true;

                    smtpMessage.DeliveryNotificationOptions = DeliveryNotificationOptions.OnFailure | DeliveryNotificationOptions.OnSuccess;
                    
                    smtpMessage.BodyEncoding = Encoding.UTF8;
                    smtpMessage.Priority = MailPriority.Normal;

                    SmtpClient smtpClient = new SmtpClient();
                    smtpClient.Port = config.Port!.Value;
                    smtpClient.DeliveryMethod = SmtpDeliveryMethod.Network;
                    smtpClient.EnableSsl = true;
                    smtpClient.UseDefaultCredentials = false;
                    smtpClient.SendCompleted += sMTPListenerlistener.smtpClient_SendCompleted;
                    smtpClient.SendMailAsync(smtpMessage);
                    return completion.Task;
               }
               catch (Exception)
               {
                    throw;
               }
          }


     }

     public class SMTPListener
     {
          private TaskCompletionSource<bool> completion;
          public SMTPListener(TaskCompletionSource<bool> completion)
          {
               this.completion = completion;
          }
          internal void smtpClient_SendCompleted(object sender, AsyncCompletedEventArgs e)
          {
               if (e.Error != null)
               {
                    completion.TrySetException(e.Error);
                    return;
               }

               if (e.Cancelled)
               {
                    completion.SetResult(false);
                    return;
               }

               completion.SetResult(true);
          }
     }

     public class MailConfig
     {
          public string? Hostname { get; set; }
          public int? Port { get; set; }
          public bool? UseSSL { get; set; }
          public string? FromEmail { get; set; }
          public string? DisplayName { get; set; }
          public string? SecretString { get; set; }
     }
}