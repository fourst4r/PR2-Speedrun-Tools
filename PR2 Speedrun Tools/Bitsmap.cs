using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Drawing;
using System.Diagnostics;

namespace PR2_Speedrun_Tools
{
    /// <summary>
    /// A bitmap with some built-in functions for drawing to it. Only functions ending with an 'A' have alpha support.
    /// </summary>
    public class Bitsmap : IDisposable
    {
        public Bitsmap()
        { }
        public Bitsmap(int nWidth, int nHeight)
        {
            Len = nWidth * nHeight * 4;
            BLoc = Marshal.AllocCoTaskMem(Len);
            Stride = nWidth * 4;
            Width = nWidth;
            Height = nHeight;
            // Make it black
            Clear(Color.Black);
        }
        public Bitsmap(Bitmap img)
        {
            Len = img.Width * img.Height * 4;
            BLoc = Marshal.AllocCoTaskMem(Len);
            Stride = img.Width * 4;
            Width = img.Width;
            Height = img.Height;
            Clear(Color.FromArgb(0, 0, 0, 0));
            //if (img.Width == 21)
            //{ int z = 0; z = 2 / z; }
            Graphics.FromImage(Bit).DrawImage(img, 0, 0, img.Width, img.Height);
        }

        public readonly int Width;
        public readonly int Height;

        int Len;
        public IntPtr BLoc;
        public byte GetByte(int X, int Y, int off)
        {
            if (X < 0 || Y < 0 || X >= Width || Y >= Height)
                return 0; // Outside bounds of bitmap

            return Marshal.ReadByte(BLoc + (X * 4) + (Y * Stride) + off);
        }
        public int GetPixel(int X, int Y)
        {
            if (X < 0 || Y < 0 || X >= Width || Y >= Height)
                return 0; // Outside bounds of bitmap

            return Marshal.ReadInt32(BLoc + (X * 4) + (Y * Stride));
        }

        public Bitmap Bit
        { get { return new Bitmap(Width, Height, Stride, System.Drawing.Imaging.PixelFormat.Format32bppArgb, BLoc); } }
        int Stride;

        public Bitsmap Clone()
        {
            Bitsmap c = new Bitsmap(Width, Height);
            int intLen = Len / 4;
            int[] temp = new int[intLen];
            Marshal.Copy(BLoc, temp, 0, intLen);
            Marshal.Copy(temp, 0, c.BLoc, intLen);

            return c;
        }
        public void Dispose()
        {
            Bit.Dispose();
            Marshal.FreeCoTaskMem(BLoc);
        }

        public void Clear(Color Col)
        {
            int locWidth = Width; // Using this makes it go oh so much faster.
            // Get integer value for color
            int val = Col.ToArgb();
            // Get an array for one bit stride
            int[] arr4 = new int[locWidth];
            for (int i = 0; i < arr4.Length; i++)
            {
                arr4[i] = val;
            }
            // Copy from the array to every row of pixels
            for (int i = 0; i < Len; i += Stride)
            {
                Marshal.Copy(arr4, 0, BLoc + i, locWidth);
            }
        }

        public void SetPixel(Color Col, int X, int Y)
        {
            if (X < 0 || Y < 0 || X >= Width || Y >= Height)
                return; // Outside bounds of bitmap

            Marshal.WriteInt32(BLoc + (X * 4) + (Y * Stride), Col.ToArgb());
        }
        public void SetByte(byte val, int X, int Y, int off)
        {
            if (X < 0 || Y < 0 || X >= Width || Y >= Height)
                return; // Outside bounds of bitmap

            Marshal.WriteByte(BLoc + (X * 4) + (Y * Stride) + off, val);
        }

        /// <summary>
        /// Set alpha of Bitsmap.
        /// </summary>
        /// <param name="value">Alpha value from 0 to 1</param>
        public void SetAlpha(float value)
        {
            Debug.Assert(value != 1.0f);
            // (32 bits = 4 bytes, 3 for RGB and 1 byte for alpha).
            int numBytes = Width * Height * 4;
            byte[] argbValues = new byte[numBytes];

            // Copy the ARGB values into the array.
            Marshal.Copy(BLoc, argbValues, 0, numBytes);

            // Manipulate the bitmap, such as changing the
            // RGB values for all pixels in the bitmap.
            for (int counter = 0; counter < argbValues.Length; counter += 4)
            {
                // argbValues is in format BGRA

                // If 100% transparent, skip pixel
                if (argbValues[counter + 3] == 0)
                    continue;

                int pos = 0;
                pos++; // B value
                pos++; // G value
                pos++; // R value

                argbValues[counter + pos] = (byte)(255 * value);
            }

            Marshal.Copy(argbValues, 0, BLoc, numBytes);
        }

