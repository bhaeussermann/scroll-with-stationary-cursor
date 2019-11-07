using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Utilities;

namespace ScrollWithStationaryCursor.EditorClassifier
{
    [Export(typeof(IClassifierProvider))]
    [ContentType("text")]
    internal class Provider : IClassifierProvider
    {
#pragma warning disable 649
        [Import]
        private IClassificationTypeRegistryService classificationRegistry;
#pragma warning restore 649

        public IClassifier GetClassifier(ITextBuffer buffer)
        {
            return buffer.Properties.GetOrCreateSingletonProperty(creator: () => new EditorClassifier(this.classificationRegistry));
        }
    }
}
