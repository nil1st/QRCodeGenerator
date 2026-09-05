namespace QRCodeGenerator;

internal class QrOptions
{
    public int PixelDensity { get; set; } = 20;
    public int Spacing { get; set; } = 60;
    public string SeparatorColor { get; set; } = "Black";
    public float SeparatorThickness { get; set; } = 3f;
    public string DefaultAuth { get; set; } = "WPA";
}
