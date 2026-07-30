using System.Text.RegularExpressions;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Kernel.Font;

namespace ResumeBuilder.Services
{
    public class PdfService
    {
        private static readonly Regex BoldPattern = new(@"\*\*(.+?)\*\*", RegexOptions.Compiled);

        public void SaveAsPdf(string filePath, string content)
        {
            try
            {
                using (var writer = new PdfWriter(filePath))
                using (var pdf = new PdfDocument(writer))
                {
                    var document = new Document(pdf);
                    var font = PdfFontFactory.CreateFont(iText.IO.Font.Constants.StandardFonts.HELVETICA);

                    var cleanedText = content
                        .Replace("```markdown", "")
                        .Replace("```", "")
                        .Replace("$", "")
                        .Replace("`", "");

                    var lines = cleanedText.Split(new[] { '\r', '\n' }, StringSplitOptions.None);

                    for (int i = 0; i < lines.Length; i++)
                    {
                        var line = lines[i].Trim();

                        if (string.IsNullOrWhiteSpace(line))
                        {
                            document.Add(new Paragraph().SetMarginBottom(4f));
                            continue;
                        }

                        try
                        {
                            document.Add(CreateParagraph(font, line));
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine($"Error processing line: {lines[i]}. Error: {ex.Message}");
                        }
                    }
                }
            }
            catch (IOException ioEx)
            {
                throw new Exception($"PDF Generation Error: {ioEx.Message}", ioEx);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error saving PDF: {ex.Message}", ex);
            }
        }

        private static Paragraph CreateParagraph(PdfFont font, string line)
        {
            var paragraph = new Paragraph();

            if (line.StartsWith("# "))
            {
                paragraph.Add(CreateText(font, line[2..].Trim(), 16f, bold: false));
                paragraph.SetMarginBottom(4f);
            }
            else if (line.StartsWith("## "))
            {
                paragraph.Add(CreateText(font, line[3..].Trim(), 13f, bold: true));
                paragraph.SetMarginTop(8f).SetMarginBottom(4f);
            }
            else if (line.StartsWith("### "))
            {
                AddJobTitleLine(paragraph, font, line[4..].Trim());
                paragraph.SetMarginTop(6f).SetMarginBottom(1f);
            }
            else if (line.StartsWith("- ") || line.StartsWith("* "))
            {
                var bulletText = StripBoldMarkers(line[2..].Trim());
                paragraph.Add(CreateText(font, "• " + bulletText, 10f, bold: false));
                paragraph.SetMarginBottom(2f).SetMarginLeft(12f);
            }
            else if (line.StartsWith("**") && line.EndsWith("**"))
            {
                AddJobTitleLine(paragraph, font, line);
                paragraph.SetMarginTop(6f).SetMarginBottom(1f);
            }
            else
            {
                paragraph.Add(CreateText(font, StripBoldMarkers(line), 10f, bold: false));
                paragraph.SetMarginBottom(2f);
            }

            return paragraph;
        }

        private static void AddJobTitleLine(Paragraph paragraph, PdfFont font, string line)
        {
            var match = BoldPattern.Match(line);
            if (match.Success)
            {
                if (match.Index > 0)
                {
                    paragraph.Add(CreateText(font, line[..match.Index], 11f, bold: false));
                }

                paragraph.Add(CreateText(font, match.Groups[1].Value, 11f, bold: true));

                var remainder = line[(match.Index + match.Length)..];
                if (!string.IsNullOrEmpty(remainder))
                {
                    paragraph.Add(CreateText(font, remainder, 11f, bold: false));
                }
            }
            else
            {
                paragraph.Add(CreateText(font, StripBoldMarkers(line), 11f, bold: true));
            }
        }

        private static Text CreateText(PdfFont font, string text, float fontSize, bool bold)
        {
            var element = new Text(text).SetFont(font).SetFontSize(fontSize);
            if (bold)
            {
                element.SetBold();
            }

            return element;
        }

        private static string StripBoldMarkers(string text) => text.Replace("**", "");
    }
}