        public void FillRectangle(Color Col, int X, int Y, int rWidth, int rHeight)
        {
            // Get integer value for the color to use
            int val = Col.ToArgb();

            // Change X, Y, rWidth, and rHeight based on Bit's size, as needed. (Don't try to change pixels that don't exist.)
            if (X >= Width || Y >= Height)
                return; // Nothing to fill!
            if (X < 0)
            {
                rWidth += X;
                X = 0;
            }
            if (Y < 0)
            {
                rHeight += Y;
                Y = 0;
            }
            if (rWidth + X > Width)
                rWidth = Width - X;
            if (rHeight + Y > Height)
                rHeight = Height - Y;
            // Width/Height under 0?
            if (rWidth <= 0 || rHeight <= 0)
                return;

            // Get an array for one row of the rectangle
            int[] arr4 = new int[rWidth];
            for (int i = 0; i < arr4.Length; i++)
            {
                arr4[i] = val;
            }
            // Copy from the array to every row of pixels
            IntPtr pixel = BLoc + (X * 4) + (Y * Stride);
            for (int iY = 0; iY < rHeight; iY++)
            {
                Marshal.Copy(arr4, 0, pixel, arr4.Length);
                pixel += Stride;
            }
        }
        public unsafe void FillRectangleA(Color Col, int X, int Y, int rWidth, int rHeight)
        {
            if (Col.A == 255) // Use the non-alpha version if possible
            { FillRectangle(Col, X, Y, rWidth, rHeight); return; }
            if (Col.A == 0) // No need to do anything here.
                return;
            // Change X, Y, rWidth, and rHeight based on Bit's size, as needed. (Don't try to change pixels that don't exist.)
            if (X >= Width || Y >= Height || rWidth <= 0 || rHeight <= 0)
                return; // Nothing to fill!
            if (X < 0)
            {
                rWidth += X;
                X = 0;
            }
            if (Y < 0)
            {
                rHeight += Y;
                Y = 0;
            }
            if (rWidth + X > Width)
                rWidth = Width - X;
            if (rHeight + Y > Height)
                rHeight = Height - Y;

            // Values for loop
            double a = (double)Col.A / 255.0;
            double alpha1 = 1 - a;
            byte r = (byte)(Col.R * a);
            byte g = (byte)(Col.G * a);
            byte b = (byte)(Col.B * a);

            IntPtr pixel = BLoc + (X * 4) + (Y * Stride);
            for (int iY = 0; iY < rHeight; iY++)
            {
                byte* p = (byte*)pixel;
                for (int iX = 0; iX < rWidth; iX++)
                {
                    // newC = oldC + (ovrC - oldC)*A
                    // newC = (1 - A)*oldC + ovrCA
                    p[0] = (byte)(p[0] * alpha1 + b);
                    p[1] = (byte)(p[1] * alpha1 + g);
                    p[2] = (byte)(p[2] * alpha1 + r);
                    p += 4;
                }
                pixel += Stride;
            }

        }

