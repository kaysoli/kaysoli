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
- **Response Audio Libraries**: `ResponseAudioLibrary` assets map reply keywords (Acknowledgement, Status, Backup, Panic, etc.) to WAV clip variations so officers can answer with different lines even when a specific `UnitVoiceProfile` clip is missing.
- **Content & Progression (Phase 5)**: `ShiftConfig` scales call spawn intensity over a shift, `CallLibrary` and `UnitRoster` let you load larger banks of incidents and units, `GameManager` now coordinates shift timing and failure conditions, and `ProfileManager` persists best scores, ranks, and settings.
- **Roster authoring**: `UnitDefinition` (single unit asset) and `UnitRoster` (list of inline units + reusable definitions) let you stamp out units with call signs, departments, voice profiles, and attached `OfficerProfile` crew members for rich identity without code changes.
- **Localization, Grammar, and Terminal Controls**: `VoiceLanguageProfile` now drives multilingual grammar packs (backup code levels, status tokens, panic words) and officer replies, while `UnitTerminalController` exposes terminal-driven status updates, call assignments, and acknowledgement labels (e.g., "10-4") without overriding the voice-first flow.
- **Voice-Spawned Callouts & Backup Types**: Add voice-trigger tokens to any `CallTemplate` so dispatchers can verbally start or end callouts, push code 2/3/pursuit/air/SWAT/EMS/firetruck backup, and update unit statuses (available, on scene, in custody, panic) with radio feedback.
- **Officer Voice Buckets & Callout Speech Libraries**: Assign a `UnitVoiceProfile` asset to every unit to tie officer-specific audio clips (hundreds of variants per response type) to the unit that was mentioned, and author `CalloutSpeechLibrary` assets so custom callouts and spoken keywords can be swapped in without code changes.
- **Records Terminal & Lookups**: Use the new `RecordsDatabase`/`RecordsManager` plus `RecordsTerminalPanel` to load subject/vehicle/officer data from assets or CSV/TSV text sheets. The terminal supports type filters (civilian/officer/prisoner/vehicle and extended roles like presidential staff, ministers, FBI, Secret Service, undercover, and military), and voice commands like "run plate 7XKZ991" trigger `LookupRecord` intent, log the response, and play the requesting unit's assigned voice.

### Dispatcher speech, keywords, and officer reply buckets
- **Dispatcher commands (default English profile)**: Assign units (`assign`, `send`, `respond`, unit tokens like "unit"), request status (`status`), request backup (`backup`, `additional` + code2/code3/pursuit/air/ambulance/EMS/fire), start/end callouts (`start call`, `begin call`, `end call`, `terminate`), cancel calls (`cancel`), panic checks (`panic`, `signal 100`), and record lookups (`run`, `check`, `lookup`, `plate`, `vehicle`, `id`, `subject`, `person`, `name`). Status tokens map to unit states such as `available`, `en route`, `on scene`, `busy`, `pursuit`, `transport`, `detained`/`in custody`, and `returning`. Add your own 10-codes or localized phrases by editing the active `VoiceLanguageProfile` asset.
- **Availability roll-call & ambient chatter**: Phrases like “are there any available units?” map to a dedicated intent that rolls up currently free units with localized formatting and optional officer voice replies. `ChatterManager` now supports looping ambient clips on realistic intervals to keep the radio alive between calls.
- **Officer voice variations**: Each `Unit` references a `UnitVoiceProfile` that holds clip lists per response type (acknowledgement, assignment, status, backup, panic, custody, termination). Populate `StatusReplies` with variations like `10-4`, `okay`, `copy`, or any custom audio so different officers answer uniquely when the dispatcher requests status.
- **Active roster control**: Use `UnitManager.SetUnits(...)` for bulk seeding (rosters, procedural pools, CSV-driven units) and `UnitManager.AddUnit(...)` to activate additional units at runtime. The voice pipeline always treats the player as dispatcher; units respond with their assigned voice profile and call sign.

### Records CSV/TSV format
- Place CSV/TSV sheets under `Assets/` and assign them to `RecordsManager.csvSheets` or attach them directly to the `csvFiles` list on a `RecordsDatabase` asset. When CSVs are present on the database asset it rebuilds itself on enable, so you can drag thousands of civilians/officers in without hand entry.
- The importer auto-detects tabs/semicolons/commas and supports either the simple legacy row (`Id,Name,Type,VehiclePlate,Notes,Metadata,Occupation`) or the full header provided by the user (`RECORD_ID\tTIMESTAMP\tDATA_VERSION...\tIS_WITNESS_PROTECTION\tTYPE`).
- Header columns are matched case-insensitively so order does not matter; unknown headers are ignored safely. Split-name columns are recombined into the display `Name`, and the `TYPE`/`RECORD_TYPE` column feeds the record category (with custom labels preserved).
- Example row (tab separated):
  ```
  RECORD_ID\tTIMESTAMP\t...\tIS_WITNESS_PROTECTION\tTYPE
  REC-1\t2025-12-07T19:12:55Z\t...\tNo\tCivilian Worker
  ```
