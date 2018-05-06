using System;
using System.ServiceModel;
using Transparity.Services.C2C.Interfaces.TMDDInterface;

namespace Transparity.Services.C2C.McCainTMDD.ExCenter.WinHost
{
    internal class McCainWindowsServiceHost : IDisposable
    {
          private readonly McCainHost _serviceHost;

          public McCainWindowsServiceHost(params Uri[] baseAddresses)
        {
            // Required for ignoring a self-signed certificate
            //if (Core.Services.Configuration.ServiceConfiguration.IgnoreCertificateValidationErrors)
            //{
                System.Net.ServicePointManager.ServerCertificateValidationCallback = ((sender, certificate, chain, sslPolicyErrors) => true);
            //}

            _serviceHost = new McCainHost(baseAddresses);
        }

        public void Start()
        {
            _serviceHost.Open();
        }

        public void Stop()
        {
            _serviceHost.Close();
        }

        private class McCainHost : ServiceHost
        {
            public McCainHost(params Uri[] baseAddresses) : base(typeof(McCainSvc), baseAddresses) { }

            protected override void OnOpening()
            {
                 base.OnOpening();
            }

            protected override void OnClosing()
            {
                base.OnClosing();
            }
        }

        void IDisposable.Dispose() { Dispose(); }

        private void Dispose()
        {
            _serviceHost.Abort();

            var disposable = _serviceHost as IDisposable;
            disposable?.Dispose();
        }
    }
}
