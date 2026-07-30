using System.Text;
using Newtonsoft.Json;

namespace ResumeBuilder
{
    public class OpenAIService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private const string OPENAI_API_URL = "https://api.openai.com/v1/chat/completions";

        public OpenAIService(string apiKey)
        {
            _apiKey = apiKey;
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiKey}");
        }

        public async Task<string> OptimizeResume(string resume, string jobDescription)
        {
            try
            {
                var prompt = $@"Revise the provided resume to precisely match the attached job description. Implement STAR method in experience. Systematically integrate keywords and phrases from the job description into the 'Summary,' 'Key Skills,' and critically, each bullet point within the 'Work Experience' section. For each work experience entry, rephrase or add bullet points that directly demonstrate experience with technologies, methodologies, and responsibilities mentioned in the job description, such as C/C++, Java, Python, FORTRAN, Agile development, system integration, troubleshooting, and supporting radar systems (if applicable from the original resume).
                    Ensure the revised resume is:
                    * Highly relevant: Maximize alignment with the job description for ATS optimization.
                    * Detailed and professional: Provide specific examples and quantifiable achievements where applicable, maintaining a natural, human-written tone.
                    * Concise: Remove any irrelevant information not pertinent to the job description.
                    * Summary: Should be within 3 sentences.
                    * Experience: Should be within 3~4 bullet points. Should include what tech stack you used in the experience and what progress you made.

                    Format the entire resume in Markdown, strictly adhering to these rules:
                    * The first line must be the candidate's name using a single hash (`#`).
                    * All contact information (`LinkedIn URL`, `Phone Number`, `Email`, `Location`) must be on one single line, separated by a pipe (`|`).
                    * Use a triple hash (`##`) for top-level section headings (e.g., `## Summary`, `## Education`, `## Key Skills`, `## Work Experience`).
                    * For each work experience entry, put the header on separate lines with a blank line before the bullet points:
                      **Job Title**
                      Company Name – City, State / Remote
                      Month Year – Month Year

                      - First bullet point...
                    * Use `**` for bolding ONLY the job title line in each work experience entry. Do NOT bold keywords, technologies, skills, tools, or phrases anywhere else — not in Summary, Key Skills, Education, or bullet points. Weave job-description keywords into plain-text sentences naturally without highlighting them.
                    * Add a blank line between each work experience entry for readability.
                    * Ensure proper Markdown spacing and line breaks for readability.

                    Return only the complete, updated resume content in Markdown format, with no additional explanations or conversational text. The output should be ready for immediate use without further editing.
                    Resume:
                    ${resume}
                    Job Description:
                    ${jobDescription}";

                var requestBody = new
                {
                    model = "gpt-5.4",
                    messages = new[]
                    {
                        new { role = "user", content = prompt }
                    }
                };

                var content = new StringContent(
                    JsonConvert.SerializeObject(requestBody),
                    Encoding.UTF8,
                    "application/json");

                var response = await _httpClient.PostAsync(OPENAI_API_URL, content);

                response.EnsureSuccessStatusCode();

                var responseContent = await response.Content.ReadAsStringAsync();
                var responseObject = JsonConvert.DeserializeObject<dynamic>(responseContent);

                return responseObject.choices[0].message.content.ToString();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error calling OpenAI API: {ex.Message}", ex);
            }
        }
    }
}
