using System.Text;
using System.Text.RegularExpressions;
using iText.Kernel.Colors;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Borders;
using iText.Layout.Element;
using iText.Layout.Properties;
using iText.Kernel.Font;

namespace ResumeBuilder.Services
{
    public class PdfService
    {
        private enum ResumeSection
        {
            Header,
            ProfessionalSummary,
            Skills,
            Experience,
            Education,
            LinkedIn
        }

        private static readonly DeviceRgb PrimaryBlue = new(57, 73, 171);
        private static readonly DeviceRgb DarkNavy = new(28, 36, 52);
        private static readonly DeviceRgb MutedGray = new(90, 100, 120);
        private static readonly Regex BoldPattern = new(@"\*\*(.+?)\*\*", RegexOptions.Compiled);
        private const float CompactLeading = 1.05f;

        public void SaveAsPdf(string filePath, string content)
        {
            try
            {
                using var writer = new PdfWriter(filePath);
                using var pdf = new PdfDocument(writer);
                using var document = new Document(pdf);
                document.SetMargins(40f, 48f, 40f, 48f);

                var font = PdfFontFactory.CreateFont(iText.IO.Font.Constants.StandardFonts.HELVETICA);
                var lines = CleanLines(content);
                var section = ResumeSection.Header;
                var headerLineCount = 0;
                var educationLineCount = 0;
                var summaryBuilder = new StringBuilder();
                var i = 0;

                while (i < lines.Count)
                {
                    var line = lines[i];

                    if (line.StartsWith("# "))
                    {
                        document.Add(CreateCenteredParagraph(font, StripMarkers(line[2..]), 22f, bold: true, color: DarkNavy, marginBottom: 2f));
                        headerLineCount = 0;
                        i++;
                        continue;
                    }

                    if (line.StartsWith("## "))
                    {
                        FlushSummary(document, font, summaryBuilder);

                        var heading = line[3..].Trim().ToUpperInvariant();
                        section = heading switch
                        {
                            "PROFESSIONAL SUMMARY" => ResumeSection.ProfessionalSummary,
                            "SKILLS" => ResumeSection.Skills,
                            "EXPERIENCE" or "WORK HISTORY" => ResumeSection.Experience,
                            "EDUCATION" => ResumeSection.Education,
                            "LINKEDIN" => ResumeSection.LinkedIn,
                            _ => section
                        };

                        if (section == ResumeSection.Education)
                        {
                            educationLineCount = 0;
                        }

                        document.Add(CreateSectionHeading(font, heading));
                        i++;
                        continue;
                    }

                    if (section == ResumeSection.Header)
                    {
                        if (headerLineCount == 0)
                        {
                            document.Add(CreateCenteredParagraph(font, FormatHeadlineLine(line), 12f, bold: false, color: PrimaryBlue, marginBottom: 1f));
                        }
                        else
                        {
                            document.Add(CreateCenteredParagraph(font, FormatContactLine(line), 10f, bold: false, color: MutedGray, marginBottom: 4f));
                        }

                        headerLineCount++;
                        i++;
                        continue;
                    }

                    if (section == ResumeSection.ProfessionalSummary)
                    {
                        AppendSummaryLine(summaryBuilder, line);
                        i++;
                        continue;
                    }

                    if (section == ResumeSection.Skills)
                    {
                        var skillText = IsBulletLine(line) ? line[2..].Trim() : line;
                        document.Add(CreateBulletParagraph(font, StripMarkers(skillText), leftIndent: 0f, marginBottom: 1f));
                        i++;
                        continue;
                    }

                    if (section == ResumeSection.Experience)
                    {
                        if (IsBulletLine(line))
                        {
                            document.Add(CreateBulletParagraph(font, StripMarkers(line[2..].Trim()), leftIndent: 14f, marginBottom: 1.5f));
                            i++;
                            continue;
                        }

                        if (line.StartsWith("Achievement:", StringComparison.OrdinalIgnoreCase))
                        {
                            document.Add(CreateAchievementParagraph(font, line));
                            document.Add(new Paragraph().SetMarginBottom(4f));
                            i++;
                            continue;
                        }

                        if (IsExperienceOrgLine(line))
                        {
                            document.Add(CreateExperienceOrgRow(font, line));
                            i++;
                            continue;
                        }

                        document.Add(CreateExperienceRoleRow(font, line));
                        i++;
                        continue;
                    }

                    if (section == ResumeSection.Education)
                    {
                        if (educationLineCount % 2 == 0)
                        {
                            document.Add(CreateParagraph(font, FormatEducationSchoolLine(line), 11f, bold: true, color: DarkNavy, marginTop: 4f, marginBottom: 0f));
                        }
                        else
                        {
                            document.Add(CreateParagraph(font, FormatEducationDetailLine(line), 10f, bold: false, italic: true, color: MutedGray, marginTop: 0f, marginBottom: 4f));
                        }

                        educationLineCount++;
                        i++;
                        continue;
                    }

                    if (section == ResumeSection.LinkedIn)
                    {
                        document.Add(CreateParagraph(font, StripMarkers(line), 10f, bold: false, color: MutedGray, marginTop: 0f, marginBottom: 2f));
                        i++;
                        continue;
                    }

                    document.Add(CreateParagraph(font, StripMarkers(line), 10.5f, bold: false, color: DarkNavy, marginTop: 0f, marginBottom: 2f));
                    i++;
                }

                FlushSummary(document, font, summaryBuilder);
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

        private static List<string> CleanLines(string content)
        {
            var cleanedText = content
                .Replace("```markdown", "", StringComparison.OrdinalIgnoreCase)
                .Replace("```", "")
                .Replace("$", "")
                .Replace("`", "");

            return cleanedText
                .Split(new[] { '\r', '\n' }, StringSplitOptions.None)
                .Select(line => line.Trim())
                .Where(line => !string.IsNullOrWhiteSpace(line))
                .ToList();
        }

        private static void AppendSummaryLine(StringBuilder summaryBuilder, string line)
        {
            if (summaryBuilder.Length > 0)
            {
                summaryBuilder.Append(' ');
            }

            summaryBuilder.Append(StripMarkers(line));
        }

        private static void FlushSummary(Document document, PdfFont font, StringBuilder summaryBuilder)
        {
            if (summaryBuilder.Length == 0)
            {
                return;
            }

            document.Add(CreateParagraph(font, summaryBuilder.ToString().Trim(), 10.5f, bold: false, color: DarkNavy, marginTop: 0f, marginBottom: 4f));
            summaryBuilder.Clear();
        }

        private static Paragraph CreateSectionHeading(PdfFont font, string heading)
        {
            var paragraph = new Paragraph()
                .SetMarginTop(6f)
                .SetMarginBottom(3f)
                .SetPaddingBottom(2f)
                .SetMultipliedLeading(CompactLeading)
                .SetBorderBottom(new SolidBorder(PrimaryBlue, 0.75f));
            paragraph.Add(CreateText(font, heading, 12.5f, bold: true, PrimaryBlue));
            return paragraph;
        }

        private static Table CreateExperienceRoleRow(PdfFont font, string line)
        {
            var (title, location) = SplitExperienceParts(line);
            return CreateTwoColumnRow(
                font,
                title,
                location,
                marginTop: 5f,
                marginBottom: 0f,
                leftFontSize: 11.5f,
                rightFontSize: 9f,
                leftBold: true,
                leftItalic: false,
                leftColor: DarkNavy,
                rightColor: MutedGray);
        }

        private static Table CreateExperienceOrgRow(PdfFont font, string line)
        {
            var (company, duration) = SplitExperienceParts(line);
            return CreateTwoColumnRow(
                font,
                company,
                duration,
                marginTop: 0f,
                marginBottom: 2f,
                leftFontSize: 10.5f,
                rightFontSize: 9f,
                leftBold: false,
                leftItalic: true,
                leftColor: PrimaryBlue,
                rightColor: MutedGray);
        }

        private static Table CreateTwoColumnRow(
            PdfFont font,
            string leftText,
            string rightText,
            float marginTop,
            float marginBottom,
            float leftFontSize,
            float rightFontSize,
            bool leftBold,
            bool leftItalic,
            DeviceRgb leftColor,
            DeviceRgb rightColor)
        {
            var table = new Table(UnitValue.CreatePercentArray(new float[] { 1f, 1f }))
                .UseAllAvailableWidth()
                .SetMarginTop(marginTop)
                .SetMarginBottom(marginBottom);

            var leftCell = new Cell()
                .SetBorder(Border.NO_BORDER)
                .SetPadding(0f)
                .SetTextAlignment(TextAlignment.LEFT);
            leftCell.Add(new Paragraph().SetMultipliedLeading(CompactLeading).SetMargin(0f).Add(CreateText(font, leftText, leftFontSize, leftBold, leftColor, leftItalic)));

            var rightCell = new Cell()
                .SetBorder(Border.NO_BORDER)
                .SetPadding(0f)
                .SetTextAlignment(TextAlignment.RIGHT);
            rightCell.Add(new Paragraph().SetMultipliedLeading(CompactLeading).SetMargin(0f).Add(CreateText(font, rightText, rightFontSize, bold: false, rightColor)));

            table.AddCell(leftCell);
            table.AddCell(rightCell);
            return table;
        }

        private static (string Left, string Right) SplitExperienceParts(string line)
        {
            var cleaned = StripMarkers(line);
            if (!cleaned.Contains('|'))
            {
                if (LooksLikeDateRange(cleaned))
                {
                    return (string.Empty, cleaned);
                }

                return (cleaned, string.Empty);
            }

            var parts = cleaned.Split('|', 2, StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 1)
            {
                return LooksLikeDateRange(parts[0]) ? (string.Empty, parts[0]) : (parts[0], string.Empty);
            }

            return (parts[0], parts[1]);
        }

        private static Paragraph CreateCenteredParagraph(PdfFont font, string text, float fontSize, bool bold, DeviceRgb color, float marginBottom)
        {
            var paragraph = new Paragraph()
                .SetTextAlignment(TextAlignment.CENTER)
                .SetMarginTop(0f)
                .SetMarginBottom(marginBottom)
                .SetMultipliedLeading(CompactLeading);
            paragraph.Add(CreateText(font, text, fontSize, bold, color));
            return paragraph;
        }

        private static Paragraph CreateParagraph(PdfFont font, string text, float fontSize, bool bold, DeviceRgb color, float marginTop, float marginBottom, bool italic = false)
        {
            var paragraph = new Paragraph()
                .SetMarginTop(marginTop)
                .SetMarginBottom(marginBottom)
                .SetMultipliedLeading(CompactLeading);
            paragraph.Add(CreateText(font, text, fontSize, bold, color, italic));
            return paragraph;
        }

        private static Paragraph CreateBulletParagraph(PdfFont font, string text, float leftIndent, float marginBottom)
        {
            var paragraph = new Paragraph()
                .SetMarginBottom(marginBottom)
                .SetMarginLeft(leftIndent)
                .SetFirstLineIndent(-10f)
                .SetMultipliedLeading(CompactLeading);
            paragraph.Add(CreateText(font, "\u2022 ", 10.5f, bold: false, DarkNavy));
            paragraph.Add(CreateText(font, text, 10.5f, bold: false, DarkNavy));
            return paragraph;
        }

        private static Paragraph CreateAchievementParagraph(PdfFont font, string line)
        {
            var paragraph = new Paragraph()
                .SetMarginTop(2f)
                .SetMarginBottom(1f)
                .SetMarginLeft(14f)
                .SetMultipliedLeading(CompactLeading);

            var achievementText = StripMarkers(line);
            const string prefix = "Achievement:";
            if (achievementText.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            {
                paragraph.Add(CreateText(font, prefix + " ", 10f, bold: true, MutedGray));
                paragraph.Add(CreateText(font, achievementText[prefix.Length..].Trim(), 10f, bold: false, DarkNavy));
            }
            else
            {
                paragraph.Add(CreateText(font, achievementText, 10f, bold: false, DarkNavy));
            }

            return paragraph;
        }

        private static bool IsBulletLine(string line) =>
            line.StartsWith("- ") || line.StartsWith("* ") || line.StartsWith("\u2022 ");

        private static bool IsExperienceOrgLine(string line)
        {
            if (IsBulletLine(line) || line.StartsWith("Achievement:", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            if (!line.Contains('|'))
            {
                return LooksLikeDateRange(line);
            }

            var parts = line.Split('|', 2);
            return parts.Length == 2 && LooksLikeDateRange(parts[1]);
        }

        private static bool LooksLikeDateRange(string text)
        {
            var normalized = text.Trim();
            return normalized.Contains("Present", StringComparison.OrdinalIgnoreCase) ||
                   Regex.IsMatch(normalized, @"\b(19|20)\d{2}\b") ||
                   Regex.IsMatch(normalized, @"\b(Jan|Feb|Mar|Apr|May|Jun|Jul|Aug|Sep|Oct|Nov|Dec)\b", RegexOptions.IgnoreCase);
        }

        private static string FormatHeadlineLine(string line) => StripMarkers(line);

        private static string FormatContactLine(string line)
        {
            var parts = StripMarkers(line)
                .Split('|', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);

            return parts.Length <= 1
                ? StripMarkers(line)
                : string.Join("   \u2022   ", parts);
        }

        private static string FormatEducationSchoolLine(string line)
        {
            var cleaned = StripMarkers(line);
            if (!cleaned.Contains('|'))
            {
                return cleaned;
            }

            var parts = cleaned.Split('|', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
            return parts.Length >= 2 ? $"{parts[0]} {parts[1]}" : cleaned.Replace("|", " ").Trim();
        }

        private static string FormatEducationDetailLine(string line)
        {
            var cleaned = StripMarkers(line);
            if (!cleaned.Contains('|'))
            {
                return cleaned;
            }

            var parts = cleaned.Split('|', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
            return parts.Length >= 2 ? $"{parts[0]}   \u2022   {parts[1]}" : cleaned.Replace("|", " \u2022 ").Trim();
        }

        private static Text CreateText(PdfFont font, string text, float fontSize, bool bold, DeviceRgb color, bool italic = false)
        {
            var element = new Text(text).SetFont(font).SetFontSize(fontSize).SetFontColor(color);
            if (bold)
            {
                element.SetBold();
            }

            if (italic)
            {
                element.SetItalic();
            }

            return element;
        }

        private static string StripMarkers(string text)
        {
            var stripped = BoldPattern.Replace(text, "$1");
            return stripped.Replace("**", "").Trim();
        }
    }
}