        public void FillEllipse(Color Col, int X, int Y, int rWidth, int rHeight)
        {
            // Get integer value for the color to use
            int val = Col.ToArgb();

            // Is ellipse inside bounds of bitmap?
            if (X >= Width || Y >= Height || rWidth <= 0 || rHeight <= 0)
                return; // Nothing to fill!
            // Cut off edges that are past edges of the bitmap...
            int iStart = 0;
            if (Y < 0)
                iStart = -Y;
            int iEnd = rHeight;
            if (Y + rHeight >= Height)
                iEnd = Height - Y;
            int xMin = -(X + (int)(rWidth * 0.5));
            int xMax = Width - (X + (int)(rWidth * 0.5));


            double mWidth = (double)rWidth / (double)rHeight;
            // Get an array for one row (max possible length)
            int a4Len = (int)(rWidth * mWidth) + 1;
            if (mWidth < 1)
                a4Len = rWidth;
            int[] arr4 = new int[a4Len];
            for (int i = 0; i < arr4.Length; i++)
            {
                arr4[i] = val;
            }
            // each row of pixels
            double middle = (double)rHeight / 2;
            double middle2 = middle * middle;
            int mPtr = (int)(X + (rWidth * 0.5)) * 4;
            mPtr += (int)BLoc + (Y * Stride);
            IntPtr ptr = (IntPtr)mPtr;
            ptr += (iStart * Stride);
            for (int i = iStart; i < iEnd; i++)
            {
                // start^2 + cHeight^2 = radius^2
                double cHeight = middle - i;
                int start = (int)(Math.Sqrt(middle2 - (cHeight * cHeight)) * mWidth + 0.5);

                // Limit left-right to edges of bitmap
                int xStart = -start;
                if (xStart < xMin)
                    xStart = xMin;
                int xEnd = start;
                if (xEnd > xMax)
                    xEnd = xMax;

                // Change the pixels
                Marshal.Copy(arr4, 0, ptr + (xStart * 4), (xEnd - xStart));
                ptr += Stride;
            }
        }
        public unsafe void FillEllipseA(Color Col, int X, int Y, int rWidth, int rHeight)
        {
            if (Col.A == 255) // Use the non-alpha version if possible
            { FillEllipse(Col, X, Y, rWidth, rHeight); return; }
            if (Col.A == 0) // No need to do anything here.
                return;
            // Is ellipse inside bounds of bitmap?
            if (X >= Width || Y >= Height || rWidth <= 0 || rHeight <= 0)
                return; // Nothing to fill!
            // Cut off edges that are past edges of the bitmap...
            int iStart = 0;
            if (Y < 0)
                iStart = -Y;
            int iEnd = rHeight;
            if (Y + rHeight >= Height)
                iEnd = Height - Y;
            int xMin = -(X + (int)(rWidth * 0.5));
            int xMax = Width - (X + (int)(rWidth * 0.5));


            // Values for loop
            double a = (double)Col.A / 255.0;
            double alpha1 = 1 - a;
            byte r = (byte)(Col.R * a);
            byte g = (byte)(Col.G * a);
            byte b = (byte)(Col.B * a);

            // each row of pixels
            double mWidth = (double)rWidth / (double)rHeight;
            double middle = rHeight / 2;
            double middle2 = middle * middle;

            int mPtr = (int)(X + (rWidth * 0.5)) * 4;
            mPtr += (int)BLoc + (Y * Stride);
            IntPtr ptr = (IntPtr)mPtr;
            ptr += (iStart * Stride);
            for (int i = iStart; i < iEnd; i++)
            {
                // start^2 + cHeight^2 = radius^2
                double cHeight = middle - i;
                int start = (int)(Math.Sqrt(middle2 - (cHeight * cHeight)) * mWidth + 0.5);

                // Limit left-right to edges of bitmap
                int xStart = -start;
                if (xStart < xMin)
                    xStart = xMin;
                int xEnd = start;
                if (xEnd > xMax)
                    xEnd = xMax;

                // Change the pixels
                byte* p = (byte*)(ptr + (xStart * 4));
                for (int iX = xStart; iX < xEnd; iX++)
                {
                    // newC = (1 - A)*oldC + ovrCA
                    p[0] = (byte)(p[0] * alpha1 + b);
                    p[1] = (byte)(p[1] * alpha1 + g);
                    p[2] = (byte)(p[2] * alpha1 + r);
                    p += 4;
                }
                ptr += Stride;
            }
        }

        private struct Line
        {
            public Line(Point p1, Point p2)
            {
                start = p1;
                end = p2;

                double yT = end.Y - start.Y;
                double xT = end.X - start.X;
                if (yT == 0) //-V3024
                    xPerY = 0;
                else
                    xPerY = xT / yT;
            }
            public readonly Point start;
            public readonly Point end;

            private double xPerY;
            public int XatY(int y)
            {
                y -= start.Y;
                return start.X + (int)(y * xPerY);
            }
        }
        private struct LineF
        {
            public LineF(PointF p1, PointF p2)
            {
                start = p1;
                end = p2;

                float yT = end.Y - start.Y;
                float xT = end.X - start.X;
                if (yT == 0)
                    xPerY = 0;
                else
                    xPerY = xT / yT;
            }
            public readonly PointF start;
            public readonly PointF end;

