namespace Bytescout.PDF
{
    internal class PopupAnnotation: Annotation
    {
        internal PopupAnnotation(PDFDictionary dict, IDocumentEssential owner)
            : base(dict, owner) { }

        public override AnnotationType Type { get { return AnnotationType.Text; } }

        internal override Annotation Clone(IDocumentEssential owner, Page page)
        {
            PDFDictionary res = AnnotationBase.Copy(Dictionary);

            IPDFObject open = Dictionary["Open"];
            if (open != null)
                res.AddItem("Open", open.Clone());

            PopupAnnotation annot = new PopupAnnotation(res, owner);
            annot.SetPage(Page, false);
            annot.SetPage(page, true);
            return annot;
        }

        internal override void CreateApperance()
        {
        }

        internal override void ApplyOwner(IDocumentEssential owner)
        {
            Owner = owner;
        }

        internal void SetParent(Annotation annot)
        {
            Dictionary.AddItem("Parent", annot.Dictionary);
        }
    }
}
