using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

public static class BuildPlayerWalkLegSheet
{
    private struct LegShift
    {
        public int BackDx;
        public int BackDy;
        public int FrontDx;
        public int FrontDy;
        public int TorsoDy;
    }

    private class Pixel
    {
        public int X;
        public int Y;
        public Color C;
    }

    private static bool IsCheckerPixel(int r, int g, int b, int a)
    {
        if (a < 16) return true;
        int avg = (r + g + b) / 3;
        int spread = Math.Max(Math.Abs(r - g), Math.Max(Math.Abs(g - b), Math.Abs(r - b)));
        if (spread > 55) return false;
        if (avg >= 165 && spread <= 48) return true;
        if (avg >= 95 && avg <= 155 && spread <= 42) return true;
        return false;
    }

    private static bool IsPouchPixel(Color c)
    {
        if (c.A < 16) return false;
        int r = c.R, g = c.G, b = c.B;
        if (r < 25 || r > 145) return false;
        if (g < 15 || g > 110) return false;
        if (b < 8 || b > 95) return false;
        if (r <= g + 3) return false;
        if (r + g + b < 60) return false;
        if (r > 90 && g < 35 && b < 35) return false;
        return true;
    }

    private static bool IsBootPixel(Color c)
    {
        if (c.A < 16) return false;
        if (IsPouchPixel(c)) return false;
        int avg = (c.R + c.G + c.B) / 3;
        return avg < 95;
    }

    private static void RemoveCheckerboard(Bitmap bmp)
    {
        int w = bmp.Width;
        int h = bmp.Height;
        var rect = new Rectangle(0, 0, w, h);
        var data = bmp.LockBits(rect, ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
        int stride = data.Stride;
        var px = new byte[Math.Abs(stride) * h];
        System.Runtime.InteropServices.Marshal.Copy(data.Scan0, px, 0, px.Length);

        bool[,] bg = new bool[w, h];
        var q = new Queue<Point>();
        int[] dx4 = { 1, -1, 0, 0 };
        int[] dy4 = { 0, 0, 1, -1 };

        Action<int, int> tryAdd = (x, y) =>
        {
            if (x < 0 || y < 0 || x >= w || y >= h || bg[x, y]) return;
            int o = y * stride + x * 4;
            int b = px[o];
            int g = px[o + 1];
            int r = px[o + 2];
            int a = px[o + 3];
            if (!IsCheckerPixel(r, g, b, a)) return;
            bg[x, y] = true;
            q.Enqueue(new Point(x, y));
        };

        for (int x = 0; x < w; x++)
        {
            tryAdd(x, 0);
            tryAdd(x, h - 1);
        }
        for (int y = 0; y < h; y++)
        {
            tryAdd(0, y);
            tryAdd(w - 1, y);
        }

        while (q.Count > 0)
        {
            Point p = q.Dequeue();
            for (int i = 0; i < 4; i++)
                tryAdd(p.X + dx4[i], p.Y + dy4[i]);
        }

        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                int o = y * stride + x * 4;
                if (bg[x, y])
                {
                    px[o] = px[o + 1] = px[o + 2] = px[o + 3] = 0;
                    continue;
                }
                if (px[o + 3] > 0)
                    px[o + 3] = 255;
            }
        }

