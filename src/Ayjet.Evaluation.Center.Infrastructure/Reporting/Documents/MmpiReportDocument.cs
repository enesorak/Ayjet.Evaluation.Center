using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using SkiaSharp;
using Ayjet.Evaluation.Center.Application.Features.TestResults.Models;
using Ayjet.Evaluation.Center.Application.Services.Scoring;
using Ayjet.Evaluation.Center.Infrastructure.Reporting.Helpers;

namespace Ayjet.Evaluation.Center.Infrastructure.Reporting.Documents;

/// <summary>
/// MMPI Test Sonuç Raporu - Frontend tasarımına uygun landscape layout
/// Sol: Chart + Tablo | Sağ: Kullanıcı Bilgileri
/// </summary>
public class MmpiReportDocument : IDocument
{
    private readonly MmpiReportModel _model;

    public MmpiReportDocument(MmpiReportModel model)
    {
        _model = model;
    }

    public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

    public void Compose(IDocumentContainer container)
    {
        container.Page(page =>
        {
            // LANDSCAPE (Yatay) Sayfa
            page.Size(PageSizes.A4.Landscape());
            page.Margin(10, Unit.Millimetre);
            page.PageColor(Colors.White);
            page.DefaultTextStyle(x => x.FontSize(9).FontFamily(Fonts.Arial));

            page.Header().Element(ComposeHeader);
            page.Content().Element(ComposeContent);
            
            page.Footer().AlignCenter().Text(text =>
            {
                text.Span("Sayfa ");
                text.CurrentPageNumber();
                text.Span(" / ");
                text.TotalPages();
            });
        });
    }

    void ComposeHeader(IContainer container)
    {
        container.Column(column =>
        {
            // Üst başlık satırı
            column.Item().Row(row =>
            {
              /*  row.RelativeItem().Text($"Print Tarihi: {DateTime.Now:dd MMMM yyyy HH:mm}")
                    .FontSize(9).FontColor(Colors.Grey.Darken1);
                
                row.RelativeItem().AlignRight().Text($"Test Uygulama Tarihi: {_model.CompletedAt?.ToString("dd MMMM yyyy HH:mm") ?? "-"}")
                    .FontSize(9).FontColor(Colors.Grey.Darken1);
                    */
            });

            // Alt çizgi
            column.Item().PaddingTop(5).BorderBottom(2).BorderColor(Colors.Grey.Lighten1);
        });
    }

