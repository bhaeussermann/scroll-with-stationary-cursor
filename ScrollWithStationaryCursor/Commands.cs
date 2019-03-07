using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.TextManager.Interop;
using System;
using System.ComponentModel.Design;
using Task = System.Threading.Tasks.Task;

namespace ScrollWithStationaryCursor
{
    internal sealed class Commands
    {
        private const int UpCommandId = 0x0100, DownCommandId = 0x0101;
        private const int VerticalScrollBar = 1;
        
        private readonly AsyncPackage package;
        
        private Commands(AsyncPackage package, OleMenuCommandService commandService)
        {
            this.package = package ?? throw new ArgumentNullException(nameof(package));
            commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));

            RegisterCommand(UpCommandId, ExecuteUp, commandService);
            RegisterCommand(DownCommandId, ExecuteDown, commandService);
        }
        
        public static Commands Instance { get; private set; }
        
        private IServiceProvider ServiceProvider => this.package;

        public static async Task InitializeAsync(AsyncPackage package)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(package.DisposalToken);

            var commandService = await package.GetServiceAsync(typeof(IMenuCommandService)) as OleMenuCommandService;
            Instance = new Commands(package, commandService);
        }

        private void RegisterCommand(int commandId, EventHandler commandDelegate, OleMenuCommandService commandService)
        {
            var menuCommandId = new CommandID(Package.CommandSetGuid, commandId);
            var menuItem = new MenuCommand(commandDelegate, menuCommandId);
            commandService.AddCommand(menuItem);
        }
        
        private void ExecuteUp(object sender, EventArgs e)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            Scroll(-1);
        }

        private void ExecuteDown(object sender, EventArgs e)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            Scroll(1);
        }

        private void Scroll(int direction)
        {
            var textManager = (IVsTextManager2)ServiceProvider.GetService(typeof(SVsTextManager));
            if ((textManager != null)
                && (textManager.GetActiveView2(1, null, (uint)_VIEWFRAMETYPE.vftCodeWindow, out var view) == VSConstants.S_OK)
                && (view.GetScrollInfo(VerticalScrollBar, out int min, out int max, out int visibleLines, out int top) == VSConstants.S_OK)
                && (view.GetCaretPos(out int caretLine, out int caretColumn) == VSConstants.S_OK))
            {
                view.SetScrollPosition(VerticalScrollBar, top + direction);
                view.SetCaretPos(caretLine + direction, caretColumn);
            }
        }
    }
}
