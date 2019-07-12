using System;
using System.Windows.Forms;

namespace PR2_Speedrun_Tools
{
	public partial class TokenManagerForm : Form
	{
		public TokenManagerForm()
		{
			InitializeComponent();
		}

		Settings settings { get { return General.Settings; } }

		private void Token_Manager_Load(object sender, EventArgs e)
		{
			for (int i = 0; i < settings.Users.Count; i++)
				usersList.Items.Add(settings.Users[i]);

			usersList.SelectedIndex = settings.SelectedUser;
		}

		private void usersList_SelectedIndexChanged(object sender, EventArgs e)
		{
			if (usersList.SelectedIndex == -1)
			{
				userBox.Enabled = false;
				tokenBox.Enabled = false;
				return;
			}

			userBox.Enabled = true;
			tokenBox.Enabled = true;
			userBox.Text = settings.Users[usersList.SelectedIndex];
			tokenBox.Text = settings.Tokens[usersList.SelectedIndex];
		}

		private void removeBtn_Click(object sender, EventArgs e)
		{
			if (usersList.SelectedIndex == -1)
				return;

			settings.Users.RemoveAt(usersList.SelectedIndex);
			settings.Tokens.RemoveAt(usersList.SelectedIndex);
			usersList.Items.RemoveAt(usersList.SelectedIndex);
		}
		private void addBtn_Click(object sender, EventArgs e)
		{
			settings.Users.Add("new user");
			settings.Tokens.Add("123");

			usersList.Items.Add("new user");
			usersList.SelectedIndex = usersList.Items.Count - 1;
		}

		private void userBox_TextChanged(object sender, EventArgs e)
		{
			if (usersList.SelectedIndex == -1)
				return;

			settings.Users[usersList.SelectedIndex] = userBox.Text;
		}
		private void tokenBox_TextChanged(object sender, EventArgs e)
		{
			if (usersList.SelectedIndex == -1)
				return;

			settings.Tokens[usersList.SelectedIndex] = tokenBox.Text;
		}

		bool saveExit = false;
		private void saveBtn_Click(object sender, EventArgs e)
		{
			settings.SelectedUser = usersList.SelectedIndex;
			settings.Save();
			saveExit = true;
			this.Close();
		}
		private void Token_Manager_FormClosing(object sender, FormClosingEventArgs e)
		{
			if (!saveExit)
			{
				DialogResult result = MessageBox.Show("Do you want to save your changes?", "Save?", MessageBoxButtons.YesNoCancel);
				if (result == DialogResult.Yes)
					saveBtn_Click(null, null);
				else if (result == DialogResult.Cancel)
					e.Cancel = true;
				else // Re-load settings
				{
					Settings old = Settings.Load();
					settings.Users = old.Users;
					settings.Tokens = old.Tokens;
				}
			}
		}
	}
}
