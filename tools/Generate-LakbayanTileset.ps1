Add-Type -AssemblyName System.Drawing

$tileSize = 32
$columns = 8
$rows = 8
$width = $tileSize * $columns
$height = $tileSize * $rows

$outputPath = Join-Path $PSScriptRoot "..\Assets\Art\Tilesets\LakbayanTileset.png"
$indexPath = Join-Path $PSScriptRoot "..\Assets\Art\Tilesets\LakbayanTileset_Index.md"

function New-Color($r, $g, $b, $a = 255) {
    [System.Drawing.Color]::FromArgb($a, $r, $g, $b)
}

function Set-Pixel($bmp, $x, $y, $color) {
    if ($x -ge 0 -and $x -lt $bmp.Width -and $y -ge 0 -and $y -lt $bmp.Height) {
        $bmp.SetPixel($x, $y, $color)
    }
}

function Fill-Rect($bmp, $x, $y, $w, $h, $color) {
    for ($yy = $y; $yy -lt ($y + $h); $yy++) {
        for ($xx = $x; $xx -lt ($x + $w); $xx++) {
            Set-Pixel $bmp $xx $yy $color
        }
    }
}

function Draw-Rect($bmp, $x, $y, $w, $h, $color) {
    for ($xx = $x; $xx -lt ($x + $w); $xx++) {
        Set-Pixel $bmp $xx $y $color
        Set-Pixel $bmp $xx ($y + $h - 1) $color
    }
    for ($yy = $y; $yy -lt ($y + $h); $yy++) {
        Set-Pixel $bmp $x $yy $color
        Set-Pixel $bmp ($x + $w - 1) $yy $color
    }
}

function Tile-Origin($index) {
    $col = $index % $columns
    $row = [math]::Floor($index / $columns)
    return @{ X = $col * $tileSize; Y = $row * $tileSize }
}

function Pattern-Value($x, $y, $seed, $count) {
    $raw = (($x * 73) -bxor ($y * 151) -bxor ($seed * 199))
    $raw = [math]::Abs($raw)
    return $raw % $count
}

function Paint-Base($bmp, $ox, $oy, $palette, $seed) {
    for ($y = 0; $y -lt $tileSize; $y++) {
        for ($x = 0; $x -lt $tileSize; $x++) {
            $index = Pattern-Value $x $y $seed $palette.Count
            $color = $palette[$index]
            if ($x -lt 10 -and $y -lt 10 -and $palette.Count -ge 2) {
                $color = $palette[0]
            }
            Set-Pixel $bmp ($ox + $x) ($oy + $y) $color
        }
    }
}

function Add-GrassDetails($bmp, $ox, $oy, $seed) {
    $dark = New-Color 40 118 46
    $light = New-Color 105 201 76
    for ($i = 0; $i -lt 22; $i++) {
        $x = ((($i * 7) + ($seed * 5)) % 28) + 2
        $y = ((($i * 11) + ($seed * 3)) % 24) + 4
        Set-Pixel $bmp ($ox + $x) ($oy + $y) $dark
        Set-Pixel $bmp ($ox + $x) ($oy + $y - 1) $light
    }
}

function Add-DirtPebbles($bmp, $ox, $oy, $seed) {
    $pebble = New-Color 110 82 48
    $light = New-Color 170 127 76
    for ($i = 0; $i -lt 16; $i++) {
        $x = ((($i * 9) + $seed) % 28) + 2
        $y = ((($i * 5) + ($seed * 2)) % 26) + 3
        Set-Pixel $bmp ($ox + $x) ($oy + $y) $pebble
        Set-Pixel $bmp ($ox + $x + 1) ($oy + $y) $light
    }
}

function Draw-Flower($bmp, $cx, $cy, $petal, $center) {
    Set-Pixel $bmp ($cx) ($cy - 1) $petal
    Set-Pixel $bmp ($cx - 1) ($cy) $petal
    Set-Pixel $bmp ($cx + 1) ($cy) $petal
    Set-Pixel $bmp ($cx) ($cy + 1) $petal
    Set-Pixel $bmp $cx $cy $center
}

function Draw-Mushroom($bmp, $cx, $cy, $cap, $stem) {
    for ($x = -2; $x -le 2; $x++) {
        Set-Pixel $bmp ($cx + $x) ($cy - 1) $cap
    }
    for ($x = -1; $x -le 1; $x++) {
        Set-Pixel $bmp ($cx + $x) $cy $cap
    }
    Set-Pixel $bmp $cx ($cy + 1) $stem
    Set-Pixel $bmp $cx ($cy + 2) $stem
}

