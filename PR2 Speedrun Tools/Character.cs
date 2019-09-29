using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Collections.Generic; 
using System.Linq;

namespace PR2_Speedrun_Tools
{
    public class Character
	{
		public string Name;
		public Map course;

		private double _X;
		private double _Y;
		public double X
		{
			get { return _X; }
			set { _X = (int)(value * 20) / 20.0; }
		}
		public double Y
		{
			get { return _Y; }
			set { _Y = (int)(value * 20) / 20.0; }
		}
		public int lastX;
		public int lastY;
		public int sendFrame = 0;

		public double velX; // is int for remote char?
		public double velY;

		public string lastState = "";
		public int SuperJumpVel = 0;

		public int ScaleX = 1;
		public int Rotation = 0;
		public int RotateFrom = 0;

		public Hat[] Hats = new Hat[0];
		public int cItem = 0;

		private int _Time;
		// <summary>
		// In frames
		// </summary>
		// <value></value>
		// <returns></returns>
		// <remarks></remarks>
		public int Time
		{
			get
			{
				return _Time;
			}
			set
			{
				_Time = value;
				if (value != 0)
				{
					TimeStr = General.FramesToTime(value);
					General.PlaySound(General.sound_path + "sound 439 (VictorySound).wav");
				}

			}
		}
		public string TimeStr;
		public int finish_hit = 0;

		public int tempID = 0;
        public double RecoveryTimer = 0;
        public string State;
        protected bool isAprilFools;

        public Character()
        {
            method_11("stand");
        }

		public void Reset(bool stats = true)
		{
			// Location
			X = course.PStart[tempID].X;
			Y = course.PStart[tempID].Y;
			velX = 0;
			velY = 0;
			SuperJumpVel = 0;
			// Item
			cItem = Item.NONE;
			// Finish string
			TimeStr = "";
			finish_hit = 0;
			// Unrotate
			Rotation = 0;
			RotateFrom = 0;
			// Hats
			Hats = new Hat[] { };
		}
		public void goFrame()
		{
			X += velX;
			Y += velY;
		}

        // method_51
        public void BeginRecovery(double time)
        {
            RecoveryTimer = time;
            //ENTER_FRAME -= RecoveryUpdate;
            //ENTER_FRAME += RecoveryUpdate;
        }

        // method_106
        private void RecoveryUpdate()
        {
            //var _loc2_ = this.RecoveryTimer % 8;
            //if (!this.var_304)
            //{
            //    if (_loc2_ >= 4)
            //    {
            //        alpha = 0.5;
            //    }
            //    else
            //    {
            //        alpha = 0.75;
            //    }
            //}
            this.RecoveryTimer--;
            if (this.RecoveryTimer <= 0)
            {
                this.EndRecovery();
            }
        }

        protected virtual void EndRecovery()
        {
            //alpha = 1;
            //ENTER_FRAME -= RecoveryUpdate;
        }

        public virtual void BecomeInvincible(int time)
        {
            this.BeginRecovery(time);
            //this.method_200(new class_126(33, 5000, this));
        }

        public void method_11(string newState)
        {
            if (State != newState)
            {
                State = newState;
                // TODO: some other shit
            }
        }
    }

	public class LocalCharacter : Character
	{
		public LocalCharacter()
		{
			parts = new AnimatedPart[] { foot1, foot2, body, head }.ToList();
			foot1.rotCenter = new PointF(0, -15);
			foot1.SetPos(-6, -8);
			foot2.rotCenter = new PointF(0, -15);
			foot2.SetPos(-9, -8);
			body.rotCenter = new PointF(0, -5);
			body.SetPos(-9, -33);
			head.rotCenter = new PointF(0, -5);
			head.SetPos(-11, -60);
		}

