/* hook.c */
#define WIN32_LEAN_AND_MEAN		// Windows ヘッダーから使用されていない部分を除外します。
#include <windows.h>
#include "Main.h"

/* フックプロシージャで使用する変数は共有メモリにおく */
/*                                                    */
/* これは Visual C++ の場合で                         */
/* リンカのオプションに /SECTION:.share,RWS を追加    */
#pragma comment(linker, "/section:.share,rws")
#pragma data_seg(".share")
HHOOK _hHook = NULL;
HWND  _hWnd  = NULL;
#pragma data_seg()

/* DLL のインスタンス ハンドル */
static HINSTANCE _hInstance;

BOOL WINAPI DllMain(HINSTANCE hInstDLL, DWORD dwReason, LPVOID lpReserved)
{
    switch (dwReason) {
        case DLL_PROCESS_ATTACH:
            _hInstance = hInstDLL;
            break;
        case DLL_PROCESS_DETACH:
            break;
    }
    return TRUE;
}

// フック プロシージャ
LRESULT CALLBACK HookProc(int nCode, WPARAM wParam, LPARAM lParam)
{
	if(nCode == HC_ACTION){
		if(lParam & 0x80000000){
			//MessageBox(NULL, L"dll up",L"",MB_OK);
			PostMessage(_hWnd, WM_KEYUP, wParam, lParam);
		}
		else{
			//MessageBox(NULL, L"dll down",L"",MB_OK);
			PostMessage(_hWnd, WM_KEYDOWN, wParam, lParam);
		}
	}
	return CallNextHookEx(_hHook, nCode, wParam, lParam);
}

// グローバルフックのセット
BOOL SetHook(HWND hWnd)
{
	// キーのフック
	_hHook = SetWindowsHookEx(WH_KEYBOARD, (HOOKPROC)HookProc, _hInstance, 0);
	if(_hHook != NULL){
		// ここで hInstance は DLL のインスタンス ハンドル
		_hWnd = hWnd;
		return TRUE;
	}
	return FALSE;
}

// グローバルフックの解除
BOOL ResetHook()
{
	if (_hHook != NULL) {
		if(UnhookWindowsHookEx(_hHook) == 0)
		{
			return FALSE;
		}
		else{
			_hHook = NULL;
		}
	}
	return TRUE;
}
