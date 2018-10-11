using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PR2_Speedrun_Tools
{
	interface IStatable
	{
		byte[] GetState();
		void UseState(byte[] b);
	}
}
