# Radio Control / Dispatch Line (Prototype)

A Unity 6 prototype scaffolding for the "Radio Control / Dispatch Line" dispatcher fantasy game. The project focuses on gameplay logic and scripting; UI and art can be layered on later.

## Project Structure
```
Assets/
  Scripts/
    Core/       // GameManager, Scoring
    Calls/      // Call data models and manager
    Units/      // Unit models and manager
    Voice/      // Voice input, command parsing, execution
    Radio/      // Radio transcript logging
    UI/         // Radio panel, split screen terminal + units, UI helpers
    Codes/      // 10-code ScriptableObjects
  Tests/
    EditMode/   // NUnit edit mode tests
```

## Key Systems
- **CallManager**: Spawns and tracks incidents with priorities and timers.
- **UnitManager**: Maintains unit roster and assignment/status changes.
- **VoiceInputManager**: Stub for platform speech recognition events.
- **CommandInterpreter / CommandExecutor**: Converts recognized speech to intents and executes them.
- **RadioSystem**: Logs radio-style transcript lines (extend with audio later).
- **ScoringSystem**: Awards points when calls are resolved within target times.
- **UI Panels (Phase 2 scaffolding)**: `UIRadioPanel` updates top-bar info and transcript, `UITerminalPanel` and `UIUnitsPanel` populate call/unit lists, and `UISwipeController` animates the split-screen reveal.
- **Voice Command Stack (Phase 3-ready)**: `VoiceInputManager` exposes listening start/stop and recognition events, while `VoiceCommandController` hooks recognized text into `CommandInterpreter`/`CommandExecutor` so spoken orders dispatch units or resolve calls.
- **Immersion & Feedback (Phase 4)**: `CodeLibrary` aggregates any number of `CodeSet` assets for 10-codes and synonyms, `RadioAudioProfile`/`RadioFeedbackProfile` configure clicks/static/voice reply sounds and success/error highlights, and `TutorialManager` drives a guided training sequence with customizable steps.
- **Content & Progression (Phase 5)**: `ShiftConfig` scales call spawn intensity over a shift, `CallLibrary` and `UnitRoster` let you load larger banks of incidents and units, `GameManager` now coordinates shift timing and failure conditions, and `ProfileManager` persists best scores, ranks, and settings.
- **Localization, Grammar, and Terminal Controls**: `VoiceLanguageProfile` now drives multilingual grammar packs (backup code levels, status tokens, panic words) and officer replies, while `UnitTerminalController` exposes terminal-driven status updates, call assignments, and acknowledgement labels (e.g., "10-4") without overriding the voice-first flow.
- **Voice-Spawned Callouts & Backup Types**: Add voice-trigger tokens to any `CallTemplate` so dispatchers can verbally start or end callouts, push code 2/3/pursuit/air/SWAT/EMS/firetruck backup, and update unit statuses (available, on scene, in custody, panic) with radio feedback.
- **Officer Voice Buckets & Callout Speech Libraries**: Assign a `UnitVoiceProfile` asset to every unit to tie officer-specific audio clips (hundreds of variants per response type) to the unit that was mentioned, and author `CalloutSpeechLibrary` assets so custom callouts and spoken keywords can be swapped in without code changes.

## Running Edit Mode Tests
From Unity's Test Runner, select **Edit Mode** and run all tests. Sample coverage ensures that the command interpreter can parse an assignment command and link it to a known unit and call, and that the voice controller routes recognized phrases into executable dispatches.

## Next Steps
- Hook UI buttons/gestures to the provided methods.
- Replace VoiceInputManager stubs with platform-specific microphone integrations.
- Add more parsing rules, code sets, audio, and content templates.
- Author additional `ShiftConfig`, `CallLibrary`, and `UnitRoster` assets to tune difficulty curves and content variety.
