#Requires -Version 5.1
<#
.SYNOPSIS
    Génère tous les assets d'application WinUI3 à partir d'un logo source.

.DESCRIPTION
    Ce script utilise ImageMagick pour générer tous les assets requis pour une application WinUI3,
    incluant les icônes AppList, tiles, splash screens, store logos, et fichiers .ico/.png.

.PARAMETER SourceLogo
    Chemin vers l'image source (recommandé: 2048x2048 PNG avec fond transparent)

.PARAMETER OutputPath
    Dossier de sortie pour les assets (défaut: src/CCEM/Assets)
#>

param(
    [string]$SourceLogo = "Logo_Original.png",
    [string]$SourceWideSplash = "CCEM_wide_splash.jpeg",
    [string]$OutputPath = "src\CCEM\Assets"
)

# Configuration
$ErrorActionPreference = "Stop"
$ProgressPreference = "Continue"

# Couleurs pour l'output
function Write-ColorOutput {
    param([string]$Message, [string]$Color = "White")
    Write-Host $Message -ForegroundColor $Color
}

# Vérifier ImageMagick
Write-ColorOutput "`n=== Vérification d'ImageMagick ===" "Cyan"
try {
    $magickVersion = & magick -version 2>&1 | Select-Object -First 1
    Write-ColorOutput "✓ ImageMagick détecté: $magickVersion" "Green"
} catch {
    Write-ColorOutput "✗ ImageMagick n'est pas installé!" "Red"
    Write-ColorOutput "Installation: winget install ImageMagick.ImageMagick" "Yellow"
    exit 1
}

# Vérifier le fichier source
Write-ColorOutput "`n=== Vérification des fichiers source ===" "Cyan"
if (-not (Test-Path $SourceLogo)) {
    Write-ColorOutput "✗ Fichier logo introuvable: $SourceLogo" "Red"
    exit 1
}

if (-not (Test-Path $SourceWideSplash)) {
    Write-ColorOutput "✗ Fichier wide/splash introuvable: $SourceWideSplash" "Red"
    exit 1
}

$sourceInfo = & magick identify -format "%wx%h %[colorspace] %[channels]" $SourceLogo
Write-ColorOutput "✓ Logo source: $sourceInfo" "Green"

$wideInfo = & magick identify -format "%wx%h %[colorspace] %[channels]" $SourceWideSplash
Write-ColorOutput "✓ Wide/Splash source: $wideInfo" "Green"

# Créer la structure de dossiers
Write-ColorOutput "`n=== Création de la structure de dossiers ===" "Cyan"
$folders = @(
    "$OutputPath\AppIcons",
    "$OutputPath\Tiles",
    "$OutputPath\Splash",
    "$OutputPath\Store"
)

foreach ($folder in $folders) {
    if (-not (Test-Path $folder)) {
        New-Item -ItemType Directory -Path $folder -Force | Out-Null
        Write-ColorOutput "✓ Créé: $folder" "Green"
    } else {
        Write-ColorOutput "○ Existe: $folder" "Gray"
    }
}

# Fonction de génération d'image
function New-Asset {
    param(
        [string]$OutputFile,
        [int]$Width,
        [int]$Height = $Width,
        [string]$Background = "none",
        [string]$Description = "",
        [string]$SourceImage = $SourceLogo,
        [bool]$CropToFit = $false
    )

    $fullPath = Join-Path $PSScriptRoot $OutputFile

    try {
        # Redimensionner avec haute qualité
        if ($CropToFit) {
            # Pour les images wide/splash: redimensionner pour couvrir puis rogner au centre
            & magick $SourceImage -resize "${Width}x${Height}^" -gravity center -extent "${Width}x${Height}" -flatten $fullPath 2>&1 | Out-Null
        } elseif ($Background -eq "none") {
            & magick $SourceImage -background none -resize "${Width}x${Height}!" -flatten $fullPath 2>&1 | Out-Null
        } else {
            & magick $SourceImage -background $Background -resize "${Width}x${Height}!" -flatten $fullPath 2>&1 | Out-Null
        }

        if ($LASTEXITCODE -eq 0) {
            Write-ColorOutput "  ✓ $Description" "DarkGray"
            return $true
        } else {
            Write-ColorOutput "  ✗ Erreur: $Description" "Red"
            return $false
        }
    } catch {
        Write-ColorOutput "  ✗ Exception: $Description - $($_.Exception.Message)" "Red"
        return $false
    }
}

