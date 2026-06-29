param(
    [Parameter(Mandatory = $true)][string]$SrcDir,
    [Parameter(Mandatory = $true)][string]$DstDir
)

Add-Type -AssemblyName System.Drawing
$fmt = [System.Drawing.Imaging.PixelFormat]::Format32bppArgb

function Test-CheckerboardColor([int]$r, [int]$g, [int]$b) {
    $max = [Math]::Max($r, [Math]::Max($g, $b))
    $min = [Math]::Min($r, [Math]::Min($g, $b))
    return (($r + $g + $b) / 3.0 -ge 224) -and (($max - $min) -le 16)
}

function Test-BlackBackground([int]$r, [int]$g, [int]$b) {
    return ($r -le 40 -and $g -le 40 -and $b -le 40)
}

function Test-BackgroundPixel([int]$r, [int]$g, [int]$b) {
    return (Test-CheckerboardColor $r $g $b) -or (Test-BlackBackground $r $g $b)
}

function Test-StragglerBackground([int]$r, [int]$g, [int]$b) {
    $max = [Math]::Max($r, [Math]::Max($g, $b))
    $min = [Math]::Min($r, [Math]::Min($g, $b))
    return (($r + $g + $b) / 3.0 -ge 208) -and (($max - $min) -le 24)
}

function Remove-Background([System.Drawing.Bitmap]$bmp) {
    $w = $bmp.Width
    $h = $bmp.Height
    $rect = New-Object System.Drawing.Rectangle 0, 0, $w, $h
    $data = $bmp.LockBits($rect, [System.Drawing.Imaging.ImageLockMode]::ReadWrite, $fmt)
    $bytes = New-Object byte[] ($w * $h * 4)
    [System.Runtime.InteropServices.Marshal]::Copy($data.Scan0, $bytes, 0, $bytes.Length)

    $isBg = New-Object bool[] ($w * $h)
    $queue = New-Object System.Collections.Generic.Queue[int]

    function Try-Enqueue([int]$x, [int]$y) {
        if ($x -lt 0 -or $y -lt 0 -or $x -ge $w -or $y -ge $h) { return }
        $idx = $y * $w + $x
        if ($isBg[$idx]) { return }
        $i = $idx * 4
        if (Test-BackgroundPixel $bytes[$i + 2] $bytes[$i + 1] $bytes[$i]) {
            $isBg[$idx] = $true
            $queue.Enqueue($idx) | Out-Null
        }
    }

    for ($x = 0; $x -lt $w; $x++) {
        Try-Enqueue $x 0
        Try-Enqueue $x ($h - 1)
    }

    for ($y = 0; $y -lt $h; $y++) {
        Try-Enqueue 0 $y
        Try-Enqueue ($w - 1) $y
    }

    while ($queue.Count -gt 0) {
        $idx = $queue.Dequeue()
        $x = $idx % $w
        $y = [int]($idx / $w)
        Try-Enqueue ($x - 1) $y
        Try-Enqueue ($x + 1) $y
        Try-Enqueue $x ($y - 1)
        Try-Enqueue $x ($y + 1)
    }

    for ($idx = 0; $idx -lt ($w * $h); $idx++) {
        if ($isBg[$idx]) { continue }
        $i = $idx * 4
        if (Test-StragglerBackground $bytes[$i + 2] $bytes[$i + 1] $bytes[$i]) {
            $isBg[$idx] = $true
        }
    }

    for ($idx = 0; $idx -lt ($w * $h); $idx++) {
        $i = $idx * 4
        if ($isBg[$idx]) {
            $bytes[$i] = 0
            $bytes[$i + 1] = 0
            $bytes[$i + 2] = 0
            $bytes[$i + 3] = 0
        }
        else {
            $bytes[$i + 3] = 255
        }
    }

    [System.Runtime.InteropServices.Marshal]::Copy($bytes, 0, $data.Scan0, $bytes.Length)
    $bmp.UnlockBits($data)
}

function Fit-Sprite([string]$srcPath, [string]$dstPath, [int]$canvasW, [int]$canvasH, [int]$margin) {
    $src = [System.Drawing.Bitmap]::FromFile($srcPath)
    $out = New-Object System.Drawing.Bitmap $canvasW, $canvasH, $fmt
    $g = [System.Drawing.Graphics]::FromImage($out)
    $g.Clear([System.Drawing.Color]::FromArgb(0, 0, 0, 0))
    $g.InterpolationMode = [System.Drawing.Drawing2D.InterpolationMode]::NearestNeighbor
    $g.PixelOffsetMode = [System.Drawing.Drawing2D.PixelOffsetMode]::Half

    $maxW = $canvasW - ($margin * 2)
    $maxH = $canvasH - ($margin * 2)
    $scale = [Math]::Min($maxW / [double]$src.Width, $maxH / [double]$src.Height)
    $drawW = [int][Math]::Round($src.Width * $scale)
    $drawH = [int][Math]::Round($src.Height * $scale)
    $offsetX = [int][Math]::Floor(($canvasW - $drawW) / 2)
    $offsetY = [int][Math]::Floor(($canvasH - $drawH) / 2)

    $g.DrawImage($src, $offsetX, $offsetY, $drawW, $drawH)
    $g.Dispose()
    $src.Dispose()

    Remove-Background $out
    $out.Save($dstPath, [System.Drawing.Imaging.ImageFormat]::Png)
    $out.Dispose()

    return "${drawW}x${drawH} at (${offsetX},${offsetY})"
}

Write-Output "general_store: $(Fit-Sprite (Join-Path $SrcDir 'general_store_front34.png') (Join-Path $DstDir 'general_store.png') 192 192 8)"
Write-Output "saloon: $(Fit-Sprite (Join-Path $SrcDir 'saloon_front34.png') (Join-Path $DstDir 'saloon.png') 192 192 8)"
Write-Output "sheriff: $(Fit-Sprite (Join-Path $SrcDir 'sheriff_office_front34.png') (Join-Path $DstDir 'sheriff_office.png') 192 288 8)"
