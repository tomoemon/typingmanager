using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;

namespace TypingManager
{
    public class MultipleBootCheck
    {
        // �O���v���Z�X�̃��C���E�E�B���h�E���N�����邽�߂�Win32 API
        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern bool ShowWindowAsync(IntPtr hWnd, int nCmdShow);

        [DllImport("user32.dll")]
        private static extern bool IsIconic(IntPtr hWnd);
		
        // �O���v���Z�X�̃E�B���h�E���őO�ʂɕ\������
		public static void WakeupWindow(IntPtr hWnd) 
		{ 
			// ���C���E�E�B���h�E���ŏ�������Ă���Ό��ɖ߂�
			if (IsIconic(hWnd))
			{
				ShowWindowAsync(hWnd, SW_RESTORE);
			}

			// ���C���E�E�B���h�E���őO�ʂɕ\������
			SetForegroundWindow(hWnd);
		}

        // ShowWindowAsync�֐��̃p�����[�^�ɓn����`�l
		private const int SW_RESTORE = 9;  // ��ʂ����̑傫���ɖ߂�

		/// <summary>
        /// ���s���̃v���Z�X�Ɠ���ProcessName�����v���Z�X���擾����
		/// </summary>
		/// <returns></returns>
		public static Process GetPreviousProcess()
		{
			Process curProcess = Process.GetCurrentProcess(); 
            // ProcessName�͎��s�t�@�C������.exe�𔲂�������
			Process[] allProcesses = Process.GetProcessesByName (curProcess.ProcessName); 

			foreach (Process checkProcess in allProcesses) 
			{ 
				// �������g�̃v���Z�XID�͖�������
				if (checkProcess.Id != curProcess.Id) 
				{
                    string prev = checkProcess.MainModule.FileName;
                    string cur = curProcess.MainModule.FileName;

					// �v���Z�X�̃t���p�X���r���ē����A�v���P�[�V����������
					if (String.Compare(prev, cur, true) == 0)
					{
						// �����t���p�X���̃v���Z�X���擾
						return checkProcess;
					}
				}  
			}

			// �����A�v���P�[�V�����̃v���Z�X��������Ȃ��I  
			return null;  
		}
    }
}
