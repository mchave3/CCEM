# Task Plan: Migration sccmclictr -> CCEM

## Goal
Porter l’application legacy **sccmclictr** (.NET Framework 4.8 / WPF) vers **CCEM** (.NET 10 / WinUI 3) en migrant la logique (lib + services) et l’UI (écrans & workflows) avec une architecture maintenable.

## Phases
- [x] Phase 1: Réactiver le code legacy (CCCM_source)
- [x] Phase 2: Analyser sccmclictr (archi + features)
- [x] Phase 3: Analyser CCEM (archi + patterns)
- [x] Phase 4: Plan de migration (mapping + milestones)
- [x] Phase 5: Portage logique (libs .NET 10)
- [x] Phase 6: Portage UI (WinUI 3 pages)
- [ ] Phase 7: Validation (build, smoke tests)

## Key Questions
1. Quelles sont les “capabilities” principales de sccmclictr (features, écrans, plugins) à reproduire ?
2. Quelle séparation CCEM doit-on viser (Core / Modules / UI) pour garder une base propre ?
3. Quels composants legacy sont réutilisables tels quels (WMI, registry, services) et lesquels doivent être réécrits (UI, threading, interop) ?
4. Comment gérer plugins/add-ons côté WinUI (chargement, navigation, permissions, packaging) ?

## Decisions Made
- Restaurer le code legacy depuis l’historique git (commit `0d01d34`) plutôt que les placeholders “submodule” actuels, afin de pouvoir analyser et porter.

## Errors Encountered
- `dotnet build src/CCEM/CCEM.csproj` échoue si `Platform` n’est pas `x64` (WindowsAppSDKSelfContained) : utiliser `dotnet build src/CCEM.slnx -p:Platform=x64`.

## Status
**Currently in Phase 7** - UI SCCM: tous les onglets legacy + un écran `Sccm.Tools` (équivalent plugins) sont portés ; reste à faire: smoke tests (runtime), polish UX et packaging.