# Compteurs
$totalFiles = 0
$successCount = 0

# ===== 1. APPICON.PNG et APPICON.ICO =====
Write-ColorOutput "`n=== Génération AppIcon.png et AppIcon.ico ===" "Cyan"

# AppIcon.png (256x256)
if (New-Asset -OutputFile "$OutputPath\AppIcon.png" -Width 256 -Description "AppIcon.png (256x256)") {
    $successCount++
}
$totalFiles++

# AppIcon.ico (multi-tailles)
Write-ColorOutput "  Génération AppIcon.ico (multi-résolution)..." "White"
$icoPath = Join-Path $PSScriptRoot "$OutputPath\AppIcon.ico"
$tempIcoFolder = Join-Path $PSScriptRoot "temp_ico"
New-Item -ItemType Directory -Path $tempIcoFolder -Force | Out-Null

# Générer les tailles pour ICO
$icoSizes = @(16, 24, 32, 48, 64, 96, 128, 256)
$icoFiles = @()

foreach ($size in $icoSizes) {
    $tempFile = Join-Path $tempIcoFolder "icon_$size.png"
    & magick $SourceLogo -background none -resize "${size}x${size}!" $tempFile 2>&1 | Out-Null
    $icoFiles += $tempFile
}

# Créer le fichier ICO
& magick $icoFiles $icoPath 2>&1 | Out-Null
if ($LASTEXITCODE -eq 0) {
    Write-ColorOutput "  ✓ AppIcon.ico (16,24,32,48,64,96,128,256)" "Green"
    $successCount++
} else {
    Write-ColorOutput "  ✗ Erreur AppIcon.ico" "Red"
}
$totalFiles++

# Nettoyer les fichiers temporaires
Remove-Item -Path $tempIcoFolder -Recurse -Force

# ===== 2. APPLIST ICONS (targetsize) =====
Write-ColorOutput "`n=== Génération AppList Icons (42 fichiers) ===" "Cyan"

$targetSizes = @(16, 20, 24, 30, 32, 36, 40, 48, 60, 64, 72, 80, 96, 256)

foreach ($size in $targetSizes) {
    # Default (plated)
    if (New-Asset -OutputFile "$OutputPath\AppIcons\AppList.targetsize-$size.png" -Width $size -Description "AppList.targetsize-$size.png") {
        $successCount++
    }
    $totalFiles++

    # Dark theme (unplated)
    if (New-Asset -OutputFile "$OutputPath\AppIcons\AppList.targetsize-${size}_altform-unplated.png" -Width $size -Description "AppList.targetsize-${size}_altform-unplated.png") {
        $successCount++
    }
    $totalFiles++

    # Light theme (lightunplated)
    if (New-Asset -OutputFile "$OutputPath\AppIcons\AppList.targetsize-${size}_altform-lightunplated.png" -Width $size -Description "AppList.targetsize-${size}_altform-lightunplated.png") {
        $successCount++
    }
    $totalFiles++
}

# ===== 3. SQUARE 44x44 LOGO (scale) =====
Write-ColorOutput "`n=== Génération Square44x44Logo (5 fichiers) ===" "Cyan"

$scaleFactors = @{
    "100" = 44
    "125" = 55
    "150" = 66
    "200" = 88
    "400" = 176
}

foreach ($scale in $scaleFactors.Keys) {
    $size = $scaleFactors[$scale]
    if (New-Asset -OutputFile "$OutputPath\AppIcons\Square44x44Logo.scale-$scale.png" -Width $size -Description "Square44x44Logo.scale-$scale.png (${size}x${size})") {
        $successCount++
    }
    $totalFiles++
}

# ===== 4. SQUARE 150x150 LOGO (Medium Tile) =====
Write-ColorOutput "`n=== Génération Square150x150Logo (5 fichiers) ===" "Cyan"

$mediumTileSizes = @{
    "100" = 150
    "125" = 188
    "150" = 225
    "200" = 300
    "400" = 600
}

