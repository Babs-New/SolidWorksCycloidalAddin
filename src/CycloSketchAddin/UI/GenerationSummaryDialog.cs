using System;
using System.Drawing;
using System.Windows.Forms;

namespace CycloSketchAddin.UI;

public static class GenerationSummaryDialog
{
    public static void Show(string details)
    {
        using var form = new Form
        {
            Text = "Generation Summary",
            StartPosition = FormStartPosition.CenterScreen,
            Width = 760,
            Height = 620,
            MinimumSize = new Size(620, 420),
            FormBorderStyle = FormBorderStyle.Sizable,
            MaximizeBox = true,
            MinimizeBox = true,
            BackColor = Color.White
        };

        var appIcon = AppWindowIcon.TryGetIcon();
        if (appIcon != null)
        {
            form.Icon = appIcon;
            form.ShowIcon = true;
        }

        var header = new Label
        {
            Dock = DockStyle.Top,
            Height = 46,
            TextAlign = ContentAlignment.MiddleLeft,
            Padding = new Padding(12, 0, 12, 0),
            Font = new Font("Segoe UI", 10.5f, FontStyle.Bold),
            Text = "Sketch generation completed"
        };

        var txtDetails = new TextBox
        {
            Dock = DockStyle.Fill,
            Multiline = true,
            ReadOnly = true,
            ScrollBars = ScrollBars.Vertical,
            Font = new Font("Consolas", 10f, FontStyle.Regular),
            Text = details
        };

        var footer = new FlowLayoutPanel
        {
            Dock = DockStyle.Bottom,
            Height = 56,
            FlowDirection = FlowDirection.RightToLeft,
            Padding = new Padding(8),
            BackColor = Color.FromArgb(245, 247, 250)
        };

        var btnClose = new Button
        {
            Text = "Close",
            Width = 120,
            Height = 34
        };
        btnClose.Click += (_, _) => form.Close();

        var btnCopy = new Button
        {
            Text = "Copy details",
            Width = 120,
            Height = 34
        };
        btnCopy.Click += (_, _) =>
        {
            try
            {
                Clipboard.SetText(details ?? string.Empty);
                MessageBox.Show("Generation details copied to clipboard.", "Copied", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Copy failed: {ex.Message}", "Copy error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        };

        footer.Controls.Add(btnClose);
        footer.Controls.Add(btnCopy);

        form.Controls.Add(txtDetails);
        form.Controls.Add(footer);
        form.Controls.Add(header);

        form.ShowDialog();
    }
}
