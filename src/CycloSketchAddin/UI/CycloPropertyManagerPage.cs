using System;
using System.Drawing;
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
            Width = 680,
            Height = 700,
            FormBorderStyle = FormBorderStyle.FixedDialog,
            MaximizeBox = false,
            MinimizeBox = false,
            BackColor = Color.FromArgb(244, 247, 252)
        };

        var header = new Panel
        {
            Dock = DockStyle.Top,
            Height = 56,
            BackColor = Color.FromArgb(28, 66, 110)
        };

        var title = new Label
        {
            Text = "CycloSketch Add-In | Sketch Generator",
            ForeColor = Color.White,
            AutoSize = true,
            Left = 16,
            Top = 18,
            Font = new Font("Segoe UI", 11F, FontStyle.Bold)
        };
        header.Controls.Add(title);

        var tabs = new TabControl
        {
            Dock = DockStyle.Fill,
            Font = new Font("Segoe UI", 9F, FontStyle.Regular)
        };

        var tabNecessary = new TabPage("01 Necessary") { BackColor = Color.FromArgb(250, 252, 255) };
        var tabOptional = new TabPage("02 Optional") { BackColor = Color.FromArgb(250, 252, 255) };
        var tabDetailed = new TabPage("03 Detailed") { BackColor = Color.FromArgb(250, 252, 255) };
        tabs.TabPages.Add(tabNecessary);
        tabs.TabPages.Add(tabOptional);
        tabs.TabPages.Add(tabDetailed);

        var pnlNecessary = CreateTabPanel();
        var pnlOptional = CreateTabPanel();
        var pnlDetailed = CreateTabPanel();
        tabNecessary.Controls.Add(pnlNecessary);
        tabOptional.Controls.Add(pnlOptional);
        tabDetailed.Controls.Add(pnlDetailed);

        var gbNecessary = CreateGroupBox("Necessary parameters", Color.FromArgb(40, 88, 140));
        pnlNecessary.Controls.Add(gbNecessary);

        var numReduction = CreateIntField(gbNecessary, "Reduction ratio", defaults.ReductionRatio, 2, 5000);
        var numEccentric = CreateDoubleField(gbNecessary, "Eccentric amount [mm]", defaults.EccentricMm, 0.001M, 500M, 3);
        var numRingPinDia = CreateDoubleField(gbNecessary, "Ring pin diameter [mm]", defaults.RingPinDiaMm, 0.001M, 10000M, 3);
        var numRingPinPitchDia = CreateDoubleField(gbNecessary, "Ring pin pitch diameter [mm]", defaults.RingPinPitchDiaMm, 0.001M, 100000M, 3);
        var numPlotPerTooth = CreateIntField(gbNecessary, "Cycloidal curve plot per tooth", defaults.PlotPerTooth, 5, 2000);

        var gbCenterHole = CreateGroupBox("Cycloidal gear center hole", Color.FromArgb(0, 122, 77));
        pnlOptional.Controls.Add(gbCenterHole);

        var chkCenterHole = CreateCheckField(gbCenterHole, "Draw center hole", defaults.DrawCenterHole);
        var numCenterHoleDia = CreateDoubleField(gbCenterHole, "Diameter [mm]", defaults.CenterHoleDiaMm, 0.001M, 10000M, 3);

        var gbAroundToOutput = CreateGroupBox("Cycloidal gear to output disk", Color.FromArgb(148, 74, 0));
        pnlOptional.Controls.Add(gbAroundToOutput);

        var chkAroundHoles = CreateCheckField(gbAroundToOutput, "Draw around hole", defaults.DrawAroundHoles);
        var chkOutputPins = CreateCheckField(gbAroundToOutput, "Draw output disk pin", defaults.DrawOutputDiskPins);

        var lblSetAbout = new Label
        {
            Text = "Set about",
            AutoSize = true,
            Margin = new Padding(8, 10, 8, 2)
        };
        gbAroundToOutput.Controls.Add(lblSetAbout);

        var radioMasterAround = new RadioButton
        {
            Text = "Cycloidal gear hole",
            Checked = defaults.LinkOutputPinsToAroundHoles,
            AutoSize = true,
            Margin = new Padding(24, 2, 8, 2)
        };
        gbAroundToOutput.Controls.Add(radioMasterAround);

        var radioMasterOutput = new RadioButton
        {
            Text = "Output disk pin",
            Checked = !defaults.LinkOutputPinsToAroundHoles,
            AutoSize = true,
            Margin = new Padding(24, 2, 8, 8)
        };
        gbAroundToOutput.Controls.Add(radioMasterOutput);

        var numAroundNum = CreateIntField(gbAroundToOutput, "Hole num", defaults.AroundHoleNum, 1, 1000);
        var numAroundDia = CreateDoubleField(gbAroundToOutput, "Hole diameter [mm]", defaults.AroundHoleDiaMm, 0.001M, 10000M, 3);
        var numAroundPosDia = CreateDoubleField(gbAroundToOutput, "Hole position diameter [mm]", defaults.AroundHolePositionDiaMm, 0.001M, 100000M, 3);

        var numOutputNum = CreateIntField(gbAroundToOutput, "Pin num", defaults.OutputPinNum, 1, 1000);
        var numOutputDia = CreateDoubleField(gbAroundToOutput, "Pin diameter [mm]", defaults.OutputPinDiaMm, 0.001M, 10000M, 3);
        var numOutputPosDia = CreateDoubleField(gbAroundToOutput, "Pin position diameter [mm]", defaults.OutputPinPositionDiaMm, 0.001M, 100000M, 3);

        var gbDetailed = CreateGroupBox("Detailed setting", Color.FromArgb(86, 52, 135));
        pnlDetailed.Controls.Add(gbDetailed);

        var chkSeparateSketches = CreateCheckField(gbDetailed, "Separate sketch", defaults.SeparateSketches);

        var info = new Label
        {
            AutoSize = false,
            Height = 150,
            Dock = DockStyle.Top,
            Padding = new Padding(10),
            Text =
                "Generated sketches:\r\n" +
                "- 00_SK_REFERENCE_AXES\r\n" +
                "- 01_SK_CYCLO_DISC_PROFILE\r\n" +
                "- 02_SK_CENTER_BORE\r\n" +
                "- 03_SK_OUTPUT_HOLES_DISC\r\n" +
                "- 04_SK_RING_PINS_REFERENCE\r\n" +
                "- 05_SK_OUTPUT_PINS_DISC",
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
            radioMasterAround.Enabled = outputEnabled && aroundEnabled;
            radioMasterOutput.Enabled = outputEnabled;

            bool manualOutput = outputEnabled && radioMasterOutput.Checked;
            numOutputNum.Enabled = manualOutput;
            numOutputDia.Enabled = manualOutput;
            numOutputPosDia.Enabled = manualOutput;
        };

        chkCenterHole.CheckedChanged += (_, _) => updateUi();
        chkAroundHoles.CheckedChanged += (_, _) => updateUi();
        chkOutputPins.CheckedChanged += (_, _) => updateUi();
        radioMasterAround.CheckedChanged += (_, _) => updateUi();
        radioMasterOutput.CheckedChanged += (_, _) => updateUi();

        updateUi();

        var panel = new Panel { Dock = DockStyle.Bottom, Height = 56 };
        var btnGenerate = new Button
        {
            Text = "Generate",
            Width = 110,
            Height = 32,
            Left = 430,
            Top = 12,
            BackColor = Color.FromArgb(35, 122, 80),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat
        };
        btnGenerate.FlatAppearance.BorderSize = 0;

        var btnCancel = new Button
        {
            Text = "Cancel",
            Width = 110,
            Height = 32,
            Left = 548,
            Top = 12,
            BackColor = Color.FromArgb(110, 110, 110),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat
        };
        btnCancel.FlatAppearance.BorderSize = 0;

        btnGenerate.Click += (_, _) =>
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
                LinkOutputPinsToAroundHoles = radioMasterAround.Checked,

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
                return;
            }

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

    private static GroupBox CreateGroupBox(string title, Color accent)
    {
        var gb = new GroupBox
        {
            Text = title,
            Width = 620,
            AutoSize = true,
            Padding = new Padding(10),
            ForeColor = accent,
            Font = new Font("Segoe UI", 9F, FontStyle.Bold),
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
            Width = 585,
            Height = 34,
            Margin = new Padding(4)
        };
        row.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 65));
        row.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 35));

        var lbl = new Label
        {
            Text = label,
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleLeft,
            Font = new Font("Segoe UI", 9F, FontStyle.Regular),
            ForeColor = Color.FromArgb(35, 35, 35)
        };

        var num = new NumericUpDown
        {
            Minimum = min,
            Maximum = max,
            Value = Math.Max(min, Math.Min(max, value)),
            DecimalPlaces = 0,
            Dock = DockStyle.Fill,
            Font = new Font("Segoe UI", 9F, FontStyle.Regular)
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
            Width = 585,
            Height = 34,
            Margin = new Padding(4)
        };
        row.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 65));
        row.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 35));

        var lbl = new Label
        {
            Text = label,
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleLeft,
            Font = new Font("Segoe UI", 9F, FontStyle.Regular),
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
            Font = new Font("Segoe UI", 9F, FontStyle.Regular)
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
            Font = new Font("Segoe UI", 9F, FontStyle.Regular),
            ForeColor = Color.FromArgb(30, 30, 30)
        };
        layout.Controls.Add(chk);
        return chk;
    }
}
