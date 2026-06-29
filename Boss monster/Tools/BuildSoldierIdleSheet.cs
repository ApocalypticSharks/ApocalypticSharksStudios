using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;

public static class BuildSoldierIdleSheet
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

    private static void DrawFrame(Bitmap target, Bitmap source, int slotX, int slotY, int slotW, int slotH, float scaleX, float scaleY, int offsetY)
    {
        Rectangle? bounds = GetAlphaBounds(source);
        if (bounds == null) return;

        int drawW = Math.Max(1, (int)Math.Round(bounds.Value.Width * scaleX));
        int drawH = Math.Max(1, (int)Math.Round(bounds.Value.Height * scaleY));
        int destX = slotX + (slotW - drawW) / 2;
        int destY = slotY + (slotH - drawH) / 2 + offsetY;

        using (var g = Graphics.FromImage(target))
        {
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;
            g.CompositingQuality = CompositingQuality.HighQuality;
            g.SmoothingMode = SmoothingMode.None;
            g.DrawImage(source, new Rectangle(destX, destY, drawW, drawH), bounds.Value, GraphicsUnit.Pixel);
        }
    }

    public static void Main(string[] args)
    {
        string root = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ".."));
        string srcPath = Path.Combine(root, "Assets", "Sprites", "Player", "soldier_topdown_idle_source.png");
        if (!File.Exists(srcPath))
            srcPath = Path.Combine(root, "Assets", "Sprites", "Player", "soldier_topdown_idle_empty.png");

        string dstPath = Path.Combine(root, "Assets", "Sprites", "Player", "soldier_topdown_idle_empty.png");

        using (var sourceSheet = Load32(srcPath))
        {
            int frameW = sourceSheet.Width / 4;
            int frameH = sourceSheet.Height;

            using (var baseFrame = new Bitmap(frameW, frameH, PixelFormat.Format32bppArgb))
            {
                using (var g = Graphics.FromImage(baseFrame))
                {
                    g.Clear(Color.Transparent);
                    g.DrawImage(sourceSheet, new Rectangle(0, 0, frameW, frameH), new Rectangle(0, 0, frameW, frameH), GraphicsUnit.Pixel);
                }

                var scales = new[]
                {
                    new { ScaleX = 1.000f, ScaleY = 1.000f, OffsetY = 0 },
                    new { ScaleX = 1.008f, ScaleY = 1.018f, OffsetY = -1 },
                    new { ScaleX = 1.012f, ScaleY = 1.028f, OffsetY = -2 },
                    new { ScaleX = 1.006f, ScaleY = 1.012f, OffsetY = -1 },
                };

                using (var sheet = new Bitmap(512, 128, PixelFormat.Format32bppArgb))
                {
                    using (var g = Graphics.FromImage(sheet))
                        g.Clear(Color.Transparent);

                    for (int i = 0; i < scales.Length; i++)
                    {
                        var s = scales[i];
                        DrawFrame(sheet, baseFrame, 11 + i * 128, 13, 128, 128, s.ScaleX, s.ScaleY, s.OffsetY);
                    }

                    sheet.Save(dstPath, ImageFormat.Png);
                }
            }
        }

        PlayerSpritePipeline.ProcessBodySheet(dstPath, dstPath, 4);
        Console.WriteLine("Built idle sheet: " + dstPath);
    }
}