		#region "Graphics"
		private Matrix fMatrix = new Matrix();
		public void drawSelf(Graphics MG, int DX, int DY)
		{
			GraphicsState gSave = MG.Save();

			MG.SmoothingMode = SmoothingMode.AntiAlias;

			// Default matrix (before part specific changes)
			fMatrix = new Matrix();
			if (ScaleX < 0)
			{
				fMatrix.Scale(-1, 1);
				fMatrix.Translate(-DX * 2, 0);
			}

			if (SuperJumpVel > 25)
			{
				float yScale = 1 - (SuperJumpVel / 300.0f);
				fMatrix.Translate(0, DY - (yScale * DY));
				fMatrix.Scale(1, yScale);
			}
			else if (!crouching)
			{
				if (!TouchingGround)
				{
					if (Mode != "water")
						fMatrix.RotateAt(-10, new PointF(DX, DY - 5));
					else
						fMatrix.RotateAt(-1, new PointF(DX, DY - 5));
				}
				else if (LeftK || RightK)
					fMatrix.RotateAt(-8, new PointF(DX, DY - 5));
			}
			// Idle anim
			fMatrix.RotateAt(standRot * 1.5f, new PointF(DX + 5, DY));
			fMatrix.RotateAt(-standRot, new PointF(DX + 5, DY - 10));


			MG.Transform = fMatrix;
			// BODY (should this be first, for crouching?)
			int bH = crouching ? 23 : 33;
			MG.DrawImage(General.BodyC, DX - 9, DY - bH);
			MG.DrawImage(General.Body, DX - 9, DY - bH);

			// HEAD
			bH = crouching ? 40 : 60;
			// bH -= (int)((fRot + 45) / 45);
			MG.DrawImage(General.HeadC, DX - 11, DY - bH);
			MG.DrawImage(General.Head, DX - 11, DY - bH);

			// HATS
			bH = crouching ? 24 : 44;
			Point HatPos = new Point(0, 0);
			for (int i2 = 0; i2 < Hats.Length; i2++)
			{
				Bitmap iHat = General.Hatpic[Hats[i2].ID].Bit;
				HatPos = new Point(0, 0);
				switch (Hats[i2].ID)
				{
					case Hat.ID_COWBOY:
						HatPos = new Point(1, -1);
						break;
					case Hat.ID_SANTA:
						HatPos = new Point(-1, -1);
						break;
				}
				MG.DrawImage(iHat, DX - 18 + HatPos.X, DY - bH - iHat.Height - (8 * i2) + HatPos.Y);
			}

			// Idle anim
			Matrix nMatrix = fMatrix.Clone();
			nMatrix.RotateAt(standRot * 2, new PointF(DX + 5, DY));
			// And yOffset
			float fY = 0;
			if (crouching)
				fY += fRot / 28.0f;
			// FEET
			PointF fRotPointH = new PointF(0, 22);
			float sRot = 0;
			if (!TouchingGround && Mode == "water")
			{
				fRotPointH = new PointF(-5, 8);
				sRot = 20;
				//nMatrix.RotateAt(20, new PointF(DX , DY));
				nMatrix.Translate(2, 0);
			}
			MG.Transform = nMatrix;
			if (!crouching)
			{
				MG.TranslateTransform(DX - 0 + fRotPointH.X, DY - fRotPointH.Y);
				MG.RotateTransform(-fRot + sRot);
				MG.TranslateTransform(-DX + 0 - fRotPointH.X, -DY + fRotPointH.Y);
			}
			MG.DrawImage(General.FeetC, DX - 7, DY - 8 + fY);
			MG.DrawImage(General.Feet, DX - 7, DY - 8 + fY);

			MG.Transform = nMatrix;
			if (!crouching)
			{
				MG.TranslateTransform(DX - 3 + fRotPointH.X, DY - fRotPointH.Y);
				MG.RotateTransform(fRot + sRot);
				MG.TranslateTransform(-DX + 3 - fRotPointH.X, -DY + fRotPointH.Y);
			}
			MG.DrawImage(General.FeetC, DX - 10, DY - 8 - fY);
			MG.DrawImage(General.Feet, DX - 10, DY - 8 - fY);

            nMatrix.Dispose();
			MG.Transform = fMatrix;
			// ITEM
			if (cItem != 0)
			{
				Bitmap img = General.ItemB[cItem].Bit;
				bH = crouching ? 21 : 31;
				if (cItem == Item.JETPACK)
					MG.DrawImage(img, DX - 21, DY - bH, img.Width, img.Height);
				//else if (cItem == Item.SWORD)
				//	Sword and laser gun(?) need to be drawn differently.
				else
					MG.DrawImage(img, DX + 5, DY - bH, (int)(img.Width * 0.5), (int)(img.Height * 0.5));
				img.Dispose();
				// Animations
				if (cItem == Item.JETPACK && SpaceK)
				{
					MG.DrawImage(General.jetPic.Bit, DX - 31, DY - bH + 15);
				}
				else if (cItem == Item.SPEEDY && ItemTime > 0 && ItemTime % 3 == 1)
					course.AddSparkle((int)X + General.R.Next(-17, 12), (int)Y - General.R.Next(50));
			}

			//GraphicsState gSave = MG.Save();
			//MG.SmoothingMode = SmoothingMode.AntiAlias;

			//for (int i = 0; i < parts.Count; i++)
			//    parts[i].Draw(MG, DX, DY);

			MG.Restore(gSave);
		}
		private float fRot = 0;
		private int fRev = _fRev; const int _fRev = 12;
		private float standRot = 0;
		const float _sRev = 0.3f;
		private AnimatedPart foot1 = new AnimatedPart(General.Feet, General.FeetC);
		private AnimatedPart foot2 = new AnimatedPart(General.Feet, General.FeetC);
		private AnimatedPart body = new AnimatedPart(General.Body, General.BodyC);
		private AnimatedPart head = new AnimatedPart(General.Head, General.HeadC);
		private List<AnimatedPart> parts;

		private bool wasTouchingGround = false;
		private float standTar; const float _sTar = 5;
		private void animate()
		{
			// Feet rotations
			if (!TouchingGround)
			{
				if (Mode == "water")
				{
					if (fRot > 25)
						fRot = 24.9f;
					else if (fRot < -25)
						fRot = -24.9f;
					fRot += fRev * 0.4f;
					if (Math.Abs(fRot) > 25)
						fRev *= -1;
				}
				else
				{
					if (fRot < 0)
						fRot *= -1;
					fRot += ((42.0f - fRot) * 0.2f);
					fRev = -_fRev;
				}
				wasTouchingGround = false;
			}
			else
			{
				if (!wasTouchingGround)
				{ wasTouchingGround = true; fRot = 10; }
				if ((LeftK || RightK) && SuperJumpVel < 25)
				{
					fRot += fRev;
					if (Math.Abs(fRot) > 45)
						fRev *= -1;
				}
				else // Idle
				{
					fRot = 0; fRev = _fRev;
					if (!crouching)
					{
						standRot += (standTar - standRot) / 10;
						if ((standTar != 0 && standRot > standTar - 0.7) || standRot < 0.7)
							standTar = _sTar - standTar;
					}
				}
			}
			if (fRot != 0)
				standRot = 0;
		}
		#endregion

		public const double spaceX = 10;
		public const double spaceY = 55;

		// Safe
		public int SafeX;
		public int SafeY;
		public int SafeSegX;
		public int SafeSegY;

		// Gravity
		public int RotateTo = 0;

		// Stats
		public double Speed = 0;
		public double Accel = 0;
		public double Jump = 0;
		private int iSp = 50, iAc = 50, iJp = 50;
		public int SpStat = 50;
		public int AccStat = 48;
		public int JumpStat = 50;

