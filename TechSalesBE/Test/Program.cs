using MailModule;
using MailModule.Application.Interfaces;
using MailModule.Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Test
{
    internal class Program
    {
        static void Main(string[] args)
        {
            DotNetEnv.Env.Load();
            IConfiguration configuration = new ConfigurationBuilder().AddEnvironmentVariables().Build();
            IServiceCollection services = new ServiceCollection();

            services.AddMailModule(configuration);
            IServiceProvider provider = services.BuildServiceProvider();

            IEmailService email = provider.GetRequiredService<IEmailService>();

            EmailMessage m = new EmailMessage
            {
                To = new List<string>() { "duongthuan0405@gmail.com" },
                Subject = "TEST",
                HtmlBody = "<div><b>Hello<b><div>"
            };

            email.SendAsync(m).Wait();

        }
    }
}
