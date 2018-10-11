using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PR2_Speedrun_Tools
{
	class Item
	{
		public const int NONE = 0;
		public const int LASERGUN = 1;
		public const int MINE = 2;
		public const int LIGHTNING = 3;
		public const int TELEPORT = 4;
		public const int SUPERJUMP = 5;
		public const int JETPACK = 6;
		public const int SPEEDY = 7;
		public const int SWORD = 8;
		public const int FREEZERAY = 9;

		public static int NameToID(string Name)
		{
			int p;
			if (int.TryParse(Name, out p))
				return p;
			if (Name == "")
				return 0;
			switch (Name.Substring(0, 2).ToLower())
			{
				case "no":
					return 0;
				case "la":
					return 1;
				case "mi":
					return 2;
				case "li":
					return 3;
				case "te":
					return 4;
				case "su":
					return 5;
				case "je":
					return 6;
				case "sp":
					return 7;
				case "sw":
					return 8;
				case "fr":
					return 9;
			}
			return -1;
		}
		public static int GetItemID(string Str)
		{
			int IID = 0;
			if (Str == null)
				return 0;
			if (Str.ToLower().StartsWith("la"))
				IID = 1;
			else if (Str.ToLower().StartsWith("mi"))
				IID = 2;
			else if (Str.ToLower().StartsWith("li"))
				IID = 3;
			else if (Str.ToLower().StartsWith("te"))
				IID = 4;
			else if (Str.ToLower().StartsWith("su"))
				IID = 5;
			else if (Str.ToLower().StartsWith("je"))
				IID = 6;
			else if (Str.ToLower().StartsWith("sp"))
				IID = 7;
			else if (Str.ToLower().StartsWith("sw"))
				IID = 8;
			return IID;
		}
	}
}
