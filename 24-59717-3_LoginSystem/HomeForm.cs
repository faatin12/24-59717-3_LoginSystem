using System;
using System.Windows.Forms;

namespace LoginSystem
{
    public class HomeForm : Form
    {
        private readonly int loginHistoryId;
        private readonly LoginForm loginForm;
        private bool logoutRecorded = false;

        private Label lblWelcome;
        private DataGridView grid;
        private TextBox txtSearch;
        private Button btnSearch, btnRefresh, btnDelete, btnLogout;

        public HomeForm(string fullName, int loginHistoryId, LoginForm loginForm)
        {
            this.loginHistoryId = loginHistoryId;
            this.loginForm = loginForm;
            InitializeComponent(fullName);
            LoadUsers();
        }

        private void InitializeComponent(string fullName)
        {
            this.Text = "Home";
            this.Width = 660;
            this.Height = 520;
            this.StartPosition = FormStartPosition.CenterScreen;

            lblWelcome = new Label
            {
                Text = $"Welcome, {fullName}",
                Left = 20,
                Top = 15,
                Width = 500,
                Height = 40,
                Font = new System.Drawing.Font("Segoe UI", 18, System.Drawing.FontStyle.Bold)
            };

            txtSearch = new TextBox { Left = 20, Top = 75, Width = 200, Height = 30 };

            btnSearch = new Button { Text = "Search", Left = 230, Top = 72, Width = 80, Height = 36 };
            btnSearch.Click += (s, e) => LoadUsers(txtSearch.Text.Trim());

            btnRefresh = new Button { Text = "Refresh", Left = 320, Top = 72, Width = 80, Height = 36 };
            btnRefresh.Click += (s, e) => { txtSearch.Clear(); LoadUsers(); };

            btnDelete = new Button { Text = "Delete Selected", Left = 410, Top = 72, Width = 120, Height = 36 };
            btnDelete.Click += BtnDelete_Click;

            btnLogout = new Button { Text = "Logout", Left = 540, Top = 72, Width = 90, Height = 36 };
            btnLogout.Click += BtnLogout_Click;

            grid = new DataGridView
            {
                Left = 20,
                Top = 120,
                Width = 610,
                Height = 350,
                ReadOnly = true,
                AllowUserToAddRows = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };

            this.Controls.AddRange(new Control[]
            {
                lblWelcome, txtSearch, btnSearch, btnRefresh, btnDelete, btnLogout, grid
            });

            this.FormClosing += (s, e) =>
            {
                RecordLogoutOnce();
                loginForm.ClearForm();
                loginForm.Show();
            };
        }

        private void LoadUsers(string term = null)
        {
            grid.DataSource = string.IsNullOrWhiteSpace(term)
                ? DatabaseHelper.GetUsersTable()
                : DatabaseHelper.SearchUsers(term);

            foreach (DataGridViewColumn col in grid.Columns)
            {
                if (col.Name.IndexOf("password", StringComparison.OrdinalIgnoreCase) >= 0 ||
                    col.Name.IndexOf("hash", StringComparison.OrdinalIgnoreCase) >= 0 ||
                    col.Name.IndexOf("salt", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    col.Visible = false;
                }
            }
        }

        private void BtnDelete_Click(object sender, EventArgs e)
        {
            if (grid.CurrentRow == null)
            {
                MessageBox.Show("Select a row first.");
                return;
            }

            int selectedId = Convert.ToInt32(grid.CurrentRow.Cells["UserID"].Value);

            var confirm = MessageBox.Show($"Delete user #{selectedId}? This cannot be undone.",
                "Confirm delete", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

            if (confirm == DialogResult.Yes)
            {
                DatabaseHelper.DeleteUser(selectedId);
                LoadUsers();
            }
        }

        private void RecordLogoutOnce()
        {
            if (!logoutRecorded)
            {
                DatabaseHelper.RecordLogout(loginHistoryId);
                logoutRecorded = true;
            }
        }

        private void BtnLogout_Click(object sender, EventArgs e)
        {
            RecordLogoutOnce();
            loginForm.ClearForm();
            loginForm.Show();
            this.Close();
        }
    }
}