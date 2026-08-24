# Shared Model Prefab Extraction Report

A model is a GameObject with a MeshFilter or SkinnedMeshRenderer. Existing prefab-source objects are retained; non-model objects are excluded. Candidate roots are the highest modeled transforms in a hierarchy.

## Content Prefabs
- `Assets/MoMing/FormalLevels/Prefabs/L01_Content.prefab`: 98 model roots, 98 existing prefab sources, 0 extractable.
- `Assets/MoMing/FormalLevels/Prefabs/L02_Content.prefab`: 159 model roots, 159 existing prefab sources, 0 extractable.
- `Assets/MoMing/FormalLevels/Prefabs/L03_Content.prefab`: 202 model roots, 202 existing prefab sources, 0 extractable.
- `Assets/MoMing/FormalLevels/Prefabs/L04_Content.prefab`: 226 model roots, 226 existing prefab sources, 0 extractable.
- `Assets/MoMing/FormalLevels/Prefabs/L045_Content.prefab`: 143 model roots, 143 existing prefab sources, 0 extractable.
- `Assets/MoMing/FormalLevels/Prefabs/L05_Content.prefab`: 100 model roots, 100 existing prefab sources, 0 extractable.

## Shared Art Scenes
- `Assets/MoMing/FormalLevels/FormalSharedArt_L01_L02.unity`: 6 model roots, 6 existing prefab sources, 0 extractable.
- `Assets/MoMing/FormalLevels/FormalSharedArt_L02_L03.unity`: 5 model roots, 5 existing prefab sources, 0 extractable.
- `Assets/MoMing/FormalLevels/FormalSharedArt_L03_L04.unity`: 4 model roots, 4 existing prefab sources, 0 extractable.
- `Assets/MoMing/FormalLevels/FormalSharedArt_L04_L045.unity`: 4 model roots, 4 existing prefab sources, 0 extractable.
- `Assets/MoMing/FormalLevels/FormalSharedArt_L045_L05.unity`: 7 model roots, 7 existing prefab sources, 0 extractable.

## Totals
- Model roots: 954
- Existing prefab sources retained: 954
- Extractable model roots: 0
- Non-model objects: excluded by rule.

## Validation
- Editor validation confirmed every formal modeled object has prefab ownership.
- Existing independent prefab sources retained: 13.
- Formal EditMode validation: 13 passed, 0 failed, 0 skipped.
