using CommandLine;

namespace QRCodeGenerator;

[Verb("multi-wifi", HelpText = "Generate multiple WiFi QR codes from a JSON file with an array of WifiEntry objects.")]
internal class MultiWifiCliOptions
{
    [Option('i', "input", Required = true, HelpText = "Path to JSON file containing an array of WifiEntry objects.")]
    public string? Input { get; set; }

    [Option('o', "output", HelpText = "Output file path for combined image. If omitted, uses current directory wifi-multi.png.")]
    public string? Output { get; set; }

    [Option("spacing", Default = 60)] public int Spacing { get; set; }
    [Option("separatorColor", Default = "Black")] public string? SeparatorColor { get; set; }
    [Option("separatorThickness", Default = 3f)] public float SeparatorThickness { get; set; }
}