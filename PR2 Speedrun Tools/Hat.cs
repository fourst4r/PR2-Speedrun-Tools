using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Numerics;
using PR2_Speedrun_Tools.Properties;
using System.Diagnostics;

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

    public class Projectile
    {
        private double posX, posY;
        private double velX, velY;
        protected double life;

        //protected void HitBlock(Block block)
        //{
        //    block.onDamage(this.velX);
        //    this.HitAnything();
        //}

        //protected void HitPlayer(Character character)
        //{
        //    if (character.Type == "local")
        //    {
        //        ((LocalCharacter)character).Hit(this.velX, this.velY);
        //    }
        //    x = param1.x - this.velX;
        //    this.HitAnything();
        //}

        protected void HitAnything()
        {
            // empty :)
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

    public class Zap
    {
        public int X;
        public int Y;
        public float Alpha = 1;
        public int ID = -1;
    }

    //  public interface IEffect
    //  {
    //      int Id { get; set; }
    //      bool Go();
    //      void Draw(Bitsmap b);
    //  }

    //  public abstract class BaseEffect : IEffect
    //  {
    //      public int Id { get; set; }

    //      protected int _x, _y;
    //      protected int _life = 100;
    //      protected ICharacter Character; // owner of the effect e.g. person who used lightning

    //      public abstract void Draw(Bitsmap b);
    //      public abstract bool Go();
    //  }

    //  public interface IProjectile : IEffect
    //  {

    //  }

    ////  public class BaseProjectile : IProjectile
    ////  {
    ////      private Map course;
    ////private double var_154 = 5;
    ////private double posX;
    ////private double posY;
    ////private double velX;
    ////private double velY;
    ////private double var_278 = 0;
    ////protected int var_377;
    ////protected int life = 100;
    ////protected int var_357 = -1;
    ////protected bool var_493 = false;

    ////      public bool Go() { return false; }
    ////      public void Draw(Bitsmap b) { }

    ////      protected virtual void HitBlock(Block block) { }
    ////      protected virtual void HitPlayer(ICharacter character) { }
    ////      protected virtual void HitAnything() { }

    ////      protected void Move()
    ////      {
    ////          posX += velX;
    ////          posY += velY;
    ////      }

    ////      protected void Position()
    ////      {
    ////          //General.RotatePoint(ref posX, ref posY, -course.MainChar.Rot)
    ////          //var _loc1_:Point = class_28.method_9(this.posX, this.posY, -(this.course.blockBackground.rotation - this.var_377));
    ////          //x = _loc1_.x;
    ////          //y = _loc1_.y;
    ////      }

    ////  }

    //  public class Wave : BaseEffect
    //  {
    //      public Wave(ICharacter character)
    //      {
    //          Character = character;
    //      }

    //      public override bool Go()
    //      {
    //          return false;
    //      }

    //      public override void Draw(Bitsmap b)
    //      {
    //          b.DrawImage(ref General.WavePic, _x, _y);
    //      }
    //  }

    //  public class Zap : BaseEffect
    //  {
    //      private readonly bool _isZapper;
    //      private Bitsmap _sprite = General.ZapPic.Clone();

    //      //public Zap(ICharacter character, bool isZapper)
    //      //{
    //      //    Character = character;
    //      //    _isZapper = isZapper;
    //      //    Pos();
    //      //}

    //      public override bool Go()
    //      {
    //          Pos();
    //          return --_life > 0;
    //      }

    //      public override void Draw(Bitsmap b)
    //      {
    //          // screen flashes, if you got zapped then you see only your own character get hit with a lightning strike
    //          if (!_isZapper)
    //          {
    //              _sprite.SetAlpha(_life / 100);
    //              b.DrawImage(ref _sprite, _x, _y);
    //          }
    //      }

    //      private void Pos()
    //      {
    //          _x = (int)Character.X;
    //          _y = (int)Character.Y;
    //      }
    //  }
}
