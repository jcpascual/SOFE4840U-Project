namespace Conference.Models;

public class ConferenceUserIndexState
{
    public string? ConnectionId
    {
        get;
        set;
    } = null;

    public ConferenceUserStatus Status
    {
        get;
        set;
    } = ConferenceUserStatus.Offline;
}