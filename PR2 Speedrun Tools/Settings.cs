using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Xml.Serialization;

namespace PR2_Speedrun_Tools
{
	public class Settings
	{
		public string RecordingsPath;
		public string SavestatesPath;
		public string LevelsPath = "";

		public List<string> Users = new List<string>();
		public List<string> Tokens = new List<string>();
		public int SelectedUser = -1;

		public static Settings Load()
		{
			Settings ret = new Settings();
			if (File.Exists(Directory.GetCurrentDirectory() + "\\Settings"))
			{
				FileStream stream = new FileStream(Directory.GetCurrentDirectory() + "\\Settings", FileMode.Open);

				XmlSerializer xml = new XmlSerializer(typeof(Settings));
				ret = xml.Deserialize(stream) as Settings;

				stream.Close();
			}
			else
			{
				ret.RecordingsPath = Directory.GetCurrentDirectory() + "\\Recordings";
				ret.SavestatesPath = Directory.GetCurrentDirectory() + "\\Savestates";
				ret.Save();
			}

			if (!Directory.Exists(ret.RecordingsPath))
				Directory.CreateDirectory(ret.RecordingsPath);
			if (!Directory.Exists(ret.SavestatesPath))
				Directory.CreateDirectory(ret.SavestatesPath);

			return ret;
		}
		public void Save()
		{
			FileStream stream = new FileStream(Directory.GetCurrentDirectory() + "\\Settings", FileMode.Create);

			XmlSerializer xml = new XmlSerializer(typeof(Settings));
			xml.Serialize(stream, this);

			stream.Close();
		}
	}
}