- **Chatter & Lookup Replies**: `ChatterLibrary`/`ChatterManager` can auto-play ambient unit requests (plate, subject, victim follow-ups) with dispatcher responses routed through the records system, unit voice buckets, response audio libraries, and roger beeps to keep the airwaves alive even during automated chatter.
- **Simulation Helpers & PTT Hotkeys**: `UnitGenesisPool` can bulk-generate 100+ units from rosters, CSV sheets, or procedural LAPD-style call signs, while `VoiceHotkeyRouter` + `QueuedSpeechProviderAsset` let you test push-to-talk and speech routing in the Editor using a keyboard and queued phrases. `DispatchSimulationController` glues generation, voice routing, and shift start-up together for quick end-to-end simulations.

## Running Edit Mode Tests
From Unity's Test Runner, select **Edit Mode** and run all tests. Sample coverage ensures that the command interpreter can parse an assignment command and link it to a known unit and call, and that the voice controller routes recognized phrases into executable dispatches.

## Next Steps
- Hook UI buttons/gestures to the provided methods.
- Replace VoiceInputManager stubs with platform-specific microphone integrations.
- Add more parsing rules, code sets, audio, and content templates.
- Author additional `ShiftConfig`, `CallLibrary`, and `UnitRoster` assets to tune difficulty curves and content variety.

## Minimal CSV-driven scenario sandbox (no full UI required)
You can spin up a quick test scene without the full terminal/UI stack by wiring only the simulation helpers and CSV inputs:

1. **Create a folder for your sheets**: Drop any CSV/TSV files under `Assets/Scenarios/` (Unity imports them as `TextAsset`).
   - **Records**: Use the verbose header (`RECORD_ID\tTIMESTAMP...IS_WITNESS_PROTECTION\tTYPE`) or the simple header
     (`Id,Name,Type,VehiclePlate,Notes,Metadata,Occupation`). Assign these sheets to `RecordsManager.csvSheets`.
   - **Units**: Point `UnitGenesisPool` at a `UnitRoster` asset or a units CSV (Id, DisplayName, UnitType, Status, Zone, VoiceProfileId).
   - **Calls**: Assign a `CallLibrary` asset with your `CallTemplate` ScriptableObjects, or author a `CalloutSpeechLibrary` to spawn calls by voice.

2. **Scene wiring**: Add a `DispatchSimulationController` to an empty scene and connect:
   - `GameManager`, `CallManager`, `UnitManager`, `RadioSystem`, `RecordsManager`, `VoiceCommandController`, and `VoiceInputManager` references.
   - Optional: `UnitGenesisPool` (to auto-seed 100+ rostered/procedural units), `VoiceHotkeyRouter` (desktop push-to-talk testing),
     `RadioAudioProfile` with your roger beep, and `RadioFeedbackProfile` for transcript cues.

3. **Run the sandbox**: Press Play—`DispatchSimulationController` will generate units (if present), initialize the voice stack, and begin a shift.
   You can trigger chatter via `ChatterManager.TriggerChatter`, spawn calls via `CallManager.SpawnCallFromTemplate`, or press your PTT button (wired
   to `UIManager.OnPttPressed/OnPttReleased` or `VoiceHotkeyRouter`) to speak commands. The radio system will still play the roger beep and any
   officer voice replies even without the full terminal UI.

This setup lets you validate CSV data and radio/audio behavior quickly before layering on the complete HUD and terminal windows.

## Trial scene with UI PTT and audio replies (quick hands-on)
Follow these steps to hear a dispatcher→unit exchange with your own button and WAV clips:

1. **Create a new scene and GameObjects**
   - Add an empty **Dispatcher** object and attach `GameManager`, `CallManager`, `UnitManager`, `RadioSystem`, `RecordsManager`, `VoiceCommandController`, and `VoiceInputManager`.
   - Add a **PTTButton** UI Button (or Image + `EventTrigger`). Set its **OnPointerDown** to `UIManager.OnPttPressed` and **OnPointerUp/PointerExit** to `UIManager.OnPttReleased` (or wire directly to `VoiceInputManager.BeginListening/EndListening` if you prefer a minimal stack).
   - Add `UIRadioPanel` if you want to see the transcript/listening indicator; otherwise the radio beeps still fire without the panel.

2. **Attach content & audio**
   - Drag a `UnitRoster` (or `UnitDefinition` assets) into `UnitManager` and a `CallLibrary` into `CallManager`. If you rely on CSV records, assign your sheets to `RecordsManager.csvSheets` or to a `RecordsDatabase`’s `csvFiles` list.
   - Assign a `RadioAudioProfile` on `RadioSystem` that contains `transmissionStart`, `transmissionEnd` (roger beep), and optional `staticLoop` clips. The audio folder structure can follow:
     ```
     Assets/Audio/OfficerVoices/
       Adam-7/dispatch_01.wav, status_01.wav...
       Bravo-12/dispatch_01.wav...
       Static/radio_static.wav, transmission_start.wav, transmission_end.wav
     ```
   - For officer-specific replies, assign a `UnitVoiceProfile` to each unit and fill its reply buckets. For generic fallbacks, create a `ResponseAudioLibrary` asset and add keyword → clip variations (e.g., Acknowledgement → ["10-4.wav", "Copy.wav"]).

