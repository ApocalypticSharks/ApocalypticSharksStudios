param(
    [string]$SpritesDir = (Join-Path $PSScriptRoot "..\Assets\Sprites\Buildings"),
    [string]$ResourcesDir = (Join-Path $PSScriptRoot "..\Assets\Resources\NotSoWild\BuildingSprites"),
    [string]$SourceDir = (Join-Path $PSScriptRoot "BuildingSpriteSources")
)

Add-Type -AssemblyName System.Drawing
$fmt = [System.Drawing.Imaging.PixelFormat]::Format32bppArgb
$magenta = [System.Drawing.Color]::FromArgb(255, 255, 0, 255)

function Get-ContentBounds([System.Drawing.Bitmap]$bmp) {
    $w = $bmp.Width
    $h = $bmp.Height
    $minX = $w; $minY = $h; $maxX = 0; $maxY = 0
    for ($y = 0; $y -lt $h; $y++) {
        for ($x = 0; $x -lt $w; $x++) {
            if ($bmp.GetPixel($x, $y).A -le 16) { continue }
            if ($x -lt $minX) { $minX = $x }
            if ($y -lt $minY) { $minY = $y }
            if ($x -gt $maxX) { $maxX = $x }
            if ($y -gt $maxY) { $maxY = $y }
        }
    }
    return @($minX, $minY, $maxX, $maxY)
}

function Remove-Magenta([System.Drawing.Bitmap]$bmp) {
    $bmp.MakeTransparent($magenta)
    for ($y = 0; $y -lt $bmp.Height; $y++) {
        for ($x = 0; $x -lt $bmp.Width; $x++) {
            $c = $bmp.GetPixel($x, $y)
            if ($c.A -le 16) { continue }
            if ($c.R -gt 150 -and $c.B -gt 150 -and $c.G -lt 130) {
                $bmp.SetPixel($x, $y, [System.Drawing.Color]::Transparent)
            }
        }
    }
}

function Process-BuildingSprite([string]$srcPath, [string]$dstPath, [int]$canvasW, [int]$canvasH, [int]$margin) {
    $src = [System.Drawing.Bitmap]::FromFile($srcPath)
    Remove-Magenta $src

    $minX, $minY, $maxX, $maxY = Get-ContentBounds $src
    if ($maxX -lt $minX) { throw "No visible pixels in $srcPath" }

    $cropW = $maxX - $minX + 1
    $cropH = $maxY - $minY + 1
    $crop = New-Object System.Drawing.Bitmap -ArgumentList $cropW, $cropH, $fmt
    $cg = [System.Drawing.Graphics]::FromImage($crop)
    $cg.DrawImage($src, 0, 0, (New-Object System.Drawing.Rectangle $minX, $minY, $cropW, $cropH), [System.Drawing.GraphicsUnit]::Pixel)
    $cg.Dispose()
    $src.Dispose()

    $out = New-Object System.Drawing.Bitmap -ArgumentList $canvasW, $canvasH, $fmt
    $g = [System.Drawing.Graphics]::FromImage($out)
    $g.Clear([System.Drawing.Color]::Transparent)
    $g.InterpolationMode = [System.Drawing.Drawing2D.InterpolationMode]::HighQualityBicubic

    $drawW = $canvasW - 4
    $drawH = $canvasH - $margin
    $offsetX = 2
    $offsetY = $canvasH - $margin - $drawH
    $g.DrawImage($crop, $offsetX, $offsetY, $drawW, $drawH)
    $g.Dispose()
    $crop.Dispose()

    if (Test-Path $dstPath) { Remove-Item $dstPath -Force }
    $out.Save($dstPath, [System.Drawing.Imaging.ImageFormat]::Png)
    $out.Dispose()
    return "${drawW}x${drawH} on ${canvasW}x${canvasH}"
}

function Update-MetaImportSettings([string]$metaPath) {
    if (-not (Test-Path $metaPath)) { return }
    $text = Get-Content $metaPath -Raw
    if ($text -notmatch 'spriteMeshType: 0') {
        $text = $text -replace 'spriteMeshType: 1', 'spriteMeshType: 0'
    }
    $text = $text -replace 'filterMode: 0', 'filterMode: 0'
    $text = $text -replace 'spritePixelsToUnits: 96', 'spritePixelsToUnits: 144'
    $text = $text -replace 'spritePivot: \{x: 0\.5, y: 0\.5\}', 'spritePivot: {x: 0.5, y: 0}'
    $text = $text -replace 'spriteGenerateFallbackPhysicsShape: 1', 'spriteGenerateFallbackPhysicsShape: 0'
    Set-Content -Path $metaPath -Value $text -NoNewline
}

$projectRoot = (Resolve-Path (Join-Path $PSScriptRoot "..")).Path
$SpritesDir = Join-Path $projectRoot "Assets\Sprites\Buildings"
$ResourcesDir = Join-Path $projectRoot "Assets\Resources\NotSoWild\BuildingSprites"
$SourceDir = if (Test-Path $SourceDir) { (Resolve-Path $SourceDir).Path } else { $SourceDir }

Write-Output "Preparing building sprites..."
$generalSource = Join-Path $SourceDir 'general_store_source.png'
$saloonSource = Join-Path $SourceDir 'saloon_source.png'
$sheriffSource = Join-Path $SourceDir 'sheriff_office_source.png'
if ((Test-Path $generalSource) -and (Test-Path $saloonSource) -and (Test-Path $sheriffSource))
{
    Write-Output "Processing source sprites from $SourceDir"
    Write-Output "general_store: $(Process-BuildingSprite $generalSource (Join-Path $SpritesDir 'general_store.png') 288 288 8)"
    Write-Output "saloon: $(Process-BuildingSprite $saloonSource (Join-Path $SpritesDir 'saloon.png') 288 288 8)"
    Write-Output "sheriff_office: $(Process-BuildingSprite $sheriffSource (Join-Path $SpritesDir 'sheriff_office.png') 288 432 10)"
}
else
{
    Write-Output "No source sprites in $SourceDir; keeping existing Assets/Sprites/Buildings PNGs."
}

New-Item -ItemType Directory -Force -Path $ResourcesDir | Out-Null
Copy-Item (Join-Path $SpritesDir 'general_store.png') (Join-Path $ResourcesDir 'general_store.png') -Force
Copy-Item (Join-Path $SpritesDir 'saloon.png') (Join-Path $ResourcesDir 'saloon.png') -Force
Copy-Item (Join-Path $SpritesDir 'sheriff_office.png') (Join-Path $ResourcesDir 'sheriff_office.png') -Force

foreach ($meta in @(
    (Join-Path $SpritesDir 'general_store.png.meta'),
    (Join-Path $SpritesDir 'saloon.png.meta'),
    (Join-Path $SpritesDir 'sheriff_office.png.meta'),
    (Join-Path $ResourcesDir 'general_store.png.meta'),
    (Join-Path $ResourcesDir 'saloon.png.meta'),
    (Join-Path $ResourcesDir 'sheriff_office.png.meta')
)) { Update-MetaImportSettings $meta }

Write-Output "Done."
