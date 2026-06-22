# Fix double-encoded UTF-8 strings in Designer file using byte-level replacement.
# Each garbled string was created by reading UTF-8 bytes as CP1252 and re-encoding as UTF-8.
# We reverse this by replacing the garbled byte sequences with the original UTF-8 bytes.

$file = "c:\Users\EMB7866\Desktop\PC7866\Views\AutomaticTestPanel.Designer.cs"

function Replace-Bytes {
    param([byte[]]$data, [byte[]]$find, [byte[]]$replace)
    $result = New-Object System.Collections.Generic.List[byte]
    $i = 0
    while ($i -lt $data.Length) {
        $matched = $false
        if ($i + $find.Length -le $data.Length) {
            $matched = $true
            for ($j = 0; $j -lt $find.Length; $j++) {
                if ($data[$i + $j] -ne $find[$j]) { $matched = $false; break }
            }
        }
        if ($matched) {
            $result.AddRange([byte[]]$replace)
            $i += $find.Length
        } else {
            $result.Add($data[$i])
            $i++
        }
    }
    return ,$result.ToArray()
}

$bytes = [System.IO.File]::ReadAllBytes($file)
$orig  = $bytes.Length

# ---- Replacements: [garbled file bytes] -> [correct UTF-8 bytes] ----
#
# o-acute (U+00F3): UTF-8 C3 B3 was read as CP1252 -> Ã (C3->C3 83) + ³ (B3->C2 B3)
$bytes = Replace-Bytes $bytes @(0xC3,0x83,0xC2,0xB3) @(0xC3,0xB3)

# Omega (U+03A9): UTF-8 CE A9 -> Î (CE->C3 8E) + (c) (A9->C2 A9)
$bytes = Replace-Bytes $bytes @(0xC3,0x8E,0xC2,0xA9) @(0xCE,0xA9)

# plus-minus (U+00B1): UTF-8 C2 B1 -> A (C2->C3 82) + ± (B1->C2 B1)
$bytes = Replace-Bytes $bytes @(0xC3,0x82,0xC2,0xB1) @(0xC2,0xB1)

# refresh emoji U+1F504 (F0 9F 94 84):
#   F0->C3 B0  9F(CP1252 Ÿ)->C5 B8  94(CP1252 ")->E2 80 9D  84(CP1252 „)->E2 80 9E
$bytes = Replace-Bytes $bytes @(0xC3,0xB0,0xC5,0xB8,0xE2,0x80,0x9D,0xE2,0x80,0x9E) @(0xF0,0x9F,0x94,0x84)

# right-pointing triangle U+25B6 (E2 96 B6):
#   E2->C3 A2  96(CP1252 en-dash)->E2 80 93  B6->C2 B6
$bytes = Replace-Bytes $bytes @(0xC3,0xA2,0xE2,0x80,0x93,0xC2,0xB6) @(0xE2,0x96,0xB6)

# no-entry U+26D4 (E2 9B 94):
#   E2->C3 A2  9B(CP1252 single-angle-quote)->E2 80 BA  94(CP1252 ")->E2 80 9D
$bytes = Replace-Bytes $bytes @(0xC3,0xA2,0xE2,0x80,0xBA,0xE2,0x80,0x9D) @(0xE2,0x9B,0x94)

# em-dash U+2014 (E2 80 94):
#   E2->C3 A2  80(CP1252 euro)->E2 82 AC  94(CP1252 ")->E2 80 9D
$bytes = Replace-Bytes $bytes @(0xC3,0xA2,0xE2,0x82,0xAC,0xE2,0x80,0x9D) @(0xE2,0x80,0x94)

[System.IO.File]::WriteAllBytes($file, $bytes)
Write-Host "Done. Bytes: $orig -> $($bytes.Length) (reduced by $($orig - $bytes.Length))"
