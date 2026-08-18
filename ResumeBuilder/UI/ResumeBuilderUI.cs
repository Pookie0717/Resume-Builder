using System.Drawing;
using System.Windows.Forms;
using Microsoft.Web.WebView2.WinForms;

namespace ResumeBuilder.UI
{
    public class ResumeBuilderUI
    {
        private Panel previewPanel = null!;
        private Panel controlsPanel = null!;
        public Panel PreviewPanel => previewPanel;
        public Panel ControlsPanel => controlsPanel;
        public WebView2 WebView { get; private set; } = null!;
        public TextBox TxtJobDescription { get; private set; } = null!;
        public Button BtnUploadResume { get; private set; } = null!;
        public Button BtnUpdateResume { get; private set; } = null!;
        public Button BtnDownloadResume { get; private set; } = null!;
        public Button BtnDownloadMD { get; private set; } = null!;
        public Label LblStatus { get; private set; } = null!;
        public TextBox TxtChatHistory { get; private set; } = null!;
        public TextBox TxtChatQuestion { get; private set; } = null!;
        public Button BtnSendQuestion { get; private set; } = null!;
        public Button BtnClearChat { get; private set; } = null!;
        public Label LblChatStatus { get; private set; } = null!;

        public void InitializeUI(Form form)
        {
            form.Text = "AI Resume Builder";
            form.Size = new Size(1200, 700);

            previewPanel = new Panel
            {
                Location = new Point(20, 20),
                Size = new Size(500, 640),
                BorderStyle = BorderStyle.FixedSingle
            };

            WebView = new WebView2
            {
                Dock = DockStyle.Fill
            };
            previewPanel.Controls.Add(WebView);

            controlsPanel = new Panel
            {
                Location = new Point(540, 20),
                Size = new Size(620, 640),
                BorderStyle = BorderStyle.FixedSingle
            };

            var tabControl = new TabControl
            {
                Dock = DockStyle.Fill
            };

            var resumeTab = new TabPage("Resume");
            var interviewTab = new TabPage("Interview Chat");

            BuildResumeTab(resumeTab);
            BuildInterviewTab(interviewTab);

            tabControl.TabPages.Add(resumeTab);
            tabControl.TabPages.Add(interviewTab);
            controlsPanel.Controls.Add(tabControl);

            form.Controls.AddRange(new Control[] {
                previewPanel,
                controlsPanel
            });

            previewPanel.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left;
            controlsPanel.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;

            string iconPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "appicon.ico");
            form.Icon = new Icon(iconPath);
        }

        private void BuildResumeTab(TabPage resumeTab)
        {
            BtnUploadResume = new Button
            {
                Text = "Upload Resume",
                Location = new Point(20, 20),
                Size = new Size(150, 30)
            };

            var lblJobDescription = new Label
            {
                Text = "Job Description:",
                Location = new Point(20, 70),
                Size = new Size(560, 20)
            };

            TxtJobDescription = new TextBox
            {
                Multiline = true,
                Location = new Point(20, 100),
                Size = new Size(560, 420),
                ScrollBars = ScrollBars.Vertical
            };

            BtnUpdateResume = new Button
            {
                Text = "Update Resume with AI",
                Location = new Point(20, 540),
                Size = new Size(200, 30)
            };

            LblStatus = new Label
            {
                Location = new Point(240, 545),
                Size = new Size(340, 20),
                ForeColor = Color.Blue
            };

            BtnDownloadResume = new Button
            {
                Text = "Download as PDF",
                Location = new Point(20, 580),
                Size = new Size(200, 30)
            };

            BtnDownloadMD = new Button
            {
                Text = "Download as Markdown",
                Location = new Point(240, 580),
                Size = new Size(200, 30)
            };

            resumeTab.Controls.AddRange(new Control[] {
                BtnUploadResume,
                lblJobDescription,
                TxtJobDescription,
                BtnUpdateResume,
                BtnDownloadResume,
                BtnDownloadMD,
                LblStatus
            });
        }

        private void BuildInterviewTab(TabPage interviewTab)
        {
            interviewTab.Padding = new Padding(12);

            var lblInstructions = new Label
            {
                Text = "Answers use your resume and job description from the Resume tab.",
                Dock = DockStyle.Top,
                Height = 24,
                AutoSize = false
            };

            var inputPanel = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 72
            };

            var lblQuestion = new Label
            {
                Text = "Your question:",
                Dock = DockStyle.Top,
                Height = 18
            };

            TxtChatQuestion = new TextBox
            {
                Dock = DockStyle.Top,
                Height = 26
            };

            var actionPanel = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 28
            };

            BtnSendQuestion = new Button
            {
                Text = "Get Answer",
                Location = new Point(0, 0),
                Size = new Size(110, 26)
            };

            BtnClearChat = new Button
            {
                Text = "Clear",
                Location = new Point(116, 0),
                Size = new Size(70, 26)
            };

            LblChatStatus = new Label
            {
                Location = new Point(196, 4),
                Size = new Size(344, 20),
                ForeColor = Color.Blue,
                AutoSize = false,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };

            TxtChatQuestion.KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.Enter)
                {
                    BtnSendQuestion.PerformClick();
                    e.SuppressKeyPress = true;
                }
            };

            actionPanel.Controls.Add(BtnSendQuestion);
            actionPanel.Controls.Add(BtnClearChat);
            actionPanel.Controls.Add(LblChatStatus);
            inputPanel.Controls.Add(actionPanel);
            inputPanel.Controls.Add(TxtChatQuestion);
            inputPanel.Controls.Add(lblQuestion);

            TxtChatHistory = new TextBox
            {
                Multiline = true,
                ReadOnly = true,
                Dock = DockStyle.Fill,
                ScrollBars = ScrollBars.Vertical,
                BackColor = Color.White
            };

            interviewTab.Controls.Add(TxtChatHistory);
            interviewTab.Controls.Add(inputPanel);
            interviewTab.Controls.Add(lblInstructions);
        }

        public void AppendChatMessage(string question, string answer)
        {
            if (TxtChatHistory.TextLength > 0)
            {
                TxtChatHistory.AppendText(Environment.NewLine + Environment.NewLine);
            }

            TxtChatHistory.AppendText($"Q: {question}{Environment.NewLine}{Environment.NewLine}A: {answer}");
            TxtChatHistory.SelectionStart = TxtChatHistory.TextLength;
            TxtChatHistory.ScrollToCaret();
        }

        public void ClearChatHistory()
        {
            TxtChatHistory.Clear();
            LblChatStatus.Text = string.Empty;
        }
    }
}