		// Hats
		public bool PropellerHat = false;
		public bool CowboyHat = false;
		public bool SantaHat = false;
		public bool CrownHat = false;
		public bool TopHat = false;
		public bool JumpHat = false;
		public bool PartyHat = false;
		public bool JiggHat = false;
        public bool ArtiHat = false;

		// State/mode things
		public string Mode = "land";
		public bool TouchingGround = true;
		public bool crouching = false;
		public int WaterTimer = 0;
		public double Traction = 0.35;
		public int HurtTimer = 0;
        public int SquashedTimer;
		public bool Invincible = false;
        public bool frozenSolid;

		// Keys
		private RecordedFrame _input = new RecordedFrame();
		private RecordedFrame _nextInput = new RecordedFrame();
		public RecordedFrame input
		{
			get { return _input; }
			set { _nextInput = value; }
		}

		public RecordedFrame nextInput { get { return _nextInput; } }
		private void GetInput()
		{
			_input.kValue = _nextInput.kValue;

            if (RightK)
                ScaleX = 1;
            if (LeftK)
                ScaleX = -1;
            //if (SpaceK)
		}

		public bool UpK
		{
			get { return input.up; }
			set { _nextInput.up = value; }
		}

		public bool DownK { get { return input.down; } set { _nextInput.down = value; } }
		public bool LeftK { get { return input.left; } set { _nextInput.left = value; } }
		public bool RightK { get { return input.right; } set { _nextInput.right = value; } }
		public bool SpaceK { get { return input.space; } set { _nextInput.space = value; } }
		public bool UpKP;
		public bool DownKP;
		public bool LeftKP;
		public bool RightKP;
		public bool SpaceKP;
		public void SetKeys(RecordedFrame k)
		{
			UpK = k.up;
			DownK = k.down;
			RightK = k.right;
			LeftK = k.left;
			SpaceK = k.space;
		}

		public void SetKeys(byte k)
		{
			RecordedFrame r = new RecordedFrame();
			r.kValue = k;
			SetKeys(r);
		}
		//private void testKeys()
		//{
		//    UpK = GetAsyncKeyState(38) != 0;
		//    LeftK = GetAsyncKeyState(37) != 0;
		//    RightK = GetAsyncKeyState(39) != 0;
		//    DownK = GetAsyncKeyState(40) != 0;
		//    SpaceK = GetAsyncKeyState(32) != 0;
		//}

		// Jumps
		public bool HoldingJump = false;
		public double JumpVel = 0;

		// Item
		public int lastItem = Item.NONE;
		private bool spaceReleased = false;
		public int ItemUses = 0;
		public int ItemTime = 0;
		public bool jet = false;

		public double TargetVel;

		// Reset everything, place at map's start
		public new void Reset(bool stats = true)
		{
			// Location
			X = course.PStart[tempID].X;
			Y = course.PStart[tempID].Y;
			// Safe
			SafeX = (int)X;
			SafeY = (int)Y;
			SafeSegX = 0;
			SafeSegY = 0;
			// Vel
			TargetVel = 0;
			velX = 0;
			velY = 0;
			// Traction
			Traction = 0.35;
			// Jumps
			JumpVel = 0;
			SuperJumpVel = 0;
			TouchingGround = false;
			// State
			Mode = "land";
			crouching = false;
			WaterTimer = 0;
			HurtTimer = 0;
			Invincible = false;
			// Item
			cItem = Item.NONE;
			ItemUses = 0;
			ItemTime = 0;
			// Finish string
			Time = 0;
			TimeStr = "";
			finish_hit = 0;
			// Unrotate
			Rotation = 0;
			RotateFrom = 0;
			RotateTo = 0;
			// Hats
			Hats = new Hat[] { };
			PropellerHat = false;
			CowboyHat = false;
			SantaHat = false;
			CrownHat = false;
			TopHat = false;
			JumpHat = false;
			PartyHat = false;
			JiggHat = false;
			// Stats
			if (stats)
			{
				SpStat = 50;
				AccStat = 50;
				JumpStat = 50;
			}
			else
			{
				SpStat = iSp;
				AccStat = iAc;
				JumpStat = iJp;
			}
			SetHats();
			// Previous keys
			UpKP = false;
			DownKP = false;
			RightKP = false;
			LeftKP = false;
			SpaceKP = false;
		}

		// Frame
		public new void goFrame()
		{
			// if this is the first frame, set initial stats
			if (course.Frames == 0)
			{
				iSp = SpStat; iAc = AccStat; iJp = JumpStat;
			}

			// Round your location.
			X = Math.Floor(X + 0.5);
			Y = Math.Floor(Y + 0.5);
			// ONLINE STUFFS GO HERE (p, exact pos, scaleX, state, parentbackground, item)
			// This is for TAS only for now.

			// Item timer
			if (course.Frames == 0 && JumpHat)
			{
				cItem = Item.SPEEDY;
				ItemUses = 1;
				UseItem();
				ItemTime = 54;
			}
			if (ItemTime > 0)
			{
				ItemTime -= 1;
				if (ItemTime == 0 && ItemUses == 0)
					LoseItem();
			}
			else if (ItemTime < 0)
				ItemTime += 1;

			if (HurtTimer > 0) // TODO: What does happen if you are hit before the countdown ends?
			{
				HurtTimer--;
				if (HurtTimer == 0)
					Invincible = false;
			}

            TestSquash(); 

            if (CowboyHat && course.Frames % 27 == 1)
                SetHats();

			if ((course.Frames > 54 || JumpHat))
			{

                // What go?
                if (Mode == "land")
                    LandGo();
                else if (Mode == "hurt")
                    HurtGo();
                else if (Mode == "water")
                    WaterGo();
                // freeze go is nothing
                else if (Mode == "squashed")
                    SquashedGo();
			}

			// Rotating
			if (Rotation != RotateTo)
			{
				int Rot = 3;
				if (Rotation > RotateTo)
					Rot = -3;
				Rotation += Rot;
				if (Rotation == RotateTo)
				{
                    // End rotation
                    SetMode("land");
					// Rotate your position
					if (RotateTo > RotateFrom)
					{
						double Temp = X;
						X = -Y;
						Y = Temp;
					}
					else
					{
						double Temp = X;
						X = Y;
						Y = -Temp;
					}
					// Change RotateFrom
					// (Change 270 or -270 to -90 or 90 first)
					if (Rotation == 270)
					{
						Rotation = -90;
						RotateTo = -90;
					}
					else if (Rotation == -270)
					{
						Rotation = 90;
						RotateTo = 90;
					}
					else if (Rotation == -180)
					{
						Rotation = 180;
						RotateTo = 180;
					}
					RotateFrom = RotateTo;
					lastX = (int)X;
					lastY = (int)Y;
					// Rotate blocks!
					course.SetRotation(RotateFrom);
				}
			}

			// Prevs
			UpKP = UpK;
			DownKP = DownK;
			LeftKP = LeftK;
			RightKP = RightK;
			SpaceKP = SpaceK;
			lastItem = cItem;

			// Graphical animations
			animate();
		}

