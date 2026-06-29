using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;

public static class BuildCharacterIdleSheet
{
    private static Rectangle? GetAlphaBounds(Bitmap bmp)
    {
        int minX = bmp.Width, minY = bmp.Height, maxX = -1, maxY = -1;
        for (int y = 0; y < bmp.Height; y++)
        {
            for (int x = 0; x < bmp.Width; x++)
            {
                Color c = bmp.GetPixel(x, y);
                if (c.A < 16) continue;
                if (c.R < 24 && c.G < 24 && c.B < 24) continue;
                minX = Math.Min(minX, x);
                minY = Math.Min(minY, y);
                maxX = Math.Max(maxX, x);
                maxY = Math.Max(maxY, y);
            }
        }

        if (maxX < minX) return null;
        return Rectangle.FromLTRB(minX, minY, maxX + 1, maxY + 1);
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

    private static float ComputeFitScale(Rectangle bounds, int targetW, int targetH)
    {
        float sx = targetW / (float)bounds.Width;
        float sy = targetH / (float)bounds.Height;
        return Math.Min(sx, sy);
    }

    private static void DrawFittedFrame(Bitmap target, Bitmap source, Rectangle srcRect, int slotX, int slotY, int slotW, int slotH)
    {
        using (var frame = new Bitmap(srcRect.Width, srcRect.Height, PixelFormat.Format32bppArgb))
        {
            using (var g = Graphics.FromImage(frame))
            {
                g.Clear(Color.Transparent);
                g.DrawImage(source, new Rectangle(0, 0, srcRect.Width, srcRect.Height), srcRect, GraphicsUnit.Pixel);
            }

            Rectangle? boundsOpt = GetAlphaBounds(frame);
            if (boundsOpt == null) return;

            Rectangle bounds = boundsOpt.Value;
            float fitScale = ComputeFitScale(bounds, 106, 102);
            int drawW = Math.Max(1, (int)Math.Round(bounds.Width * fitScale));
            int drawH = Math.Max(1, (int)Math.Round(bounds.Height * fitScale));
            int destX = slotX + (slotW - drawW) / 2;
            int destY = slotY + (slotH - drawH) / 2;

            using (var g = Graphics.FromImage(target))
            {
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                g.CompositingQuality = CompositingQuality.HighQuality;
                g.SmoothingMode = SmoothingMode.None;
                g.DrawImage(frame, new Rectangle(destX, destY, drawW, drawH), bounds, GraphicsUnit.Pixel);
            }
        }
    }

    public static void Main(string[] args)
    {
        string root = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ".."));
        string srcPath = args.Length > 0
            ? args[0]
            : Path.Combine(root, "Assets", "Sprites", "Player", "medic_topdown_idle_source.png");
        string dstPath = args.Length > 1
            ? args[1]
            : Path.Combine(root, "Assets", "Sprites", "Player", "medic_topdown_idle_empty.png");

        if (!File.Exists(srcPath))
        {
            Console.WriteLine("Missing source: " + srcPath);
            return;
        }

        const int frameCount = 4;
        const int sheetW = 512;
        const int sheetH = 128;

        using (var source = Load32(srcPath))
        {
            int frameW = source.Width / frameCount;
            int frameH = source.Height;

            using (var sheet = new Bitmap(sheetW, sheetH, PixelFormat.Format32bppArgb))
            {
                using (var g = Graphics.FromImage(sheet))
                    g.Clear(Color.Transparent);

                for (int i = 0; i < frameCount; i++)
                {
                    Rectangle srcRect = new Rectangle(i * frameW, 0, frameW, frameH);
                    DrawFittedFrame(sheet, source, srcRect, 11 + i * 128, 13, 128, 128);
                }

                sheet.Save(dstPath, ImageFormat.Png);
            }
        }

        PlayerSpritePipeline.ProcessBodySheet(dstPath, dstPath, frameCount);
        Console.WriteLine("Built idle sheet: " + dstPath);
    }
}
