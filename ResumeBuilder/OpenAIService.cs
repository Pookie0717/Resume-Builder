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
                var prompt = $@"ATS RESUME OPTIMIZATION ENGINE

Revise the provided resume to closely match the attached job description while keeping all information realistic and factually supported by the original resume.

GOAL:
Create a recruiter-friendly, ATS-optimized, highly relevant 2-page resume with approximately 950-1100 words.

1. KEYWORD OPTIMIZATION
Extract important keywords from the job description, including:
- Job title
- Programming languages
- Frameworks
- Cloud
- Databases
- APIs
- Architecture
- DevOps and CI/CD
- Security
- Testing
- AI tools
- Methodologies
- Domain knowledge
- Soft skills

Naturally integrate relevant keywords throughout the Professional Summary, Skills, and Experience. Prioritize exact job-description terminology when supported by the candidate's actual experience.

Do not keyword-stuff or invent unsupported experience.

2. PROFESSIONAL SUMMARY
- One dense paragraph.
- 4~5 sentences.
- Begin with ""12 years of experience...""
- Align closely with the target job title.
- Include relevant technical stack, system design, scalable platforms, APIs, architecture, cloud, databases, DevOps, delivery methodology, collaboration, and business impact.
- Naturally include 10-15 important job-description keywords.

3. SKILLS
Use 6-8 categorized lines:

- Category Name: skill1, skill2, skill3

Prioritize technologies and skills from the job description that are supported by the original resume.

Include AI development tools such as Cursor, Claude, GitHub Copilot, or ChatGPT when relevant and supported.

4. EXPERIENCE
Include EVERY role from the original resume.

Most recent 2 roles:
- 5-6 bullets each.

Older roles:
- Approximately 4 bullets each.

Each bullet must:
- Start with a strong past-tense action verb.
- Follow STAR principles naturally.
- Include technical context, engineering work, and outcome/impact.
- Integrate relevant job-description keywords.
- Use credible metrics where supported.
- Demonstrate architecture, system design, performance, cloud, CI/CD, security, testing, collaboration, or delivery where relevant.

Use strong verbs such as:
Architected, Designed, Engineered, Built, Developed, Implemented, Automated, Refactored, Scaled, Deployed, Optimized, Integrated, Modernized, Streamlined, Led.

Avoid:
assisted, helped, worked on, responsible for, participated in.

Do not invent metrics, technologies, responsibilities, or achievements.

End EVERY role with:

Achievement: One or two sentences describing the hardest problem solved in that role and its impact.

5. FORMATTING

Return ONLY the resume in Markdown.

Use exactly:

# Full Name
Job Title | Key Skill, Key Skill, Key Skill | Platform or Specialty
City, State | Phone | Email

## PROFESSIONAL SUMMARY
One paragraph.

## SKILLS
- Category Name: skill1, skill2, skill3

## EXPERIENCE

Job Title | City, State
Company | Mon Year - Mon Year
- Bullet
- Bullet
- Bullet
Achievement: ...

Job Title | City, State
Company | Mon Year - Mon Year
- Bullet
- Bullet
Achievement: ...

## EDUCATION

University Name | Start Year - End Year
Degree Name | City, State

## LINKEDIN
Linkedin profile

6. STRICT RULES
- Preserve all original employers, dates, locations, education, and factual information.
- Do not change historical job titles unless clearly supported by the original resume.
- Do not invent experience.
- Do not use bold, <b> tags, tables, icons, emojis, or graphics.
- Do not use em dashes.
- Use American English.
- Keep the resume approximately 450-650 words and optimized for exactly 2 pages in A4 paper.
- Remove irrelevant content.
- Prioritize recent and job-relevant experience.
- Keep bullets concise and results-oriented.
- Every experience entry must end with ""Achievement:"".
- Section headings must be exactly:
  ## PROFESSIONAL SUMMARY
  ## SKILLS
  ## EXPERIENCE
  ## EDUCATION
  ## LINKEDIN

FINAL CHECK:
Ensure strong ATS keyword alignment, natural keyword density, technical credibility, STAR-style experience bullets, measurable impact where credible, and a professional 2-page layout.

Return ONLY the complete revised resume.

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

        public async Task<string> AnswerInterviewQuestion(string question, string resume, string jobDescription)
        {
            if (string.IsNullOrWhiteSpace(question))
            {
                throw new ArgumentException("Please enter an interview question.");
            }

            if (string.IsNullOrWhiteSpace(resume))
            {
                throw new ArgumentException("Please upload a resume first.");
            }

            if (string.IsNullOrWhiteSpace(jobDescription))
            {
                throw new ArgumentException("Please enter a job description on the Resume tab first.");
            }

            var prompt = $@"You are an interview coach helping a candidate prepare spoken answers for a specific job.

The answer MUST be grounded in BOTH the resume and the job description below. Do not give generic interview advice.

Rules:
- Write exactly 2 or 3 sentences total.
- Use only employers, roles, projects, technologies, and achievements that appear in the resume.
- Tie the answer to requirements, responsibilities, or keywords from the job description.
- Mention relevant resume experience that proves fit for this job.
- Answer in a confident, natural, first-person interview tone.
- Do not invent employers, projects, metrics, or technologies.
- Do not use bullet points, headings, or labels.
- Return only the answer text.

Resume:
{resume}

Job Description:
{jobDescription}

Interview Question:
{question}";

            try
            {
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
