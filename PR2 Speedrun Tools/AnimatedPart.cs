using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace PR2_Speedrun_Tools
{
    class AnimatedPart
    {
        public AnimatedPart(Bitmap img)
        {
            Image = img;
        }
        public AnimatedPart(Bitmap img, Bitmap cImg)
        {
            Image = img;
            colImage = cImg;
        }

        private Bitmap Image;
        private Bitmap colImage = null;
        private Matrix matrix = new Matrix();

        private float X, Y;
        private float tX, tY;
        private float rotation, tRotation;
        public PointF rotCenter;
        private float scaleX = 1, scaleY = 1;

        public void Draw(Graphics g, float dX, float dY)
        {
            Matrix tempMatrix = g.Transform;
            SetMatrix(dX, dY);
            g.Transform = matrix;

            if (colImage != null)
                g.DrawImage(colImage, X, Y); // ? , 0, 0);
            g.DrawImage(Image, X, Y); // ? , 0, 0);

            matrix.Translate(-dX, -dY);
            g.Transform = tempMatrix;
        }
        public void Animate()
        {
            X += (tX - X) * 0.3f;
            Y += (tY - Y) * 0.3f;
            rotation += (tRotation - rotation) * 0.3f;
        }
        private void SetMatrix(float dX, float dY, Matrix multiply = null)
        {
            matrix.Dispose(); matrix = new Matrix();
            matrix.Translate(dX, dY);
            matrix.Scale(scaleX, scaleY);
            matrix.RotateAt(rotation, rotCenter);

            if (multiply != null)
                matrix.Multiply(multiply);
        }

        public void Scale(float sX, float sY)
        {
            scaleX = sX;
            scaleY = sY;
        }
        public void SetPos(float x, float y)
        {
            tX = x;
            tY = y;
        }
        public void SetRotation(float r)
        {
            tRotation = r;
        }

    }
}
