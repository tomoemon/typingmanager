using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace TypingManager
{
    public class ProcessWindowName
    {
        const int MAX_TITLE_LEN = 512;
        const int MAX_MODULE_LEN = 256;

        // OpenProcess�֐��ŃA�N�Z�X�t���O��ݒ肷�邽�߂̒萔
        const int SYNCHRONIZE = 0x00100000;
        const int STANDARD_RIGHTS_REQUIRED = 0x000F0000;
        const int PROCESS_TERMINATE = 0x0001;
        const int PROCESS_CREATE_THREAD = 0x0002;
        const int PROCESS_SET_SESSIONID = 0x0004;
        const int PROCESS_VM_OPERATION = 0x0008;
        const int PROCESS_VM_READ = 0x0010;
        const int PROCESS_VM_WRITE = 0x0020;
        const int PROCESS_DUP_HANDLE = 0x0040;
        const int PROCESS_CREATE_PROCESS = 0x0080;
        const int PROCESS_SET_QUOTA = 0x0100;
        const int PROCESS_SET_INFORMATION = 0x0200;
        const int PROCESS_QUERY_INFORMATION = 0x0400;
        const int PROCESS_ALL_ACCESS = STANDARD_RIGHTS_REQUIRED | SYNCHRONIZE | 0xFFF;

        [DllImport("user32.dll")]
        extern static IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        extern static int GetWindowText(IntPtr hWnd, StringBuilder lpStr, int nMaxCount);

        [DllImport("user32.dll")]
        extern static int GetWindowThreadProcessId(IntPtr hWnd, ref IntPtr ProcessId);

        [DllImport("psapi.dll")]
        extern static int GetModuleFileNameEx(IntPtr hProcess, IntPtr hModule, StringBuilder lpStr, int nSize);

        /// <summary>
        /// ���̌Ăяo���̌��ʁA�v���Z�X�̍ŏ��̃��W���[���̃n���h���� hModule �ϐ��Ɋi�[����܂��B
        /// ���ۂɂ̓v���Z�X�ɂ͖��O������܂��񂪁A�v���Z�X���̍ŏ��̃��W���[���͂��̃v���Z�X��
        /// ���s�\���W���[���ɂȂ邱�Ƃ��o���Ă����Ă��������B����ŁA�Ԃ��ꂽ���W���[����
        /// �n���h�� (hModule) �� GetModuleFileNameEx() API �܂��� GetModuleBaseName() API �Ŏg�p���āA
        /// �v���Z�X�̎��s�\���W���[���̃t���p�X���܂��͊ȒP�ȃ��W���[�������擾�ł��܂��B
        /// �ǂ���̊֐��ł��A�v���Z�X�ւ̃n���h���A���W���[���ւ̃n���h���A���O���Ԃ����
        /// �o�b�t�@�ւ̃|�C���^�A�o�b�t�@�̃T�C�Y���p�����[�^�Ƃ��Ďg�p����܂��B 
        /// </summary>
        /// <param name="hProcess"></param>
        /// <param name="hModule"></param>
        /// <param name="size"></param>
        /// <param name="cbReturn"></param>
        /// <returns></returns>
        [DllImport("psapi.dll")]
        extern static bool EnumProcessModules(IntPtr hProcess, ref IntPtr hModule, int size, ref int cbReturn);

        [DllImport("kernel32.dll")]
        extern static IntPtr OpenProcess(int dwDesireAccess, bool bInheritHandle, int dwProcessId);

        [DllImport("kernel32.dll")]
        extern static bool CloseHandle(IntPtr hObject);

        static StringBuilder title_text = new StringBuilder(MAX_TITLE_LEN);
        static StringBuilder mod_path = new StringBuilder(MAX_MODULE_LEN);

        /// <summary>
        /// �őO�ʂɂ���E�B���h�E�̃^�C�g����������擾
        /// </summary>
        /// <returns></returns>
        public static string GetFrontWindowTitle()
        {
            // �őO�ʃE�B���h�E�� hwnd ���擾
            IntPtr hwnd = GetForegroundWindow();

            // �^�C�g���o�[��������擾
            GetWindowText(hwnd, title_text, title_text.Capacity);

            return title_text.ToString();
        }

        /// <summary>
        /// �őO�ʂɂ���E�B���h�E�𐶐������v���Z�X�̃t���p�X���擾
        /// </summary>
        /// <returns></returns>
        public static string GetFrontProcessName()
        {
            IntPtr hwnd = GetForegroundWindow();
            IntPtr proc_id = IntPtr.Zero;

            // �A�N�e�B�u�E�B���h�E�̃n���h������v���Z�XID���擾
            GetWindowThreadProcessId(hwnd, ref proc_id);

            // �v���Z�XID����v���Z�X�n���h�����擾
            IntPtr proc_handle = OpenProcess(PROCESS_QUERY_INFORMATION | PROCESS_VM_READ,
                false, proc_id.ToInt32());

            if (proc_handle != IntPtr.Zero)
            {
                // �v���Z�X�̎��s���W���[���n���h�����擾
                IntPtr mod_handle = IntPtr.Zero;
                int return_size = 0;
                bool bModules = EnumProcessModules(proc_handle, ref mod_handle,
                    Marshal.SizeOf(mod_handle), ref return_size);

                if (bModules)
                {
                    // ���s���W���[���̃t���p�X���擾
                    int path_len = GetModuleFileNameEx(proc_handle, mod_handle, mod_path, mod_path.Capacity);
                    /*
                    Debug.Write("ProcID=" + proc_id.ToString() + ", ");
                    Debug.Write("ProcHandle=" + proc_handle.ToString() + ", ");
                    Debug.Write("ModHandle=" + mod_handle.ToString() + ", ");
                    Debug.Write("ModName=" + mod_path.ToString() + "\n");
                    */
                    CloseHandle(proc_handle);
                    return mod_path.ToString();
                }
            }
            return "";
        }
    }
}
