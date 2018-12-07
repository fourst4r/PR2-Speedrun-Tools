using System.Windows.Forms;

namespace PR2_Speedrun_Tools
{
    public partial class LoginForm : Form
    {
        public string Username { get => textBoxUsername.Text; }
        public string Password { get => textBoxPassword.Text; }

        public LoginForm()
        {
            InitializeComponent();
        }
    }
}
