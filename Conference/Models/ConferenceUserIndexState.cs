namespace Conference.Models;

public class ConferenceUserIndexState
{
    public ConferenceUserStatus Status
    {
        get;
        set;
    } = ConferenceUserStatus.Offline;
}