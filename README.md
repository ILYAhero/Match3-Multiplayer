# Mono Orbit — multiplayer match-3

Unity project for a competitive two-player match-3 game on one shared 9 × 9 board.

## Development baseline

- Unity Editor **6000.3.7f1**
- C#, Unity Test Framework 1.6.0
- Architecture: Unity-independent gameplay domain, then application/networking/presentation layers
- Primary branch: `main`; work in feature branches and review before merging

## Implemented in feature/match-core

- Immutable row-major `BoardState` (`Y=0` is bottom)
- Six weather elements with stable numeric IDs
- Deterministic seeded board creation without initial matches; every board has a legal move
- Adjacent swap validation, horizontal/vertical match detection, unique crossed matches
- Cascade gravity/refill, per-cascade scoring, regeneration of boards without valid moves
- Domain tests using NUnit (Edit Mode)

The gameplay library does not depend on `UnityEngine`. It does **not** yet render a board or enable multiplayer. No gameplay scene, Relay connection, or mobile build is claimed ready.

## How to run tests

1. Open the project with Unity Editor **6000.3.7f1**.
2. In Unity, open **Window → General → Test Runner**.
3. Choose **EditMode**, then **Run All**.
4. Report any import, compile, or test errors before merging the pull request.

## Repository conventions

Keep gameplay rules out of MonoBehaviours and network transport classes.
The authoritative host will accept player commands, call `MatchEngine`, and broadcast outcomes/snapshots. Presentation will replay `CascadeStep` transitions; clients must never independently resolve authoritative moves. Do not alter element IDs once sent over the network without protocol versioning.

See [architecture and milestones](Docs/ARCHITECTURE.md).
