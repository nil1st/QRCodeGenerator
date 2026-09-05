namespace QRCodeGenerator;

public class WifiEntry
{
    public string? Ssid { get; set; }
    public string? Password { get; set; }
    public string? Auth { get; set; }
    public string? Label { get; set; }
    public string? Position { get; set; } // "AddAbove" or "AddBelow"
    public int PixelDensity { get; set; } // optional, used by multi-wifi input
}
