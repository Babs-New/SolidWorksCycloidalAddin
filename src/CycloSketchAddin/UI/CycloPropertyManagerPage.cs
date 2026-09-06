using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using CycloSketchAddin.Core;

namespace CycloSketchAddin.UI;

public sealed class CycloPropertyManagerPage
{
    private readonly Func<CycloParams, bool> _onGenerate;

    public CycloPropertyManagerPage(Func<CycloParams, bool> onGenerate)
    {
        _onGenerate = onGenerate;
    }

    public void Show()
    {
        var defaults = new CycloParams();

        using var form = new Form
        {
            Text = "Create Cycloidal Reducer Sketches",
            StartPosition = FormStartPosition.CenterScreen,
            Width = 800,
            Height = 840,
            FormBorderStyle = FormBorderStyle.FixedDialog,
            MaximizeBox = false,
            MinimizeBox = false,
            BackColor = Color.FromArgb(244, 247, 252)
        };

        var header = new Panel
        {
            Dock = DockStyle.Top,
            Height = 62,
            BackColor = Color.FromArgb(28, 66, 110)
        };

        var title = new Label
        {
            Text = "CycloSketch Add-In | Sketch Generator",
            ForeColor = Color.White,
            AutoSize = true,
            Left = 18,
            Top = 17,
            Font = UiFont(14F, FontStyle.Bold)
        };
        header.Controls.Add(title);

        var tabs = new TabControl
        {
            Dock = DockStyle.Fill,
            Font = UiFont(11.5F, FontStyle.Regular)
        };

        var tabNecessary = new TabPage("Necessary param") { BackColor = Color.FromArgb(250, 252, 255) };
        var tabOptional = new TabPage("optionary param") { BackColor = Color.FromArgb(250, 252, 255) };
        var tabDetailed = new TabPage("Detailed setting") { BackColor = Color.FromArgb(250, 252, 255) };
        tabs.TabPages.Add(tabNecessary);
        tabs.TabPages.Add(tabOptional);
        tabs.TabPages.Add(tabDetailed);

        var pnlNecessary = CreateTabPanel();
        var pnlOptional = CreateTabPanel();
        var pnlDetailed = CreateTabPanel();
        tabNecessary.Controls.Add(pnlNecessary);
        tabOptional.Controls.Add(pnlOptional);
        tabDetailed.Controls.Add(pnlDetailed);

        var btnTestView = new Button
        {
            Text = "Test view",
            Width = 112,
            Height = 31,
            Font = UiFont(10.5F, FontStyle.Regular),
            BackColor = Color.White,
            ForeColor = Color.FromArgb(35, 35, 35),
            FlatStyle = FlatStyle.Flat,
            Margin = new Padding(0, 0, 0, 10)
        };
        btnTestView.FlatAppearance.BorderColor = Color.FromArgb(198, 206, 217);
        pnlNecessary.Controls.Add(btnTestView);

        var picNecessary = CreatePreviewImageBox(
            new[] {  "preview_necessary_params.png" },
            BuildNecessaryFallbackImage,
            700,
            205);
        pnlNecessary.Controls.Add(picNecessary);

        var gbNecessary = CreateGroupBox("Necessary parameters", Color.FromArgb(40, 88, 140));
        pnlNecessary.Controls.Add(gbNecessary);

        var numReduction = CreateIntField(gbNecessary, "Reduction ratio", defaults.ReductionRatio, 2, 5000);
        var numEccentric = CreateDoubleField(gbNecessary, "Eccentric amount [mm]", defaults.EccentricMm, 0.001M, 500M, 3);
        var numRingPinDia = CreateDoubleField(gbNecessary, "Ring pin diameter [mm]", defaults.RingPinDiaMm, 0.001M, 10000M, 3);
        var numRingPinPitchDia = CreateDoubleField(gbNecessary, "Ring pin pitch diameter [mm]", defaults.RingPinPitchDiaMm, 0.001M, 100000M, 3);
        var numPlotPerTooth = CreateIntField(gbNecessary, "Cycloidal curve plot per tooth", defaults.PlotPerTooth, 5, 2000);

        var picOptional = CreatePreviewImageBox(
            new[] {  "preview_optional_params.png" },
            BuildOptionalFallbackImage,
            700,
            205);
        pnlOptional.Controls.Add(picOptional);

        var gbCenterHole = CreateGroupBox("Cycloidal gear center hole", Color.FromArgb(0, 122, 77));
        pnlOptional.Controls.Add(gbCenterHole);

        var chkCenterHole = CreateCheckField(gbCenterHole, "Draw center hole", defaults.DrawCenterHole);
        var numCenterHoleDia = CreateDoubleField(gbCenterHole, "Diameter [mm]", defaults.CenterHoleDiaMm, 0.001M, 10000M, 3);

        var gbAroundToOutput = CreateGroupBox("Cycloidal gear to output disk", Color.FromArgb(148, 74, 0));
        pnlOptional.Controls.Add(gbAroundToOutput);

        var chkAroundHoles = CreateCheckField(gbAroundToOutput, "Draw around hole", defaults.DrawAroundHoles);
        var chkOutputPins = CreateCheckField(gbAroundToOutput, "Draw output disk pin", defaults.DrawOutputDiskPins);

        var cmbSetAbout = CreateComboField(
            gbAroundToOutput,
            "Set about",
            new[] { "Cycloidal gear hole", "Output disk pin" },
            defaults.LinkOutputPinsToAroundHoles ? 0 : 1);

        var numAroundNum = CreateIntField(gbAroundToOutput, "Hole num", defaults.AroundHoleNum, 1, 1000);
        var numAroundDia = CreateDoubleField(gbAroundToOutput, "Hole diameter [mm]", defaults.AroundHoleDiaMm, 0.001M, 10000M, 3);
        var numAroundPosDia = CreateDoubleField(gbAroundToOutput, "Hole position diameter [mm]", defaults.AroundHolePositionDiaMm, 0.001M, 100000M, 3);

        var numOutputNum = CreateIntField(gbAroundToOutput, "Pin num", defaults.OutputPinNum, 1, 1000);
        var numOutputDia = CreateDoubleField(gbAroundToOutput, "Pin diameter [mm]", defaults.OutputPinDiaMm, 0.001M, 10000M, 3);
        var numOutputPosDia = CreateDoubleField(gbAroundToOutput, "Pin position diameter [mm]", defaults.OutputPinPositionDiaMm, 0.001M, 100000M, 3);

        var picDetailed = CreatePreviewImageBox(
            new[] { "preview_detailed_settings.png", "cyclo_Discription_Image_opt.png", "preview_optional_params.png" },
            BuildDetailedFallbackImage,
            700,
            205);
        pnlDetailed.Controls.Add(picDetailed);

        var gbDetailed = CreateGroupBox("Detailed setting", Color.FromArgb(86, 52, 135));
        pnlDetailed.Controls.Add(gbDetailed);

        var chkSeparateSketches = CreateCheckField(gbDetailed, "Separate sketch", defaults.SeparateSketches);

        var info = new Label
        {
            AutoSize = false,
            Height = 260,
            Dock = DockStyle.Top,
            Padding = new Padding(10),
            Font = UiFont(10.5F, FontStyle.Regular),
            Text =
                "Generated sketches (two-disc mode):\r\n" +
                "- SK_CYCLO_DISC_PROFILE_A\r\n" +
                "- SK_CYCLO_DISC_PROFILE_B_180DEG\r\n" +
                "- SK_RING_PINS_REFERENCE\r\n" +
                "- SK_CENTER_BORE_A\r\n" +
                "- SK_CENTER_BORE_B_180DEG\r\n" +
                "- SK_OUTPUT_HOLES_DISC_A\r\n" +
                "- SK_OUTPUT_HOLES_DISC_B_180DEG\r\n" +
                "- SK_OUTPUT_PINS_DISC\r\n" +
                "- SK_REFERENCE_AXES\r\n\r\n" +
                "Disc A: +e\r\n" +
                "Disc B: -e\r\n" +
                "Phase B: 180deg",
            ForeColor = Color.FromArgb(40, 40, 40)
        };
        gbDetailed.Controls.Add(info);

        Action updateUi = () =>
        {
            numCenterHoleDia.Enabled = chkCenterHole.Checked;

            bool aroundEnabled = chkAroundHoles.Checked;
            numAroundNum.Enabled = aroundEnabled;
            numAroundDia.Enabled = aroundEnabled;
            numAroundPosDia.Enabled = aroundEnabled;

            bool outputEnabled = chkOutputPins.Checked;
            cmbSetAbout.Enabled = outputEnabled;

            bool linkAround = cmbSetAbout.SelectedIndex <= 0;
            bool manualOutput = outputEnabled && !linkAround;
            numOutputNum.Enabled = manualOutput;
            numOutputDia.Enabled = manualOutput;
            numOutputPosDia.Enabled = manualOutput;
        };

        chkCenterHole.CheckedChanged += (_, _) => updateUi();
        chkAroundHoles.CheckedChanged += (_, _) => updateUi();
        chkOutputPins.CheckedChanged += (_, _) => updateUi();
        cmbSetAbout.SelectedIndexChanged += (_, _) => updateUi();

        updateUi();

        var panel = new Panel { Dock = DockStyle.Bottom, Height = 64 };
        var btnGenerate = new Button
        {
            Text = "Generate",
            Width = 130,
            Height = 38,
            Left = 500,
            Top = 12,
            BackColor = Color.FromArgb(35, 122, 80),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Font = UiFont(11F, FontStyle.Bold)
        };
        btnGenerate.FlatAppearance.BorderSize = 0;

        var btnCancel = new Button
        {
            Text = "Cancel",
            Width = 130,
            Height = 38,
            Left = 640,
            Top = 12,
            BackColor = Color.FromArgb(110, 110, 110),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Font = UiFont(11F, FontStyle.Regular)
        };
        btnCancel.FlatAppearance.BorderSize = 0;

        Func<CycloParams?> readParams = () =>
        {
            var p = new CycloParams
            {
                ReductionRatio = (int)numReduction.Value,
                EccentricMm = (double)numEccentric.Value,
                RingPinDiaMm = (double)numRingPinDia.Value,
                RingPinPitchDiaMm = (double)numRingPinPitchDia.Value,
                PlotPerTooth = (int)numPlotPerTooth.Value,

                DrawCenterHole = chkCenterHole.Checked,
                CenterHoleDiaMm = (double)numCenterHoleDia.Value,

                DrawAroundHoles = chkAroundHoles.Checked,
                DrawOutputDiskPins = chkOutputPins.Checked,
                LinkOutputPinsToAroundHoles = cmbSetAbout.SelectedIndex <= 0,

                AroundHoleNum = (int)numAroundNum.Value,
                AroundHoleDiaMm = (double)numAroundDia.Value,
                AroundHolePositionDiaMm = (double)numAroundPosDia.Value,

                OutputPinNum = (int)numOutputNum.Value,
                OutputPinDiaMm = (double)numOutputDia.Value,
                OutputPinPositionDiaMm = (double)numOutputPosDia.Value,

                SeparateSketches = chkSeparateSketches.Checked
            };

            if (!CycloValidator.TryValidate(p, out var error))
            {
                MessageBox.Show(error, "Invalid parameters", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return null;
            }

            return p;
        };

        btnTestView.Click += (_, _) =>
        {
            var p = readParams();
            if (p == null) return;
            _ = _onGenerate(p);
        };

        btnGenerate.Click += (_, _) =>
        {
            var p = readParams();
            if (p == null) return;

            if (_onGenerate(p))
            {
                form.DialogResult = DialogResult.OK;
                form.Close();
            }
        };

        btnCancel.Click += (_, _) => form.Close();

        panel.Controls.Add(btnGenerate);
        panel.Controls.Add(btnCancel);

        form.Controls.Add(tabs);
        form.Controls.Add(header);
        form.Controls.Add(panel);

        form.ShowDialog();
    }

    private static FlowLayoutPanel CreateTabPanel()
    {
        return new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
            AutoScroll = true,
            Padding = new Padding(10),
            BackColor = Color.FromArgb(250, 252, 255)
        };
    }

