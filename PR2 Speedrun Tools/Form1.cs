using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Net;
using System.Threading;
using System.Security.Cryptography;
using System.Globalization;
using System.Runtime.InteropServices;

namespace PR2_Speedrun_Tools
{
	public partial class Form1 : Form
	{
		string RecordingsPath
		{
			get { return General.Settings.RecordingsPath; }
			set
			{
				General.Settings.RecordingsPath = value;
				General.Settings.Save();
			}
		}
		string LevelsPath
		{
			get { return General.Settings.LevelsPath; }
			set
			{
				General.Settings.LevelsPath = value;
				General.Settings.Save();
			}
		}
		string SavestatesPath
		{
			get { return General.Settings.SavestatesPath; }
			set
			{
				General.Settings.SavestatesPath = value;
				General.Settings.Save();
			}
		}

		public Form1()
		{
			InitializeComponent();
            this.Text = "PR2 Speedrun Tools v" + Application.ProductVersion;
		}
		private void ResetPaths()
		{
			RecordingsPath = "";
			LevelsPath = "";
		}

		Game_ART game;

		Map theMap
        {
            get => game.map;
            set => game.map = value;
        }
        LocalCharacter You
        {
            get => game.Players[SelectedPlayer];
            set => game.Players[SelectedPlayer] = value;
        }

        CheckBox[] chkItems;
		CheckBox[] chkInput;
		CheckBox[] chkHats;
		private bool loaded = false;
		private void Form1_Load(object sender, EventArgs e)
		{
			Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
			General.FormRef = this;

			// Set up the game.
			game = new Game_ART(pnlGame.Width, pnlGame.Height);
			if (General.Settings.SelectedUser == -1)
				You.Name = "Player";
			else
			{
				You.Name = General.Settings.Users[General.Settings.SelectedUser];
				theMap.userName = You.Name;
				username = You.Name;
				login_token = General.Settings.Tokens[General.Settings.SelectedUser];
			}
			pnlGame.BackgroundImage = game.img.Bit;

			// Event handlers
			game.FinishDrawing += drawGame;
			game.endFrame += endOfFrame;

			// Control array(s)
			chkItems = new CheckBox[] { chkLaserGun, chkMine, chkLightning, chkTeleport, chkSuperJump, chkJetPack, chkSpeedy, chkSword, chkFreezeRay };
			chkHats = new CheckBox[] { null, null, null, null, chkProp, chkCowboy, chkCrown, chkSanta, null, chkTop, chkJump, null, null, chkJigg, chkArti };
			for (int i = 0; i < chkItems.Length; i++)
				chkItems[i].Tag = i + 1;
			chkInput = new CheckBox[] { chkSpace, chkLeft, chkRight, chkUp, chkDown };
			for (int i = 0; i < chkInput.Length; i++)
				chkInput[i].Tag = i;
			for (int i = 0; i < chkHats.Length; i++)
			{
				if (chkHats[i] != null)
					chkHats[i].Tag = i;
			}

			orderBox.SelectedIndex = 0;
			modeBox.SelectedIndex = 0;

			// Initialize video(?) recordings

			loaded = true;
		}

		private void Form1_FormClosing(object sender, FormClosingEventArgs e)
		{
			game.Dispose();
		}

		private void pnlGame_Resize(object sender, EventArgs e)
		{
			if (!loaded || pnlGame.Width == 0)
				return;
			game.Resize(pnlGame.Width, pnlGame.Height);
			pnlGame.BackgroundImage = game.img.Bit;
		}

		bool manual = true;

		#region "Event handled"
		private void drawGame()
		{
			pnlGame.SuspendLayout();
			pnlGame.CreateGraphics().DrawImage(pnlGame.BackgroundImage, 0, 0);
			pnlGame.ResumeLayout();
		}

		private void endOfFrame()
		{
			if (cChannel.Frames != 0)
			{
				if (numFrame.Value > cChannel.Frames - 1)
					numFrame.Value = cChannel.Frames - 1;
				numFrame.Maximum = cChannel.Frames - 1;
			}

			DisplayInfos();

			if (recordingVideo)
			{
				recordingFrame++;
				game.img.Bit.Save(recordingPath + "\\img" + recordingFrame.ToString().PadLeft(5, '0') + ".png");
			}
		}
		#endregion