function Draw-Bush($bmp, $ox, $oy, $palette) {
    $shadow = $palette[2]
    $mid = $palette[1]
    $light = $palette[0]
    Fill-Rect $bmp ($ox + 7) ($oy + 15) 18 10 $shadow
    Fill-Rect $bmp ($ox + 6) ($oy + 12) 20 10 $mid
    Fill-Rect $bmp ($ox + 8) ($oy + 10) 16 8 $light
    Fill-Rect $bmp ($ox + 12) ($oy + 7) 8 5 $light
}

function Draw-Rock($bmp, $ox, $oy, $variant) {
    $outline = New-Color 67 78 88
    $shadow = New-Color 92 104 118
    $mid = New-Color 129 142 154
    $light = New-Color 166 178 188
    if ($variant -eq 0) {
        Fill-Rect $bmp ($ox + 9) ($oy + 14) 14 11 $shadow
        Fill-Rect $bmp ($ox + 10) ($oy + 12) 12 10 $mid
        Fill-Rect $bmp ($ox + 12) ($oy + 10) 7 5 $light
        Draw-Rect $bmp ($ox + 9) ($oy + 14) 14 11 $outline
    } else {
        Fill-Rect $bmp ($ox + 6) ($oy + 13) 20 13 $shadow
        Fill-Rect $bmp ($ox + 8) ($oy + 11) 16 10 $mid
        Fill-Rect $bmp ($ox + 11) ($oy + 9) 10 6 $light
        Draw-Rect $bmp ($ox + 6) ($oy + 13) 20 13 $outline
    }
}

function Draw-PathShape($bmp, $ox, $oy, $up, $down, $left, $right) {
    $dirtPalette = @(
        (New-Color 182 141 82),
        (New-Color 160 118 67),
        (New-Color 136 96 53)
    )

    for ($y = 0; $y -lt $tileSize; $y++) {
        for ($x = 0; $x -lt $tileSize; $x++) {
            $paint = $false

            if ($x -ge 10 -and $x -le 21 -and $y -ge 10 -and $y -le 21) { $paint = $true }
            if ($up -and $x -ge 10 -and $x -le 21 -and $y -le 15) { $paint = $true }
            if ($down -and $x -ge 10 -and $x -le 21 -and $y -ge 16) { $paint = $true }
            if ($left -and $y -ge 10 -and $y -le 21 -and $x -le 15) { $paint = $true }
            if ($right -and $y -ge 10 -and $y -le 21 -and $x -ge 16) { $paint = $true }

            if ($paint) {
                $edge = ($x -eq 10 -or $x -eq 21 -or $y -eq 10 -or $y -eq 21)
                $color = if ($edge) { $dirtPalette[2] } else { $dirtPalette[(Pattern-Value $x $y 22 $dirtPalette.Count)] }
                Set-Pixel $bmp ($ox + $x) ($oy + $y) $color
            }
        }
    }
}

function Draw-WaterBase($bmp, $ox, $oy) {
    $palette = @(
        (New-Color 92 196 247),
        (New-Color 52 143 214),
        (New-Color 33 96 173)
    )
    Paint-Base $bmp $ox $oy $palette 31
    $foam = New-Color 202 244 255
    for ($y = 3; $y -lt $tileSize; $y += 7) {
        for ($x = 2; $x -lt $tileSize; $x += 8) {
            Set-Pixel $bmp ($ox + $x) ($oy + $y) $foam
            Set-Pixel $bmp ($ox + $x + 1) ($oy + $y) $foam
        }
    }
}

