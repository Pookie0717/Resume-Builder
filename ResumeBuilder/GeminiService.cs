using System.Text;
using Newtonsoft.Json;

namespace ResumeBuilder
{
    public class GeminiService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private const string GEMINI_API_URL = "https://generativelanguage.googleapis.com/v1beta/models/gemini-2.0-flash:generateContent";
        

        public GeminiService(string apiKey)
        {
            _apiKey = apiKey;
            _httpClient = new HttpClient();
        }

        public async Task<string> OptimizeResume(string resume, string jobDescription)
        {
            try
            {
                var prompt = $@"Revise the provided resume to precisely match the attached job description. Systematically integrate keywords and phrases from the job description into the 'Summary,' 'Key Skills,' and critically, each bullet point within the 'Work Experience' section. For each work experience entry, rephrase or add bullet points that directly demonstrate experience with technologies, methodologies, and responsibilities mentioned in the job description, such as C/C++, Java, Python, FORTRAN, Agile development, system integration, troubleshooting, and supporting radar systems (if applicable from the original resume).
                    Ensure the revised resume is:
                    * Highly relevant: Maximize alignment with the job description for ATS optimization.
                    * Detailed and professional: Provide specific examples and quantifiable achievements where applicable, maintaining a natural, human-written tone.
                    * Concise: Remove any irrelevant information not pertinent to the job description.

                    Format the entire resume in Markdown, strictly adhering to these rules:
                    * The first line must be the candidate's name using a single hash (`#`).
                    * All contact information (`LinkedIn URL`, `Phone Number`, `Email`, `Location`) must be on one single line, separated by a pipe (`|`).
                    * Use a triple hash (`##`) for top-level section headings (e.g., `## Summary`, `## Education`, `## Key Skills`, `## Work Experience`, `## Clearance`).
                    * For each work experience entry, the `Job Title`, `Company Name`, `Location`, and `Employment Period` must be on one single line, separated by pipes (`|`). For example: `**Job Title** | Company Name – City, State / Remote | Month Year – Month Year`.
                    * Use `**` for bolding text.
                    * Ensure proper Markdown spacing and line breaks for readability.

                    Return only the complete, updated resume content in Markdown format, with no additional explanations or conversational text. The output should be ready for immediate use without further editing.
                    Resume:
                    ${resume}
                    Job Description:
                    ${jobDescription}";

                var requestBody = new
                {
                    contents = new[]
                    {
                        new
                        {
                            parts = new[]
                            {
                                new { text = prompt }
                            }
                        }
                    }
                };

                var content = new StringContent(
                    JsonConvert.SerializeObject(requestBody),
                    Encoding.UTF8,
                    "application/json");

                var response = await _httpClient.PostAsync(
                    $"{GEMINI_API_URL}?key={_apiKey}",
                    content);

                response.EnsureSuccessStatusCode();

                var responseContent = await response.Content.ReadAsStringAsync();
                var responseObject = JsonConvert.DeserializeObject<dynamic>(responseContent);

                return responseObject.candidates[0].content.parts[0].text.ToString();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error calling Gemini API: {ex.Message}", ex);
            }
        }
    }
} 