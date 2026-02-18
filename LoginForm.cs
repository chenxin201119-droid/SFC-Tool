namespace MES_PRINT;

public class LoginForm : Form
{
    private readonly ComboBox _cboAccount;
    private readonly TextBox _txtPassword;

    /// <summary>登录成功时返回 "SFC" 或 "EQ"，取消或失败为 null。</summary>
    public string? LoggedInAccount { get; private set; }

    public LoginForm()
    {
        Text = "Login";
        Size = new Size(320, 180);
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;

        var lblAccount = new Label { Text = "Account", Location = new Point(16, 20), AutoSize = true };
        _cboAccount = new ComboBox
        {
            Location = new Point(100, 16),
            Size = new Size(180, 24),
            DropDownStyle = ComboBoxStyle.DropDownList
        };
        _cboAccount.Items.AddRange(new object[] { "SFC", "EQ" });
        _cboAccount.SelectedIndex = 0;

        var lblPassword = new Label { Text = "Password", Location = new Point(16, 56), AutoSize = true };
        _txtPassword = new TextBox
        {
            Location = new Point(100, 52),
            Size = new Size(180, 24),
            UseSystemPasswordChar = true
        };

        var btnOk = new Button { Text = "OK", Location = new Point(120, 96), Size = new Size(75, 28) };
        var btnCancel = new Button { Text = "Cancel", Location = new Point(205, 96), Size = new Size(75, 28), DialogResult = DialogResult.Cancel };
        AcceptButton = btnOk;
        CancelButton = btnCancel;

        btnOk.Click += (_, _) =>
        {
            var account = (_cboAccount.SelectedItem?.ToString() ?? "").Trim();
            var password = _txtPassword.Text ?? "";
            if (string.IsNullOrEmpty(account))
            {
                MessageBox.Show("Please select account.", "Login", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (account == "SFC" && password != "12345")
            {
                MessageBox.Show("Invalid password.", "Login", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (account == "EQ" && password != "12345")
            {
                MessageBox.Show("Invalid password.", "Login", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            LoggedInAccount = account;
            DialogResult = DialogResult.OK;
            Close();
        };

        Controls.AddRange(new Control[] { lblAccount, _cboAccount, lblPassword, _txtPassword, btnOk, btnCancel });
    }
}
