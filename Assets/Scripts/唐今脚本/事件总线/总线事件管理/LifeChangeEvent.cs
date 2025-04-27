public class LifeChangedEvent
{
    public int ChangeAmount { get; private set; }
    public LifeChangedEvent( int change)
    {
        ChangeAmount = change;
    }
}

