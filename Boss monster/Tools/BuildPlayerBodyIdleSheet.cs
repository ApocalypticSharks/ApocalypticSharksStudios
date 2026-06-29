using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

public static class BuildPlayerBodyIdleSheet
{
    private struct FrameOffset
    {
        public int OffsetX;
        public int OffsetY;
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

    private static void Blit1x1(Bitmap sheet, Bitmap crop, int destX, int destY)
    {
        for (int y = 0; y < crop.Height; y++)
        {
            int ty = destY + y;
            if (ty < 0 || ty >= sheet.Height) continue;
            for (int x = 0; x < crop.Width; x++)
            {
                int tx = destX + x;
                if (tx < 0 || tx >= sheet.Width) continue;
                Color c = crop.GetPixel(x, y);
                if (c.A == 0) continue;
                sheet.SetPixel(tx, ty, c);
            }
        }
    }

    public static void Main(string[] args)
    {
        string root = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ".."));
        string srcPath = args.Length > 0
            ? args[0]
            : Path.Combine(root, "Assets", "Sprites", "Player", "PlayerBody.png");
        string dstPath = args.Length > 1
            ? args[1]
            : Path.Combine(root, "Assets", "Sprites", "Player", "PlayerBody_idle.png");

        if (!File.Exists(srcPath))
        {
            Console.WriteLine("Missing source: " + srcPath);
            return;
        }

        const int frameCount = 4;

        using (var source = Load32(srcPath))
        {
            RemoveCheckerboard(source);
            Rectangle? boundsOpt = GetAlphaBounds(source);
            if (boundsOpt == null)
            {
                Console.WriteLine("No opaque pixels in " + srcPath);
                return;
            }

            Rectangle bounds = boundsOpt.Value;
            int bobPx = Math.Max(4, (int)Math.Round(bounds.Height * 0.022));
            int halfBob = Math.Max(2, bobPx / 2);

            var idleOffsets = new[]
            {
                new FrameOffset { OffsetX = 0, OffsetY = 0 },
                new FrameOffset { OffsetX = 0, OffsetY = -halfBob },
                new FrameOffset { OffsetX = 0, OffsetY = -bobPx },
                new FrameOffset { OffsetX = 0, OffsetY = -halfBob },
            };

            int maxUp = bobPx;

            using (var crop = Crop(source, bounds))
            {
                int frameW = bounds.Width;
                int frameH = bounds.Height + maxUp;
                int sheetW = frameW * frameCount;
                int sheetH = frameH;

                using (var sheet = new Bitmap(sheetW, sheetH, PixelFormat.Format32bppArgb))
                {
                    for (int y = 0; y < sheetH; y++)
                    {
                        for (int x = 0; x < sheetW; x++)
                            sheet.SetPixel(x, y, Color.FromArgb(0, 0, 0, 0));
                    }

                    for (int i = 0; i < frameCount; i++)
                    {
                        int slotX = i * frameW;
                        int destY = maxUp + idleOffsets[i].OffsetY;
                        Blit1x1(sheet, crop, slotX + idleOffsets[i].OffsetX, destY);
                    }

                    sheet.Save(dstPath, ImageFormat.Png);
                }

                Console.WriteLine("bounds=" + bounds.Width + "x" + bounds.Height);
                Console.WriteLine("bobPx=" + bobPx);
                Console.WriteLine("sheet=" + sheetW + "x" + sheetH + " (1:1, no scale)");
            }
        }

        PlayerSpritePipeline.ProcessBodySheet(dstPath, dstPath, frameCount, false);
        Console.WriteLine("Built idle sheet: " + dstPath);
    }
}
