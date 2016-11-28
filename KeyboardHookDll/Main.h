#pragma once

extern "C" {
	__declspec(dllexport) LRESULT CALLBACK HookProc(int, WPARAM, LPARAM);
	__declspec(dllexport) BOOL SetHook(HWND);
	__declspec(dllexport) BOOL ResetHook();
}

