using System;
using System.Drawing;
using System.Windows.Forms;
using System.Threading.Tasks;
using FarmToTable.Gui.Models;
using FarmToTable.Gui.Services;

namespace FarmToTable.Gui.Forms
{
    public partial class CropsForm : Form
    {
        private readonly CropGuiService _cropService;
        private DataGridView dgvCrops;
        private TextBox txtName, txtUnit;
        private Button btnSave;

        public CropsForm(CropGuiService cropService)
        {
            _cropService = cropService;
            InitializeComponentCustom(); // غيرنا الاسم لتجنب التعارض مع ملف الديزاينر
            this.Load += async (s, e) => await LoadCrops();
        }

        private void InitializeComponentCustom()
        {
            this.Text = "إدارة المحاصيل";
            this.Size = new Size(800, 500);
            this.BackColor = Color.White;

            dgvCrops = new DataGridView
            {
                Location = new Point(20, 20),
                Size = new Size(450, 400),
                AllowUserToAddRows = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };

            var lblName = new Label { Text = "اسم المحصول:", Location = new Point(500, 50), Size = new Size(100, 25) };
            txtName = new TextBox { Location = new Point(610, 50), Size = new Size(150, 25) };

            var lblUnit = new Label { Text = "الوحدة:", Location = new Point(500, 90), Size = new Size(100, 25) };
            txtUnit = new TextBox { Location = new Point(610, 90), Size = new Size(150, 25) };

            btnSave = new Button
            {
                Text = "➕ إضافة محصول",
                Location = new Point(540, 140),
                Size = new Size(150, 40),
                BackColor = Color.FromArgb(46, 204, 113),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnSave.Click += async (s, e) => await BtnSave_Click();

            this.Controls.AddRange(new Control[] { dgvCrops, lblName, txtName, lblUnit, txtUnit, btnSave });
        }

        private async Task LoadCrops()
        {
            try {
                var crops = await _cropService.GetAllAsync();
                dgvCrops.DataSource = null;
                dgvCrops.DataSource = crops;
                if (dgvCrops.Columns["CropId"] != null) dgvCrops.Columns["CropId"].Visible = false;
            } catch (Exception ex) {
                MessageBox.Show("خطأ في تحميل البيانات: " + ex.Message);
            }
        }

        private async Task BtnSave_Click()
        {
            if (string.IsNullOrWhiteSpace(txtName.Text))
            {
                MessageBox.Show("يرجى إدخال اسم المحصول");
                return;
            }

            var req = new CreateCropRequest(txtName.Text.Trim(), txtUnit.Text.Trim());
            await _cropService.CreateAsync(req);
            txtName.Clear();
            txtUnit.Clear();
            await LoadCrops();
            MessageBox.Show("تمت الإضافة بنجاح!");
        }
    }
}