    private static Font UiFont(float size, FontStyle style)
    {
        string[] preferred = { "Roboto", "Calibri", "Arial" };
        foreach (var name in preferred)
        {
            try
            {
                return new Font(name, size, style, GraphicsUnit.Point);
            }
            catch
            {
            }
        }

        return new Font(SystemFonts.MessageBoxFont.FontFamily, size, style, GraphicsUnit.Point);
    }

    private static GroupBox CreateGroupBox(string title, Color accent)
    {
        var gb = new GroupBox
        {
            Text = title,
            Width = 720,
            AutoSize = true,
            Padding = new Padding(10, 24, 10, 10),
            ForeColor = accent,
            Font = UiFont(11.5F, FontStyle.Bold),
            Margin = new Padding(0, 0, 0, 10)
        };

        var layout = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
            AutoSize = true,
            BackColor = Color.White
        };
        gb.Controls.Add(layout);
        return gb;
    }

    private static FlowLayoutPanel EnsureGroupLayout(Control groupBox)
    {
        return (FlowLayoutPanel)groupBox.Controls[0];
    }

    private static NumericUpDown CreateIntField(Control groupBox, string label, int value, int min, int max)
    {
        var layout = EnsureGroupLayout(groupBox);
        var row = new TableLayoutPanel
        {
            ColumnCount = 2,
            Width = 690,
            Height = 40,
            Margin = new Padding(4)
        };
        row.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 65));
        row.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 35));

        var lbl = new Label
        {
            Text = label,
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleLeft,
            Font = UiFont(10.5F, FontStyle.Regular),
            ForeColor = Color.FromArgb(35, 35, 35)
        };

        var num = new NumericUpDown
        {
            Minimum = min,
            Maximum = max,
            Value = Math.Max(min, Math.Min(max, value)),
            DecimalPlaces = 0,
            Dock = DockStyle.Fill,
            Font = UiFont(10.5F, FontStyle.Regular)
        };

        row.Controls.Add(lbl, 0, 0);
        row.Controls.Add(num, 1, 0);
        layout.Controls.Add(row);
        return num;
    }

    private static NumericUpDown CreateDoubleField(Control groupBox, string label, double value, decimal min, decimal max, int decimals)
    {
        var layout = EnsureGroupLayout(groupBox);
        var row = new TableLayoutPanel
        {
            ColumnCount = 2,
            Width = 690,
            Height = 40,
            Margin = new Padding(4)
        };
        row.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 65));
        row.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 35));

        var lbl = new Label
        {
            Text = label,
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleLeft,
            Font = UiFont(10.5F, FontStyle.Regular),
            ForeColor = Color.FromArgb(35, 35, 35)
        };

        decimal clamped = (decimal)Math.Max((double)min, Math.Min((double)max, value));
        var num = new NumericUpDown
        {
            Minimum = min,
            Maximum = max,
            Value = clamped,
            DecimalPlaces = decimals,
            Increment = (decimal)Math.Pow(10, -decimals),
            Dock = DockStyle.Fill,
            Font = UiFont(10.5F, FontStyle.Regular)
        };

        row.Controls.Add(lbl, 0, 0);
        row.Controls.Add(num, 1, 0);
        layout.Controls.Add(row);
        return num;
    }

    private static CheckBox CreateCheckField(Control groupBox, string label, bool value)
    {
        var layout = EnsureGroupLayout(groupBox);
        var chk = new CheckBox
        {
            Text = label,
            Checked = value,
            AutoSize = true,
            Margin = new Padding(8, 6, 8, 6),
            Font = UiFont(10.5F, FontStyle.Regular),
            ForeColor = Color.FromArgb(30, 30, 30)
        };
        layout.Controls.Add(chk);
        return chk;
    }

    private static ComboBox CreateComboField(Control groupBox, string label, string[] items, int selectedIndex)
    {
        var layout = EnsureGroupLayout(groupBox);

        var row = new TableLayoutPanel
        {
            ColumnCount = 2,
            Width = 690,
            Height = 40,
            Margin = new Padding(4)
        };
        row.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 65));
        row.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 35));

        var lbl = new Label
        {
            Text = label,
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleLeft,
            Font = UiFont(10.5F, FontStyle.Regular),
            ForeColor = Color.FromArgb(35, 35, 35)
        };

        var combo = new ComboBox
        {
            Dock = DockStyle.Fill,
            DropDownStyle = ComboBoxStyle.DropDownList,
            Font = UiFont(10.5F, FontStyle.Regular)
        };
        combo.Items.AddRange(items);
        combo.SelectedIndex = Math.Max(0, Math.Min(items.Length - 1, selectedIndex));

        row.Controls.Add(lbl, 0, 0);
        row.Controls.Add(combo, 1, 0);
        layout.Controls.Add(row);

        return combo;
    }

    private static PictureBox CreatePreviewImageBox(string[] fileNames, Func<int, int, Image> fallbackFactory, int width, int height)
    {
        var pb = new PictureBox
        {
            Width = width,
            Height = height,
            SizeMode = PictureBoxSizeMode.Zoom,
            BorderStyle = BorderStyle.FixedSingle,
            BackColor = Color.White,
            Margin = new Padding(0, 0, 0, 10)
        };

        string? foundPath = null;
        foreach (var fileName in fileNames)
        {
            foreach (var assetDir in ResolveAssetDirectories())
            {
                var candidate = Path.Combine(assetDir, fileName);
                if (File.Exists(candidate))
                {
                    foundPath = candidate;
                    break;
                }
            }

            if (!string.IsNullOrWhiteSpace(foundPath))
                break;
        }

        if (!string.IsNullOrWhiteSpace(foundPath))
        {
            pb.Image = Image.FromFile(foundPath);
        }
        else
        {
            pb.Image = fallbackFactory(width, height);
        }

        return pb;
    }

    private static string[] ResolveAssetDirectories()
    {
        var results = new System.Collections.Generic.List<string>();

        void add(string? p)
        {
            if (string.IsNullOrWhiteSpace(p)) return;
            if (!Directory.Exists(p)) return;
            bool exists = false;
            foreach (var existing in results)
            {
                if (string.Equals(existing, p, StringComparison.OrdinalIgnoreCase))
                {
                    exists = true;
                    break;
                }
            }

            if (!exists)
                results.Add(p);
        }

        add(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "assets"));

        var asmDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        add(Path.Combine(asmDir ?? string.Empty, "assets"));

        var cur = new DirectoryInfo(asmDir ?? AppDomain.CurrentDomain.BaseDirectory);
        for (int i = 0; i < 8 && cur != null; i++)
        {
            add(Path.Combine(cur.FullName, "assets"));
            cur = cur.Parent;
        }

        return results.ToArray();
    }

    private static Image BuildNecessaryFallbackImage(int width, int height)
    {
        return BuildFallbackImage(width, height, DrawNecessary);
    }

    private static Image BuildOptionalFallbackImage(int width, int height)
    {
        return BuildFallbackImage(width, height, DrawOptional);
    }

    private static Image BuildDetailedFallbackImage(int width, int height)
    {
        return BuildFallbackImage(width, height, DrawDetailed);
    }

    private static Image BuildFallbackImage(int width, int height, Action<Graphics, Rectangle> painter)
    {
        var bmp = new Bitmap(width, height);
        using var g = Graphics.FromImage(bmp);
        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.Clear(Color.White);
        painter(g, new Rectangle(0, 0, width, height));
        return bmp;
    }

    private static void DrawNecessary(Graphics g, Rectangle r)
    {
        float cy = r.Height * 0.62f;
        float cxA = r.Width * 0.36f;
        float cxB = r.Width * 0.64f;
        using var ringPen = new Pen(Color.FromArgb(130, 130, 130), 2f);
        using var axisPen = new Pen(Color.FromArgb(88, 88, 88), 1f) { DashStyle = DashStyle.Dash };
        using var discPenA = new Pen(Color.FromArgb(35, 122, 80), 2.2f);
        using var discPenB = new Pen(Color.FromArgb(200, 90, 20), 2.2f);
        using var txt = UiFont(11F, FontStyle.Bold);

        g.DrawEllipse(ringPen, r.Width * 0.2f, r.Height * 0.30f, r.Width * 0.6f, r.Height * 0.50f);
        g.DrawLine(axisPen, r.Width * 0.1f, cy, r.Width * 0.9f, cy);

        g.FillEllipse(Brushes.White, cxA - 34, cy - 34, 68, 68);
        g.DrawEllipse(discPenA, cxA - 34, cy - 34, 68, 68);
        g.FillEllipse(Brushes.White, cxB - 34, cy - 34, 68, 68);
        g.DrawEllipse(discPenB, cxB - 34, cy - 34, 68, 68);

        g.DrawString("Disc A: +e", txt, Brushes.DarkGreen, cxA - 58, cy + 42);
        g.DrawString("Disc B: -e", txt, Brushes.SaddleBrown, cxB - 58, cy + 42);
        g.DrawString("Phase B = 180deg", txt, Brushes.Black, r.Width * 0.37f, r.Height * 0.09f);
    }

    private static void DrawOptional(Graphics g, Rectangle r)
    {
        float cxa = r.Width * 0.33f;
        float cxb = r.Width * 0.67f;
        float cy = r.Height * 0.58f;
        float orbit = Math.Min(r.Width, r.Height) * 0.22f;
        using var penA = new Pen(Color.FromArgb(35, 122, 80), 2f);
        using var penB = new Pen(Color.FromArgb(200, 90, 20), 2f);
        using var holePen = new Pen(Color.FromArgb(30, 30, 30), 1.2f);
        using var txt = UiFont(10.5F, FontStyle.Bold);

        g.DrawEllipse(penA, cxa - 33, cy - 33, 66, 66);
        g.DrawEllipse(penB, cxb - 33, cy - 33, 66, 66);

        for (int i = 0; i < 8; i++)
        {
            double a = (2.0 * Math.PI * i) / 8.0;
            float xa = cxa + orbit * (float)Math.Cos(a);
            float ya = cy + orbit * (float)Math.Sin(a);
            float xb = cxb + orbit * (float)Math.Cos(a + Math.PI);
            float yb = cy + orbit * (float)Math.Sin(a + Math.PI);
            g.DrawEllipse(holePen, xa - 8, ya - 8, 16, 16);
            g.DrawEllipse(holePen, xb - 8, yb - 8, 16, 16);
        }

        g.DrawString("Around holes A", txt, Brushes.DarkGreen, cxa - 55, cy + 42);
        g.DrawString("Around holes B (180deg)", txt, Brushes.SaddleBrown, cxb - 80, cy + 42);
        g.DrawString("Output pins are referenced on ring center", txt, Brushes.Black, r.Width * 0.24f, r.Height * 0.10f);
    }

    private static void DrawDetailed(Graphics g, Rectangle r)
    {
        using var txt = UiFont(11F, FontStyle.Bold);
        using var pen = new Pen(Color.FromArgb(80, 80, 80), 2f);
        using var phasePen = new Pen(Color.FromArgb(30, 90, 150), 2f) { EndCap = LineCap.ArrowAnchor };

        float cx = r.Width * 0.5f;
        float cy = r.Height * 0.58f;
        float rad = Math.Min(r.Width, r.Height) * 0.20f;
        g.DrawEllipse(pen, cx - rad, cy - rad, rad * 2, rad * 2);
        g.DrawEllipse(pen, cx - rad * 0.55f, cy - rad * 0.55f, rad * 1.1f, rad * 1.1f);

        g.DrawLine(phasePen, cx + rad + 10, cy, cx - rad - 10, cy);
        g.DrawString("Disc B is phase-shifted by 180deg from Disc A", txt, Brushes.Black, r.Width * 0.20f, r.Height * 0.12f);
        g.DrawString("A: +e | B: -e", txt, Brushes.Black, r.Width * 0.42f, r.Height * 0.79f);
    }
}
