using System.ServiceProcess;

namespace Transparity.Services.C2C.McCainTMDD.ExCenter.WinHost
{
    public partial class WinService : ServiceBase
    {
        public WinService()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            Program.Start();
        }

        protected override void OnStop()
        {
            Program.Stop();
        }
    }
}
