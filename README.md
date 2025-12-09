# Object Pool Debugger

A lightweight runtime + editor package to debug GameObject pools:
- Live pool overview: active/inactive counts, rates, max observed size
- Per-pool details and timeline
- Hierarchy icons for pooled instances
- Simple example pool & spawner included

## Installation
- Use Unity Package Manager → Add package from Git URL:
<br>https://github.com/draganstamenkovic/object-pool-debugger.git

## Quick test
1. Create a scene.
2. Create an empty GameObject -> add `SimplePool` component.
3. Assign a small cube prefab to `Prefab` and set InitialSize=8.
4. Create another GameObject -> add `ExampleSpawner`, assign the pool.
5. Enter Play Mode.
6. Open Window -> Diagnostics -> Object Pool Debugger.

The window will show the created pool; spawn/despawn activity is tracked and displayed.

## Integration
Call `PoolDebuggerRuntime.Instance.NotifySpawn(poolId, gameObject)` and `NotifyDespawn` in your own pool implementation to report events.
