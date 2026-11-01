using Sep2026.ConsoleApp5.Database.AppDbContextModels;

namespace Sep2026.WinFormsApp
{
    public partial class FrmStudent : Form
    {
        public FrmStudent()
        {
            InitializeComponent();
        }

        private void btnClick_Click(object sender, EventArgs e)
        {
            MessageBox.Show("hello world!");
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            ClearControls();
        }

        private void ClearControls()
        {
            txtNo.Clear();
            txtName.Clear();
            txtClassNo.Clear();

            txtNo.Focus();
        }

        // Ctrl + R, R
        private void btnSave_Click(object sender, EventArgs e)
        {
            try
            {
                AppDbContext db = new AppDbContext();
                db.Students.Add(new Student
                {
                    No = txtNo.Text.Trim(),
                    Name = txtName.Text.Trim(),
                    ClassNo = txtClassNo.Text.Trim(),
                });
                int result = db.SaveChanges();
                string message = result > 0 ? "Saving Successful." : "Saving Failed.";
                MessageBoxIcon icon = result > 0 ? MessageBoxIcon.Information : MessageBoxIcon.Error;

                MessageBox.Show(message, "", MessageBoxButtons.OK, icon);

                ClearControls();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }
    }
}