    void ComposeContent(IContainer container)
    {
        container.PaddingTop(10).Row(mainRow =>
        {
            // SOL TARAF: Chart + Tablo (2/3 genişlik)
            mainRow.RelativeItem(2).Column(leftColumn =>
            {
                leftColumn.Spacing(10);

                // CHART BÖLÜMÜ
                leftColumn.Item().Background(Colors.White)
                    .Border(0f).BorderColor(Colors.Grey.Medium) // Kalın border
                    .Padding(0)
                    .Column(chartColumn =>
                    {
                    
                        chartColumn.Item().PaddingTop(0).Height(360)
                            .SkiaSharpSvgCanvas((canvas, size) =>
                            {
                                DrawLineChart(canvas, size);
                            });
                    });

                // TABLO BÖLÜMÜ
                leftColumn.Item().Background(Colors.White)
                    .Border(0).BorderColor(Colors.Grey.Medium) // Kalın border
                    .Padding(0)
                    .Column(tableColumn =>
                    { 
                        tableColumn.Item().PaddingTop(0).Element(ComposeScoreTable);
                    });
            });

            // SAĞ TARAF: Kullanıcı Bilgileri (1/3 genişlik)
            mainRow.RelativeItem(1).PaddingLeft(10).Column(rightColumn =>
            {
                rightColumn.Spacing(10);

                // ADAY BİLGİLERİ KARTI
                rightColumn.Item().Background(Colors.White)
                 
                    .Column(candidateColumn =>
                    {
                        // Fotoğraf + İsim başlık
                        candidateColumn.Item().Row(headerRow =>
                        {
                            // Fotoğraf placeholder (60x60)
                            headerRow.ConstantItem(60).Height(60)
                                .Border(2).BorderColor(Colors.Blue.Medium)
                                .Background(Colors.Blue.Lighten3)
                                .AlignCenter().AlignMiddle()
                                .Text(GetInitials(_model.Candidate.FullName))
                                .FontSize(22).Bold().FontColor(Colors.Blue.Darken2);

                            // İsim ve başlık
                            headerRow.RelativeItem().PaddingLeft(10).Column(nameColumn =>
                            {
                                nameColumn.Item().Text(_model.Candidate.FullName)
                                    .FontSize(14).Bold().FontColor(Colors.Grey.Darken3);
                                nameColumn.Item().Text("Aday Bilgileri")
                                    .FontSize(9).FontColor(Colors.Grey.Medium);
                            });
                        });

                        // Bilgi satırları
                        candidateColumn.Item().PaddingTop(12).Column(infoColumn =>
                        {
                            AddInfoRow(infoColumn, "Meslek:", _model.Candidate.Profession);
                            AddInfoRow(infoColumn, "Cinsiyet:", _model.Candidate.Gender);
                            AddInfoRow(infoColumn, "Yaş:", _model.Candidate.Age?.ToString());
                        });
                    });

                // SINAV KÜNYESİ KARTI
                rightColumn.Item().Background(Colors.White)
                    
                    .Column(testColumn =>
                    {
                        testColumn.Item().Text("SINAV KÜNYESİ")
                            .FontSize(10).Bold().FontColor(Colors.Grey.Darken2);

                        testColumn.Item().PaddingTop(10).Column(infoColumn =>
                        {
                            AddInfoRow(infoColumn, "Test Adı:", _model.TestTitle);
                            AddInfoRow(infoColumn, "Test Tipi:", "Psychometric");
                            AddInfoRow(infoColumn, "Tarih:", _model.CompletedAt?.ToString("dd MMMM yyyy HH:mm"));
                            AddInfoRow(infoColumn, "Tamamlanma Süresi:", CalculateDuration());
                        });
                    });
            });
        });
    }