            private float xPerY;
            public float XatY(float y)
            {
                y -= start.Y;
                return start.X + (y * xPerY);
            }
        }
        private Point PointFtoPoint(PointF p)
        {
            return new Point((int)p.X, (int)p.Y);
        }
        /// <summary>
        /// Fills a convex polygon. Ish. Concave polygons will not always draw properly.
        /// </summary>
        /// <param name="Col"></param>
        /// <param name="points"></param>
        /// <returns></returns>
        public void FillPolygon(Color Col, ref Point[] points)
        {
            int pCount = points.Count();
            int val = Col.ToArgb();
            // Add the first point to the end of the array, for easier line access
            Array.Resize(ref points, points.Length + 1);
            points[points.Count() - 1] = points[0];

            // Convert array of points to an array of Lines
            Line[] lines = new Line[pCount];
            int LID = 0;
            // Get min/max Ys too
            int minY = points[0].Y;
            int maxY = points[0].Y;
            for (int i = 0; i < pCount; i++)
            {
                if (points[i].Y < minY)
                    minY = points[i].Y;
                else if (points[i].Y > maxY)
                    maxY = points[i].Y;

                if (points[i].Y == points[i + 1].Y) // Exclude lines that are perfectly horizontal
                    continue;

                if (points[i].Y < points[i + 1].Y)
                    lines[LID] = new Line(points[i], points[i + 1]);
                else
                    lines[LID] = new Line(points[i + 1], points[i]);
                LID++;
            }
            Array.Resize(ref lines, LID);
            // Remove last element of points, which was added here
            Array.Resize(ref points, points.Length - 1);

            // Array for maximum possible length fill line
            int[] arr4 = new int[Width];
            for (int i = 0; i < arr4.Length; i++)
                arr4[i] = val;

            // Stay within top/bottom bounds
            if (minY < 0)
                minY = 0;
            if (maxY >= Height)
                maxY = Height - 1;

            // Lines to Check (for if a new line matches iY)
            List<int> ltc = new List<int>();
            for (int i = 0; i < lines.Length; i++)
                ltc.Add(i);
            IntPtr YPtr = BLoc + (minY * Stride);
            List<Line> intersect = new List<Line>(); // Lines that went through last loop
            // Find lines to fill for each Y pixel of any line
            for (int iY = minY; iY < maxY; iY++)
            {
                List<int> Xpos = new List<int>();
                // Find lines that still go through iY
                for (int i2 = 0; i2 < intersect.Count; i2++)
                {
                    if (intersect[i2].end.Y > iY)
                        Xpos.Add(intersect[i2].XatY(iY));
                    else
                    {
                        intersect.RemoveAt(i2);
                        i2--;
                    }
                }
                // Find new lines that go through iY
                for (int i2 = 0; i2 < ltc.Count; i2++)
                {
                    Line cL = lines[ltc[i2]];
                    if (iY >= cL.start.Y)
                    {
                        intersect.Add(cL);
                        ltc.RemoveAt(i2);
                        i2--;
                        if (iY < cL.end.Y)
                            Xpos.Add(cL.XatY(iY));
                    }
                }
                // Sort by Xpos at iY
                Xpos.Sort();
                // Fill!
                int iF = 0;
                for (; iF < Xpos.Count && Xpos[iF] < 0; iF += 2)
                {
                    Xpos[iF] = 0;
                    if (Xpos[iF + 1] >= 0)
                        break;
                }
                for (; iF < Xpos.Count; iF += 2)
                {
                    if (Xpos[iF + 1] >= Width)
                    {
                        Xpos[iF + 1] = Width - 1;
                        if (Xpos[iF] >= Width)
                            break;
                    }
                    IntPtr dest = YPtr + (Xpos[iF] * 4);
                    Marshal.Copy(arr4, 0, dest, Xpos[iF + 1] - Xpos[iF]);
                }
                YPtr += Stride;
            }
        }
        public void FillPolygon(Color Col, ref PointF[] points)
        {
            int pCount = points.Count();
            int val = Col.ToArgb();
            // Add the first point to the end of the array, for easier line access
            Array.Resize(ref points, points.Length + 1);
            points[points.Count() - 1] = points[0];

            // Convert array of points to an array of Lines
            LineF[] lines = new LineF[pCount];
            int LID = 0;
            // Get min/max Ys too
            float FminY = points[0].Y;
            float FmaxY = points[0].Y;
            for (int i = 0; i < pCount; i++)
            {
                if (points[i].Y < FminY)
                    FminY = points[i].Y;
                else if (points[i].Y > FmaxY)
                    FmaxY = points[i].Y;

                if (points[i].Y == points[i + 1].Y) // Exclude lines that are perfectly horizontal
                    continue;

                if (points[i].Y < points[i + 1].Y)
                    lines[LID] = new LineF(points[i], points[i + 1]);
                else
                    lines[LID] = new LineF(points[i + 1], points[i]);
                LID++;
            }
            Array.Resize(ref lines, LID);
            // Remove last element of points, which was added here
            Array.Resize(ref points, points.Length - 1);

            // Array for maximum possible length fill line
            int[] arr4 = new int[Width];
            for (int i = 0; i < arr4.Length; i++)
                arr4[i] = val;

            // Stay within top/bottom bounds
            int minY = (int)FminY;
            int maxY = (int)FmaxY;
            if (minY < 0)
                minY = 0;
            if (maxY >= Height)
                maxY = Height - 1;

            // Lines to Check (for if a new line matches iY)
            List<int> ltc = new List<int>();
            for (int i = 0; i < lines.Length; i++)
                ltc.Add(i);
            IntPtr YPtr = BLoc + (minY * Stride);
            List<LineF> intersect = new List<LineF>(); // Lines that went through last loop
            // Find lines to fill for each Y pixel of any line
            for (int iY = minY; iY < maxY; iY++)
            {
                List<int> Xpos = new List<int>();
                // Find lines that still go through iY
                for (int i2 = 0; i2 < intersect.Count; i2++)
                {
                    if (intersect[i2].end.Y > iY)
                        Xpos.Add((int)intersect[i2].XatY(iY));
                    else
                    {
                        intersect.RemoveAt(i2);
                        i2--;
                    }
                }
                // Find new lines that go through iY
                for (int i2 = 0; i2 < ltc.Count; i2++)
                {
                    LineF cL = lines[ltc[i2]];
                    if (iY >= cL.start.Y)
                    {
                        intersect.Add(cL);
                        ltc.RemoveAt(i2);
                        i2--;
                        if (iY < cL.end.Y)
                            Xpos.Add((int)cL.XatY(iY));
                    }
                }
                // Sort by Xpos at iY
                Xpos.Sort();
                // Fill!
                int iF = 0;
                for (; iF < Xpos.Count && Xpos[iF] < 0; iF += 2)
                {
                    Xpos[iF] = 0;
                    if (Xpos[iF + 1] >= 0)
                        break;
                }
                for (; iF < Xpos.Count; iF += 2)
                {
                    if (Xpos[iF + 1] >= Width)
                    {
                        Xpos[iF + 1] = Width - 1;
                        if (Xpos[iF] >= Width)
                            break;
                    }
                    IntPtr dest = YPtr + (Xpos[iF] * 4);
                    Marshal.Copy(arr4, 0, dest, Xpos[iF + 1] - Xpos[iF]);
                }
                YPtr += Stride;
            }
        }
        public unsafe void FillPolygonA(Color Col, ref Point[] points)
        {
            if (Col.A == 255) // Use the non-alpha version if possible
            { FillPolygon(Col, ref points); return; }
            if (Col.A == 0) // No need to do anything here.
                return;

            int pCount = points.Count();
            // Values for alpha stuff
            double a = (double)Col.A / 255.0;
            double alpha1 = 1 - a;
            byte r = (byte)(Col.R * a);
            byte g = (byte)(Col.G * a);
            byte b = (byte)(Col.B * a);

            // Convert array of points to an array of Lines
            Line[] lines = new Line[pCount];
            // Add the first point to the end of the array, for easier line access
            Array.Resize(ref points, points.Length + 1);
            points[points.Count() - 1] = points[0];
            for (int i = 0; i < pCount; i++)
            {
                //lines[i] = new Line();
                //if (points[i].Y < points[i + 1].Y)
                //{
                //    lines[i].start = points[i];
                //    lines[i].end = points[i + 1];
                //}
                //else
                //{
                //    lines[i].start = points[i + 1];
                //    lines[i].end = points[i];
                //}
            }
            // Remove last element of points, since it was passed by ref
            Array.Resize(ref points, points.Length - 1);


            // Loop through each line and see if it is at the same height as another.
            for (int i1 = 0; i1 < lines.Length - 1; i1++)
            {
                Line line1 = lines[i1];
                for (int i2 = i1 + 1; i2 < lines.Length; i2++)
                {
                    Line line2 = lines[i2];
                    if (line1.end.Y >= line2.start.Y && line2.end.Y >= line1.start.Y)
                    {
                        // Some area between these lines needs be filled. Find exact range.
                        int fillMin = line1.start.Y;
                        int fillMax = line1.end.Y;
                        if (fillMin < line2.start.Y)
                            fillMin = line2.start.Y;
                        if (fillMax > line2.end.Y)
                            fillMax = line2.end.Y;

                        // Find starting sX and eX, and how much they change per line.
                        int lineH1 = line1.end.Y - line1.start.Y;
                        int lineW1 = line1.end.X - line1.start.X;
                        double cStart = (double)lineW1 / (double)lineH1;
                        int lineH2 = line2.end.Y - line2.start.Y;
                        int lineW2 = line2.end.X - line2.start.X;
                        double cEnd = (double)lineW2 / (double)lineH2;

                        int startX = line1.start.X + (int)(cStart * (fillMin - line1.start.Y));
                        int endX = line2.start.X + (int)(cEnd * (fillMin - line2.start.Y));
                        // Make sure startX is the lower of the two
                        if (endX < startX)
                        {
                            int temp = startX;
                            startX = endX;
                            endX = temp;
                            double dTemp = cStart;
                            cStart = cEnd;
                            cEnd = dTemp;
                        }

                        // Fill loop
                        IntPtr rowPointer = BLoc + (fillMin * Stride);
                        for (int i = 0; i < fillMax - fillMin; i++)
                        {
                            int sX = startX + (int)(i * cStart);
                            int eX = endX + (int)(i * cEnd);
                            // Make sure they are within the bounds of the bitmap
                            if (sX < 0)
                                sX = 0;
                            if (eX >= Width)
                                eX = Width - 1;

                            // Fill those pixels
                            byte* p = (byte*)(rowPointer + (sX * 4));
                            for (int iX = sX; iX < eX; iX++)
                            {
                                // newC = (1 - A)*oldC + ovrCA
                                p[0] = (byte)(p[0] * alpha1 + b);
                                p[1] = (byte)(p[1] * alpha1 + g);
                                p[2] = (byte)(p[2] * alpha1 + r);
                                p += 4;
                            }
                            rowPointer += Stride;
                        }
                    }
                }
            }
        }

