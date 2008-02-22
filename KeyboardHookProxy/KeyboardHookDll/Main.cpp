/* hook.c */
#define WIN32_LEAN_AND_MEAN		// Windows �w�b�_�[����g�p����Ă��Ȃ����������O���܂��B
#include <windows.h>
#include "Main.h"

/* �t�b�N�v���V�[�W���Ŏg�p����ϐ��͋��L�������ɂ��� */
/*                                                    */
/* ����� Visual C++ �̏ꍇ��                         */
/* �����J�̃I�v�V������ /SECTION:.share,RWS ��ǉ�    */
#pragma comment(linker, "/section:.share,rws")
#pragma data_seg(".share")
HHOOK _hHook = NULL;
HWND  _hWnd  = NULL;
#pragma data_seg()

/* DLL �̃C���X�^���X �n���h�� */
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

// �t�b�N �v���V�[�W��
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

// �O���[�o���t�b�N�̃Z�b�g
BOOL SetHook(HWND hWnd)
{
	// �L�[�̃t�b�N
	_hHook = SetWindowsHookEx(WH_KEYBOARD, (HOOKPROC)HookProc, _hInstance, 0);
	if(_hHook != NULL){
		// ������ hInstance �� DLL �̃C���X�^���X �n���h��
		_hWnd = hWnd;
		return TRUE;
	}
	return FALSE;
}

// �O���[�o���t�b�N�̉���
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
