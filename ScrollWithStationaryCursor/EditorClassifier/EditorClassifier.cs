using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;

namespace ScrollWithStationaryCursor.EditorClassifier
{
    internal class EditorClassifier : IClassifier
    {
        private readonly IClassificationType classificationType;

        internal EditorClassifier(IClassificationTypeRegistryService registry)
        {
            this.classificationType = registry.GetClassificationType(nameof(EditorClassifier));
        }

#pragma warning disable 67
        public event EventHandler<ClassificationChangedEventArgs> ClassificationChanged;
#pragma warning restore 67

        public IList<ClassificationSpan> GetClassificationSpans(SnapshotSpan span)
        {
            return new List<ClassificationSpan>()
            {
                new ClassificationSpan(new SnapshotSpan(span.Snapshot, new Span(span.Start, span.Length)), this.classificationType)
            };
        }
    }
}
