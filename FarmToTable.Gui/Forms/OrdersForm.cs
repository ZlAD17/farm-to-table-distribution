using FarmToTable.Gui.Models;
using FarmToTable.Gui.Services;

namespace FarmToTable.Gui.Forms;

public partial class OrdersForm : Form
{
    private readonly OrderGuiService _orderService;
    private DataGridView dgvOrders;
    private ComboBox cmbStatus;
    private Button btnRefresh, btnUpdateStatus;
    private Label lblStatus;
    private int? _selectedOrderId;

    public OrdersForm(OrderGuiService orderService)
    {
        _orderService = orderService;
        InitializeComponent();
        Load += async (s, e) => await LoadOrders();
    }

    private void InitializeComponent()
    {
        this.Text = "إدارة الطلبات";
        this.Size = new Size(1100, 600);
        this.BackColor = Color.White;

        dgvOrders = new DataGridView
        {
            Location = new Point(20, 60),
            Size = new Size(1050, 480),
            AllowUserToAddRows = false,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            MultiSelect = false,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
            BackgroundColor = Color.White,
            ReadOnly = true
        };
        dgvOrders.CellClick += DgvOrders_CellClick;

        lblStatus = new Label
        {
            Text = "تغيير الحالة إلى:",
            Location = new Point(20, 20),
            Size = new Size(120, 30),
            Font = new Font("Segoe UI", 10)
        };

        cmbStatus = new ComboBox
        {
            Location = new Point(150, 20),
            Size = new Size(150, 30),
            DropDownStyle = ComboBoxStyle.DropDownList
        };
        cmbStatus.Items.AddRange(new[] { "Pending", "Confirmed", "Delivered", "Cancelled" });
        cmbStatus.SelectedIndex = 0;

        btnUpdateStatus = new Button
        {
            Text = "✅ تحديث الحالة",
            Location = new Point(320, 18),
            Size = new Size(130, 35),
            BackColor = Color.FromArgb(52, 152, 219),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Font = new Font("Segoe UI", 9, FontStyle.Bold)
        };
        btnUpdateStatus.Click += async (s, e) => await BtnUpdateStatus_Click();

        btnRefresh = new Button
        {
            Text = "🔄 تحديث",
            Location = new Point(460, 18),
            Size = new Size(100, 35),
            BackColor = Color.FromArgb(46, 204, 113),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat
        };
        btnRefresh.Click += async (s, e) => await LoadOrders();

        this.Controls.AddRange(new Control[] { dgvOrders, lblStatus, cmbStatus, btnUpdateStatus, btnRefresh });
    }

    private async Task LoadOrders()
    {
        try
        {
            var orders = await _orderService.GetAllAsync();
            dgvOrders.DataSource = null;
            dgvOrders.DataSource = orders;

            // تنسيق الأعمدة
            if (dgvOrders.Columns.Contains("OrderId"))
                dgvOrders.Columns["OrderId"].HeaderText = "رقم الطلب";
            if (dgvOrders.Columns.Contains("RestaurantName"))
                dgvOrders.Columns["RestaurantName"].HeaderText = "اسم المطعم";
            if (dgvOrders.Columns.Contains("DriverName"))
                dgvOrders.Columns["DriverName"].HeaderText = "السائق";
            if (dgvOrders.Columns.Contains("Status"))
                dgvOrders.Columns["Status"].HeaderText = "الحالة";
            if (dgvOrders.Columns.Contains("OrderedAt"))
                dgvOrders.Columns["OrderedAt"].HeaderText = "تاريخ الطلب";
            if (dgvOrders.Columns.Contains("TotalAmount"))
                dgvOrders.Columns["TotalAmount"].HeaderText = "المبلغ الإجمالي";
            if (dgvOrders.Columns.Contains("Batches"))
                dgvOrders.Columns["Batches"].Visible = false;

            // تلوين حالة الطلب
            dgvOrders.CellFormatting += (s, e) =>
            {
                if (dgvOrders.Columns[e.ColumnIndex].Name == "Status" && e.Value != null)
                {
                    var status = e.Value.ToString();
                    switch (status)
                    {
                        case "Delivered":
                            e.CellStyle.BackColor = Color.FromArgb(46, 204, 113);
                            e.CellStyle.ForeColor = Color.White;
                            break;
                        case "Confirmed":
                            e.CellStyle.BackColor = Color.FromArgb(52, 152, 219);
                            e.CellStyle.ForeColor = Color.White;
                            break;
                        case "Pending":
                            e.CellStyle.BackColor = Color.FromArgb(241, 196, 15);
                            e.CellStyle.ForeColor = Color.White;
                            break;
                        case "Cancelled":
                            e.CellStyle.BackColor = Color.FromArgb(231, 76, 60);
                            e.CellStyle.ForeColor = Color.White;
                            break;
                    }
                }
            };
        }
        catch (Exception ex)
        {
            MessageBox.Show($"خطأ في تحميل الطلبات: {ex.Message}", "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void DgvOrders_CellClick(object? sender, DataGridViewCellEventArgs e)
    {
        if (dgvOrders.CurrentRow == null) return;
        _selectedOrderId = (int)dgvOrders.CurrentRow.Cells["OrderId"].Value;
        var currentStatus = dgvOrders.CurrentRow.Cells["Status"].Value?.ToString();
        
        if (!string.IsNullOrEmpty(currentStatus))
        {
            cmbStatus.SelectedItem = currentStatus;
        }
    }

    private async Task BtnUpdateStatus_Click()
    {
        if (_selectedOrderId == null)
        {
            MessageBox.Show("يرجى اختيار طلب أولاً", "تنبيه", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        var newStatus = cmbStatus.SelectedItem?.ToString();
        if (string.IsNullOrEmpty(newStatus)) return;

        var confirm = MessageBox.Show($"هل أنت متأكد من تغيير حالة الطلب إلى {newStatus}؟", 
            "تأكيد", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

        if (confirm == DialogResult.Yes)
        {
            try
            {
                var req = new UpdateOrderRequest
                {
                    Status = newStatus,
                    DriverId = null,
                    Notes = null
                };
                await _orderService.UpdateAsync(_selectedOrderId.Value, req);
                await LoadOrders();
                MessageBox.Show("تم تحديث الحالة بنجاح!", "نجاح", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطأ: {ex.Message}", "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}