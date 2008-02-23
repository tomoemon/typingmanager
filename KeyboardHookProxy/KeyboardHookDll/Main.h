#pragma once

extern "C" __declspec(dllexport) LRESULT CALLBACK HookProc(int, WPARAM, LPARAM);
extern "C" __declspec(dllexport) BOOL SetHook(HWND);
extern "C" __declspec(dllexport) BOOL ResetHook();
