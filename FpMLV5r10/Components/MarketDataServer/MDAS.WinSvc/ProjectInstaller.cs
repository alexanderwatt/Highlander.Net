using System.ComponentModel;
using System.Configuration.Install;


namespace Orion.MDAS.WinSvc
{
    [RunInstaller(true)]
    public partial class ProjectInstaller : Installer
    {
        public ProjectInstaller()
        {
            InitializeComponent();
        }
    }
}
