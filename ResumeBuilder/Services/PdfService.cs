using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Kernel.Font;

namespace ResumeBuilder.Services
{
    public class PdfService
    {
        public void SaveAsPdf(string filePath, string content)
        {
            try
            {
                using (var writer = new PdfWriter(filePath))
                using (var pdf = new PdfDocument(writer))
                {
                    var document = new Document(pdf);
                    
                    // Set default font
                    var font = PdfFontFactory.CreateFont(iText.IO.Font.Constants.StandardFonts.HELVETICA);
                    
                    // Clean and split the text into lines
                    var cleanedText = content
                        .Replace("```markdown", "")
                        .Replace("```", "")
                        .Replace("$", "")
                        .Replace("`", "")
                        .Replace("*", "-");
                    
                    var lines = cleanedText.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                    
                    // Process all lines with consistent formatting
                    for (int i = 0; i < lines.Length; i++)
                    {
                        if (!string.IsNullOrWhiteSpace(lines[i]))
                        {
                            try
                            {
                                var paragraph = new Paragraph();
                                var line = lines[i].Trim();
                                
                                // Handle markdown headings
                                if (line.StartsWith("# "))
                                {
                                    // Main heading
                                    var text = new Text(line.Substring(2).Trim())
                                        .SetFont(font)
                                        .SetFontSize(16f);
                                    paragraph.Add(text);
                                }
                                else if (line.StartsWith("## "))
                                {
                                    // Subheading
                                    var text = new Text(line.Substring(3).Trim())
                                        .SetFont(font)
                                        .SetBold()
                                        .SetFontSize(13f);
                                    paragraph.Add(text);
                                }
                                else if (line.StartsWith("### "))
                                {
                                    // Subheading
                                    var text = new Text(line.Substring(3).Trim())
                                        .SetFont(font)
                                        .SetFontSize(12f);
                                    paragraph.Add(text);
                                }
                                else
                                {
                                    // Handle markdown-style formatting for regular text
                                    var parts = line.Split(new[] { "--" }, StringSplitOptions.None);
                                    for (int j = 0; j < parts.Length; j++)
                                    {
                                        var text = new Text(parts[j].Trim())
                                            .SetFont(font)
                                            .SetFontSize(10f); // Default content size
                                        
                                        // If this part is between ** markers, make it bold and add space after
                                        if (j % 2 == 1)
                                        {
                                            text.SetBold();
                                            text.SetText(" " + text.GetText() + " "); // Add space before and after bold text
                                        }
                                        paragraph.Add(text);
                                    }
                                }
                                
                                paragraph.SetMarginBottom(2f);
                                document.Add(paragraph);
                            }
                            catch (Exception ex)
                            {
                                System.Diagnostics.Debug.WriteLine($"Error processing line: {lines[i]}. Error: {ex.Message}");
                                continue;
                            }
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
    }
} 