		private void DisplayInfos()
		{
            lblVelX.Text = "VelX: " + General.FormatNumber(Math.Round(You.velX, 2), 2);
			lblVelY.Text = "VelY: " + General.FormatNumber(Math.Round(You.velY, 2), 2);
			double X = Math.Round(You.X, 2);
			X -= (Math.Floor(X / 30) * 30);
			double Y = Math.Round(You.Y, 2);
			Y -= (Math.Floor(Y / 30) * 30);
			lblPosX.Text = "PosX: " + General.FormatNumber(Math.Round(X, 2), 2);
			lblPosY.Text = "PosY: " + General.FormatNumber(Math.Round(Y, 2), 2);
			lblTVel.Text = "TVel: " + General.FormatNumber(Math.Round(You.TargetVel, 2), 2);
			lblSJump.Text = "SJump: " + General.FormatNumber(You.SuperJumpVel, 0, 0);
			lblHurt.Text = "Hurt: " + General.FormatNumber(You.HurtTimer, 0, 0);
			lblMode.Text = "Mode: " + You.Mode;
            lblState.Text = "State: " + You.State;

			numSpeed.Value = You.SpStat;
			numAccel.Value = You.AccStat;
			numJump.Value = You.JumpStat;

			lblFrames.Text = cChannel.Frames.ToString();
			currentFPSLbl.Text = General.FormatNumber(Math.Round(game.currentFPS, 2), 2);
		}

		// Load a level
		private void btnLoadLevel_Click(object sender, EventArgs e)
		{
            using (var dialog = new OpenFileDialog())
            {
                dialog.InitialDirectory = LevelsPath;

                if (dialog.ShowDialog() == DialogResult.Cancel)
                    return;

                theMap.enterLE(); // Idk what happens if you try to load a level mid-game
                theMap.LoadLevel(File.ReadAllText(dialog.FileName));

                UpdateLevelInfo();

                LevelsPath = Directory.GetParent(dialog.FileName).FullName;
            }
		}

		// Update displayed level info
		private void UpdateLevelInfo()
		{
			manual = false;

			txtTitle.Text = theMap.Title;
			txtNote.Text = theMap.note;
			txtCredits.Text = theMap.credits;

			numGravity.Value = (decimal)theMap.Gravity;
			numCowboyChance.Value = (decimal)theMap.cowboyChance;
			cmbMusic.SelectedIndex = theMap.song;
			cmbGameMode.Text = theMap.gameMode;

			for (int i = 0; i < chkItems.Length; i++)
				chkItems[i].Checked = theMap.avItems.Contains(i + 1);

			if (theMap.live == 1)
				chkLive.CheckState = CheckState.Checked;
			else if (theMap.hasPass)
				chkLive.CheckState = CheckState.Indeterminate;
			else
				chkLive.CheckState = CheckState.Unchecked;

			manual = true;
		}

		// Save states
		int curSS = 0;

		// Recording
		private RecordedChannel cChannel => game.recording.channels[SelectedPlayer];

	    private void btnLoadRec_Click(object sender, EventArgs e)
		{
			game.paused = true;
			lblPause.Text = "Paused for Rec";
			if (game.isPlayingRec)
				btnPlayRec_Click(null, null);

            using (var dialog = new OpenFileDialog())
            {
                dialog.InitialDirectory = RecordingsPath;

                if (dialog.ShowDialog() == DialogResult.Cancel || dialog.FileName == "")
                    return;

                game.recording = new Recording(dialog.FileName);
                RecordingsPath = Directory.GetParent(dialog.FileName).FullName;

                string SSPath = RecordingsPath + "\\States\\" + dialog.SafeFileName;
                if (File.Exists(SSPath))
                    game.recording.SS = new SaveState(SSPath);

                lblRecStatus.Text = "Loaded: " + dialog.SafeFileName;
            }
            numFrame.Maximum = cChannel.Frames - 1;
		}

		private void btnSaveRec_Click(object sender, EventArgs e)
		{
            using (var dialog = new SaveFileDialog())
            {
                dialog.InitialDirectory = RecordingsPath;

                if (dialog.ShowDialog() == DialogResult.Cancel || dialog.FileName == "")
                    return;

                game.recording.Save(dialog.FileName);
                DirectoryInfo dir = new DirectoryInfo(dialog.FileName);

                RecordingsPath = dir.Parent.FullName;

                if (game.recording.SS != null)
                    game.recording.SS.Save(dir.Parent.FullName + "\\States\\" + dir.Name);
            }
			lblRecStatus.Text = "Saved Recording";
		}

		private void btnSetRecSS_Click(object sender, EventArgs e)
		{
			game.recording.SS = new SaveState(game);
			lblRecStatus.Text = "SS Set";
		}

		private void btnPlayRec_Click(object sender, EventArgs e)
		{
			if (game.isPlayingRec)
			{
				game.StopPlayback();
				btnPlayRec.Text = "Play Rec";
				lblRecStatus.Text = "Recording Input";
			}
			else
			{
				game.PlayRecording(chkLoadSS.Checked);
				btnPlayRec.Text = "Stop Rec";
				lblRecStatus.Text = "Playing Recording";
			}
		}

