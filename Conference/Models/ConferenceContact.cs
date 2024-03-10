namespace Conference.Models;

public class ConferenceContact
{
    public int Id
    {
        get;
        set;
    }

    public int OwnerId
    {
        get;
        set;
    }

    public int TargetId
    {
        get;
        set;
    }
}