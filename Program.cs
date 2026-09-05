using CommandLine;
using Microsoft.Extensions.Configuration;
using QRCoder;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Runtime.Versioning;


[assembly: SupportedOSPlatform("windows")]

namespace QRCodeGenerator;

public class Program
{
    static int Main(string[] args)
    {
        // If CLI args are present, parse them and ignore appsettings.json
        if (args != null && args.Length > 0)
        {
            return Parser.Default.ParseArguments<WifiCliOptions, UrlCliOptions, MultiWifiCliOptions>(args)
                .MapResult(
                    (WifiCliOptions opt) => RunWithWifiCli(opt),
                    (UrlCliOptions opt) => RunWithUrlCli(opt),
                    (MultiWifiCliOptions opt) => RunWithMultiWifiCli(opt),
                    errs => 1);
        }

        // No CLI args — load config from appsettings.json
        var config = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();

        var qrOptions = config.GetSection("QrOptions").Get<QrOptions>() ?? new QrOptions();
        var outputPath = config.GetValue<string>("Output:SavePath") ?? Path.Combine(Path.GetTempPath(), "wifi-multi.png");

        var entries = config.GetSection("QRCodes").Get<List<WifiEntry>>() ?? new List<WifiEntry>();
        if (entries.Count == 0)
        {
            Console.WriteLine("No entries found in configuration (QRCodes). Nothing to generate.");
            return 1;
        }

        var bitmaps = new List<Bitmap>();
        foreach (var e in entries)
        {
            var pos = Label.AddBelow;
            if (!string.IsNullOrEmpty(e.Position) && Enum.TryParse<Label>(e.Position, true, out var parsed))
                pos = parsed;

            var bmp = GenerateWifiQr(e.Ssid!, e.Password!, string.IsNullOrEmpty(e.Auth) ? qrOptions.DefaultAuth : e.Auth, e.Label!, pos, qrOptions.PixelDensity);
            bitmaps.Add(bmp);
        }

        using var combined = CombineQrCodes(bitmaps.ToArray(), qrOptions.Spacing, qrOptions.SeparatorColor, qrOptions.SeparatorThickness);
        Directory.CreateDirectory(Path.GetDirectoryName(outputPath) ?? Directory.GetCurrentDirectory());
        combined.Save(outputPath);
        Console.WriteLine($"Saved: {outputPath}");
        return 0;
    }

    // Reuse QrOptions and WifiEntry classes from workspace files (QrOptions.cs, WifiEntry.cs)
    enum Label
    {
        AddAbove,
        AddBelow
    }

    // Run handlers for CLI verbs
    static int RunWithWifiCli(WifiCliOptions opt)
    {
        var pos = Label.AddBelow;
        if (!string.IsNullOrEmpty(opt.Position) && Enum.TryParse<Label>(opt.Position, true, out var parsed))
            pos = parsed;

        var bmp = GenerateWifiQr(opt.Ssid!, opt.Password!, opt.Auth!, opt.Label!, pos, opt.PixelDensity);
        string outPath = !string.IsNullOrEmpty(opt.Output) ? opt.Output : Path.Combine(Directory.GetCurrentDirectory(), $"{SanitizeFileName(opt.Ssid!)}-wifi.png");
        Directory.CreateDirectory(Path.GetDirectoryName(outPath) ?? Directory.GetCurrentDirectory());
        bmp.Save(outPath);
        Console.WriteLine($"Saved: {outPath}");
        return 0;
    }

    static int RunWithUrlCli(UrlCliOptions opt)
    {
        var generator = new QRCoder.QRCodeGenerator();
        var data = generator.CreateQrCode(opt.Value!, QRCoder.QRCodeGenerator.ECCLevel.Q);
        var qr = new QRCode(data);
        using var bmp = qr.GetGraphic(opt.PixelDensity);
        string outPath = !string.IsNullOrEmpty(opt.Output) ? opt.Output : Path.Combine(Directory.GetCurrentDirectory(), "url-qrcode.png");
        Directory.CreateDirectory(Path.GetDirectoryName(outPath) ?? Directory.GetCurrentDirectory());
        bmp.Save(outPath);
        Console.WriteLine($"Saved: {outPath}");
        return 0;
    }

    static int RunWithMultiWifiCli(MultiWifiCliOptions opt)
    {
        if (!File.Exists(opt.Input))
        {
            Console.WriteLine($"Input file not found: {opt.Input}");
            return 1;
        }

        var json = File.ReadAllText(opt.Input);
        var entries = System.Text.Json.JsonSerializer.Deserialize<List<WifiEntry>>(json) ?? new List<WifiEntry>();
        if (entries.Count == 0)
        {
            Console.WriteLine("No entries found in input file.");
            return 1;
        }

        var bitmaps = new List<Bitmap>();
        foreach (var e in entries)
        {
            var pos = Label.AddBelow;
            if (!string.IsNullOrEmpty(e.Position) && Enum.TryParse<Label>(e.Position, true, out var parsed))
                pos = parsed;

            var bmp = GenerateWifiQr(e.Ssid!, e.Password!, string.IsNullOrEmpty(e.Auth) ? "WPA" : e.Auth, e.Label!, pos, e.PixelDensity > 0 ? e.PixelDensity : 20);
            bitmaps.Add(bmp);
        }

        using var combined = CombineQrCodes(bitmaps.ToArray(), opt.Spacing, opt.SeparatorColor!, opt.SeparatorThickness);
        string outPath = !string.IsNullOrEmpty(opt.Output) ? opt.Output : Path.Combine(Directory.GetCurrentDirectory(), "wifi-multi.png");
        Directory.CreateDirectory(Path.GetDirectoryName(outPath) ?? Directory.GetCurrentDirectory());
        combined.Save(outPath);
        Console.WriteLine($"Saved: {outPath}");
        return 0;
    }

