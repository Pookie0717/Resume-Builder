using ResumeBuilder.Services;
using ResumeBuilder.UI;

namespace ResumeBuilder
{
    public partial class Form1 : Form
    {
        private string currentResumePath;
        private string originalResumeText;
        private string currentResumeText;
        private string jobDescription;
        private readonly HttpClient httpClient;
        private readonly OpenAIService openAIService;
        private readonly PdfService pdfService;
        private readonly FileService fileService;
        private readonly ResumeBuilderUI ui;

        public Form1()
        {
            InitializeComponent();
            this.MaximizeBox = false;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            httpClient = new HttpClient();
            openAIService = new OpenAIService(ApiKeyLoader.GetOpenAIApiKey());
            pdfService = new PdfService();
            fileService = new FileService();
            ui = new ResumeBuilderUI();
            
            InitializeUI();
            WireUpEvents();
            this.Icon = new Icon("Resources/appicon.ico");
        }

        private void InitializeUI()
        {
            ui.InitializeUI(this);
            ui.PreviewPanel.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left;
            ui.ControlsPanel.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
        }

        private void WireUpEvents()
        {
            ui.BtnUploadResume.Click += BtnUploadResume_Click;
            ui.BtnUpdateResume.Click += BtnUpdateResume_Click;
            ui.BtnDownloadResume.Click += BtnDownloadResume_Click;
            ui.BtnDownloadMD.Click += BtnDownloadMD_Click;
            ui.BtnSendQuestion.Click += BtnSendQuestion_Click;
            ui.BtnClearChat.Click += BtnClearChat_Click;
            ui.TxtJobDescription.TextChanged += (s, e) => jobDescription = ui.TxtJobDescription.Text;
        }

        private async void BtnUploadResume_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "PDF files (*.pdf)|*.pdf|Word files (*.docx)|*.docx|All files (*.*)|*.*";
                openFileDialog.FilterIndex = 1;

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    currentResumePath = openFileDialog.FileName;
                    originalResumeText = await fileService.ExtractTextFromFile(currentResumePath);
                    currentResumeText = originalResumeText;
                    
                    // Display PDF preview
                    try
                    {
                        await ui.WebView.EnsureCoreWebView2Async();
                        ui.WebView.CoreWebView2.Navigate(new Uri(currentResumePath).AbsoluteUri);
                        ui.LblStatus.Text = "Resume uploaded successfully. Enter job description to update.";
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error loading preview: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private async void BtnUpdateResume_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(originalResumeText) || string.IsNullOrEmpty(jobDescription))
            {
                MessageBox.Show("Please upload a resume and enter a job description first.", "Missing Information", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                ui.LblStatus.Text = "Updating resume with AI...";
                ui.BtnUpdateResume.Enabled = false;
                
                string updatedResume = await GetAIUpdatedResume(originalResumeText, jobDescription);
                currentResumeText = updatedResume;

                // Save the updated resume to a temporary file
                string tempFile = Path.Combine(Path.GetTempPath(), "updated_resume.pdf");
                pdfService.SaveAsPdf(tempFile, currentResumeText);

                // Show the updated resume preview
                await ui.WebView.EnsureCoreWebView2Async();
                ui.WebView.CoreWebView2.Navigate(new Uri(tempFile).AbsoluteUri);
                
                ui.LblStatus.Text = "Resume updated successfully!";
                ui.BtnUpdateResume.Enabled = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error updating resume: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                ui.LblStatus.Text = "Error updating resume.";
                ui.BtnUpdateResume.Enabled = true;
            }
        }

        private void BtnDownloadResume_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(currentResumeText))
            {
                MessageBox.Show("Please update the resume first.", "No Content", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using (SaveFileDialog saveFileDialog = new SaveFileDialog())
            {
                saveFileDialog.Filter = "PDF files (*.pdf)|*.pdf";
                saveFileDialog.FilterIndex = 1;

                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        // Use the same PDF service that creates the preview
                        pdfService.SaveAsPdf(saveFileDialog.FileName, currentResumeText);
                        ui.LblStatus.Text = "Resume saved successfully!";
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error saving file: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        ui.LblStatus.Text = "Error saving resume.";
                    }
                }
            }
        }

        private void BtnDownloadMD_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(currentResumeText))
            {
                MessageBox.Show("Please update the resume first.", "No Content", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using (SaveFileDialog saveFileDialog = new SaveFileDialog())
            {
                saveFileDialog.Filter = "Markdown files (*.md)|*.md";
                saveFileDialog.FilterIndex = 1;

                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        fileService.SaveTextToFile(saveFileDialog.FileName, currentResumeText);
                        ui.LblStatus.Text = "Markdown resume saved successfully!";
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error saving file: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        ui.LblStatus.Text = "Error saving markdown resume.";
                    }
                }
            }
        }

        private async Task<string> GetAIUpdatedResume(string resume, string jobDescription)
        {
            try
            {
                return await openAIService.OptimizeResume(resume, jobDescription);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error optimizing resume: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return resume;
            }
        }

        private async void BtnSendQuestion_Click(object sender, EventArgs e)
        {
            var question = ui.TxtChatQuestion.Text.Trim();
            if (string.IsNullOrEmpty(question))
            {
                MessageBox.Show("Please enter an interview question.", "Missing Question", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (string.IsNullOrEmpty(originalResumeText))
            {
                MessageBox.Show("Please upload a resume first so answers can be based on your experience.", "Missing Resume", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (string.IsNullOrEmpty(jobDescription))
            {
                MessageBox.Show("Please enter a job description on the Resume tab first so answers match the target role.", "Missing Job Description", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                ui.BtnSendQuestion.Enabled = false;
                ui.LblChatStatus.Text = "Generating answer...";

                var resumeContext = string.IsNullOrEmpty(currentResumeText) ? originalResumeText : currentResumeText;
                var answer = await openAIService.AnswerInterviewQuestion(question, resumeContext, jobDescription);

                ui.AppendChatMessage(question, answer.Trim());
                ui.TxtChatQuestion.Clear();
                ui.LblChatStatus.Text = "Answer ready.";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error getting interview answer: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                ui.LblChatStatus.Text = "Error generating answer.";
            }
            finally
            {
                ui.BtnSendQuestion.Enabled = true;
            }
        }

        private void BtnClearChat_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(ui.TxtChatHistory.Text))
            {
                return;
            }

            var result = MessageBox.Show(
                "Clear all interview chat history?",
                "Clear Chat",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                ui.ClearChatHistory();
            }
        }
    }
}
