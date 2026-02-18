using Microsoft.Data.SqlClient;
using System.Data;
using System.Diagnostics;

namespace MES_PRINT;

public class MainForm : Form
{
    private const string ConnectionString =
        "Server=10.80.1.23;Database=SFCS_TH_Assembly;User Id=sfcreport;Password=rpt.abc654;TrustServerCertificate=True;";

    private TextBox txtBarcode = null!;
    private Button btnSearchBarcode = null!;
    private Label lblStatusBarcode = null!;

    private TextBox txtWoNo = null!;
    private Button btnSearchWo = null!;
    private Label lblStatusWo = null!;

    private TextBox txtRouteFolder = null!;
    private TextBox txtWorkOrder = null!;
    private Button btnBrowseFolder = null!;
    private Button btnLaunch = null!;
    private Label lblStatusRoute = null!;

    private DataGridView dgvResult = null!;
    private TextBox txtLaunchLog = null!;
    private Process? _launchedProcess;

    private TabControl _tabControl = null!;
    private TableLayoutPanel _panelFolderRow = null!;
    private TabPage _tabPrintLog = null!;
    private TabPage _tabWoUnpacked = null!;
    private TabPage _tabRouteLaunch = null!;
    private bool _sfcLoggedIn;
    private bool _eqLoggedIn;
    private Button _btnLogin = null!;
    private Button _btnLogout = null!;
    private Button _btnTabSettings = null!;

    private const string SfcUser = "SFC";
    private const string SfcPassword = "12345";
    private const string EqUser = "EQ";
    private const string EqPassword = "12345";

    public MainForm()
    {
        InitializeComponent();
        AppConfig.Load();
        txtRouteFolder.Text = AppConfig.RouteLaunchFolder ?? "";
        ApplyTabVisibility();
        UpdateFolderRowVisibility();
    }

