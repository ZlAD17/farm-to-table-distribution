using FarmToTable.Gui.Models;
using FarmToTable.Gui.Services;

namespace FarmToTable.Gui.Forms;

public partial class ReportsForm : Form
{
    private readonly ReportGuiService _reportService;
    private TabControl tabControl;
    private DataGridView dgvTopCrop, dgvRevenue;

    public ReportsForm(ReportGuiService reportService)
    {
        _reportService = reportService;
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        this.Text = "التقارير";
        this.Size = new Size(900, 550);
        this.BackColor = Color.White;

        tabControl = new TabControl
        {
            Dock = DockStyle.Fill
        };

        // تبويب 1: أكثر المحاصيل طلباً
        var tabTopCrop = new TabPage("🏆 أكثر المحاصيل طلباً");
        
        var lblFrom = new Label { Text = "من تاريخ:", Location = new Point(20, 20), Size = new Size(80, 30) };
        var dtpFrom = new DateTimePicker { Location = new Point(100, 20), Size = new Size(150, 30), Value = DateTime.Now.AddMonths(-1) };
        
        var lblTo = new Label { Text = "إلى تاريخ:", Location = new Point(270, 20), Size = new Size(80, 30) };
        var dtpTo = new DateTimePicker { Location = new Point(350, 20), Size = new Size(150, 30), Value = DateTime.Now };
        
        var btnSearch = new Button
        {
            Text = "🔍 بحث",
            Location = new Point(520, 18),
            Size = new Size(100, 35),
            BackColor = Color.FromArgb(52, 152, 219),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat
        };
        
        dgvTopCrop = new DataGridView
        {
            Location = new Point(20, 70),
            Size = new Size(830, 400),
            AllowUserToAddRows = false,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
        };

        btnSearch.Click += async (s, e) =>
        {
            var from = DateOnly.FromDateTime(dtpFrom.Value);
            var to = DateOnly.FromDateTime(dtpTo.Value);
            var result = await _reportService.GetTopCropAsync(from, to);
            dgvTopCrop.DataSource = null;
            dgvTopCrop.DataSource = result;
            dgvTopCrop.Columns["CropId"].Visible = false;
            dgvTopCrop.Columns["CropName"].HeaderText = "اسم المحصول";
            dgvTopCrop.Columns["OrderCount"].HeaderText = "عدد الطلبات";
        };

        tabTopCrop.Controls.AddRange(new Control[] { lblFrom, dtpFrom, lblTo, dtpTo, btnSearch, dgvTopCrop });

        // تبويب 2: إيرادات المزارع
        var tabRevenue = new TabPage("💰 إيرادات المزارع");
        
        var btnLoadRevenue = new Button
        {
            Text = "💰 عرض الإيرادات",
            Location = new Point(20, 20),
            Size = new Size(150, 40),
            BackColor = Color.FromArgb(46, 204, 113),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat
        };
        
        dgvRevenue = new DataGridView
        {
            Location = new Point(20, 80),
            Size = new Size(830, 390),
            AllowUserToAddRows = false,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
        };

        btnLoadRevenue.Click += async (s, e) =>
        {
            var result = await _reportService.GetFarmRevenueAsync();
            dgvRevenue.DataSource = null;
            dgvRevenue.DataSource = result;
            dgvRevenue.Columns["FarmId"].Visible = false;
            dgvRevenue.Columns["FarmName"].HeaderText = "اسم المزرعة";
            dgvRevenue.Columns["TotalRevenue"].HeaderText = "إجمالي الإيرادات";
            dgvRevenue.Columns["TotalRevenue"].DefaultCellStyle.Format = "C";
        };

        tabRevenue.Controls.AddRange(new Control[] { btnLoadRevenue, dgvRevenue });

        tabControl.TabPages.Add(tabTopCrop);
        tabControl.TabPages.Add(tabRevenue);

        this.Controls.Add(tabControl);
    }
}