using PR2_Speedrun_Tools.Properties;
using System;
using System.Drawing;
using System.Text;
using System.Security.Cryptography;
using System.Media;

namespace PR2_Speedrun_Tools
{
	public static class General
	{
		// Some settings stuffs
		public static bool HQ = true;
		private static bool soundOn = false;

		public static Form1 FormRef;
		public static Settings Settings = Settings.Load();

		#region "Bitsmaps"
		public static Bitsmap[] blockImgs = new Bitsmap[] { new Bitsmap(Resources.BB1), new Bitsmap(Resources.BB2)
        , new Bitsmap(Resources.BB3), new Bitsmap(Resources.BB4), new Bitsmap(Resources.Brick)
        , new Bitsmap(Resources.Down_Arrow), new Bitsmap(Resources.Up_Arrow), new Bitsmap(Resources.Left_Arrow)
        , new Bitsmap(Resources.Right_Arrow), new Bitsmap(Resources.Mine), new Bitsmap(Resources.Item)
        , new Bitsmap(Resources.P1_Start), new Bitsmap(Resources.P2_Start), new Bitsmap(Resources.P3_Start)
        , new Bitsmap(Resources.P4_Start), new Bitsmap(Resources.Ice), new Bitsmap(Resources.Finish)
        , new Bitsmap(Resources.Crumble), new Bitsmap(Resources.Vanish), new Bitsmap(Resources.Move)
        , new Bitsmap(Resources.Water), new Bitsmap(Resources.RotateR), new Bitsmap(Resources.RotateL)
        , new Bitsmap(Resources.Push), new Bitsmap(Resources.Safety), new Bitsmap(Resources.Inf_Item)
        , new Bitsmap(Resources.Happy), new Bitsmap(Resources.Sad), new Bitsmap(Resources.Heart)
        , new Bitsmap(Resources.Time), new Bitsmap(Resources.Egg)};
		public static Bitsmap GrayBlock = new Bitsmap(Resources.GrayedBlockCover);
		public static Color cGrayBlock = Color.FromArgb(80, 0, 0, 0);

		public static Bitsmap[] BlockI = blockImgs;

		public static Bitsmap[] ItemB = new Bitsmap[] { null, new Bitsmap(Resources.Lasergun), new Bitsmap(Resources.Mine1)
        , new Bitsmap(Resources.Lightning), new Bitsmap(Resources.Item_Teleport), new Bitsmap(Resources.Item_Superjump)
        , new Bitsmap(Resources.Item_Jetpack), new Bitsmap(Resources.Item_Speedy), new Bitsmap(Resources.Sword), new Bitsmap(Resources.Ice)};
		public static Bitsmap laserPic = new Bitsmap(Resources.laser);
		public static Bitsmap slashPic = new Bitsmap(Resources.Slash);
		public static Bitsmap jetPic = new Bitsmap(Resources.Jet);
		public static Bitsmap sparklePic = new Bitsmap(Resources.Sparkle);

		public static Bitsmap[] Hatpic = new Bitsmap[] { new Bitsmap(20, 20), new Bitsmap(20, 20)
        , new Bitsmap(Resources.Exp), new Bitsmap(Resources.Kong), new Bitsmap(Resources.Propeller)
        , new Bitsmap(Resources.Hat_Cowboy), new Bitsmap(Resources.Crown), new Bitsmap(Resources.Hat_Santa)
        , new Bitsmap(Resources.Party), new Bitsmap(Resources.Hat_Top), new Bitsmap(Resources.JumpStart)
        , new Bitsmap(Resources.Moon), new Bitsmap(Resources.Thieft), new Bitsmap(Resources.Jigg)
        , new Bitsmap(Resources.Artifact)};

		public static Bitmap Feet = new Bitmap(Resources.Feet_Standard_S);
		public static Bitmap FeetC = new Bitmap(Resources.Feet_Standard_C);
		public static Bitmap Body = new Bitmap(Resources.Body_Standard_S);
		public static Bitmap BodyC = new Bitmap(Resources.Body_Standard_C);
		public static Bitmap Head = new Bitmap(Resources.Head_Standard_S);
		public static Bitmap HeadC = new Bitmap(Resources.Head_Standard_C);
		#endregion

		public static string FormatNumber(string val, int atLeast = 0)
		{
			string v = val.PadLeft(atLeast, '0');
			int i = v.Length;
			if (i < 4)
				return v;
			do
			{
				i -= 3;
				v = v.Insert(i, ",");
			} while (i > 2);

			return v;
		}