function Draw-Shore($bmp, $ox, $oy, $landTop, $landBottom, $landLeft, $landRight, $innerNW, $innerNE, $innerSW, $innerSE) {
    $grassPalette = @(
        (New-Color 110 186 79),
        (New-Color 76 151 63),
        (New-Color 52 120 50)
    )
    $shore = New-Color 215 214 167

    for ($y = 0; $y -lt $tileSize; $y++) {
        for ($x = 0; $x -lt $tileSize; $x++) {
            $isLand = $false

            if ($landTop -and $y -lt 10) { $isLand = $true }
            if ($landBottom -and $y -gt 21) { $isLand = $true }
            if ($landLeft -and $x -lt 10) { $isLand = $true }
            if ($landRight -and $x -gt 21) { $isLand = $true }

            if ($innerNW -and $x -lt 16 -and $y -lt 16 -and ($x + $y) -lt 18) { $isLand = $true }
            if ($innerNE -and $x -gt 15 -and $y -lt 16 -and (($tileSize - 1 - $x) + $y) -lt 18) { $isLand = $true }
            if ($innerSW -and $x -lt 16 -and $y -gt 15 -and ($x + ($tileSize - 1 - $y)) -lt 18) { $isLand = $true }
            if ($innerSE -and $x -gt 15 -and $y -gt 15 -and ((($tileSize - 1 - $x) + ($tileSize - 1 - $y))) -lt 18) { $isLand = $true }

            if ($isLand) {
                $color = $grassPalette[(Pattern-Value $x $y 13 $grassPalette.Count)]
                Set-Pixel $bmp ($ox + $x) ($oy + $y) $color
            }
        }
    }

    for ($y = 0; $y -lt $tileSize; $y++) {
        for ($x = 0; $x -lt $tileSize; $x++) {
            $current = $bmp.GetPixel($ox + $x, $oy + $y)
            if ($current.A -eq 0) { continue }
            if ($current.B -lt 160 -or $current.R -gt 120) { continue }

            $touchLand = $false
            foreach ($delta in @(@(-1,0), @(1,0), @(0,-1), @(0,1))) {
                $nx = $x + $delta[0]
                $ny = $y + $delta[1]
                if ($nx -ge 0 -and $nx -lt $tileSize -and $ny -ge 0 -and $ny -lt $tileSize) {
                    $neighbor = $bmp.GetPixel($ox + $nx, $oy + $ny)
                    if ($neighbor.B -lt 160 -or $neighbor.R -gt 120) {
                        $touchLand = $true
                    }
                }
            }
            if ($touchLand) {
                Set-Pixel $bmp ($ox + $x) ($oy + $y) $shore
            }
        }
    }
}

function Draw-Cliff($bmp, $ox, $oy, $variant) {
    $grass = @((New-Color 108 188 77), (New-Color 79 154 65), (New-Color 53 123 50))
    $rock = @((New-Color 164 123 82), (New-Color 130 91 56), (New-Color 102 67 39))
    Paint-Base $bmp $ox $oy $rock 41

    switch ($variant) {
        "top" {
            Fill-Rect $bmp $ox $oy $tileSize 10 $grass[1]
            for ($x = 0; $x -lt $tileSize; $x++) {
                $drop = 8 + (($x * 3) % 4)
                for ($y = 0; $y -lt $drop; $y++) {
                    Set-Pixel $bmp ($ox + $x) ($oy + $y) $grass[(Pattern-Value $x $y 7 $grass.Count)]
                }
            }
        }
        "face" {
            for ($y = 3; $y -lt $tileSize; $y += 6) {
                for ($x = 0; $x -lt $tileSize; $x += 8) {
                    Fill-Rect $bmp ($ox + $x) ($oy + $y) 3 2 $rock[0]
                }
            }
        }
        "left" {
            Fill-Rect $bmp $ox ($oy + 3) 10 29 $rock[2]
            Fill-Rect $bmp ($ox + 10) ($oy + 3) 22 29 $rock[1]
            Fill-Rect $bmp $ox $oy $tileSize 10 $grass[1]
        }
        "right" {
            Fill-Rect $bmp ($ox + 22) ($oy + 3) 10 29 $rock[2]
            Fill-Rect $bmp $ox ($oy + 3) 22 29 $rock[1]
            Fill-Rect $bmp $ox $oy $tileSize 10 $grass[1]
        }
        "corner_nw" { Draw-Cliff $bmp $ox $oy "top"; Fill-Rect $bmp $ox ($oy + 3) 10 29 $rock[2] }
        "corner_ne" { Draw-Cliff $bmp $ox $oy "top"; Fill-Rect $bmp ($ox + 22) ($oy + 3) 10 29 $rock[2] }
        "corner_sw" { Paint-Base $bmp $ox $oy $rock 52; Fill-Rect $bmp $ox $oy 10 29 $rock[2] }
        "corner_se" { Paint-Base $bmp $ox $oy $rock 53; Fill-Rect $bmp ($ox + 22) $oy 10 29 $rock[2] }
    }
}

