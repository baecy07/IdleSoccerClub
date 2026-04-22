# Idle Soccer Club MVP

Unity-based mobile idle soccer club management MVP.

## Goal
- Build a playable MVP first.
- Keep the architecture ready for future live-service/server-authoritative migration.
- Validate the core loop before adding production backend features.

## Current Scope
- Idle warmup farming
- Offline reward calculation
- League challenge flow and match result transition
- Player leveling and star promotion
- Scout 1x / 10x
- Facility upgrades for 4 facilities
- Formation / tactic / team-color bonuses
- Save / load
- Runtime debug panel

## Architecture
- `Assets/Scripts/Core`: bootstrap and entry point
- `Assets/Scripts/Data`: config, domain, and save models
- `Assets/Scripts/Services`: interfaces and local implementations
- `Assets/Scripts/Systems`: gameplay calculations
- `Assets/Scripts/UI`: runtime-generated uGUI
- `Assets/Resources/Configs`: JSON dummy balance data

## Open In Unity
1. Clone the repository.
2. Open this folder in Unity Hub.
3. Use Unity 2021.3 LTS or newer.
4. Open `Assets/Scenes/Main.unity`.
5. Press Play.

The project uses a runtime bootstrap, so the UI and gameplay systems are created automatically when the scene runs.

## Git Workflow
- Main remote repository: `https://github.com/baecy07/IdleSoccerClub.git`
- Work on feature branches when possible.
- Pull before starting work on another machine.
- Push often so home/office machines stay in sync.

## Notes
- Server APIs are not implemented yet.
- Service boundaries are already split so local implementations can be replaced later.
- If Unity generates `.meta`, `Library`, or lock files on first open, that is expected.
