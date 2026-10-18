# Cozy World integration — stage 1

This branch is based on `feature/match-screen`, not `main`.

## Implemented
- Preserves `MatchEngine`, `PracticeSession`, all `WeatherElement` numeric IDs and gameplay tests.
- Adds optional `CozyWorldTheme`, applied to the existing real 9x9 practice field.
- Explicit art mapping (presentation only): Sun → star, Rain → drop, Snow → flower, Wind → leaf, Thunder → orange orb, Cloud → heart.
- Uses authored sprites for board cells, selected HUD panels and the six weather elements when a theme exists; original Mono Orbit rendering remains a fallback.
- Retains existing tap/swipe input and the existing cascade/score updates. Does not simulate online play.

## Install and review in Unity 6000.3.7f1
1. Pull `feature/cozy-world-integration` in GitHub Desktop (commit local work beforehand).
2. In Unity choose **Assets → Import Package → Custom Package** and import your `CozyWorld_Assets_v0.1.unitypackage`. It is **not committed to this branch**; import only once and check the Console. Keep its original paths under `Assets/CozyWorld/Art/Sprites` and preserve the provided `.meta` files.
3. Choose **Tools → Mono Orbit → Cozy World → Build Theme from Imported Sprites**. This creates `Assets/MonoOrbit/Presentation/Resources/CozyWorldTheme.asset`. Inspect the six explicit mapping entries and their sizes.
4. Open the existing match screen scene created by **Tools → Mono Orbit → Create Match Scene** (create it only if missing; the original creator asks before replacement). Set the Game view to portrait 9:16, press Play, and verify swaps, scores and cascades.
5. Run **Window → General → Test Runner → EditMode → Run All**. Previous eleven tests remain relevant. If red errors appear, do not merge; report the first Console error.
6. Review on Android/iOS and measure device performance before production release.

## Scope and remaining work
This is an opt-in practice-match reskin, **not** the complete Cozy World UI package, scene replacement, or multiplayer implementation. The original PNGs, prefabs, blockers, boosters, effects, room UI and audio have not been committed to GitHub by this change. The theme must be built after importing assets. HUD layout remains largely Mono Orbit (with selective art/color changes), not the standalone CozyWorld v0.3 art-review screen.

Gameplay/network rules for concurrent and 5-second alternating turns, Relay/Sessions, join codes, authoritative timer, weather abilities and full visual VFX require separate tested milestones.

## Testing
Repository source was inspected and the changes were saved to this feature branch. **Compilation in Unity, EditMode tests on the new branch and phone testing have not been executed here**. Do not merge into main until Unity QA succeeds.
