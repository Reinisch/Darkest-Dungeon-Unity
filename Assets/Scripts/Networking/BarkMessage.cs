public class BarkMessage
{
    public Team Team { get; set; }
    public string Message { get; set; }

    public BarkMessage(Team team, string message)
    {
        Team = team;
        Message = message;
    }
}
