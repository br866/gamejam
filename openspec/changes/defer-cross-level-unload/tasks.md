## 1. Transition Lifecycle

- [x] 1.1 Add route-edge shared-door lookup and open-before-load flow behavior.
- [x] 1.2 Keep the predecessor loaded when the successor checkpoint confirms arrival.
- [x] 1.3 Close the shared transition door and unload the predecessor during successor restart.

## 2. Key And Checkpoint Integration

- [x] 2.1 Update the Level01 key to initiate the shared-door transition through the flow controller.
- [x] 2.2 Preserve checkpoint activation as arrival confirmation without unloading.

## 3. Verification

- [x] 3.1 Add regression coverage for key pickup, door opening, successor loading, delayed unload, and restart cleanup.
- [x] 3.2 Run the complete formal traversal test suite and inspect the Unity Console.