		private void Position()
		{
			// Called in LandGo
			velY = velY + course.gravMod;
			if (UpK && PropellerHat && velY > 0)
				velY = velY * 0.85;
			TargetVel *= 0.985;
			if (crouching)
				TargetVel = TargetVel * 0.7;

			// Max TargetVel
			if (TargetVel < -Speed)
				TargetVel = -Speed;
			else if (TargetVel > Speed)
				TargetVel = Speed;

			// Tracion
			double Trac = Math.Abs(velX) * (1 / 28.0);
			Trac = 1.0 - Trac;
			Trac = Trac * 0.9;
			Trac = Trac + 0.1;
			Trac = Traction * Trac;
			Traction = 0.35; // Resets - will be different by next frame if need be

			// Max speed
			velX = velX + (TargetVel - velX) * Trac;
			if (velX < -28)
				velX = -28;
			else if (velX > 28)
				velX = 28;
			if (velY < -28)
				velY = -28;
			else if (velY > 28)
				velY = 28;

			// Actual change
			X += velX;
			Y += velY;

			// Fall off course?
			if (course.OffCourse(this))
				Reappear();
		}

		private void LandGo()
		{ // Executed while on land or in the air
			GetInput();
			// Left/right
			if (RightK)
				TargetVel += Accel;
			if (LeftK)
				TargetVel -= Accel;
			if (!RightK && !LeftK)
				TargetVel = 0;
			// Up
			if (UpK)
			{
				if (TouchingGround && !crouching)
				{
					HoldingJump = true;
					velY -= Jump;
					JumpVel = -Jump;
				}
				else if (HoldingJump)
				{
					velY += JumpVel;
                    JumpVel *= 0.75;
				}
			}
			else
				HoldingJump = false;
			// Down
			if (DownK)
			{
				if (!crouching)
				{
					if (!TouchingGround)
					{
						velY += 0.5;
						SuperJumpVel = 0;
					}
					else
					{
						if (SuperJumpVel < 100)
                            SuperJumpVel += 2;
						if (SuperJumpVel > 25)
							TargetVel = 0;
					}
				}
			}
			else
			{ // Not holding down
				if (SuperJumpVel > 25)
				{
					velY = -SuperJumpVel * 0.24;
					HoldingJump = false;
					General.PlaySound(General.sound_path + "sound 904 (SuperJumpSound).wav");
				}
				SuperJumpVel = 0;
			}
			// Item?
            // TODO: should this be here?
			jet = false;
			if (SpaceK)
				UseItem();
			else
				spaceReleased = true;

			Position();
            //ScaleY = 1;

            if (!this.TouchingGround)
            {
                method_11("jump");
            }
            else if (this.SuperJumpVel > 25)
            {
                method_11("superJump");
            }
            else if (this.LeftK || this.RightK)
            {
                if (this.crouching)
                {
                    method_11("crouchWalk");
                }
                else
                {
                    method_11("run");
                }
            }
            else if (this.crouching)
            {
                method_11("crouch");
            }
            else
            {
                method_11("stand");
            }

            TestBlocks();

			// Cowboy check
			if (CowboyHat && TouchingGround == false)
			{
				WaterTimer = 2;
                SetMode("water");
                method_11("swim");
			}
		}

		private void HurtGo()
		{
			TargetVel = 0;
			Position();
			TestBlocks();
			if (HurtTimer <= 0)
                SetMode("land");
        }

		private void WaterGo()
		{
			GetInput();
			// ScaleX is not working here! D:
			if (SpaceK)
				UseItem();
			else
				spaceReleased = true;
			// TODO: SpaceK is below movement stuffs in LandGo. Is this one backwards?
			if (RightK)
				velX += Accel * 0.5;
			if (LeftK)
				velX -= Accel * 0.5;
			if (DownK)
				velY += Accel * 0.65;
			if (UpK)
				velY -= Accel * 0.65;
			velY += 0.7 * 0.25; // 0.7 is "gravity", not gravitymod
			velX *= 0.92;
			velY *= 0.92;
			if (velX < -28)
				velX = -28;
			else if (velX > 28)
				velX = 28;
			if (velY < -28)
				velY = -28;
			else if (velY > 28)
				velY = 28;
			X += velX;
			Y += velY;
			TestBlocks();
			WaterTimer--;
			if (CowboyHat && !TouchingGround)
				WaterTimer = 1;
			if (WaterTimer <= 0)
			{
				if (UpK)
				{
					velY -= Jump * 0.5;
					JumpVel = -Jump * 0.5;
					HoldingJump = true;
				}
				//TargetVel = 0;
                SetMode("land");
			}
		}

