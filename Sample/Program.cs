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
            var server = new LdapServer
            {
                Port = 3389, 
                IPAddress = IPAddress.Any
            };
            var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
            var ldapLogger = loggerFactory.CreateLogger<LdapEventListener>();
            server.RegisterEventListener(new LdapEventListener(ldapLogger, true));
            var logger = loggerFactory.CreateLogger<Program>();
            server.RegisterLogger(logger);
            server.RegisterCertificate(new X509Certificate2(GetTlsCertificatePath()));
            var startTask = server.Start();
            logger.LogInformation($"Server started on port {server.Port}, address: {server.IPAddress}, Use start/stop to start/stop and exit to exit");
            var status = string.Empty;
            do
            {
                status = Console.ReadLine();
                switch (status)
                {
                    case "start":
                        _ = server.Start();
                        break;
                    case "stop":
                        server.Stop();
                        break;
                }
            }
            while (status != "exit");
            //var cki = Console.ReadKey();
            //if (cki.Key == ConsoleKey.C && cki.Modifiers.HasFlag(ConsoleModifiers.Control))
            //{
            //    server.Stop();
            //    return;
            //}
        }

        private static string GetTlsCertificatePath()
        {
            var certificateStream = System.Reflection.Assembly.GetAssembly(typeof(Program)).GetManifestResourceStream("Sample.example_certificate.pfx");
            string path = Path.GetTempFileName();
            var fileStream = File.Create(path);
            certificateStream.Seek(0, SeekOrigin.Begin);
            certificateStream.CopyTo(fileStream);
            fileStream.Close();
            return path;
        }
    }
}
