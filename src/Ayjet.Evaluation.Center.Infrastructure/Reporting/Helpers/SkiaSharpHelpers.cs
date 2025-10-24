using System.Text;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
using SkiaSharp;

namespace Ayjet.Evaluation.Center.Infrastructure.Reporting.Helpers;

/// <summary>
/// QuestPDF 2024.3.0+ için SkiaSharp entegrasyonu
/// Kaynak: https://www.questpdf.com/api-reference/skiasharp-integration.html
/// </summary>
public static class SkiaSharpHelpers
{
    /// <summary>
    /// SkiaSharp ile SVG canvas oluşturur (vektör grafik - ölçeklenebilir)
    /// Chart ve grafikler için önerilir
    /// </summary>
    public static void SkiaSharpSvgCanvas(this IContainer container, Action<SKCanvas, Size> drawOnCanvas)
    {
        container.Svg(size =>
        {
            using var stream = new MemoryStream();
            using (var canvas = SKSvgCanvas.Create(new SKRect(0, 0, size.Width, size.Height), stream))
                drawOnCanvas(canvas, size);

            var svgData = stream.ToArray();
            return Encoding.UTF8.GetString(svgData);
        });
    }

    /// <summary>
    /// SkiaSharp ile raster (bitmap) canvas oluşturur
    /// Özel efektler (shadow, blur, vs.) için kullanılabilir
    /// Performans: SVG'den daha yavaş olabilir
    /// </summary>
    public static void SkiaSharpRasterizedCanvas(this IContainer container, Action<SKCanvas, ImageSize> drawOnCanvas)
    {
        container.Image(payload =>
        {
            using var bitmap = new SKBitmap(payload.ImageSize.Width, payload.ImageSize.Height);
            using (var canvas = new SKCanvas(bitmap))
            {
                canvas.Scale(
                    payload.ImageSize.Width / payload.AvailableSpace.Width,
                    payload.ImageSize.Height / payload.AvailableSpace.Height);

                drawOnCanvas(canvas, new ImageSize(
                    (int)payload.AvailableSpace.Width,
                    (int)payload.AvailableSpace.Height));
            }

            using var image = SKImage.FromBitmap(bitmap);
            using var data = image.Encode(SKEncodedImageFormat.Png, 100);
            return data.ToArray();
        });
    }
}