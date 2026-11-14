using System.Drawing;
using System.Windows.Forms;

namespace Plot3D.UI
{
    /// <summary>
    /// Modal dialog that displays the project's MIT license text (read-only, selectable and scrollable).
    /// </summary>
    internal sealed class LicenseDialog : Form
    {
        // Keep in sync with the LICENSE file in the repository root.
        private const string LicenseText = @"MIT License

Copyright (c) 2026 David Bolsover

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the ""Software""), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED ""AS IS"", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.";

        private readonly TextBox _txtLicense;
        private readonly Button _btnClose;

        public LicenseDialog()
        {
            Text = "MIT License";
            Font = new Font("Segoe UI", 9F);
            AutoScaleMode = AutoScaleMode.Font;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterParent;
            ClientSize = new Size(560, 420);

            _txtLicense = new TextBox
            {
                Dock = DockStyle.Fill,
                Multiline = true,
                ReadOnly = true,
                ScrollBars = ScrollBars.Vertical,
                WordWrap = true,
                BorderStyle = BorderStyle.None,
                BackColor = Color.White,
                Font = new Font("Consolas", 9F),
                TabStop = false,
                Text = LicenseText.Replace("\r\n", "\n").Replace("\n", "\r\n")
            };

            _btnClose = new Button
            {
                Text = "&Close",
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                MinimumSize = new Size(80, 27),
                DialogResult = DialogResult.OK,
                Margin = new Padding(0)
            };

            var buttonPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Bottom,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                FlowDirection = FlowDirection.RightToLeft,
                WrapContents = false,
                Padding = new Padding(10)
            };
            buttonPanel.Controls.Add(_btnClose);

            // Add the fill control first so the bottom-docked panel is laid out before it.
            Controls.Add(_txtLicense);
            Controls.Add(buttonPanel);

            AcceptButton = _btnClose;
            CancelButton = _btnClose;
            ActiveControl = _btnClose;

            Load += (s, e) => _txtLicense.Select(0, 0);
        }
    }
}
