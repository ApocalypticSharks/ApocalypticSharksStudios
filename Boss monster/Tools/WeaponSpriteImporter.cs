using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;

public static class WeaponSpriteImporter
{
    private static bool IsColored(int r, int g, int b)
    {
        if (r > 18 || g > 18 || b > 18) return true;
        if (r + g + b > 55) return true;
        int spread = Math.Max(Math.Abs(r - g), Math.Max(Math.Abs(g - b), Math.Abs(r - b)));
        return spread > 10 && (r + g + b) > 35;
    }

    private static bool IsBlack(int r, int g, int b)
    {
        return r <= 10 && g <= 10 && b <= 10;
    }

    private static Bitmap Load32(string path)
    {
        byte[] bytes = File.ReadAllBytes(path);
        using (var ms = new MemoryStream(bytes))
        using (var src = new Bitmap(ms))
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

    private static int Idx(int x, int y, int stride)
    {
        return y * stride + x * 4;
    }

    private static byte[] ReadPx(Bitmap bmp, out int stride)
    {
        var rect = new Rectangle(0, 0, bmp.Width, bmp.Height);
        var data = bmp.LockBits(rect, ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
        stride = data.Stride;
        var px = new byte[Math.Abs(stride) * bmp.Height];
        System.Runtime.InteropServices.Marshal.Copy(data.Scan0, px, 0, px.Length);
        bmp.UnlockBits(data);
        return px;
    }

    private static void WritePx(Bitmap bmp, byte[] px)
    {
        var rect = new Rectangle(0, 0, bmp.Width, bmp.Height);
        var data = bmp.LockBits(rect, ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
        System.Runtime.InteropServices.Marshal.Copy(px, 0, data.Scan0, px.Length);
        bmp.UnlockBits(data);
    }

    private static void ZeroTransparent(byte[] px)
    {
        for (int i = 0; i < px.Length; i += 4)
        {
            if (px[i + 3] == 0)
            {
                px[i] = 0;
                px[i + 1] = 0;
                px[i + 2] = 0;
            }
        }
    }

    public static Size Import(string srcPath, string dstPath, int maxW, int maxH)
    {
        using (var bmp = Load32(srcPath))
        {
            int w = bmp.Width;
            int h = bmp.Height;
            int stride;
            byte[] px = ReadPx(bmp, out stride);

            bool[,] fg = new bool[w, h];
            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    int o = Idx(x, y, stride);
                    int b = px[o];
                    int g = px[o + 1];
                    int r = px[o + 2];
                    int a = px[o + 3];
                    if (a < 20) continue;
                    int avg = (r + g + b) / 3;
                    if (IsColored(r, g, b)) fg[x, y] = true;
                    else if (avg > 22 && a > 80) fg[x, y] = true;
                }
            }

            int[] dx8 = { -1, 0, 1, -1, 1, -1, 0, 1 };
            int[] dy8 = { -1, -1, -1, 0, 0, 1, 1, 1 };
            for (int pass = 0; pass < 2; pass++)
            {
                bool[,] next = (bool[,])fg.Clone();
                for (int y = 0; y < h; y++)
                {
                    for (int x = 0; x < w; x++)
                    {
                        if (fg[x, y]) continue;
                        for (int i = 0; i < 8; i++)
                        {
                            int nx = x + dx8[i];
                            int ny = y + dy8[i];
                            if (nx < 0 || ny < 0 || nx >= w || ny >= h) continue;
                            if (fg[nx, ny])
                            {
                                next[x, y] = true;
                                break;
                            }
                        }
                    }
                }
                fg = next;
            }

            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    int o = Idx(x, y, stride);
                    if (!fg[x, y])
                    {
                        px[o] = 0;
                        px[o + 1] = 0;
                        px[o + 2] = 0;
                        px[o + 3] = 0;
                        continue;
                    }
                    px[o + 3] = 255;
                }
            }

            bool[,] seen = new bool[w, h];
            int[] dx4 = { 1, -1, 0, 0 };
            int[] dy4 = { 0, 0, 1, -1 };
            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    int o = Idx(x, y, stride);
                    if (px[o + 3] == 0 || seen[x, y]) continue;
                    int r = px[o + 2];
                    int g = px[o + 1];
                    int b = px[o];
                    if (!IsBlack(r, g, b)) continue;

                    var q = new Queue<Point>();
                    var comp = new List<Point>();
                    q.Enqueue(new Point(x, y));
                    seen[x, y] = true;
                    bool touchesColored = false;
                    while (q.Count > 0)
                    {
                        Point p = q.Dequeue();
                        comp.Add(p);
                        for (int i = 0; i < 4; i++)
                        {
                            int nx = p.X + dx4[i];
                            int ny = p.Y + dy4[i];
                            if (nx < 0 || ny < 0 || nx >= w || ny >= h) continue;
                            int no = Idx(nx, ny, stride);
                            if (px[no + 3] == 0) continue;
                            int nr = px[no + 2];
                            int ng = px[no + 1];
                            int nb = px[no];
                            if (!IsBlack(nr, ng, nb))
                            {
                                if (IsColored(nr, ng, nb)) touchesColored = true;
                                continue;
                            }
                            if (seen[nx, ny]) continue;
                            seen[nx, ny] = true;
                            q.Enqueue(new Point(nx, ny));
                        }
                    }
                    if (!touchesColored)
                    {
                        foreach (Point p in comp)
                        {
                            int po = Idx(p.X, p.Y, stride);
                            px[po] = 0;
                            px[po + 1] = 0;
                            px[po + 2] = 0;
                            px[po + 3] = 0;
                        }
                    }
                }
            }

            ZeroTransparent(px);
            WritePx(bmp, px);

            px = ReadPx(bmp, out stride);
            int minX = w;
            int minY = h;
            int maxX = -1;
            int maxY = -1;
            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    if (px[Idx(x, y, stride) + 3] == 0) continue;
                    minX = Math.Min(minX, x);
                    minY = Math.Min(minY, y);
                    maxX = Math.Max(maxX, x);
                    maxY = Math.Max(maxY, y);
                }
            }
            if (maxX < minX) return Size.Empty;

            minX = Math.Max(0, minX - 2);
            minY = Math.Max(0, minY - 2);
            maxX = Math.Min(w - 1, maxX + 2);
            maxY = Math.Min(h - 1, maxY + 2);
            Rectangle crop = Rectangle.FromLTRB(minX, minY, maxX + 1, maxY + 1);

            float scale = Math.Min(1f, Math.Min(maxW / (float)crop.Width, maxH / (float)crop.Height));
            int ow = Math.Max(1, (int)Math.Round(crop.Width * scale));
            int oh = Math.Max(1, (int)Math.Round(crop.Height * scale));
            using (var dst = new Bitmap(ow, oh, PixelFormat.Format32bppArgb))
            using (var g = Graphics.FromImage(dst))
            {
                g.Clear(Color.Transparent);
                g.InterpolationMode = InterpolationMode.NearestNeighbor;
                g.PixelOffsetMode = PixelOffsetMode.Half;
                g.DrawImage(bmp, new Rectangle(0, 0, ow, oh), crop, GraphicsUnit.Pixel);
                byte[] outPx = ReadPx(dst, out stride);
                ZeroTransparent(outPx);
                WritePx(dst, outPx);
                dst.Save(dstPath, ImageFormat.Png);
            }
            return new Size(ow, oh);
        }
    }
}
