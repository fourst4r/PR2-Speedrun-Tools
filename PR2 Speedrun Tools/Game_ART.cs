using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Threading;
using System.Drawing;

namespace PR2_Speedrun_Tools
{
	// Contains all components of a PR2 Game
	public class Game_ART : IDisposable
	{
		#region "IDisposable Support"
		bool disposedValue = false;
		public void Dispose(bool disposing) { Dispose(); }
		public void Dispose()
		{
			disposedValue = true;
			img.Dispose(); img = null;
			map.Dispose(); map = null;
			Players.Clear(); Players = null;
			recording = null;
		}
		#endregion
		public Game_ART(int displayWidth = 540, int displayHeight = 405)
		{
			img = new Bitsmap(displayWidth, displayHeight);
			MG = Graphics.FromImage(img.Bit);

			map = new Map(null, displayWidth, displayHeight);
			Players = new List<LocalCharacter>();
			recording = new Recording();
			AddPlayer(true);

			// Set-up
			targetFPS = 27;
			if (map == null)
				map = new Map(null);

			StartGame();
		}
		// Start the auto-frame progression
		private void StartGame()
		{
			gameTimer = new System.Windows.Forms.Timer();
			gameTimer.Interval = 1;
			gameTimer.Tick += StartFrames;
			gameTimer.Enabled = true;
		}

		public delegate void void_method();
		public event void_method FinishDrawing;
		public event void_method beginFrame;
		public event void_method endFrame;
		private void InvokeEvent(void_method ev)
		{
			if (ev != null)
				ev.Invoke();
		}

		#region "Graphics"
		public void Resize(int displayWidth, int displayHeight)
		{
			img = new Bitsmap(displayWidth, displayHeight);
			MG = Graphics.FromImage(img.Bit);

			map.ResizeView(displayWidth, displayHeight);

			ReDraw();
		}

		public Bitsmap img;
		Graphics MG;
		public bool draw = true;
		public void DrawFrame()
		{
			map.Draw();
			// Ghosts
			if (Ghosts != null)
			{
				for (int i = 0; i < Ghosts.Count; i++)
					map.DrawCharacter(Ghosts[i]);
			}

			ReDraw();
		}
		public void ReDraw()
		{
			img.DrawImage(ref map.BlBit, 0, 0);
			DrawInput();

			InvokeEvent(FinishDrawing);
		}
		private Font timeFont = new Font("Courier New", 14);
		private void DrawInput()
		{
			// Keys
			int dX;
			int dY;
			dX = img.Width - 56;
			dY = img.Height - 42;
			img.FillRectangleA(Color.FromArgb(96, 255, 0, 0), dX - 2, dY - 2, 53, 39);

			if (currentInput.space)
				MG.DrawString("s", timeFont, Brushes.Green, dX, dY - 2);
			if (currentInput.up)
				MG.DrawString("^", timeFont, Brushes.Green, dX + 15, dY + 3);
			if (currentInput.left)
				MG.DrawString("<", timeFont, Brushes.Green, dX, dY + 16);
			if (currentInput.down)
				MG.DrawString("v", timeFont, Brushes.Green, dX + 14, dY + 14);
			if (currentInput.right)
				MG.DrawString(">", timeFont, Brushes.Green, dX + 28, dY + 16);
		}
		#endregion

		#region "Automatic frame progression"
		double _targetFPS;
		public double targetFPS
		{
			get => _targetFPS;
		    set
			{
				_targetFPS = value;
				TicksPerFrame = (long)(Stopwatch.Frequency / value);
			}
		}
		public double currentFPS;
		long TicksPerFrame;
		System.Windows.Forms.Timer gameTimer;
		private void StartFrames(object sender, EventArgs e)
		{
			gameTimer.Enabled = false;
			StartFrames();
		}
		private void StartFrames()
		{
			Stopwatch t = new Stopwatch();
			t.Start();
			do
			{
				if (t.ElapsedTicks >= TicksPerFrame)
				{
					// Time FPS
					double cTicks = (double)t.ElapsedTicks;
					t.Restart();
					if (!paused)
					{
						currentFPS += (1 / (cTicks / Stopwatch.Frequency)) * (1d / 3d);
						currentFPS *= 0.75;
					}

					// Actual frame progress
					goFrame();

					System.Windows.Forms.Application.DoEvents();
				}
				else if (t.ElapsedTicks < TicksPerFrame - (Stopwatch.Frequency / 50))
				{
					Thread.Yield();
					long time = TicksPerFrame - t.ElapsedTicks;
					time /= Stopwatch.Frequency / 1000;
					if (time > 1)
						Thread.Sleep((int)time - 1);
					System.Windows.Forms.Application.DoEvents();
				}
			} while (!disposedValue && !paused); // IT NEVAR ENDS!
		}
		#endregion
		private void goFrame()
		{
			InvokeEvent(beginFrame);
			// If first frame, set recording's SaveState
			// Recording/Playing
			if (playingRec)
				UseInputRec();
			else
				RecordInput();

			if (recording.SS == null && map.Frames == 0)
				recording.SS = new SaveState(map.GetSSData());

			map.goFrame();

			// Ghosts
			if (Ghosts != null)
			{
				for (int i = 0; i < Ghosts.Count; i++)
				{
					Ghosts[i].input = ghostRecs[i].channels[0].GetFrame(Ghosts[i].course.Frames);
					Ghosts[i].course.goFrame();
				}
			}

			// Draw
			if (draw)
				DrawFrame();

			InvokeEvent(endFrame);
		}