		public static string FormatNumber(int val, int atLeast = 0)
		{
			return FormatNumber(val.ToString(), atLeast);
		}

		public static string FormatNumber(double val, int numDecimal, int atLeast = 0)
		{
			string v = val.ToString();
			int d = v.IndexOf('.');
			if (d == -1) // No decimal
				d = v.Length;
			// Add/remove decimal if required
			if (numDecimal == 0)
				return FormatNumber(v.Substring(0, d));
			else if (d == v.Length)
				v += ".";

			if (d >= v.Length - numDecimal)
				v = v.PadRight(d + numDecimal + 1, '0');
			else
				v = v.Substring(0, d + 1 + numDecimal);

			return FormatNumber(v.Substring(0, d), atLeast) + v.Substring(d);
		}

		public static byte BoolAsByte(bool val)
		{
            return val ? (byte)255 : (byte)0;
        }

		public static void RotatePoint(ref int X, ref int Y, int RotVal)
		{
			// PR2 is backwards, methinks
			RotVal = -RotVal;
			int Temp = X;
			if (RotVal == 90)
			{
				X = Y;
				Y = -Temp;
			}
			else if (RotVal == 180 || RotVal == -180)
			{
				X = -X;
				Y = -Y;
			}
			else if (RotVal == -90)
			{
				X = -Y;
				Y = Temp;
			}
		}

		public static Random R = new Random();

		public static string ColorToHex(Color c)
		{
			return (Convert.ToString(c.R, 16).PadLeft(2, '0') + Convert.ToString(c.G, 16).PadLeft(2, '0')
				+ Convert.ToString(c.B, 16).PadLeft(2, '0')).ToLower();
		}

		public static string FramesToTime(int f)
		{
			if (f < 0)
				return "-" + FramesToTime(-f);
			double secs = f / 27.0;
			int mins = (int)Math.Floor(secs / 60);
			return mins + ":" + FormatNumber(secs - (mins * 60), 2, 2);
		}

		public static string GenerateHash(string stringToHash)
		{
			byte[] bytesToHash = Encoding.UTF8.GetBytes(stringToHash);
			byte[] bytesHashed = (new MD5CryptoServiceProvider()).ComputeHash(bytesToHash);
			return BitConverter.ToString(bytesHashed).Replace("-", "").ToLower();
		}

		private static SoundPlayer[] soundPlayer = { new SoundPlayer(), new SoundPlayer(), new SoundPlayer() };
		static private int spID = 0;
		public static string sound_path = "C:\\pr2source\\Sound2\\";
		public static void PlaySound(string fileName)
		{
			if (!soundOn)
				return;
			// soundPlayer.Stop();
			soundPlayer[spID].SoundLocation = fileName;
			soundPlayer[spID].Load();
			soundPlayer[spID].Play();
			spID = (spID + 1) % 3;
		}

		static public void SaveStringArray(string path, string[] sa)
		{
			byte[][] data = new byte[sa.Length][];
			int totalLen = 5;
			for (int i = 0; i < data.Length; i++)
			{
				if (sa[i] == null)
					continue;
				int len = sa[i].Length * 2 + 4;
				totalLen += len;
				data[i] = new byte[len];
				BitConverter.GetBytes(len).CopyTo(data[i], 0);
				System.Text.Encoding.Unicode.GetBytes(sa[i]).CopyTo(data[i], 4);
			}
			// Merge byte arrays
			byte[] saveData = new byte[totalLen];
			saveData[0] = 1; // Version number
			BitConverter.GetBytes(sa.Length).CopyTo(saveData, 1);
			int index = 5;
			for (int i = 0; i < data.Length; i++)
			{
				if (data[i] == null)
					continue;
				data[i].CopyTo(saveData, index);
				index += data[i].Length;
				data[i] = null;
			}

			System.IO.File.WriteAllBytes(path, saveData);
		}

		public static string[] LoadStringArray(string path)
		{
			byte[] loadData = System.IO.File.ReadAllBytes(path);
			int ver = 0;
			if (loadData[0] == 1)
				ver = 1;
			else if (loadData[0] != 0)
				return null; // Only v.0 - 1 supported.

			int count = BitConverter.ToInt32(loadData, 1);
			string[] sa = new string[count];
			int index = 5;
			for (int i = 0; i < count; i++)
			{
				byte[] data = new byte[BitConverter.ToInt32(loadData, index) - 4];
				index += 4;
				Array.Copy(loadData, index, data, 0, data.Length);
				index += data.Length;

				if (ver == 0)
					sa[i] = Encoding.UTF8.GetString(data);
				else if (ver == 1)
					sa[i] = Encoding.Unicode.GetString(data);
			}

			return sa;
		}

