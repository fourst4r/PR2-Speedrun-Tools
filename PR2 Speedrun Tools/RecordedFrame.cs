using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PR2_Speedrun_Tools
{
	public class Recording
	{
		public Recording() { }

		public SaveState SS;

		public List<RecordedChannel> channels = new List<RecordedChannel>();

		public void Save(string path)
		{
			byte[][] channelData = new byte[channels.Count][];
			int totalLength = 0;
			for (int i = 0; i < channels.Count; i++)
			{
				channelData[i] = channels[i].GetSaveData();
				totalLength += channelData[i].Length + 4;
			}
			byte[] saveData = new byte[totalLength + 4];
			BitConverter.GetBytes(channels.Count).CopyTo(saveData, 0);
			int cIndex = 4;
			for (int i = 0; i < channels.Count; i++)
			{
				BitConverter.GetBytes(channelData[i].Length).CopyTo(saveData, cIndex);
				cIndex += 4;
				channelData[i].CopyTo(saveData, cIndex);
				cIndex += channelData[i].Length;
				channelData[i] = null;
			}

			System.IO.File.WriteAllBytes(path, saveData);
		}

		public Recording(string path) // Load
		{
			byte[] loadBytes = System.IO.File.ReadAllBytes(path);
			int channelCount = BitConverter.ToInt32(loadBytes, 0);
			int cIndex = 4;
			for (int i = 0; i < channelCount; i++)
			{
				byte[] cBytes = new byte[BitConverter.ToInt32(loadBytes, cIndex)];
				cIndex += 4;
				Array.Copy(loadBytes, cIndex, cBytes, 0, cBytes.Length);
				cIndex += cBytes.Length;
				channels.Add(new RecordedChannel(cBytes));
			}
		}

		public static Recording LoadOld(string path)
		{
			Recording r = new Recording();
			r.channels.Add(new RecordedChannel(path));

			return r;
		}

		// Operate on all channels
		public void DeleteFrames(int start, int count)
		{
		    foreach (var t in channels)
		        t.DeleteFrames(start, count);
		}
	}

	public class RecordedChannel
	{
		public RecordedChannel() { }

	    public RecordedChannel(string path)
	    {
	        byte[] loadBytes = System.IO.File.ReadAllBytes(path);

	        int keysCount = BitConverter.ToInt32(loadBytes, 0);

	        for (int i = 0; i < keysCount; i++)
	            _Keys.Add(loadBytes[4 + i]);

	        Time = BitConverter.ToInt32(loadBytes, loadBytes.Length - 4);
	    }

	    public RecordedChannel(byte[] b)
	    {
	        int keysCount = BitConverter.ToInt32(b, 0);

	        for (int i = 0; i < keysCount; i++)
	            _Keys.Add(b[4 + i]);

	        Time = BitConverter.ToInt32(b, b.Length - 4);
	    }

        public List<byte> _Keys = new List<byte>();
		public int Time;

		public int Frames => _Keys.Count;

	    // Add a frame
		public void AddFrame(bool down, bool up, bool right, bool left, bool space, int item)
		{
            // Get byte for keys
            RecordedFrame nFrame = new RecordedFrame
            {
                down = down,
                up = up,
                right = right,
                left = left,
                space = space
            };

            _Keys.Add(nFrame.kValue);
		}

		public void AddFrame(RecordedFrame f)
		{
			_Keys.Add(f.kValue);
		}

		public void DeleteFrames(int start, int count)
		{
			_Keys.RemoveRange(start, count);
		}

		// Remove all frames at and past a certain point
		public void SetEndPoint(int frameID)
		{
			if (frameID > _Keys.Count)
			{
				do
				{
					_Keys.Add(0);
				} while (frameID > _Keys.Count);
			}
			else
				_Keys.RemoveRange(frameID, _Keys.Count - frameID);
		}

		// Reading a recording for playback
		public RecordedFrame GetFrame(int frameID)
		{
			RecordedFrame ret = new RecordedFrame();
			if (frameID < _Keys.Count)
				ret.kValue = _Keys[frameID];
			return ret;
		}
		public void SetFrame(RecordedFrame f, int fID)
		{
			if (_Keys.Count > fID)
				_Keys[fID] = f.kValue;
			else
				AddFrame(f);
		}

		// Save/Load
		public void Save(string path)
		{
			byte[] saveBytes = GetSaveData();
			System.IO.File.WriteAllBytes(path, saveBytes);
		}

		public byte[] GetSaveData()
		{
			// Get array of bytes : 4-byte length of keys, keys, 4-byte time
			byte[] saveBytes = new byte[_Keys.Count + 8];
			BitConverter.GetBytes(_Keys.Count).CopyTo(saveBytes, 0);
			_Keys.CopyTo(saveBytes, 4);
			BitConverter.GetBytes(Time).CopyTo(saveBytes, saveBytes.Length - 4);

			return saveBytes;
		}

		public static RecordedChannel LoadOld(string path)
		{
			RecordedChannel rec = new RecordedChannel();
			byte[] loadBytes = System.IO.File.ReadAllBytes(path);

			int keysCount = BitConverter.ToInt32(loadBytes, 0);

			for (int i = 1; i < keysCount; i++)
			{
				rec._Keys.Add(loadBytes[4 + i]);
				rec._Keys[i - 1] /= 8;
			}

			rec.Time = BitConverter.ToInt32(loadBytes, loadBytes.Length - 4);

			return rec;
		}
	}

	public class RecordedFrame
	{
		public RecordedFrame() { }
		public RecordedFrame(byte v) => kValue = v;

	    // Keys (each key has a bit)
		public const byte _downK = 16;
		public const byte _upK = 8;
		public const byte _rightK = 4;
		public const byte _leftK = 2;
		public const byte _spaceK = 1;

		byte[] Bits = new byte[] { 1, 2, 4, 8, 16, 32, 64, 128 };
		public void SetButton(byte BitID, bool OnOff)
		{
			byte bOnOff = 0;
			if (OnOff) { bOnOff = 255; }
			kValue = (byte)((Bits[BitID] & bOnOff) | (kValue & ~Bits[BitID]));
			// bit & bOnOff == the bit to be set, turned on/off as required
			// kValue & ~bit == kValue with the bit to be set turned off
		}
		public bool GetButton(byte BitID)
		{
			return kValue == (kValue | Bits[BitID]);
		}

		public byte kValue;

		public bool down
		{
			get => kValue == (kValue | _downK);
		    set
			{
				byte bVal = General.BoolAsByte(value);
				kValue = (byte)((_downK & bVal) | (kValue & ~_downK));
			}
		}
		public bool up
		{
			get => kValue == (kValue | _upK);
		    set
			{
				byte bVal = General.BoolAsByte(value);
				kValue = (byte)((_upK & bVal) | (kValue & ~_upK));
			}
		}
		public bool right
		{
			get => kValue == (kValue | _rightK);
		    set
			{
				byte bVal = General.BoolAsByte(value);
				kValue = (byte)((_rightK & bVal) | (kValue & ~_rightK));
			}
		}
		public bool left
		{
			get => kValue == (kValue | _leftK);
		    set
			{
				byte bVal = General.BoolAsByte(value);
				kValue = (byte)((_leftK & bVal) | (kValue & ~_leftK));
			}
		}
		public bool space
		{
			get => kValue == (kValue | _spaceK);
		    set
			{
				byte bVal = General.BoolAsByte(value);
				kValue = (byte)((_spaceK & bVal) | (kValue & ~_spaceK));
			}
		}
	}


	public class SaveState
	{
		public string[] str = null;
		public bool vOld = false;

		public SaveState(Game_ART g) => str = g.GetSS();
	    public SaveState(string[] ss) => str = ss;
	    public SaveState(string filePath, bool oldLoad = false)
		{
			if (oldLoad)
			{
				string info = System.IO.File.ReadAllText(filePath);
				str = info.Split((char)0);
				vOld = true;
			}
			else
				str = General.LoadStringArray(filePath);
		}

		public void Use(Game_ART g)
		{
			g.UseSS(str);
		}

		public void Save(string filePath)
		{
			if (!System.IO.Directory.Exists(System.IO.Directory.GetParent(filePath).FullName))
				System.IO.Directory.CreateDirectory(System.IO.Directory.GetParent(filePath).FullName);
			General.SaveStringArray(filePath, str);
		}
	}
}
