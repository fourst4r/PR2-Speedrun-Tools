using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PR2_Speedrun_Tools
{
    class ReplayUpdate : ICloneable
    {
        public DateTimeOffset Timestamp;
        public string[] Bits;

        public object Clone()
        {
            return new ReplayUpdate
            {
                Timestamp = this.Timestamp,
                Bits = (string[])this.Bits.Clone(),
            };
        }

        public override string ToString()
        {
            return $"{Timestamp.ToUnixTimeMilliseconds()}`{string.Join("`", Bits)}";
        }
    }

    public class Replay
    {
        private List<ReplayUpdate> _updates;
        private int _tick;
        private DateTimeOffset _startTimestamp;
        private const double _interpFactor = 6;

        public Replay(string[] lines, bool interpolateMovement)
        {
            interpolateMovement = false;
            _updates = new List<ReplayUpdate>();
            ReplayUpdate prevUpdate = null;
            foreach (var p in lines)
            {
                var spl = p.Split('`');
                var update = new ReplayUpdate();
                update.Timestamp = DateTimeOffset.FromUnixTimeMilliseconds(long.Parse(spl[0]));
                update.Bits = spl.Skip(1).ToArray();
                if (update.Bits[0] == "beginRace")
                {
                    _startTimestamp = update.Timestamp;
                }
                else if (interpolateMovement)
                {
                    if (update.Bits[0] == "p0")
                    {
                        if (prevUpdate != null)
                        {
                            double endX = Convert.ToDouble(update.Bits[1]), endY = Convert.ToDouble(update.Bits[2]);
                            for (int i = 1; i < _interpFactor; i++)
                            {
                                long time = (long)Lerp(
                                    prevUpdate.Timestamp.ToUnixTimeMilliseconds(),
                                    update.Timestamp.ToUnixTimeMilliseconds(),
                                    (1 / _interpFactor) * i);
                                _updates.Add(new ReplayUpdate
                                {
                                    Timestamp = DateTimeOffset.FromUnixTimeMilliseconds(time),
                                    Bits = new[] { update.Bits[0], (endX / _interpFactor).ToString(), (endY / _interpFactor).ToString() },
                                });
                                Console.WriteLine($"{i} " + _updates.Last());
                            }

                            update.Bits[1] = (endX / _interpFactor).ToString();
                            update.Bits[2] = (endY / _interpFactor).ToString();
                            Console.WriteLine("last " + update);
                        }
                        prevUpdate = (ReplayUpdate)update.Clone();
                    }
                }

                _updates.Add(update);
            }
        }

        double Lerp(double a, double b, double t)
        {
            return a * (1 - t) + b * t;
        }

        (double x, double y) LerpXY(double x1, double y1, double x2, double y2, double t)
        {
            return (Lerp(x1, x2, t), Lerp(y1, y2, t));
        }

        public int PendingUpdates(int frames, int fps)
        {
            if (_tick >= _updates.Count) return -1;

            double msPerFrame = 1000d / fps;
            double msTotalElapsed = msPerFrame * frames;
            var elapsed = _startTimestamp.AddMilliseconds(msTotalElapsed);
            int pending = 0;
            while (_tick+pending < _updates.Count && _updates[_tick+pending].Timestamp <= elapsed)
            {
                pending++;
            }
            return pending;
        }

        public void ProcessUpdates(Game_ART g, int n)
        {
            for (int i=0;i<n;i++,_tick++)
            {
                var update = _updates[_tick];
                var b = update.Bits;
                switch (b[0])
                {
                case "p0":
                    //((RemoteCharacter)g.map.Chars[0]).pos(b.Skip(1).Take(2).ToArray());
                    g.map.Chars[0].X += Convert.ToDouble(b[1]);
                    g.map.Chars[0].Y += Convert.ToDouble(b[2]);
                    Console.WriteLine($"({b[1]},{b[2]})");

                    // if next is exactPos, we need to interpolate here because
                    // we don't have player position info in the preprocessing,
                    // so we can't do it there.
                    //var nextUpdate = _updates[_tick + 1];
                    //if (nextUpdate.Bits[0] == "exactPos0")
                    //{
                    //    double startX = g.map.Chars[0].X, startY = g.map.Chars[0].Y;
                    //    double endX = Convert.ToDouble(nextUpdate.Bits[1]), endY = Convert.ToDouble(nextUpdate.Bits[2]);
                    //    for (int j = 1; j < _interpFactor; j++)
                    //    {
                    //        long fromTime = update.Timestamp.ToUnixTimeMilliseconds();
                    //        long toTime = nextUpdate.Timestamp.ToUnixTimeMilliseconds();
                    //        long time = (long)Lerp(fromTime, toTime, (1 / _interpFactor) * j);

                    //        var (x, y) = LerpXY(startX, startY, endX, endY, (1 / _interpFactor) * j);

                    //        var interped = new ReplayUpdate
                    //        {
                    //            Timestamp = DateTimeOffset.FromUnixTimeMilliseconds(time),
                    //            Bits = new[] { update.Bits[0], x.ToString(), y.ToString() },
                    //        };
                    //        _updates.Insert(_tick + 1, interped);
                    //        Console.WriteLine($"exact {j} " + interped);
                    //    }

                    //    //update.Bits[1] = (endX / _interpFactor).ToString();
                    //    //update.Bits[2] = (endY / _interpFactor).ToString();
                    //    //Console.WriteLine("last " + update);
                    //}
                    break;
                case "exactPos0":
                    //((RemoteCharacter)g.map.Chars[0]).exactPos(b.Skip(1).Take(2).ToArray());
                    g.map.Chars[0].X = Convert.ToDouble(b[1]);
                    g.map.Chars[0].Y = Convert.ToDouble(b[2]);
                    break;
                case "var0":
                    if (b[1] == "state") g.map.Chars[0].method_11(b[2]);
                    else if (b[1] == "scaleX") g.map.Chars[0].ScaleX = Convert.ToInt32(b[2]);
                    else if (b[1] == "item") g.map.Chars[0].cItem = Convert.ToInt32(b[2]);
                    else if (b[1] == "rot")
                    {
                        var rot = Convert.ToInt32(b[2]);
                        //g.map.SetRotation(rot);
                        g.map.Chars[0].RotateTo = rot;
                    }
                    break;
                case "activate":
                    var bx = Convert.ToInt32(b[1]);
                    var by = Convert.ToInt32(b[2]);
                    var data = b[3];
                    var block = g.map.getBlock(bx, by, 0);
                    block.RemoteActivate(data);
                    break;
                case "addEffect":
                    if (b[1] == "Laser")
                    {
                        var x = Convert.ToInt32(b[2]);
                        var y = Convert.ToInt32(b[3]);
                        var dir = b[4];
                        var rot = Convert.ToInt32(b[5]);
                        var id = Convert.ToInt32(b[6]);
                        g.map.MakeLaser(x, y, id, dir, rot);
                    }
                    else if (b[1] == "Slash")
                    {
                        var x = Convert.ToInt32(b[2]);
                        var y = Convert.ToInt32(b[3]);
                        //g.map.MakeLaser(x, y, )
                    }
                    else if (b[1] == "Mine")
                    {
                        var x = Convert.ToInt32(b[2]);
                        var y = Convert.ToInt32(b[3]);
                        //g.map.MakeLaser(x, y, )
                    }
                    else if (b[1] == "Hat")
                    {
                        var x = Convert.ToDouble(b[2]);
                        var y = Convert.ToDouble(b[3]);
                        var rot = Convert.ToInt32(b[4]);
                        var hattype = Convert.ToInt32(b[5]);
                        var serverid = Convert.ToInt32(b[8]);
                        g.map.MakeHat((int)x, (int)y, hattype, Color.White, serverid, rot);
                    }
                    else if (b[1] == "IceWave")
                    {
                        var x = Convert.ToInt32(b[2]);
                        var y = Convert.ToInt32(b[3]);
                        //g.map.MakeLaser(x, y, )
                    }
                    break;
                case "createRemoteCharacter":
                    g.AddRemotePlayer();
                    break;
                case "createLocalCharacter":
                    g.AddPlayer();
                    break;
                }
            }
        }
    }
}