        private void SquashedGo()
        {
            this.crouching = true;
            this.LandGo();
            this.SquashedTimer--;
            if (this.SquashedTimer <= 0)
            {
                velY = -5;
                this.SetMode("land");
            }
        }

        public void onSquash()
        {
            if (this.Mode != "squashed" && RecoveryTimer <= 0)
            {
                this.SetMode("squashed");
                //class_88.method_16(new SquashSound(), x, y, 0.66);
            }
        }

		// Reappear (hit net/fall off course)
		public void Reappear()
		{
			// Rotate.
			int SX = SafeX;
			int SY = SafeY;
			General.RotatePoint(ref SX, ref SY, RotateFrom);
			X = SX;
			Y = SY;
			velX = 0;
			velY = 0;
            this.LoseLife();
        }

        private void LoseLife()
        {
            if (this.HurtTimer <= 0)
            {
                this.HurtTimer = 60;
                BeginRecovery(65);
                if (this.course.gameMode == "deathmatch")
                {
                    // TODO: this dm stuff
                    //this.life--;
                    //this.setLife(this.life);
                    //if (this.life <= 0)
                    //{
                    //    this.course.finish();
                    //}
                }
            }
        }

        public override void BecomeInvincible(int time)
        {
            base.BecomeInvincible(time);
            this.HurtTimer = time;
            this.Invincible = true;
        }

        protected override void EndRecovery()
        {
            base.EndRecovery();
            this.Invincible = false;
        }

		// Item useage
		private void UseItem()
		{
			if (ItemTime != 0 || !spaceReleased)
				return;

			if (cItem == Item.SUPERJUMP)
				velY -= 25;
			else if (cItem == Item.TELEPORT)
			{
				if (!course.getBlock((int)X + 120 * ScaleX, (int)Y - 5, RotateFrom, true).IsSolid())
					X += 120 * ScaleX;
				else
					ItemUses += 1; // Don't lose if can't use.
			}
			else if (cItem == Item.SPEEDY)
			{
				Speed *= 2;
				Accel *= 2;
				ItemTime = 135; // 5 seconds
			}
			else if (cItem == Item.JETPACK)
			{
				if (velY > -5)
					velY -= 1.25;
				else
					velY -= 0.5;
				jet = true;
			}
			else if (cItem == Item.LASERGUN)
			{
				// Cool-down is 0.8 secs, 22 frames
				ItemTime = 22;
				string dir = "right";
				if (ScaleX < 0)
				{
					velX = velX + 15;
					dir = "left";
				}
				else
					velX = velX - 15;
				// make the laser
				int relX = 36;
				int relY = -24;
				if (RightK || LeftK)
				{
					if (!(RightKP || LeftKP))
					{
						relX = 40;
						relY = -28;
					}
					else
					{
						relX = 34;
						relY = -35;
					}
				}
				if (dir == "left")
					relX = -relX;
				course.MakeLaser((int)X + relX, (int)Y + relY, tempID, dir, RotateFrom);
			}
			else if (cItem == Item.SWORD)
			{
				ItemTime = 22;
				string dir = "right";
				if (ScaleX < 0)
				{
					dir = "left";
					velX -= 8;
				}
				else
					velX += 8;
				// make the Slash
				int relX = 10;
				int relY = -2;
				if (TouchingGround && (RightK || LeftK))
				{
					relX = 8;
					relY = -11;
				}
				if (dir == "left")
					relX = -relX;
				course.MakeSlash((int)X + relX, (int)Y + relY, tempID, dir, RotateFrom);
			}
			else if (cItem == Item.MINE)
			{
				int bombX = (int)X;
				int bombY = (int)Y;
				General.RotatePoint(ref bombX, ref bombY, -RotateFrom);
				// TODO: Get correct placement position
				course.PlaceBomb(bombX, bombY);
			}
            else if (cItem == Item.LIGHTNING)
            {
                course.MakeZap((int)X, (int)Y, tempID);
                //course.Effects.Add(new Zap(this, true));
            }
            else if (cItem == Item.FREEZERAY)
            {
                //course.MakeWave()
            }
			// TODO: Freeze Ray!

            if (ItemUses > 0)
			    ItemUses -= 1;
			if (ItemUses == 0 && cItem != Item.SPEEDY)
				LoseItem(); // Speedy is not lost until it wears out.
		}

		private void LoseItem()
		{
			if (cItem == Item.SPEEDY)
				SetStats();
			cItem = Item.NONE;
			ItemUses = 0;
			ItemTime = 0;
		}

		public void GiveItem()
		{
			if (course.avItems.Length == 0)
				return;

			LoseItem(); // Lose current item if (you have one)
			int _loc_2;
			int _loc_3;
			_loc_2 = General.R.Next(0, course.avItems.Length);
			_loc_3 = course.avItems[_loc_2];
			cItem = _loc_3;
			ItemUses = 1;
			if (cItem == Item.LASERGUN || cItem == Item.SWORD || cItem == Item.FREEZERAY)
				ItemUses = 3;
			else if (cItem == Item.JETPACK)
				ItemUses = 200;

			ItemTime = -2;
			if (cItem == Item.JETPACK)
				ItemTime = 0;

			spaceReleased = false;
		}

        //public void GetZapped(int zapperID) => MakeZap;
        //public void GetFrozenSolid() => course.Effects

