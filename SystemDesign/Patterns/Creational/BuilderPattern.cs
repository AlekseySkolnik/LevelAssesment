using System.Net.Mail;
using System.Text;

namespace Patterns.Creational
{
    public sealed class MailMessageBuilder
    {
        private readonly MailMessage _mailMessage = new();
        public MailMessageBuilder From(string address)
        {
            _mailMessage.From = new MailAddress(address);
            return this;
        }
        public MailMessageBuilder To(string address)
        {
            _mailMessage.To.Add(address);
            return this;
        }
        public MailMessageBuilder Cc(string address)
        {
            _mailMessage.CC.Add(address);
            return this;
        }
        public MailMessageBuilder Subject(string subject)
        {
            _mailMessage.Subject = subject;
            return this;
        }

        public MailMessageBuilder Body(string body, Encoding enconding)
        {
            _mailMessage.Body = body;
            _mailMessage.BodyEncoding = enconding;
            return this;
        }
        public MailMessage Build()
        {
            return _mailMessage;
        }
    }

    internal static class BuilderPattern
    {
        public static void Demo()
        {
            Console.WriteLine("***Builder Pattern Demo***");

            var mail = new MailMessageBuilder()
                .From("st@unknown.com")
                .To("support@microsof.com")
                .Cc("my_boss@unknown.com")
                .Subject("Msdn is down!")
                .Body("Please fix!", Encoding.UTF8)
                .Build();
        }
    }
}