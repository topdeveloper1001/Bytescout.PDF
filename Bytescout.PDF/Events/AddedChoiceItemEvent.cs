namespace Bytescout.PDF
{
    internal delegate void AddedChoiceItemEventHandler(object sender, AddedChoiceItemEvent e);

    internal class AddedChoiceItemEvent
    {
	    private readonly string _item;

	    internal string Item { get { return _item; } }

	    internal AddedChoiceItemEvent(string item)
        {
            _item = item;
        }
    }
}
