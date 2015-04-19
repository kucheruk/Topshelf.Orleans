using System.Configuration;
using System.Threading.Tasks;
using Orleans.Runtime.Host;
using Topshelf;

namespace TopshelfSiloHost
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            HostFactory.Run(x =>
            {
                x.EnableServiceRecovery(a =>
                {
                    a.OnCrashOnly();
                    a.RestartService(1);
                });
                x.Service<WindowsServerHost>(s =>
                {
                    s.ConstructUsing(name => new WindowsServerHost());
                    s.WhenStarted((tc, hc) =>
                    {
                        if (!tc.ParseArguments(SiloStartupArgs()))
                        {
                            tc.PrintUsage();
                        }
                        else
                        {
                            Task.Factory.StartNew(() =>
                            {
                                tc.Init();
                                tc.Run();
                                hc.Stop();
                            });
                        }
                        return true;
                    });
                    s.WhenStopped(tc => { tc?.Dispose(); });
                });
                x.RunAsLocalSystem();

                x.SetDescription("my orleans service");
                x.SetDisplayName("testorleans");
                x.SetServiceName("testorleans");
            });
        }

        private static string[] SiloStartupArgs()
        {
            var siloName = ConfigurationManager.AppSettings.Get("SiloName");
            if (!string.IsNullOrEmpty(siloName))
            {
                return new[] {siloName};
            }
            return new string[] {};
        }
    }
}