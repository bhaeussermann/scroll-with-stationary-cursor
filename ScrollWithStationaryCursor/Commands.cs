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
        private const int RegionPlaceholderWidth = 23;
        
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
                && (view.GetScrollInfo(VerticalScrollBar, out int _min, out int _max, out int _visibleLines, out int top) == VSConstants.S_OK)
                && ((direction == 1) || (top > 0))
                && (view.GetCaretPos(out int currentCaretLine, out int currentCaretColumn) == VSConstants.S_OK))
            {
                var currentLinePoint = new Microsoft.VisualStudio.OLE.Interop.POINT[1];
                view.GetPointOfLineColumn(currentCaretLine, currentCaretColumn, currentLinePoint);

                void TestLine(int line, out bool isValid, out bool isInsideCollapsedRegion)
                {
                    var linePoint = new Microsoft.VisualStudio.OLE.Interop.POINT[1];
                    isValid = view.GetPointOfLineColumn(line, currentCaretColumn, linePoint) == VSConstants.S_OK;
                    isInsideCollapsedRegion = isValid && (linePoint[0].x != currentLinePoint[0].x) && (Math.Abs(linePoint[0].x - currentLinePoint[0].x) != RegionPlaceholderWidth);
                }

                int potentialNextCaretLine = currentCaretLine + direction;
                TestLine(potentialNextCaretLine, out bool isPotentialNextCaretLineValid, out bool isPotentialNextCaretLineInsideRegion);
                if (!isPotentialNextCaretLineValid)
                    return;

                int nextCaretLine, nextCaretColumn;
                if (!isPotentialNextCaretLineInsideRegion)
                {
                    nextCaretLine = potentialNextCaretLine;
                    nextCaretColumn = currentCaretColumn;
                }
                else
                {
                    potentialNextCaretLine = currentCaretLine;
                    bool isCaretLineInsideRegion;
                    do
                    {
                        int nextCaretLineToTest = potentialNextCaretLine + direction;
                        TestLine(nextCaretLineToTest, out bool isCaretLineValid, out isCaretLineInsideRegion);
                        if (!isCaretLineValid)
                            break;

                        potentialNextCaretLine = nextCaretLineToTest;
                    }
                    while ((potentialNextCaretLine > 0) && isCaretLineInsideRegion);
                    nextCaretLine = potentialNextCaretLine;
                    nextCaretColumn = isCaretLineInsideRegion ? 0 : currentCaretColumn;
                }

                bool didCaretMove = nextCaretLine != currentCaretLine;
                if (didCaretMove)
                {
                    view.SetCaretPos(nextCaretLine, nextCaretColumn);
                    view.SetScrollPosition(VerticalScrollBar, top + direction);
                }
            }
        }
    }
}
