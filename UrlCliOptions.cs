using CommandLine;

namespace QRCodeGenerator;

[Verb("url", HelpText = "Generate a QR for a URL/text. Example: url --value \"https://example.com\" --output c:\\temp\\url.png")]
internal class UrlCliOptions
{
    [Value(0, MetaName = "value", Required = true, HelpText = "URL or text to encode as QR.")] public string? Value { get; set; }
    [Option('o', "output")] public string? Output { get; set; }
    [Option('d', "density", Default = 20)] public int PixelDensity { get; set; }
}
