#define WIN32_LEAN_AND_MEAN		// Windows �w�b�_�[����g�p����Ă��Ȃ����������O���܂��B
// Windows �w�b�_�[ �t�@�C��:
#include <windows.h>
#include <tchar.h>
#include "dprintf.h"

// �t�b�N�p�w�b�_�[�t�@�C��
#include "../KeyboardHookDll/Main.h"

typedef BOOL (__stdcall *FUNCTYPE)(UINT, DWORD);
#define MSGFLT_ADD 1
#define MSGFLT_REMOVE 2

static HWND target_window = NULL;

#ifdef _DEBUG
#pragma comment(lib,"../debug/KeyboardHookDll.lib")
#else
#pragma comment(lib,"../release/_proxy.lib")
#endif

/*
unsigned int GetMiliTime()
{
	LARGE_INTEGER nFreq, nCount;
	QueryPerformanceFrequency(&nFreq);
	QueryPerformanceCounter(&nCount);
	return (unsigned int)(nCount.QuadPart * 1000 / nFreq.QuadPart);
}
*/

LRESULT CALLBACK WndProc(HWND hwnd , UINT msg , WPARAM wp , LPARAM lp)
{
	switch (msg)
	{
		case WM_CREATE:
			if(!SetHook(target_window)){
				MessageBox(NULL, L"�t�b�N�Ɏ��s���܂���", L"", MB_OK);
			}
			break;
		case WM_CLOSE:
			ResetHook();
			break;
		case WM_DESTROY:
			PostQuitMessage(0);
			return 0;
	}
	return DefWindowProc(hwnd , msg , wp , lp);
}

int APIENTRY _tWinMain(HINSTANCE hInstance,
                     HINSTANCE hPrevInstance,
                     LPTSTR    lpCmdLine,
                     int       nCmdShow)
{
	bool show_window = false;

	// �R�}���h���C�������̃`�F�b�N
	// [�v���Z�X��] /proxy [�E�B���h�E�^�C�g��]
	// ��Fmain.exe /proxy 124375
	int cmd_length = lstrlen(lpCmdLine);
	if(cmd_length > 0)
	{
		TCHAR cmd[2][256] = {L"", L""};
		int cmd_index = -1;
		int length = 0;
		bool last_space = false;
		bool quote = false;
		dprintf(L"%s\n", lpCmdLine);
		for(int i=0; i < cmd_length && i < 256; i++)
		{
			if(!quote && lpCmdLine[i] == L' '){
				last_space = true;
				continue;
			}
			if(lpCmdLine[i] == L'"'){
				if(!quote){
					length = 0;
					cmd_index++;
				}
				quote = !quote;
				last_space = false;
				continue;
			}
			if(last_space || i==0){
				length = 0;
				cmd_index++;
			}
			if(cmd_index >= 2) break;
			cmd[cmd_index][length++] = lpCmdLine[i];
			last_space = false;
		}
		dprintf(L"%s\n", cmd[0]);
		dprintf(L"%s\n", cmd[1]);
		if(lstrlen(cmd[1]) > 0){
			if(lstrcmp(cmd[0], L"/proxy") == 0 || lstrcmp(cmd[0], L"/proxydebug") == 0)
			{
				if(lstrcmp(cmd[0], L"/proxydebug") == 0)
					show_window = true;
				
				target_window = FindWindow(NULL, cmd[1]);
				if(target_window != NULL)
					goto proxyok;
			}
		}
	}
	MessageBox(NULL, L"���̃v���Z�X�𒼐ڋN�����邱�Ƃ͂ł��܂���", L"�N���G���[", MB_OK);
	return -1;
proxyok:
	/*
	HANDLE hMutex = CreateMutex(NULL, true, L"TypingManagerHookProxy");
	if ( hMutex == NULL ) {
		MessageBox( NULL, L"�~���[�e�b�N�X�̍쐬�Ɏ��s���܂���", L"�m�F", MB_OK );
		return 0;
	}
	DWORD nRet = GetLastError();
	if ( nRet == ERROR_ALREADY_EXISTS ) {
		return 0;
	}
	*/

	FUNCTYPE ChangeWindowMessageFilter;
	HMODULE dll = LoadLibrary(TEXT("user32.dll"));
	ChangeWindowMessageFilter = (FUNCTYPE)GetProcAddress(LoadLibrary(TEXT("user32.dll")) ,
												"ChangeWindowMessageFilter");
	// Vista�ȊO��OS�ł͂��̊֐��͑��݂��Ȃ����C
	// �����������b�Z�[�W�̃t�B���^�����O�Ƃ�����Ȃ��̂Ŗ��Ȃ�
	if(ChangeWindowMessageFilter != NULL){
		ChangeWindowMessageFilter(WM_KEYDOWN, MSGFLT_ADD);
		ChangeWindowMessageFilter(WM_KEYUP, MSGFLT_ADD);
	}
	FreeLibrary(dll);

	HWND hwnd;
	MSG msg;
	WNDCLASS winc;

	winc.style       = CS_HREDRAW | CS_VREDRAW;
	winc.lpfnWndProc = WndProc;
	winc.cbClsExtra  = winc.cbWndExtra = 0;
	winc.hInstance   = hInstance;
	winc.hIcon       = LoadIcon(NULL , IDI_APPLICATION);
	winc.hCursor     = LoadCursor(NULL , IDC_ARROW);
	winc.hbrBackground = (HBRUSH)GetStockObject(WHITE_BRUSH);
	winc.lpszMenuName  = NULL;
	winc.lpszClassName = TEXT("TypingManagerHookProxy");

	if (!RegisterClass(&winc)) return -1;

	hwnd = CreateWindow(
			TEXT("TypingManagerHookProxy") , TEXT("TypingManagerHookProxy") ,
			WS_OVERLAPPEDWINDOW,
			100 , 100 , 200 , 200 , NULL , NULL ,
			hInstance , NULL
	);

	if(hwnd == NULL) return -1;
	if(show_window) ShowWindow(hwnd, SW_SHOW);

	while (GetMessage(&msg , NULL , 0 , 0))
	{
		TranslateMessage(&msg);
		DispatchMessage(&msg);
	}
	
	/*
	if(hMutex){											
		ReleaseMutex(hMutex);							
		CloseHandle(hMutex);							
		hMutex = NULL;
	}
	*/

	return (int)msg.wParam;
}
