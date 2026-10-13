using System;
using System.Windows.Forms;
using WixSharp;
using WixSharp.Forms;
using File = WixSharp.File;
using static ThisAssembly.Constants;

namespace ClinicManager.Setup
{
    public class Program
    {
        static void Main()
        {
            string version = string.Join(".", ThisAssembly.AssemblyFileVersion.Split('.').Take(3));
            string projectName = $"ClinicManager.{version}";
            var project = new ManagedProject(projectName,
                             new Dir(@"%ProgramFiles%\My Company\My Product",
                                     new DirFiles($@"{BuildInfo.DesktopPublishDir}\*.*"),
                                     new File("Program.cs")
                                    ));

            project.GUID = new Guid("fcda5411-1261-472f-8c5a-472b90d55a6f");

            project.ManagedUI = ManagedUI.Empty;    //no standard UI dialogs
            project.ManagedUI = ManagedUI.Default;  //all standard UI dialogs

            //custom set of standard UI dialogs
            project.ManagedUI = new ManagedUI();

            project.ManagedUI.InstallDialogs.Add(Dialogs.Welcome)
                                            .Add(Dialogs.Licence)
                                            .Add(Dialogs.SetupType)
                                            .Add(Dialogs.Features)
                                            .Add(Dialogs.InstallDir)
                                            .Add(Dialogs.Progress)
                                            .Add(Dialogs.Exit);

            project.ManagedUI.ModifyDialogs.Add(Dialogs.MaintenanceType)
                                           .Add(Dialogs.Features)
                                           .Add(Dialogs.Progress)
                                           .Add(Dialogs.Exit);

            project.Load += Msi_Load;
            project.BeforeInstall += Msi_BeforeInstall;
            project.AfterInstall += Msi_AfterInstall;

            //project.SourceBaseDir = "<input dir path>";
            project.OutDir = BuildInfo.InstallerDir;

            project.BuildMsi();
        }

        static void Msi_Load(SetupEventArgs e)
        {
            if (!e.IsUISupressed && !e.IsUninstalling)
                MessageBox.Show(e.ToString(), "Load");
        }

        static void Msi_BeforeInstall(SetupEventArgs e)
        {
            if (!e.IsUISupressed && !e.IsUninstalling)
                MessageBox.Show(e.ToString(), "BeforeInstall");
        }

        static void Msi_AfterInstall(SetupEventArgs e)
        {
            if (!e.IsUISupressed && !e.IsUninstalling)
                MessageBox.Show(e.ToString(), "AfterExecute");
        }
    }
}