    // Existing helpers (GenerateWifiQr, AddLabel, CombineQrCodes)
    static Bitmap GenerateWifiQr(string ssid, string password, string auth = "WPA", string label = null!, Label position = Label.AddBelow, int pixelDensity = 20)
    {
        string payload = $"WIFI:T:{auth};S:{ssid};P:{password};;";

        var generator = new QRCoder.QRCodeGenerator();
        var data = generator.CreateQrCode(payload, QRCoder.QRCodeGenerator.ECCLevel.Q);
        QRCode qr = new QRCode(data);

        Bitmap qrBmp = qr.GetGraphic(pixelDensity); // configurable pixel density

        if (string.IsNullOrEmpty(label))
        {
            return qrBmp;
        }

        return AddLabel(qrBmp, label, position);
    }

    static Bitmap AddLabel(Bitmap image, string label, Label position)
    {
        if (string.IsNullOrEmpty(label))
        {
            return image;
        }

        // Measure text size on a temporary surface
        using (var measureBmp = new Bitmap(1, 1))
        using (var measureG = Graphics.FromImage(measureBmp))
        {
            measureG.TextRenderingHint = TextRenderingHint.AntiAlias;
            float fontSize = Math.Max(10f, image.Width / 18f);
            using (var font = new Font("Arial", fontSize, FontStyle.Bold, GraphicsUnit.Pixel))
            {
                SizeF textSize = measureG.MeasureString(label, font);
                int padding = 8;

                int finalWidth = image.Width;
                int topAreaHeight = 0;
                int bottomAreaHeight = 0;

                if (position == Label.AddAbove)
                {
                    topAreaHeight = padding + (int)System.Math.Ceiling(textSize.Height) + padding;
                }
                else // AddBelow
                {
                    bottomAreaHeight = padding + (int)System.Math.Ceiling(textSize.Height) + padding;
                }

                int finalHeight = image.Height + topAreaHeight + bottomAreaHeight;

                Bitmap finalBmp = new Bitmap(finalWidth, finalHeight);
                using (Graphics g = Graphics.FromImage(finalBmp))
                {
                    g.Clear(Color.White);
                    g.SmoothingMode = SmoothingMode.AntiAlias;
                    g.TextRenderingHint = TextRenderingHint.AntiAlias;

                    using (var textBrush = new SolidBrush(Color.Black))
                    {
                        if (position == Label.AddAbove)
                        {
                            // Draw label centered at top
                            float textX = (finalWidth - textSize.Width) / 2f;
                            float textY = padding;
                            g.DrawString(label, font, textBrush, textX, textY);

                            // Draw image below the label area
                            g.DrawImage(image, 0, topAreaHeight);
                        }
                        else // AddBelow
                        {
                            // Draw image at top
                            g.DrawImage(image, 0, 0);

                            // Draw label centered beneath image
                            float textX = (finalWidth - textSize.Width) / 2f;
                            float textY = image.Height + padding;
                            g.DrawString(label, font, textBrush, textX, textY);
                        }
                    }
                }

                return finalBmp;
            }
        }
    }

    static Bitmap CombineQrCodes(params Bitmap[] qrCodes) => CombineQrCodes(qrCodes, 60, "Black", 3f);

    static Bitmap CombineQrCodes(Bitmap[] qrCodes, int spacing, string separatorColorName, float separatorThickness)
    {
        if (qrCodes == null || qrCodes.Length == 0)
            return new Bitmap(1, 1);

        int width = qrCodes.Max(q => q.Width);
        int height = qrCodes.Sum(q => q.Height) + spacing * (qrCodes.Length - 1);

        Bitmap final = new Bitmap(width, height);
        using (Graphics g = Graphics.FromImage(final))
        {
            g.Clear(Color.White);
            g.SmoothingMode = SmoothingMode.AntiAlias;

            int y = 0;
            for (int i = 0; i < qrCodes.Length; i++)
            {
                var qr = qrCodes[i];
                g.DrawImage(qr, 0, y);

                // Draw a horizontal separator line between QR codes (not after the last one)
                if (i < qrCodes.Length - 1)
                {
                    int dividerY = y + qr.Height + spacing / 2;
                    using (var pen = new Pen(Color.FromName(separatorColorName), separatorThickness))
                    {
                        // short left/right margins so line doesn't touch edges
                        int left = 8;
                        int right = width - 8;
                        pen.StartCap = LineCap.Round;
                        pen.EndCap = LineCap.Round;
                        g.DrawLine(pen, left, dividerY, right, dividerY);
                    }
                }

                // Advance y: for all but last add spacing, for last just add height
                y += qr.Height + (i < qrCodes.Length - 1 ? spacing : 0);
            }
        }

        return final;
    }

    static string SanitizeFileName(string input)
    {
        foreach (var c in Path.GetInvalidFileNameChars())
            input = input.Replace(c, '_');
        return input;
    }
}
