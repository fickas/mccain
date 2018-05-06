using System;
using System.Diagnostics;
using System.ServiceProcess;
using System.Threading;
using Transparity.Services.C2C.Interfaces.TMDDInterface;

namespace Transparity.Services.C2C.McCainTMDD.ExCenter.WinHost
{
    static class Program
    {
        private static readonly McCainWindowsServiceHost ServiceHost = new McCainWindowsServiceHost();

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main()
        {
            var currentDomain = AppDomain.CurrentDomain;
            currentDomain.UnhandledException += MyHandler;

            try
            {
                if (Environment.UserInteractive)
                {
                    // Console app
                    Start();
                    Console.BackgroundColor = ConsoleColor.DarkMagenta;
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.Title = @"ExCenter Service";
                    Console.Clear();
                    Console.WriteLine($"{typeof(ItmddECSoapHttpServicePortType)} started. Ctrl-Break to terminate.");
                    using (var e = new ManualResetEvent(false))
                    {
                        e.WaitOne();
                    }
                }
                else
                {
                    // Windows Service app
                    ServiceBase[] servicesToRun;
                    try
                    {
                        servicesToRun = new ServiceBase[] 
                        { 
                            new WinService()
                        };
                    }
                    catch (Exception e)
                    {
                        Trace.WriteLine($"ServiceBase[] initialization Exception: {e.Message}");
                        return;
                    }

                    try
                    {
                        ServiceBase.Run(servicesToRun);
                    }
                    catch (Exception e)
                    {
                        Trace.WriteLine($"ServiceBase.Run Exception: {e.Message}");
                    }
                }
            }
            catch (Exception e)
            {
                // can't go any further
                Console.WriteLine(e);
            }
        }

        static void MyHandler(object sender, UnhandledExceptionEventArgs args)
        {
            var e = (Exception)args.ExceptionObject;
            Console.WriteLine(@"MyHandler caught : " + e.Message);
            Console.WriteLine($@"Runtime terminating: {args.IsTerminating}");
        }

        public static void Start()
        {
            try
            {
                ServiceHost.Start();
            }
            catch (Exception e)
            {
                Trace.WriteLine($"McCainWindowsServiceHost.Start Exception: {e.Message}");
                throw;
            }
        }

        public static void Stop()
        {
            try
            {
                ServiceHost.Stop();
            }
            catch (Exception e)
            {
                Trace.WriteLine($"McCainWindowsServiceHost.Stop Exception: {e.Message}");
                throw;
            }
        }
    }
}