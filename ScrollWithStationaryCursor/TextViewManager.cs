using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Outlining;
using Microsoft.VisualStudio.Utilities;
using System;
using System.ComponentModel.Composition;

namespace ScrollWithStationaryCursor
{
    [Export(typeof(ITextViewCreationListener))]
    [ContentType("text")]
    [TextViewRole(PredefinedTextViewRoles.Document)]
    internal class TextViewManager : ITextViewCreationListener
    {
        public static TextViewManager Instance { get; private set; }

#pragma warning disable 649
        [Import]
        private IOutliningManagerService outliningManagerService;
#pragma warning restore 649

        private TextViewManager()
        {
            Instance = this;
        }

        public ITextView ActiveTextView { get; private set; }

        public IOutliningManager GetOutliningManager(ITextView textView)
        {
            return this.outliningManagerService.GetOutliningManager(textView);
        }

        public void TextViewCreated(ITextView textView)
        {
            ActiveTextView = textView;
            textView.GotAggregateFocus += TextView_GotAggregateFocus;
            textView.Closed += TextView_Closed;
        }

        private void TextView_GotAggregateFocus(object sender, EventArgs e)
        {
            ActiveTextView = (ITextView)sender;
        }

        private void TextView_Closed(object sender, EventArgs e)
        {
            var textView = (ITextView)sender;
            textView.GotAggregateFocus -= TextView_GotAggregateFocus;
            textView.Closed -= TextView_Closed;
        }
    }
}
