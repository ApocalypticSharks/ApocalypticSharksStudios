using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

public static class BuildMedicBodyNoArm
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
        if (avg > 165) return true;
        if (avg < 12) return false;

        int spread = Math.Max(Math.Abs(c.R - c.G), Math.Max(Math.Abs(c.G - c.B), Math.Abs(c.R - c.B)));
        if (spread > 70) return false;
        return true;
    }

    private static bool IsCoatPixel(Color c)
    {
        if (IsBackground(c)) return false;
        if (IsPouchPixel(c)) return false;
        if (IsCrossPixel(c)) return false;
        int avg = (c.R + c.G + c.B) / 3;
        if (avg > 140) return false;
        int spread = Math.Max(Math.Abs(c.R - c.G), Math.Max(Math.Abs(c.G - c.B), Math.Abs(c.R - c.B)));
        return spread <= 55;
    }

    private static Color SampleCoatFill(Bitmap src, bool[,] arm, int x, int y, int charMinX)
    {
        for (int sx = x - 1; sx >= charMinX; sx--)
        {
            if (arm[sx, y]) continue;
            Color c = src.GetPixel(sx, y);
            if (IsCoatPixel(c))
                return Color.FromArgb(255, c.R, c.G, c.B);
        }

        for (int dy = 1; dy <= 16; dy++)
        {
            if (y - dy >= 0 && !arm[x, y - dy])
            {
                Color c = src.GetPixel(x, y - dy);
                if (IsCoatPixel(c))
                    return Color.FromArgb(255, c.R, c.G, c.B);
            }
            if (y + dy < src.Height && !arm[x, y + dy])
            {
                Color c = src.GetPixel(x, y + dy);
                if (IsCoatPixel(c))
                    return Color.FromArgb(255, c.R, c.G, c.B);
            }
        }

        return Color.FromArgb(255, 28, 28, 32);
    }

    public static void Main()
    {
        string root = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ".."));
        string srcPath = Path.Combine(root, "Assets", "Sprites", "Player", "image-1b10e8c7-e127-4548-9ff7-75024a1ce793.png");
        string dstPath = Path.Combine(root, "Assets", "Sprites", "Player", "medic_body_no_arm.png");

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
                Console.WriteLine("Cross not found.");
                return;
            }

            int armLeft = Math.Max(charMinX, crossMinX - 18);
            int armRight = Math.Min(charMaxX, crossMaxX + 32);
            int armTop = Math.Max(charMinY, crossMinY - 32);
            int armBottom = Math.Min(charMaxY, crossMinY + (int)(charH * 0.58f));
            int pouchTop = crossMinY + (int)(charH * 0.34f);
            int pouchBottom = crossMinY + (int)(charH * 0.52f);

            bool[,] arm = new bool[src.Width, src.Height];
            var q = new Queue<Point>();

            Action<int, int> seed = (x, y) =>
            {
                if (x < armLeft || x > armRight || y < armTop || y > armBottom) return;
                if (arm[x, y]) return;
                if (!IsArmTone(src.GetPixel(x, y), y, pouchTop, pouchBottom)) return;
                arm[x, y] = true;
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
                    if (arm[nx, ny]) continue;
                    if (!IsArmTone(src.GetPixel(nx, ny), ny, pouchTop, pouchBottom)) continue;
                    arm[nx, ny] = true;
                    q.Enqueue(new Point(nx, ny));
                }
            }

            int gloveTop = pouchBottom + 2;
            for (int y = gloveTop; y <= armBottom; y++)
            {
                for (int x = armLeft; x <= armRight; x++)
                {
                    if (arm[x, y]) continue;
                    Color c = src.GetPixel(x, y);
                    if (IsBackground(c)) continue;
                    if (IsPouchPixel(c)) continue;
                    int avg = (c.R + c.G + c.B) / 3;
                    if (avg > 145)
                        arm[x, y] = true;
                }
            }

            using (var dst = new Bitmap(src.Width, src.Height, PixelFormat.Format32bppArgb))
            {
                for (int y = 0; y < src.Height; y++)
                {
                    for (int x = 0; x < src.Width; x++)
                    {
                        if (arm[x, y])
                        {
                            dst.SetPixel(x, y, SampleCoatFill(src, arm, x, y, charMinX));
                        }
                        else
                        {
                            Color c = src.GetPixel(x, y);
                            if (IsBackground(c))
                                dst.SetPixel(x, y, Color.FromArgb(0, 0, 0, 0));
                            else
                                dst.SetPixel(x, y, Color.FromArgb(255, c.R, c.G, c.B));
                        }
                    }
                }

                dst.Save(dstPath, ImageFormat.Png);
            }

            Console.WriteLine("Saved: " + dstPath);
        }
    }
}
