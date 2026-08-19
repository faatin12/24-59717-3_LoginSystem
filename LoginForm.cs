using System;
using System.Windows.Forms;

namespace LoginSystem
{
    public class LoginForm : Form
    {
        private TextBox txtUsername;
        private TextBox txtPassword;
        private Button btnLogin;
        private Button btnGoToRegister;
        private Label lblStatus;
        private int failedAttempts = 0;

        public LoginForm()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = "Login";
            this.Width = 340;
            this.Height = 260;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.StartPosition = FormStartPosition.CenterScreen;

            var lblTitle = new Label
            {
                Text = "Sign in",
                Left = 20,
                Top = 15,
                Width = 200,
                Font = new System.Drawing.Font("Segoe UI", 14)
            };

            var lblUsername = new Label { Text = "Username", Left = 20, Top = 60, Width = 80 };
            txtUsername = new TextBox { Left = 110, Top = 57, Width = 190 };

            var lblPassword = new Label { Text = "Password", Left = 20, Top = 95, Width = 80 };
            txtPassword = new TextBox { Left = 110, Top = 92, Width = 190, PasswordChar = '*' };

            btnLogin = new Button { Text = "Login", Left = 110, Top = 130, Width = 90 };
            btnLogin.Click += BtnLogin_Click;

            btnGoToRegister = new Button { Text = "Register", Left = 210, Top = 130, Width = 90 };
            btnGoToRegister.Click += BtnGoToRegister_Click;

            lblStatus = new Label { Left = 20, Top = 170, Width = 280, Height = 40, ForeColor = System.Drawing.Color.Red };

            this.Controls.Add(lblTitle);
            this.Controls.Add(lblUsername);
            this.Controls.Add(txtUsername);
            this.Controls.Add(lblPassword);
            this.Controls.Add(txtPassword);
            this.Controls.Add(btnLogin);
            this.Controls.Add(btnGoToRegister);
            this.Controls.Add(lblStatus);

            this.Load += (s, e) =>
            {
                if (!DatabaseHelper.TestConnection(out string err))
                {
                    MessageBox.Show("Could not connect to the database:\n" + err, "Connection error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            };

            this.AcceptButton = btnLogin;
            txtUsername.Focus();
        }

        public void ClearForm()
        {
            txtUsername.Clear();
            txtPassword.Clear();
            lblStatus.Text = "";
            failedAttempts = 0;
            btnLogin.Enabled = true;
            txtUsername.Focus();
        }

        private void BtnLogin_Click(object sender, EventArgs e)
        {
            string username = txtUsername.Text.Trim();
            string password = txtPassword.Text;

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                lblStatus.Text = "Enter both username and password.";
                return;
            }

            var result = DatabaseHelper.ValidateLogin(username, password);

            if (result == null)
            {
                failedAttempts++;
                lblStatus.Text = $"Invalid username or password. ({failedAttempts}/3)";
                if (failedAttempts >= 3)
                {
                    btnLogin.Enabled = false;
                    lblStatus.Text = "Too many failed attempts. Login disabled.";
                }
                return;
            }

            int loginHistoryId = DatabaseHelper.RecordLogin(result.Value.UserID);

            var home = new HomeForm(result.Value.FullName, loginHistoryId, this);
            this.Hide();
            home.Show();
        }

        private void BtnGoToRegister_Click(object sender, EventArgs e)
        {
            var registerForm = new RegisterForm();
            registerForm.ShowDialog();
        }
    }
}