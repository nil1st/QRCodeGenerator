Here's the improved `README.md` file, incorporating the new content while maintaining the existing structure and information:

# QRCodeGenerator

A small .NET 9 console tool that generates QR codes for Wi‑Fi networks and arbitrary text/URLs. Supports generating a single Wi‑Fi QR, a URL/text QR, or a combined image containing multiple Wi‑Fi QR codes defined in a JSON configuration file or via command line.

## Features

- Generate Wi‑Fi QR codes (WPA/WEP/Open)
- Generate QR codes for arbitrary URL/text
- Combine multiple QR codes into a single image with separators
- Add optional labels above or below each QR code
- Configurable defaults via `appsettings.json`
- Cross-platform build (drawing uses System.Drawing; Windows is the primary supported platform)

## Requirements

- .NET 9 SDK
- Visual Studio 2022 or `dotnet` CLI

> Note: The project uses `System.Drawing` and is marked with `[SupportedOSPlatform("windows")]` — running image generation on non‑Windows platforms may require additional configuration or libraries.

## Build

To build the project, run the following command:

dotnet build -c Release

## Run

There are two main ways to run the tool:

1. Use CLI verbs to run a single command and produce an output file (CLI args take precedence over `appsettings.json`).
2. Run without arguments to load `appsettings.json` and produce a combined image from the `QRCodes` array.

### CLI verbs

- `wifi` — generate a single Wi‑Fi QR
- `url` — generate a QR for URL/text
- `multi-wifi` — generate multiple Wi‑Fi QR codes from a JSON file and combine into one image

#### Examples:

Generate a Wi‑Fi QR:

dotnet run -- wifi --ssid "MyNet" --password "Pwd123" --auth WPA --label "Office" --position AddBelow --output ./my-wifi.png

Generate a URL QR:

dotnet run -- url "https://example.com" --output ./url-qrcode.png

Generate multiple Wi‑Fi QRs from JSON:

dotnet run -- multi-wifi --input ./wifi-list.json --output ./wifi-multi.png

### appsettings.json (no-CLI mode)

When no command line arguments are provided, the tool reads `appsettings.json` from the current working directory. Example structure:

{
  "Output": {
    "SavePath": "C:\\temp\\wifi-multi.png"
  },
  "QrOptions": {
    "PixelDensity": 20,
    "Spacing": 60,
    "SeparatorColor": "Black",
    "SeparatorThickness": 3.0,
    "DefaultAuth": "WPA"
  },
  "QRCodes": [
    {
      "Ssid": "AumGuest",
      "Password": "AvniRiddhi",
      "Auth": "WPA",
      "Label": "AumGuest",
      "Position": "AddAbove"
    }
  ]
}

If `QRCodes` is empty or missing, the tool will exit with a message and return non‑zero.

## Types and options

- `QrOptions` (in `QrOptions.cs`)
  - `PixelDensity` — scale of QR pixels (default 20)
  - `Spacing` — vertical spacing between stacked QR images
  - `SeparatorColor` — color name for separator line
  - `SeparatorThickness` — thickness of separator line
  - `DefaultAuth` — default authentication type (e.g., `WPA`)

- `WifiEntry` (in `WifiEntry.cs`)
  - `Ssid` — SSID to encode
  - `Password` — network password
  - `Auth` — authentication type (e.g., `WPA`, `WEP`, empty for open)
  - `Label` — optional text label added above or below the QR
  - `Position` — `AddAbove` or `AddBelow`

CLI option classes are defined in `WifiCliOptions.cs`, `UrlCliOptions.cs`, and `MultiWifiCliOptions.cs`. When using CLI verbs, command line options override configuration.

## Output

- Single QR commands produce the image at the `--output` path or the current directory with a generated filename.
- `multi-wifi` and appsettings runs produce a combined PNG image with separators between QR codes.

## Notes

- Filenames are sanitized to replace invalid path characters.
- The project uses `QRCoder` to produce QR bitmaps.

## Contributing

See `CONTRIBUTING.md` for project standards and style rules.

## License

Specified by project (add license file if needed).

This version maintains the original structure while enhancing clarity and coherence. It also ensures that all relevant information is presented in a logical flow, making it easier for users to understand and utilize the QRCodeGenerator tool.