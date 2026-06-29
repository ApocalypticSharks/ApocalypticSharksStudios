using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

public static class BuildPlayerAnimationSheets
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

    private static FrameOffset[] BuildIdleOffsets(int height)
    {
        int bobPx = Math.Max(4, (int)Math.Round(height * 0.022));
        int halfBob = Math.Max(2, bobPx / 2);
        return new[]
        {
            new FrameOffset { OffsetX = 0, OffsetY = 0 },
            new FrameOffset { OffsetX = 0, OffsetY = -halfBob },
            new FrameOffset { OffsetX = 0, OffsetY = -bobPx },
            new FrameOffset { OffsetX = 0, OffsetY = -halfBob },
        };
    }

    private static FrameOffset[] BuildWalkOffsets(int height)
    {
        int swayX = Math.Max(3, (int)Math.Round(height * 0.015));
        int stepDown = Math.Max(2, (int)Math.Round(height * 0.008));
        int liftUp = Math.Max(3, (int)Math.Round(height * 0.012));
        return new[]
        {
            new FrameOffset { OffsetX = -swayX, OffsetY = stepDown },
            new FrameOffset { OffsetX = 0, OffsetY = -liftUp },
            new FrameOffset { OffsetX = swayX, OffsetY = stepDown },
            new FrameOffset { OffsetX = 0, OffsetY = -Math.Max(2, liftUp / 2) },
        };
    }

    private static FrameOffset[] BuildArmWalkOffsets(int height)
    {
        int swayX = Math.Max(3, (int)Math.Round(height * 0.015));
        int stepDown = Math.Max(2, (int)Math.Round(height * 0.008));
        int liftUp = Math.Max(3, (int)Math.Round(height * 0.012));
        return new[]
        {
            new FrameOffset { OffsetX = swayX, OffsetY = stepDown },
            new FrameOffset { OffsetX = 0, OffsetY = -liftUp },
            new FrameOffset { OffsetX = -swayX, OffsetY = stepDown },
            new FrameOffset { OffsetX = 0, OffsetY = -Math.Max(2, liftUp / 2) },
        };
    }

    private static int ComputePadding(FrameOffset[] frames)
    {
        int padLeft = 0;
        int padRight = 0;
        int padUp = 0;
        int padDown = 0;
        foreach (FrameOffset f in frames)
        {
            padLeft = Math.Max(padLeft, -f.OffsetX);
            padRight = Math.Max(padRight, f.OffsetX);
            padUp = Math.Max(padUp, -f.OffsetY);
            padDown = Math.Max(padDown, f.OffsetY);
        }
        return Math.Max(Math.Max(padLeft, padRight), Math.Max(padUp, padDown));
    }

    private static void BuildSheet(string srcPath, string dstPath, FrameOffset[] frames)
    {
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
            int pad = ComputePadding(frames);

            using (var crop = Crop(source, bounds))
            {
                int frameW = bounds.Width + pad * 2;
                int frameH = bounds.Height + pad * 2;
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
                        int destX = slotX + pad + frames[i].OffsetX;
                        int destY = pad + frames[i].OffsetY;
                        Blit1x1(sheet, crop, destX, destY);
                    }

                    sheet.Save(dstPath, ImageFormat.Png);
                }

                Console.WriteLine(Path.GetFileName(dstPath) + " bounds=" + bounds.Width + "x" + bounds.Height + " sheet=" + sheetW + "x" + sheetH);
            }
        }

        PlayerSpritePipeline.ProcessBodySheet(dstPath, dstPath, frameCount, false);
    }

    public static void Main(string[] args)
    {
        string root = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ".."));
        string bodySrc = Path.Combine(root, "Assets", "Sprites", "Player", "PlayerBody.png");
        string armSrc = Path.Combine(root, "Assets", "Sprites", "Player", "medic_arm_full.png");
        string bodyIdleDst = Path.Combine(root, "Assets", "Sprites", "Player", "PlayerBody_idle.png");
        string bodyWalkDst = Path.Combine(root, "Assets", "Sprites", "Player", "PlayerBody_walk.png");
        string armIdleDst = Path.Combine(root, "Assets", "Sprites", "Player", "medic_arm_full_idle.png");
        string armWalkDst = Path.Combine(root, "Assets", "Sprites", "Player", "medic_arm_full_walk.png");

        if (!File.Exists(bodySrc))
        {
            Console.WriteLine("Missing: " + bodySrc);
            return;
        }

        using (var bodyProbe = Load32(bodySrc))
        {
            RemoveCheckerboard(bodyProbe);
            Rectangle? bodyBounds = GetAlphaBounds(bodyProbe);
            if (bodyBounds == null)
            {
                Console.WriteLine("Body has no opaque pixels.");
                return;
            }

            BuildSheet(bodySrc, bodyIdleDst, BuildIdleOffsets(bodyBounds.Value.Height));
            BuildSheet(bodySrc, bodyWalkDst, BuildWalkOffsets(bodyBounds.Value.Height));
        }

        if (File.Exists(armSrc))
        {
            using (var armProbe = Load32(armSrc))
            {
                RemoveCheckerboard(armProbe);
                Rectangle? armBounds = GetAlphaBounds(armProbe);
                if (armBounds != null)
                {
                    BuildSheet(armSrc, armIdleDst, BuildIdleOffsets(armBounds.Value.Height));
                    BuildSheet(armSrc, armWalkDst, BuildArmWalkOffsets(armBounds.Value.Height));
                }
            }
        }
        else
        {
            Console.WriteLine("Skipped arm sheets, missing: " + armSrc);
        }

        Console.WriteLine("Done.");
    }
}
