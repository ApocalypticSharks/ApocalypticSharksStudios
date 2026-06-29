Add-Type -AssemblyName System.Drawing

function Get-AlphaBounds($bitmap) {
    $minX = $bitmap.Width
    $minY = $bitmap.Height
    $maxX = -1
    $maxY = -1

    for ($y = 0; $y -lt $bitmap.Height; $y++) {
        for ($x = 0; $x -lt $bitmap.Width; $x++) {
            $c = $bitmap.GetPixel($x, $y)
            if ($c.A -lt 16) { continue }
            if ($c.R -lt 24 -and $c.G -lt 24 -and $c.B -lt 24) { continue }
            if ($minX -gt $x) { $minX = $x }
            if ($minY -gt $y) { $minY = $y }
            if ($maxX -lt $x) { $maxX = $x }
            if ($maxY -lt $y) { $maxY = $y }
        }
    }

    if ($maxX -lt $minX) { return $null }
    return [PSCustomObject]@{
        X = $minX
        Y = $minY
        Width = ($maxX - $minX + 1)
        Height = ($maxY - $minY + 1)
    }
}

function New-TransparentBitmap($width, $height) {
    $bmp = New-Object System.Drawing.Bitmap($width, $height, [System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
    $g = [System.Drawing.Graphics]::FromImage($bmp)
    $g.Clear([System.Drawing.Color]::FromArgb(0, 0, 0, 0))
    $g.Dispose()
    return $bmp
}

function Draw-ScaledFrame($target, $source, $slotX, $slotY, $slotW, $slotH, $scaleX, $scaleY, $offsetY) {
    $bounds = Get-AlphaBounds $source
    if ($null -eq $bounds) { return }

    $drawW = [int][Math]::Round($bounds.Width * $scaleX)
    $drawH = [int][Math]::Round($bounds.Height * $scaleY)
    $destX = $slotX + [int][Math]::Round(($slotW - $drawW) / 2.0)
    $destY = $slotY + [int][Math]::Round(($slotH - $drawH) / 2.0) + $offsetY

    $g = [System.Drawing.Graphics]::FromImage($target)
    $g.InterpolationMode = [System.Drawing.Drawing2D.InterpolationMode]::HighQualityBicubic
    $g.PixelOffsetMode = [System.Drawing.Drawing2D.PixelOffsetMode]::HighQuality
    $g.CompositingQuality = [System.Drawing.Drawing2D.CompositingQuality]::HighQuality
    $g.SmoothingMode = [System.Drawing.Drawing2D.SmoothingMode]::None

    $srcRect = New-Object System.Drawing.Rectangle $bounds.X, $bounds.Y, $bounds.Width, $bounds.Height
    $dstRect = New-Object System.Drawing.Rectangle $destX, $destY, $drawW, $drawH
    $g.DrawImage($source, $dstRect, $srcRect, [System.Drawing.GraphicsUnit]::Pixel)
    $g.Dispose()
}

$srcPath = Join-Path $PSScriptRoot "..\Assets\Sprites\Player\soldier_topdown_idle_empty.png"
$dstPath = Join-Path $PSScriptRoot "..\Assets\Sprites\Player\soldier_topdown_idle_empty.png"
$tmpPath = Join-Path $PSScriptRoot "..\Assets\Sprites\Player\soldier_topdown_idle_empty_build.png"

$sourceSheet = [System.Drawing.Image]::FromFile($srcPath)
$frameW = [int]($sourceSheet.Width / 4)
$frameH = $sourceSheet.Height

$baseFrame = New-TransparentBitmap($frameW, $frameH)
$gBase = [System.Drawing.Graphics]::FromImage($baseFrame)
$gBase.DrawImage($sourceSheet, 0, 0, (New-Object System.Drawing.Rectangle 0, 0, $frameW, $frameH), [System.Drawing.GraphicsUnit]::Pixel)
$gBase.Dispose()
$sourceSheet.Dispose()

$sheetW = 512
$sheetH = 128
$slotW = 128
$slotH = 128
$slotY = 13

$frames = @(
    @{ ScaleX = 1.000; ScaleY = 1.000; OffsetY = 0 },
    @{ ScaleX = 1.008; ScaleY = 1.018; OffsetY = -1 },
    @{ ScaleX = 1.012; ScaleY = 1.028; OffsetY = -2 },
    @{ ScaleX = 1.006; ScaleY = 1.012; OffsetY = -1 }
)

$sheet = New-TransparentBitmap($sheetW, $sheetH)
for ($i = 0; $i -lt $frames.Count; $i++) {
    $slotX = 11 + ($i * 128)
    $f = $frames[$i]
    Draw-ScaledFrame $sheet $baseFrame $slotX $slotY $slotW $slotH $f.ScaleX $f.ScaleY $f.OffsetY
}

$baseFrame.Dispose()
$sheet.Save($tmpPath, [System.Drawing.Imaging.ImageFormat]::Png)
$sheet.Dispose()

Move-Item -Path $tmpPath -Destination $dstPath -Force
Write-Output "Built idle sheet: $dstPath (512x128, 4 frames)"