		private bool _Paused = false;
		public bool paused
		{
			get => _Paused;
		    set
			{
				if (value == _Paused)
					return;
				_Paused = value;
				if (!_Paused)
					StartGame();
			}
		}
		public void FrameForward()
		{
			if (!paused)
				return;

			long tpf = TicksPerFrame;
			TicksPerFrame = 0;
			StartFrames();
			TicksPerFrame = tpf;
		}


		private void RecordInput()
		{
			for (int i = 0; i < Players.Count; i++)
				recording.channels[i].SetFrame(Players[i].nextInput, RecFrame);
		}
		private void UseInputRec()
		{
			for (int i = 0; i < Players.Count; i++)
				SetInput(recording.channels[i].GetFrame(RecFrame), i);
		}

		public Map map;
		public List<LocalCharacter> Players;
		private List<LocalCharacter> Ghosts; private List<Recording> ghostRecs;
		public void AddPlayer(bool main = false)
		{
			LocalCharacter c = new LocalCharacter();
			c.course = map;
			Players.Add(c);
			map.AddCharacter(c);
			if (main)
				map.MainChar = c;

			recording.channels.Add(new RecordedChannel());
			recording.channels.Last().SetEndPoint(RecFrame);
		}
		public void AddGhost(Recording rec)
		{
			if (Ghosts == null)
			{
				Ghosts = new List<LocalCharacter>();
				ghostRecs = new List<Recording>();
			}

			Ghosts.Add(new LocalCharacter());
			Map m = new Map(null);
			m.AddCharacter(Ghosts.Last());
			Ghosts.Last().course = m;
			m.UseSSData(rec.SS.str);

			ghostRecs.Add(rec);
		}
		public void ClearGhosts()
		{
			Ghosts = null;
			ghostRecs = null;
		}

		#region "Input/Recording handling"
		public Recording recording;
		private bool playingRec;
		public bool isPlayingRec => playingRec;
	    private int _startRecAt = 0;
		public int RecFrame => map.Frames - _startRecAt;

	    public void BeginRecording()
		{
			playingRec = false;
			_startRecAt = map.Frames;
			recording = new Recording();
			for (int i = 0; i < Players.Count; i++)
				recording.channels.Add(new RecordedChannel());
		}

		public void PlayRecording(bool loadState = true)
		{
			playingRec = true;

			if (loadState)
				recording.SS.Use(this);
			_startRecAt = map.Frames;
		}

		public void StopPlayback()
		{
			playingRec = false;
		}

		public RecordedFrame GetCurrentFrameInput(int player)
		{
			return recording.channels[player].GetFrame(RecFrame);
		}

		internal RecordedFrame GetFrameInput(int player, int frame)
		{
			return recording.channels[player].GetFrame(frame);
		}

		public int currentChannel = 0;
		private RecordedFrame currentInput => Players[currentChannel].nextInput;

	    private RecordedFrame GetInput(int channel = -1)
		{
			if (channel == -1)
				channel = currentChannel;
			return Players[channel].nextInput;
		}
		public void SetInput(RecordedFrame input, int channel = -1)
		{
			if (channel == -1)
				channel = currentChannel;

			Players[channel].input = input;
			if (paused)
				ReDraw();
		}
		// Character's input should be obtained via a callback
		#endregion

		#region "SaveStates"
		public void SaveState(string path)
		{
			string[] SSData = map.GetSSData();
			System.IO.File.WriteAllLines(path, SSData);
		}
		public string[] GetSS()
		{
			List<string> ret = map.GetSSData().ToList();
			if (Ghosts != null)
			{
			    foreach (var t in Ghosts)
			    {
			        ret.Add("Ghost");
			        ret.AddRange(t.course.GetSSData());
			    }
			}
			return ret.ToArray();
		}
		public void LoadState(string path)
		{
			string[] SSData = System.IO.File.ReadAllLines(path);
			UseSS(SSData);
		}
		public void UseSS(string[] ss)
		{
			RecordedFrame currentInput = null;
			if (!playingRec)
				currentInput = GetInput();

			map.UseSSData(ss);

			Players.Clear();
			foreach (LocalCharacter c in map.Chars)
				Players.Add(c);

			if (currentInput != null)
				SetInput(currentInput);

			// Ghosts
			if (!ss.Contains("Ghost"))
			{
				Ghosts = null;
				return;
			}
			if (Ghosts == null)
				Ghosts = new List<LocalCharacter>();
			// TODO: Ghost recordings are not saved. They probably should be!

			List<string> s = ss.ToList();
			// Remove regular data
			int gIndex = s.IndexOf("Ghost");
			s.RemoveRange(0, gIndex);
			gIndex = 0;

			int i = 0;
			while (s.Count != 0)
			{
				int start = gIndex + 1;
				gIndex = s.IndexOf("Ghost", gIndex + 1);
				if (gIndex == -1)
					gIndex = s.Count;

				string[] gSS = new string[gIndex - start];
				s.CopyTo(start, gSS, 0, gSS.Length);
				s.RemoveRange(0, gSS.Length + 1);

				if (Ghosts.Count <= i)
				{
					Ghosts.Add(new LocalCharacter());
					Ghosts[i].course = new Map(null);
					Ghosts[i].course.AddCharacter(Ghosts[i]);
				}
				Ghosts[i].course.UseSSData(gSS);
				i++;
			};
		}
		#endregion

	}
}
