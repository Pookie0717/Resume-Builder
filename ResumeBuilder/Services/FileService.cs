using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser;
using DocumentFormat.OpenXml.Packaging;

namespace ResumeBuilder.Services
{
    public class FileService
    {
        public async Task<string> ExtractTextFromFile(string filePath)
        {
            string extension = Path.GetExtension(filePath).ToLower();
            string text = "";

            try
            {
                if (extension == ".pdf")
                {
                    using (PdfReader reader = new PdfReader(filePath))
                    using (PdfDocument pdf = new PdfDocument(reader))
                    {
                        for (int i = 1; i <= pdf.GetNumberOfPages(); i++)
                        {
                            text += PdfTextExtractor.GetTextFromPage(pdf.GetPage(i));
                        }
                    }
                }
                else if (extension == ".docx")
                {
                    using (WordprocessingDocument doc = WordprocessingDocument.Open(filePath, false))
                    {
                        text = doc.MainDocumentPart.Document.Body.InnerText;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error reading file: {ex.Message}", ex);
            }

            return text;
        }

        public void SaveTextToFile(string filePath, string content)
        {
            try
            {
                File.WriteAllText(filePath, content);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error saving file: {ex.Message}", ex);
            }
        }
    }
} 