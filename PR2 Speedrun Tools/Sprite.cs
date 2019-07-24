using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PR2_Speedrun_Tools
{
    public class Sprite : IDisposable
    {
        public event Action ENTER_FRAME;
        public event Action EXIT_FRAME;

        public readonly Matrix Matrix;
        private  Bitmap _bitmap;

        public Sprite(Bitmap bitmap)
        {
            Matrix = new Matrix();
            _bitmap = bitmap;
        }

        public void Draw(Bitsmap graphics)
        {
            //Matrix old = graphics.Transform;

            //graphics.Transform = Matrix;
            graphics.DrawImageA(ref _bitmap, 0, 0);
            
            //graphics.Transform = old;
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                Matrix.Dispose();
                _bitmap.Dispose();
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        ~Sprite()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(false);
        }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            GC.SuppressFinalize(this);
        }
        #endregion
    }

    public class Character2
    {
  //      public static const const_52:String = "p";
		//public static const const_31:String = "c";
		//public static const const_13:String = "g";
		//public static const const_11:String = "s";
		//public static const const_56:String = "a";
		//public static const const_55:String = "t";
		//public static const const_27:String = "h";
		//public static const const_51:String = "j";
		//public static const const_25:String = "b";
		//private var var_387:class_127;
		//private var var_140:SoundChannel;
		//public var m:class_128;
  //      public var var_301:MovieClip;
		//private var var_217:Array;
		//public var curWeapon:MovieClip;
		public int hat1;
		public int hat2 = 1;
		public int hat3 = 1;
		public int hat4 = 1;
		public int head;
		public int body;
		public int feet;
		public int headColor;
		public int bodyColor;
		public int feetColor;
		public int hat1Color;
		public int hat2Color;
		public int hat3Color;
		public int hat4Color;
		public int headColor2 = -1;
		public int bodyColor2 = -1;
		public int feetColor2 = -1;
		public int hat1Color2 = -1;
		public int hat2Color2 = -1;
		public int hat3Color2 = -1;
		public int hat4Color2 = -1;
		//public var item:int = 0;
		//public var seg1:Point;
		//public var seg2:Point;
		//public var velX:Number = 0;
		//public var velY:Number = 0;
		//public var type:String = "remote";
		//public var var_670:Number;
		//protected var isAprilFools:Boolean = false;
		//public var state:String;
		//public var var_269:Number = 0;
		//public var tempID:int;
		//protected var var_448:int = 5;
		//protected var var_215:int = 0;
		//protected var var_304:Boolean = false;
		//public var var_214:Boolean = false;
		//public var var_4:class_20;
		//private var var_375:class_125;
    }

    public class LocalCharacter2 : Character2
    {

    }

    public class RemoteCharacter2 : Character2
    {

    }
}