function Draw-Bridge($bmp, $ox, $oy, $orientation, $part) {
    $dark = New-Color 87 54 25
    $mid = New-Color 126 82 39
    $light = New-Color 174 126 72

    if ($orientation -eq "horizontal") {
        Fill-Rect $bmp ($ox + 4) ($oy + 9) 24 14 $mid
        Fill-Rect $bmp ($ox + 4) ($oy + 9) 2 14 $dark
        Fill-Rect $bmp ($ox + 26) ($oy + 9) 2 14 $dark
        for ($x = 6; $x -lt 26; $x += 4) {
            Fill-Rect $bmp ($ox + $x) ($oy + 10) 2 12 $light
        }
        if ($part -eq "left") { Fill-Rect $bmp ($ox + 2) ($oy + 12) 4 8 $dark }
        if ($part -eq "right") { Fill-Rect $bmp ($ox + 26) ($oy + 12) 4 8 $dark }
    } else {
        Fill-Rect $bmp ($ox + 9) ($oy + 4) 14 24 $mid
        Fill-Rect $bmp ($ox + 9) ($oy + 4) 14 2 $dark
        Fill-Rect $bmp ($ox + 9) ($oy + 26) 14 2 $dark
        for ($y = 6; $y -lt 26; $y += 4) {
            Fill-Rect $bmp ($ox + 10) ($oy + $y) 12 2 $light
        }
        if ($part -eq "top") { Fill-Rect $bmp ($ox + 12) ($oy + 2) 8 4 $dark }
        if ($part -eq "bottom") { Fill-Rect $bmp ($ox + 12) ($oy + 26) 8 4 $dark }
    }
}

function Draw-Tree($bmp, $tileIndex, $part) {
    $origin = Tile-Origin $tileIndex
    $ox = $origin.X
    $oy = $origin.Y
    $outline = New-Color 35 74 29
    $dark = New-Color 49 109 45
    $mid = New-Color 76 153 62
    $light = New-Color 118 195 79
    $trunk = New-Color 126 82 39
    $trunkLight = New-Color 170 121 76

    switch ($part) {
        "tl" {
            Fill-Rect $bmp ($ox + 6) ($oy + 6) 20 18 $dark
            Fill-Rect $bmp ($ox + 9) ($oy + 4) 16 14 $mid
            Fill-Rect $bmp ($ox + 11) ($oy + 2) 11 8 $light
        }
        "tr" {
            Fill-Rect $bmp ($ox + 6) ($oy + 6) 20 18 $dark
            Fill-Rect $bmp ($ox + 7) ($oy + 4) 16 14 $mid
            Fill-Rect $bmp ($ox + 10) ($oy + 2) 11 8 $light
        }
        "bl" {
            Fill-Rect $bmp ($ox + 7) ($oy + 0) 19 16 $dark
            Fill-Rect $bmp ($ox + 9) ($oy + 2) 16 13 $mid
            Fill-Rect $bmp ($ox + 13) ($oy + 12) 6 12 $trunk
            Fill-Rect $bmp ($ox + 15) ($oy + 12) 2 10 $trunkLight
        }
        "br" {
            Fill-Rect $bmp ($ox + 6) ($oy + 0) 19 16 $dark
            Fill-Rect $bmp ($ox + 7) ($oy + 2) 16 13 $mid
        }
    }

    Draw-Rect $bmp ($ox + 7) ($oy + 6) 18 14 $outline
}