		// Hat give/take
		public void GiveHat(Hat h)
		{
			Array.Resize(ref Hats, Hats.Length + 1);
			Hats[Hats.Length - 1] = h;
			SetHatPowers();
		}
		public void SetHatPowers()
		{
			PropellerHat = false;
			CowboyHat = false;
			SantaHat = false;
			CrownHat = false;
			TopHat = false;
			JumpHat = false;
			PartyHat = false;
			JiggHat = false;
			for (int i = 0; i < Hats.Length; i++)
			{
				switch (Hats[i].ID)
				{
					case Hat.ID_PROP:
						PropellerHat = true;
						break;
					case Hat.ID_COWBOY:
						CowboyHat = true;
						break;
					case Hat.ID_SANTA:
						SantaHat = true;
						break;
					case Hat.ID_CROWN:
						CrownHat = true;
						break;
					case Hat.ID_TOP:
						TopHat = true;
						break;
					case Hat.ID_JUMP:
						JumpHat = true;
						break;
					case Hat.ID_PARTY:
						PartyHat = true;
						break;
					case Hat.ID_JIGG:
						JiggHat = true;
						break;
				}
			}
			SetHats();
		}

		// Blocks
		Block bottomRight;
		Block bottomCenter;
		Block bottomLeft;
		Block lowerRight;
		Block lowerCenter;
		Block lowerLeft;
		Block midRight;
		Block midCenter;
		Block midLeft;
		Block highCenter;
		Block topCenter;

        private void testGround()
		{
			// Solid check (required because of TopHat)
			if (bottomCenter.IsSolid() && midCenter.IsSolid(this) == false)
			{
				bottomCenter.StandOn(this);
				setBlocks();
				TouchingGround = true;
			}
			else
				TouchingGround = false;
		}
		private void TestBlocks()
		{
			Block _loc1_;
			setBlocks();
			testGround();
			bool Frz = false;
			if (SantaHat)
			{
				_loc1_ = course.getBlock((int)X, (int)Y, RotateFrom, true);
				if (_loc1_.T == BlockID.Water && Mode != "water" || _loc1_.T == BlockID.Net)
				{
                    //_loc1_.StandOn(this);
                    if (!_loc1_.WasIced || _loc1_.TurnedToIce) {
                        _loc1_.StandOn(this);
                        setBlocks();
                        if (!_loc1_.WasIced)
                            Frz = true;
                    }
                }
			}
			// Right/left pushing never happens on a freeze. I really don't get how PR2 does it.
			if (!Frz)
			{
				if (velX >= -1)
				{
					if (lowerRight.IsSolid(this) && course.getBlock(lowerRight.X - 1, lowerRight.Y, RotateFrom).IsSolid(this) == false)
					{
						lowerRight.HitLeft(this);
						setBlocks();
					}
				}
				if (velX <= 1)
				{
					if (lowerLeft.IsSolid(this) && course.getBlock(lowerLeft.X + 1, lowerLeft.Y, RotateFrom).IsSolid(this) == false)
					{
						lowerLeft.HitRight(this);
						setBlocks();
					}
				}
			}

			// Bumps, only when going up
			if ((velY < 0))
			{
				if (TouchingGround)
					crouching = true; // Referenced in Bump to determine Y pos

				// You can swim through mid and highCenter. (Swim with head in blocks glitch)
				if (midCenter.IsSolid(this) && course.getBlock(midCenter.X, midCenter.Y + 1, RotateFrom).IsSolid(this) == false && Mode != "water")
				{
					midCenter.Bump(this);
					setBlocks();
				}
				else if (highCenter.IsSolid(this) && course.getBlock(highCenter.X, highCenter.Y + 1, RotateFrom).IsSolid(this) == false && Mode != "water")
				{
					highCenter.Bump(this);
					setBlocks();
				}
				else if (topCenter.IsSolid(this) && course.getBlock(topCenter.X, topCenter.Y + 1, RotateFrom).IsSolid(this) == false)
				{
					topCenter.Bump(this);
					setBlocks();
				}
			}

			if (!TouchingGround)
				testGround();

			Block _loc_1;
			Block _loc_2;
			crouching = false;
			if (TouchingGround)
			{
				_loc_1 = course.getBlock((int)X, (int)Y - 40, RotateFrom, true);
				_loc_2 = course.getBlock((int)X, (int)Y - 10, RotateFrom, true);
				if (_loc_1.IsSolid(this) && !_loc_2.IsSolid(this))
				{
					crouching = true;
					if (UpK)
					{
						int _loc_4 = (int)Y;  // Cause Jiggmin is weird with his code.
						_loc_1.Bump(this);
						Y = _loc_4;
						velY = 0;
					}
					if (velY < 0)
						velY = 0;
				}
			}

			_loc_1 = course.getBlock((int)X, (int)Y - 15, RotateFrom, true);
			if (_loc_1.T != 99)
				_loc_1.onTouch(this);
			if (!crouching)
			{
				_loc_1 = course.getBlock((int)X, (int)Y - 45, RotateFrom, true);
				if ((_loc_1.T != 99))
					_loc_1.onTouch(this);
			}
		}
		// Setting blocks
		private void setBlocks()
		{
			bottomLeft = course.getBlock((int)(X - LocalCharacter.spaceX), (int)(Y), RotateFrom, true);
			bottomCenter = course.getBlock((int)X, (int)(Y), RotateFrom, true);
			bottomRight = course.getBlock((int)(X + LocalCharacter.spaceX), (int)(Y), RotateFrom, true);
			lowerLeft = course.getBlock((int)(X - LocalCharacter.spaceX), (int)(Y) - 10, RotateFrom, true);
			lowerCenter = course.getBlock((int)(X), (int)(Y - 10), RotateFrom, true);
			lowerRight = course.getBlock((int)(X + LocalCharacter.spaceX), (int)(Y) - 10, RotateFrom, true);
			midLeft = course.getBlock((int)(X - LocalCharacter.spaceX), (int)(Y) - 30, RotateFrom, true);
			midCenter = course.getBlock((int)(X), (int)(Y) - 30, RotateFrom, true);
			midRight = course.getBlock((int)(X + LocalCharacter.spaceX), (int)(Y) - 30, RotateFrom, true);
			highCenter = course.getBlock((int)(X), (int)(Y - LocalCharacter.spaceY) + 30, RotateFrom, true);
			topCenter = course.getBlock((int)(X), (int)(Y - LocalCharacter.spaceY), RotateFrom, true);
		}