    private void InitializeComponent()
    {
        Text = "MES 查询工具";
        Size = new Size(1080, 660);
        StartPosition = FormStartPosition.CenterScreen;
        MinimumSize = new Size(920, 500);
        BackColor = Color.FromArgb(245, 245, 247);
        Font = new Font("Microsoft YaHei UI", 9f);

        // ========== 登录与权限栏 ==========
        var panelLogin = new Panel
        {
            Dock = DockStyle.Top,
            Height = 44,
            Padding = new Padding(12, 8, 12, 4),
            BackColor = Color.FromArgb(248, 248, 250)
        };
        _btnLogin = new Button { Text = "Login", Location = new Point(12, 8), Size = new Size(70, 26), FlatStyle = FlatStyle.Flat };
        _btnLogout = new Button { Text = "Logout", Location = new Point(12, 8), Size = new Size(70, 26), FlatStyle = FlatStyle.Flat, Visible = false };
        _btnTabSettings = new Button { Text = "Tab 设置", Location = new Point(96, 8), Size = new Size(80, 26), FlatStyle = FlatStyle.Flat, Visible = false };
        _btnLogin.Click += (_, _) => TryLogin();
        _btnLogout.Click += (_, _) => Logout();
        _btnTabSettings.Click += (_, _) => OpenTabSettings();
        panelLogin.Controls.AddRange(new Control[] { _btnLogin, _btnLogout, _btnTabSettings });

        _tabControl = new TabControl
        {
            Dock = DockStyle.Top,
            Height = 116,
            Font = new Font("Microsoft YaHei UI", 9.5f),
            Padding = new Point(14, 6)
        };

        // ========== Tab1: Print Log ==========
        _tabPrintLog = new TabPage("Print Log");
        _tabPrintLog.BackColor = Color.White;
        _tabPrintLog.Padding = new Padding(8);

        var panelBarcode = new Panel { Height = 52, Dock = DockStyle.Top, Padding = new Padding(16, 6, 16, 4) };
        var lblBarcode = new Label
        {
            Text = "Barcode",
            Location = new Point(16, 14),
            AutoSize = true,
            ForeColor = Color.FromArgb(60, 60, 67)
        };
        txtBarcode = new TextBox
        {
            Location = new Point(140, 10),
            Size = new Size(280, 24),
            Font = new Font("Consolas", 10f),
            BorderStyle = BorderStyle.FixedSingle
        };
        txtBarcode.KeyDown += (s, e) =>
        {
            if (e.KeyCode == Keys.Enter) { e.SuppressKeyPress = true; SearchByBarcode(); }
        };
        btnSearchBarcode = new Button
        {
            Text = "Select",
            Location = new Point(432, 9),
            Size = new Size(88, 26),
            BackColor = Color.FromArgb(0, 122, 204),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand
        };
        btnSearchBarcode.FlatAppearance.BorderSize = 0;
        btnSearchBarcode.Click += (_, _) => SearchByBarcode();
        lblStatusBarcode = new Label
        {
            AutoSize = true,
            Location = new Point(532, 14),
            ForeColor = Color.FromArgb(110, 110, 118),
            Text = "Enter Barcode and click Select or press Enter",
            MaximumSize = new Size(480, 0)
        };
        panelBarcode.Controls.AddRange(new Control[] { lblBarcode, txtBarcode, btnSearchBarcode, lblStatusBarcode });
        _tabPrintLog.Controls.Add(panelBarcode);

        // ========== Tab2: Barcodes Not Packed ==========
        _tabWoUnpacked = new TabPage("Barcodes Not Packed");
        _tabWoUnpacked.BackColor = Color.White;
        _tabWoUnpacked.Padding = new Padding(8);

        var panelWo = new Panel { Height = 52, Dock = DockStyle.Top, Padding = new Padding(16, 6, 16, 4) };
        var lblWoNo = new Label
        {
            Text = "WO_NO",
            Location = new Point(16, 14),
            AutoSize = true,
            ForeColor = Color.FromArgb(60, 60, 67)
        };
        txtWoNo = new TextBox
        {
            Location = new Point(140, 10),
            Size = new Size(280, 24),
            Font = new Font("Consolas", 10f),
            BorderStyle = BorderStyle.FixedSingle
        };
        txtWoNo.KeyDown += (s, e) =>
        {
            if (e.KeyCode == Keys.Enter) { e.SuppressKeyPress = true; SearchByWoNo(); }
        };
        btnSearchWo = new Button
        {
            Text = "Select",
            Location = new Point(432, 9),
            Size = new Size(88, 26),
            BackColor = Color.FromArgb(0, 122, 204),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand
        };
        btnSearchWo.FlatAppearance.BorderSize = 0;
        btnSearchWo.Click += (_, _) => SearchByWoNo();
        lblStatusWo = new Label
        {
            AutoSize = true,
            Location = new Point(532, 14),
            ForeColor = Color.FromArgb(110, 110, 118),
            Text = "Enter WO_NO to list barcodes not yet packed",
            MaximumSize = new Size(480, 0)
        };
        panelWo.Controls.AddRange(new Control[] { lblWoNo, txtWoNo, btnSearchWo, lblStatusWo });
        _tabWoUnpacked.Controls.Add(panelWo);

        // ========== Tab3: Route Launch ==========
        _tabRouteLaunch = new TabPage("Route Launch");
        _tabRouteLaunch.BackColor = Color.White;
        _tabRouteLaunch.Padding = new Padding(8);

        var panelRoute = new Panel { Height = 92, Dock = DockStyle.Top, Padding = new Padding(16, 4, 16, 4) };
        _panelFolderRow = new TableLayoutPanel
        {
            Height = 40,
            Dock = DockStyle.Top,
            ColumnCount = 3,
            RowCount = 1,
            Padding = new Padding(0)
        };
        _panelFolderRow.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 56F));
        _panelFolderRow.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        _panelFolderRow.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 88F));
        _panelFolderRow.RowStyles.Add(new RowStyle(SizeType.Absolute, 40F));
        var lblFolder = new Label { Text = "Folder", AutoSize = true, ForeColor = Color.FromArgb(60, 60, 67), Anchor = AnchorStyles.Left };
        txtRouteFolder = new TextBox
        {
            Dock = DockStyle.Fill,
            Font = new Font("Consolas", 9f),
            BorderStyle = BorderStyle.FixedSingle,
            ReadOnly = true,
            Margin = new Padding(4, 4, 8, 4)
        };
        btnBrowseFolder = new Button
        {
            Text = "Browse",
            Size = new Size(80, 26),
            Anchor = AnchorStyles.Left,
            BackColor = Color.FromArgb(0, 122, 204),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand
        };
        btnBrowseFolder.FlatAppearance.BorderSize = 0;
        btnBrowseFolder.Click += (_, _) =>
        {
            using var dlg = new FolderBrowserDialog { Description = "Select folder containing MODEL_NO .lnk or .exe files" };
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                txtRouteFolder.Text = dlg.SelectedPath;
                AppConfig.RouteLaunchFolder = dlg.SelectedPath;
                AppConfig.Save();
                AppendLaunchLog($"Folder selected: {dlg.SelectedPath}");
            }
        };
        _panelFolderRow.Controls.Add(lblFolder, 0, 0);
        _panelFolderRow.Controls.Add(txtRouteFolder, 1, 0);
        _panelFolderRow.Controls.Add(btnBrowseFolder, 2, 0);

        var panelWorkOrderRow = new TableLayoutPanel
        {
            Height = 40,
            Dock = DockStyle.Top,
            ColumnCount = 4,
            RowCount = 1
        };
        panelWorkOrderRow.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 88F));
        panelWorkOrderRow.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        panelWorkOrderRow.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 88F));
        panelWorkOrderRow.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        panelWorkOrderRow.RowStyles.Add(new RowStyle(SizeType.Absolute, 40F));
        var lblWorkOrder = new Label { Text = "Work_Order", AutoSize = true, ForeColor = Color.FromArgb(60, 60, 67), Anchor = AnchorStyles.Left };
        txtWorkOrder = new TextBox
        {
            Dock = DockStyle.Fill,
            Font = new Font("Consolas", 10f),
            BorderStyle = BorderStyle.FixedSingle,
            Margin = new Padding(4, 4, 8, 4)
        };
        txtWorkOrder.KeyDown += (s, e) =>
        {
            if (e.KeyCode == Keys.Enter) { e.SuppressKeyPress = true; LaunchByWorkOrder(); }
        };
        btnLaunch = new Button
        {
            Text = "Launch",
            Size = new Size(88, 26),
            Anchor = AnchorStyles.Left,
            BackColor = Color.FromArgb(0, 122, 204),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand
        };
        btnLaunch.FlatAppearance.BorderSize = 0;
        btnLaunch.Click += (_, _) => LaunchByWorkOrder();
        lblStatusRoute = new Label
        {
            AutoSize = true,
            Anchor = AnchorStyles.Left,
            ForeColor = Color.FromArgb(110, 110, 118),
            Text = "Select folder, enter Work_Order, then Launch",
            MaximumSize = new Size(400, 0)
        };
        panelWorkOrderRow.Controls.Add(lblWorkOrder, 0, 0);
        panelWorkOrderRow.Controls.Add(txtWorkOrder, 1, 0);
        panelWorkOrderRow.Controls.Add(btnLaunch, 2, 0);
        panelWorkOrderRow.Controls.Add(lblStatusRoute, 3, 0);
        panelRoute.Controls.Add(_panelFolderRow);
        panelRoute.Controls.Add(panelWorkOrderRow);
        _tabRouteLaunch.Controls.Add(panelRoute);

        _tabControl.SelectedIndexChanged += (s, _) =>
        {
            var sel = _tabControl.SelectedTab;
            var isRouteTab = sel == _tabRouteLaunch;
            _tabControl.Height = isRouteTab ? 148 : 116;
            dgvResult.Visible = !isRouteTab;
            txtLaunchLog.Visible = isRouteTab;
            if (!isRouteTab)
                dgvResult.DataSource = null;
            UpdateFolderRowVisibility();
            if (sel == _tabPrintLog)
            {
                lblStatusBarcode.Text = "Enter Barcode and click Select or press Enter";
                lblStatusBarcode.ForeColor = Color.FromArgb(110, 110, 118);
            }
            else if (sel == _tabWoUnpacked)
            {
                lblStatusWo.Text = "Enter WO_NO to list barcodes not yet packed";
                lblStatusWo.ForeColor = Color.FromArgb(110, 110, 118);
            }
            else if (sel == _tabRouteLaunch)
            {
                lblStatusRoute.Text = "Select folder, enter Work_Order, then Launch";
                lblStatusRoute.ForeColor = Color.FromArgb(110, 110, 118);
            }
        };

        // ========== 结果表格区域 ==========
        var panelGrid = new Panel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(10, 4, 10, 8),
            BackColor = Color.FromArgb(245, 245, 247)
        };

        dgvResult = new DataGridView
        {
            Dock = DockStyle.Fill,
            AllowUserToAddRows = false,
            AllowUserToDeleteRows = false,
            ReadOnly = true,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
            BackgroundColor = Color.White,
            BorderStyle = BorderStyle.None,
            RowHeadersVisible = true,
            SelectionMode = DataGridViewSelectionMode.CellSelect,
            MultiSelect = true,
            ClipboardCopyMode = DataGridViewClipboardCopyMode.EnableWithAutoHeaderText,
            Font = new Font("Microsoft YaHei UI", 9f),
            RowTemplate = { Height = 28 },
            ColumnHeadersHeight = 32,
            GridColor = Color.FromArgb(230, 230, 230)
        };
        dgvResult.BorderStyle = BorderStyle.FixedSingle;
        dgvResult.EnableHeadersVisualStyles = false;
        dgvResult.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(248, 248, 250);
        dgvResult.ColumnHeadersDefaultCellStyle.ForeColor = Color.FromArgb(60, 60, 67);
        dgvResult.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.Single;
        dgvResult.DefaultCellStyle.SelectionBackColor = Color.FromArgb(220, 235, 252);
        dgvResult.DefaultCellStyle.SelectionForeColor = Color.Black;
        dgvResult.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(252, 252, 253);

        txtLaunchLog = new TextBox
        {
            Dock = DockStyle.Fill,
            Multiline = true,
            ReadOnly = true,
            ScrollBars = ScrollBars.Vertical,
            Font = new Font("Consolas", 9f),
            BackColor = Color.FromArgb(30, 30, 30),
            ForeColor = Color.FromArgb(220, 220, 220),
            BorderStyle = BorderStyle.FixedSingle
        };
        txtLaunchLog.Visible = false;

        panelGrid.Controls.Add(txtLaunchLog);
        panelGrid.Controls.Add(dgvResult);
        Controls.Add(panelGrid);
        Controls.Add(_tabControl);
        Controls.Add(panelLogin);
    }

    private void SearchByBarcode()
    {
        var barcode = txtBarcode.Text?.Trim() ?? "";
        if (string.IsNullOrEmpty(barcode))
        {
            lblStatusBarcode.Text = "Please enter Barcode";
            lblStatusBarcode.ForeColor = Color.FromArgb(255, 59, 48);
            return;
        }

        lblStatusBarcode.Text = "Loading...";
        lblStatusBarcode.ForeColor = Color.FromArgb(110, 110, 118);
        btnSearchBarcode.Enabled = false;
        dgvResult.DataSource = null;
        Application.DoEvents();

        try
        {
            using var conn = new SqlConnection(ConnectionString);
            conn.Open();
            const string sql = "SELECT * FROM MES_PRINT_LOG d WHERE d.BARCODE = @BARCODE";
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@BARCODE", barcode);
            var adapter = new SqlDataAdapter(cmd);
            var dt = new DataTable();
            adapter.Fill(dt);
            dgvResult.DataSource = dt;
            lblStatusBarcode.Text = $"{dt.Rows.Count} record(s)";
            lblStatusBarcode.ForeColor = dt.Rows.Count > 0 ? Color.FromArgb(52, 199, 89) : Color.FromArgb(110, 110, 118);
        }
        catch (Exception ex)
        {
            lblStatusBarcode.Text = "Error: " + ex.Message;
            lblStatusBarcode.ForeColor = Color.FromArgb(255, 59, 48);
        }
        finally
        {
            btnSearchBarcode.Enabled = true;
        }
    }

    private void SearchByWoNo()
    {
        var woNo = txtWoNo.Text?.Trim() ?? "";
        if (string.IsNullOrEmpty(woNo))
        {
            lblStatusWo.Text = "Please enter WO_NO";
            lblStatusWo.ForeColor = Color.FromArgb(255, 59, 48);
            return;
        }

        lblStatusWo.Text = "Loading...";
        lblStatusWo.ForeColor = Color.FromArgb(110, 110, 118);
        btnSearchWo.Enabled = false;
        dgvResult.DataSource = null;
        Application.DoEvents();

        try
        {
            using var conn = new SqlConnection(ConnectionString);
            conn.Open();
            const string sql = @"
SELECT s.WO_NO, s.BARCODE, s.IS_OUTPUT
FROM MES_WO_BARCODE s
WHERE s.WO_NO = @WO_NO
  AND NOT EXISTS (
    SELECT 1
    FROM MES_Packing_Detail d
    WHERE d.Packing_WO = @WO_NO
      AND d.BARCODE = s.BARCODE
  )";
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@WO_NO", woNo);
            var adapter = new SqlDataAdapter(cmd);
            var dt = new DataTable();
            adapter.Fill(dt);
            dgvResult.DataSource = dt;
            lblStatusWo.Text = $"{dt.Rows.Count} barcode(s) not packed";
            lblStatusWo.ForeColor = dt.Rows.Count > 0 ? Color.FromArgb(52, 199, 89) : Color.FromArgb(110, 110, 118);
        }
        catch (Exception ex)
        {
            lblStatusWo.Text = "Error: " + ex.Message;
            lblStatusWo.ForeColor = Color.FromArgb(255, 59, 48);
        }
        finally
        {
            btnSearchWo.Enabled = true;
        }
    }

    private void AppendLaunchLog(string message)
    {
        var line = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {message}";
        if (txtLaunchLog.Text.Length > 0)
            txtLaunchLog.AppendText(Environment.NewLine);
        txtLaunchLog.AppendText(line);
        txtLaunchLog.SelectionStart = txtLaunchLog.Text.Length;
        txtLaunchLog.ScrollToCaret();
    }

    private void LaunchByWorkOrder()
    {
        var folder = txtRouteFolder.Text?.Trim() ?? "";
        var workOrder = txtWorkOrder.Text?.Trim() ?? "";
        if (string.IsNullOrEmpty(folder))
        {
            lblStatusRoute.Text = "Please select a folder";
            lblStatusRoute.ForeColor = Color.FromArgb(255, 59, 48);
            return;
        }
        if (string.IsNullOrEmpty(workOrder))
        {
            lblStatusRoute.Text = "Please enter Work_Order";
            lblStatusRoute.ForeColor = Color.FromArgb(255, 59, 48);
            return;
        }
        if (!Directory.Exists(folder))
        {
            lblStatusRoute.Text = "Folder does not exist";
            lblStatusRoute.ForeColor = Color.FromArgb(255, 59, 48);
            return;
        }

        lblStatusRoute.Text = "Loading...";
        lblStatusRoute.ForeColor = Color.FromArgb(110, 110, 118);
        btnLaunch.Enabled = false;
        Application.DoEvents();

        AppendLaunchLog($"-------- Launch started for Work_Order: {workOrder} --------");

        try
        {
            // 1. Close previously launched process if still running
            if (_launchedProcess != null && !_launchedProcess.HasExited)
            {
                try
                {
                    _launchedProcess.Kill(entireProcessTree: true);
                    AppendLaunchLog("Previous process closed.");
                }
                catch (Exception ex)
                {
                    AppendLaunchLog($"Warning: failed to close previous process - {ex.Message}");
                }
                _launchedProcess.Dispose();
                _launchedProcess = null;
            }

            // 2. Query MODEL_NO from MES_WO (first row)
            AppendLaunchLog("Querying MODEL_NO from MES_WO...");
            string? modelNo = null;
            using (var conn = new SqlConnection(ConnectionString))
            {
                conn.Open();
                const string sql = "SELECT TOP 1 e.MODEL_NO FROM MES_WO e WHERE e.Work_Order = @Work_Order";
                using var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@Work_Order", workOrder);
                var obj = cmd.ExecuteScalar();
                modelNo = obj?.ToString()?.Trim();
            }

            if (string.IsNullOrEmpty(modelNo))
            {
                AppendLaunchLog("No MODEL_NO found for this Work_Order.");
                lblStatusRoute.Text = "No MODEL_NO found for this Work_Order";
                lblStatusRoute.ForeColor = Color.FromArgb(255, 59, 48);
                return;
            }
            AppendLaunchLog($"MODEL_NO: {modelNo}");

            // 3. Find .lnk or .exe in folder by MODEL_NO (name without extension)
            var targetPath = FindModelFile(folder, modelNo);
            if (targetPath == null)
            {
                AppendLaunchLog($"No .lnk or .exe found for MODEL_NO: {modelNo}");
                lblStatusRoute.Text = $"No .lnk or .exe found for MODEL_NO: {modelNo}";
                lblStatusRoute.ForeColor = Color.FromArgb(255, 59, 48);
                return;
            }
            AppendLaunchLog($"Found: {Path.GetFileName(targetPath)}");

            // ========== 功能节点一 ==========
            // 功能说明：通过解析 .lnk 快捷方式得到目标 exe 路径（及参数、工作目录），再直接启动该 exe 并保存其 Process，
            //           这样 _launchedProcess 指向真实运行的程序进程，下次 Launch 时即可正确关闭上一次启动的程序。
            // 当前状态：未实现。此处为直接 Process.Start(targetPath) 启动 .lnk，返回的是 Shell 进程而非目标 exe 进程，
            //           Shell 很快退出导致无法在下次 Launch 时关闭上一程序。
            // 后期实现：在此处增加 .lnk 解析（如 WScript.Shell CreateShortcut → TargetPath/Arguments/WorkingDirectory），
            //           用解析结果启动目标 exe（UseShellExecute=false），将返回的 Process 赋给 _launchedProcess。
            // 4. Start process
            var startInfo = new ProcessStartInfo
            {
                FileName = targetPath,
                WorkingDirectory = Path.GetDirectoryName(targetPath) ?? folder,
                UseShellExecute = true
            };
            _launchedProcess = Process.Start(startInfo);
            AppendLaunchLog($"Launched: {Path.GetFileName(targetPath)}");
            AppendLaunchLog("-------- Launch completed --------");
            lblStatusRoute.Text = $"Launched: {Path.GetFileName(targetPath)} (MODEL_NO: {modelNo})";
            lblStatusRoute.ForeColor = Color.FromArgb(52, 199, 89);
        }
        catch (Exception ex)
        {
            AppendLaunchLog($"Error: {ex.Message}");
            AppendLaunchLog("-------- Launch failed --------");
            lblStatusRoute.Text = "Error: " + ex.Message;
            lblStatusRoute.ForeColor = Color.FromArgb(255, 59, 48);
        }
        finally
        {
            btnLaunch.Enabled = true;
        }
    }

    private static string? FindModelFile(string folder, string modelNo)
    {
        var dir = new DirectoryInfo(folder);
        var nameMatch = modelNo.Trim();
        foreach (var ext in new[] { ".lnk", ".exe" })
        {
            var path = Path.Combine(folder, nameMatch + ext);
            if (File.Exists(path))
                return path;
        }
        foreach (var f in dir.EnumerateFiles("*.lnk").Concat(dir.EnumerateFiles("*.exe")))
        {
            if (string.Equals(Path.GetFileNameWithoutExtension(f.Name), nameMatch, StringComparison.OrdinalIgnoreCase))
                return f.FullName;
        }
        return null;
    }

    private void UpdateLoginState()
    {
        var anyLoggedIn = _sfcLoggedIn || _eqLoggedIn;
        _btnLogin.Visible = !anyLoggedIn;
        _btnLogout.Visible = anyLoggedIn;
        _btnTabSettings.Visible = _sfcLoggedIn;
        UpdateFolderRowVisibility();
    }

    private void UpdateFolderRowVisibility()
    {
        var showFolder = (_sfcLoggedIn || _eqLoggedIn) && _tabControl.SelectedTab == _tabRouteLaunch;
        _panelFolderRow.Visible = showFolder;
    }

    private void ApplyTabVisibility()
    {
        _tabControl.TabPages.Clear();
        if (AppConfig.PrintLogEnabled)
            _tabControl.TabPages.Add(_tabPrintLog);
        if (AppConfig.BarcodesNotPackedEnabled)
            _tabControl.TabPages.Add(_tabWoUnpacked);
        if (AppConfig.RouteLaunchEnabled)
            _tabControl.TabPages.Add(_tabRouteLaunch);
        if (_tabControl.TabPages.Count > 0 && _tabControl.SelectedIndex < 0)
            _tabControl.SelectedIndex = 0;
        var sel = _tabControl.SelectedTab;
        if (sel != null)
        {
            var isRouteTab = sel == _tabRouteLaunch;
            _tabControl.Height = isRouteTab ? 148 : 116;
            dgvResult.Visible = !isRouteTab;
            txtLaunchLog.Visible = isRouteTab;
        }
        UpdateFolderRowVisibility();
    }

    private void OpenTabSettings()
    {
        using var frm = new TabSettingsForm(
            AppConfig.PrintLogEnabled,
            AppConfig.BarcodesNotPackedEnabled,
            AppConfig.RouteLaunchEnabled);
        if (frm.ShowDialog(this) != DialogResult.OK)
            return;
        AppConfig.PrintLogEnabled = frm.PrintLogEnabled;
        AppConfig.BarcodesNotPackedEnabled = frm.BarcodesNotPackedEnabled;
        AppConfig.RouteLaunchEnabled = frm.RouteLaunchEnabled;
        AppConfig.Save();
        ApplyTabVisibility();
    }

    private void TryLogin()
    {
        using var frm = new LoginForm();
        if (frm.ShowDialog(this) != DialogResult.OK || string.IsNullOrEmpty(frm.LoggedInAccount))
            return;
        var account = frm.LoggedInAccount;
        _sfcLoggedIn = account == SfcUser;
        _eqLoggedIn = account == EqUser;
        UpdateLoginState();
    }

    private void Logout()
    {
        _sfcLoggedIn = false;
        _eqLoggedIn = false;
        UpdateLoginState();
    }
}
