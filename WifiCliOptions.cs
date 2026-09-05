using CommandLine;

namespace QRCodeGenerator;

// CLI verbs
[Verb("wifi", HelpText = "Generate a WiFi QR. Example: wifi --ssid MyNet --password Pwd123 --auth WPA --label MyNet --position AddBelow --output c:\\temp\\out.png")]
internal class WifiCliOptions
{
    [Option('s', "ssid", Required = true)] public string? Ssid { get; set; }
    [Option('p', "password", Required = true)] public string? Password { get; set; }
    [Option('a', "auth", Default = "WPA")] public string? Auth { get; set; }
    [Option('l', "label")] public string? Label { get; set; }
    [Option("position", Default = "AddBelow")] public string? Position { get; set; } // AddAbove or AddBelow
    [Option('o', "output")] public string? Output { get; set; } // if omitted, uses current directory wifi.png
    [Option('d', "density", Default = 20)] public int PixelDensity { get; set; }
    [Option("spacing", Default = 60)] public int Spacing { get; set; }
    [Option("separatorColor", Default = "Black")] public string? SeparatorColor { get; set; }
    [Option("separatorThickness", Default = 3f)] public float SeparatorThickness { get; set; }
}


