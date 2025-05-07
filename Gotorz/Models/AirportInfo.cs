using Gotorz.DTOs;

public class AirportInfo
{
    public string Name { get; set; } = "";
    public string Id { get; set; } = "";
    public string Time { get; set; } = "";

    public static implicit operator AirportInfo(AirportInfoDto v)
    {
        throw new NotImplementedException();
    }

}
