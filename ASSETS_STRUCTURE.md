# Structure des Assets CCEM

## 📁 Organisation des Assets

Tous les assets de l'application sont organisés dans `src/CCEM/Assets/` selon la structure suivante :

```
Assets/
├── AppIcon.ico                  ← Icône multi-résolution (16-256px)
├── AppIcon.png                  ← Icône principale 256x256
│
├── AppIcons/                    ← 47 fichiers (Icônes de l'application)
│   ├── AppList.targetsize-*.png                    (14 tailles: 16-256px)
│   ├── AppList.targetsize-*_altform-unplated.png   (14 variantes thème sombre)
│   ├── AppList.targetsize-*_altform-lightunplated.png (14 variantes thème clair)
│   └── Square44x44Logo.scale-*.png                 (5 scales: 100-400%)
│
├── Tiles/                       ← 15 fichiers (Tuiles Windows)
│   ├── Square150x150Logo.scale-*.png  (5 scales - Medium Tile)
│   ├── Square71x71Logo.scale-*.png    (5 scales - Small Tile)
│   └── Wide310x150Logo.scale-*.png    (5 scales - Wide Tile)
│
├── Splash/                      ← 5 fichiers (Écran de démarrage)
│   └── SplashScreen.scale-*.png       (5 scales: 100-400%)
│
├── Store/                       ← 5 fichiers (Microsoft Store)
│   └── StoreLogo.scale-*.png          (5 scales: 100-400%)
│
├── Cover/                       ← Images de couverture
├── Fluent/                      ← Icônes Fluent Design
└── NavViewMenu/                 ← Icônes de navigation
```

## 📊 Récapitulatif

| Catégorie | Nombre de fichiers | Tailles générées | Usage |
|-----------|-------------------|------------------|--------|
| **AppIcon** | 2 | 256x256, multi-résolution | Icône principale de l'app |
| **AppList** | 42 | 16-256px (3 variantes/taille) | Barre des tâches, menu Démarrer, Alt+Tab |
| **Square44x44Logo** | 5 | 44-176px | App icon avec scaling |
| **Square150x150Logo** | 5 | 150-600px | Medium Tile (Windows 10) |
| **Square71x71Logo** | 5 | 71-284px | Small Tile (Windows 10) |
| **Wide310x150Logo** | 5 | 310x150 - 1240x600 | Wide Tile (Windows 10) |
| **SplashScreen** | 5 | 620x300 - 2480x1200 | Écran de démarrage |
| **StoreLogo** | 5 | 50-200px | Microsoft Store |
| **TOTAL** | **74 fichiers** | | |

## 🎯 Facteurs d'échelle (Scale Factors)

Les assets sont générés pour tous les facteurs d'échelle Windows :

| Scale | DPI | Usage typique | Exemple de résolution |
|-------|-----|---------------|----------------------|
| 100% | 96 DPI | Écrans 1080p standard | 1920x1080 |
| 125% | 120 DPI | Écrans 1080p avec scaling | 1920x1080 (Surface) |
| 150% | 144 DPI | Écrans haute résolution | 2560x1440 |
| 200% | 192 DPI | Écrans 4K, Retina | 3840x2160 |
| 400% | 384 DPI | Écrans 8K | 7680x4320 |

## 🎨 Variantes de thème (AppList uniquement)

Les icônes AppList existent en 3 variantes pour s'adapter aux thèmes Windows :

1. **Default (plated)** : `AppList.targetsize-*.png`
   - Avec plaque de fond système
   - Utilisé si les variantes unplated ne sont pas présentes

2. **Dark theme (unplated)** : `AppList.targetsize-*_altform-unplated.png`
   - Fond transparent
   - Optimisé pour le thème sombre Windows

3. **Light theme (lightunplated)** : `AppList.targetsize-*_altform-lightunplated.png`
   - Fond transparent
   - Optimisé pour le thème clair Windows

## 📝 Références dans Package.appxmanifest

```xml
<Properties>
    <Logo>Assets\Store\StoreLogo.png</Logo>
</Properties>

<uap:VisualElements
    Square150x150Logo="Assets\Tiles\Square150x150Logo.png"
    Square44x44Logo="Assets\AppIcons\Square44x44Logo.png">
    <uap:DefaultTile
        Wide310x150Logo="Assets\Tiles\Wide310x150Logo.png"
        Square71x71Logo="Assets\Tiles\Square71x71Logo.png"/>
    <uap:SplashScreen Image="Assets\Splash\SplashScreen.png"/>
</uap:VisualElements>
```

**Note importante** : Seuls les noms de base sont spécifiés dans le manifeste. Windows sélectionne automatiquement la bonne variante (scale/theme) via le système MRT Core.

## 🔄 Régénération des assets

Pour régénérer tous les assets à partir d'un nouveau logo :

1. Remplacer `Logo_Original.png` (recommandé : 2048x2048 PNG avec fond transparent)
2. Exécuter le script : `.\Generate-AppAssets.ps1`
3. Tous les assets seront régénérés automatiquement

### Prérequis

- **ImageMagick** : `winget install ImageMagick.ImageMagick`
- PowerShell 5.1 ou supérieur

## 📌 Notes importantes

- ✅ Tous les assets sont générés avec fond transparent (background=none)
- ✅ Les images sont redimensionnées avec haute qualité (ImageMagick)
- ✅ La structure respecte les guidelines Microsoft pour WinUI3
- ✅ Compatible Windows 10 et Windows 11
- ✅ Requis minimum pour publication sur le Microsoft Store

## 🔗 Documentation Microsoft

- [App icon construction](https://learn.microsoft.com/en-us/windows/apps/design/style/iconography/app-icon-construction)
- [Tailor resources for scale, theme, and contrast](https://learn.microsoft.com/en-us/windows/apps/windows-app-sdk/mrtcore/tailor-resources-lang-scale-contrast)
- [MRT Core resource management](https://learn.microsoft.com/en-us/windows/apps/windows-app-sdk/mrtcore/mrtcore-overview)

---

**Dernière mise à jour** : 4 janvier 2026
**Source logo** : `Logo_Original.png` (2048x2048)
**Script de génération** : `Generate-AppAssets.ps1`