3. **Hook the radio + voice pipeline**
   - Link `VoiceCommandController` to the managers, `RadioSystem`, and `RecordsManager`; set the active `VoiceLanguageProfile` in the controller so dispatcher keywords like “Unit 7 respond…”, “status check”, “run plate” are parsed correctly.
   - Ensure `UIManager` references the radio, call/unit panels (optional), and voice input/command controllers. The PTT button now drives listening → interpretation → execution → officer audio + roger beep.

4. **Press Play and test**
   - Hold the PTT button, speak a command (e.g., “Unit Adam-7 respond to 123 Main for a 211”), then release. The system will log the speech, dispatch the unit, play its voice reply from `UnitVoiceProfile`/`ResponseAudioLibrary`, and finish with the roger beep.
   - Ask, “Unit Adam-7, what’s your status?” to hear one of the status reply variants. Run a lookup (“Run plate 7XKZ991”) to hear the dispatcher’s record response and the unit acknowledgement.

If you want desktop testing without UI, add `VoiceHotkeyRouter` to the scene and use the default hotkey (Right Alt) to simulate PTT; it sends queued phrases or speech-provider output into the same pipeline.

## Sample scene recipe (hear chatter and dispatcher/unit answers quickly)
Use `SampleScenarioBuilder` for a barebones scene that still exercises calls, unit replies, records lookups, and chatter:

1. Create a new empty scene and add an empty GameObject named **SampleScenario**.
2. Add these components to the object and wire references:
   - `SampleScenarioBuilder` (Simulation) – assign `UnitManager`, `CallManager`, `RadioSystem`, `ChatterManager`, and `RecordsManager` from the scene.
   - Content assets: a `UnitRoster`, `CallLibrary`, optional `ChatterLibrary`, optional `RecordsDatabase`, and any CSV/TSV sheets you want to merge.
   - Audio: a `RadioAudioProfile` with your roger beep and any shared reply clips so transmissions sound live.
3. Press Play. The builder will:
   - Load the roster/templates/records into their managers.
   - Begin the shift (if a `GameManager` is linked).
   - Spawn the first call from your `CallLibrary`, dispatch the first available unit, and log the radio traffic.
   - Play a chatter line after a short delay, wrapping it with the radio begin/end (roger) tones so you can hear unit → dispatch → unit flow.

This sample scene runs without the terminal UI, letting you vet audio and voice interactions quickly. Toggle `autoRun` off on the builder if you want to trigger `BuildScenario` and `RunSampleSequence` manually from a button or debug console.

## Script dependency map (who depends on whom)
- **GameManager** → orchestrates **CallManager**, **UnitManager**, **ScoringSystem**, **RadioSystem**, **RecordsManager** and starts/stops shifts.
- **CallManager** → owns `CallData` and uses **RadioSystem** (logs), **ScoringSystem** (events), optional **UnitManager** (assignment checks).
- **UnitManager** → owns `Unit`/`UnitDefinition`/`UnitRoster`, uses **RadioSystem** to announce changes, optional **RecordsManager** for lookup contexts.
- **RecordsManager** → loads `RecordsDatabase` + CSV/TSV sheets, serves lookup results to **VoiceCommandController**, **ChatterManager**, **RecordsTerminalPanel**.
- **VoiceInputManager** → raises recognition events; driven by **UIManager** PTT or **VoiceHotkeyRouter**.
- **VoiceCommandController** → listens to **VoiceInputManager**, parses via **CommandInterpreter** (needs `VoiceLanguageProfile`, `CodeLibrary`), executes via **CommandExecutor** (needs **UnitManager**, **CallManager**, **RadioSystem**, **RecordsManager**).
- **RadioSystem** → plays click/static/roger and officer responses from `UnitVoiceProfile` or `ResponseAudioLibrary`; subscribed to by UI panels for transcript updates.
- **UIManager** → references **UIRadioPanel**, **UITerminalPanel**, **UIUnitsPanel**, **UnitTerminalController**, and hooks into **VoiceInputManager** / **VoiceCommandController** / **RadioSystem** for PTT and refreshes.
- **ChatterManager** → uses **RadioSystem**, **RecordsManager**, **UnitManager** to play ambient chatter and dispatcher replies.
- **TutorialManager** → subscribes to **VoiceCommandController** and **CallManager** to gate tutorial steps.
- **SampleScenarioBuilder / DispatchSimulationController** → glue managers, rosters, call libraries, chatter, and radio/voice assets for quick test scenes.

Keep this map handy when wiring new scenes: every dependency above must be assigned in the Inspector to avoid null-reference errors at play time.
