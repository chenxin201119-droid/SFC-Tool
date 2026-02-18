namespace MES_PRINT;

public class TabSettingsForm : Form
{
    private readonly CheckBox _chkPrintLog;
    private readonly CheckBox _chkBarcodesNotPacked;
    private readonly CheckBox _chkRouteLaunch;

    public bool PrintLogEnabled => _chkPrintLog.Checked;
    public bool BarcodesNotPackedEnabled => _chkBarcodesNotPacked.Checked;
    public bool RouteLaunchEnabled => _chkRouteLaunch.Checked;

    public TabSettingsForm(bool printLog, bool barcodesNotPacked, bool routeLaunch)
    {
        Text = "Tab 启用设置";
        Size = new Size(320, 200);
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;

        _chkPrintLog = new CheckBox { Text = "Print Log", Location = new Point(24, 24), Checked = printLog, AutoSize = true };
        _chkBarcodesNotPacked = new CheckBox { Text = "Barcodes Not Packed", Location = new Point(24, 52), Checked = barcodesNotPacked, AutoSize = true };
        _chkRouteLaunch = new CheckBox { Text = "Route Launch", Location = new Point(24, 80), Checked = routeLaunch, AutoSize = true };

        var btnSave = new Button { Text = "Save", Location = new Point(120, 120), Size = new Size(80, 28) };
        var btnCancel = new Button { Text = "Cancel", Location = new Point(210, 120), Size = new Size(80, 28), DialogResult = DialogResult.Cancel };
        btnSave.Click += (_, _) =>
        {
            if (!_chkPrintLog.Checked && !_chkBarcodesNotPacked.Checked && !_chkRouteLaunch.Checked)
            {
                MessageBox.Show("至少保留一个 Tab 启用。", "Tab 设置", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            DialogResult = DialogResult.OK;
            Close();
        };
        AcceptButton = btnSave;
        CancelButton = btnCancel;
        Controls.AddRange(new Control[] { _chkPrintLog, _chkBarcodesNotPacked, _chkRouteLaunch, btnSave, btnCancel });
    }
}
