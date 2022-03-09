using Gatekeeper.LdapServerLibrary;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

namespace Sample
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            LdapServer server = new LdapServer
            {
                Port = 3389, 
                IPAddress = IPAddress.Any
            };
            server.RegisterEventListener(new LdapEventListener(true));
            ILoggerFactory loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
            var logger = loggerFactory.CreateLogger<Program>();
            server.RegisterLogger(logger);
            server.RegisterCertificate(new X509Certificate2(GetTlsCertificatePath()));
            var startTask = server.Start();
            logger.LogInformation($"Server started on port {server.Port}, address: {server.IPAddress}, Press control-c to stop");
            var cki = Console.ReadKey();
            if (cki.Key == ConsoleKey.C && cki.Modifiers.HasFlag(ConsoleModifiers.Control))
            {
                server.Stop();
                return;
            }
        }

        private static string GetTlsCertificatePath()
        {
            var certificateStream = System.Reflection.Assembly.GetAssembly(typeof(Sample.Program)).GetManifestResourceStream("Sample.example_certificate.pfx");
            string path = Path.GetTempFileName();
            var fileStream = File.Create(path);
            certificateStream.Seek(0, SeekOrigin.Begin);
            certificateStream.CopyTo(fileStream);
            fileStream.Close();
            return path;
        }
    }
}
