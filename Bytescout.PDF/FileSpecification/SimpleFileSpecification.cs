namespace Bytescout.PDF
{
    internal class SimpleFileSpecification : FileSpecification
    {
        public SimpleFileSpecification(string fileName)
        {
            FileName = fileName;
            UF = fileName;
        }
    }
}