		// Set stats
		public void SetStats()
		{
			if (SpStat > 100)
				SpStat = 100;
			else if (SpStat < 0)
				SpStat = 0;

			if (AccStat > 100)
				AccStat = 100;
			else if (AccStat < 0)
				AccStat = 0;

			if (JumpStat > 100)
				JumpStat = 100;
			else if (JumpStat < 0)
				JumpStat = 0;

			Speed = SpStat / 10.0 + 2;
			Accel = AccStat / 60.0 + 0.2;
			Jump = JumpStat / 40.0 + 2;
		}
		// Set hats
		public void SetHats()
		{
			SetStats();
			if (CowboyHat)
			{
				Speed = 12;
				Accel = 1.86;
				Jump = 4.5;
			}
			if (SantaHat)
			{
				Speed = Speed + 1;
			}
            if (ArtiHat)
            {
                //cItem = Item.SPEEDY;
                //SpeedBurst(this.var_99).var_335 = 30000;
                //this.var_99.useItem();
                //_loc6_ = class_30.course.timer;
                //if (_loc6_.getTime() > 30)
                //{
                //    _loc6_.setTime(30);
                //    _loc6_.init();
                //}
                //_loc7_ = new class_140(this, false, false);
                ////_loc7_.transform.colorTransform = new ColorTransform(1, 1, 1, 1, 0, 0, 255, 0);
                ////class_88.method_19(new Sound(new URLRequest("http://cdn.jiggmin.com/games/platform-racing-2/yeah.mp3")));
                //class_30.course.musicSelection.var_216.method_629();
            }
		}

        public void GetZapped(int zapperID)
        {
            if (!PartyHat && HurtTimer <= 0 && tempID != zapperID)
                HurtTimer = 60;
        }

        // method_704
        private void TestSquash()
        {
            if (velY > 0 && JiggHat)
            {
                //foreach(var player in this.course.var_40)
                //{
                //    if (player is RemoteCharacter && player.state != "crouch" 
                //        && player.state != "crouchWalk" && player.x > x - 20 
                //        && player.x < x + 20 && player.y > y + 35 && player.y < y + 65 
                //        && player.rotation == this.rotation)
                //    {
                //        player.method_11("crouch");
                //        //class_88.method_16(new SquashSound(), x, y, 0.66);
                //        //this.socket.write("squash`" + _loc1_.tempID + "`" + x + "`" + y);
                //        velY = -3;
                //        this.TouchingGround = true;
                //    }
                //}
            }
        }

        public void SetMode(string mode)
        {
            if (this.Mode != mode)
            {
                this.Mode = mode;
                this.TargetVel = 0;
                if (mode == "hurt")
                {
                    method_11("bumped");
                    this.LoseLife();
                }
                if (mode == "water" && this.State != "bumped")
                {
                    method_11("swim");
                }
                if (mode == "squashed")
                {
                    this.SquashedTimer = 60;
                    BeginRecovery(70);
                }
            }
        }

		// get hit (by bomb, laser, sword)
		public void GetHit(double vX, double vY)
		{
			if (Invincible)
				return;

			if (!CrownHat || course.gameMode.StartsWith("d"))
			{ // TODO: What happens to a running character when shot in the back with a laser, while wearing crown in deathmatch?
				velX = velX + vX;
				velY = velY + vY;
                BeginRecovery(50);
                if (!this.frozenSolid)
                {
                    this.SetMode("hurt");
                }

                if (Hats.Length > 0)
					Array.Resize(ref Hats, Hats.Length - 1);
			}
		}