function Paint-Tile($bmp, $index, $kind) {
    $origin = Tile-Origin $index
    $ox = $origin.X
    $oy = $origin.Y

    switch ($kind) {
        "grass" {
            Paint-Base $bmp $ox $oy @((New-Color 112 192 80), (New-Color 80 158 64), (New-Color 52 125 49)) 11
            Add-GrassDetails $bmp $ox $oy 7
        }
        "grass_alt" {
            Paint-Base $bmp $ox $oy @((New-Color 118 199 83), (New-Color 86 163 67), (New-Color 56 130 51)) 15
            Add-GrassDetails $bmp $ox $oy 13
        }
        "grass_flower_yellow" {
            Paint-Tile $bmp $index "grass"
            Draw-Flower $bmp ($ox + 10) ($oy + 11) (New-Color 245 222 85) (New-Color 162 111 37)
            Draw-Flower $bmp ($ox + 22) ($oy + 21) (New-Color 245 222 85) (New-Color 162 111 37)
        }
        "grass_flower_pink" {
            Paint-Tile $bmp $index "grass_alt"
            Draw-Flower $bmp ($ox + 12) ($oy + 13) (New-Color 239 112 176) (New-Color 255 236 160)
            Draw-Flower $bmp ($ox + 20) ($oy + 20) (New-Color 239 112 176) (New-Color 255 236 160)
        }
        "grass_mushroom" {
            Paint-Tile $bmp $index "grass"
            Draw-Mushroom $bmp ($ox + 12) ($oy + 18) (New-Color 192 62 52) (New-Color 233 214 172)
            Draw-Mushroom $bmp ($ox + 20) ($oy + 22) (New-Color 192 62 52) (New-Color 233 214 172)
        }
        "bush_small" {
            Paint-Tile $bmp $index "grass"
            Draw-Bush $bmp $ox $oy @((New-Color 132 208 85), (New-Color 72 148 61), (New-Color 44 103 43))
        }
        "rock_small" { Paint-Tile $bmp $index "grass"; Draw-Rock $bmp $ox $oy 0 }
        "rock_large" { Paint-Tile $bmp $index "grass"; Draw-Rock $bmp $ox $oy 1 }
        "dirt" {
            Paint-Base $bmp $ox $oy @((New-Color 178 136 79), (New-Color 152 111 65), (New-Color 124 87 49)) 9
            Add-DirtPebbles $bmp $ox $oy 17
        }
        "path_h" { Paint-Tile $bmp $index "grass"; Draw-PathShape $bmp $ox $oy $false $false $true $true }
        "path_v" { Paint-Tile $bmp $index "grass"; Draw-PathShape $bmp $ox $oy $true $true $false $false }
        "path_ne" { Paint-Tile $bmp $index "grass"; Draw-PathShape $bmp $ox $oy $true $false $false $true }
        "path_nw" { Paint-Tile $bmp $index "grass"; Draw-PathShape $bmp $ox $oy $true $false $true $false }
        "path_se" { Paint-Tile $bmp $index "grass"; Draw-PathShape $bmp $ox $oy $false $true $false $true }
        "path_sw" { Paint-Tile $bmp $index "grass"; Draw-PathShape $bmp $ox $oy $false $true $true $false }
        "path_cross" { Paint-Tile $bmp $index "grass"; Draw-PathShape $bmp $ox $oy $true $true $true $true }
        "path_t_n" { Paint-Tile $bmp $index "grass"; Draw-PathShape $bmp $ox $oy $false $true $true $true }
        "path_t_s" { Paint-Tile $bmp $index "grass"; Draw-PathShape $bmp $ox $oy $true $false $true $true }
        "path_t_e" { Paint-Tile $bmp $index "grass"; Draw-PathShape $bmp $ox $oy $true $true $true $false }
        "path_t_w" { Paint-Tile $bmp $index "grass"; Draw-PathShape $bmp $ox $oy $true $true $false $true }
        "path_end_n" { Paint-Tile $bmp $index "grass"; Draw-PathShape $bmp $ox $oy $true $false $false $false }
        "path_end_s" { Paint-Tile $bmp $index "grass"; Draw-PathShape $bmp $ox $oy $false $true $false $false }
        "path_end_e" { Paint-Tile $bmp $index "grass"; Draw-PathShape $bmp $ox $oy $false $false $false $true }
        "path_end_w" { Paint-Tile $bmp $index "grass"; Draw-PathShape $bmp $ox $oy $false $false $true $false }
        "water_center" { Draw-WaterBase $bmp $ox $oy }
        "water_shore_n" { Draw-WaterBase $bmp $ox $oy; Draw-Shore $bmp $ox $oy $true $false $false $false $false $false $false $false }
        "water_shore_s" { Draw-WaterBase $bmp $ox $oy; Draw-Shore $bmp $ox $oy $false $true $false $false $false $false $false $false }
        "water_shore_w" { Draw-WaterBase $bmp $ox $oy; Draw-Shore $bmp $ox $oy $false $false $true $false $false $false $false $false }
        "water_shore_e" { Draw-WaterBase $bmp $ox $oy; Draw-Shore $bmp $ox $oy $false $false $false $true $false $false $false $false }
        "water_corner_nw" { Draw-WaterBase $bmp $ox $oy; Draw-Shore $bmp $ox $oy $true $false $true $false $false $false $false $false }
        "water_corner_ne" { Draw-WaterBase $bmp $ox $oy; Draw-Shore $bmp $ox $oy $true $false $false $true $false $false $false $false }
        "water_corner_sw" { Draw-WaterBase $bmp $ox $oy; Draw-Shore $bmp $ox $oy $false $true $true $false $false $false $false $false }
        "water_corner_se" { Draw-WaterBase $bmp $ox $oy; Draw-Shore $bmp $ox $oy $false $true $false $true $false $false $false $false }
        "water_inner_nw" { Draw-WaterBase $bmp $ox $oy; Draw-Shore $bmp $ox $oy $false $false $false $false $true $false $false $false }
        "water_inner_ne" { Draw-WaterBase $bmp $ox $oy; Draw-Shore $bmp $ox $oy $false $false $false $false $false $true $false $false }
        "water_inner_sw" { Draw-WaterBase $bmp $ox $oy; Draw-Shore $bmp $ox $oy $false $false $false $false $false $false $true $false }
        "water_inner_se" { Draw-WaterBase $bmp $ox $oy; Draw-Shore $bmp $ox $oy $false $false $false $false $false $false $false $true }
        "bridge_h_left" { Draw-Bridge $bmp $ox $oy "horizontal" "left" }
        "bridge_h_mid" { Draw-Bridge $bmp $ox $oy "horizontal" "mid" }
        "bridge_h_right" { Draw-Bridge $bmp $ox $oy "horizontal" "right" }
        "cliff_top" { Draw-Cliff $bmp $ox $oy "top" }
        "cliff_face" { Draw-Cliff $bmp $ox $oy "face" }
        "cliff_left" { Draw-Cliff $bmp $ox $oy "left" }
        "cliff_right" { Draw-Cliff $bmp $ox $oy "right" }
        "cliff_corner_nw" { Draw-Cliff $bmp $ox $oy "corner_nw" }
        "cliff_corner_ne" { Draw-Cliff $bmp $ox $oy "corner_ne" }
        "cliff_corner_sw" { Draw-Cliff $bmp $ox $oy "corner_sw" }
        "cliff_corner_se" { Draw-Cliff $bmp $ox $oy "corner_se" }
        "tree_tl" { Draw-Tree $bmp $index "tl" }
        "tree_tr" { Draw-Tree $bmp $index "tr" }
        "tree_bl" { Draw-Tree $bmp $index "bl" }
        "tree_br" { Draw-Tree $bmp $index "br" }
        "bush_round" {
            Draw-Bush $bmp $ox $oy @((New-Color 148 219 88), (New-Color 93 171 67), (New-Color 52 118 46))
        }
        "flower_blue" {
            Paint-Tile $bmp $index "grass"
            Draw-Flower $bmp ($ox + 11) ($oy + 12) (New-Color 98 166 255) (New-Color 255 244 184)
            Draw-Flower $bmp ($ox + 19) ($oy + 21) (New-Color 98 166 255) (New-Color 255 244 184)
            Draw-Flower $bmp ($ox + 24) ($oy + 15) (New-Color 98 166 255) (New-Color 255 244 184)
        }
        "flower_red" {
            Paint-Tile $bmp $index "grass_alt"
            Draw-Flower $bmp ($ox + 8) ($oy + 14) (New-Color 222 76 78) (New-Color 255 235 160)
            Draw-Flower $bmp ($ox + 16) ($oy + 22) (New-Color 222 76 78) (New-Color 255 235 160)
            Draw-Flower $bmp ($ox + 23) ($oy + 13) (New-Color 222 76 78) (New-Color 255 235 160)
        }
        "mushroom_cluster" {
            Paint-Tile $bmp $index "grass"
            Draw-Mushroom $bmp ($ox + 10) ($oy + 16) (New-Color 190 64 52) (New-Color 232 213 178)
            Draw-Mushroom $bmp ($ox + 17) ($oy + 20) (New-Color 190 64 52) (New-Color 232 213 178)
            Draw-Mushroom $bmp ($ox + 23) ($oy + 14) (New-Color 190 64 52) (New-Color 232 213 178)
        }
        "grass_clover" {
            Paint-Tile $bmp $index "grass_alt"
            $leaf = New-Color 69 146 54
            foreach ($p in @(@(10,15), @(20,17), @(15,23))) {
                Fill-Rect $bmp ($ox + $p[0]) ($oy + $p[1]) 2 2 $leaf
                Fill-Rect $bmp ($ox + $p[0] - 2) ($oy + $p[1]) 2 2 $leaf
                Fill-Rect $bmp ($ox + $p[0]) ($oy + $p[1] - 2) 2 2 $leaf
                Fill-Rect $bmp ($ox + $p[0] - 2) ($oy + $p[1] - 2) 2 2 $leaf
            }
        }
        "dirt_pebbles" { Paint-Tile $bmp $index "dirt" }
        "bridge_v_top" { Draw-Bridge $bmp $ox $oy "vertical" "top" }
        "bridge_v_mid" { Draw-Bridge $bmp $ox $oy "vertical" "mid" }
        "bridge_v_bottom" { Draw-Bridge $bmp $ox $oy "vertical" "bottom" }
        "rock_mossy" {
            Paint-Tile $bmp $index "grass"
            Draw-Rock $bmp $ox $oy 1
            Fill-Rect $bmp ($ox + 12) ($oy + 11) 6 3 (New-Color 88 145 63)
        }
        "flower_white" {
            Paint-Tile $bmp $index "grass"
            Draw-Flower $bmp ($ox + 12) ($oy + 12) (New-Color 250 250 250) (New-Color 255 215 114)
            Draw-Flower $bmp ($ox + 20) ($oy + 19) (New-Color 250 250 250) (New-Color 255 215 114)
        }
        "sapling" {
            Paint-Tile $bmp $index "grass_alt"
            Fill-Rect $bmp ($ox + 15) ($oy + 15) 2 8 (New-Color 118 78 37)
            Fill-Rect $bmp ($ox + 11) ($oy + 8) 10 8 (New-Color 86 161 63)
            Fill-Rect $bmp ($ox + 13) ($oy + 6) 6 4 (New-Color 128 199 84)
        }
    }
}

