using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;

public static class BuildSoldierAnimationSheets
{
    private struct FrameTransform
    {
        public float ScaleX;
        public float ScaleY;
        public int OffsetX;
        public int OffsetY;
        public float AnchorY;
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

    private static void DrawFrame(Bitmap target, Bitmap source, Rectangle bounds, float fitScale, int slotX, int slotY, int slotW, int slotH, FrameTransform t)
    {
        float scaleX = fitScale * t.ScaleX;
        float scaleY = fitScale * t.ScaleY;
        int drawW = Math.Max(1, (int)Math.Round(bounds.Width * scaleX));
        int drawH = Math.Max(1, (int)Math.Round(bounds.Height * scaleY));

        int anchorX = slotX + slotW / 2;
        int anchorY = slotY + (int)Math.Round(slotH * t.AnchorY);
        int destX = anchorX - drawW / 2 + t.OffsetX;
        int destY = anchorY - (int)Math.Round(drawH * t.AnchorY) + t.OffsetY;

        using (var g = Graphics.FromImage(target))
        {
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;
            g.CompositingQuality = CompositingQuality.HighQuality;
            g.SmoothingMode = SmoothingMode.None;
            g.DrawImage(source, new Rectangle(destX, destY, drawW, drawH), bounds, GraphicsUnit.Pixel);
        }
    }

    private static void BuildSheet(string srcPath, string dstPath, FrameTransform[] frames)
    {
        const int sheetW = 512;
        const int sheetH = 128;
        const int slotW = 128;
        const int slotH = 128;
        const int slotY = 13;
        const int targetW = 106;
        const int targetH = 102;

        using (var source = Load32(srcPath))
        {
            Rectangle? boundsOpt = GetAlphaBounds(source);
            if (boundsOpt == null)
            {
                Console.WriteLine("No opaque pixels in " + srcPath);
                return;
            }

            Rectangle bounds = boundsOpt.Value;
            float fitScale = ComputeFitScale(bounds, targetW, targetH);

            using (var sheet = new Bitmap(sheetW, sheetH, PixelFormat.Format32bppArgb))
            {
                using (var g = Graphics.FromImage(sheet))
                    g.Clear(Color.Transparent);

                for (int i = 0; i < frames.Length; i++)
                    DrawFrame(sheet, source, bounds, fitScale, 11 + i * 128, slotY, slotW, slotH, frames[i]);

                sheet.Save(dstPath, ImageFormat.Png);
            }

            Console.WriteLine(Path.GetFileName(dstPath) + " fitScale=" + fitScale.ToString("0.###") + " bounds=" + bounds.Width + "x" + bounds.Height);
        }

        PlayerSpritePipeline.ProcessBodySheet(dstPath, dstPath, frames.Length);
    }

    public static void Main(string[] args)
    {
        string root = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ".."));
        string srcPath = Path.Combine(root, "Assets", "Sprites", "Player", "soldier_topdown_strict.png");
        string idlePath = Path.Combine(root, "Assets", "Sprites", "Player", "soldier_topdown_idle_empty.png");
        string walkPath = Path.Combine(root, "Assets", "Sprites", "Player", "soldier_topdown_walk_empty.png");

        if (!File.Exists(srcPath))
        {
            Console.WriteLine("Missing source: " + srcPath);
            return;
        }

        var idleFrames = new[]
        {
            new FrameTransform { ScaleX = 1.000f, ScaleY = 1.000f, OffsetX = 0, OffsetY = 0, AnchorY = 0.50f },
            new FrameTransform { ScaleX = 1.008f, ScaleY = 1.018f, OffsetX = 0, OffsetY = -1, AnchorY = 0.50f },
            new FrameTransform { ScaleX = 1.012f, ScaleY = 1.028f, OffsetX = 0, OffsetY = -2, AnchorY = 0.50f },
            new FrameTransform { ScaleX = 1.006f, ScaleY = 1.012f, OffsetX = 0, OffsetY = -1, AnchorY = 0.50f },
        };

        var walkFrames = new[]
        {
            new FrameTransform { ScaleX = 1.012f, ScaleY = 0.985f, OffsetX = -3, OffsetY = 1, AnchorY = 0.54f },
            new FrameTransform { ScaleX = 0.992f, ScaleY = 1.020f, OffsetX = 0, OffsetY = -2, AnchorY = 0.54f },
            new FrameTransform { ScaleX = 1.012f, ScaleY = 0.985f, OffsetX = 3, OffsetY = 1, AnchorY = 0.54f },
            new FrameTransform { ScaleX = 0.996f, ScaleY = 1.012f, OffsetX = 0, OffsetY = -1, AnchorY = 0.54f },
        };

        BuildSheet(srcPath, idlePath, idleFrames);
        BuildSheet(srcPath, walkPath, walkFrames);

        Console.WriteLine("Built idle sheet: " + idlePath);
        Console.WriteLine("Built walk sheet: " + walkPath);
    }
}