		private void btnCurrentFrame_Click(object sender, EventArgs e)
		{
            int target = game.RecFrame;
            if (target > numFrame.Maximum) target = (int)numFrame.Maximum;
			numFrame.Value = target;
			numFrame_ValueChanged(numFrame, e);
			txtNoSelect.Select();
		}

		// Edits!
		private void btnDeleteFrame_Click(object sender, EventArgs e)
		{
            if (chkx10.Checked)
            {
                if (numFrame.Value >= cChannel.Frames - 10)
                {
                    MessageBox.Show("There are not 10 frames to delete.");
                    return;
                }
                cChannel._Keys.RemoveRange((int)numFrame.Value, 10);
            }
            else
                cChannel._Keys.RemoveAt((int)numFrame.Value);
            if (numFrame.Value >= cChannel.Frames)
                numFrame.Value = cChannel.Frames - 1;
            numFrame.Maximum = cChannel.Frames - 1;

			lblFrames.Text = (cChannel.Frames).ToString();
			numFrame_ValueChanged(numFrame, e);

			txtNoSelect.Select();
		}

		private void btnInsertFrame_Click(object sender, EventArgs e)
		{
			byte iVal = 0;
			if (chkCopy.Checked)
				iVal = cChannel._Keys[(int)numFrame.Value];
			cChannel._Keys.Insert((int)numFrame.Value, iVal);
			if (chkx10.Checked)
			{
				for (int i = 0; i < 9; i++)
					cChannel._Keys.Insert((int)numFrame.Value, iVal);
			}

			lblFrames.Text = (cChannel.Frames).ToString();
			numFrame_ValueChanged(numFrame, e);

			txtNoSelect.Select();
		}

		private void btnSetNextTo_Click(object sender, EventArgs e)
		{
			RecordedFrame sVal = cChannel.GetFrame((int)numFrame.Value);
			int from = (int)numFrame.Value;
			int to = (int)numFrame.Value;
			if (rdoNext.Checked)
				to += (int)numNextLast.Value + 1;
			else
				from -= (int)numNextLast.Value;

			// Check bounds
			if (from < 0) from = 0;
            if (to >= cChannel.Frames) to = cChannel.Frames - 1;

			for (int i = from; i < to; i++)
			{
				for (byte iB = 0; iB < 5; iB++)
				{
					if (chkInput[iB].ForeColor.R == 0)
					{
						RecordedFrame n = cChannel.GetFrame(i);
						n.SetButton(iB, sVal.GetButton(iB));
						cChannel.SetFrame(n, i);
					}
				}
			}

			txtNoSelect.Select();
		}

		private void chkInput_CheckedChanged(object sender, EventArgs e)
		{
			if (!manual)
				return;

			RecordedFrame f = cChannel.GetFrame((int)numFrame.Value);
			f.up = chkUp.Checked;
			f.down = chkDown.Checked;
			f.left = chkLeft.Checked;
			f.right = chkRight.Checked;
			f.space = chkSpace.Checked;

			cChannel._Keys[(int)numFrame.Value] = f.kValue;

			txtNoSelect.Select();
		}

		// Start/Stop the level
		private void btnPlay_Click(object sender, EventArgs e)
		{
			if (theMap.inLE)
			{
				theMap.exitLE();
				game.ReDraw();
				game.BeginRecording();
				DisplayInfos();
				btnPlay.Text = "Stop Level";
				game.paused = true;
            }
			else
			{
				theMap.enterLE();
				btnPlay.Text = "Play Level";
				game.paused = false;
                lblPause.Text = "Game Paused";
			}

			txtNoSelect.Select();
		}

		// Keyboard input
		private void Form1_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.Escape)
			{ txtNoSelect.Select(); e.SuppressKeyPress = true; }

			if (ActiveControl != pnlGame && ActiveControl != txtNoSelect)
				return;

			if (e.KeyCode == Keys.A)
			{
				game.paused = !game.paused;
				if (game.paused)
					lblPause.Text = "Game Paused";
				else
					lblPause.Text = "Game Running";
			}
			else if (e.KeyCode == Keys.S || e.KeyCode == Keys.D)
			{
				game.paused = true;
				game.FrameForward();
				lblPause.Text = "Frame Forward";
			}
			else if (e.KeyCode == Keys.Z)
			{
				theMap.CamX = (int)(You.X - theMap.Image.Width * 0.5);
				theMap.CamY = (int)(You.Y - theMap.Image.Height * 0.5);
			}
			else if (e.KeyCode == Keys.Tab)
			{
				game.targetFPS = 9999;
			}