		static private RandomMachine rng = new RandomMachine(1);
		static public void ResetRNG()
		{
			rng = new RandomMachine(1);
		}

		static public int GetNextMove()
		{
			return rng.nextMax(4);
		}

		static public string getRNGss()
		{
			return rng.getRNGss();
		}

		static public void useRNGss(string str)
		{
			rng.useRNGss(str);
		}
	}

	public class RandomMachine
	{
		// Class taken from https://github.com/aurelien-defossez/la-poudre-blanche/blob/master/src/RandomMachine.as#L6

		// Fields
		private int _inext;
		private int _inextp;
		private const int MBIG = 0x7fffffff;
		private const int MSEED = 0x9a4ec86;
		private const int MZ = 0;
		private int _seed;
		private int[] _seedArray;
		public string getRNGss()
		{
			string[] arr = new string[_seedArray.Length];
			for (int i = 0; i < arr.Length; i++)
				arr[i] = _seedArray[i].ToString();

			return _inext + ";" + _inextp + ";" + string.Join(";", arr);
		}

		public void useRNGss(string str)
		{
			string[] arr = str.Split(';');
			_inext = int.Parse(arr[0]);
			_inextp = int.Parse(arr[1]);
			for (int i = 2; i < arr.Length; i++)
				_seedArray[i - 2] = int.Parse(arr[i]);
		}

		// Methods
		public RandomMachine(int seed)
		{
			_seed = seed;
			_seedArray = new int[0x38];
			int num2 = 0x9a4ec86 - Math.Abs(seed);
			_seedArray[0x37] = num2;
			int num3 = 1;
			for (int i = 1; i < 0x37; i++)
			{
				int index = (0x15 * i) % 0x37;
				_seedArray[index] = num3;
				num3 = num2 - num3;
				if (num3 < 0)
				{
					num3 += 0x7fffffff;
				}
				num2 = _seedArray[index];
			}
			for (int j = 1; j < 5; j++)
			{
				for (int k = 1; k < 0x38; k++) //-V3081
				{
					_seedArray[k] -= _seedArray[1 + ((k + 30) % 0x37)];
					if (_seedArray[k] < 0)
					{
						_seedArray[k] += 0x7fffffff;
					}
				}
			}
			_inext = 0;
			_inextp = 0x15;
			seed = 1;
		}

		public int getseed()
		{
			return _seed;
		}

		private double getSampleForLargeRange()
		{
			int num = internalSample();
			if ((internalSample() % 2) == 0)
			{
				num = -num;
			}
			double num2 = num;
			num2 += 2147483646.0;
			return num2 / 4294967293;
		}

		private int internalSample()
		{
			int inext = _inext;
			int inextp = _inextp;
			if (++inext >= 0x38)
			{
				inext = 1;
			}
			if (++inextp >= 0x38)
			{
				inextp = 1;
			}
			int num = _seedArray[inext] - _seedArray[inextp];
			if (num < 0)
			{
				num += 0x7fffffff;
			}
			_seedArray[inext] = num;
			_inext = inext;
			_inextp = inextp;
			return num;
		}

		public int nextInt()
		{	
			return internalSample();
		}

		public int nextMax(int maxValue)
		{
			if (maxValue < 0)
			{
				throw new Exception("Argument \"maxValue\" must be positive.");
			}
			return (int)(sample() * maxValue);
		}

		public int nextMinMax(int minValue, int maxValue)
		{
			if (minValue > maxValue)
			{
				throw new Exception("Argument \"minValue\" must be less than or equal to \"maxValue\".");
			}
			double num = maxValue - minValue;
			if (num <= 0x7fffffff)
			{
				return ((int)(sample() * num)) + minValue;
			}
			return ((int)(getSampleForLargeRange() * num)) + minValue;
		}

		//public void nextBytes(byte[] buffer, int length)
		//{
		//	if (buffer == null)
		//	{
		//		throw new Exception("Argument \"buffer\" cannot be null.");
		//	}
		//	for (int i = 0; i < length; i++)
		//	{
		//		buffer.writeByte(internalSample() % 0x100);
		//	}
		//}

		public double nextNumber()
		{
			return sample();
		}

		protected double sample()
		{
			return internalSample() * 4.6566128752457969E-10;
		}
	}
}