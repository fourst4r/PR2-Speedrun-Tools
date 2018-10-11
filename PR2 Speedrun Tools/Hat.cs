using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace PR2_Speedrun_Tools
{
	public class Hat
	{
		#region "IDs"
		// "null", "none", "exp", "kong", "prop", "cowboy", "crown", "santa", "party", "top", "jump", "moon", "thief", "jigg", "artifact"
		public const int ID_NULL = 0;
		public const int ID_NONE = 1;
		public const int ID_EXP = 2;
		public const int ID_KONG = 3;
		public const int ID_PROP = 4;
		public const int ID_COWBOY = 5;
		public const int ID_CROWN = 6;
		public const int ID_SANTA = 7;
		public const int ID_PARTY = 8;
		public const int ID_TOP = 9;
		public const int ID_JUMP = 10;
		public const int ID_MOON = 11;
		public const int ID_THIEF = 12;
		public const int ID_JIGG = 13;
		public const int ID_ARTIFACT = 14;
		#endregion

		public int X;
		public int Y;
		public double VelY = -5;
		public Color Color;
		public int ID;
		public int ServID = -1;
		public int rot = 0;

		public Hat()
		{ }
		public Hat(int hatID)
		{
			ID = hatID;
		}
		public Hat(int hatID, Color col)
		{
			ID = hatID;
			Color = col;
		}

	}

	public class Laser
	{
		public int X;
		public int Y;
		public string Dir;
		public int TTV = 100;
		public int ID = -1;
		public int rot = 0;
	}

	public class Slash
	{
		public int X;
		public int Y;
		public string Dir;
		public int ID = -1;
		public int rot = 0;
	}

}
