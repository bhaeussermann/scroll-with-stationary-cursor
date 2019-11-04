using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Utilities;

namespace ScrollWithStationaryCursor.EditorClassifier
{
    internal static class ClassificationDefinition
    {
#pragma warning disable 169
        [Export(typeof(ClassificationTypeDefinition))]
        [Name(nameof(EditorClassifier))]
        private static ClassificationTypeDefinition typeDefinition;
#pragma warning restore 169
    }
}
