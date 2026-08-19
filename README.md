# 24-59717-3_LoginSystem

A WinForms Registration / Login / Logout application backed by SQL Server,
built for Lab 1 (Login, Registration & Logout with C# and SQL Server).

## Environment

- SQL Server: SQL Server Express LocalDB (`(localdb)\MSSQLLocalDB`)
- Visual Studio: _(fill in your exact version, e.g. Visual Studio 2022 17.x)_
- .NET: .NET 8.0 (Windows Forms App)
- Database access: `Microsoft.Data.SqlClient` NuGet package (the .NET 8
  replacement for `System.Data.SqlClient`), plus
  `System.Configuration.ConfigurationManager` to read `App.config`
- Connection string format (no real password - Windows Authentication is
  used, so there is no password to leak):

  ```
  Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=YourID_LoginDB;Integrated Security=True;Connect Timeout=30
  ```

## Database

Created with `Schema.sql` (included in this repo) using a query window
connected to `(localdb)\MSSQLLocalDB` in Visual Studio. The script creates:

- `dbo.Users` - holds accounts. Beyond the required columns, it also has a
  `PasswordSalt` column (allowed by the spec, which says columns may be
  added but not removed) used for per-user salted password hashing.
- `dbo.LoginHistory` (bonus) - logs every login/logout with a foreign key
  back to `Users`.



## How registration, login, and logout work

- **`DatabaseHelper.cs`** is the only class in the project that ever opens a
  `SqlConnection`. Every method wraps its connection and command in
  `using(...)` blocks (so connections close even if an exception is thrown)
  and every query is parameterized - nothing is ever built with string
  concatenation.
- **`RegisterForm.cs`** validates input client-side first (no empty fields,
  password ≥ 6 characters, passwords match, email contains `@`), then calls
  `DatabaseHelper.UsernameExists()` (an `ExecuteScalar` `COUNT(*)` query)
  before `DatabaseHelper.RegisterUser()`, which hashes the password before
  it ever reaches SQL and inserts with a parameterized `ExecuteNonQuery()`.
  On success it shows a message box, clears the form, and closes, returning
  to `LoginForm`.
- **`LoginForm.cs`** calls `DatabaseHelper.ValidateLogin()`, which looks up
  the user by username with a parameterized `SqlDataReader` query, re-hashes
  the entered password using that user's stored salt, and compares hashes -
  never plain text. On success it calls `DatabaseHelper.RecordLogin()`
  (writing the bonus `LoginHistory` row) and opens `HomeForm` with
  `"Welcome, {FullName}"`, hiding the login form. On failure it shows both an
  inline red status label and a message box; after 3 failed attempts the
  Login button is disabled.
- **`HomeForm.cs`** shows the welcome message and a `DataGridView` of users
  (via `DatabaseHelper.GetUsersTable()` / `SearchUsers()`, both
  `SqlDataAdapter` + `DataTable`). The Logout button calls
  `DatabaseHelper.RecordLogout()` (stamping `LogoutTime`), clears
  `LoginForm` via its public `ClearForm()` method, and shows it again - the
  application process never exits and no form is left running in the
  background. A `FormClosing` handler acts as a safety net if the window is
  closed via the X button instead of the Logout button.


## Password hashing

Passwords are hashed with SHA-256 over `salt + password`
(`PasswordHelper.cs`), where the salt is a random 16-byte value generated
per user with `RandomNumberGenerator`. The hash is computed once at
registration and stored, then recomputed with the same salt at each login
attempt for comparison - the real password itself is never stored anywhere,
and even two users with the same password end up with different hashes
because their salts differ. Storing passwords in plain text is unacceptable
because a single database leak (backup theft, SQL injection, misconfigured
access, etc.) immediately exposes every user's real password, including on
any other site they reused it on - hashing means a leak only exposes hashes,
which are computationally infeasible to reverse.

## SQL injection demo (Task 6)

- **Vulnerable code:** `InjectionDemo/VulnerableLoginDemo.cs`,
  `VulnerableLogin()` method. It builds the query by string concatenation,
  exactly like the bug in the sample project:

  ```csharp
  string sql = "SELECT COUNT(*) FROM Users WHERE Username='" + username +
               "' AND PasswordHash='" + password + "'";
  ```

- **Exploit input:** username `x`, password `' OR '1'='1`
- **Result:** the concatenated SQL becomes
  `...WHERE Username='x' AND PasswordHash='' OR '1'='1'`, and because `AND`
  binds tighter than `OR`, this is evaluated as
  `(Username='x' AND PasswordHash='') OR ('1'='1')` - always true - so every
  row matches and the "login" succeeds with no valid password at all.

 

- **Fixed code:** same file, `FixedLogin()` method, and the real app's
  `DatabaseHelper.ValidateLogin()`, both parameterized:

  ```csharp
  string sql = "SELECT COUNT(*) FROM Users WHERE Username=@username AND PasswordHash=@hash";
  cmd.Parameters.AddWithValue("@username", username);
  cmd.Parameters.AddWithValue("@hash", passwordHash);
  ```

  

- **Why parameters stop it:** with a parameterized query, the value the
  user typed is sent to SQL Server *separately* from the SQL command text
  and is never parsed as SQL syntax - it is only ever compared as a literal
  string value. So `' OR '1'='1` is just treated as a (wrong) password to
  compare against a stored hash, instead of being interpreted as `OR`
  logic that widens the WHERE clause. The attacker's input can change what
  data is matched, but it can never change the shape of the query itself.

## Bonus tasks attempted

- [x] `LoginHistory` table with a foreign key to `Users`, stamped with
      `LoginTime` on login and `LogoutTime` on logout
- [x] Delete a user from the grid, with a confirmation dialog
      (`HomeForm.BtnDelete_Click`)
- [x] All database code moved into `DatabaseHelper` - no form ever
      touches `SqlConnection` directly
- [x] Search/filter the grid by username using a parameterized
      `LIKE @term` query (`DatabaseHelper.SearchUsers`)
- [ ] Change-password screen - not attempted (four bonus tasks were
      completed, exceeding the required two)

## Problems I hit and how I solved them

_(Fill this in with your own experience - a few real examples from this
build:)_

- My `.cs` files initially got added to the "Solution Items" node instead
  of the actual project, so the build couldn't see classes like `LoginForm`
  (`CS0246: type or namespace could not be found`). Fixed by moving the
  files into the actual project node in Solution Explorer.
- Since I used .NET 8 instead of .NET Framework 4.7.2, `System.Data.SqlClient`
  wasn't available by default - I installed the `Microsoft.Data.SqlClient`
  and `System.Configuration.ConfigurationManager` NuGet packages instead and
  updated my `using` statements accordingly.
- LocalDB wasn't installed on my machine initially, causing connection
  failures - installed "SQL Server Express LocalDB" as an individual
  component through the Visual Studio Installer.
- _(add anything else you personally ran into)_