		// GetSS data
		public string GetSSData()
		{
			// Safe
			string Str = SafeSegX + "," + SafeSegY + "," + SafeX + "," + SafeY;
			// Gravity
			Str += "," + RotateFrom + "," + Rotation + "," + RotateTo;
			// Stats
			Str += "," + Speed.ToString("R") + "," + SpStat + "," + Accel.ToString("R") + "," + AccStat + "," + Jump.ToString("R") + "," + JumpStat;
			// Hats
			Str += "," + PropellerHat + "," + CowboyHat + "," + SantaHat + "," + CrownHat + "," + TopHat + "," + JumpHat + "," + PartyHat + "," + JiggHat;
			// State/mode things
			Str += "," + Mode + "," + TouchingGround + "," + crouching + "," + WaterTimer + "," + HurtTimer;
			// Keys
			Str += "," + UpKP + "," + spaceReleased + "," + LeftKP + "," + RightKP + "," + SpaceKP;
			// Jumps, vels
			Str += "," + HoldingJump + "," + JumpVel.ToString("R") + "," + SuperJumpVel + "," + TargetVel.ToString("R");
			Str += "," + X.ToString("R") + "," + Y.ToString("R") + "," + lastX + "," + lastY + "," + velX.ToString("R") + "," + velY.ToString("R");
			// Item
			Str += "," + cItem + "," + lastItem + "," + ItemUses + "," + ItemTime + "," + jet;
			// Other stuffs?
			Str += "," + Traction.ToString("R") + "," + Time + "," + TimeStr + "," + ScaleX;
			// Hats
			Str += ",";
			for (int i = 0; i < Hats.Length; i++)
			{
				Str += Hats[i].ID + ";" + Hats[i].Color.ToArgb() + ";";
			}
			// More
			Str += "," + finish_hit + "," + Name + "," + Invincible;

			return Str;
		}
		public void UseSSData(string Str)
		{
			// Safe
			string[] SS = Str.Split(',');
			//Str string = SafeSegX & "," & SafeSegY & "," & SafeX & "," & SafeY
			SafeSegX = Convert.ToInt32(SS[0]);
			SafeSegY = Convert.ToInt32(SS[1]);
			SafeX = Convert.ToInt32(SS[2]);
			SafeY = Convert.ToInt32(SS[3]);
			// Gravity
			//Str &= "," & RotateFrom & "," & Rotation & "," & RotateTo
			RotateFrom = Convert.ToInt32(SS[4]);
			Rotation = Convert.ToInt32(SS[5]);
			RotateTo = Convert.ToInt32(SS[6]);
			course.SetRotation(RotateFrom);
			// Stats
			//Str &= "," & Speed & "," & SpStat & "," & Accel & "," & AccStat & "," & Jump & "," & JumpStat
			Speed = Convert.ToDouble(SS[7]);
			SpStat = Convert.ToInt32(SS[8]);
			Accel = Convert.ToDouble(SS[9]);
			AccStat = Convert.ToInt32(SS[10]);
			Jump = Convert.ToDouble(SS[11]);
			JumpStat = Convert.ToInt32(SS[12]);
			// Hats
			//Str &= "," & PropellerHat & "," & CowboyHat & "," & SantaHat & "," & CrownHat & "," & TopHat & "," & JumpHat & "," & PartyHat & "," & JiggHat
			PropellerHat = Convert.ToBoolean(SS[13]);
			CowboyHat = Convert.ToBoolean(SS[14]);
			SantaHat = Convert.ToBoolean(SS[15]);
			CrownHat = Convert.ToBoolean(SS[16]);
			TopHat = Convert.ToBoolean(SS[17]);
			JumpHat = Convert.ToBoolean(SS[18]);
			PartyHat = Convert.ToBoolean(SS[19]);
			JiggHat = Convert.ToBoolean(SS[20]);
			// State/mode things
			//Str &= "," & State & "," & TouchingGround & "," & crouching & "," & WaterTimer & "," & HurtTimer
			Mode = (SS[21]);
			TouchingGround = Convert.ToBoolean(SS[22]);
			crouching = Convert.ToBoolean(SS[23]);
			WaterTimer = Convert.ToInt32(SS[24]);
			HurtTimer = Convert.ToInt32(SS[25]);
			// Keys
			//Str &= "," & UpKP & "," & DownKP & "," & LeftKP & "," & RightKP & "," & SpaceKP
			UpKP = Convert.ToBoolean(SS[26]);
			spaceReleased = Convert.ToBoolean(SS[27]); // DownKP isn't actually used anywhere!
			LeftKP = Convert.ToBoolean(SS[28]);
			RightKP = Convert.ToBoolean(SS[29]);
			SpaceKP = Convert.ToBoolean(SS[30]);
			// Jumps, vels
			//Str &= "," & HoldingJump & "," & JumpVel & "," & SuperJumpVel & "," & TargetVel
			//Str &= "," & X & "," & Y & "," & lastX & "," & lastY & "," & velX & "," & velY
			HoldingJump = Convert.ToBoolean(SS[31]);
			JumpVel = Convert.ToDouble(SS[32]);
			SuperJumpVel = Convert.ToInt32(SS[33]);
			TargetVel = Convert.ToDouble(SS[34]);
			X = Convert.ToDouble(SS[35]);
			Y = Convert.ToDouble(SS[36]);
			lastX = Convert.ToInt32(SS[37]);
			lastY = Convert.ToInt32(SS[38]);
			velX = Convert.ToDouble(SS[39]);
			velY = Convert.ToDouble(SS[40]);
			// Item
			//Str &= "," & cItem & "," & lastItem & "," & ItemUses & "," & ItemTime & "," & jet
			cItem = Convert.ToInt32(SS[41]);
			lastItem = Convert.ToInt32(SS[42]);
			ItemUses = Convert.ToInt32(SS[43]);
			ItemTime = Convert.ToInt32(SS[44]);
			jet = Convert.ToBoolean(SS[45]);
			// Other stuffs?
			//Str &= "," & Traction & "," & Time & "," & FinStr & "," & ScaleX
			Traction = Convert.ToDouble(SS[46]);
			Time = Convert.ToInt32(SS[47]);
			TimeStr = (SS[48]);
			ScaleX = Convert.ToInt32(SS[49]);
			// Hats
			//Str &= ","
			//For i int = 0 To Hats.Length - 1
			//    Str &= Hats(i).ID & ";" & Hats(i).Color.ToArgb() & ";"
			//Next
			string[] hStr = SS[50].Split(';');
			Array.Resize(ref Hats, 0);
			Array.Resize(ref Hats, hStr.Length / 2);
			for (int i = 0; i < hStr.Length - 1; i += 2)
				Hats[i / 2] = new Hat(Convert.ToInt32(hStr[i]), System.Drawing.Color.FromArgb(Convert.ToInt32(hStr[i + 1])));

			// Everything added after previous code must be after, for backwards compatibility
			// TODO: Determine if I still have any old savestates that I want to keep.
			if (SS.Length > 51)
			{
				finish_hit = Convert.ToInt32(SS[51]);
				if (SS.Length > 52)
				{
					Name = SS[52];
					if (SS.Length > 53)
						Invincible = bool.Parse(SS[53]);
					else
						Invincible = false;
				}
				else
					Name = "Player";
			}
		}

	}

}
