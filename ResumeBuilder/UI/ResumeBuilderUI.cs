using System;
using System.Drawing;
using System.Windows.Forms;
using Microsoft.Web.WebView2.WinForms;
using System.IO;

namespace ResumeBuilder.UI
{
    public class ResumeBuilderUI
    {
        private Panel previewPanel;
        private Panel controlsPanel;
        public Panel PreviewPanel => previewPanel;
        public Panel ControlsPanel => controlsPanel;
        public WebView2 WebView { get; private set; }
        public TextBox TxtJobDescription { get; private set; }
        public Button BtnUploadResume { get; private set; }
        public Button BtnUpdateResume { get; private set; }
        public Button BtnDownloadResume { get; private set; }
        public Button BtnDownloadMD { get; private set; }
        public Label LblStatus { get; private set; }

        public void InitializeUI(Form form)
        {
            form.Text = "AI Resume Builder";
            form.Size = new Size(1200, 700); // Remove static size for flexibility

            // Create preview panel
            previewPanel = new Panel
            {
                Location = new Point(20, 20),
                Size = new Size(500, 600),
                BorderStyle = BorderStyle.FixedSingle
            };

            // Create WebView2 for PDF preview
            WebView = new WebView2
            {
                Dock = DockStyle.Fill
            };
            previewPanel.Controls.Add(WebView);

            // Create controls panel
            controlsPanel = new Panel
            {
                Location = new Point(540, 20),
                Size = new Size(620, 600),
                BorderStyle = BorderStyle.FixedSingle
            };

            // Create controls
            BtnUploadResume = new Button
            {
                Text = "Upload Resume",
                Location = new Point(20, 20),
                Size = new Size(150, 30)
            };

            Label lblJobDescription = new Label
            {
                Text = "Job Description:",
                Location = new Point(20, 70),
                Size = new Size(580, 20)
            };

            TxtJobDescription = new TextBox
            {
                Multiline = true,
                Location = new Point(20, 100),
                Size = new Size(580, 400),
                ScrollBars = ScrollBars.Vertical
            };

            BtnUpdateResume = new Button
            {
                Text = "Update Resume with AI",
                Location = new Point(20, 520),
                Size = new Size(200, 30)
            };

            LblStatus = new Label
            {
                Location = new Point(240, 525),
                Size = new Size(360, 20),
                ForeColor = Color.Blue
            };

            BtnDownloadResume = new Button
            {
                Text = "Download as PDF",
                Location = new Point(20, 560),
                Size = new Size(200, 30)
            };

            BtnDownloadMD = new Button
            {
                Text = "Download as Markdown",
                Location = new Point(240, 560),
                Size = new Size(200, 30)
            };

            // Add controls to controls panel
            controlsPanel.Controls.AddRange(new Control[] {
                BtnUploadResume,
                lblJobDescription,
                TxtJobDescription,
                BtnUpdateResume,
                BtnDownloadResume,
                BtnDownloadMD,
                LblStatus
            });

            // Add panels to form
            form.Controls.AddRange(new Control[] {
                previewPanel,
                controlsPanel
            });

            previewPanel.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left;
            controlsPanel.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;

            string iconPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "appicon.ico");
            form.Icon = new Icon(iconPath);
        }
    }
} 