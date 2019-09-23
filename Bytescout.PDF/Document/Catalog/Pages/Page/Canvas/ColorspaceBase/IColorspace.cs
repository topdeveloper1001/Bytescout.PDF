namespace Bytescout.PDF
{
    internal interface IColorspace
    {
        string Name
        {
            get;
        }

        int CountComponents
        {
            get;
        }
    }
}
