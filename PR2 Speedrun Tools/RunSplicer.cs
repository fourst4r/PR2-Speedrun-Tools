using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PR2_Speedrun_Tools
{
	class RunSplicer
	{
		public Recording Splice(Recording r1, Recording r2)
		{
			Game_ART game1 = new Game_ART(500, 400);
			Game_ART game2 = new Game_ART(1, 1);
			Game_ART gameSpliced = new Game_ART(504, 404);

			game1.paused = true;
			game2.paused = true;
			gameSpliced.paused = true;

			game1.recording = r1;
			game2.recording = r2;

			gameSpliced.recording = game1.recording;
			game1.PlayRecording();
			game2.PlayRecording();
			gameSpliced.PlayRecording();

			if (!Match(game1, game2))
			{
				MessageBox.Show("Initial states do not match.");
				return null;
			}

			do
			{
				game1.FrameForward();
				game2.FrameForward();
				gameSpliced.FrameForward();
			} while (game1.RecFrame < 55);

			// Splice time
			int recordingFrame = 0;
			string recordingPath = "C:\\Users\\Tommy\\Videos\\ImgSeq\\SpliceTest";
			do
			{
				AdvanceSplice(game1, game2, gameSpliced);
				recordingFrame++;
				game1.img.Bit.Save(recordingPath + "\\img" + recordingFrame.ToString().PadLeft(5, '0') + ".png");
				System.Diagnostics.Debug.Print("Splice progress: " + game1.map.Frames);
			} while (game1.map.Frames < game1.recording.channels[0].Frames);

			return gameSpliced.recording;
		}

		private bool Match(Game_ART g1, Game_ART g2)
		{
			int f = g2.map.Frames;
			g2.map.Frames = g1.map.Frames;
			bool ret = true;
			string[] ss1 = g1.GetSS();
			string[] ss2 = g2.GetSS();
			for (int i = 0; i < ss2.Length; i++)
			{
				if (ss1[i] != ss2[i])
				{
					ret = false;
					break;
				}
			}
			g2.map.Frames = f;
			return ret;
		}
		private bool Match(Game_ART g1, string[] ss2, int frame)
		{
			int f = g1.map.Frames;
			g1.map.Frames = frame;
			bool ret = true;
			string[] ss1 = g1.GetSS();
			for (int i = ss2.Length - 1; i >= 0; i--)
			{
				if (ss1[i] != ss2[i])
				{
					ret = false;
					break;
				}
			}
			g1.map.Frames = f;
			return ret;
		}

		private void AdvanceSplice(Game_ART g1, Game_ART g2, Game_ART gS)
		{
			const int maxFrames = 5;
			string[] ss1 = g1.GetSS();
			string[] ss2 = g2.GetSS();

			// Already match, input match?
			if (Match(g1, ss2, g2.map.Frames))
			{
				g1.FrameForward();
				g2.FrameForward();
				if (Match(g1, g2))
					return;
				g1.UseSS(ss1);
				g2.UseSS(ss2);
			}

			for (int i = 0; i < maxFrames || g1.map.Frames > g1.recording.channels[0].Frames; i++)
			{
				g1.FrameForward();
				if (Match(g1, ss2, g2.map.Frames))
				{
					g1.recording.DeleteFrames(g2.map.Frames, i + 1);
					g1.UseSS(ss1);
					gS.recording = g1.recording;
					return;
				}
			}
			g1.UseSS(ss1);

			for (int i = 0; i < maxFrames || g2.map.Frames > g2.recording.channels[0].Frames; i++)
			{
				g2.FrameForward();
				if (Match(g2, ss1, g1.map.Frames))
				{
					g2.recording.DeleteFrames(g1.map.Frames, i + 1);
					g2.UseSS(ss2);
					gS.recording = g2.recording;
					return;
				}
			}
			g2.UseSS(ss2);

			g1.FrameForward();
			g2.FrameForward();
		}
	}
}
