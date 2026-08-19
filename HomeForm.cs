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
            this.Width = 640;
            this.Height = 480;
            this.StartPosition = FormStartPosition.CenterScreen;

            lblWelcome = new Label
            {
                Text = $"Welcome, {fullName}",
                Left = 20,
                Top = 15,
                Width = 400,
                Font = new System.Drawing.Font("Segoe UI", 12)
            };

            txtSearch = new TextBox { Left = 20, Top = 50, Width = 200 };

            btnSearch = new Button { Text = "Search", Left = 230, Top = 48, Width = 80 };
            btnSearch.Click += (s, e) => LoadUsers(txtSearch.Text.Trim());

            btnRefresh = new Button { Text = "Refresh", Left = 320, Top = 48, Width = 80 };
            btnRefresh.Click += (s, e) => { txtSearch.Clear(); LoadUsers(); };

            btnDelete = new Button { Text = "Delete Selected", Left = 410, Top = 48, Width = 110 };
            btnDelete.Click += BtnDelete_Click;

            btnLogout = new Button { Text = "Logout", Left = 530, Top = 48, Width = 80 };
            btnLogout.Click += BtnLogout_Click;

            grid = new DataGridView
            {
                Left = 20,
                Top = 90,
                Width = 590,
                Height = 330,
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