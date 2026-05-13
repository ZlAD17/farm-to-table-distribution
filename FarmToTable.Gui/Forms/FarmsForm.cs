using FarmToTable.Gui.Models;
using FarmToTable.Gui.Services;

namespace FarmToTable.Gui.Forms;

public partial class FarmsForm : Form
{
    private readonly FarmGuiService _farmService;
    private int? _selectedFarmId = null;
    
    private DataGridView dgvFarms;
    private TextBox txtName, txtLocation, txtEmail, txtPhone;
    private Button btnSave, btnDelete, btnClear;
    private Label lblName, lblLocation, lblEmail, lblPhone;

    public FarmsForm(FarmGuiService farmService)
    {
        _farmService = farmService;
        InitializeComponent();
        Load += async (s, e) => await LoadFarms();
    }

    private void InitializeComponent()
    {
        this.Text = "إدارة المزارع";
        this.Size = new Size(1000, 550);
        this.BackColor = Color.White;

        dgvFarms = new DataGridView
        {
            Location = new Point(20, 20),
            Size = new Size(600, 450),
            AllowUserToAddRows = false,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            MultiSelect = false,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
            BackgroundColor = Color.White,
            BorderStyle = BorderStyle.FixedSingle
        };
        dgvFarms.CellClick += DgvFarms_CellClick;

        lblName = new Label { Text = "اسم المزرعة:", Location = new Point(650, 30), Size = new Size(100, 25), Font = new Font("Segoe UI", 10) };
        txtName = new TextBox { Location = new Point(760, 30), Size = new Size(200, 25) };

        lblLocation = new Label { Text = "الموقع:", Location = new Point(650, 70), Size = new Size(100, 25), Font = new Font("Segoe UI", 10) };
        txtLocation = new TextBox { Location = new Point(760, 70), Size = new Size(200, 25) };

        lblEmail = new Label { Text = "البريد الإلكتروني:", Location = new Point(650, 110), Size = new Size(100, 25), Font = new Font("Segoe UI", 10) };
        txtEmail = new TextBox { Location = new Point(760, 110), Size = new Size(200, 25) };

        lblPhone = new Label { Text = "رقم الهاتف:", Location = new Point(650, 150), Size = new Size(100, 25), Font = new Font("Segoe UI", 10) };
        txtPhone = new TextBox { Location = new Point(760, 150), Size = new Size(200, 25) };

        btnSave = new Button
        {
            Text = "💾 حفظ جديد",
            Location = new Point(680, 200),
            Size = new Size(120, 40),
            BackColor = Color.FromArgb(46, 204, 113),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Font = new Font("Segoe UI", 10, FontStyle.Bold)
        };
        btnSave.Click += async (s, e) => await BtnSave_Click();

        btnDelete = new Button
        {
            Text = "🗑️ حذف",
            Location = new Point(820, 200),
            Size = new Size(120, 40),
            BackColor = Color.FromArgb(231, 76, 60),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Font = new Font("Segoe UI", 10, FontStyle.Bold)
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

        this.Controls.AddRange(new Control[] { dgvFarms, lblName, txtName, lblLocation, txtLocation,
            lblEmail, txtEmail, lblPhone, txtPhone, btnSave, btnDelete, btnClear });
    }

    private async Task LoadFarms()
    {
        try
        {
            var farms = await _farmService.GetAllAsync();
            dgvFarms.DataSource = null;
            dgvFarms.DataSource = farms;
            
            if (dgvFarms.Columns.Contains("FarmId"))
                dgvFarms.Columns["FarmId"].Visible = false;
            
            dgvFarms.Columns["Name"].HeaderText = "اسم المزرعة";
            dgvFarms.Columns["Location"].HeaderText = "الموقع";
            dgvFarms.Columns["ContactEmail"].HeaderText = "البريد الإلكتروني";
            dgvFarms.Columns["ContactPhone"].HeaderText = "رقم الهاتف";
            dgvFarms.Columns["CreatedAt"].HeaderText = "تاريخ الإنشاء";
        }
        catch (Exception ex)
        {
            MessageBox.Show($"خطأ: {ex.Message}");
        }
    }

    private async Task BtnSave_Click()
    {
        if (string.IsNullOrWhiteSpace(txtName.Text))
        {
            MessageBox.Show("يرجى إدخال اسم المزرعة");
            return;
        }

        try
        {
            if (_selectedFarmId == null)
            {
                var req = new CreateFarmRequest(txtName.Text.Trim(), txtLocation.Text.Trim(),
                    string.IsNullOrEmpty(txtEmail.Text) ? null : txtEmail.Text,
                    string.IsNullOrEmpty(txtPhone.Text) ? null : txtPhone.Text);
                await _farmService.CreateAsync(req);
                MessageBox.Show("تمت الإضافة بنجاح!");
            }
            else
            {
                var req = new UpdateFarmRequest(txtName.Text.Trim(), txtLocation.Text.Trim(),
                    string.IsNullOrEmpty(txtEmail.Text) ? null : txtEmail.Text,
                    string.IsNullOrEmpty(txtPhone.Text) ? null : txtPhone.Text);
                await _farmService.UpdateAsync(_selectedFarmId.Value, req);
                MessageBox.Show("تم التحديث بنجاح!");
            }
            ClearInputs();
            await LoadFarms();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"خطأ: {ex.Message}");
        }
    }

    private async Task BtnDelete_Click()
    {
        if (_selectedFarmId == null)
        {
            MessageBox.Show("يرجى اختيار مزرعة للحذف");
            return;
        }

        var confirm = MessageBox.Show("هل أنت متأكد من الحذف؟", "تأكيد", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
        if (confirm == DialogResult.Yes)
        {
            await _farmService.DeleteAsync(_selectedFarmId.Value);
            ClearInputs();
            await LoadFarms();
            MessageBox.Show("تم الحذف بنجاح!");
        }
    }

    private void DgvFarms_CellClick(object? sender, DataGridViewCellEventArgs e)
    {
        if (dgvFarms.CurrentRow == null) return;

        _selectedFarmId = (int)dgvFarms.CurrentRow.Cells["FarmId"].Value;
        txtName.Text = dgvFarms.CurrentRow.Cells["Name"].Value?.ToString();
        txtLocation.Text = dgvFarms.CurrentRow.Cells["Location"].Value?.ToString();
        txtEmail.Text = dgvFarms.CurrentRow.Cells["ContactEmail"].Value?.ToString();
        txtPhone.Text = dgvFarms.CurrentRow.Cells["ContactPhone"].Value?.ToString();
        btnSave.Text = "✏️ تحديث";
    }

    private void ClearInputs()
    {
        txtName.Clear();
        txtLocation.Clear();
        txtEmail.Clear();
        txtPhone.Clear();
        _selectedFarmId = null;
        btnSave.Text = "💾 حفظ جديد";
    }
}