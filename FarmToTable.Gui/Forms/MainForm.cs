using FarmToTable.Gui.Forms;
using FarmToTable.Gui.Services;

namespace FarmToTable.Gui;

public partial class MainForm : Form
{
    private readonly FarmGuiService _farmService;
    private readonly CropGuiService _cropService;
    private readonly RestaurantGuiService _restaurantService;
    private readonly OrderGuiService _orderService;
    private readonly ReportGuiService _reportService;
    
    private Button btnFarms;
    private Button btnCrops;
    private Button btnRestaurants;
    private Button btnOrders;
    private Button btnReports;
    private Panel pnlSidebar;
    private Panel pnlContent;
    private Label lblTitle;

    public MainForm(FarmGuiService farmService, CropGuiService cropService,
                    RestaurantGuiService restaurantService, OrderGuiService orderService,
                    ReportGuiService reportService)
    {
        _farmService = farmService;
        _cropService = cropService;
        _restaurantService = restaurantService;
        _orderService = orderService;
        _reportService = reportService;
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        this.Text = "نظام توزيع المزرعة إلى المائدة";
        this.Size = new Size(1200, 700);
        this.StartPosition = FormStartPosition.CenterScreen;
        this.BackColor = Color.FromArgb(240, 242, 245);

        pnlSidebar = new Panel
        {
            Dock = DockStyle.Left,
            Width = 220,
            BackColor = Color.FromArgb(44, 62, 80)
        };

        pnlContent = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.FromArgb(240, 242, 245),
            Padding = new Padding(10)
        };

        lblTitle = new Label
        {
            Text = "🌾 Farm-to-Table",
            Location = new Point(20, 20),
            Size = new Size(180, 40),
            Font = new Font("Segoe UI", 14, FontStyle.Bold),
            ForeColor = Color.White,
            TextAlign = ContentAlignment.MiddleCenter
        };

        btnFarms = CreateSidebarButton("🌾 المزارع", 80);
        btnCrops = CreateSidebarButton("🌽 المحاصيل", 140);
        btnRestaurants = CreateSidebarButton("🍽️ المطاعم", 200);
        btnOrders = CreateSidebarButton("📦 الطلبات", 260);
        btnReports = CreateSidebarButton("📊 التقارير", 320);

        btnFarms.Click += (s, e) => LoadForm(new FarmsForm(_farmService));
        btnCrops.Click += (s, e) => LoadForm(new CropsForm(_cropService));
        btnRestaurants.Click += (s, e) => LoadForm(new RestaurantsForm(_restaurantService));
        btnOrders.Click += (s, e) => LoadForm(new OrdersForm(_orderService));
        btnReports.Click += (s, e) => LoadForm(new ReportsForm(_reportService));

        pnlSidebar.Controls.Add(lblTitle);
        pnlSidebar.Controls.Add(btnFarms);
        pnlSidebar.Controls.Add(btnCrops);
        pnlSidebar.Controls.Add(btnRestaurants);
        pnlSidebar.Controls.Add(btnOrders);
        pnlSidebar.Controls.Add(btnReports);

        this.Controls.Add(pnlContent);
        this.Controls.Add(pnlSidebar);
    }

    private Button CreateSidebarButton(string text, int y)
    {
        return new Button
        {
            Text = text,
            Location = new Point(10, y),
            Size = new Size(200, 50),
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.FromArgb(52, 73, 94),
            ForeColor = Color.White,
            Font = new Font("Segoe UI", 11, FontStyle.Bold),
            TextAlign = ContentAlignment.MiddleLeft,
            Padding = new Padding(15, 0, 0, 0)
        };
    }

    private void LoadForm(Form form)
    {
        pnlContent.Controls.Clear();
        form.TopLevel = false;
        form.FormBorderStyle = FormBorderStyle.None;
        form.Dock = DockStyle.Fill;
        pnlContent.Controls.Add(form);
        form.Show();
    }
}