    void ComposeScoreTable(IContainer container)
    {
        // Ölçek sıralaması - offset'siz, temiz
        var scales = new[] { "?", "L", "F", "K", "Hs", "D", "Hy", "Pd", "Mf", "Pa", "Pt", "Sc", "Ma", "Si" };
        var scaleLabels = new[] { "?", "L", "F", "K", "Hs", "D", "Hy", "Pd", "Mf", "Pa", "Pt", "Sc", "Ma", "Si" };

        var rawScores = _model.Scores.RawScores;
        var correctedScores = _model.Scores.CorrectedScores;

        container.Table(table =>
        {
            // Kolon tanımları - daha dengeli
            table.ColumnsDefinition(columns =>
            {
                columns.ConstantColumn(80); // "Ölçek" kolonu
                foreach (var _ in scales)
                    columns.RelativeColumn();
            });

            // BAŞLIK SATIRI
            table.Header(header =>
            {
                header.Cell().Element(HeaderCellStyle).Text("Ölçek").FontSize(9).Bold();
                foreach (var scale in scaleLabels)
                    header.Cell().Element(HeaderCellStyle).AlignCenter().Text(scale).FontSize(9).Bold();
                
                // Alt çizgi
                header.Cell().ColumnSpan((uint)scales.Length + 1).PaddingTop(3).BorderBottom(1).BorderColor(Colors.Grey.Medium);
            });

            // HAM PUAN SATIRI
            table.Cell().Element(CellStyle).Text("Ham Puan").FontSize(9).Bold();
            foreach (var scale in scales)
            {
                var score = rawScores.TryGetValue(scale, out var rawScore) 
                    ? rawScore.ToString() 
                    : "-";
                table.Cell().Element(CellStyle).AlignCenter().Text(score).FontSize(9);
            }

            // K+ SATIRI
            table.Cell().Element(CellStyle).Text("K+").FontSize(9).Bold();
            foreach (var scale in scales)
            {
                string score;
                
                // Geçerlilik ölçekleri için "-"
                if (new[] { "?", "L", "F", "K" ,"D","Hy","Mf","Pa","Si"}.Contains(scale))
                {
                    score = "-";
                }
                else
                {
                    // Klinik ölçekler için düzeltilmiş skor
                    score = correctedScores.TryGetValue(scale, out var correctedScore) 
                        ? Math.Round(correctedScore).ToString() 
                        : "-";
                }
                
                table.Cell().Element(CellStyle).AlignCenter().Text(score).FontSize(9);
            }
        });

        // Tablo hücre stilleri - daha temiz
        static IContainer HeaderCellStyle(IContainer container) =>
            container.BorderBottom(1).BorderColor(Colors.Grey.Lighten2)
                .Background(Colors.Grey.Lighten4)
                .Padding(6).AlignMiddle();

        static IContainer CellStyle(IContainer container) =>
            container.BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten3)
                .Padding(6).AlignMiddle();
    }

    void DrawLineChart(SKCanvas canvas, Size size)
    {
        var tScores = _model.Scores.TScores;
        if (tScores == null || tScores.Count == 0)
        {
            DrawPlaceholderText(canvas, "Grafik verisi bulunamadı");
            return;
        }

        var width = size.Width;
        var height = size.Height;
        
        // Padding'ler
        var leftPadding = 45f;
        var rightPadding = 25f;
        var topPadding = 35f;  // Legend için alan
        var bottomPadding = 45f;

        // Arka plan
        canvas.Clear(SKColors.White);

        // Y ekseni: 0-120
        var minY = 0f;
        var maxY = 120f;
        var yRange = maxY - minY;
        var chartHeight = height - topPadding - bottomPadding;
        var chartWidth = width - leftPadding - rightPadding;

        // 1. LEGEND ÇİZ (Sağ üst)
        DrawLegend(canvas, width, topPadding);

        // 2. GRİD ÇİZ
        DrawGridLines(canvas, width, height, leftPadding, rightPadding, topPadding, bottomPadding, minY, maxY, yRange, chartHeight);

        // 3. ORTALAMA ÇİZGİSİ (T=70)
        DrawAverageLine(canvas, leftPadding, rightPadding, bottomPadding, minY, yRange, chartWidth, chartHeight, topPadding);

        // 4. VERİ ÇİZGİSİ VE NOKTALAR
        DrawDataLine(canvas, leftPadding, rightPadding, topPadding, bottomPadding, minY, yRange, chartWidth, chartHeight, tScores);
    }

    void DrawLegend(SKCanvas canvas, float width, float topPadding)
    {
        var legendX = width - 120f;
        var legendY = 10f;
        
        // Kutu çerçevesi
        var boxPaint = new SKPaint
        {
            Color = new SKColor(220, 38, 38), // Kırmızı
            StrokeWidth = 2f,
            Style = SKPaintStyle.Stroke,
            IsAntialias = true
        };
        canvas.DrawRect(legendX, legendY, 100, 20, boxPaint);
        
        // Beyaz arka plan
        var bgPaint = new SKPaint
        {
            Color = SKColors.White,
            Style = SKPaintStyle.Fill
        };
        canvas.DrawRect(legendX + 1, legendY + 1, 98, 18, bgPaint);
        
        // Metin
        var textPaint = new SKPaint
        {
            Color = SKColors.Black,
            TextSize = 9,
            IsAntialias = true
        };
        canvas.DrawText("T-Puanları", legendX + 25, legendY + 14, textPaint);
    }

    void DrawGridLines(SKCanvas canvas, float width, float height, float leftPadding, float rightPadding, 
        float topPadding, float bottomPadding, float minY, float maxY, float yRange, float chartHeight)
    {
        // ÇOK İNCE AÇIK GRİ GRİD
        var gridPaint = new SKPaint
        {
            Color = new SKColor(230, 230, 230), // Çok açık gri
            StrokeWidth = 0.5f,
            IsAntialias = true,
            Style = SKPaintStyle.Stroke
        };

        // Y ekseni etiket paint
        var yLabelPaint = new SKPaint
        {
            Color = new SKColor(100, 100, 100),
            TextSize = 9,
            IsAntialias = true,
            TextAlign = SKTextAlign.Right
        };

        // Yatay grid çizgileri (her 10 birimde: 0, 10, 20, ..., 120)
        for (int i = 0; i <= 120; i += 10)
        {
            var y = height - bottomPadding - ((i - minY) / yRange * chartHeight);
            
            // Grid çizgisi
            canvas.DrawLine(leftPadding, y, width - rightPadding, y, gridPaint);

            // Y ekseni etiketi (sol tarafta)
            canvas.DrawText(i.ToString(), leftPadding - 8, y + 4, yLabelPaint);
        }
        
        // Sağ ve alt border çizgileri (chart kutusunu tamamla)
        var borderPaint = new SKPaint
        {
            Color = new SKColor(180, 180, 180),
            StrokeWidth = 1f,
            IsAntialias = true,
            Style = SKPaintStyle.Stroke
        };
        
        // Sol border
        canvas.DrawLine(leftPadding, topPadding, leftPadding, height - bottomPadding, borderPaint);
        // Alt border
        canvas.DrawLine(leftPadding, height - bottomPadding, width - rightPadding, height - bottomPadding, borderPaint);
    }

    void DrawAverageLine(SKCanvas canvas, float leftPadding, float rightPadding, float bottomPadding, 
        float minY, float yRange, float chartWidth, float chartHeight, float topPadding)
    {
        // T=70 çizgisi - SİYAH KESİKLİ
        var avgY = topPadding + chartHeight - ((70 - minY) / yRange * chartHeight);
        
        var avgPaint = new SKPaint
        {
            Color = SKColors.Black, // Siyah
            StrokeWidth = 1.5f,
            IsAntialias = true,
            Style = SKPaintStyle.Stroke,
            PathEffect = SKPathEffect.CreateDash(new[] { 5f, 3f }, 0) // Kesikli
        };
        
        canvas.DrawLine(leftPadding, avgY, leftPadding + chartWidth, avgY, avgPaint);
    }

    void DrawDataLine(SKCanvas canvas, float leftPadding, float rightPadding, float topPadding, 
        float bottomPadding, float minY, float yRange, float chartWidth, float chartHeight, 
        Dictionary<string, int> tScores)
    {
        // Ölçekler
        var scales = new[] { "?", "L", "F", "K", "Hs", "D", "Hy", "Pd", "Mf", "Pa", "Pt", "Sc", "Ma", "Si" };
        var labels = new[] { "?", "L", "F", "K", "Hs+5K", "D", "Hy", "Pd+4K", "Mf", "Pa", "Pt+1K", "Sc+1K", "Ma+2K", "Si" };
        
        // X pozisyonları hesapla
        var positions = CalculateXPositions(leftPadding, chartWidth, scales.Length);
        
        // ÇİZGİ ÇIZMEK İÇİN PATH
        var linePaint = new SKPaint
        {
            Color = new SKColor(220, 38, 38), // Parlak kırmızı
            StrokeWidth = 2f,
            IsAntialias = true,
            Style = SKPaintStyle.Stroke
        };

        // NOKTA PAINT
        var pointPaint = new SKPaint
        {
            Color = new SKColor(220, 38, 38), // Parlak kırmızı
            IsAntialias = true,
            Style = SKPaintStyle.Fill
        };

        // X EKSENİ ETİKET PAINT (ölçek isimleri)
        var labelPaint = new SKPaint
        {
            Color = SKColors.Black,
            TextSize = 8,
            IsAntialias = true,
            TextAlign = SKTextAlign.Center
        };

        // X EKSENİ DEĞER PAINT (T-Score sayıları)
        var valuePaint = new SKPaint
        {
            Color = SKColors.Black,
            TextSize = 9,
            IsAntialias = true,
            TextAlign = SKTextAlign.Center
        };

        // 3 GRUP TANIMLA: GAP'lerle ayrılmış gruplar
        // Grup 1: ? (tek başına)
        // Grup 2: L, F, K (birleşik)
        // Grup 3: Hs, D, Hy, Pd, Mf, Pa, Pt, Sc, Ma, Si (birleşik)
        
        var groups = new[]
        {
            new[] { "?" },                                                  // Grup 1
            new[] { "L", "F", "K" },                                       // Grup 2
            new[] { "Hs", "D", "Hy", "Pd", "Mf", "Pa", "Pt", "Sc", "Ma", "Si" }  // Grup 3
        };

        // Her grup için ayrı path oluştur
        for (int groupIndex = 0; groupIndex < groups.Length; groupIndex++)
        {
            var group = groups[groupIndex];
            var path = new SKPath();
            bool isFirstPointInGroup = true;
            
            // Bu gruptaki her ölçek için
            for (int i = 0; i < scales.Length; i++)
            {
                var scale = scales[i];
                
                // Bu ölçek bu grupta mı?
                if (!group.Contains(scale))
                    continue;
                
                var label = labels[i];
                var x = positions[i];
                var baseY = topPadding + chartHeight;

                // X ekseni etiketi (ölçek ismi - 1. satır)
                canvas.DrawText(label, x, baseY + 15, labelPaint);

                // T-Score var mı?
                if (tScores.TryGetValue(scale, out var tScore))
                {
                    // Y pozisyonu hesapla
                    var y = topPadding + chartHeight - ((tScore - minY) / yRange * chartHeight);

                    // Grup içinde çizgi çiz
                    if (isFirstPointInGroup)
                    {
                        path.MoveTo(x, y);
                        isFirstPointInGroup = false;
                    }
                    else
                    {
                        path.LineTo(x, y);  // ← Sadece GRUP İÇİNDE birleştir
                    }

                    // Nokta çiz
                    canvas.DrawCircle(x, y, 4f, pointPaint);
                    
                    // T-Score değerini yaz (2. satır)
                    canvas.DrawText(tScore.ToString(), x, baseY + 28, valuePaint);
                }
            }
            
            // Bu grubun path'ini çiz
            canvas.DrawPath(path, linePaint);
        }
    }

    float[] CalculateXPositions(float leftPadding, float chartWidth, int scaleCount)
    {
        // ? için offset
        var startOffset = 20f;
        
        // ? ile L arası gap
        var gapAfterQuestion = 20f;
        
        // K ile Hs arası gap
        var gapAfterK = 20f;
        
        // Kalan alan
        var remainingWidth = chartWidth - startOffset - gapAfterQuestion - gapAfterK;
        
        // Adım boyutu (13 adım: ? sonrası 3 + K sonrası 10)
        var step = remainingWidth / 13f;
        
        var positions = new float[scaleCount];
        var currentX = leftPadding + startOffset;
        
        for (int i = 0; i < scaleCount; i++)
        {
            positions[i] = currentX;
            currentX += step;
            
            // ? sonrası gap
            if (i == 0) currentX += gapAfterQuestion;
            // K sonrası gap
            else if (i == 3) currentX += gapAfterK;
        }
        
        return positions;
    }

    void DrawPlaceholderText(SKCanvas canvas, string text)
    {
        var paint = new SKPaint
        {
            Color = SKColors.Gray,
            TextSize = 12,
            IsAntialias = true
        };
        canvas.DrawText(text, 20, 100, paint);
    }

    void AddInfoRow(ColumnDescriptor column, string label, string value)
    {
        column.Item().PaddingVertical(3).Row(row =>
        {
            row.RelativeItem().Text(label).FontSize(9).Bold().FontColor(Colors.Grey.Darken2);
            row.RelativeItem().Text(value ?? "-").FontSize(9).FontColor(Colors.Black);
        });
    }

    string GetInitials(string fullName)
    {
        if (string.IsNullOrWhiteSpace(fullName)) return "?";

        var parts = fullName.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length >= 2)
        {
            return $"{parts[0][0]}{parts[1][0]}".ToUpper();
        }
        return parts[0].Substring(0, Math.Min(2, parts[0].Length)).ToUpper();
    }

    string CalculateDuration()
    {
        // Model'de StartedAt ve CompletedAt varsa hesapla
        // Yoksa placeholder döndür
        return "N/A"; // TODO: Model'e StartedAt eklenirse hesaplanacak
    }
}