        public void DrawImage(ref Bitmap img, int X, int Y)
        {
            if (img.PixelFormat != System.Drawing.Imaging.PixelFormat.Format32bppArgb)
            {
                if (img.PixelFormat == System.Drawing.Imaging.PixelFormat.Format32bppPArgb)
                    throw new Exception("The image is not in the supported format. (32bppArgb)" + "\n" + "Picture is 32bppPArgb. Please use the DrawImageA function with this picture.");
                throw new Exception("The image is not in the supported format. (32bppArgb)");
            }
            int dWidth = img.Width;
            int dHeight = img.Height;
            // Change X, Y, Width, and Height based on Bit's size, as needed. (Don't try to change pixels that don't exist.)
            if (X >= Width || Y >= Height)
                return; // Nothing to fill!  
            int leftCutOff = 0;
            int topCutOff = 0;
            if (X < 0)
            {
                dWidth += X;
                leftCutOff = -X;
                X = 0;
            }
            if (Y < 0)
            {
                dHeight += Y;
                topCutOff = -Y;
                Y = 0;
            }
            if (dWidth + X > Width)
                dWidth = Width - X;

            if (dHeight + Y > Height)
                dHeight = Height - Y;

            if (dWidth <= 0 || dHeight <= 0)
                return;

            // Copy rows of bytes
            int[] myBytes = new int[Stride / 4];
            // Marshal.Copy(BLoc, myBytes, 0, Stride * Height);
            System.Drawing.Imaging.BitmapData dmb = img.LockBits(new Rectangle(0, 0, 1, 1), System.Drawing.Imaging.ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            IntPtr ptrMe = BLoc + (X * 4) + (Y * Stride);
            IntPtr ptrIt = dmb.Scan0;
            ptrIt += (leftCutOff * 4);
            ptrIt += (topCutOff * dmb.Stride);
            int ItStride = img.Width * 4;
            int copyLen = dWidth; //*4;
            for (int iY = 0; iY < dHeight; iY++)
            {
                Marshal.Copy(ptrIt, myBytes, 0, copyLen);
                Marshal.Copy(myBytes, 0, ptrMe, copyLen);
                ptrMe += Stride;
                ptrIt += ItStride;
            }
            img.UnlockBits(dmb);
        }
        public unsafe void DrawImage(ref Bitsmap img, int X, int Y)
        {
            int dWidth = img.Width;
            int dHeight = img.Height;
            // Change X, Y, Width, and Height based on Bit's size, as needed. (Don't try to change pixels that don't exist.)
            if (X >= Width || Y >= Height)
                return; // Nothing to fill!  
            int leftCutOff = 0;
            int topCutOff = 0;
            if (X < 0)
            {
                dWidth += X;
                leftCutOff = -X;
                X = 0;
            }
            if (Y < 0)
            {
                dHeight += Y;
                topCutOff = -Y;
                Y = 0;
            }
            if (dWidth + X > Width)
                dWidth = Width - X;

            if (dHeight + Y > Height)
                dHeight = Height - Y;

            if (dWidth <= 0 || dHeight <= 0)
                return;

            // Copy rows of bits
            IntPtr ptrMe = BLoc + (X * 4) + (Y * Stride);
            int ItStride = img.Width * 4;
            int copyLen = dWidth * 1;
            int[] newItBytes = new int[copyLen];
            IntPtr ItPtr = img.BLoc;
            ItPtr += (leftCutOff * 4);
            ItPtr += (topCutOff * img.Stride);

            for (int iY = 0; iY < dHeight; iY++)
            {
                Marshal.Copy(ItPtr, newItBytes, 0, copyLen);
                Marshal.Copy(newItBytes, 0, ptrMe, copyLen);
                //CopyIntArray((int*)ItPtr, (int*)ptrMe, copyLen);
                ptrMe += Stride;
                ItPtr += ItStride;
            }
        }

        public unsafe void DrawImageA(ref Bitmap img, int X, int Y)
        {
            if (img.PixelFormat != System.Drawing.Imaging.PixelFormat.Format32bppArgb)
            {
                if (img.PixelFormat == System.Drawing.Imaging.PixelFormat.Format32bppPArgb)
                { preADrawImage(ref img, X, Y); return; } // Use another function, return.
                throw new Exception("The image is not in a supported format. (32bpp)");
            }
            int dWidth = img.Width;
            int dHeight = img.Height;
            // Change X, Y, Width, and Height based on Bit's size, as needed. (Don't try to change pixels that don't exist.)
            if (X >= Width || Y >= Height)
                return; // Nothing to fill!  
            int leftCutOff = 0;
            int topCutOff = 0;
            if (X < 0)
            {
                dWidth += X;
                leftCutOff = -X;
                X = 0;
            }
            if (Y < 0)
            {
                dHeight += Y;
                topCutOff = -Y;
                Y = 0;
            }
            if (dWidth + X > Width)
                dWidth = Width - X;

            if (dHeight + Y > Height)
                dHeight = Height - Y;

            if (dWidth <= 0 || dHeight <= 0)
                return;

            // Copy/blend bytes
            System.Drawing.Imaging.BitmapData dmb = img.LockBits(new Rectangle(0, 0, 1, 1), System.Drawing.Imaging.ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            IntPtr ItPtr = dmb.Scan0;
            IntPtr ptrMe = BLoc + (X * 4) + (Y * Stride);
            int ItStride = img.Width * 4;
            ItPtr += (leftCutOff * 4);
            ItPtr += (topCutOff * dmb.Stride);

            for (int iY = 0; iY < dHeight; iY++)
            {
                byte* pMe = (byte*)ptrMe;
                byte* pIt = (byte*)ItPtr;
                for (int iX = 0; iX < dWidth; iX++)
                {
                    // Values for loop
                    double a = (double)pIt[3] / 255.0;
                    double alpha1 = 1 - a;

                    // newC = (1 - A)*oldC + ovrCA
                    pMe[0] = (byte)(pMe[0] * alpha1 + pIt[0] * a);
                    pMe[1] = (byte)(pMe[1] * alpha1 + pIt[1] * a);
                    pMe[2] = (byte)(pMe[2] * alpha1 + pIt[2] * a);

                    pMe += 4;
                    pIt += 4;
                }
                ptrMe += Stride;
                ItPtr += ItStride;
            }
            img.UnlockBits(dmb);
        }
        //This function is used in DrawImageA(bitmap overload) if the given bitmap is PArgb.
        private unsafe void preADrawImage(ref Bitmap img, int X, int Y)
        {
            int dWidth = img.Width;
            int dHeight = img.Height;
            // Change X, Y, Width, and Height based on Bit's size, as needed. (Don't try to change pixels that don't exist.)
            if (X >= Width || Y >= Height)
                return; // Nothing to fill!  
            int leftCutOff = 0;
            int topCutOff = 0;
            if (X < 0)
            {
                dWidth += X;
                leftCutOff = -X;
                X = 0;
            }
            if (Y < 0)
            {
                dHeight += Y;
                topCutOff = -Y;
                Y = 0;
            }
            if (dWidth + X > Width)
                dWidth = Width - X;

            if (dHeight + Y > Height)
                dHeight = Height - Y;

            if (dWidth <= 0 || dHeight <= 0)
                return;

            // Copy/blend bytes
            System.Drawing.Imaging.BitmapData dmb = img.LockBits(new Rectangle(0, 0, 1, 1), System.Drawing.Imaging.ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            IntPtr ItPtr = dmb.Scan0;
            IntPtr ptrMe = BLoc + (X * 4) + (Y * Stride);
            int ItStride = img.Width * 4;
            ItPtr += (leftCutOff * 4);
            ItPtr += (topCutOff * dmb.Stride);

            for (int iY = 0; iY < dHeight; iY++)
            {
                byte* pMe = (byte*)ptrMe;
                byte* pIt = (byte*)ItPtr;
                for (int iX = 0; iX < dWidth; iX++)
                {
                    // Values for loop
                    double a = (double)pIt[3] / 255.0;
                    double alpha1 = 1 - a;

                    // newC = (1 - A)*oldC + ovrCA
                    pMe[0] = (byte)(pMe[0] * alpha1 + pIt[0]);
                    pMe[1] = (byte)(pMe[1] * alpha1 + pIt[1]);
                    pMe[2] = (byte)(pMe[2] * alpha1 + pIt[2]);

                    pMe += 4;
                    pIt += 4;
                }
                ptrMe += Stride;
                ItPtr += ItStride;
            }
            img.UnlockBits(dmb);
        }
        public unsafe void DrawImageA(ref Bitsmap img, int X, int Y)
        {
            int dWidth = img.Width;
            int dHeight = img.Height;
            // Change X, Y, Width, and Height based on Bit's size, as needed. (Don't try to change pixels that don't exist.)
            if (X >= Width || Y >= Height)
                return; // Nothing to fill!  
            int leftCutOff = 0;
            int topCutOff = 0;
            if (X < 0)
            {
                dWidth += X;
                leftCutOff = -X;
                X = 0;
            }
            if (Y < 0)
            {
                dHeight += Y;
                topCutOff = -Y;
                Y = 0;
            }
            if (dWidth + X > Width)
                dWidth = Width - X;

            if (dHeight + Y > Height)
                dHeight = Height - Y;

            if (dWidth <= 0 || dHeight <= 0)
                return;

            // Copy/blend bytes
            int ItStride = img.Width * 4;
            IntPtr ItPtr = img.BLoc;
            ItPtr += (leftCutOff * 4);
            ItPtr += (topCutOff * img.Stride);
            IntPtr ptrMe = BLoc + (X * 4) + (Y * Stride);
            ItPtr += (leftCutOff * 4);
            ItPtr += (topCutOff * img.Stride);

            for (int iY = 0; iY < dHeight; iY++)
            {
                byte* pMe = (byte*)ptrMe;
                byte* pIt = (byte*)ItPtr;
                for (int iX = 0; iX < dWidth; iX++)
                {
                    // Values for loop
                    double a = (double)pIt[3] / 255.0;
                    double alpha1 = 1 - a;

                    // newC = (1 - A)*oldC + ovrCA
                    pMe[0] = (byte)(pMe[0] * alpha1 + pIt[0] * a);
                    pMe[1] = (byte)(pMe[1] * alpha1 + pIt[1] * a);
                    pMe[2] = (byte)(pMe[2] * alpha1 + pIt[2] * a);

                    pMe += 4;
                    pIt += 4;
                }
                ptrMe += Stride;
                ItPtr += ItStride;
            }
        }


        //Private void DrawBitmap(ref g As Graphics, ref Source As Bitmap, Angle As Double, X As Integer, Y As Integer, Optional center As Boolean = False)
        //    if( center = False )
        //        X += Math.Floor(Source.Width * 0.5)
        //        Y += Math.Floor(Source.Height * 0.5)
        //    }
        //    //Dim p As Bitmap = Rotate(Angle, Source)
        //    //X -= Math.Floor(p.Width * 0.5)
        //    //Y -= Math.Floor(p.Height * 0.5)
        //    //g.DrawImage(p, X, Y)
        //}

        public unsafe void DrawRotatedImage(double Angle, ref Bitsmap Img, int X, int Y)
        {
            do
            {
                if (Angle > 90)
                {
                    Angle -= 90;
                    Img.Bit.RotateFlip(RotateFlipType.Rotate90FlipNone);
                }
                else if (Angle < 0)
                {
                    Angle += 90;
                    Img.Bit.RotateFlip(RotateFlipType.Rotate270FlipNone);
                }
            } while (Angle <= 0 || Angle >= 90);
            Angle = Angle * (Math.PI / 180); // Radians

            for (int iY = 0; iY < Img.Height; iY++)
            {
                for (int iX = 0; iX < Img.Width; iX++)
                {
                    // Rotate iX iY to get new coordinates
                    double h = Math.Sqrt((iY * iY) + (iX * iX));
                    double Ang = Math.Atan2(iY, iX);
                    Ang += Angle;
                    int nX = (int)(Math.Cos(Ang) * h);
                    int nY = (int)(Math.Sin(Ang) * h);

                    // Set pixel
                    int* p = (int*)(BLoc + ((nX + X) * 4) + ((nY + Y) * Stride));
                    int* po = (int*)(Img.BLoc + (iX * 4) + (iY * Img.Stride));
                    *p = *po;
                }
            }

        }
    }
}