			#region "SaveStates"
			else if (e.KeyCode >= Keys.D1 && e.KeyCode <= Keys.D9)
			{
				curSS = e.KeyCode - Keys.D0;
				lblSS.Text = "SS " + curSS + " selected";
			}
			else if (e.KeyCode == Keys.D0)
			{
				curSS = 0;
				lblSS.Text = "No SS selected";
			}
			if (curSS != 0)
			{
				if (e.KeyCode == Keys.U)
				{
					new SaveState(game).Save(SavestatesPath + "\\" + curSS);
					lblSS.Text = "Saved to slot " + curSS;
				}
				else if (e.KeyCode == Keys.P)
				{
					string path = SavestatesPath + "\\" + curSS;
					if (File.Exists(path))
					{
						new SaveState(SavestatesPath + "\\" + curSS).Use(game);
						numSelectedPlayer.Maximum = game.Players.Count;
						lblPlayers.Text = game.Players.Count.ToString();
						endOfFrame();
						lblSS.Text = "Loaded from slot " + curSS;
						game.DrawFrame();
						UpdateLevelInfo();
					}
					else
						lblSS.Text = "Slot " + curSS + " is empty.";
				}
			}
			#endregion

			#region "Player input"
			if (theMap.inLE && this.ActiveControl == txtNoSelect)
			{
				if (e.KeyCode == Keys.Left)
					theMap.CamX -= 30;
				else if (e.KeyCode == Keys.Right)
					theMap.CamX += 30;
				else if (e.KeyCode == Keys.Up)
					theMap.CamY -= 30;
				else if (e.KeyCode == Keys.Down)
					theMap.CamY += 30;
			}
			else
			{
				RecordedFrame i = new RecordedFrame(You.nextInput.kValue);
				if (e.KeyCode == Keys.Left)
					i.left = true;
				else if (e.KeyCode == Keys.Right)
					i.right = true;
				else if (e.KeyCode == Keys.Up)
					i.up = true;
				else if (e.KeyCode == Keys.Down)
					i.down = true;
				else if (e.KeyCode == Keys.Space)
					i.space = true;
				if (i.kValue != You.nextInput.kValue)
					game.SetInput(i, SelectedPlayer);
			}
			#endregion

		}
		private void Form1_KeyUp(object sender, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.Tab)
			{
				game.targetFPS = (double)fpsNum.Value;
			}
			#region "Player input"
			RecordedFrame i = new RecordedFrame(You.nextInput.kValue);
			if (e.KeyCode == Keys.Left)
				i.left = false;
			else if (e.KeyCode == Keys.Right)
				i.right = false;
			else if (e.KeyCode == Keys.Up)
				i.up = false;
			else if (e.KeyCode == Keys.Down)
				i.down = false;
			else if (e.KeyCode == Keys.Space)
				i.space = false;
			if (i.kValue != You.nextInput.kValue)
				game.SetInput(i, SelectedPlayer);
			#endregion
		}

		// Double-Clicking input checkboxes
		private void chkInput_MouseDown(object sender, MouseEventArgs e)
		{
			if (e.Clicks != 2)
				return;

			CheckBox s = sender as CheckBox;
			if (s.ForeColor.R == 0)
				s.ForeColor = Color.Red;
			else
				s.ForeColor = Color.Black;
		}

		private void numFrame_ValueChanged(object sender, EventArgs e)
		{
			manual = false;
			RecordedFrame frame = game.GetFrameInput(SelectedPlayer, (int)numFrame.Value);
			for (byte i = 0; i < chkInput.Length; i++)
				chkInput[i].Checked = frame.GetButton(i);
			manual = true;

			//txtNoSelect.Select();
		}

		private void chkPlayRec_CheckedChanged(object sender, EventArgs e)
		{

		}

		private void chkx10_CheckedChanged(object sender, EventArgs e)
		{
			if (chkx10.Checked)
				chkx10.ForeColor = Color.Red;
			else
				chkx10.ForeColor = Color.Black;
		}

		private void btnPartial_Click(object sender, EventArgs e)
		{
            using (var frm = new PartialsForm())
            {
                frm.mov = cChannel;
                frm.Show();
            }
		}

		// Setting player stats
		private void btnSetStats_Click(object sender, EventArgs e)
		{
            bool hats = (You.Accel == 1.86 || You.Speed != You.SpStat / 10.0 + 2);
			You.SpStat = (int)numSpeed.Value;
			You.AccStat = (int)numAccel.Value;
			You.JumpStat = (int)numJump.Value;
            if (hats)
                You.SetHats(); // sets stats then looks for cb/santa
            else
                You.SetStats();
			txtNoSelect.Select();
			game.DrawFrame();
		}

		private void numStat_ValueChanged(object sender, EventArgs e)
		{
			lblTotalStats.Text = "(" + (numSpeed.Value + numAccel.Value + numJump.Value) + ")";
		}

		private void tournamentBth_Click(object sender, EventArgs e)
		{
			numSpeed.Value = 65;
			numAccel.Value = 65;
			numJump.Value = 65;
			btnSetStats_Click(null, null);
		}

		// Video recording
		bool recordingVideo = false;
		string recordingPath;
		int recordingFrame = 0;
		private void button1_Click(object sender, EventArgs e)
		{
			if (recordingVideo)
			{
				button1.Text = "Record Vid";
				recordingVideo = false;
			}
			else
			{
                using (var dialog = new FolderBrowserDialog())
                {
                    dialog.RootFolder = Environment.SpecialFolder.MyVideos;
                    dialog.SelectedPath = Environment.SpecialFolder.MyVideos.ToString() + "\\ImgSeq";
                    dialog.ShowDialog();

                    recordingPath = dialog.SelectedPath;
                    recordingFrame = 0;
                    button1.Text = "Recording.";
                    recordingVideo = true;
                }
			}
		}

		// TxtNoSelect.Select();
		private void pnlGame_MouseDown(object sender, MouseEventArgs e)
		{
			txtNoSelect.Select();
            if (Control.ModifierKeys == Keys.Shift) 
            {
                You.X = theMap.CamX + e.X;
                You.Y = theMap.CamY + e.Y;
            }
            else if (Control.ModifierKeys == Keys.Control)
                You.course.MakeHat(theMap.CamX + e.X, theMap.CamY + e.Y, 3, Color.White, 1, 0);
            else if (tabControl1.SelectedTab == tabLE && theMap.inLE)
                LE_Action(e);
        }

        private void pnlGame_MouseMove(object sender, MouseEventArgs e)
        {
            if (tabControl1.SelectedTab == tabLE && theMap.inLE)
                LE_Action(e);
        }

        private void ScreenToWorld(ref int x, ref int y, int rot)
        {
            x += theMap.CamX;
            y += theMap.CamY;
            General.RotatePoint(ref x, ref y, rot);
            x = (int)Math.Floor(x / 30.0);
            y = (int)Math.Floor(y / 30.0);
        }

        private void LE_Action(MouseEventArgs e)
        {
            int x = e.X, y = e.Y;
            ScreenToWorld(ref x, ref y, -You.Rotation);

            if (e.Button == MouseButtons.Left && listViewBlocks.SelectedIndices.Count != 0)
            {
                var t = listViewBlocks.SelectedIndices[0];
                if (theMap.getBlock(x, y, 0).T == 99)
                    theMap.AddBlock(x, y, t);
                else if (theMap.getBlock(x, y, 0).T != t)
                {
                    theMap.DeleteBlock(x, y);
                    theMap.AddBlock(x, y, t);
                }
            }
            else if (e.Button == MouseButtons.Right)
            {
                if (theMap.getBlock(x, y, 0).T != 99)
                    theMap.DeleteBlock(x, y);
            }
        }

        // Hat sets
        private void chkHats_CheckedChanged(object sender, EventArgs e)
		{
			if (!manual)
				return;
			CheckBox s = sender as CheckBox;
			if (s.Checked)
			{
				You.GiveHat(new Hat((int)s.Tag));
			}
			else
			{
				for (int i = 0; i < You.Hats.Length; i++)
				{
					if (You.Hats[i].ID == (int)s.Tag)
					{
						for (int i2 = i + 1; i2 < You.Hats.Length; i2++)
							You.Hats[i2 - 1] = You.Hats[i2];
						Array.Resize(ref You.Hats, You.Hats.Length - 1);
						break;
					}
				}
				You.SetHatPowers();
			}

			txtNoSelect.Select();
		}

		// Item availability
		private void chkItem_CheckedChanged(object sender, EventArgs e)
		{
			if (!manual)
				return;

			// Available Items
			Array.Resize(ref theMap.avItems, 9);
			int IID = 0;
			for (int i = 0; i < 9; i++)
			{
				if (chkItems[i].Checked)
				{
					theMap.avItems[IID] = i + 1;
					IID += 1;
				}
			}
			Array.Resize(ref theMap.avItems, IID);

		}

        // Player management
        private int SelectedPlayer => (int)numSelectedPlayer.Value - 1;

        private void numSelectedPlayer_ValueChanged(object sender, EventArgs e)
		{
			manual = false;
			chkCowboy.Checked = You.CowboyHat;
			chkCrown.Checked = You.CrownHat;
			chkProp.Checked = You.PropellerHat;
			chkSanta.Checked = You.SantaHat;
			chkTop.Checked = You.TopHat;
			chkJump.Checked = You.JumpHat;

			lblFrames.Text = cChannel.Frames.ToString();
			numFrame.Maximum = cChannel.Frames;

			DisplayInfos();

			theMap.FollowChar = You;
			theMap.MainChar = You;

			game.currentChannel = SelectedPlayer;
			game.DrawFrame();
		}

		private void btnAddPlayer_Click(object sender, EventArgs e)
		{
			AddPlayer();
		}

		private void AddPlayer()
		{
			game.AddPlayer();
			game.Players.Last().Name = "Player " + game.Players.Count;
			numSelectedPlayer.Maximum = game.Players.Count;
			lblPlayers.Text = "/ " + game.Players.Count;

			game.recording.channels.Add(new RecordedChannel());
		}

		private void ghostBtn_Click(object sender, EventArgs e)
		{
            using (var dialog = new OpenFileDialog())
            {
                dialog.InitialDirectory = RecordingsPath;

                if (dialog.ShowDialog() == DialogResult.Cancel || dialog.FileName == "")
                    return;

                Recording rec = new Recording(dialog.FileName);
                string SSPath = Directory.GetParent(dialog.FileName).FullName + "\\States\\" + dialog.SafeFileName;
                if (File.Exists(SSPath))
                    rec.SS = new SaveState(SSPath);
                else
                {
                    MessageBox.Show("Ghost recordings must have a savestate.");
                    return;
                }

                game.AddGhost(rec);
                lblRecStatus.Text = "Loaded Ghost: " + dialog.SafeFileName;
            }
		}

		private void removeGhostsBtn_Click(object sender, EventArgs e)
		{
			game.ClearGhosts();
		}

		// Online level loading/searching
		private class LevelInfo
		{
			public string Title;
			public string User;
			public string Note;
			public bool HasPassword;
			public int Version;
			public int ID;
			public double Rating;
			public int PlayCount;
			public int MinRank;
			public string Type;
		}

		private List<LevelInfo> Levels = new List<LevelInfo>();

		private void searchBtn_Click(object sender, EventArgs e)
		{
			string SD = "search_str=" + searchBox.Text + "&mode=" + (modeBox.Text == "Level Title" ? "title" : "user") +
				"&order=" + orderBox.Text.ToLower() + "&dir=" + (ascBox.Checked ? "asc" : "desc") +
				"&page=" + pageNum.Value;
            string Lnk = "http://pr2hub.com/search_levels.php?";
			string Lvls = PostLoad(Lnk, SD);
			DisplayLevels(Lvls);
		}

		private void myLevelsButton_Click(object sender, EventArgs e)
		{
			string Lnk = "http://pr2hub.com/get_levels.php?random_num=0.1932&token=" + General.Settings.Tokens[General.Settings.SelectedUser] + "&count=9999";
			string Lvls = LoadURL(Lnk);
			if (Lvls.StartsWith("error"))
			{
				noteBox.Text = Lvls;
			}
			else
				DisplayLevels(Lvls);
		}

		private string LoadURL(string Link)
		{
			StreamReader inStream;
			WebRequest webRequest;
			WebResponse webresponse;
			webRequest = WebRequest.Create(Link);
			webresponse = webRequest.GetResponse();
			inStream = new StreamReader(webresponse.GetResponseStream());
			return inStream.ReadToEnd();
		}

		private void DisplayLevels(string str)
		{
			Levels.Clear();
			int i = -1;
            while (str.Contains("=") && !str.StartsWith("hash="))
            {
                string param = str.Substring(0, str.IndexOf("="));
                string value = str.Substring(param.Length + 1, str.IndexOf("&") - param.Length - 1);
                str = str.Substring(param.Length + value.Length + 2); // one for the =, one for the &
                if (param.EndsWith((i + 1).ToString()))
                {
                    i++;
                    Levels.Add(new LevelInfo());
                }
                param = param.Substring(0, param.Length - i.ToString().Length);
                switch (param)
                {
                    case "levelID":
                        Levels[i].ID = int.Parse(value);
                        break;
                    case "version":
                        Levels[i].Version = int.Parse(value);
                        break;
                    case "title":
                        Levels[i].Title = WebUtility.UrlDecode(value);
                        break;
                    case "rating":
                        Levels[i].Rating = double.Parse(value.Replace(".", ","), System.Globalization.NumberStyles.Any);
                        break;
                    case "playCount":
                        Levels[i].PlayCount = int.Parse(value);
                        break;
                    case "minLevel":
                        Levels[i].MinRank = int.Parse(value);
                        break;
                    case "userName":
                        Levels[i].User = WebUtility.UrlDecode(value);
                        break;
                    case "pass":
                        Levels[i].HasPassword = value != "";
                        break;
                    case "type":
                        Levels[i].Type = value;
                        break;
                    case "note":
                        Levels[i].Note = WebUtility.UrlDecode(value);
                        break;
                }
            }

            // Display them
            levelsList.Items.Clear();
			if (Levels.Count == 0)
			{
				levelsList.Items.Add("NO LEVELS");
				levelsList.Items.Add("if loading your levels, verify your token");
				return;
			}
			for (i = 0; i < Levels.Count; i++)
			{
				levelsList.Items.Add(Levels[i].Title);
			}
		}

		private void levelsList_SelectedIndexChanged(object sender, EventArgs e)
		{
			if (levelsList.SelectedIndex == -1)
				return;

			int index = levelsList.SelectedIndex;
			userLbl.Text = "Username: " + Levels[index].User;
			noteBox.Text = Levels[index].Note;
			typeLbl.Text = "Type: " + Levels[index].Type;
			versionLbl.Text = "Version: " + Levels[index].Version;
			ratingLbl.Text = "Rating: " + Levels[index].Rating;
			playsLbl.Text = "Plays: " + Levels[index].PlayCount;
			minRankLbl.Text = "Min Rank: " + Levels[index].MinRank;
			levelIDLbl.Text = "Level ID: " + Levels[index].ID;
			hasPassLbl.Visible = Levels[index].HasPassword;

			loadLevelBtn.Text = "Load Level";
		}

		string data;
		private void loadLevelBtn_Click(object sender, EventArgs e)
		{
			if (levelsList.SelectedIndex == -1)
				return;

			if (loadLevelBtn.Text.StartsWith("S"))
			{
                using (var dialog = new SaveFileDialog())
                {
                    dialog.InitialDirectory = LevelsPath;
                    dialog.Filter = "Text Files (*.txt)|*.txt";

                    if (dialog.ShowDialog() == DialogResult.Cancel || dialog.FileName == "")
                        return;

                    File.WriteAllText(dialog.FileName, data);
                }
            }
			else
			{
				theMap.enterLE(); // Idk what happens if you try to load a level mid-game
				data = LoadURL("http://pr2hub.com/levels/" + Levels[levelsList.SelectedIndex].ID + ".txt"
					+ "?version=" + Levels[levelsList.SelectedIndex].Version);
				//data = LoadURL("http://pr2hub.com/levels/" + 6413759 + ".txt");
				theMap.LoadLevel(data);

				UpdateLevelInfo();

				game.paused = false;
				loadLevelBtn.Text = "Save Level";
			}
		}

		private void tokenBtn_Click(object sender, EventArgs e)
		{
			TokenManagerForm form = new TokenManagerForm();
			form.ShowDialog();

			if (General.Settings.SelectedUser == -1)
				You.Name = "Player";
			else
			{
				You.Name = General.Settings.Users[General.Settings.SelectedUser];
				theMap.userName = You.Name;
				username = You.Name;
				login_token = General.Settings.Tokens[General.Settings.SelectedUser];
			}
		}

		private void fpsNum_ValueChanged(object sender, EventArgs e)
		{
			game.targetFPS = (double)fpsNum.Value;
		}

		// NO SELECTIONS
		private void Random_Events(object sender, EventArgs e)
		{
			txtNoSelect.Select();
		}

		private void txtNoSelect_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
		{
			if (e.KeyCode == Keys.Tab)
			{
				e.IsInputKey = true;
			}
		}

		private void spliceBtn_Click(object sender, EventArgs e)
		{
			game.paused = true;

			OpenFileDialog dialog = new OpenFileDialog();
			dialog.InitialDirectory = RecordingsPath;
			if (dialog.ShowDialog() == DialogResult.Cancel || dialog.FileName == "")
				return;
			Recording r1 = new Recording(dialog.FileName);
			string SSPath = RecordingsPath + "\\States\\" + dialog.SafeFileName;
			r1.SS = new SaveState(SSPath);
			if (dialog.ShowDialog() == DialogResult.Cancel || dialog.FileName == "")
				return;
			Recording r2 = new Recording(dialog.FileName);
			SSPath = RecordingsPath + "\\States\\" + dialog.SafeFileName;
			r2.SS = new SaveState(SSPath);

			RunSplicer splicer = new RunSplicer();
			splicer.Splice(r1, r2);
		}

		private void uploadBtn_Click(object sender, EventArgs e)
		{
			MessageBox.Show(UploadLevel());
		}

		#region "Map Saving"
		public string login_token = "";
		public string username = "";

		public string GetSaveData()
		{
			string ret = theMap.GetUploadData(true);
			return ret;
		}

		private string UploadHash(string title, string name, string data)
		{
			string stringToHash = title + username.ToLower() + data + "84ge5tnr";
			byte[] bytesToHash = Encoding.UTF8.GetBytes(stringToHash);
			byte[] bytesHashed = (new MD5CryptoServiceProvider()).ComputeHash(bytesToHash);
			return BitConverter.ToString(bytesHashed).Replace("-", "").ToLower();
		}

		private string PassHash(string pass)
		{
			string stringToHash = pass + "WGZSL3JWcUE9L3Q4YipZIQ==";
			byte[] bytesToHash = Encoding.UTF8.GetBytes(stringToHash);
			byte[] bytesHashed = (new MD5CryptoServiceProvider()).ComputeHash(bytesToHash);
			return BitConverter.ToString(bytesHashed).Replace("-", "").ToLower();
		}
		// Save to pr2hub
		public string UploadLevel()
		{
			string LData = GetSaveData() + "&token=" + login_token;
			return PostLoad("http://pr2hub.com/upload_level.php", LData);
		}

		private string PostLoad(string Link, string postData)
		{
			// Create a request using a URL that can receive a post. 
			WebRequest request = WebRequest.Create(Link);
			// Set the Method property of the request to POST.
			request.Method = "POST";
			// Create POST data and convert it to a byte array.
			byte[] byteArray = Encoding.UTF8.GetBytes(postData);
			// Set the ContentType property of the WebRequest.
			request.ContentType = "application/x-www-form-urlencoded";
			// Set the ContentLength property of the WebRequest.
			request.ContentLength = byteArray.Length;
			// Get the request stream.
			Stream dataStream = request.GetRequestStream();
			// Write the data to the request stream.
			dataStream.Write(byteArray, 0, byteArray.Length);
			// Close the Stream object.
			dataStream.Close();
			// Get the response.
			WebResponse response = null;
			try
			{
				response = request.GetResponse();
			}
			catch (Exception ex)
			{
				if (ex is Exception)
					ex = null;
			}
			// Get the stream containing content returned by the server.
			dataStream = response.GetResponseStream();
			// Open the stream using a StreamReader for easy access.
			StreamReader reader = new StreamReader(dataStream);
			// Read the content.
			string responseFromServer = reader.ReadToEnd();
			// Clean up the streams.
			reader.Close();
			dataStream.Close();
			response.Close();
			return responseFromServer;
		}
		// Save to file
		public void SaveLevel(string path)
		{
			string LData = GetSaveData();
			File.WriteAllText(path, LData);
		}
		#endregion

		private void Form1_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
		{
			if (ActiveControl == txtNoSelect)
				e.IsInputKey = true;
		}

		private void txtNote_TextChanged(object sender, EventArgs e)
		{
			theMap.note = txtNote.Text;
		}

		private void txtTitle_TextChanged(object sender, EventArgs e)
		{
			theMap.Title = txtTitle.Text;
		}

		private void txtCredits_TextChanged(object sender, EventArgs e)
		{
			theMap.credits = txtCredits.Text;
		}

		private void numGravity_ValueChanged(object sender, EventArgs e)
		{
			theMap.Gravity = (double)numGravity.Value;
		}

		private void cmbMusic_SelectedIndexChanged(object sender, EventArgs e)
		{
			theMap.song = cmbMusic.SelectedIndex;
		}

		private void numMinLevel_ValueChanged(object sender, EventArgs e)
		{
			theMap.min_level = (sbyte)numMinLevel.Value;
		}

		private void cmbGameMode_SelectedIndexChanged(object sender, EventArgs e)
		{
			theMap.gameMode = cmbGameMode.Text.ToLower();
		}

		private void numCowboyChance_ValueChanged(object sender, EventArgs e)
		{
			theMap.cowboyChance = (int)numCowboyChance.Value;
		}

		private void chkLive_CheckStateChanged(object sender, EventArgs e)
		{
			if (chkLive.CheckState == CheckState.Indeterminate)
			{
				theMap.live = 0;
				theMap.hasPass = true;
				theMap.password = "";
				theMap.passImpossible = false;
			}
			else if (chkLive.CheckState == CheckState.Checked)
			{
				theMap.live = 1;
				theMap.hasPass = false;
			}
			else
			{
				theMap.live = 0;
				theMap.hasPass = false;
			}
		}

        private void levelsList_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (levelsList.SelectedIndex == -1)
                return;

            loadLevelBtn_Click(null, null);
        }

        private void linkLabelToggleItems_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            bool hasChecked = false;
            foreach (var chk in chkItems)
                if (chk.Checked)
                    hasChecked = true;

            foreach (var chk in chkItems)
                chk.Checked = !hasChecked;
        }

        private void buttonSaveToFile_Click(object sender, EventArgs e)
        {
            using (var sfd = new SaveFileDialog()) {
                sfd.InitialDirectory = Directory.GetCurrentDirectory();
                sfd.Filter = "Text files (*.txt)|*.txt";
                sfd.DefaultExt = ".txt";
                if (sfd.ShowDialog(this) == DialogResult.OK) {
                    File.WriteAllText(sfd.FileName, theMap.GetUploadData(false));
                }
            }
        }
    }
}
