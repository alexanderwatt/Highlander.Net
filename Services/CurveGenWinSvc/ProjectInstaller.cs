using System.ComponentModel;
using System.Configuration.Install;

namespace CurveGenWinSvc
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
