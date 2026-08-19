using System;
using System.Windows.Forms;

namespace LoginSystem
{
    public class RegisterForm : Form
    {
        private TextBox txtUsername, txtPassword, txtConfirmPassword, txtEmail, txtFullName;
        private Button btnRegister, btnCancel;
        private Label lblStatus;

        public RegisterForm()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = "Register";
            this.Width = 380;
            this.Height = 380;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.StartPosition = FormStartPosition.CenterScreen;

            int top = 20;

            var lblUsername = new Label { Text = "Username", Left = 20, Top = top, Width = 110 };
            txtUsername = new TextBox { Left = 140, Top = top, Width = 190 };
            top += 35;

            var lblPassword = new Label { Text = "Password", Left = 20, Top = top, Width = 110 };
            txtPassword = new TextBox { Left = 140, Top = top, Width = 190, PasswordChar = '*' };
            top += 35;

            var lblConfirm = new Label { Text = "Confirm Password", Left = 20, Top = top, Width = 110 };
            txtConfirmPassword = new TextBox { Left = 140, Top = top, Width = 190, PasswordChar = '*' };
            top += 35;

            var lblEmail = new Label { Text = "Email", Left = 20, Top = top, Width = 110 };
            txtEmail = new TextBox { Left = 140, Top = top, Width = 190 };
            top += 35;

            var lblFullName = new Label { Text = "Full Name", Left = 20, Top = top, Width = 110 };
            txtFullName = new TextBox { Left = 140, Top = top, Width = 190 };
            top += 45;

            btnRegister = new Button { Text = "Register", Left = 140, Top = top, Width = 90 };
            btnRegister.Click += BtnRegister_Click;

            btnCancel = new Button { Text = "Cancel", Left = 240, Top = top, Width = 90 };
            btnCancel.Click += (s, e) => this.Close();

            top += 40;
            lblStatus = new Label { Left = 20, Top = top, Width = 320, Height = 60, ForeColor = System.Drawing.Color.Red };

            this.Controls.AddRange(new Control[]
            {
                lblUsername, txtUsername, lblPassword, txtPassword,
                lblConfirm, txtConfirmPassword, lblEmail, txtEmail,
                lblFullName, txtFullName, btnRegister, btnCancel, lblStatus
            });

            this.AcceptButton = btnRegister;
        }

        private void BtnRegister_Click(object sender, EventArgs e)
        {
            string username = txtUsername.Text.Trim();
            string password = txtPassword.Text;
            string confirm = txtConfirmPassword.Text;
            string email = txtEmail.Text.Trim();
            string fullName = txtFullName.Text.Trim();

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password) ||
                string.IsNullOrWhiteSpace(confirm) || string.IsNullOrWhiteSpace(email) ||
                string.IsNullOrWhiteSpace(fullName))
            {
                lblStatus.Text = "All fields are required.";
                return;
            }

            if (password.Length < 6)
            {
                lblStatus.Text = "Password must be at least 6 characters.";
                return;
            }

            if (password != confirm)
            {
                lblStatus.Text = "Passwords do not match.";
                return;
            }

            if (!email.Contains("@"))
            {
                lblStatus.Text = "Enter a valid email address.";
                return;
            }

            try
            {
                if (DatabaseHelper.UsernameExists(username))
                {
                    lblStatus.Text = "Username already taken.";
                    return;
                }

                DatabaseHelper.RegisterUser(username, password, email, fullName);

                MessageBox.Show("Registration successful. You can now log in.", "Success",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);

                txtUsername.Clear();
                txtPassword.Clear();
                txtConfirmPassword.Clear();
                txtEmail.Clear();
                txtFullName.Clear();
                this.Close();
            }
            catch (Exception ex)
            {
                lblStatus.Text = "Registration failed: " + ex.Message;
            }
        }
    }
}