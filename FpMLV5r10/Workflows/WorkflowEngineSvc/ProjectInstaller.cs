using System.ComponentModel;
using System.Configuration.Install;


namespace WorkflowEngineSvc
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
