using System;

namespace PR2_Speedrun_Tools
{
    public static class BlockID
    {
        public const int BB0 = 0;
        public const int BB1 = 1;
        public const int BB2 = 2;
        public const int BB3 = 3;
        public const int Brick = 4;
        public const int Down = 5;
        public const int Up = 6;
        public const int Left = 7;
        public const int Right = 8;
        public const int Mine = 9;
        public const int Item = 10;
        public const int P1 = 11;
        public const int P2 = 12;
        public const int P3 = 13;
        public const int P4 = 14;
        public const int Ice = 15;
        public const int Finish = 16;
        public const int Crumble = 17;
        public const int Vanish = 18;
        public const int Move = 19;
        public const int Water = 20;
        public const int GravRight = 21;
        public const int GravLeft = 22;
        public const int Push = 23;
        public const int Net = 24;
        public const int InfItem = 25;
        public const int Happy = 26;
        public const int Sad = 27;
        public const int Heart = 28;
        public const int Time = 29;
        public const int Egg = 30;
        public const int Invisible = 88; // Obviously not in real PR2 LE, but it works in real PR2 levels. Well, used to.
    }

    public class Block
    {
        #region "const intants"
        public const int NORMAL = 0;
        public const int VANISHING = 1;
        public const int INACTIVE = 2;
        public const int REAPPEARING = 3;
        #endregion

        public int X = 0;
        public int Y = 0;
        public int T = BlockID.BB0;
        public bool TurnedToIce = false;
        public bool WasIced = false;
        public bool frozen;
        public int FreezeTime = 0;
        public int FadeTime = 0;
        public int Health = 10;
        public bool[] Used = new bool[4];
        public int Fade = NORMAL;
        public double BumpVel = 0;
        public double BumpY = 0;

        public Map course;
        public int mapID;

        public Block Clone()
        {
            Block Cln = new Block();
            Cln.X = X;
            Cln.Y = Y;
            Cln.T = T;
            Cln.TurnedToIce = TurnedToIce;
            Cln.FadeTime = FadeTime;
            Cln.Health = Health;
            Cln.Used = Used;
            Cln.Fade = 0;
            Cln.BumpVel = BumpVel;
            Cln.BumpY = BumpY;
            // Cln.Under = Under.Clone
            return Cln;
        }

        public bool IsSolid(LocalCharacter c = null)
        {
            if (T != BlockID.Net && T != BlockID.Water && T != BlockID.Egg && T != 99 && (T < BlockID.P1 || T > BlockID.P4))
            {
                // If it's vanish and faded away, or player has Top Hat, false
                if (T == BlockID.Vanish && (Fade == INACTIVE || (c != null && c.TopHat)))
                    return false;

                // Mines that aren't fully placed aren't solid
                if (T == BlockID.Mine && FadeTime != 0)
                    return false;

                return true;
            }
            else
                return TurnedToIce;
        }

