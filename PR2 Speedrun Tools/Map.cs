using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace PR2_Speedrun_Tools
{
    public class Map : IDisposable
    {
        // Normal (?) = 600, 540
        // PR2 = 540??, 500??
        // Video = 640, 480
        public Map(LocalCharacter tChar, int width = 540, int height = 405)
        {
            BlBit = new Bitsmap(width, height);
            BlockViewSize = new Size((int)Math.Ceiling(BlBit.Width / 30.0), (int)Math.Ceiling(BlBit.Height / 30.0));
            MG = Graphics.FromImage(BlBit.Bit);
            BlockI = General.BlockI.Clone() as Bitsmap[];

            // characters
            if (tChar != null)
            {
                MainChar = tChar;
                Chars = new Character[] { MainChar }.ToList();
                tChar.course = this;
            }
        }

        #region "Graphics"
        private Graphics MG;
        public Bitsmap BlBit;
        public Bitmap Image
        {
            get
            {
                return BlBit.Bit;
            }
        }

        public void ResizeView(int width, int height)
        {
            BlBit = new Bitsmap(width, height);
            BlockViewSize = new Size((int)Math.Ceiling(BlBit.Width / 30.0), (int)Math.Ceiling(BlBit.Height / 30.0));
            MG = Graphics.FromImage(BlBit.Bit);

            CamSnap = true;
            Draw();
            CamSnap = false;
        }

        // Camera
        public int CamX;
        public int CamY;
        private bool CamSnap = false;
        public void Draw()
        {
            if (!inLE)
                FollowCharacter(ref CamX, ref CamY);

            //if ( bgID > 0 ) {
            //    BlBit.DrawImage(BG(bgID), 0, 0)
            //}

            DrawBlocks();

            if (inLE)
            {
                DrawGrid();
                if (General.HQ) // BG fading (alpha value may be slightly off)
                    BlBit.FillRectangleA(Color.FromArgb(25, BGC.R, BGC.G, BGC.B), 0, 0, BlBit.Width, BlBit.Height);
            }
            else
            {
                DrawCharacters();
                if (General.HQ) // BG fading
                    BlBit.FillRectangleA(Color.FromArgb(25, BGC.R, BGC.G, BGC.B), 0, 0, BlBit.Width, BlBit.Height);
                DrawEffects();
                DrawInfos();
            }

            //// How to RaiseEvent on another thread?
            //if (General.formRef.InvokeRequired)
            //    General.formRef.Invoke(ReDraw);
            //else
            //    ReDraw.Invoke();
        }

        private Size BlockViewSize;
        private Bitsmap[] BlockI = General.BlockI;
        public void SetRotation(int rot)
        {
            BlockI = General.blockImgs.Clone() as Bitsmap[];
            rot = (rot / 90 + 4) % 4;
            CamSnap = true;
            RotateFlipType r = (RotateFlipType)(rot % 4);
            for (int i = 0; i < BlockI.Length; i++)
            {
                Bitmap temp = BlockI[i].Bit;
                temp.RotateFlip(r);
                BlockI[i] = new Bitsmap(temp);
            }
        }

        private const bool DrawCrumbleHealth = true;
        private const bool DrawVanishTimers = true;
        private const bool DrawFreezeTimers = true;
        private Font HealthFont = new Font(FontFamily.GenericSansSerif, 8.0f);
        private void DrawBlocks()
        {
            // BlBit is Bitsmap to draw to
            BlBit.Clear(BGC);

            // Find X, Y range that is in bounds of camera
            int minX = (int)Math.Floor(CamX / 30.0);
            int minY = (int)Math.Floor(CamY / 30.0);
            if (FollowChar.RotateFrom == 90)
                minX += 1;
            else if (FollowChar.RotateFrom == -90)
                minY += 1;
            else if (FollowChar.RotateFrom == 180 || FollowChar.RotateFrom == -180)
            {
                minX += 1;
                minY += 1;
            }

            int oX = (minX * 30) - CamX;
            int dY = (minY * 30) - CamY;
            if (FollowChar.RotateFrom == 90)
                oX -= 30;
            else if (FollowChar.RotateFrom == -90)
                dY -= 30;
            else if (FollowChar.RotateFrom == 180 || FollowChar.RotateFrom == -180)
            {
                oX -= 30;
                dY -= 30;
            }
            int dX = oX;

            for (int iY = minY; iY <= minY + BlockViewSize.Height; iY++)
            {
                for (int iX = minX; iX <= minX + BlockViewSize.Width; iX++)
                {
                    Block cBlock = getBlock(iX, iY, FollowChar.RotateFrom);
                    // Draw if block if visible
                    if (cBlock.T < 99 && (cBlock.T != BlockID.Vanish || cBlock.Fade != Block.INACTIVE))
                    {
                        dY -= (int)cBlock.BumpY;
                        if (cBlock.T == BlockID.Invisible)
                            BlBit.FillRectangleA(General.cGrayBlock, dX, dY, 30, 30);
                        else
                            BlBit.DrawImage(ref BlockI[cBlock.T], dX, dY);

                        // Darken for frozen, fading, etc.
                        if (cBlock.Health == 0 || cBlock.TurnedToIce || (cBlock.T == BlockID.Mine && cBlock.FadeTime > 0) || cBlock.Used[FollowChar.tempID])
                        {
                            if (General.HQ && cBlock.TurnedToIce)
                                BlBit.DrawImageA(ref General.BlockI[BlockID.Ice], dX, dY);
                            else
                                BlBit.FillRectangleA(General.cGrayBlock, dX, dY, 30, 30);
                        }
                        dY += (int)cBlock.BumpY;
                    }

                    if (DrawCrumbleHealth && cBlock.T == BlockID.Crumble && cBlock.Health != 10)
                        MG.DrawString(cBlock.Health.ToString(), HealthFont, Brushes.Black, dX + 3, dY + 5);
                    if (DrawVanishTimers && cBlock.T == BlockID.Vanish && cBlock.Health != 10)
                    {
                        MG.DrawString(cBlock.FadeTime.ToString(), HealthFont, Brushes.White, dX + 4, dY + 6);
                        MG.DrawString(cBlock.FadeTime.ToString(), HealthFont, Brushes.Black, dX + 3, dY + 5);
                    }
                    if (DrawFreezeTimers) {
                        if (cBlock.TurnedToIce) {
                            MG.DrawString(cBlock.FreezeTime.ToString(), HealthFont, Brushes.Black, dX + 11, dY + 13);
                        }
                        else if (cBlock.FreezeTime > 0) {
                            MG.DrawString(cBlock.FreezeTime.ToString(), HealthFont, Brushes.White, dX + 12, dY + 14);
                            MG.DrawString(cBlock.FreezeTime.ToString(), HealthFont, Brushes.Black, dX + 11, dY + 13);
                        }
                    }

                    dX += 30;
                }
                dY += 30;
                dX = oX;
            }
        }

        private void DrawCharacters()
        {
            foreach (LocalCharacter c in Chars)
                DrawCharacter(c);
        }

        public void DrawCharacter(LocalCharacter c)
        {
            Color YouColor = Color.Green;
            int YH = 55;
            if (c.crouching)
                YH = 40;
            if (c.SuperJumpVel > 25)
            {
                YouColor = Color.Yellow;
                YH = 55 - (int)((c.SuperJumpVel / 100.0) * 15);
            }
            if (c.HurtTimer > 0)
                YouColor = Color.Red;

            int relativeRotation;
            if (FollowChar == null)
                relativeRotation = c.RotateFrom;
            else
                relativeRotation = c.RotateFrom - FollowChar.RotateFrom;
            int DX = (int)(c.X + 0.5);
            int DY = (int)(c.Y + 0.5);
            General.RotatePoint(ref DX, ref DY, -relativeRotation);

            DX = DX - 10 - CamX;
            DY = DY - YH - CamY;
            if (FollowChar.RotateFrom == 90)
                DX -= 1;
            else if (FollowChar.RotateFrom == -90)
                DX += 1;
            else if (FollowChar.RotateFrom == 180 || FollowChar.RotateFrom == -180)
            {
                DX -= 1;
                DY -= 1;
            }
            int DW = 20;
            int DH = YH;
            if ((relativeRotation + 360) % 360 == 270)
                DX += 30;
            else if ((relativeRotation + 360) % 360 == 180)
            { }
            else if ((relativeRotation + 360) % 360 == 90)
                DX -= 30; // ??

            if (!General.HQ)
            {
                BlBit.FillRectangle(YouColor, DX, DY, DW, DH);
                // Hats
                for (int i2 = 0; i2 < c.Hats.Length; i2++)
                {
                    Bitsmap iHat = General.Hatpic[c.Hats[i2].ID];
                    BlBit.DrawImage(ref iHat, DX - 5, DY + 13 - iHat.Height - (8 * i2));
                }
            }
            else // HQ (when I say "HQ", I do NOT mean Headquarters)
            {
                byte alpha = 80;
                if (MainChar != c)
                    alpha = 50;
                BlBit.FillRectangleA(Color.FromArgb(alpha, YouColor), DX, DY, 20, YH);
                c.drawSelf(MG, DX + 10, DY + YH);
                if (MainChar == c)
                    MG.DrawRectangle(new Pen(Color.FromArgb(190, YouColor)), DX, DY, 20 - 1, YH - 1);
            }
        }

        private List<Laser> sparkles = new List<Laser>();
        public void AddSparkle(int X, int Y)
        {
            Laser L = new Laser();
            L.X = X;
            L.Y = Y;
            L.TTV = 255;
            sparkles.Add(L);
        }

        private void DrawEffects()
        {
            // Lasers
            for (int i = 0; i < lasers.Count; i++)
            {
                int xoff = 40;
                if (lasers[i].Dir == "left")
                    xoff = 0;
                BlBit.DrawImage(ref General.laserPic, (lasers[i].X - CamX - xoff), (lasers[i].Y - CamY - 8));
            }
            // Slashes
            for (int i = 0; i < slashes.Count; i++)
            {
                int xoff = 0;
                if (slashes[i].Dir == "left")
                    xoff = 50;
                BlBit.DrawImageA(ref General.slashPic, (slashes[i].X - CamX - xoff), (slashes[i].Y - CamY - 50));
            }
            // Zaps
            foreach (var zap in Zaps)
            {
                var bgBmp = new Bitmap(BlBit.Width, BlBit.Height);
                var bgG = Graphics.FromImage(bgBmp);
                var brush = new SolidBrush(Color.FromArgb((int)(255*zap.Alpha), Color.White));

                bgG.FillRectangle(brush, 0, 0, BlBit.Width, BlBit.Height);
                MG.DrawImage(bgBmp, (BlBit.Width/2)-CamX, (BlBit.Height/2)-CamY);

                //if (zap.I)
                //var zapPic = General.ZapPic.Clone();
                //zapPic.SetAlpha(zap.Alpha);
                //BlBit.DrawImageA(ref General.ZapPic, zap.X - CamX, zap.Y - CamY);

                bgBmp.Dispose();
                bgG.Dispose();
                brush.Dispose();
            }
            // Hats
            for (int i = 0; i < hats.Count; i++)
            {
                BlBit.DrawImage(ref General.Hatpic[hats[i].ID], (hats[i].X - CamX - 10), (hats[i].Y - CamY - 20));
            }
            // Sparkles!!
            if (General.HQ)
            {
                for (int i = 0; i < sparkles.Count; i++)
                {
                    Bitmap sparkle = new Bitmap(22, 20);
                    Graphics sG = Graphics.FromImage(sparkle);

                    sG.DrawImage(General.sparklePic.Bit, 0, 0);
                    sG.FillRectangle(/*dispose not called*/new SolidBrush(Color.FromArgb(255 - sparkles[i].TTV, BGC)), 0, 0, 22, 20);
                    sparkle.MakeTransparent(sparkle.GetPixel(0, 0));
                    MG.DrawImage(sparkle, sparkles[i].X - CamX, sparkles[i].Y - CamY);

                    sparkles[i].TTV -= 12;
                    sparkles[i].Y -= 1;
                    if (sparkles[i].TTV < 1)
                        sparkles.RemoveAt(i);

                    sG.Dispose();
                    sparkle.Dispose();
                }
            }

            
        }

        // FONTS
        private Font itemStuff = new Font("Arial", 12);
        private Font timeFont = new Font("Courier New", 14);
        private Font finishFont = new Font("Courier New", 12);
        private void DrawInfos()
        {
            // Item
            if (MainChar.cItem > 0)
                BlBit.DrawImageA(ref General.ItemB[MainChar.cItem], 5, 5);

            // Item uses/time
            MG.DrawString(MainChar.ItemUses + " (" + MainChar.ItemTime + ")", itemStuff, Brushes.Green, 3, 22);

            // Timer
            string timeStr;
            Brush timeColor = Brushes.White;
            if (max_time == 0)
            {
                timeStr = General.FramesToTime(Frames);
            }
            else
            {
                var framesLeft = (max_time * 27) - Frames;
                timeStr = General.FramesToTime(framesLeft);

                var secsLeft = framesLeft / 27;
                if (secsLeft < 30)
                    timeColor = Brushes.Red;
            }
            int dX = Image.Width - 7 - (int)MG.MeasureString(timeStr, timeFont).Width;
            BlBit.FillRectangleA(Color.FromArgb(96, 0, 0, 0), dX - 2, 3, Image.Width - dX - 8, 22);
            MG.DrawString(timeStr, timeFont, timeColor, dX - 2, 4);
            // Frames
            timeStr = "(" + Frames + ")";
            dX = Image.Width - 7 - (int)MG.MeasureString(timeStr, timeFont).Width;
            BlBit.FillRectangleA(Color.FromArgb(96, 0, 0, 0), dX, 27, Image.Width - dX - 8, 22);
            MG.DrawString(timeStr, timeFont, Brushes.White, dX, 28);

            // Player finish time
            for (int i = 0; i < Chars.Count; i++)
            {
                if (Chars[i].TimeStr == "")
                    continue;
                timeStr = Chars[i].Name + " - " + Chars[i].TimeStr;
                dX = (int)MG.MeasureString(timeStr, finishFont).Width;
                BlBit.FillRectangleA(Color.FromArgb(96, 0, 0, 0), 2, 43 + i * 25, dX + 4, 23);
                MG.DrawString(timeStr, finishFont, Brushes.White, 4, 45 + i * 25);
            }

            // Stats
            timeStr = "Stats: " + MainChar.SpStat.ToString().PadLeft(3) + " " +
                MainChar.AccStat.ToString().PadLeft(3) + " " + MainChar.JumpStat.ToString().PadLeft(3) +
                "  [Rank " + (MainChar.SpStat + MainChar.AccStat + MainChar.JumpStat - 150) + "]";
            dX = (int)MG.MeasureString(timeStr, finishFont).Width;
            BlBit.FillRectangleA(Color.FromArgb(60, 0, 0, 0), 2, Image.Height - 28, dX + 4, 23);
            MG.DrawString(timeStr, finishFont, Brushes.White, 4, Image.Height - 26);
        }

        private void DrawGrid()
        {
            // Axis
            if (MainChar.RotateFrom == 0)
            {
                MG.DrawLine(Pens.Red, -CamX, 0, -CamX, BlBit.Height);
                MG.DrawLine(Pens.Red, 0, -CamY, BlBit.Width, -CamY);
            }
            else if (Math.Abs(MainChar.RotateFrom) == 180)
            {
                MG.DrawLine(Pens.Red, -(CamX - 1), 0, -(CamX - 1), BlBit.Height);
                MG.DrawLine(Pens.Red, 0, -(CamY - 1), BlBit.Width, -(CamY - 1));
            }
            else if (MainChar.RotateFrom == -90)
            {
                MG.DrawLine(Pens.Red, -CamX, 0, -CamX, BlBit.Height);
                MG.DrawLine(Pens.Red, 0, -(CamY - 1), BlBit.Width, -(CamY - 1));
            }
            else if (MainChar.RotateFrom == 90)
            {
                MG.DrawLine(Pens.Red, -(CamX - 1), 0, -(CamX - 1), BlBit.Height);
                MG.DrawLine(Pens.Red, 0, -CamY, BlBit.Width, -CamY);
            }
            // Draw grid
            Pen BPen = new Pen(Brushes.Black, 1);
            int R = 0;
            int G = 0;
            int B = 0;
            if (BGC.R < 128)
            {
                R = 255;
            }
            if (BGC.G < 128)
            {
                G = 255;
            }
            if (BGC.B < 128)
            {
                B = 255;
            }
            BPen.Color = Color.FromArgb(77, R, G, B);
            double X = 0;
            double Y = 0;
            for (int i = 0; i <= BlockViewSize.Height; i++)
            {
                // Horizontal lines
                // off by CamY % 30
                Y = (i * 30) - CamY % (30);
                MG.DrawLine(BPen, 0, (int)Y, BlBit.Width, (int)Y);
            }
            for (int i = 0; i <= BlockViewSize.Width; i++)
            {
                X = (i * 30) - CamX % (30);
                MG.DrawLine(BPen, (int)X, 0, (int)X, BlBit.Height);
            }
        }
        #endregion

        // BlockData
        private StringBuilder BlockData = new StringBuilder("");
        private int pX = 0;
        private int pY = 0;
        private int pT = 0;
        // Block array(s)
        public List<List<Block>> Blocks = new List<List<Block>>();
        int XStart = 0;
        List<int> YStart = new List<int>();
        public int BlockCount = 0;

        int MinX = 0;
        int MaxX = 0;
        int MinY = 0;
        public int MaxY = 0;

        public Color BGC = Color.Black;
        public int bgID = 0;

        // Options
        public int song;
        public sbyte min_level;
        private double _Gravity = 1.0;
        public double Gravity
        {
            get
            {
                return _Gravity;
            }
            set
            {
                _Gravity = value;
                gravMod = value * 0.7;
            }
        }
        public int[] avItems = new int[0];
        public int max_time;
        public int cowboyChance;
        public bool hasPass = false;
        public string password;
        private bool useOldPass = false;
        public string gameMode = "race";
        public int finish_count = 0;

        // Other options
        public string Title;
        public string credits;
        public string note;
        /// <summary>
        /// 0 for un-published, 1 for published
        /// </summary>
        /// <remarks></remarks>
        public int live;
        public string userName;
        public int ID;
        public int userID;
        public int version;
        public uint timestamp;

        // Art
        public string[] ArtCodes = new string[10];

        // Options that are not in PR2
        public bool Mapless = false;
        public bool passImpossible = false;
        public double gravMod = 0.7;
        public bool inLE = true;
        public void exitLE(bool resetPlayers = true)
        {
            // Reload blocks
            LoadBlocks(BlockData.ToString());
            // Frame count
            Frames = 0;
            // Move blocks?
            General.ResetRNG();
            MoveTime = 27 * 3 - 2;
            // Clear all lasers, slashes, and hats
            lasers.Clear();
            slashes.Clear();
            PrevNum = 0;
            hats.Clear();
            // Now out of LE
            inLE = false;
            // Remove PlayerStart blocks (so other blocks can be moved/placed there)
            List<Point> sP = new List<Point>();
            for (int iX = 0; iX < Blocks.Count; iX++)
            {
                if (Blocks[iX] == null)
                    continue;
                for (int iY = 0; iY < Blocks[iX].Count; iY++)
                {
                    if (BlockExists(iX, iY, true))
                    {
                        Block cB = Blocks[iX][iY];
                        if (cB.T > 10 && cB.T < 15)
                        {
                            sP.Add(new Point(cB.X, cB.Y));
                        }
                    }
                }
            }
            for (int i = 0; i < sP.Count; i++)
            {
                DeleteBlock(sP[i].X, sP[i].Y);
            }
            // Characters
            if (resetPlayers)
            {
                for (int i = 0; i < Chars.Count; i++)
                {
                    if (Chars[i].GetType() == typeof(LocalCharacter))
                    {
                        LocalCharacter c = Chars[i] as LocalCharacter;
                        c.Reset(false);
                    }
                    else
                        Chars[i].Reset();
                }
            }
            CamSnap = true;
            if (MainChar != null)
                Draw();
            CamSnap = false;
        }

        public void enterLE()
        {
            inLE = true;
            SetRotation(0);

            // Reload blocks
            LoadBlocks(BlockData.ToString());

            Draw();
        }

        // Players
        public Point[] PStart = new Point[4];
        public List<Character> Chars = new List<Character>();
        public LocalCharacter MainChar;
        private int _FollowChar;
        public Character FollowChar
        {
            get
            {
                if (_FollowChar == -1)
                {
                    return MainChar;
                }
                return Chars[_FollowChar];
            }
            set
            {
                if (value == null)
                    _FollowChar = -1;
                else
                    _FollowChar = value.tempID;
            }
        }

        public void AddCharacter(Character c)
        {
            Chars.Add(c);
            c.tempID = (Chars.Count - 1) % 4;
            if (!inLE)
            {
                if (c.GetType() == typeof(LocalCharacter))
                    (c as LocalCharacter).Reset();
                else
                    c.Reset();
            }
        }

        // Frame
        public int Frames = 0;
        public void goFrame()
        {
            Frames += 1;
            // Level Editor
            if (inLE)
            {
                return;
            }
            // All characters go
            for (int i = 0; i < Chars.Count; i++)
            {
                if (Chars[i].GetType() == typeof(LocalCharacter))
                {
                    LocalCharacter c = Chars[i] as LocalCharacter;
                    c.goFrame();
                }
                else
                    Chars[i].goFrame();
            }
            // Handle: Move Blocks, Lasers, Slashes, Hats, Bomb placement
            // Move blocks
            MoveTime += 1;
            if (MoveTime == 162)
            { // 6 seconds is 162 frames
                MoveTime = 0;
                MoveBlocksGo();
            }
            // Other blocks
            HandleBlocks();

            // 'Effects'
            LaserGo();
            SlashGo();
            ZapGo();
            HatGo();

            // Sounds
            if (General.HQ)
            {
                if (Frames == 1 || Frames == 18 || Frames == 37)
                    General.PlaySound(General.sound_path + "sound 429 (ReadySound).wav");
                else if (Frames == 55)
                    General.PlaySound(General.sound_path + "sound 428 (GoSound).wav");
            }

            if (max_time > 0 && max_time * 27 <= Frames)
            { // Out of time
                MainChar.Mode = "end";
            }
        }

        // Handle move blocks. This is so weird.
        private List<Point> MLoc = new List<Point>();
        public int MoveTime = 0;
        public void MoveBlocksGo()
        {
            for (int i = 0; i < MLoc.Count; i++)
            {
                int DX = 0;
                int DY = 0;
                int randDir = General.GetNextMove();
                if (randDir == 3) // LEFT
                    DX = -1;
                else if (randDir == 2) // RIGHT
                    DX = 1;
                else if (randDir == 1) // UP
                    DY = -1;
                else if (randDir == 0) // DOWN
                    DY = 1;

                PushBlock(getBlock(MLoc[i].X, MLoc[i].Y, 0), DX, DY);
            }
        }

        // Handle vanishing and such
        public delegate void Ticker();
        private List<Ticker> faders = new List<Ticker>();
        public const int EVENT_VANISH = 0;
        public const int EVENT_PLACE = 1;
        public const int EVENT_THAW = 2;
        public const int EVENT_BUMP = 3;
        public const int EVENT_REFRZ = 4;
        public void AddBlockEvent(Block B, int t)
        {
            Ticker toAdd = GetEvent(B, t);
            if (!faders.Contains(toAdd))
            {
                faders.Add(toAdd);
            }
        }

        public void RemoveBlockEvent(Block B, int t)
        {
            Ticker toRemove = GetEvent(B, t);
            int index = faders.IndexOf(toRemove);
            if (index != -1)
            {
                faders.RemoveAt(index);
            }
        }

        private Ticker GetEvent(Block B, int t)
        {
            if (t == EVENT_VANISH)
            {
                return B.goVanish;
            }
            else if (t == EVENT_PLACE)
            {
                return B.Place;
            }
            else if (t == EVENT_THAW)
            {
                return B.Thaw;
            }
            else if (t == EVENT_BUMP)
            {
                return B.Bumping;
            }
            else if (t == EVENT_REFRZ)
            {
                return B.ReFreezeable;
            }
            return null;
        }

        private void HandleBlocks()
        {
            if (faders.Count == 0)
                return;
            int lastCount = faders.Count;
            int i = 0;
            do
            {
                faders[i].Invoke();
                if (lastCount != faders.Count)
                {
                    lastCount = faders.Count;
                    i -= 1;
                }
                i += 1;
            } while (i < lastCount);
        }

        // Zaps
        public List<Zap> Zaps = new List<Zap>();
        private void ZapGo()
        {
            for (int iZ = 0; iZ < Zaps.Count; iZ++)
            {
                foreach (var character in Chars)
                {
                    if (character is LocalCharacter lc)
                        lc.GetZapped(Zaps[iZ].ID);
                }

                if (Zaps[iZ].Alpha < .1f)
                    Zaps.RemoveAt(iZ--);
                else
                {
                    Zaps[iZ].Alpha -= .1f;
                    Zaps[iZ].X = (int)Chars[Zaps[iZ].ID].X;
                    Zaps[iZ].Y = (int)Chars[Zaps[iZ].ID].Y;
                }
            }
        }

        public void MakeZap(int x, int y, int pID)
        {
            var zap = new Zap();
            zap.X = x;
            zap.Y = y;
            zap.ID = pID;
            Zaps.Add(zap);
        }

        // Lasers
        public List<Laser> lasers = new List<Laser>();
        private void LaserGo()
        {
            if (lasers.Count == 0)
                return;
            int iL = 0;
            do
            {
                // check for collisions
                bool hitChar = false;
                for (int iC = 0; iC < Chars.Count; iC++)
                {
                    if (lasers[iL].ID != iC)
                    {
                        if (laserTouchingChar(lasers[iL], Chars[iC]))
                        {
                            hitChar = true;
                            // Hurt localChar
                            if (Chars[iC].GetType() == typeof(LocalCharacter))
                            {
                                int vX = -29;
                                if (lasers[iL].Dir == "right")
                                    vX = 29;
                                (Chars[iC] as LocalCharacter).GetHit(vX, 0);
                            }
                        }
                    }
                }
                Block Bloc = getBlock(lasers[iL].X, lasers[iL].Y, lasers[iL].rot, true);
                if (Bloc.IsSolid())
                {
                    // blow up mines, hit bricks
                    if (Bloc.T == 9 || Bloc.T == 4)
                        DeleteBlock(Bloc.X, Bloc.Y);
                    hitChar = true;
                }

                // Disappear or move
                if (hitChar)
                {
                    lasers.RemoveAt(iL);
                    iL -= 1;
                }
                else
                {
                    if (lasers[iL].Dir == "right")
                        lasers[iL].X += 29;
                    else
                        lasers[iL].X -= 29;

                    // time to vanish
                    lasers[iL].TTV -= 1;
                    if (lasers[iL].TTV == 0)
                    {
                        // remove laser
                        lasers.RemoveAt(iL);
                        iL -= 1;
                    }
                }

                iL += 1;
            } while (iL < lasers.Count);
        }

        private bool laserTouchingChar(Laser las, Character tChar)
        {
            int r = 0;
            int l = 33;
            if (las.Dir == "right")
            {
                r = 44;
                l = 0;
            }

            if (las.X - r < tChar.X + 11 && las.X + l > tChar.X - 11)
            {
                if (las.Y < tChar.Y && las.Y > tChar.Y - 57)
                {
                    return true;
                }
            }

            return false;
        }

        public void MakeLaser(int X, int Y, int ID, string Dir, int rot)
        {
            Laser nL = new Laser();
            nL.X = X;
            nL.Y = Y;
            nL.Dir = Dir;
            nL.ID = ID;
            nL.rot = rot;
            lasers.Add(nL);
        }

        // Waves
        public List<Laser> Waves = new List<Laser>();

        public void MakeWave(int x, int y, int id, string dir, int rot)
        {
            Laser nL = new Laser();
            nL.X = x;
            nL.Y = y;
            nL.Dir = dir;
            nL.ID = id;
            nL.rot = rot;
            Waves.Add(nL);
        }

        // Slashes
        public List<Slash> slashes = new List<Slash>();
        int PrevNum;
        private void SlashGo()
        {
            // remove all slashes from previous frame
            // (not removed earlier for draws)
            if (PrevNum > 0)
            {
                do
                {
                    slashes.RemoveAt(0);
                    PrevNum -= 1;
                } while (PrevNum != 0);
            }
            PrevNum = slashes.Count;
            for (int i = 0; i < slashes.Count; i++)
            {
                // check for hitting you (don't need to check for others!)
                if (slashes[i].ID != MainChar.tempID)
                {
                    if (slashTouchingchar(slashes[i], MainChar))
                    {
                        int vX = -29;
                        if (slashes[i].Dir == "right")
                            vX = 29;
                        MainChar.GetHit(vX, -14);
                    }
                }
                // check for hitting blocks (mines, bricks, !ERROR! what else?)
                int xoff = -58;
                if (slashes[i].Dir == "right")
                    xoff = 28;
                Point[] hitsAt = new Point[] { new Point(slashes[i].X + xoff, slashes[i].Y - 10), new Point(slashes[i].X + xoff + 30, slashes[i].Y - 10),
					new Point(slashes[i].X + xoff, slashes[i].Y - 40), new Point(slashes[i].X + xoff + 30, slashes[i].Y - 40) };
                for (int iB = 0; iB < 4; iB++)
                {
                    Block Bloc = getBlock(hitsAt[iB].X, hitsAt[iB].Y, slashes[i].rot, true);
                    // blow up mines, hit bricks
                    if (Bloc.T == 9 || Bloc.T == 4)
                        DeleteBlock(Bloc.X, Bloc.Y);
                    else if (Bloc.T == BlockID.Vanish)
                        Bloc.Vanish();
                    else if (Bloc.T == BlockID.Crumble)
                        Bloc.Health -= 10;
                }
            }
        }

        private bool slashTouchingchar(Slash sla, Character tChar)
        {
            int r = 0;
            int l = 58;
            if (sla.Dir == "right")
            {
                l = 0;
                r = 58;
            }

            if (sla.X - l < tChar.X + 11 && sla.X + r > tChar.X - 11)
            {
                if (sla.Y < tChar.Y && sla.Y + 47 > tChar.Y - 56)
                {
                    return false;
                }
            }

            return true;
        }

        public void MakeSlash(int X, int Y, int ID, string Dir, int rot)
        {
            Slash nS = new Slash();
            nS.X = X;
            nS.Y = Y;
            nS.Dir = Dir;
            nS.ID = ID;
            nS.rot = rot;
            slashes.Add(nS);
            // Should hit mines immediately
            // check for hitting blocks (mines, bricks, !ERROR! what else?)
            int xoff = -58;
            if (nS.Dir == "right")
                xoff = 28;
            Point[] hitsAt = new Point[] { new Point(nS.X + xoff, nS.Y - 10), new Point(nS.X + xoff + 30, nS.Y - 10),
				new Point(nS.X + xoff, nS.Y - 40), new Point(nS.X + xoff + 30, nS.Y - 40) };
            for (int iB = 0; iB < 4; iB++)
            {
                Block Bloc = getBlock(hitsAt[iB].X, hitsAt[iB].Y, nS.rot, true);
                // blow up mines, hit bricks
                if (Bloc.T == 9 || Bloc.T == 4)
                    DeleteBlock(Bloc.X, Bloc.Y);
            }

        }

        // Hats
        public List<Hat> hats = new List<Hat>();
        private void HatGo()
        {
            if (hats.Count == 0)
                return;
            int i = 0;
            do
            {
                hats[i].VelY += 0.2;
                if (hats[i].VelY > 8)
                {
                    hats[i].VelY = 8;
                }
                hats[i].Y += (int)hats[i].VelY;

                // check for blocks
                Block Bloc = getBlock(hats[i].X, hats[i].Y, hats[i].rot, true);
                if (Bloc.IsSolid())
                {
                    if (hats[i].Y < Bloc.Y * 30 + 15)
                        hats[i].Y = Bloc.Y * 30;
                    else if (hats[i].VelY < 0)
                        hats[i].VelY = 0;

                    //int x = Bloc.X;
                    //int y = Bloc.Y;
                    //General.RotatePoint(ref x, ref y, hats[i].rot);
                    //if (hats[i].VelY < 0) {
                    //    hats[i].VelY *= -0.5;
                    //    hats[i].Y = y + 31;
                    //}
                    //else {
                    //    hats[i].VelY = 0;
                    //    hats[i].Y = y;
                    //}
                }
                // check for character
                if (hatTouchingChar(hats[i], MainChar))
                { // TODO: do hats go away when you see another touch it? I think not...
                    // remove hat
                    // give hat to you
                    Array.Resize(ref MainChar.Hats, MainChar.Hats.Length + 1);
                    MainChar.Hats[MainChar.Hats.Length - 1] = hats[i];
                    hats.RemoveAt(i);
                    i -= 1;
                }
                i += 1;
            } while (i < hats.Count);
        }

        private bool hatTouchingChar(Hat hat, LocalCharacter tChar)
        {
            if (hat.X - 20 < tChar.X + 11 && hat.X + 20 > tChar.X - 11) {
                if (hat.Y - 20 < tChar.Y && hat.Y + 20 > tChar.Y - 56) {
                    return true;
                }
            }

            return false;

            //if (Math.Abs(tChar.X - hat.X) < 25 && tChar.Y > hat.Y - 5 
            //    && (!tChar.crouching && tChar.Y < hat.Y + 65 || tChar.crouching && tChar.Y < hat.Y + 25)) {
            //    return true;
            //}
            //return false;
        }

        public void MakeHat(int X, int Y, int ID, Color clr, int ServID, int rot)
        {
            Hat nH = new Hat();
            nH.X = X;
            nH.Y = Y;
            nH.ID = ID;
            nH.Color = clr;
            nH.ServID = ServID;
            nH.rot = rot;
            hats.Add(nH);
        }

        // Are you off the course?
        public bool OffCourse(LocalCharacter tChar)
        {
            if (tChar.RotateFrom == 90)
            {
                if (tChar.Y > MaxX * 30 + 500)
                    return true;
            }
            else if (tChar.RotateFrom == 180)
            {
                if (tChar.Y > -MinY * 30 + 500)
                    return true;
            }
            else if (tChar.RotateFrom == -90)
            {
                if (tChar.Y > -MinX * 30 + 500)
                    return true;
            }
            else
            {
                if (tChar.Y > MaxY * 30 + 500)
                    return true;
            }
            return false;
        }

        // Camera follow character
        public void FollowCharacter(ref int CamX, ref int CamY)
        {
            if (CamSnap)
            {
                CamX = (int)FollowChar.X - (BlBit.Width / 2);
                CamY = (int)FollowChar.Y - 45 - (BlBit.Height / 2);
                CamSnap = false;
            }

            double ChX = (FollowChar.X - CamX - (BlBit.Width * 0.5)) / 3;
            if (Math.Abs(ChX) < 1)
                ChX = 0;
            if (ChX > 0)
                ChX = Math.Ceiling(ChX);
            else
                ChX = Math.Floor(ChX);

            double ChY = (FollowChar.Y - 45 - CamY - (BlBit.Height * 0.5)) / 3;
            if (Math.Abs(ChY) < 1)
                ChY = 0;
            if (ChY > 0)
                ChY = Math.Ceiling(ChY);
            else
                ChY = Math.Floor(ChY);

            CamX += (int)ChX;
            CamY += (int)ChY;
        }

        // Add a block to the level (increase BlockCount)
        public void AddBlock(int X, int Y, int T)
        {
            CreateIndex(X, Y);
            // So I will not have to type X - XStart over and over
            int BX = X - XStart;
            int BY = Y - YStart[BX];

            // Increase BlockCount
            BlockCount += 1;

            // Add block
            Blocks[BX][BY] = new Block();
            // Now to set the block! :D
            Blocks[BX][BY].X = X;
            Blocks[BX][BY].Y = Y;
            Blocks[BX][BY].T = T;
            Blocks[BX][BY].course = this;
            Blocks[BX][BY].mapID = BlockCount - 1;

            // Check if this is a new min/max
            if (X < MinX)
                MinX = X;
            else if (X > MaxX)
                MaxX = X;
            if (Y < MinY)
                MinY = Y;
            else if (Y > MaxY)
                MaxY = Y;

            if (inLE)
            {
                // Move block?
                if (T == BlockID.Move)
                {
                    MLoc.Add(new Point(X, Y));
                }
                // Add to level's BlockData
                string addStr = "";
                if (pT != T)
                {
                    addStr = ";" + T;
                    pT = T;
                    if (pY == Y)
                        addStr = ";" + addStr;
                }
                if (pY != Y)
                {
                    addStr = ";" + (Y - pY) + addStr;
                    pY = Y;
                }
                if (pX != X)
                {
                    addStr = (X - pX) + addStr;
                    pX = X;
                }
                addStr = "," + addStr;
                if (BlockData.Length == 0)
                    BlockData = new StringBuilder(addStr.Substring(1));
                else
                    BlockData.Append(addStr);
                // Player starts
                if (T >= BlockID.P1 && T <= BlockID.P4)
                {
                    int psID = T - BlockID.P1;
                    PStart[psID] = new Point(X * 30 + 15, Y * 30 + 15);
                }
                // Finish count for objective
                if (T == BlockID.Finish)
                    finish_count += 1;
            }
        }

        // Delete a block (Decreases BlockCount)
        public void DeleteBlock(int X, int Y)
        {
            if (inLE)
            { // if it's a move block, remove it's MLoc
                if (getBlock(X, Y, 0).T == BlockID.Move)
                {
                    for (int i = MLoc.Count - 1; i >= 0; i--)
                    {
                        if (MLoc[i].X == X && MLoc[i].Y == Y)
                        {
                            MLoc.RemoveAt(i);
                        }
                    }
                }
            }

            // Block array
            Block delBlock;
            X = X - XStart;
            Y = Y - YStart[X];
            delBlock = Blocks[X][Y];
            Blocks[X][Y] = null; // Set to nothing
            // if it is at YStart or at the end of the Y's, change array.
            if (Y == 0)
            {
                // if this is the ONLY Y, array = nothing
                if (Blocks[X].Count == 1)
                    Blocks[X] = null;
                else
                {
                    Blocks[X].RemoveAt(0);
                    // Remove all nothings from the array until the first block
                    if (Blocks[X][0] == null)
                    {
                        do
                        {
                            Blocks[X].RemoveAt(0);
                        } while (Blocks[X][0] == null);
                    }
                    // This is the new YStart.
                    YStart[X] = Blocks[X][0].Y;
                }
            }
            else if (Y == Blocks[X].Count - 1)
            {
                Blocks[X].RemoveAt(Blocks[X].Count - 1);
                // Remove all nothings from the array until the last block
                if (Blocks[X][Blocks[X].Count - 1] == null)
                {
                    do
                    {
                        Blocks[X].RemoveAt(Blocks[X].Count - 1);
                    } while (Blocks[X][Blocks[X].Count - 1] == null);
                }
            }
            // if this X is now null, and
            // if we are at XStart or the end of the X's, change array.
            if (Blocks[X] == null)
            {
                if (X == 0)
                {
                    // if this is the ONLY X, reset Lists
                    if (Blocks.Count == 1)
                    {
                        Blocks = new List<List<Block>>();
                        YStart = new List<int>();
                    }
                    else
                    {
                        // Remove all nothings from the list until the first 
                        do
                        {
                            Blocks.RemoveAt(0);
                            YStart.RemoveAt(0);
                        } while (Blocks[0] == null);
                        // new XStart
                        XStart = Blocks[0][0].X;
                    }
                }
                else if (X == Blocks.Count - 1)
                {
                    // Remove all nothings from the list until the last block
                    do
                    {
                        YStart.RemoveAt(Blocks.Count - 1);
                        Blocks.RemoveAt(Blocks.Count - 1);
                    } while (Blocks[Blocks.Count - 1] == null);
                }
            }

            if (inLE) // level data
            {
                BlockCount -= 1;
                int mID = delBlock.mapID;
                // Find and remove data from BlockData
                List<string> BlockDs = BlockData.ToString().Split(',').ToList();
                // Find block's offset X,Y
                Block offP = dataToBlock(BlockDs[mID]);
                // Add block's offset X,Y to next block's X,Y
                if (mID < BlockDs.Count - 1)
                {
                    Block nP = dataToBlock(BlockDs[mID + 1]);
                    nP.X += offP.X;
                    nP.Y += offP.Y;
                    // Block type
                    if (offP.T != -1 && nP.T == -1)
                    {
                        nP.T = offP.T;
                        BlockDs[mID + 1] = string.Join(";", new string[] { nP.X.ToString(), nP.Y.ToString(), nP.T.ToString() });
                    }
                    else
                    {
                        if (nP.T == -1)
                            BlockDs[mID + 1] = nP.X + ";" + nP.Y;
                        else
                            BlockDs[mID + 1] = nP.X + ";" + nP.Y + ";" + nP.T;
                    }
                }
                else
                {
                    // Just remove offP from pX,pY
                    pX -= offP.X;
                    pY -= offP.Y;
                    if (offP.T != -1)
                        pT = -1;
                }
                BlockDs.RemoveAt(mID);
                BlockData = new StringBuilder(string.Join(",", BlockDs));
                // Change the mapID of all blocks after deleted block!??!
                decMapID(mID);
            }
        }

        private Block dataToBlock(string bData)
        {
            string[] p = bData.Split(';');
            Block ret = new Block();
            if (p.Length > 0 && p[0] != "")
                ret.X = Convert.ToInt32(p[0]);
            if (p.Length > 1 && p[1] != "")
                ret.Y = Convert.ToInt32(p[1]);
            if (p.Length > 2 && p[2] != "")
                ret.T = Convert.ToInt32(p[2]);
            else
                ret.T = -1;

            return ret;
        }

        private void decMapID(int fromID)
        {
            for (int iX = 0; iX < Blocks.Count; iX++)
            {
                if (Blocks[iX] == null)
                    continue;
                for (int iY = 0; iY < Blocks[iX].Count; iY++)
                {
                    if (Blocks[iX][iY] == null)
                        continue;

                    if (Blocks[iX][iY].mapID > fromID)
                        Blocks[iX][iY].mapID -= 1;
                }
            }
        }

        // Move a block - do not delete/place. (placing can change off-course bounds)
        private void MoveBlock(int X, int Y, int MovX, int MovY)
        {
            Block Bloc = getBlock(X, Y, 0);
            if (Bloc.T != 99) // Movers in spaces w/o a block
            {
                if (!BlockExists(X + MovX, Y + MovY, false))
                {
                    // Make sure the index it's moving to exists before moving it
                    CreateIndex(X + MovX, Y + MovY);
                    // Get blocks at old position and at new position
                    Block newB = Blocks[X - XStart][Y - YStart[X - XStart]];
                    // MOVE IT
                    newB.X += MovX;
                    newB.Y += MovY;
                    Blocks[newB.X - XStart][newB.Y - YStart[newB.X - XStart]] = newB;
                    Blocks[X - XStart][Y - YStart[X - XStart]] = null;
                    // Trim arrays
                    DeleteBlock(X, Y);
                    // Move block's MLoc
                    if (newB.T == BlockID.Move)
                    {
                        int index = MLoc.LastIndexOf(new Point(X, Y));
                        MLoc[index] = new Point(MLoc[index].X + MovX, MLoc[index].Y + MovY);
                    }
                }
            }
        }

        // Push a block
        public void PushBlock(Block Bloc, int X, int Y)
        {
            // if there's another push block in the way, (try to) push it first.
            Block pushTo = getBlock(Bloc.X + X, Bloc.Y + Y, 0, false);
            if (pushTo.T == BlockID.Push)
                PushBlock(pushTo, X, Y);
            // see if this one can be pushed.
            if (getBlock(Bloc.X + X, Bloc.Y + Y, 0, false).T == 99 && !PlayerInside(Bloc.X + X, Bloc.Y + Y))
            {
                MoveBlock(Bloc.X, Bloc.Y, X, Y);
            }
        }

        // Place a bomb
        public void PlaceBomb(int X, int Y)
        {
            // TODO: Get correct placement position
            Y -= 24;

            Block Bloc = getBlock(X, Y, 0, true);
            if (Bloc.T == 99)
            {
                AddBlock(Bloc.X, Bloc.Y, 9);
                Bloc = getBlock(X, Y, 0, true);
                Bloc.FadeTime = 28; // One second +1 for late event TODO: Verify
                AddBlockEvent(Bloc, EVENT_PLACE);
            }
        }

        // Make sure a given index exists in the Blocks list
        private void CreateIndex(int X, int Y)
        {
            int BX = X - XStart;
            // if ( this is for the very first block
            if (Blocks.Count == 0 || Blocks[0] == null)
            {
                XStart = X;
                BX = X - XStart;
                Blocks = new List<List<Block>>();
                Blocks.Add(new List<Block>());
                YStart = new List<int>();
                YStart.Add(Y);
            }
            // Check if X is out of bounds
            if (BX < 0)
            { // new X is less than XStart
                for (int i = BX; i < 0; i++)
                {
                    Blocks.Insert(0, null);
                    YStart.Insert(0, 0);
                }
                XStart = X;
            }
            else if (BX > Blocks.Count - 1)
            { // X is higher than max X in the array
                for (int i = Blocks.Count; i <= BX; i++)
                {
                    Blocks.Add(null);
                    YStart.Add(Y);
                }
            }
            // BX could have changed - add a BY
            BX = X - XStart;
            int BY = Y - YStart[BX];
            // Make sure list is not nothing
            if (Blocks[BX] == null)
            {
                Blocks[BX] = new List<Block>();
                YStart[BX] = Y;
                BY = 0;
            }
            // Check if Y is out of bounds
            if (BY < 0)
            { // new Y is less than YStart
                for (int i = BY; i < 0; i++)
                    Blocks[BX].Insert(0, null);
                YStart[BX] = Y;
            }
            else if (BY > Blocks[BX].Count - 1)
            { // Y is higher than max Y in the array
                for (int i = Blocks[BX].Count; i <= BY; i++)
                {
                    Blocks[BX].Add(null);
                }
            }
        }

        // Check if a block exists
        public bool BlockExists(int X, int Y, bool IndexR)
        {
            // get the index.
            if (IndexR == false)
            {
                X -= XStart;
            }
            // See if that X exists.
            if (X < 0 || X >= Blocks.Count)
            {
                // It does not.
                return false;
            }
            else if (Blocks[X] == null)
            {
                // if it's in bounds, the array can still be a nothing
                return false;
            }
            // Y index, and Y exist
            if (IndexR == false)
            {
                Y -= YStart[X];
            }
            if (Y < 0 || Y >= Blocks[X].Count)
            {
                return false;
            }
            else if (Blocks[X][Y] == null)
            {
                return false;
            }
            // It exists if it doesn't not.
            return true;
        }

        // get Block by it's real X and Y
        public Block getBlock(int LocX, int LocY, int Rot, bool Pixels = false)
        {
            // Un-rotate coordinates
            General.RotatePoint(ref LocX, ref LocY, -Rot);
            // Pixels to Blocks
            if (Pixels)
            {
                LocX = (int)Math.Floor(LocX / 30.0);
                LocY = (int)Math.Floor(LocY / 30.0);
            }
            // get the block
            Block TheBlock;
            // if ( the block exists, return it. Otherwise return a 99
            if (BlockExists(LocX, LocY, false))
            {
                TheBlock = Blocks[LocX - XStart][LocY - YStart[LocX - XStart]];
            }
            else
            {
                TheBlock = new Block();
                TheBlock.X = LocX;
                TheBlock.Y = LocY;
                TheBlock.T = 99;
            }
            return TheBlock;
        }

        // Is a player in a given block space?
        private bool PlayerInside(int x, int y)
        {
            for (int i = 0; i < Chars.Count; i++)
            {
                if (Chars[i].X >= x * 30 && Chars[i].X - 30 <= x * 30
                    && Chars[i].Y >= y * 30 && Chars[i].Y - 30 <= y * 30)
                    return true;
            }
            return false;
        }

        // get the level's data
        public string GetUploadData(bool Hsh, bool keepArt = true)
        {
            string Data = GetDataParam();
            string Items = "";
            for (int i = 0; i < avItems.Length; i++)
                Items += avItems[i] + "`";

            if (Items.Length > 0)
                Items = Items.Substring(0, Items.Length - 1);

            // Hash?
            string H = "0000";
            if (Hsh)
                H = General.GenerateHash(Title + userName.ToLower() + Data + "******");

            // Put all the data bits into one string
            string LData = "credits=" + credits + "&live=" + live + "&max_time=" + max_time;
            LData += "&items=" + Items + "&title=" + Title + "&gravity=" + Gravity.ToString("R") + "&hash=";
            LData += H + "&data=" + Data + "&note=" + note + "&min_level=" + min_level + "&song=";
            if (song > -1)
            {
                LData += song;
            }

            // Password
            int HPass = 0;
            H = "";
            if (hasPass)
            {
                HPass = 1;
                H = General.GenerateHash(password + "******");
                if (useOldPass)
                {
                    H = "";
                }
            }
            if (passImpossible)
            {
                HPass = 1;
                H = "383";
            }
            LData += "&hasPass=" + HPass + "&passHash=" + H + "&gameMode=" + gameMode
                + "&cowboyChance=" + cowboyChance;

            return LData;
        }

        public string GetDownloadData(bool Hash = true)
        {
            // Download hash is dependent on the order of things. So, by convention, we will use:
            // level_ID, version, time, user_ID, title, note, credits
            // min_level, has_pass, live, gameMode, data, items
            // song, max_time, gravity, cowboyChance
            string ret = "level_ID=" + ID + "&version=" + version + "&time=" + timestamp + "&user_ID=" + userID +
                "&title=" + Title + "&note=" + note + "&credits=" + credits + "&min_level=" + min_level +
                "&has_pass=" + (hasPass ? 1 : 0) + "&live=" + live + "&gameMode=" + gameMode + "&data=" + GetDataParam() +
                "&items=" /* todo */ + "&song=" + song + "&max_time=" + max_time + "&gravity=" + Gravity +
                "&cowboyChance=" + cowboyChance;

            if (Hash)
                ret += GetDownloadHash(ret);
            return ret;
        }

        // Load level
        public void LoadLevel(string LvlData)
        {
            if (!LvlData.Substring(LvlData.Length - 32).Contains('&') && !LvlData.Substring(LvlData.Length - 32).Contains('`'))
                LvlData = LvlData.Substring(0, LvlData.Length - 32); // Remove hash at end of level code, if present

            string[] Parts = LvlData.Split('&');

            string levelData = "";
            string SongStr = "";
            string ItemStr = "1`2`3`4`5`6`7`8`9";
            gameMode = "race";
            max_time = 120;
            Gravity = 1;
            cowboyChance = 5;
            for (int i = 0; i < Parts.Length; i++)
            {
                int e = Parts[i].IndexOf('=');
                string LD = Parts[i].Substring(e + 1);
                string pName = Parts[i].Substring(0, e);
                switch (pName.ToLower())
                {
                    case "credits":
                        credits = LD;
                        break;
                    case "title":
                        Title = LD;
                        break;
                    case "data":
                        levelData = LD;
                        break;
                    case "note":
                        note = LD;
                        break;
                    case "min_level":
                        min_level = 0;
                        sbyte.TryParse(LD, out min_level);
                        break;
                    case "gravity":
                        Gravity = double.Parse(LD);
                        break;
                    case "max_time":
                        if (LD != "NaN")
                            max_time = Convert.ToInt32(LD);
                        break;
                    case "song":
                        SongStr = LD;
                        break;
                    case "items":
                        ItemStr = LD;
                        break;
                    case "gameMode":
                        gameMode = LD;
                        break;
                    case "cowboyChance":
                        cowboyChance = Convert.ToInt32(LD);
                        break;
                    case "has_pass":
                        if (LD == "0")
                            hasPass = false;
                        else if (LD == "1")
                            hasPass = true;
                        else
                            hasPass = Convert.ToBoolean(LD);
                        if (hasPass)
                        {
                            password = "";
                            useOldPass = true;
                        }
                        break;
                    case "level_id":
                        ID = int.Parse(LD);
                        break;
                    case "version":
                        version = int.Parse(LD);
                        break;
                    case "time":
                        timestamp = uint.Parse(LD);
                        break;
                    case "user_ID":
                        userID = int.Parse(LD);
                        break;
                }
            }

            // Music isn't always user-set
            if (SongStr != "" && SongStr != "random")
                song = Convert.ToInt32(SongStr);
            else
                song = -1; // PR2's internal value is unknown. It is not -1.

            string[] Codes = levelData.Split('`');
            // Set arts 1-3
            for (int i = 0; i < 6; i++)
            {
                ArtCodes[i] = Codes[i + 3];
            }
            string[] rgb;
            if (Codes[1].Length >= 6)
            {
                rgb = new string[] { Codes[1].Substring(0, 2), Codes[1].Substring(2, 2), Codes[1].Substring(4, 2) };
                BGC = Color.FromArgb(Convert.ToInt32(rgb[0], 16), Convert.ToInt32(rgb[1], 16), Convert.ToInt32(rgb[2], 16));
            }
            else
                BGC = Color.Black;

            if (Codes[9] == "")
                bgID = -1;
            else
            {
                if (Codes[9].StartsWith("BG"))
                    Codes[9] = Codes[9].Substring(2);
                else if (Codes[9] == "Square")
                    bgID = -1;
                else
                    bgID = Convert.ToInt32(Codes[9]);
            }
            // Art 0 and 00
            if (Codes.Length > 10)
            {
                for (int i = 0; i < 4; i++)
                    ArtCodes[i + 6] = Codes[i + 10];
            }

            // get blocks from data
            LoadBlocks(Codes[2]);
            CamX = PStart[0].X - 210;
            CamY = PStart[0].Y - 210;

            // Set other stuff
            // Items
            string[] avItemStr = ItemStr.Split('`');
            Array.Resize(ref avItems, avItemStr.Length);

            for (int i = 0; i < avItemStr.Length; i++)
                avItems[i] = Item.NameToID(avItemStr[i]);
        }

        private void LoadBlocks(string bData)
        {
            pX = 0;
            pY = 0;
            pT = 0;
            BlockData = new StringBuilder("");
            PStart = new Point[] { new Point(), new Point(), new Point(), new Point() };
            // Reset move block locations
            MLoc.Clear();
            // get blocks from data
            Blocks = new List<List<Block>>();
            XStart = 0;
            YStart = new List<int>();
            BlockCount = 0;
            MinX = 0;
            MaxX = 0;
            MinY = 0;
            MaxY = 0;
            finish_count = 0;

            string[] BlocksC = bData.Split(',');
            int LpX = 0;
            int LpY = 0;
            int LpT = 0;
            // Place blocks
            for (int i = 0; i < BlocksC.Length; i++)
            {
                // get X, Y, and Type
                string[] BCS = BlocksC[i].Split(';');
                if (BCS.Length > 2)
                {
                    if (BCS[2] == "")
                        BCS[2] = "0"; // Blank string means 0
                    if (BCS[2].Length == 3)
                        // fix for when bls made blocks start at 100
                        LpT = Convert.ToInt32(BCS[2]) - 100;
                    else
                        LpT = Convert.ToInt32(BCS[2]);
                }
                if (BCS.Length > 1)
                {
                    if (BCS[1] == "")
                        BCS[1] = "0";
                    LpY += Convert.ToInt32(BCS[1]);
                }
                if (BCS[0] == "")
                    BCS[0] = "0";
                LpX += Convert.ToInt32(BCS[0]);

                AddBlock(LpX, LpY, LpT);
            }
        }

        public string GetDataParam(bool keepArt = true)
        {
            string BlockD = BlockData.ToString();
            // Mapless
            if (Mapless)
            {
                BlockD += ",100000000;-100000000;0";
            }

            string Data = "m3`" + General.ColorToHex(BGC) + "`" + BlockD + "`";

            if (keepArt)
            {
                // Layers 1-3
                for (int i = 0; i < 6; i++)
                {
                    Data += ArtCodes[i] + "`";
                }
                Data += bgID + "`"; // I think.
                // Layers 00 and 0
                for (int i = 6; i < 10; i++)
                {
                    Data += ArtCodes[i] + "`";
                }
            }
            else
            {
                Data += "``````";
                Data += bgID; // I think. TODO: Verify
                Data += "````";
            }

            return Data;
        }

        public string GetDownloadHash(string dataStr = "")
        {
            if (dataStr == "")
            {
                string data = GetDownloadData(false);
                return General.GenerateHash(version.ToString() + ID + data + "84ge5tnr");
            }
            else
            {
                return General.GenerateHash(version.ToString() + ID + dataStr + "84ge5tnr");
            }
        }

        public void Compress()
        {
            string newData = LevelCompressor.CompressLevel(GetDataParam());

            string[] Codes = newData.Split('`');
            // Set arts 1-3
            for (int i = 0; i < 6; i++)
            {
                ArtCodes[i] = Codes[i + 3];
            }
            string[] rgb;
            if (Codes[1].Length >= 6)
            {
                rgb = new string[] { Codes[1].Substring(0, 2), Codes[1].Substring(2, 2), Codes[1].Substring(4, 2) };
                BGC = Color.FromArgb(Convert.ToInt32(rgb[0], 16), Convert.ToInt32(rgb[1], 16), Convert.ToInt32(rgb[2], 16));
            }
            else
                BGC = Color.Black;

            if (Codes[9] == "")
                bgID = -1;
            else
            {
                if (Codes[9].StartsWith("BG"))
                    Codes[9] = Codes[9].Substring(2);
                else if (Codes[9] == "Square")
                    bgID = -1;
                else
                    bgID = Convert.ToInt32(Codes[9]);
            }
            // Art 0 and 00
            if (Codes.Length > 10)
            {
                for (int i = 0; i < 4; i++)
                    ArtCodes[i + 6] = Codes[i + 10];
            }

            // get blocks from data
            LoadBlocks(Codes[2]);
        }

        // get the map's SaveState data
        public string[] GetSSData()
        {
            string[] Str = new string[6 + Chars.Count];
            Str[0] = GetUploadData(false);
            //Str[0] = Str[0].Replace("\n", "`;2");
            //Str[0] = Str[0].Replace(vbLf, "`;3");

            Str[1] = hasPass + "," + password + "," + useOldPass + "," + passImpossible;
            Str[1] += "," + userName + "," + Mapless + "," + inLE;
            Str[1] += "," + Frames + "," + MoveTime + "," + General.getRNGss();
            Str[1] += "," + CamX + "," + CamY;

            // blocks not matching originals (moved, pushed, placed, and deleted)
            // Block types that can be moved/placed/deleted: Brick, Crumble, Move, Push, Mine
            // So, generate block data for just those types. (Load SS will delete all of those types and then load from the data)
            Str[2] = generateSSMovedBlockData();

            // record MLoc
            Str[3] = "";
            for (int i = 0; i < MLoc.Count; i++)
            {
                Str[3] += "," + MLoc[i].X + ";" + MLoc[i].Y;
            }
            if (Str[3].Length > 0)
                Str[3] = Str[3].Substring(1);

            // all blocks that were used/hurt/vanishing/etc
            Str[4] = generateSSUsedBlockData();

            // and Effects
            Str[5] = generateSSEffectData();

            // All Characters
            for (int i = 0; i < Chars.Count; i++)
            {
                if (Chars[i].GetType() == typeof(LocalCharacter))
                {
                    LocalCharacter c = Chars[i] as LocalCharacter;
                    Str[6 + i] = c.GetSSData();
                }
            }

            return Str;
        }

        private string generateSSMovedBlockData()
        {
            // Block types that can be moved/placed/deleted: Brick, Crumble, Move, Push, Mine
            int prevX = 0;
            int prevY = 0;
            int prevT = 0;
            List<string> dataList = new List<string>();
            for (int iX = 0; iX < Blocks.Count; iX++)
            {
                if (Blocks[iX] == null)
                    continue;
                for (int iY = 0; iY < Blocks[iX].Count; iY++)
                {
                    if (Blocks[iX][iY] == null)
                        continue;
                    Block cB = Blocks[iX][iY];
                    if (cB.T == BlockID.Brick || cB.T == BlockID.Crumble || cB.T == BlockID.Move || cB.T == BlockID.Push || cB.T == BlockID.Mine)
                    {
                        string cD = (cB.X - prevX) + ";" + (cB.Y - prevY);
                        prevX = cB.X;
                        prevY = cB.Y;
                        if (cB.T != prevT)
                        {
                            cD += ";" + cB.T;
                            prevT = cB.T;
                        }

                        dataList.Add(cD);
                    }
                }
            }

            return string.Join(",", dataList);
        }

        private string generateSSUsedBlockData()
        {
            StringBuilder str = new StringBuilder("");
            for (int iX = 0; iX < Blocks.Count; iX++)
            {
                if (Blocks[iX] == null)
                    continue;
                for (int iY = 0; iY < Blocks[iX].Count; iY++)
                {
                    if (Blocks[iX][iY] == null)
                        continue;
                    Block cB = Blocks[iX][iY];
                    string addStr = "";
                    if (cB.TurnedToIce)
                        addStr += ";i" + cB.FreezeTime;
                    else if (cB.WasIced)
                        addStr += ";w" + cB.FreezeTime;
                    if (cB.Fade != Block.NORMAL)
                        addStr += ";f" + cB.Fade;
                    if (cB.FadeTime != 0)
                        addStr += ";t" + cB.FadeTime;
                    if (cB.BumpY != 0)
                        addStr += ";y" + cB.BumpY + ";" + cB.startBumping;
                    if (cB.Health != 10)
                        addStr += ";h" + cB.Health;
                    string uC = ";u";
                    for (int i = 0; i < cB.Used.Length; i++)
                    {
                        if (cB.Used[i])
                            uC += i.ToString();
                    }
                    if (uC != ";u")
                        addStr += uC;
                    if (cB.BumpVel != 0)
                        addStr += ";v" + cB.BumpVel;

                    if (addStr != "")
                        str.Append(cB.X + ";" + cB.Y + addStr + ",");
                }
            }

            if (str.Length > 0)
                str.Remove(str.Length - 1, 1);
            return str.ToString();
        }

        private string generateSSEffectData()
        {
            List<string> dataList = new List<string>();
            for (int i = 0; i < lasers.Count; i++)
            {
                Laser L = lasers[i];
                dataList.Add(L.X + ";" + L.Y + ";" + L.TTV + ";" + L.ID + ";" + L.Dir + ";" + L.rot);
            }
            string laserStr = string.Join(",", dataList);

            dataList = new List<string>();
            for (int i = 0; i < slashes.Count; i++)
            {
                Slash S = slashes[i];
                dataList.Add(S.X + ";" + S.Y + ";" + S.ID + ";" + S.Dir + ";" + S.rot);
            }
            string slashStr = string.Join(",", dataList);

            dataList = new List<string>();
            for (int i = 0; i < hats.Count; i++)
            {
                Hat H = hats[i];
                dataList.Add(H.X + ";" + H.Y + ";" + H.ID + ";" + H.ServID + ";" + H.rot + ";" + H.VelY + ";" + H.Color.ToArgb());
            }
            string hatStr = string.Join(",", dataList);

            return laserStr + "`" + slashStr + "`" + hatStr;
        }

        public void UseSSData(string[] Str, bool loadOld = false)
        {
            if (loadOld || Str[0][0] == '0')
            {
                List<String> newStr = new List<String>();
                for (int i = 0; i < Str.Length - 7; i++)
                    newStr.Add(Str[i]);
                int pCount = newStr.Count;
                for (int i = newStr.Count; i < Str.Length - 1; i++)
                    newStr.Insert(i - pCount, Str[i]);
                Str = newStr.ToArray();
            }
            //Str(0) = GetData(false)
            //Str[0] = Str[0].Replace("`;2", "\n");
            //Str[0] = Str[0].Replace("`;3", vbLf);
            inLE = true;
            LoadLevel(Str[0]);

            //Str(1) = hasPass & "," & password & "," & useOldPass & "," & passImpossible
            //Str(1) &= "," & userName & "," & Mapless & "," & inLE
            //Str(1) &= "," & Frames & "," & MoveTime & "," & MoveID
            //Str(1) &= "," & CamX & "," & CamY
            string[] SS = Str[1].Split(',');
            hasPass = Convert.ToBoolean(SS[0]);
            password = SS[1];
            useOldPass = Convert.ToBoolean(SS[2]);
            passImpossible = Convert.ToBoolean(SS[3]);
            userName = SS[4];
            Mapless = Convert.ToBoolean(SS[5]);
            if (SS[6] == "true")
                enterLE();
            else
                exitLE(false);

            Frames = Convert.ToInt32(SS[7]);
            MoveTime = Convert.ToInt32(SS[8]);
            if (SS[9].Contains(';')) // Old states are old.
                General.useRNGss(SS[9]);
            if (SS.Length > 10)
            {
                CamX = Convert.ToInt32(SS[10]);
                CamY = Convert.ToInt32(SS[11]);
            }

            //Str(2) = generateSSMovedBlockData()
            useSSMovedBlockData(Str[2]);

            // record MLoc
            //Str(3) = ""
            //for (int i  int = 0; i < MLoc.Count - 1
            //    Str(3) &= "," & MLoc[i].X & ";" & MLoc[i].Y
            //}
            //Str(3) = Str(3).Substring(1)
            SS = Str[3].Split(',');
            MLoc.Clear();
            MLoc = new List<Point>();
            if (SS[0] != "")
            {
                for (int i = 0; i < SS.Length; i++)
                    MLoc.Add(new Point(Convert.ToInt32(SS[i].Split(';')[0]), Convert.ToInt32(SS[i].Split(';')[1])));
            }

            //Str(4) = generateSSUsedBlockData()
            useSSUsedBlockData(Str[4]);

            //Str(5) = generateSSEffectData()
            useSSEffectData(Str[5]);

            // LocalCharacters
            for (int i = 6; i < Str.Length; i++)
            {
                if (Str[i] == "Ghost")
                    break;

                if (Chars.Count < i - 5)
                    Chars.Add(new LocalCharacter());

                if (Str[i] == "")
                    Chars[i].Reset();
                else
                    (Chars[i - 6] as LocalCharacter).UseSSData(Str[i]);
            }
        }

        private void useSSMovedBlockData(string Str)
        {
            // Block types that can be moved/placed/deleted: Brick, Crumble, Move, Push, Mine
            // Delete all of those types, and re-load based on given Str
            for (int iX = 0; iX < Blocks.Count; iX++)
            {
                if (Blocks[iX] == null)
                    continue;
                for (int iY = 0; iY < Blocks[iX].Count; iY++)
                {
                    if (Blocks[iX][iY] == null)
                        continue;
                    Block cB = Blocks[iX][iY];
                    if (cB.T == BlockID.Brick || cB.T == BlockID.Crumble || cB.T == BlockID.Move || cB.T == BlockID.Push || cB.T == BlockID.Mine)
                    {
                        DeleteBlock(cB.X, cB.Y);
                        if (iX >= Blocks.Count || Blocks[iX] == null)
                            break;
                    }
                    if (iY >= Blocks[iX].Count - 1)
                        break;
                }
            }

            string[] BlocksC = Str.Split(',');
            int LpX = 0;
            int LpY = 0;
            int LpT = 0;
            // Place blocks
            for (int i = 0; i < BlocksC.Length; i++)
            {
                // get X, Y, and Type
                string[] BCS = BlocksC[i].Split(';');
                if (BCS.Length > 2)
                {
                    if (BCS[2] == "")
                        BCS[2] = "0"; // Blank string means 0
                    LpT = Convert.ToInt32(BCS[2]);
                }
                if (BCS.Length > 1)
                {
                    if (BCS[1] == "")
                        BCS[1] = "0";
                    LpY += Convert.ToInt32(BCS[1]);
                }
                if (BCS[0] == "")
                    BCS[0] = "0";
                LpX += Convert.ToInt32(BCS[0]);

                AddBlock(LpX, LpY, LpT);
            }
        }

        private void useSSUsedBlockData(string Str)
        {
            faders.Clear();

            if (Str == "")
                return;
            string[] SS = Str.Split(',');
            for (int i = 0; i < SS.Length; i++)
            {
                string[] bStr = SS[i].Split(';');
                Block cB = getBlock(Convert.ToInt32(bStr[0]), Convert.ToInt32(bStr[1]), 0);

                int ID = 2;
                if (ID >= bStr.Length)
                    continue;
                // if ( cB.TurnedToIce ) { addStr &= ";i"
                if (bStr[ID].StartsWith("i"))
                {
                    if (bStr[ID].Length > 1)
                        cB.FreezeTime = Convert.ToInt32(bStr[ID].Substring(1));
                    cB.TurnedToIce = true;
                    cB.WasIced = true;
                    AddBlockEvent(cB, EVENT_THAW);
                    ID += 1;
                    if (ID >= bStr.Length)
                        continue;
                }
                if (bStr[ID].StartsWith("w"))
                {
                    if (bStr[ID].Length > 1)
                        cB.FreezeTime = Convert.ToInt32(bStr[ID].Substring(1));
                    cB.WasIced = true;
                    AddBlockEvent(cB, EVENT_REFRZ);
                    ID += 1;
                    if (ID >= bStr.Length)
                        continue;
                }
                //if ( cB.Fade != Block.NORMAL ) { addStr &= ";f" & cB.Fade
                if (bStr[ID].StartsWith("f"))
                {
                    cB.Fade = Convert.ToInt32(bStr[ID].Substring(1));
                    ID += 1;
                    AddBlockEvent(cB, EVENT_VANISH);
                    if (ID >= bStr.Length)
                        continue;
                }
                //if ( cB.FadeTime != 0 ) { addStr &= ";t" & cB.FadeTime
                if (bStr[ID].StartsWith("t"))
                {
                    cB.FadeTime = Convert.ToInt32(bStr[ID].Substring(1));
                    ID += 1;
                    if (cB.Fade == Block.NORMAL)
                        AddBlockEvent(cB, EVENT_PLACE);
                    if (ID >= bStr.Length)
                        continue;
                }
                //if ( cB.BumpY != 0 ) { addStr &= ";y" & cB.BumpY
                if (bStr[ID].StartsWith("y"))
                {
                    cB.BumpY = Convert.ToDouble(bStr[ID].Substring(1));
                    ID += 1;
                    AddBlockEvent(cB, EVENT_BUMP);
                    if (ID >= bStr.Length)
                        continue;
                    if (int.TryParse(bStr[ID], out int val))
                    {
                        cB.startBumping = val;
                        ID++;
                        if (ID >= bStr.Length)
                            continue;
                    }
                }
                //if ( cB.Health != 10 ) { addStr &= ";h" & cB.Health
                if (bStr[ID].StartsWith("h"))
                {
                    cB.Health = Convert.ToInt32(bStr[ID].Substring(1));
                    ID += 1;
                    if (ID >= bStr.Length)
                        continue;
                }
                //if ( cB.Used ) { addStr &= ";u"
                if (bStr[ID].StartsWith("u"))
                {
                    for (int iU = 1; iU < bStr[ID].Length; iU++)
                        cB.Used[Convert.ToUInt32(bStr[ID].Substring(iU, 1))] = true;
                    ID += 1;
                    if (ID >= bStr.Length)
                        continue; ;
                }
                //if ( cB.BumpVel != 0 ) { addStr &= ";v" & cB.BumpVel
                if (bStr[ID].StartsWith("v"))
                    cB.BumpVel = Convert.ToDouble(bStr[ID].Substring(1));
            }
        }

        private void useSSEffectData(string Str)
        {
            string[] effSS = Str.Split('`');
            //dataList.Add(L.X & ";" & L.Y & ";" & L.TTV & ";" & L.ID & ";" & L.Dir & ";" & L.rot)
            string[] SS = effSS[0].Split(',');
            if (effSS[0].Length > 0)
            {
                lasers.Clear();
                for (int i = 0; i < SS.Length; i++)
                {
                    Laser L = new Laser();
                    string[] lStr = SS[i].Split(';');
                    L.X = Convert.ToInt32(lStr[0]);
                    L.Y = Convert.ToInt32(lStr[1]);
                    L.TTV = Convert.ToInt32(lStr[2]);
                    L.ID = Convert.ToInt32(lStr[3]);
                    L.Dir = lStr[4];
                    L.rot = Convert.ToInt32(lStr[5]);
                    lasers.Add(L);
                }
            }

            //dataList.Add(S.X & ";" & S.Y & ";" & S.ID & ";" & S.Dir & ";" & S.rot)
            SS = effSS[1].Split(';');
            if (effSS[1].Length > 0)
            {
                slashes.Clear();
                for (int i = 0; i < SS.Length; i++)
                {
                    Slash S = new Slash();
                    S.X = Convert.ToInt32(SS[0]);
                    S.Y = Convert.ToInt32(SS[1]);
                    S.ID = Convert.ToInt32(SS[2]);
                    S.Dir = SS[3];
                    S.rot = Convert.ToInt32(SS[4]);
                    slashes.Add(S);
                }
            }

            //dataList.Add(H.X & ";" & H.Y & ";" & H.ID & ";" & H.ServID & ";" & H.rot & ";" & H.VelY & ";" & H.Color.ToArgb())
            SS = effSS[2].Split(';');
            hats.Clear();
            if (effSS[2].Length > 0)
            {
                for (int i = 0; i < SS.Length; i++)
                {
                    Hat H = new Hat();
                    H.X = Convert.ToInt32(SS[0]);
                    H.Y = Convert.ToInt32(SS[1]);
                    H.ID = Convert.ToInt32(SS[2]);
                    H.ServID = Convert.ToInt32(SS[3]);
                    H.rot = Convert.ToInt32(SS[4]);
                    H.VelY = Convert.ToDouble(SS[5]);
                    H.Color = Color.FromArgb(Convert.ToInt32(SS[6]));
                    hats.Add(H);
                }
            }
        }

        #region "IDisposable Support"
        private bool disposedValue;   //; i < detect redundant calls

        // IDisposable
        protected void Dispose(bool disposing)
        {
            if (!this.disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                    //'runThread.Abort()
                    MG.Dispose();
                    BlBit.Dispose();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override Finalize() below.
                // TODO: set large fields to null.
                // gameThread.Abort();

                Blocks = null;
                Chars = null;
                MainChar = null;
                FollowChar = null;
                faders = null;
                lasers = null;
                slashes = null;
                hats = null;
            }
            this.disposedValue = true;
        }

        // TODO: override Finalize() only if Dispose( disposing  bool) above has code to free unmanaged resources.
        //'Protected Overrides void Finalize()
        //    // Do not change this code.  Put cleanup code in Dispose( disposing  bool) above.
        //    Dispose(false)
        //    MyBase.Finalize()
        //'}

        // This code added by Visual Basic to correctly implement the disposable pattern.
        public void Dispose() // Implements IDisposable.Dispose
        {
            // Do not change this code.  Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion

    }

}
