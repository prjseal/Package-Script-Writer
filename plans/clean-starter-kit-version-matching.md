# Clean starter kit: match version to the Umbraco major

## Context

When "Clean Starter Kit (version matched to Umbraco)" is chosen, PSW pins Clean to the newest version whose
NuGet `Umbraco.Cms.*` dependency range accepts the selected Umbraco version
(`PSW.Shared/Services/PackageCompatibilityService.cs`, called from
`ScriptGeneratorService.GenerateAddStarterKitScript`, line ~269). Nothing is hard-coded per package.

Clean's version numbers used to sit 10 below Umbraco's (Clean 7 is for Umbraco 17, Clean 8 for Umbraco 18). From
Umbraco 19 they match (Clean 19 is for Umbraco 19). Every Clean release declares an **open-ended** range, as
NuGet shows:

| Clean | Umbraco.Cms.Web.Website range |
|---|---|
| 7.0.8 | `[17.5.1, )` |
| 8.0.2 | `[18.3.1, )` |
| 19.0.0-beta01 | `[19.0.0-beta1, )` |

So Clean 8.0.2 also "satisfies" Umbraco 19. Stable versions are tried before prereleases, so:

- **Umbraco 19.0.0-beta1** (on NuGet now) is pinned to **Clean 8.0.2**, which is wrong. It should get 19.0.0-beta01.
- **Umbraco 19 stable, before a stable Clean 19 ships**, would also get 8.0.2.
- The same thing happens for any package that uses open-ended ranges, whenever a new Umbraco major comes out.

Intended outcome: prefer a package version built for the same Umbraco major. If no stable version matches the
major, use a prerelease that does (this was the user's decision). Only if nothing matches the major, keep today's
"any range that satisfies" behaviour.

## Implementation

1. **Branch**: there's no `develop` branch, so start from `main`: `git checkout main`, `git pull`,
   `git checkout -b fix/clean-starter-kit-version-matching`.

2. **`src/PSW.Shared/Services/PackageCompatibilityService.cs`**
   - Make `IsCompatibleAsync` return a private enum instead of `bool?`: `Unknown` (today's `null`), `Incompatible`,
     `Compatible`, `CompatibleSameMajor`.
     - Direct Umbraco dependencies: no range satisfies → `Incompatible`. A satisfying range with
       `HasLowerBound && MinVersion.Major == umbraco.Major` → `CompatibleSameMajor`. Otherwise `Compatible`.
     - Meta-package recursion (uSync and similar) passes the resolved dependency's result up unchanged. It keeps
       treating `Unknown` as "try the next dependency", as `null` works today.
   - `GetCompatibleVersionAsync`:
     - Candidates: **all listed** versions, ordered stable first, then by descending version. Prereleases are no
       longer filtered out up front.
     - Loop once. Return the first `CompatibleSameMajor`. Because stable versions come first, a stable same-major
       match always beats a prerelease one.
     - Remember the first `Compatible`/`CompatibleSameMajor` that passes the old prerelease rule
       (`!IsPrerelease || umbraco.IsPrerelease`) as the fallback. After the loop, return it.
     - `NoCompatibleVersion`/`Unknown` work as before (`foundIncompatible` tracking).
   - Update the class XML doc to describe the same-major preference and the prerelease fallback.
   - Accepted side effect: a stable Umbraco can now be pinned to a prerelease when that is the only version that
     matches its major (e.g. stable Umbraco 19 plus Clean 19.0.0-beta01).

3. **Unit tests**: `src/PackageCliTool.Tests/PackageCompatibilityServiceTests.cs`. Keep every existing test
   passing, and add these cases using the existing `Index`/`Leaf` helpers:
   - Clean set 7.0.8 `[17.5.1, )`, 8.0.2 `[18.3.1, )`, 19.0.0-beta01 `[19.0.0-beta1, )`:
     - `19.0.0-beta1` → `19.0.0-beta01`
     - `19.0.0` (stable, only the Clean 19 prerelease exists) → `19.0.0-beta01`
     - `18.3.1` → `8.0.2`, and `17.7.0` → `7.0.8` (no regression)
   - Add stable 19.0.0 `[19.0.0, )` to that set: `19.0.0` → `19.0.0` (stable wins over the prerelease).
   - Fallback is kept: a package whose only version is 2.0.0 `[14.0.0, )` gives Umbraco `17.7.0` → `2.0.0`.
   - Stable is still preferred in the fallback tier: stable 2.0.0 `[14.0.0, )` plus 3.0.0-beta `[16.0.0, )`
     gives Umbraco `17.7.0` → `2.0.0`.
   - Meta-package: uSync 19.0.0 → uSync.BackOffice `[19.0.0, 20.0.0)` resolves as same-major.

4. **Integration test**: `src/PSW.IntegrationTests/ScriptGeneratorApiTests.cs` line ~200. Add
   `[InlineData("19.0.0-beta1", "clean", "package clean --version 19.")]` next to the existing starter kit cases.
   It calls live NuGet, as its neighbours do.

5. **CHANGELOG**: `src/PackageCliTool/CHANGELOG.md`, under `[Unreleased]` / `### Fixed`. Add an entry along the
   lines of: **Starter Kit Version Matching Across Umbraco Majors**. Packages with open-ended Umbraco dependency
   ranges (e.g. Clean) are now pinned to the version built for the selected Umbraco major (Umbraco 19 → Clean 19)
   instead of an older major whose open range also accepts it. A prerelease is used when it is the only version for
   that major.

No changes are needed in `Options.cshtml`, `site.js` or the CLI. They all go through `ScriptGeneratorService` and
`PackageCompatibilityService`.

## Verification

- `dotnet build ./src/PSW.sln` and `dotnet build ./src/PackageCliTool.sln`
- `dotnet test ./src/PackageCliTool.Tests/PackageCliTool.Tests.csproj` (the unit tests above)
- `dotnet test ./src/PSW.IntegrationTests/PSW.IntegrationTests.csproj` (needs network: the 17.7.0, 13.8.0 and
  19.0.0-beta1 Clean cases)
- Manual: `dotnet watch run --project ./src/PSW/`, choose Umbraco.Templates `19.0.0-beta1` with the Clean starter
  kit (version matched). The script should contain `package clean --version 19.0.0-beta01`. `18.3.1` should give
  `8.0.2`.
- Commit locally. Don't push.
