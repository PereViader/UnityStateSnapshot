---
name: unity-cli
description: Use it to run Unity EditMode or PlayMode tests, trigger a Unity AssetDatabase refresh and recompilation, force a full C# recompilation, inspect Unity compiler warnings or errors printed to the terminal, debug failed Unity tests, keep a background Unity instance warm for faster repeated runs, check/stop/wait for that background instance, or execute a Unity static method with optional primitive or JSON object parameters and terminal-returned results.
---

# Unity CLI

## Overview

`unitycli.sh` allows interacting with Unity3d to refresh the AssetDatabase before test/method work, surface compilation diagnostics in the terminal, print failed test details.

On windows, run it using git bash.

Run commands with the current working directory set to the root of the unity project so it can find `ProjectSettings`, `Temp`, `UnityLockFile` and other Unity specific files.

When running in a sandboxed environment, the CLI needs local network permission to connect to the UnityCliRunner TCP socket on 127.0.0.1. Request or grant that network permission before running.

Command executions should not be timed out. Agents should wait for the command to finish or for the user to interrupt it manually.

## Refresh Workflow

Use `refresh` whenever the task needs Unity to import pending asset/script changes, wait for compilation to finish, and print compiler diagnostics without running tests or a custom method.

Use `refresh` after Unity C# or asset changes when compilation status matters but tests are unnecessary. Use `test` after test changes when failed-test details are needed. 

```bash
bash ./unitycli.sh refresh
```

When Unity is already running for this project, the wrapper connects to the UnityCliRunner socket, clears the active editor console, triggers `AssetDatabase.Refresh()`, waits for refresh/compilation/domain reloads to settle, then prints compiler warnings and errors captured from the Unity console.

When Unity is not running, the wrapper automatically starts a background Unity instance in batchmode first, and then executes the refresh command over TCP.

Treat `refresh` as a compile probe:

- Compiler warnings are printed and the command succeeds.
- Compiler errors are printed and the command exits non-zero.
- If Unity fails before compiler diagnostics are available, the wrapper prints the tail of the Unity refresh log.

## Recompile Workflow

Use `recompile` when you want to force a full C# recompilation (clearing the build cache) to reliably output and fetch all compiler warnings and errors from a clean state.

```bash
bash ./unitycli.sh recompile
```

This clears the compiler cache and forces Unity to rebuild all script assemblies from scratch.

## Test Workflow

Use `test` to run EditMode and/or PlayMode tests and print test results.

```bash
bash ./unitycli.sh test --editmode
bash ./unitycli.sh test --playmode
bash ./unitycli.sh test --editmode --filter "MainMenuIntegrationTests"
```

## Background Unity Instance Workflow

Use `start`, `stop`, or `status` to manage the background Unity instance.

```bash
bash ./unitycli.sh start batchmode
bash ./unitycli.sh status
bash ./unitycli.sh stop
```
