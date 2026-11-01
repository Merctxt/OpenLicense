using OpenLicense.Application.DTOs;
using OpenLicense.Application.Services.Interfaces;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace OpenLicense.Infrastructure.Services;

public class PdfReportService : IPdfReportService
{
    public PdfReportService()
    {
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public async Task<byte[]> GenerateReportPdfAsync(ReportDataDto report)
    {
        var document = CreateDocument(report);
        using var stream = new MemoryStream();
        stream.SetLength(0);
        document.GeneratePdf(stream);
        return stream.ToArray();
    }

    private static IDocument CreateDocument(ReportDataDto report)
    {
        return QuestPDF.Fluent.Document.Create(document =>
        {
            document.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(40);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(x => x.FontFamily("Helvetica").FontSize(10).FontColor(Colors.Grey.Darken2));

                // HEADER — UM call
                page.Header().Column(col =>
                {
                    col.Item().Row(hr =>
                    {
                        hr.ConstantItem(4).Background(Colors.Blue.Medium);
                        hr.ConstantItem(6);
                        hr.RelativeItem().Text("OpenLicense").FontColor(Colors.Grey.Darken3).Bold().FontSize(26);
                    });
                    col.Item().PaddingTop(8).Row(hr2 =>
                    {
                        hr2.ConstantItem(4).Background(Colors.Blue.Medium);
                        hr2.ConstantItem(6);
                        hr2.RelativeItem().Text("License Report").FontColor(Colors.Grey.Darken2).Bold().FontSize(16);
                    });
                    col.Item().PaddingTop(4).Row(hr3 =>
                    {
                        hr3.ConstantItem(4).Background(Colors.Blue.Medium);
                        hr3.ConstantItem(6);
                        hr3.RelativeItem().Text($"Generated: {report.GeneratedAt:dddd, MMMM dd, yyyy 'at' HH:mm:ss UTC}").FontColor(Colors.Grey.Medium).FontSize(10);
                    });
                    col.Item().PaddingTop(2).Row(hr4 =>
                    {
                        hr4.ConstantItem(4).Background(Colors.Blue.Medium);
                        hr4.ConstantItem(6);
                        hr4.RelativeItem().Text($"User: {report.UserName}").FontColor(Colors.Grey.Medium).FontSize(10);
                    });
                });

                // CONTENT
                page.Content().Column(col =>
                {
                    col.Item().PaddingTop(20);

                    // --- Summary cards ---
                    col.Item().Border(1).BorderColor(Colors.Grey.Lighten2).Background(Colors.Grey.Lighten4).Padding(16).Column(cc =>
                    {
                        cc.Item().Text("Summary").FontColor(Colors.Grey.Darken3).Bold().FontSize(12);
                        cc.Item().PaddingTop(12).Row(cards =>
                        {
                            CardCell(cards.RelativeItem(), "Total Licenses", report.TotalLicenses.ToString(), Colors.Blue.Medium);
                            CardCell(cards.RelativeItem(), "Active", report.ActiveLicenses.ToString(), Colors.Green.Medium);
                            CardCell(cards.RelativeItem(), "Suspended", report.SuspendedLicenses.ToString(), Colors.Orange.Medium);
                            CardCell(cards.RelativeItem(), "Expiring Soon", report.ExpiringCount.ToString(), Colors.Amber.Medium);
                            CardCell(cards.RelativeItem(), "Expired", report.ExpiredCount.ToString(), Colors.Red.Medium);
                        });
                    });

                    // --- Product section ---
                    col.Item().PaddingTop(24).Text("Licenses by Product").FontColor(Colors.Grey.Darken3).Bold().FontSize(12);
                    col.Item().PaddingTop(8).Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn(3f);
                            columns.RelativeColumn(1.5f);
                            columns.RelativeColumn(1.5f);
                            columns.RelativeColumn(1f);
                            columns.RelativeColumn(2f);
                        });
                        foreach (var name in new[] { "Product", "Total / Max", "Active", "Suspended", "Usage" })
                            table.Cell().Border(1).BorderColor(Colors.Grey.Lighten2).Padding(6).Text(name);
                        if (report.LicenseSummaries != null)
                        {
                            foreach (var item in report.LicenseSummaries)
                            {
                                int pctVal = item.MaxLicenses > 0 ? (int)((float)item.TotalLicenses / item.MaxLicenses * 100f) : 0;
                                string barColor = pctVal >= 90 ? Colors.Red.Medium : pctVal >= 70 ? Colors.Orange.Medium : Colors.Green.Medium;
                                table.Cell().Border(1).BorderColor(Colors.Grey.Lighten2).Background(Colors.White).Padding(6).Text(item.ProductName);
                                table.Cell().Border(1).BorderColor(Colors.Grey.Lighten2).Background(Colors.White).Padding(6).Text($"{item.TotalLicenses} / {item.MaxLicenses}");
                                table.Cell().Border(1).BorderColor(Colors.Grey.Lighten2).Background(Colors.White).Padding(6).Text(item.ActiveLicenses.ToString());
                                table.Cell().Border(1).BorderColor(Colors.Grey.Lighten2).Background(Colors.White).Padding(6).Text(item.SuspendedLicenses.ToString());
                                table.Cell().Border(1).BorderColor(Colors.Grey.Lighten2).Padding(6).Row(barRow =>
                                {
                                    if (pctVal == 0)
                                    {
                                        barRow.RelativeItem().Background(Colors.Grey.Lighten3);
                                    }
                                    else if (pctVal == 100)
                                    {
                                        barRow.RelativeItem().Background(barColor);
                                    }
                                    else
                                    {
                                        barRow.RelativeItem(pctVal / 100f).Background(barColor);
                                        barRow.RelativeItem(1f - pctVal / 100f).Background(Colors.Grey.Lighten3);
                                    }
                                });
                            }
                        }
                    });

                    // --- Expiring section ---
                    if (report.ExpiringLicenses?.Count > 0)
                    {
                        col.Item().PaddingTop(24).Text("Expiring Within 7 Days").FontColor(Colors.Amber.Medium).Bold().FontSize(12);
                        col.Item().PaddingTop(8).Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(2.5f);
                                columns.RelativeColumn(2f);
                                columns.RelativeColumn(2.5f);
                                columns.RelativeColumn(1.5f);
                                columns.RelativeColumn(1f);
                            });
                            foreach (var name in new[] { "Name", "Product", "License Key", "Expires", "Days Left" })
                                table.Cell().Border(1).BorderColor(Colors.Grey.Lighten2).Padding(6).Text(name);
                            foreach (var item in report.ExpiringLicenses)
                            {
                                bool urgent = item.DaysRemaining <= 3;
                                string bg = urgent ? Colors.Red.Lighten4 : Colors.Amber.Lighten3;
                                string label = item.DaysRemaining == 1 ? $"{item.DaysRemaining} day (urgent)" : $"{item.DaysRemaining} days";
                                string txtColor = urgent ? Colors.Red.Medium : Colors.Orange.Medium;
                                table.Cell().Border(1).BorderColor(Colors.Grey.Lighten2).Background(bg).Padding(6).Text(item.LicenseName);
                                table.Cell().Border(1).BorderColor(Colors.Grey.Lighten2).Background(bg).Padding(6).Text(item.ProductName);
                                table.Cell().Border(1).BorderColor(Colors.Grey.Lighten2).Background(bg).Padding(6).Text(FormatKey(item.LicenseKey));
                                table.Cell().Border(1).BorderColor(Colors.Grey.Lighten2).Background(bg).Padding(6).Text(item.ExpiresAt.ToString("MMM dd, yyyy"));
                                table.Cell().Border(1).BorderColor(Colors.Grey.Lighten2).Background(bg).Padding(6).Text(label).FontColor(txtColor);
                            }
                        });
                    }

                    // --- Expired section ---
                    if (report.ExpiredLicenses?.Count > 0)
                    {
                        col.Item().PaddingTop(24).Text("Expired Licenses").FontColor(Colors.Red.Medium).Bold().FontSize(12);
                        col.Item().PaddingTop(8).Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(2.5f);
                                columns.RelativeColumn(2f);
                                columns.RelativeColumn(2.5f);
                                columns.RelativeColumn(1.5f);
                                columns.RelativeColumn(1f);
                            });
                            foreach (var name in new[] { "Name", "Product", "License Key", "Expired", "Days Overdue" })
                                table.Cell().Border(1).BorderColor(Colors.Grey.Lighten2).Padding(6).Text(name);
                            foreach (var item in report.ExpiredLicenses)
                            {
                                table.Cell().Border(1).BorderColor(Colors.Grey.Lighten2).Background(Colors.Red.Lighten4).Padding(6).Text(item.LicenseName);
                                table.Cell().Border(1).BorderColor(Colors.Grey.Lighten2).Background(Colors.Red.Lighten4).Padding(6).Text(item.ProductName);
                                table.Cell().Border(1).BorderColor(Colors.Grey.Lighten2).Background(Colors.Red.Lighten4).Padding(6).Text(FormatKey(item.LicenseKey));
                                table.Cell().Border(1).BorderColor(Colors.Grey.Lighten2).Background(Colors.Red.Lighten4).Padding(6).Text(item.ExpiresAt.ToString("MMM dd, yyyy"));
                                table.Cell().Border(1).BorderColor(Colors.Grey.Lighten2).Background(Colors.Red.Lighten4).Padding(6).Text($"{item.DaysExpired} days").FontColor(Colors.Red.Medium);
                            }
                        });
                    }
                });

                // FOOTER — UM call
                page.Footer().Column(col =>
                {
                    col.Item().PaddingTop(24).Row(fr =>
                    {
                        fr.ConstantItem(4).Background(Colors.Blue.Medium);
                        fr.ConstantItem(6);
                        fr.RelativeItem().Text("OpenLicense — License Management System").FontColor(Colors.Grey.Medium).FontSize(8);
                    });
                    col.Item().PaddingTop(8).Row(fr2 =>
                    {
                        fr2.ConstantItem(4).Background(Colors.Blue.Medium);
                        fr2.ConstantItem(6);
                        fr2.RelativeItem().Text($"© {DateTime.UtcNow:yyyy} OpenLicense. Confidential.").FontColor(Colors.Grey.Medium).FontSize(8);
                    });
                });
            });
        });
    }

    private static void CardCell(IContainer cell, string label, string value, string color)
    {
        cell.Column(cardCol =>
        {
            cardCol.Item().BorderLeft(4).BorderColor(color).PaddingLeft(8)
                .Text(value).FontColor(color).Bold().FontSize(22);
            cardCol.Item().PaddingTop(2).Text(label).FontColor(Colors.Grey.Medium).FontSize(9);
        });
    }

    private static string FormatKey(string key)
    {
        if (string.IsNullOrEmpty(key)) return string.Empty;
        if (key.Length == 20 && !key.Contains('-'))
            return $"{key[..4]}-{key[4..8]}-{key[8..12]}-{key[12..16]}-{key[16..]}";
        return key;
    }
}
