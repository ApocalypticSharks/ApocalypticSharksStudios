using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;

public static class PlayerSpritePipeline
{
    private static int Idx(int x, int y, int stride)
    {
        return y * stride + x * 4;
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

    private static void RemoveCheckerboard(byte[] px, int w, int h, int stride)
    {
        bool[,] bg = new bool[w, h];
        var q = new Queue<Point>();
        int[] dx4 = { 1, -1, 0, 0 };
        int[] dy4 = { 0, 0, 1, -1 };

        Action<int, int> tryAdd = (x, y) =>
        {
            if (x < 0 || y < 0 || x >= w || y >= h || bg[x, y]) return;
            int o = Idx(x, y, stride);
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
            {
                tryAdd(p.X + dx4[i], p.Y + dy4[i]);
            }
        }

        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                int o = Idx(x, y, stride);
                if (bg[x, y])
                {
                    px[o] = 0;
                    px[o + 1] = 0;
                    px[o + 2] = 0;
                    px[o + 3] = 0;
                    continue;
                }
                if (px[o + 3] > 0)
                {
                    px[o + 3] = 255;
                }
            }
        }
    }

    private static void EraseArmsInFrame(byte[] px, int stride, int imgH, int frameX, int frameW)
    {
        int minX = frameW;
        int minY = imgH;
        int maxX = -1;
        int maxY = -1;
        for (int y = 0; y < imgH; y++)
        {
            for (int x = frameX; x < frameX + frameW; x++)
            {
                if (px[Idx(x, y, stride) + 3] == 0) continue;
                minX = Math.Min(minX, x);
                minY = Math.Min(minY, y);
                maxX = Math.Max(maxX, x);
                maxY = Math.Max(maxY, y);
            }
        }
        if (maxX < minX) return;

        float bw = maxX - minX + 1f;
        float bh = maxY - minY + 1f;
        for (int y = minY; y <= maxY; y++)
        {
            for (int x = minX; x <= maxX; x++)
            {
                int o = Idx(x, y, stride);
                if (px[o + 3] == 0) continue;
                float relX = (x - minX) / bw;
                float relY = (y - minY) / bh;
                if (relX > 0.50f && relX < 0.92f && relY > 0.08f && relY < 0.62f)
                {
                    px[o] = 0;
                    px[o + 1] = 0;
                    px[o + 2] = 0;
                    px[o + 3] = 0;
                }
            }
        }
    }

    public static void ProcessBodySheet(string src, string dst, int frameCount, bool eraseArms)
    {
        using (var bmp = Load32(src))
        {
            int w = bmp.Width;
            int h = bmp.Height;
            int stride;
            byte[] px = ReadPx(bmp, out stride);
            RemoveCheckerboard(px, w, h, stride);

            if (eraseArms)
            {
                int frameW = w / frameCount;
                for (int i = 0; i < frameCount; i++)
                {
                    EraseArmsInFrame(px, stride, h, i * frameW, frameW);
                }
            }

            ZeroTransparent(px);
            WritePx(bmp, px);
            bmp.Save(dst, ImageFormat.Png);
        }
    }

    public static void ProcessBodySheet(string src, string dst, int frameCount)
    {
        ProcessBodySheet(src, dst, frameCount, false);
    }

    public static void ProcessSegment(string src, string dst)
    {
        using (var bmp = Load32(src))
        {
            int w = bmp.Width;
            int h = bmp.Height;
            int stride;
            byte[] px = ReadPx(bmp, out stride);
            RemoveCheckerboard(px, w, h, stride);
            ZeroTransparent(px);
            WritePx(bmp, px);

            int minX = w;
            int minY = h;
            int maxX = -1;
            int maxY = -1;
            px = ReadPx(bmp, out stride);
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
            if (maxX < minX)
            {
                bmp.Save(dst, ImageFormat.Png);
                return;
            }

            minX = Math.Max(0, minX - 2);
            minY = Math.Max(0, minY - 2);
            maxX = Math.Min(w - 1, maxX + 2);
            maxY = Math.Min(h - 1, maxY + 2);
            var crop = Rectangle.FromLTRB(minX, minY, maxX + 1, maxY + 1);
            using (var cropped = new Bitmap(crop.Width, crop.Height, PixelFormat.Format32bppArgb))
            using (var g = Graphics.FromImage(cropped))
            {
                g.Clear(Color.Transparent);
                g.InterpolationMode = InterpolationMode.NearestNeighbor;
                g.DrawImage(bmp, new Rectangle(0, 0, crop.Width, crop.Height), crop, GraphicsUnit.Pixel);
                byte[] cpx = ReadPx(cropped, out stride);
                ZeroTransparent(cpx);
                WritePx(cropped, cpx);
                cropped.Save(dst, ImageFormat.Png);
            }
        }
    }

    public static void ExtractIdleFrame0(string idleSheet, string dst, int frameCount)
    {
        using (var bmp = Load32(idleSheet))
        {
            int frameW = bmp.Width / frameCount;
            var crop = new Rectangle(0, 0, frameW, bmp.Height);
            using (var frame = new Bitmap(crop.Width, crop.Height, PixelFormat.Format32bppArgb))
            using (var g = Graphics.FromImage(frame))
            {
                g.Clear(Color.Transparent);
                g.InterpolationMode = InterpolationMode.NearestNeighbor;
                g.DrawImage(bmp, new Rectangle(0, 0, crop.Width, crop.Height), crop, GraphicsUnit.Pixel);
                frame.Save(dst, ImageFormat.Png);
            }
        }
    }

    private static bool IsPouchColor(int r, int g, int b)
    {
        if (r < 25 || r > 145) return false;
        if (g < 15 || g > 110) return false;
        if (b < 8 || b > 95) return false;
        if (r <= g + 3) return false;
        if (r + g + b < 60) return false;
        // brown/tan leather, not red cross or skin
        if (r > 90 && g < 35 && b < 35) return false;
        return true;
    }

    private static void ClearPouchPixelsInWaist(byte[] px, int stride, int imgH, int frameX, int frameW, int bMinX, int bMinY, int bMaxX, int bMaxY)
    {
        int waistTop = bMinY + (int)((bMaxY - bMinY) * 0.28f);
        int waistBot = bMinY + (int)((bMaxY - bMinY) * 0.58f);

        for (int y = waistTop; y <= waistBot; y++)
        {
            for (int x = frameX; x < frameX + frameW; x++)
            {
                if (y < 0 || y >= imgH) continue;
                int o = Idx(x, y, stride);
                if (px[o + 3] == 0) continue;
                int r = px[o + 2];
                int g = px[o + 1];
                int b = px[o];
                if (IsPouchColor(r, g, b))
                {
                    px[o] = px[o + 1] = px[o + 2] = px[o + 3] = 0;
                }
            }
        }
    }

    private static void GetFrameBounds(byte[] px, int stride, int imgH, int frameX, int frameW, out int minX, out int minY, out int maxX, out int maxY)
    {
        minX = frameW;
        minY = imgH;
        maxX = -1;
        maxY = -1;
        for (int y = 0; y < imgH; y++)
        {
            for (int x = frameX; x < frameX + frameW; x++)
            {
                if (px[Idx(x, y, stride) + 3] == 0) continue;
                minX = Math.Min(minX, x);
                minY = Math.Min(minY, y);
                maxX = Math.Max(maxX, x);
                maxY = Math.Max(maxY, y);
            }
        }
        if (maxX < minX)
        {
            minX = minY = maxX = maxY = 0;
        }
    }

    private static bool FindPouchRect(byte[] px, int stride, int imgH, int frameX, int frameW, int bMinY, int bMaxY, out int pMinX, out int pMinY, out int pMaxX, out int pMaxY)
    {
        pMinX = frameW;
        pMinY = imgH;
        pMaxX = -1;
        pMaxY = -1;
        int waistTop = bMinY + (int)((bMaxY - bMinY) * 0.34f);
        int waistBot = bMinY + (int)((bMaxY - bMinY) * 0.52f);
        for (int y = waistTop; y <= waistBot; y++)
        {
            for (int x = frameX; x < frameX + frameW; x++)
            {
                if (px[Idx(x, y, stride) + 3] == 0) continue;
                int r = px[Idx(x, y, stride) + 2];
                int g = px[Idx(x, y, stride) + 1];
                int b = px[Idx(x, y, stride)];
                if (!IsPouchColor(r, g, b)) continue;
                pMinX = Math.Min(pMinX, x);
                pMinY = Math.Min(pMinY, y);
                pMaxX = Math.Max(pMaxX, x);
                pMaxY = Math.Max(pMaxY, y);
            }
        }
        return pMaxX >= pMinX;
    }

    public static void UnifyPouches(string path, int frameCount)
    {
        using (var bmp = Load32(path))
        {
            int imgW = bmp.Width;
            int imgH = bmp.Height;
            int frameW = imgW / frameCount;
            int stride;
            byte[] px = ReadPx(bmp, out stride);

            int bMinX, bMinY, bMaxX, bMaxY;
            int pMinX, pMinY, pMaxX, pMaxY;

            GetFrameBounds(px, stride, imgH, 0, frameW, out bMinX, out bMinY, out bMaxX, out bMaxY);
            if (bMaxX < bMinX) return;

            if (!FindPouchRect(px, stride, imgH, 0, frameW, bMinY, bMaxY, out pMinX, out pMinY, out pMaxX, out pMaxY))
                return;

            int refBodyH = bMaxY - bMinY + 1;
            int bandLeft = Math.Max(0, pMinX - 8);
            int bandRight = Math.Min(frameW - 1, pMaxX + 10);
            int bandTop = Math.Max(0, pMinY - 6);
            int bandBottom = Math.Min(imgH - 1, pMaxY + 4);
            int bandW = bandRight - bandLeft + 1;
            int bandH = bandBottom - bandTop + 1;

            byte[] band = new byte[bandW * bandH * 4];
            for (int y = 0; y < bandH; y++)
            {
                for (int x = 0; x < bandW; x++)
                {
                    int src = Idx(bandLeft + x, bandTop + y, stride);
                    int dst = (y * bandW + x) * 4;
                    band[dst] = px[src];
                    band[dst + 1] = px[src + 1];
                    band[dst + 2] = px[src + 2];
                    band[dst + 3] = px[src + 3];
                }
            }

            float relBandX = bandLeft / (float)frameW;
            float relBandY = (bandTop - bMinY) / (float)refBodyH;

            for (int f = 1; f < frameCount; f++)
            {
                int fx = f * frameW;
                GetFrameBounds(px, stride, imgH, fx, frameW, out bMinX, out bMinY, out bMaxX, out bMaxY);
                if (bMaxX < bMinX) continue;

                int bodyH = bMaxY - bMinY + 1;
                ClearPouchPixelsInWaist(px, stride, imgH, fx, frameW, bMinX, bMinY, bMaxX, bMaxY);

                int dstX = fx + (int)Math.Round(relBandX * frameW);
                int dstY = bMinY + (int)Math.Round(relBandY * bodyH);

                for (int y = 0; y < bandH; y++)
                {
                    for (int x = 0; x < bandW; x++)
                    {
                        int tx = dstX + x;
                        int ty = dstY + y;
                        if (tx < fx || tx >= fx + frameW || ty < 0 || ty >= imgH) continue;
                        int o = Idx(tx, ty, stride);
                        px[o] = px[o + 1] = px[o + 2] = px[o + 3] = 0;
                    }
                }

                for (int y = 0; y < bandH; y++)
                {
                    for (int x = 0; x < bandW; x++)
                    {
                        int bo = (y * bandW + x) * 4;
                        if (band[bo + 3] == 0) continue;
                        int tx = dstX + x;
                        int ty = dstY + y;
                        if (tx < fx || tx >= fx + frameW || ty < 0 || ty >= imgH) continue;
                        int o = Idx(tx, ty, stride);
                        px[o] = band[bo];
                        px[o + 1] = band[bo + 1];
                        px[o + 2] = band[bo + 2];
                        px[o + 3] = band[bo + 3];
                    }
                }
            }

            ZeroTransparent(px);
            WritePx(bmp, px);
            bmp.Save(path, ImageFormat.Png);
        }
    }
}
