## 1. Role-specific FormalUI content

- [x] 1.1 Rename the opening, checkpoint, and level-introduction arrays to serialization-compatible Human arrays, and add new Dog arrays without copying Human content.
- [x] 1.2 Select the current controlled role's valid array for each internal FormalUI tutorial request, preserve missing-content diagnostics, and leave explicit page-array calls unchanged.

## 2. FormalUI configuration and verification

- [x] 2.1 Update the FormalUI builder to seed the renamed Human arrays from the current shared tutorial art and leave Dog arrays empty.
- [x] 2.2 Verify compilation, page navigation, human/dog selection at popup-open time, retained Human assignments, missing-Dog diagnostics, and notice-board page presentation.