foreach ($scale in $mediumTileSizes.Keys) {
    $size = $mediumTileSizes[$scale]
    if (New-Asset -OutputFile "$OutputPath\Tiles\Square150x150Logo.scale-$scale.png" -Width $size -Description "Square150x150Logo.scale-$scale.png (${size}x${size})") {
        $successCount++
    }
    $totalFiles++
}

# ===== 5. SQUARE 71x71 LOGO (Small Tile) =====
Write-ColorOutput "`n=== Génération Square71x71Logo (5 fichiers) ===" "Cyan"

$smallTileSizes = @{
    "100" = 71
    "125" = 89
    "150" = 107
    "200" = 142
    "400" = 284
}

foreach ($scale in $smallTileSizes.Keys) {
    $size = $smallTileSizes[$scale]
    if (New-Asset -OutputFile "$OutputPath\Tiles\Square71x71Logo.scale-$scale.png" -Width $size -Description "Square71x71Logo.scale-$scale.png (${size}x${size})") {
        $successCount++
    }
    $totalFiles++
}

# ===== 6. WIDE 310x150 LOGO (Wide Tile) =====
Write-ColorOutput "`n=== Génération Wide310x150Logo (5 fichiers) ===" "Cyan"

$wideTileSizes = @{
    "100" = @(310, 150)
    "125" = @(388, 188)
    "150" = @(465, 225)
    "200" = @(620, 300)
    "400" = @(1240, 600)
}

foreach ($scale in $wideTileSizes.Keys) {
    $width = $wideTileSizes[$scale][0]
    $height = $wideTileSizes[$scale][1]
    if (New-Asset -OutputFile "$OutputPath\Tiles\Wide310x150Logo.scale-$scale.png" -Width $width -Height $height -SourceImage $SourceWideSplash -CropToFit $true -Description "Wide310x150Logo.scale-$scale.png (${width}x${height})") {
        $successCount++
    }
    $totalFiles++
}

# ===== 7. SPLASH SCREEN =====
Write-ColorOutput "`n=== Génération SplashScreen (5 fichiers) ===" "Cyan"

$splashSizes = @{
    "100" = @(620, 300)
    "125" = @(775, 375)
    "150" = @(930, 450)
    "200" = @(1240, 600)
    "400" = @(2480, 1200)
}

foreach ($scale in $splashSizes.Keys) {
    $width = $splashSizes[$scale][0]
    $height = $splashSizes[$scale][1]
    if (New-Asset -OutputFile "$OutputPath\Splash\SplashScreen.scale-$scale.png" -Width $width -Height $height -SourceImage $SourceWideSplash -CropToFit $true -Description "SplashScreen.scale-$scale.png (${width}x${height})") {
        $successCount++
    }
    $totalFiles++
}

# ===== 8. STORE LOGO =====
Write-ColorOutput "`n=== Génération StoreLogo (5 fichiers) ===" "Cyan"

$storeSizes = @{
    "100" = 50
    "125" = 63
    "150" = 75
    "200" = 100
    "400" = 200
}

foreach ($scale in $storeSizes.Keys) {
    $size = $storeSizes[$scale]
    if (New-Asset -OutputFile "$OutputPath\Store\StoreLogo.scale-$scale.png" -Width $size -Description "StoreLogo.scale-$scale.png (${size}x${size})") {
        $successCount++
    }
    $totalFiles++
}

# ===== RÉSUMÉ =====
Write-ColorOutput "`n=== RÉSUMÉ ===" "Cyan"
Write-ColorOutput "Total fichiers générés: $successCount / $totalFiles" "White"

if ($successCount -eq $totalFiles) {
    Write-ColorOutput "✓ Tous les assets ont été générés avec succès!" "Green"
} else {
    $failed = $totalFiles - $successCount
    Write-ColorOutput "⚠ $failed fichier(s) ont échoué" "Yellow"
}

Write-ColorOutput "`nStructure créée dans: $OutputPath" "Cyan"
Write-ColorOutput @"

Prochaines étapes:
1. Vérifier les assets générés
2. Mettre à jour Package.appxmanifest
3. Mettre à jour le fichier .csproj si nécessaire

"@ "Gray"