        System.Runtime.InteropServices.Marshal.Copy(px, 0, data.Scan0, px.Length);
        bmp.UnlockBits(data);
    }

    private static Bitmap Load32(string path)
    {
        using (var src = new Bitmap(path))
        {
            var bmp = new Bitmap(src.Width, src.Height, PixelFormat.Format32bppArgb);
            using (var g = Graphics.FromImage(bmp))
            {
                g.Clear(Color.Transparent);
                g.DrawImage(src, 0, 0, src.Width, src.Height);
            }
            return bmp;
        }
    }

    private static Rectangle? GetAlphaBounds(Bitmap bmp)
    {
        int minX = bmp.Width, minY = bmp.Height, maxX = -1, maxY = -1;
        for (int y = 0; y < bmp.Height; y++)
        {
            for (int x = 0; x < bmp.Width; x++)
            {
                Color c = bmp.GetPixel(x, y);
                if (c.A < 16) continue;
                minX = Math.Min(minX, x);
                minY = Math.Min(minY, y);
                maxX = Math.Max(maxX, x);
                maxY = Math.Max(maxY, y);
            }
        }

        if (maxX < minX) return null;
        return Rectangle.FromLTRB(minX, minY, maxX + 1, maxY + 1);
    }

    private static Bitmap Crop(Bitmap src, Rectangle bounds)
    {
        var crop = new Bitmap(bounds.Width, bounds.Height, PixelFormat.Format32bppArgb);
        for (int y = 0; y < bounds.Height; y++)
        {
            for (int x = 0; x < bounds.Width; x++)
            {
                Color c = src.GetPixel(bounds.X + x, bounds.Y + y);
                crop.SetPixel(x, y, c.A < 16 ? Color.FromArgb(0, 0, 0, 0) : Color.FromArgb(255, c.R, c.G, c.B));
            }
        }
        return crop;
    }

    private static void SetTransparent(Bitmap bmp, int x, int y)
    {
        if (x < 0 || y < 0 || x >= bmp.Width || y >= bmp.Height) return;
        bmp.SetPixel(x, y, Color.FromArgb(0, 0, 0, 0));
    }

    private static void SetPixel(Bitmap bmp, int x, int y, Color c)
    {
        if (x < 0 || y < 0 || x >= bmp.Width || y >= bmp.Height) return;
        if (c.A < 16) return;
        bmp.SetPixel(x, y, Color.FromArgb(255, c.R, c.G, c.B));
    }

    private static LegShift[] BuildWalkShifts(int height)
    {
        int stride = Math.Max(10, (int)Math.Round(height * 0.024));
        int half = Math.Max(5, stride / 2);
        int lift = Math.Max(6, (int)Math.Round(height * 0.012));
        int mid = Math.Max(3, stride / 3);
        int bob = Math.Max(2, (int)Math.Round(height * 0.004));

        return new[]
        {
            new LegShift { BackDx = -half, BackDy = 0, FrontDx = stride, FrontDy = 0, TorsoDy = 0 },
            new LegShift { BackDx = 0, BackDy = 0, FrontDx = mid, FrontDy = -lift, TorsoDy = -bob },
            new LegShift { BackDx = stride, BackDy = 0, FrontDx = -half, FrontDy = 0, TorsoDy = 0 },
            new LegShift { BackDx = mid, BackDy = -lift, FrontDx = 0, FrontDy = 0, TorsoDy = -bob },
        };
    }

    private static LegShift[] BuildArmWalkShifts(int height)
    {
        int swing = Math.Max(8, (int)Math.Round(height * 0.018));
        int half = Math.Max(4, swing / 2);
        int lift = Math.Max(4, (int)Math.Round(height * 0.008));

        return new[]
        {
            new LegShift { BackDx = -swing, BackDy = half, FrontDx = 0, FrontDy = 0, TorsoDy = 0 },
            new LegShift { BackDx = -half, BackDy = -lift, FrontDx = 0, FrontDy = 0, TorsoDy = 0 },
            new LegShift { BackDx = swing, BackDy = half, FrontDx = 0, FrontDy = 0, TorsoDy = 0 },
            new LegShift { BackDx = half, BackDy = -lift, FrontDx = 0, FrontDy = 0, TorsoDy = 0 },
        };
    }

    private static Bitmap BuildLegWalkFrame(Bitmap source, List<Pixel> backLeg, List<Pixel> frontLeg, int footTop, LegShift shift, int pad)
    {
        int w = source.Width + pad * 2;
        int h = source.Height + pad * 2;
        var frame = new Bitmap(w, h, PixelFormat.Format32bppArgb);

        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
                frame.SetPixel(x, y, Color.FromArgb(0, 0, 0, 0));
        }

        for (int y = 0; y < source.Height; y++)
        {
            for (int x = 0; x < source.Width; x++)
            {
                if (y >= footTop && (IsBootPixel(source.GetPixel(x, y))))
                    continue;
                Color c = source.GetPixel(x, y);
                if (c.A < 16) continue;
                SetPixel(frame, x + pad, y + pad + shift.TorsoDy, c);
            }
        }

        foreach (Pixel p in backLeg)
            SetPixel(frame, p.X + pad + shift.BackDx, p.Y + pad + shift.BackDy, p.C);

        foreach (Pixel p in frontLeg)
            SetPixel(frame, p.X + pad + shift.FrontDx, p.Y + pad + shift.FrontDy, p.C);

        return frame;
    }

    private static Bitmap BuildArmWalkFrame(Bitmap source, List<Pixel> armPixels, int armTop, LegShift shift, int pad)
    {
        int w = source.Width + pad * 2;
        int h = source.Height + pad * 2;
        var frame = new Bitmap(w, h, PixelFormat.Format32bppArgb);

        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
                frame.SetPixel(x, y, Color.FromArgb(0, 0, 0, 0));
        }

        var armSet = new HashSet<long>();
        foreach (Pixel p in armPixels)
            armSet.Add(((long)p.X << 32) | (uint)p.Y);

        for (int y = 0; y < source.Height; y++)
        {
            for (int x = 0; x < source.Width; x++)
            {
                long key = ((long)x << 32) | (uint)y;
                if (y >= armTop && armSet.Contains(key))
                    continue;
                Color c = source.GetPixel(x, y);
                if (c.A < 16) continue;
                SetPixel(frame, x + pad, y + pad, c);
            }
        }

        int dx = shift.BackDx;
        int dy = shift.BackDy;
        foreach (Pixel p in armPixels)
            SetPixel(frame, p.X + pad + dx, p.Y + pad + dy, p.C);

        return frame;
    }

    private static int ComputePad(LegShift[] shifts, int legStride)
    {
        int pad = legStride + 8;
        foreach (LegShift s in shifts)
        {
            pad = Math.Max(pad, Math.Abs(s.BackDx) + 8);
            pad = Math.Max(pad, Math.Abs(s.FrontDx) + 8);
            pad = Math.Max(pad, Math.Abs(s.BackDy) + 8);
            pad = Math.Max(pad, Math.Abs(s.FrontDy) + 8);
            pad = Math.Max(pad, Math.Abs(s.TorsoDy) + 4);
        }
        return pad;
    }

    private static void BuildBodyWalk(string srcPath, string dstPath)
    {
        const int frameCount = 4;

        using (var loaded = Load32(srcPath))
        {
            RemoveCheckerboard(loaded);
            Rectangle? boundsOpt = GetAlphaBounds(loaded);
            if (boundsOpt == null)
            {
                Console.WriteLine("No opaque pixels in " + srcPath);
                return;
            }

            Rectangle bounds = boundsOpt.Value;
            using (var source = Crop(loaded, bounds))
            {
                int h = source.Height;
                int w = source.Width;
                int footTop = (int)Math.Round(h * 0.74);
                int splitX = (int)Math.Round(w * 0.50);

                var backLeg = new List<Pixel>();
                var frontLeg = new List<Pixel>();

                for (int y = footTop; y < h; y++)
                {
                    for (int x = 0; x < w; x++)
                    {
                        Color c = source.GetPixel(x, y);
                        if (!IsBootPixel(c)) continue;
                        var p = new Pixel { X = x, Y = y, C = Color.FromArgb(255, c.R, c.G, c.B) };
                        if (x < splitX) backLeg.Add(p);
                        else frontLeg.Add(p);
                    }
                }

                LegShift[] shifts = BuildWalkShifts(h);
                int stride = Math.Max(10, (int)Math.Round(h * 0.024));
                int pad = ComputePad(shifts, stride);
                int frameW = w + pad * 2;
                int frameH = h + pad * 2;

                using (var sheet = new Bitmap(frameW * frameCount, frameH, PixelFormat.Format32bppArgb))
                {
                    for (int y = 0; y < sheet.Height; y++)
                    {
                        for (int x = 0; x < sheet.Width; x++)
                            sheet.SetPixel(x, y, Color.FromArgb(0, 0, 0, 0));
                    }

                    for (int i = 0; i < frameCount; i++)
                    {
                        using (var frame = BuildLegWalkFrame(source, backLeg, frontLeg, footTop, shifts[i], pad))
                        {
                            for (int y = 0; y < frame.Height; y++)
                            {
                                for (int x = 0; x < frame.Width; x++)
                                {
                                    Color c = frame.GetPixel(x, y);
                                    if (c.A == 0) continue;
                                    sheet.SetPixel(i * frameW + x, y, c);
                                }
                            }
                        }
                    }

                    sheet.Save(dstPath, ImageFormat.Png);
                }

                Console.WriteLine(Path.GetFileName(dstPath) + " frame=" + frameW + "x" + frameH + " legs back=" + backLeg.Count + " front=" + frontLeg.Count);
            }
        }

        PlayerSpritePipeline.ProcessBodySheet(dstPath, dstPath, frameCount, false);
    }

    private static void BuildArmWalk(string srcPath, string dstPath)
    {
        const int frameCount = 4;

        using (var loaded = Load32(srcPath))
        {
            RemoveCheckerboard(loaded);
            Rectangle? boundsOpt = GetAlphaBounds(loaded);
            if (boundsOpt == null) return;

            Rectangle bounds = boundsOpt.Value;
            using (var source = Crop(loaded, bounds))
            {
                int h = source.Height;
                int w = source.Width;
                int armTop = (int)Math.Round(h * 0.12);

                var armPixels = new List<Pixel>();
                for (int y = armTop; y < h; y++)
                {
                    for (int x = 0; x < w; x++)
                    {
                        Color c = source.GetPixel(x, y);
                        if (c.A < 16) continue;
                        armPixels.Add(new Pixel { X = x, Y = y, C = Color.FromArgb(255, c.R, c.G, c.B) });
                    }
                }

                LegShift[] shifts = BuildArmWalkShifts(h);
                int pad = ComputePad(shifts, Math.Max(8, (int)Math.Round(h * 0.018)));
                int frameW = w + pad * 2;
                int frameH = h + pad * 2;

                using (var sheet = new Bitmap(frameW * frameCount, frameH, PixelFormat.Format32bppArgb))
                {
                    for (int y = 0; y < sheet.Height; y++)
                    {
                        for (int x = 0; x < sheet.Width; x++)
                            sheet.SetPixel(x, y, Color.FromArgb(0, 0, 0, 0));
                    }

                    for (int i = 0; i < frameCount; i++)
                    {
                        using (var frame = BuildArmWalkFrame(source, armPixels, armTop, shifts[i], pad))
                        {
                            for (int y = 0; y < frame.Height; y++)
                            {
                                for (int x = 0; x < frame.Width; x++)
                                {
                                    Color c = frame.GetPixel(x, y);
                                    if (c.A == 0) continue;
                                    sheet.SetPixel(i * frameW + x, y, c);
                                }
                            }
                        }
                    }

                    sheet.Save(dstPath, ImageFormat.Png);
                }

                Console.WriteLine(Path.GetFileName(dstPath) + " frame=" + frameW + "x" + frameH);
            }
        }

        PlayerSpritePipeline.ProcessBodySheet(dstPath, dstPath, frameCount, false);
    }

    public static void Main(string[] args)
    {
        string root = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ".."));
        string bodySrc = Path.Combine(root, "Assets", "Sprites", "Player", "PlayerBody.png");
        string armSrc = Path.Combine(root, "Assets", "Sprites", "Player", "medic_arm_full.png");
        string bodyWalkDst = Path.Combine(root, "Assets", "Sprites", "Player", "PlayerBody_walk.png");
        string armWalkDst = Path.Combine(root, "Assets", "Sprites", "Player", "medic_arm_full_walk.png");

        if (!File.Exists(bodySrc))
        {
            Console.WriteLine("Missing: " + bodySrc);
            return;
        }

        BuildBodyWalk(bodySrc, bodyWalkDst);

        if (File.Exists(armSrc))
            BuildArmWalk(armSrc, armWalkDst);
        else
            Console.WriteLine("Skipped arm walk, missing: " + armSrc);

        Console.WriteLine("Done.");
    }
}
