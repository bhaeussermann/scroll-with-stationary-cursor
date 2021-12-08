using Microsoft.VisualStudio.Shell;
using System;
using System.Runtime.InteropServices;
using System.Threading;
using Task = System.Threading.Tasks.Task;

namespace ScrollWithStationaryCursor.Shared
{
    [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
    [InstalledProductRegistration("#110", "#112", "1.0", IconResourceID = 400)]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [Guid("5c1d3c79-5ac0-4757-a6ef-8c310660662b")]
    public sealed class Package : AsyncPackage
    {
        internal static readonly Guid CommandSetGuid = new Guid("0d7f8b6a-e69a-429e-b8d5-fa7937030546");

        protected override async Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
        {
            await JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);
            await Commands.InitializeAsync(this);
        }
    }
}