        public bool Safe
        {
            get
            {
                if ((T < 9 && T != 4) || T == 10 || T == 15 || T == 16 || T == 21 || T == 22 || (T > 24 && T != 99))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        public void Thaw()
        {
            FreezeTime -= 1;
            if (FreezeTime < 1)
            {
                FreezeTime = 89; // how long?
                TurnedToIce = false;
                course.RemoveBlockEvent(this, Map.EVENT_THAW);
                course.AddBlockEvent(this, Map.EVENT_REFRZ);
            }
        }

        public void ReFreezeable()
        {
            FreezeTime -= 1;
            if (FreezeTime < 1)
            {
                FreezeTime = 0;
                WasIced = false;
                course.RemoveBlockEvent(this, Map.EVENT_REFRZ);
            }
        }

        // VANISH BLOCKS DO NOT WORK CORRECTLY !ERROR!
        public void goVanish()
        {
            if (Fade == VANISHING)
            { //Fading out
                FadeTime -= 1;
                if (FadeTime == 0)
                {
                    // Make it disappear
                    FadeTime = 54;
                    Fade = INACTIVE;
                }
            }
            else if (Fade == INACTIVE)
            { //waiting to fade in
                // If FadeTime is below a certian point&&tChar are in (or <30 below), no decrease.
                FadeTime -= 1;
                if (FadeTime == 0)
                {
                    // Start fading in
                    FadeTime = 1; // So it can't be 0 and go to 10 by touching.
                    Fade = REAPPEARING;
                }
            }
            else
            { // Fading in
                FadeTime += 1;
                if (FadeTime > 10)
                {
                    // Done.
                    FadeTime = 0;
                    Health = 10; // un-darkern
                    Fade = NORMAL;
                    course.RemoveBlockEvent(this, Map.EVENT_VANISH);
                    //RemoveHandler Form1.Ticker, AddressOf goVanish
                }
            }
        }

        /// <summary>
        /// SANTA HACK: freezing blocks you stand on is delayed somehow (by 4ms?) so this hacky var
        ///             makes santa behave as it should... hopefully.
        /// </summary>
        public bool SHOULD_FREEZE_NEXT_FRAME;
        public void Freeze()
        {
            if (SHOULD_FREEZE_NEXT_FRAME) {
                SHOULD_FREEZE_NEXT_FRAME = false;
                TurnedToIce = true;
                WasIced = true;
                FreezeTime = 36;
                course.AddBlockEvent(this, Map.EVENT_THAW);
            } else {
                SHOULD_FREEZE_NEXT_FRAME = true;
            }
        }

        public void Move()
        {
            //    'Movey.
            //    Dim D  String = Form1.MoveDir(Form1.MoveID)
            //    Form1.MoveID += 1
            //    if (Form1.MoveID > 44 {
            //        Form1.MoveID = 0
            //    }
            //    Dim movX  int = 0
            //    Dim movY  int = 0
            //    if (D = "u" {
            //        movY = -1
            //    } else if (D = "d" {
            //        movY = 1
            //    } else if (D = "l" {
            //        movX = -1
            //    } else if (D = "r" {
            //        movX = 1
            //    }
            //    'See if there is a block in the way.
            //    if (Form1.getBlock(X + movX, Y + movY, false).T = 99 {
            //        ''Remove this block, add it somewhere } else {
            //        'X += movX
            //        'Y += movY
            //        'Form1.AddBlock(X, Y, T)
            //        ''May need to set other properties manually, before deleting.
            //        'Form1.DeleteBlock(X - movX, Y - movY)
            //        Form1.MoveBlock(X, Y, movX, movY)
            //    }
        }

        public bool OnScreen(LocalCharacter tChar)
        {
            // Rotation
            int YX = (int)tChar.X;
            int YY = (int)tChar.Y;
            int Temp = YX;
            if (tChar.RotateFrom == 90)
            {
                YX = YY;
                YY = -Temp;
            }
            else if (tChar.RotateFrom == 180 || tChar.RotateFrom == -180)
            {
                YX = -YX;
                YY = -YY;
            }
            else if (tChar.RotateFrom == -90)
            {
                YX = -YY;
                YY = Temp;
            }
            // What area of the screen is "on screen"?
            int MnX = YX - 300;
            int MnY = YY - 285; // approximation
            int MxX = YX + 301;
            int MxY = YY + 166;
            // If checking each of four directions
            if (X * 30 < MnX || X * 30 > MxX)
            {
                return false;
            }
            if (Y * 30 < MnY || Y * 30 > MxY)
            {
                return false;
            }
            // If none,
            return true;
        }

        public int startBumping = 2; // TODO: Verify that this is actually how it works.
        // One frame for late event, another for idk what.
        public void Bumping()
        {
            if (startBumping != 0)
            {
                startBumping--;
                return;
            }

            BumpVel *= 0.5;
            BumpY += BumpVel;
            BumpY += -BumpY * 0.35;
            if (BumpY < 0.25)
            {
                BumpY = 0;
                BumpVel = 0;
                course.RemoveBlockEvent(this, Map.EVENT_BUMP);
                startBumping = 2;
            }
        }

        public void Place()
        {
            FadeTime -= 1;
            if (FadeTime == 0)
            {
                course.RemoveBlockEvent(this, Map.EVENT_PLACE);
            }
        }


        // Touch block
        public void onStand(LocalCharacter tChar)
        {
            int BlocX = X * 30;
            int BlocY = Y * 30;
            // Rotate block coordinates (to determine where to place character)
            General.RotatePoint(ref BlocX, ref BlocY, tChar.Rotation);
            if (tChar.RotateFrom == -90 || tChar.RotateFrom == 180)
                BlocY -= 30;

            // Santa Freeze  Can't freeze starts
            if ((!WasIced && tChar.SantaHat && !(T >= 11 && T <= 14)))
            {
                this.Freeze();
            }

            if (T == BlockID.Ice || TurnedToIce)
                tChar.Traction = 0.05;
            else if (T == BlockID.Crumble)
            {
                int _loc_8 = (int)Math.Floor(tChar.velY * 2 + 0.5);
                _loc_8 = (int)Math.Floor((double)_loc_8 / 4);
                Health -= _loc_8;
                if (Health <= 0)
                {
                    T = 99;
                    course.DeleteBlock(X, Y);
                    return;
                }
            }

            tChar.Y = BlocY + BumpY;
            tChar.velY = 0;
            tChar.TouchingGround = true;
            if (Safe) // TODO: Make this code nicer.
            {
                int SX = 15;
                int SY = 0;
                if (tChar.RotateFrom == 90)
                {
                    SX = 0;
                    SY = 15;
                }
                else if (tChar.RotateFrom == 180)
                {
                    SX = 15;
                    SY = 30;
                }
                else if (tChar.RotateFrom == -90)
                {
                    SX = 30;
                    SY = 15;
                }
                tChar.SafeX = X * 30 + SX;
                tChar.SafeY = Y * 30 + SY;
                tChar.SafeSegX = X;
                tChar.SafeSegY = Y;
            }

            if (GetArrowDir(tChar.RotateFrom) == BlockID.Down)
                tChar.velY += 5;
            else if (GetArrowDir(tChar.RotateFrom) == BlockID.Up)
            {
                if (!tChar.crouching)
                    tChar.velY -= 10;
            }
            else if (GetArrowDir(tChar.RotateFrom) == BlockID.Left)
                tChar.velX -= 3;
            else if (GetArrowDir(tChar.RotateFrom) == BlockID.Right)
                tChar.velX += 3;

            if (!TurnedToIce)
            {
                if (T == BlockID.Mine && FadeTime == 0)
                { // Mine
                    HitChar(tChar);
                    course.DeleteBlock(X, Y);
                }
                else if (T == BlockID.Push)
                    GetPushed(0, 1, tChar);
                else if (T == BlockID.Vanish)
                    Vanish();
            }
        }

        public void Bump(LocalCharacter tChar)
        {
            int BlocX = X * 30;
            int BlocY = Y * 30;
            // Rotate block coordinates (to determine where to place character)
            General.RotatePoint(ref BlocX, ref BlocY, tChar.Rotation);
            if (tChar.RotateFrom == -90 || tChar.RotateFrom == 180)
                BlocY -= 30;

            if (T == BlockID.Crumble)
            {
                double _loc_8 = Math.Floor(-tChar.velY + 0.5);
                _loc_8 = Math.Floor(_loc_8 / 4);
                Health -= (int)_loc_8;
                if (Health <= 0)
                {
                    T = 99;
                    course.DeleteBlock(X, Y);
                    return;
                }
            }

            if (tChar.crouching)
                tChar.Y = BlocY + 30 - BumpY + (LocalCharacter.spaceY / 2.0);
            else
                tChar.Y = BlocY + 30 + LocalCharacter.spaceY - BumpY;

            tChar.velY *= -0.25;
            tChar.JumpVel = 0;

            if (!(T >= 5 && T <= 8))
            { // Arrows are the only known non-bumpable.
                BumpVel = 15;
                // Only add ONE handler
                if (BumpY == 0)
                    course.AddBlockEvent(this, Map.EVENT_BUMP);
            }
            General.PlaySound(General.sound_path + "sound 445 (ThumpSound).wav");

            if (GetArrowDir(tChar.RotateFrom) == BlockID.Up)
            {
                if (!tChar.DownK && !tChar.crouching)
                    tChar.velY = -14;
                else
                    tChar.velY = 0;
            }
            else if (GetArrowDir(tChar.RotateFrom) == BlockID.Down)
                tChar.velY += 5;
            else if (GetArrowDir(tChar.RotateFrom) == BlockID.Left)
                tChar.velX -= 3;
            else if (GetArrowDir(tChar.RotateFrom) == BlockID.Right)
                tChar.velX += 3;

            if (TurnedToIce)
                return;

            if (T == BlockID.Brick)
                course.DeleteBlock(X, Y);
            else if (T == BlockID.Finish)
            { // If it's egg mode, nothing will happen. Who cares about that mode, right?
                if (course.gameMode.StartsWith("r") || course.gameMode.StartsWith("d"))
                {
                    if (tChar.TimeStr == "")
                        tChar.Time = course.Frames;
                }
                else if (course.gameMode.StartsWith("o"))
                {
                    if (Health != 0)
                    {
                        Health = 0;
                        tChar.finish_hit += 1;
                        if (tChar.finish_hit == course.finish_count)
                            tChar.Time = course.Frames;
                    }
                }
            }
            else if (T == BlockID.Item)
            {
                if (!Used[tChar.tempID])
                {
                    Used[tChar.tempID] = true;
                    tChar.GiveItem();
                }
            }
            else if (T == BlockID.InfItem)
                tChar.GiveItem();
            else if (T == BlockID.Happy)
            {
                if (!Used[tChar.tempID])
                {
                    Used[tChar.tempID] = true;
                    tChar.SpStat += 5;
                    tChar.AccStat += 5;
                    tChar.JumpStat += 5;
                    tChar.SetStats();
                }
            }
            else if (T == BlockID.Sad)
            {
                if (!Used[tChar.tempID])
                {
                    Used[tChar.tempID] = true;
                    tChar.SpStat -= 5;
                    tChar.AccStat -= 5;
                    tChar.JumpStat -= 5;
                    tChar.SetStats();
                }
            }
            else if (T == BlockID.Mine && FadeTime == 0)
            {
                HitChar(tChar);
                course.DeleteBlock(X, Y);
            }
            else if (T == BlockID.Push)
                GetPushed(0, -1, tChar);
            else if (T == BlockID.GravRight)
            {
                // Set mode (tChar.State) to freeze. (While frozen, the "Go" is nothing.)
                tChar.SetMode("freeze");
                // tChar.Speed is set to 0
                tChar.velY = 0;
                tChar.velX = 0;
                // Rotate!
                tChar.RotateTo = tChar.RotateFrom + 90;
            }
            else if (T == BlockID.GravLeft)
            {
                tChar.SetMode("freeze");
                tChar.velY = 0;
                tChar.velX = 0;
                tChar.RotateTo = tChar.RotateFrom - 90;
            }
            else if (T == BlockID.Vanish)
                Vanish();
            else if (T == BlockID.Time && !Used[tChar.tempID])
            {
                if (course.max_time == 0)
                {
                    course.Frames += 10 * 27;
                }
                else
                {
                    course.max_time += 10;
                }
                Used[tChar.tempID] = true;
            }
            else if (T == BlockID.Heart)
            {
                if (!Used[tChar.tempID])
                {
                    Used[tChar.tempID] = true;
                    tChar.HurtTimer = 27 * 5; // should use current frame rate, not 27
                    tChar.Invincible = true;
                }
            }
        }

        public void HitLeft(LocalCharacter tChar)
        {
            int BlocX = X * 30;
            int BlocY = Y * 30;
            // Rotate block coordinates (to determine where to place character)
            General.RotatePoint(ref BlocX, ref BlocY, tChar.Rotation);
            if (tChar.RotateFrom == 90 || tChar.RotateFrom == 180)
                BlocX -= 30;

            if (T == BlockID.Crumble)
            {
                double _loc_8 = Math.Floor(tChar.velX * 1.75 + 0.5);
                _loc_8 = Math.Floor(_loc_8 / 4);
                Health -= (int)_loc_8;
                if (Health <= 0)
                {
                    T = 99;
                    course.DeleteBlock(X, Y);
                    return;
                }
            }

            tChar.X = BlocX - LocalCharacter.spaceX;
            if (tChar.velX > 0)
                tChar.velX = tChar.velX * -0.05;
            if (tChar.TargetVel > 0)
                tChar.TargetVel = 0;

            if (GetArrowDir(tChar.RotateFrom) == BlockID.Down)
                tChar.velY += 5;
            else if (GetArrowDir(tChar.RotateFrom) == BlockID.Up)
            {
                if (!tChar.crouching)
                    tChar.velY -= 1.2;
            }
            else if (GetArrowDir(tChar.RotateFrom) == BlockID.Left)
                tChar.velX -= 3;
            else if (GetArrowDir(tChar.RotateFrom) == BlockID.Right)
                tChar.velX += 3;


            if (TurnedToIce)
                return;

            if (T == BlockID.Mine && FadeTime == 0)
            {
                HitChar(tChar);
                course.DeleteBlock(X, Y);
            }
            else if (T == BlockID.Push)
                GetPushed(1, 0, tChar);
            else if (T == BlockID.Vanish)
                Vanish();
        }

        public void HitRight(LocalCharacter tChar)
        {
            int BlocX = X * 30;
            int BlocY = Y * 30;
            // Rotate block coordinates (to determine where to place character)
            General.RotatePoint(ref BlocX, ref BlocY, tChar.Rotation);
            if (tChar.RotateFrom == 90 || tChar.RotateFrom == 180)
                BlocX -= 30;
            if (T == BlockID.Crumble)
            { // Crumble
                double _loc_8 = Math.Floor(-tChar.velX * 1.75 + 0.5);
                _loc_8 = Math.Floor(_loc_8 / 4);
                Health -= (int)_loc_8;
                if (Health <= 0)
                {
                    T = 99;
                    course.DeleteBlock(X, Y);
                    return;
                }
            }

            tChar.X = BlocX + 30 + LocalCharacter.spaceX;
            if (tChar.velX < 0)
                tChar.velX = tChar.velX * -0.05;
            if (tChar.TargetVel < 0)
                tChar.TargetVel = 0;

            if (GetArrowDir(tChar.RotateFrom) == BlockID.Down)
                tChar.velY += 5;
            else if (GetArrowDir(tChar.RotateFrom) == BlockID.Up)
            {
                if (!tChar.crouching)
                    tChar.velY -= 1.2;
            }
            else if (GetArrowDir(tChar.RotateFrom) == BlockID.Left)
                tChar.velX -= 3;
            else if (GetArrowDir(tChar.RotateFrom) == BlockID.Right)
                tChar.velX += 3;

            if (TurnedToIce)
                return;

            if (T == BlockID.Mine && FadeTime == 0)
            { // Mine
                HitChar(tChar);
                course.DeleteBlock(X, Y);
            }
            if (T == BlockID.Push)
                GetPushed(-1, 0, tChar);
            if (T == BlockID.Vanish)
                Vanish();
        }
        public void onTouch(LocalCharacter tChar)
        {
            if (!TurnedToIce)
            {
                if (T == BlockID.Water)
                {
                    if (tChar.TouchingGround == false && tChar.Mode != "freeze" && tChar.Mode != "hurt")
                    {
                        tChar.SetMode("water");
                        tChar.WaterTimer = 2;
                    }
                    else
                    {
                        tChar.TargetVel = tChar.TargetVel * 0.9;
                        tChar.Traction = 0.1;
                    }
                    tChar.SafeSegX = X;
                    tChar.SafeSegY = Y;
                    tChar.SafeX = X * 30 + 15;
                    tChar.SafeY = Y * 30 + 15;
                }
                else if (T == BlockID.Mine && FadeTime == 0)
                {
                    HitChar(tChar);
                    course.DeleteBlock(X, Y);
                }
                else if (T == BlockID.Net)
                {
                    //if (tChar.SafeSegX != X || Y > tChar.SafeSegY || Y + 2 < tChar.SafeSegY)
                    if (tChar.SafeSegX != X || tChar.SafeSegY < Y || tChar.SafeSegY > Y + 2)
                        tChar.Reappear();
                }
            }
            // Vanish blocks that are re-appearing should not fully re-appear while tChar are inside them TODO: (see Block.Vanish)
            if (T == BlockID.Vanish && Fade == INACTIVE && Y * 30 > tChar.Y - 60)
            {
                if (FadeTime > 0 && FadeTime < 10)
                    FadeTime += 1;
            }
        }

        private int GetArrowDir(int rot)
        {
            int ret = T;
            if (rot == 90)
            {
                if (T == 5)
                    ret = 7;
                else if (T == 6)
                    ret = 8;
                else if (T == 7)
                    ret = 6;
                else if (T == 8)
                    ret = 5;
            }
            else if (rot == 180 || rot == -180)
            {
                if (T == 5)
                    ret = 6;
                else if (T == 6)
                    ret = 5;
                else if (T == 7)
                    ret = 8;
                else if (T == 8)
                    ret = 7;
            }
            else if (rot == -90)
            {
                if (T == 5)
                    ret = 8;
                else if (T == 6)
                    ret = 7;
                else if (T == 7)
                    ret = 5;
                else if (T == 8)
                    ret = 6;
            }
            return ret;
        }


        // Bombs
        private void HitChar(LocalCharacter tChar)
        {
            double TestX = tChar.X;
            double TestY = tChar.Y;

            double _loc_3 = TestX - ((X * 30.0) + 15.0);
            double _loc_4 = TestY - (LocalCharacter.spaceY / 2.0) - ((Y * 30.0) + 15.0);
            double _loc_5 = Math.Atan2(_loc_4, _loc_3);
            double _loc_6 = Math.Cos(_loc_5) * 50.0;
            double _loc_7 = Math.Sin(_loc_5) * 50.0;
            tChar.GetHit(_loc_6, _loc_7);
            //System.Diagnostics.Debug.Print(_loc_3.ToString("R"));
            //System.Diagnostics.Debug.Print(_loc_4.ToString("R"));
            //System.Diagnostics.Debug.Print(_loc_5.ToString("R"));
            //System.Diagnostics.Debug.Print(_loc_6.ToString("R"));
            //System.Diagnostics.Debug.Print(_loc_7.ToString("R"));

            General.PlaySound(General.sound_path + "sound 962 (ExplosionSound).wav");
        }
        private void GetPushed(int pushX, int pushY, LocalCharacter tChar)
        {
            General.RotatePoint(ref pushX, ref pushY, -tChar.RotateFrom);
            course.PushBlock(this, pushX, pushY);
        }
        public void Vanish()
        {
            // Fading in/out takes 10 frames, stays gone for 2.000 secs
            if (FadeTime == 0)
            {
                Health = 0; // So it will darken it
                FadeTime = 11; // 10 + one for late event
                Fade = Block.VANISHING;
                course.AddBlockEvent(this, Map.EVENT_VANISH);
            }
            else
            {
                // If it's fading in, fade out
                if (Fade == Block.REAPPEARING)
                {
                    // It's fading in, make it fade out.
                    Fade = Block.VANISHING;
                }
            }
        }

    }

}
