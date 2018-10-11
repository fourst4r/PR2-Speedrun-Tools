using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace PR2_Speedrun_Tools
{
	// extensions
	static class Extensions
	{
		public static double ratio(this Point p)
		{
			return (double)p.X / p.Y;
		}
	}

	class LevelCompressor
	{
		private static int toInt32(string str)
		{
			int ret = 0;
			if (str[0] == '-')
			{
				for (int i = 1; i < str.Length; i++)
					ret = 10 * ret + str[i] - 48;

				ret = -ret;
			}
			else
			{
				for (int i = 0; i < str.Length; i++)
					ret = 10 * ret + str[i] - 48;
			}

			//if (ret != int.Parse(str))
			//	ret = 0;

			return ret;
		}

		// Fixes things like ;7;0;8;0;7;7;8;8;2;3;4;6
		public static string CompressArt(string aData)
		{
			string[] DrawSections = aData.Split(',');
			List<Point> DrawParts = new List<Point>();
			StringBuilder str = new StringBuilder();
			for (int i = 0; i < DrawSections.Length; i++)
			{
				// Only use parts that are draw
				if (DrawSections[i].Length > 0 && DrawSections[i][0] != 'd')
					continue;
				// Set up DrawParts
				string[] dp = DrawSections[i].Split(';');
				if (dp.Length <= 2) // If length is only 2 there is no movement, only a dot.
					continue;

				DrawParts.Clear();
				for (int di = 2; di < dp.Length; di += 2)
					DrawParts.Add(new Point(LevelCompressor.toInt32(dp[di]), LevelCompressor.toInt32(dp[di + 1])));

				// Find matches
				for (int iP = 0; iP < DrawParts.Count - 1; iP += 1)
				{
					bool OrM = false;
					do
					{
						OrM = false;
						// Check if Y's are 0, so we don't get div by 0.
						if (DrawParts[iP].Y == 0)
							OrM = DrawParts[iP + 1].Y == 0;
						// Or if X-to-Y ration is same on both
						else if (DrawParts[iP + 1].Y != 0 && DrawParts[iP].ratio() == DrawParts[iP + 1].ratio())
							OrM = true;
						// If ratio matched, join
						if (OrM)
						{
							DrawParts[iP] = new Point(DrawParts[iP].X + DrawParts[iP + 1].X,
								DrawParts[iP].Y + DrawParts[iP + 1].Y);
							// Delete matched point
							DrawParts.RemoveAt(iP + 1);
						}
						else // No match, break
							break;
					} while (iP < DrawParts.Count - 1);
				}

				// Put it back into a string.
				//str = new StringBuilder(dp[0] + ";" + dp[1]);
				str.Clear();
				str.Append(dp[0] + ";" + dp[1]);
				for (int iP2 = 0; iP2 < DrawParts.Count; iP2++)
					str.Append(";" + DrawParts[iP2].X + ";" + DrawParts[iP2].Y);
				DrawSections[i] = str.ToString();
			}
			// String it all back together.
			return string.Join(",", DrawSections);
		}

		public static string CompressBlocks(string str)
		{
			List<Block> blocks = getBlocks(str);

			blocks.Sort(blockCompare);

			StringBuilder retStr = new StringBuilder();
			List<Block> skipped = new List<Block>();
			Block last = new Block();
			last.mapID = -1;
			for (int iT = 0; iT < BlockID.Egg; iT++)
			{
				bool first = true;
				for (int i = 0; i < blocks.Count; i++)
				{
					if (blocks[i].T != iT)
						continue;
					else
					{
						int b = 1;
						bool skip = false;
						while (i - b >= 0 && blocks[i - b].X == blocks[i].X && blocks[i - b].Y == blocks[i].Y)
						{
							if (blocks[i - b].mapID > blocks[i].mapID)
							{
								skip = true;
								break;
							}
						}
						if (skip)
						{
							skipped.Add(blocks[i]);
							continue;
						}
					}

					string pX = (blocks[i].X - last.X).ToString();
					if (pX == "0") pX = "";
					string pY = (blocks[i].Y - last.Y).ToString();
					if (pY == "0") pY = "";
					string add = pX + ";" + pY;
					if (first)
					{
						add += ";" + blocks[i].T.ToString();
						first = false;
					}
					else if (blocks[i].Y - last.Y == 0)
					{
						if (blocks[i].X - last.X == 0)
							add = "";
						else
							add = pX;
					}

					retStr.Append(add + ",");
					blocks[i].mapID = 0;
					last.X = blocks[i].X;
					last.Y = blocks[i].Y;
				}
			}

			retStr.Remove(retStr.Length - 1, 1);
			return retStr.ToString();
		}
		static int m3 = 3;
		private static List<Block> getBlocks(string str)
		{
			string[] blockStr = str.Split(',');
			List<Block> blocks = new List<Block>();
			Block last = new Block();
			last.mapID = -1;

			if (m3 == 3)
			{
				#region "m3"
				for (int i = 0; i < blockStr.Length; i++)
				{
					string[] pos = blockStr[i].Split(';');
					int X = LevelCompressor.toInt32(pos[0]);
					int Y = 0;
					if (pos.Length > 1) Y = LevelCompressor.toInt32(pos[1]);
					int T = -1;
					if (pos.Length > 2) T = LevelCompressor.toInt32(pos[2]);

					Block newB = new Block();
					newB.X = X + last.X;
					newB.Y = Y + last.Y;
					newB.T = (T == -1) ? last.T : T;
					newB.mapID = last.mapID + 1;
					blocks.Add(newB);

					last = newB;
				}
				#endregion
			}
			else if (m3 == 2)
			{
				#region "m2"
				for (int i = 0; i < blockStr.Length; i++)
				{
					string[] pos = blockStr[i].Split(';');
					int X = LevelCompressor.toInt32(pos[0]);
					int Y = 0;
					if (pos.Length > 1) Y = LevelCompressor.toInt32(pos[1]);
					int T = -1;
					if (pos.Length > 2) T = LevelCompressor.toInt32(pos[2]);

					Block newB = new Block();
					newB.X = (X / 30) + last.X;
					newB.Y = (Y / 30) + last.Y;
					newB.T = (T == -1) ? last.T : T;
					newB.mapID = last.mapID + 1;
					blocks.Add(newB);

					last = newB;
				}
				#endregion
			}
			else if (m3 == 1)
			{
				#region "m1"
				// 2706;1af4,b;1e;c4e,c;3c;c4e,d;5a;c4e,e;78;c4e,
				string[] parts = blockStr[0].Split(';');
				int baseX = Convert.ToInt32(parts[0], 16) / 30;
				int baseY = Convert.ToInt32(parts[1], 16) / 30;
				for (int i = 1; i < blockStr.Length; i++)
				{
					string[] pos = blockStr[i].Split(';');
					int X = Convert.ToInt32(pos[1], 16);
					int Y = Convert.ToInt32(pos[2], 16);
					int T = Convert.ToInt32(pos[0], 16);

					Block newB = new Block();
					newB.X = (X / 30) + baseX;
					newB.Y = (Y / 30) + baseY;
					newB.T = T;
					newB.mapID = last.mapID + 1;
					blocks.Add(newB);

					last = newB;
				}
				#endregion
			}
			else if (m3 == 0)
			{
				#region "o"
				for (int i = 0; i < blockStr.Length; i++)
				{
					string[] pos = blockStr[i].Split(';');
					int X = LevelCompressor.toInt32(pos[1]);
					int Y = LevelCompressor.toInt32(pos[2]);
					int T = LevelCompressor.toInt32(pos[0].Substring(1));

					Block newB = new Block();
					newB.X = X / 30;
					newB.Y = Y / 30;
					newB.T = (T == -1) ? last.T : T;
					newB.mapID = last.mapID + 1;
					blocks.Add(newB);

					last = newB;
				}
				#endregion
			}

			return blocks;
		}
		private static int blockCompare(Block b1, Block b2)
		{
			int Xcompare = b1.X.CompareTo(b2.X);
			if (Xcompare == 0)
				return b1.Y.CompareTo(b2.Y);
			return Xcompare;
		}

		public static string CompressLevel(string str)
		{
			string[] parts = str.Split('`');
			if (parts[0][0] == 'm')
				LevelCompressor.m3 = int.Parse(parts[0].Substring(1));
			else
			{
				LevelCompressor.m3 = 0;
				Array.Resize(ref parts, parts.Length + 1);
				Array.Copy(parts, 0, parts, 1, parts.Length - 1);
			}
			parts[0] = "m3";
			int[] ArtIDs = { 6, 7, 8, 12, 13 };
			if (parts.Length < 11)
				ArtIDs = new int[] { 6, 7, 8 };

			parts[2] = CompressBlocks(parts[2]);

			for (int i = 0; i < ArtIDs.Length; i++)
				parts[ArtIDs[i]] = LevelCompressor.CompressArt(parts[ArtIDs[i]]);

			return string.Join("`", parts);
		}
	}
}
