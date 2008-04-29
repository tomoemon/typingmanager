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

        [DllImport("user32.dll")]
        private static extern uint GetWindowThreadProcessId(IntPtr hWnd, ref uint procId);

        // �R�[���o�b�N���\�b�h�̃f���Q�[�g
        private delegate int EnumerateWindowsCallback(IntPtr hWnd, int lParam);

        [DllImport("user32", EntryPoint = "EnumWindows")]
        private static extern int EnumWindows(EnumerateWindowsCallback lpEnumFunc, int lParam);

        private static Process target_proc = null;
        private static IntPtr target_hwnd = IntPtr.Zero;

        // �E�B���h�E��񋓂��邽�߂̃R�[���o�b�N���\�b�h
        public static int EnumerateWindows(IntPtr hWnd, int lParam)
        {
            uint procId = 0;
            uint result = GetWindowThreadProcessId(hWnd, ref procId);
            if (procId == target_proc.Id)
            {
                // ����ID�ŕ����̃E�B���h�E��������ꍇ������
                // �Ƃ肠�����ŏ��̃E�B���h�E�������������_�ŏI������
                target_hwnd = hWnd;
                return 0;
            }

            // �񋓂��p������ɂ�0�ȊO��Ԃ��K�v������
            return 1;
        }

        // �O���v���Z�X�̃E�B���h�E���őO�ʂɕ\������
		public static void WakeupWindow(Process target)
		{
            target_proc = target;
            EnumWindows(new EnumerateWindowsCallback(EnumerateWindows), 0);
            if (target_hwnd == IntPtr.Zero)
            {
                return;
            }

			// ���C���E�E�B���h�E���ŏ�������Ă���Ό��ɖ߂�
			if (IsIconic(target_hwnd))
			{
				ShowWindowAsync(target_hwnd, SW_RESTORE);
			}

			// ���C���E�E�B���h�E���őO�ʂɕ\������
			SetForegroundWindow(target_hwnd);
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