$bitmap = New-Object System.Drawing.Bitmap($width, $height, [System.Drawing.Imaging.PixelFormat]::Format32bppArgb)

$tileMap = @(
    "grass","grass_alt","grass_flower_yellow","grass_flower_pink","grass_mushroom","bush_small","rock_small","rock_large",
    "dirt","path_h","path_v","path_ne","path_nw","path_se","path_sw","path_cross",
    "path_t_n","path_t_s","path_t_e","path_t_w","path_end_n","path_end_s","path_end_e","path_end_w",
    "water_center","water_shore_n","water_shore_s","water_shore_w","water_shore_e","water_corner_nw","water_corner_ne","water_corner_sw",
    "water_corner_se","water_inner_nw","water_inner_ne","water_inner_sw","water_inner_se","bridge_h_left","bridge_h_mid","bridge_h_right",
    "cliff_top","cliff_face","cliff_left","cliff_right","cliff_corner_nw","cliff_corner_ne","cliff_corner_sw","cliff_corner_se",
    "tree_tl","tree_tr","tree_bl","tree_br","bush_round","flower_blue","flower_red","mushroom_cluster",
    "grass_clover","dirt_pebbles","bridge_v_top","bridge_v_mid","bridge_v_bottom","rock_mossy","flower_white","sapling"
)

for ($i = 0; $i -lt $tileMap.Count; $i++) {
    Paint-Tile $bitmap $i $tileMap[$i]
}

$bitmap.Save($outputPath, [System.Drawing.Imaging.ImageFormat]::Png)
$bitmap.Dispose()

$indexLines = @(
    "# Lakbayan Tileset Index",
    "",
    "- Tile size: 32x32 pixels",
    "- Sheet size: 256x256 pixels",
    "- Layout: 8 columns x 8 rows",
    "- Import in Unity as Sprite (2D and UI), Filter Mode = Point, Compression = None, Pixels Per Unit = 32",
    "",
    "## Tile Order",
    ""
)

for ($i = 0; $i -lt $tileMap.Count; $i++) {
    $col = $i % $columns
    $row = [math]::Floor($i / $columns)
    $indexLines += "- [$i] row $row, col ${col}: $($tileMap[$i])"
}

Set-Content -Path $indexPath -Value $indexLines -Encoding UTF8
Write-Output "Generated $outputPath"
Write-Output "Generated $indexPath"
