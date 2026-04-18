$ErrorActionPreference = "Stop"
$base = Join-Path $PSScriptRoot "..\Assets\Scripts\ScriptableObjects\Cards" | Resolve-Path
$effects = @(
    @{ Folder = "Strike";       Type = 4; Tag = "Strike";       Desc = "On win: deal damage to the opponent equal to this card value (shield absorbs first)." },
    @{ Folder = "Shield";      Type = 5; Tag = "Shield";       Desc = "On win: gain shield equal to this card value." },
    @{ Folder = "Heal";        Type = 6; Tag = "Heal";         Desc = "On win: heal yourself for this card value." },
    @{ Folder = "MagicStrike"; Type = 7; Tag = "MagicStrike";  Desc = "On win: deal damage to the opponent equal to this card value, ignoring shield." },
    @{ Folder = "Poison";      Type = 8; Tag = "Poison";       Desc = "On win: apply poison stacks equal to this card's value. At round start, take damage equal to current stacks, then stacks decrease by 1." }
)

function Get-ReadableName([string]$internal) {
    foreach ($s in @("Hearts","Diamonds","Clubs","Spades")) {
        if ($internal.EndsWith($s)) {
            $rank = $internal.Substring(0, $internal.Length - $s.Length)
            return "$rank of $s"
        }
    }
    return $internal
}

function New-GuidHex() {
    return (-join ([guid]::NewGuid().ToString("N").ToCharArray()))
}

# Clear generated cards in subfolders
foreach ($e in $effects) {
    $d = Join-Path $base $e.Folder
    Get-ChildItem $d -Filter "*.asset" | ForEach-Object {
        Remove-Item $_.FullName -Force
        $m = $_.FullName + ".meta"
        if (Test-Path $m) { Remove-Item $m -Force }
    }
}

$skip = @("JockerRed.asset", "JockerBlack.asset")
$sources = Get-ChildItem $base -File -Filter "*.asset" | Where-Object { $skip -notcontains $_.Name } | Sort-Object Name

foreach ($src in $sources) {
    $content = Get-Content $src.FullName -Raw -Encoding UTF8
    if ($content -notmatch '(?m)^  m_Name: (.+)$') { continue }
    $origInternal = $Matches[1].Trim()

    foreach ($e in $effects) {
        $internal = "$($e.Folder)_$origInternal"
        $readable = Get-ReadableName $origInternal
        $cardName = "$($e.Tag) - $readable"

        $winBlock = @"
  onWinEffects:
  - type: $($e.Type)
    value: 0
    description: $($e.Tag)
"@

        $n = $content
        $n = [regex]::Replace($n, '(?m)^  m_Name: .+$', "  m_Name: $internal", 1)
        $n = [regex]::Replace($n, '(?m)^  cardName: .+$', "  cardName: $cardName", 1)
        $descEsc = $e.Desc -replace '"', '\"'
        $n = [regex]::Replace($n, '(?m)^  description: .+$', "  description: `"$descEsc`"", 1)
        $n = [regex]::Replace($n, '(?m)^  onWinEffects: \[\]\s*$', $winBlock, 1)

        if ($e.Folder -in "MagicStrike","Poison") {
            $n = [regex]::Replace($n, '(?m)^  rarity: \d+$', "  rarity: 2", 1)
            $n = [regex]::Replace($n, '(?m)^  shopCost: \d+$', "  shopCost: 20", 1)
        } else {
            $n = [regex]::Replace($n, '(?m)^  rarity: \d+$', "  rarity: 1", 1)
            $n = [regex]::Replace($n, '(?m)^  shopCost: \d+$', "  shopCost: 12", 1)
        }

        $outDir = Join-Path $base $e.Folder
        $outPath = Join-Path $outDir "$internal.asset"
        [IO.File]::WriteAllText($outPath, $n, [Text.UTF8Encoding]::new($false))

        $guid = New-GuidHex
        $meta = @"
fileFormatVersion: 2
guid: $guid
NativeFormatImporter:
  externalObjects: {}
  mainObjectFileID: 11400000
  userData: 
  assetBundleName: 
  assetBundleVariant: 
"@
        [IO.File]::WriteAllText("$outPath.meta", $meta, [Text.UTF8Encoding]::new($false))
    }
}

Write-Host "Done: $($sources.Count) base x $($effects.Count) effect folders = $($sources.Count * $effects.Count) files."
