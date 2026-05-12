using FarmToTable.Gui.Models;
using FarmToTable.Gui.Services;

namespace FarmToTable.Gui.Forms;

public partial class RestaurantsForm : Form
{
    private readonly RestaurantGuiService _restaurantService;
    private int? _selectedId = null;
    private DataGridView dgvRestaurants;
    private TextBox txtName, txtAddress, txtEmail, txtPhone;
    private Button btnSave, btnDelete, btnClear;

    public RestaurantsForm(RestaurantGuiService restaurantService)
    {
        _restaurantService = restaurantService;
        InitializeComponent();
        Load += async (s, e) => await LoadRestaurants();
    }

    private void InitializeComponent()
    {
        this.Text = "إدارة المطاعم";
        this.Size = new Size(1000, 550);
        this.BackColor = Color.White;

        dgvRestaurants = new DataGridView
        {
            Location = new Point(20, 20),
            Size = new Size(600, 450),
            AllowUserToAddRows = false,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
        };
        dgvRestaurants.CellClick += (s, e) =>
        {
            if (dgvRestaurants.CurrentRow == null) return;
            _selectedId = (int)dgvRestaurants.CurrentRow.Cells["RestaurantId"].Value;
            txtName.Text = dgvRestaurants.CurrentRow.Cells["Name"].Value?.ToString();
            txtAddress.Text = dgvRestaurants.CurrentRow.Cells["Address"].Value?.ToString();
            txtEmail.Text = dgvRestaurants.CurrentRow.Cells["ContactEmail"].Value?.ToString();
            txtPhone.Text = dgvRestaurants.CurrentRow.Cells["ContactPhone"].Value?.ToString();
            btnSave.Text = "✏️ تحديث";
        };

        var lblName = new Label { Text = "اسم المطعم:", Location = new Point(650, 30), Size = new Size(100, 25) };
        txtName = new TextBox { Location = new Point(760, 30), Size = new Size(200, 25) };

        var lblAddress = new Label { Text = "العنوان:", Location = new Point(650, 70), Size = new Size(100, 25) };
        txtAddress = new TextBox { Location = new Point(760, 70), Size = new Size(200, 25) };

        var lblEmail = new Label { Text = "البريد:", Location = new Point(650, 110), Size = new Size(100, 25) };
        txtEmail = new TextBox { Location = new Point(760, 110), Size = new Size(200, 25) };

        var lblPhone = new Label { Text = "الهاتف:", Location = new Point(650, 150), Size = new Size(100, 25) };
        txtPhone = new TextBox { Location = new Point(760, 150), Size = new Size(200, 25) };

        btnSave = new Button
        {
            Text = "💾 حفظ جديد",
            Location = new Point(680, 200),
            Size = new Size(120, 40),
            BackColor = Color.FromArgb(46, 204, 113),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat
        };
        btnSave.Click += async (s, e) => await BtnSave_Click();

        btnDelete = new Button
        {
            Text = "🗑️ حذف",
            Location = new Point(820, 200),
            Size = new Size(120, 40),
            BackColor = Color.FromArgb(231, 76, 60),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat
        };
        btnDelete.Click += async (s, e) => await BtnDelete_Click();

        btnClear = new Button
        {
            Text = "🔄 إلغاء",
            Location = new Point(680, 250),
            Size = new Size(260, 35),
            BackColor = Color.FromArgb(149, 165, 166),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat
        };
        btnClear.Click += (s, e) => ClearInputs();

        this.Controls.AddRange(new Control[] { dgvRestaurants, lblName, txtName, lblAddress, txtAddress,
            lblEmail, txtEmail, lblPhone, txtPhone, btnSave, btnDelete, btnClear });
    }

    private async Task LoadRestaurants()
    {
        var items = await _restaurantService.GetAllAsync();
        dgvRestaurants.DataSource = null;
        dgvRestaurants.DataSource = items;
        dgvRestaurants.Columns["RestaurantId"].Visible = false;
        dgvRestaurants.Columns["Name"].HeaderText = "اسم المطعم";
        dgvRestaurants.Columns["Address"].HeaderText = "العنوان";
        dgvRestaurants.Columns["ContactEmail"].HeaderText = "البريد";
        dgvRestaurants.Columns["ContactPhone"].HeaderText = "الهاتف";
    }

    private async Task BtnSave_Click()
    {
        if (string.IsNullOrWhiteSpace(txtName.Text)) return;

        if (_selectedId == null)
        {
            var req = new CreateRestaurantRequest(txtName.Text, txtAddress.Text, txtEmail.Text, txtPhone.Text);
            await _restaurantService.CreateAsync(req);
        }
        else
        {
            var req = new UpdateRestaurantRequest(txtName.Text, txtAddress.Text, txtEmail.Text, txtPhone.Text);
            await _restaurantService.UpdateAsync(_selectedId.Value, req);
        }
        ClearInputs();
        await LoadRestaurants();
        MessageBox.Show("تم الحفظ بنجاح!");
    }

    private async Task BtnDelete_Click()
    {
        if (_selectedId == null) return;
        await _restaurantService.DeleteAsync(_selectedId.Value);
        ClearInputs();
        await LoadRestaurants();
        MessageBox.Show("تم الحذف بنجاح!");
    }

    private void ClearInputs()
    {
        txtName.Clear(); txtAddress.Clear(); txtEmail.Clear(); txtPhone.Clear();
        _selectedId = null;
        btnSave.Text = "💾 حفظ جديد";
    }
}