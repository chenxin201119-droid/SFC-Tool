namespace MES_PRINT;

public class InputBoxForm : Form
{
    private readonly TextBox _txt;

    public string Input => _txt.Text.Trim();

    public InputBoxForm(string title, string prompt, string defaultValue = "")
    {
        Text = title;
        Size = new Size(360, 140);
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;

        var lbl = new Label { Text = prompt, Location = new Point(12, 12), AutoSize = true };
        _txt = new TextBox
        {
            Location = new Point(12, 36),
            Size = new Size(320, 24),
            Text = defaultValue
        };
        var btnOk = new Button { Text = "OK", Location = new Point(168, 72), Size = new Size(80, 28), DialogResult = DialogResult.OK };
        var btnCancel = new Button { Text = "Cancel", Location = new Point(254, 72), Size = new Size(80, 28), DialogResult = DialogResult.Cancel };
        AcceptButton = btnOk;
        CancelButton = btnCancel;
        Controls.AddRange(new Control[] { lbl, _txt, btnOk, btnCancel });
    }
}
