using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

public static class ExtractArmSegment
{
    private static bool IsBackground(Color c)
    {
        if (c.A < 16) return true;
        if (c.R > 175 && c.G > 175 && c.B > 175) return true;
        return false;
    }

    private static bool IsCrossPixel(Color c)
    {
        if (IsBackground(c)) return false;
        int max = Math.Max(c.R, Math.Max(c.G, c.B));
        int min = Math.Min(c.R, Math.Min(c.G, c.B));
        if (max < 80) return false;
        if (c.R >= c.G + 18 && c.R >= c.B + 18 && c.R >= 90) return true;
        if (c.R >= 110 && c.G <= 70 && c.B <= 70) return true;
        return false;
    }

    private static bool IsPouchPixel(Color c)
    {
        if (IsBackground(c)) return false;
        int r = c.R, g = c.G, b = c.B;
        if (r < 25 || r > 145) return false;
        if (g < 15 || g > 110) return false;
        if (b < 8 || b > 95) return false;
        if (r <= g + 3) return false;
        if (r + g + b < 60) return false;
        if (r > 90 && g < 35 && b < 35) return false;
        return true;
    }

    private static bool IsArmTone(Color c, int y, int pouchTop, int pouchBottom)
    {
        if (IsBackground(c)) return false;
        if (y >= pouchTop && y <= pouchBottom && IsPouchPixel(c)) return false;
        if (IsCrossPixel(c)) return true;

        int avg = (c.R + c.G + c.B) / 3;
        if (avg > 165) return false;
        if (avg < 12) return false;

        int spread = Math.Max(Math.Abs(c.R - c.G), Math.Max(Math.Abs(c.G - c.B), Math.Abs(c.R - c.B)));
        if (spread > 70) return false;
        return true;
    }

    public static void Main()
    {
        string root = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ".."));
        string srcPath = Path.Combine(root, "Assets", "Sprites", "Player", "image-1b10e8c7-e127-4548-9ff7-75024a1ce793.png");
        string dstPath = Path.Combine(root, "Assets", "Sprites", "Player", "soldier_arm_side.png");

        using (var src = new Bitmap(srcPath))
        {
            int charMinX = src.Width, charMinY = src.Height, charMaxX = -1, charMaxY = -1;
            for (int y = 0; y < src.Height; y++)
            {
                for (int x = 0; x < src.Width; x++)
                {
                    if (IsBackground(src.GetPixel(x, y))) continue;
                    charMinX = Math.Min(charMinX, x);
                    charMinY = Math.Min(charMinY, y);
                    charMaxX = Math.Max(charMaxX, x);
                    charMaxY = Math.Max(charMaxY, y);
                }
            }

            int charW = charMaxX - charMinX + 1;
            int charH = charMaxY - charMinY + 1;

            int crossMinX = src.Width, crossMinY = src.Height, crossMaxX = -1, crossMaxY = -1;
            for (int y = charMinY; y <= charMaxY; y++)
            {
                for (int x = charMinX; x <= charMaxX; x++)
                {
                    if (!IsCrossPixel(src.GetPixel(x, y))) continue;
                    crossMinX = Math.Min(crossMinX, x);
                    crossMinY = Math.Min(crossMinY, y);
                    crossMaxX = Math.Max(crossMaxX, x);
                    crossMaxY = Math.Max(crossMaxY, y);
                }
            }

            if (crossMaxX < crossMinX)
            {
                Console.WriteLine("Cross not found, using fallback region.");
                crossMinX = charMinX + (int)(charW * 0.02f);
                crossMaxX = charMinX + (int)(charW * 0.35f);
                crossMinY = charMinY + (int)(charH * 0.20f);
                crossMaxY = charMinY + (int)(charH * 0.28f);
            }

            int armLeft = Math.Max(charMinX, crossMinX - 18);
            int armRight = Math.Min(charMaxX, crossMaxX + 32);
            int armTop = Math.Max(charMinY, crossMinY - 24);
            int armBottom = Math.Min(charMaxY, crossMinY + (int)(charH * 0.58f));
            int pouchTop = crossMinY + (int)(charH * 0.34f);
            int pouchBottom = crossMinY + (int)(charH * 0.52f);

            bool[,] keep = new bool[src.Width, src.Height];
            var q = new Queue<Point>();

            Action<int, int> seed = (x, y) =>
            {
                if (x < armLeft || x > armRight || y < armTop || y > armBottom) return;
                if (keep[x, y]) return;
                if (!IsArmTone(src.GetPixel(x, y), y, pouchTop, pouchBottom)) return;
                keep[x, y] = true;
                q.Enqueue(new Point(x, y));
            };

            for (int y = crossMinY; y <= crossMaxY; y++)
            {
                for (int x = crossMinX; x <= crossMaxX; x++)
                {
                    if (IsCrossPixel(src.GetPixel(x, y)))
                        seed(x, y);
                }
            }

            int[] dx = { 1, -1, 0, 0, 1, 1, -1, -1 };
            int[] dy = { 0, 0, 1, -1, 1, -1, 1, -1 };
            while (q.Count > 0)
            {
                Point p = q.Dequeue();
                for (int i = 0; i < dx.Length; i++)
                {
                    int nx = p.X + dx[i];
                    int ny = p.Y + dy[i];
                    if (nx < armLeft || nx > armRight || ny < armTop || ny > armBottom) continue;
                    if (keep[nx, ny]) continue;
                    if (!IsArmTone(src.GetPixel(nx, ny), ny, pouchTop, pouchBottom)) continue;
                    keep[nx, ny] = true;
                    q.Enqueue(new Point(nx, ny));
                }
            }

            int minX = src.Width, minY = src.Height, maxX = -1, maxY = -1;
            for (int y = armTop; y <= armBottom; y++)
            {
                for (int x = armLeft; x <= armRight; x++)
                {
                    if (!keep[x, y]) continue;
                    minX = Math.Min(minX, x);
                    minY = Math.Min(minY, y);
                    maxX = Math.Max(maxX, x);
                    maxY = Math.Max(maxY, y);
                }
            }

            if (maxX < minX)
            {
                Console.WriteLine("Arm flood fill failed.");
                return;
            }

            minX = Math.Max(0, minX - 3);
            minY = Math.Max(0, minY - 3);
            maxX = Math.Min(src.Width - 1, maxX + 3);
            maxY = Math.Min(src.Height - 1, maxY + 3);

            int cropW = maxX - minX + 1;
            int cropH = maxY - minY + 1;

            using (var dst = new Bitmap(cropW, cropH, PixelFormat.Format32bppArgb))
            {
                for (int y = 0; y < cropH; y++)
                {
                    for (int x = 0; x < cropW; x++)
                    {
                        int sx = minX + x;
                        int sy = minY + y;
                        if (keep[sx, sy])
                        {
                            Color c = src.GetPixel(sx, sy);
                            dst.SetPixel(x, y, Color.FromArgb(255, c.R, c.G, c.B));
                        }
                        else
                        {
                            dst.SetPixel(x, y, Color.FromArgb(0, 0, 0, 0));
                        }
                    }
                }

                dst.Save(dstPath, ImageFormat.Png);
            }

            Console.WriteLine("Cross: " + crossMinX + "," + crossMinY + " - " + crossMaxX + "," + crossMaxY);
            Console.WriteLine("Arm region: " + armLeft + "," + armTop + " - " + armRight + "," + armBottom);
            Console.WriteLine("Saved bounds: " + minX + "," + minY + " - " + maxX + "," + maxY);
            Console.WriteLine("Saved: " + dstPath);
        }
    }
}
