using System;
using System.IO;
using System.Text;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace TypingManager
{
    public class ShortCut
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="link_path"></param>
        /// <param name="link_to"></param>
        /// <param name="description"></param>
        /// <code>
        ///  ShortCut.Save(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Startup),
        ///                                                 "test.lnk"), "TypingManager.pbm", "test");
        /// </code>
        public static void Save(string link_path, string link_to, string work_dir, string description)
        {
            // �����N�悪��΃p�X�w�肶��Ȃ��ꍇ�͂Ȃ����f�X�N�g�b�v����̑��΃p�X
            // �Ƃ��ăV���[�g�J�b�g���쐬�����̂ŁC�J�����g�f�B���N�g������̑��΃p�X��
            // �C������
            if (!Path.IsPathRooted(link_to))
            {
                link_to = Path.Combine(Environment.CurrentDirectory, link_to);
            }
            // �V���[�g�J�b�g���쐬
            ShellLink shortcut = new ShellLink();
            shortcut.Description = description;
            shortcut.TargetPath = link_to;
            shortcut.WorkingDirectory = work_dir;
            shortcut.DisplayMode = ShellLink.ShellLinkDisplayMode.Normal;

            shortcut.Save(link_path);
            shortcut.Dispose();
        }

        public static string Load(string link_path)
        {
            // �V���[�g�J�b�g��ǂݍ���
            ShellLink shortcut = new ShellLink(link_path);

            //Console.WriteLine("{0}��ǂݍ��݂܂��B", shortcut.CurrentFile);
            //Console.WriteLine("�^�[�Q�b�g: {0}", shortcut.TargetPath);
            //Console.WriteLine("����: {0}", shortcut.Description);
            shortcut.Dispose();

            return shortcut.TargetPath;
        }
    }

    #region "COM Interop"

    /// <summary>
    /// ShellLink �R�N���X 
    /// </summary>
    [ComImport]
    [Guid("00021401-0000-0000-C000-000000000046")]
    [ClassInterface(ClassInterfaceType.None)]
    internal class ShellLinkObject { }

    #region "Unicode���p"

    /// <summary>
    /// IShellLinkW�C���^�[�t�F�C�X
    /// </summary>
    [ComImport]
    [Guid("000214F9-0000-0000-C000-000000000046")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IShellLinkW
    {
        void GetPath
            (
            [Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszFile,
            int cch,
            [MarshalAs(UnmanagedType.Struct)] ref WIN32_FIND_DATAW pfd,
            uint fFlags
            );

        void GetIDList(out IntPtr ppidl);

        void SetIDList(IntPtr pidl);

        void GetDescription([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszName, int cch);

        void SetDescription([MarshalAs(UnmanagedType.LPWStr)] string pszName);

        void GetWorkingDirectory([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszDir, int cch);

        void SetWorkingDirectory([MarshalAs(UnmanagedType.LPWStr)] string pszDir);

        void GetArguments([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszArgs, int cch);

        void SetArguments([MarshalAs(UnmanagedType.LPWStr)] string pszArgs);

        void GetHotkey(out ushort pwHotkey);

        void SetHotkey(ushort wHotkey);

        void GetShowCmd(out int piShowCmd);

        void SetShowCmd(int iShowCmd);

        void GetIconLocation
            (
            [Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszIconPath,
            int cch,
            out int piIcon
            );

        void SetIconLocation
            (
            [MarshalAs(UnmanagedType.LPWStr)] string pszIconPath,
            int iIcon
            );

        void SetRelativePath
            (
            [MarshalAs(UnmanagedType.LPWStr)] string pszPathRel,
            uint dwReserved
            );

        void Resolve
            (
            IntPtr hwnd,
            uint fFlags
            );

        void SetPath([MarshalAs(UnmanagedType.LPWStr)] string pszFile);
    }

    /// <summary>
    /// WIN32_FIND_DATAW �\����
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 4, CharSet = CharSet.Unicode)]
    internal struct WIN32_FIND_DATAW
    {
        public const int MAX_PATH = 260;

        public uint dwFileAttributes;
        public System.Runtime.InteropServices.ComTypes.FILETIME ftCreationTime;
        public System.Runtime.InteropServices.ComTypes.FILETIME ftLastAccessTime;
        public System.Runtime.InteropServices.ComTypes.FILETIME ftLastWriteTime;
        public uint nFileSizeHigh;
        public uint nFileSizeLow;
        public uint dwReserved0;
        public uint dwReserved1;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = MAX_PATH)]
        public string cFileName;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 14)]
        public string cAlternateFileName;
    }

    #endregion

    #region "ANSI���p"

    /// <summary>
    /// IShellLinkA�C���^�[�t�F�C�X
    /// </summary>
    [ComImport]
    [Guid("000214EE-0000-0000-C000-000000000046")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IShellLinkA
    {
        void GetPath
            (
            [Out, MarshalAs(UnmanagedType.LPStr)] StringBuilder pszFile,
            int cch,
            [MarshalAs(UnmanagedType.Struct)] ref WIN32_FIND_DATAA pfd,
            uint fFlags
            );

        void GetIDList(out IntPtr ppidl);

        void SetIDList(IntPtr pidl);

        void GetDescription([Out, MarshalAs(UnmanagedType.LPStr)] StringBuilder pszName, int cch);

        void SetDescription([MarshalAs(UnmanagedType.LPStr)] string pszName);

        void GetWorkingDirectory([Out, MarshalAs(UnmanagedType.LPStr)] StringBuilder pszDir, int cch);

        void SetWorkingDirectory([MarshalAs(UnmanagedType.LPStr)] string pszDir);

        void GetArguments([Out, MarshalAs(UnmanagedType.LPStr)] StringBuilder pszArgs, int cch);

        void SetArguments([MarshalAs(UnmanagedType.LPStr)] string pszArgs);

        void GetHotkey(out ushort pwHotkey);

        void SetHotkey(ushort wHotkey);

        void GetShowCmd(out int piShowCmd);

        void SetShowCmd(int iShowCmd);

        void GetIconLocation
            (
            [Out, MarshalAs(UnmanagedType.LPStr)] StringBuilder pszIconPath,
            int cch,
            out int piIcon
            );

        void SetIconLocation
            (
            [MarshalAs(UnmanagedType.LPStr)] string pszIconPath,
            int iIcon
            );

        void SetRelativePath
            (
            [MarshalAs(UnmanagedType.LPStr)] string pszPathRel,
            uint dwReserved
            );

        void Resolve
            (
            IntPtr hwnd,
            uint fFlags
            );

        void SetPath([MarshalAs(UnmanagedType.LPStr)] string pszFile);
    }

    /// <summary>
    /// WIN32_FIND_DATAA �\����
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 4, CharSet = CharSet.Ansi)]
    internal struct WIN32_FIND_DATAA
    {
        public const int MAX_PATH = 260;

        public uint dwFileAttributes;
        public System.Runtime.InteropServices.ComTypes.FILETIME ftCreationTime;
        public System.Runtime.InteropServices.ComTypes.FILETIME ftLastAccessTime;
        public System.Runtime.InteropServices.ComTypes.FILETIME ftLastWriteTime;
        public uint nFileSizeHigh;
        public uint nFileSizeLow;
        public uint dwReserved0;
        public uint dwReserved1;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = MAX_PATH)]
        public string cFileName;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 14)]
        public string cAlternateFileName;
    }

    #endregion

    #endregion

    /// <summary>
    /// �V���[�g�J�b�g�Ɋւ��鏈�����s�����߂̃N���X�ł��B
    /// </summary>
    public sealed class ShellLink : IDisposable
    {
        // IShellLink�C���^�[�t�F�C�X
        private IShellLinkW shellLinkW;
        private IShellLinkA shellLinkA;

        // �J�����g�t�@�C��
        private string currentFile;

        // ���s��
        private bool isUnicodeEnvironment;

        // �e��萔
        internal const int MAX_PATH = 260;

        internal const uint SLGP_SHORTPATH = 0x0001; // �Z���`��(8.3�`��)�̃t�@�C�������擾����
        internal const uint SLGP_UNCPRIORITY = 0x0002; // UNC�p�X�����擾����
        internal const uint SLGP_RAWPATH = 0x0004; // ���ϐ��Ȃǂ��ϊ�����Ă��Ȃ��p�X�����擾����

        #region "[�^] ShellLinkDisplayMode�񋓌^"

        /// <summary>
        /// ���s���̃E�B���h�E�̕\�����@��\���񋓌^�ł��B
        /// </summary>
        public enum ShellLinkDisplayMode : int
        {
            /// <summary>�ʏ�̑傫���̃E�B���h�E�ŋN�����܂��B</summary>
            Normal = 1,

            /// <summary>�ő剻���ꂽ��ԂŋN�����܂��B</summary>
            Maximized = 3,

            /// <summary>�ŏ������ꂽ��ԂŋN�����܂��B</summary>
            Minimized = 7,
        }

        #endregion

        #region "[�^] ShellLinkResolveFlags�񋓌^"

        /// <summary></summary>
        [Flags]
        public enum ShellLinkResolveFlags : int
        {
            /// <summary></summary>
            SLR_ANY_MATCH = 0x2,

            /// <summary></summary>
            SLR_INVOKE_MSI = 0x80,

            /// <summary></summary>
            SLR_NOLINKINFO = 0x40,

            /// <summary></summary>
            SLR_NO_UI = 0x1,

            /// <summary></summary>
            SLR_NO_UI_WITH_MSG_PUMP = 0x101,

            /// <summary></summary>
            SLR_NOUPDATE = 0x8,

            /// <summary></summary>
            SLR_NOSEARCH = 0x10,

            /// <summary></summary>
            SLR_NOTRACK = 0x20,

            /// <summary></summary>
            SLR_UPDATE = 0x4
        }

        #endregion

        #region "�R���X�g���N�V�����E�f�X�g���N�V����"

        /// <summary>
        /// �R���X�g���N�^
        /// </summary>
        /// <exception cref="COMException">IShellLink�C���^�[�t�F�C�X���擾�ł��܂���ł����B</exception>
        public ShellLink()
        {
            currentFile = "";

            shellLinkW = null;
            shellLinkA = null;

            try
            {
                if (Environment.OSVersion.Platform == PlatformID.Win32NT)
                {
                    // Unicode��
                    shellLinkW = (IShellLinkW)(new ShellLinkObject());

                    isUnicodeEnvironment = true;
                }
                else
                {
                    // Ansi��
                    shellLinkA = (IShellLinkA)(new ShellLinkObject());

                    isUnicodeEnvironment = false;
                }
            }
            catch
            {
                throw new COMException("IShellLink�C���^�[�t�F�C�X���擾�ł��܂���ł����B");
            }
        }

        /// <summary>
        /// �R���X�g���N�^
        /// </summary>
        /// <param name="linkFile">�V���[�g�J�b�g�t�@�C��</param>
        public ShellLink(string linkFile)
            : this()
        {
            Load(linkFile);
        }

        /// <summary>
        /// �f�X�g���N�^
        /// </summary>
        ~ShellLink()
        {
            Dispose();
        }

        /// <summary>
        /// ���̃C���X�^���X���g�p���Ă��郊�\�[�X��������܂��B
        /// </summary>
        public void Dispose()
        {
            if (shellLinkW != null)
            {
                Marshal.ReleaseComObject(shellLinkW);
                shellLinkW = null;
            }

            if (shellLinkA != null)
            {
                Marshal.ReleaseComObject(shellLinkA);
                shellLinkA = null;
            }
        }

        #endregion

        #region "�v���p�e�B"

        /// <summary>
        /// �J�����g�t�@�C���B
        /// </summary>
        public string CurrentFile
        {
            get { return currentFile; }
        }

        /// <summary>
        /// �V���[�g�J�b�g�̃����N��B
        /// </summary>
        public string TargetPath
        {
            get
            {
                StringBuilder targetPath = new StringBuilder(MAX_PATH, MAX_PATH);

                if (isUnicodeEnvironment)
                {
                    WIN32_FIND_DATAW data = new WIN32_FIND_DATAW();

                    shellLinkW.GetPath(targetPath, targetPath.Capacity, ref data, SLGP_UNCPRIORITY);
                }
                else
                {
                    WIN32_FIND_DATAA data = new WIN32_FIND_DATAA();

                    shellLinkA.GetPath(targetPath, targetPath.Capacity, ref data, SLGP_UNCPRIORITY);
                }

                return targetPath.ToString();
            }
            set
            {
                if (isUnicodeEnvironment)
                {
                    shellLinkW.SetPath(value);
                }
                else
                {
                    shellLinkA.SetPath(value);
                }
            }
        }

        /// <summary>
        /// ��ƃf�B���N�g���B
        /// </summary>
        public string WorkingDirectory
        {
            get
            {
                StringBuilder workingDirectory = new StringBuilder(MAX_PATH, MAX_PATH);

                if (isUnicodeEnvironment)
                {
                    shellLinkW.GetWorkingDirectory(workingDirectory, workingDirectory.Capacity);
                }
                else
                {
                    shellLinkA.GetWorkingDirectory(workingDirectory, workingDirectory.Capacity);
                }

                return workingDirectory.ToString();
            }
            set
            {
                if (isUnicodeEnvironment)
                {
                    shellLinkW.SetWorkingDirectory(value);
                }
                else
                {
                    shellLinkA.SetWorkingDirectory(value);
                }
            }
        }

        /// <summary>
        /// �R�}���h���C�������B
        /// </summary>
        public string Arguments
        {
            get
            {
                StringBuilder arguments = new StringBuilder(MAX_PATH, MAX_PATH);

                if (isUnicodeEnvironment)
                {
                    shellLinkW.GetArguments(arguments, arguments.Capacity);
                }
                else
                {
                    shellLinkA.GetArguments(arguments, arguments.Capacity);
                }

                return arguments.ToString();
            }
            set
            {
                if (isUnicodeEnvironment)
                {
                    shellLinkW.SetArguments(value);
                }
                else
                {
                    shellLinkA.SetArguments(value);
                }
            }
        }

        /// <summary>
        /// �V���[�g�J�b�g�̐����B
        /// </summary>
        public string Description
        {
            get
            {
                StringBuilder description = new StringBuilder(MAX_PATH, MAX_PATH);

                if (isUnicodeEnvironment)
                {
                    shellLinkW.GetDescription(description, description.Capacity);
                }
                else
                {
                    shellLinkA.GetDescription(description, description.Capacity);
                }

                return description.ToString();
            }
            set
            {
                if (isUnicodeEnvironment)
                {
                    shellLinkW.SetDescription(value);
                }
                else
                {
                    shellLinkA.SetDescription(value);
                }
            }
        }

        /// <summary>
        /// �A�C�R���̃t�@�C���B
        /// </summary>
        public string IconFile
        {
            get
            {
                int iconIndex = 0;
                string iconFile = "";

                GetIconLocation(out iconFile, out iconIndex);

                return iconFile;
            }
            set
            {
                int iconIndex = 0;
                string iconFile = "";

                GetIconLocation(out iconFile, out iconIndex);

                SetIconLocation(value, iconIndex);
            }
        }

        /// <summary>
        /// �A�C�R���̃C���f�b�N�X�B
        /// </summary>
        public int IconIndex
        {
            get
            {
                int iconIndex = 0;
                string iconPath = "";

                GetIconLocation(out iconPath, out iconIndex);

                return iconIndex;
            }
            set
            {
                int iconIndex = 0;
                string iconPath = "";

                GetIconLocation(out iconPath, out iconIndex);

                SetIconLocation(iconPath, value);
            }
        }

        /// <summary>
        /// �A�C�R���̃t�@�C���ƃC���f�b�N�X���擾����
        /// </summary>
        /// <param name="iconFile">�A�C�R���̃t�@�C��</param>
        /// <param name="iconIndex">�A�C�R���̃C���f�b�N�X</param>
        private void GetIconLocation(out string iconFile, out int iconIndex)
        {
            StringBuilder iconFileBuffer = new StringBuilder(MAX_PATH, MAX_PATH);

            if (isUnicodeEnvironment)
            {
                shellLinkW.GetIconLocation(iconFileBuffer, iconFileBuffer.Capacity, out iconIndex);
            }
            else
            {
                shellLinkA.GetIconLocation(iconFileBuffer, iconFileBuffer.Capacity, out iconIndex);
            }

            iconFile = iconFileBuffer.ToString();
        }

        /// <summary>
        /// �A�C�R���̃t�@�C���ƃC���f�b�N�X��ݒ肷��
        /// </summary>
        /// <param name="iconFile">�A�C�R���̃t�@�C��</param>
        /// <param name="iconIndex">�A�C�R���̃C���f�b�N�X</param>
        private void SetIconLocation(string iconFile, int iconIndex)
        {
            if (isUnicodeEnvironment)
            {
                shellLinkW.SetIconLocation(iconFile, iconIndex);
            }
            else
            {
                shellLinkA.SetIconLocation(iconFile, iconIndex);
            }
        }

        /// <summary>
        /// ���s���̃E�B���h�E�̑傫���B
        /// </summary>
        public ShellLinkDisplayMode DisplayMode
        {
            get
            {
                int showCmd = 0;

                if (isUnicodeEnvironment)
                {
                    shellLinkW.GetShowCmd(out showCmd);
                }
                else
                {
                    shellLinkA.GetShowCmd(out showCmd);
                }

                return (ShellLinkDisplayMode)showCmd;
            }
            set
            {
                if (isUnicodeEnvironment)
                {
                    shellLinkW.SetShowCmd((int)value);
                }
                else
                {
                    shellLinkA.SetShowCmd((int)value);
                }
            }
        }

        /// <summary>
        /// �z�b�g�L�[�B
        /// </summary>
        public Keys HotKey
        {
            get
            {
                ushort hotKey = 0;

                if (isUnicodeEnvironment)
                {
                    shellLinkW.GetHotkey(out hotKey);
                }
                else
                {
                    shellLinkA.GetHotkey(out hotKey);
                }

                return (Keys)hotKey;
            }
            set
            {
                if (isUnicodeEnvironment)
                {
                    shellLinkW.SetHotkey((ushort)value);
                }
                else
                {
                    shellLinkA.SetHotkey((ushort)value);
                }
            }
        }

        #endregion

        #region "�ۑ��Ɠǂݍ���"

        /// <summary>
        /// IShellLink�C���^�[�t�F�C�X����L���X�g���ꂽIPersistFile�C���^�[�t�F�C�X���擾���܂��B
        /// </summary>
        /// <returns>IPersistFile�C���^�[�t�F�C�X�B�@�擾�ł��Ȃ������ꍇ��null�B</returns>
        private System.Runtime.InteropServices.ComTypes.IPersistFile GetIPersistFile()
        {
            if (isUnicodeEnvironment)
            {
                return shellLinkW as System.Runtime.InteropServices.ComTypes.IPersistFile;
            }
            else
            {
                return shellLinkA as System.Runtime.InteropServices.ComTypes.IPersistFile;
            }
        }

        /// <summary>
        /// �J�����g�t�@�C���ɃV���[�g�J�b�g��ۑ����܂��B
        /// </summary>
        /// <exception cref="COMException">IPersistFile�C���^�[�t�F�C�X���擾�ł��܂���ł����B</exception>
        public void Save()
        {
            Save(currentFile);
        }

        /// <summary>
        /// �w�肵���t�@�C���ɃV���[�g�J�b�g��ۑ����܂��B
        /// </summary>
        /// <param name="linkFile">�V���[�g�J�b�g��ۑ�����t�@�C��</param>
        /// <exception cref="COMException">IPersistFile�C���^�[�t�F�C�X���擾�ł��܂���ł����B</exception>
        public void Save(string linkFile)
        {
            // IPersistFile�C���^�[�t�F�C�X���擾���ĕۑ�
            System.Runtime.InteropServices.ComTypes.IPersistFile persistFile = GetIPersistFile();

            if (persistFile == null) throw new COMException("IPersistFile�C���^�[�t�F�C�X���擾�ł��܂���ł����B");

            persistFile.Save(linkFile, true);

            // �J�����g�t�@�C����ۑ�
            currentFile = linkFile;
        }

        /// <summary>
        /// �w�肵���t�@�C������V���[�g�J�b�g��ǂݍ��݂܂��B
        /// </summary>
        /// <param name="linkFile">�V���[�g�J�b�g��ǂݍ��ރt�@�C��</param>
        /// <exception cref="FileNotFoundException">�t�@�C����������܂���B</exception>
        /// <exception cref="COMException">IPersistFile�C���^�[�t�F�C�X���擾�ł��܂���ł����B</exception>
        public void Load(string linkFile)
        {
            Load(linkFile, IntPtr.Zero, ShellLinkResolveFlags.SLR_ANY_MATCH | ShellLinkResolveFlags.SLR_NO_UI, 1);
        }

        /// <summary>
        /// �w�肵���t�@�C������V���[�g�J�b�g��ǂݍ��݂܂��B
        /// </summary>
        /// <param name="linkFile">�V���[�g�J�b�g��ǂݍ��ރt�@�C��</param>
        /// <param name="hWnd">���̃R�[�h���Ăяo�����I�[�i�[�̃E�B���h�E�n���h��</param>
        /// <param name="resolveFlags">�V���[�g�J�b�g���̉����Ɋւ��铮���\���t���O</param>
        /// <exception cref="FileNotFoundException">�t�@�C����������܂���B</exception>
        /// <exception cref="COMException">IPersistFile�C���^�[�t�F�C�X���擾�ł��܂���ł����B</exception>
        public void Load(string linkFile, IntPtr hWnd, ShellLinkResolveFlags resolveFlags)
        {
            Load(linkFile, hWnd, resolveFlags, 1);
        }

        /// <summary>
        /// �w�肵���t�@�C������V���[�g�J�b�g��ǂݍ��݂܂��B
        /// </summary>
        /// <param name="linkFile">�V���[�g�J�b�g��ǂݍ��ރt�@�C��</param>
        /// <param name="hWnd">���̃R�[�h���Ăяo�����I�[�i�[�̃E�B���h�E�n���h��</param>
        /// <param name="resolveFlags">�V���[�g�J�b�g���̉����Ɋւ��铮���\���t���O</param>
        /// <param name="timeOut">SLR_NO_UI���w�肵���Ƃ��̃^�C���A�E�g�l(�~���b)</param>
        /// <exception cref="FileNotFoundException">�t�@�C����������܂���B</exception>
        /// <exception cref="COMException">IPersistFile�C���^�[�t�F�C�X���擾�ł��܂���ł����B</exception>
        public void Load(string linkFile, IntPtr hWnd, ShellLinkResolveFlags resolveFlags, TimeSpan timeOut)
        {
            Load(linkFile, hWnd, resolveFlags, (int)timeOut.TotalMilliseconds);
        }

        /// <summary>
        /// �w�肵���t�@�C������V���[�g�J�b�g��ǂݍ��݂܂��B
        /// </summary>
        /// <param name="linkFile">�V���[�g�J�b�g��ǂݍ��ރt�@�C��</param>
        /// <param name="hWnd">���̃R�[�h���Ăяo�����I�[�i�[�̃E�B���h�E�n���h��</param>
        /// <param name="resolveFlags">�V���[�g�J�b�g���̉����Ɋւ��铮���\���t���O</param>
        /// <param name="timeOutMilliseconds">SLR_NO_UI���w�肵���Ƃ��̃^�C���A�E�g�l(�~���b)</param>
        /// <exception cref="FileNotFoundException">�t�@�C����������܂���B</exception>
        /// <exception cref="COMException">IPersistFile�C���^�[�t�F�C�X���擾�ł��܂���ł����B</exception>
        public void Load(string linkFile, IntPtr hWnd, ShellLinkResolveFlags resolveFlags, int timeOutMilliseconds)
        {
            if (!File.Exists(linkFile)) throw new FileNotFoundException("�t�@�C����������܂���B", linkFile);

            // IPersistFile�C���^�[�t�F�C�X���擾
            System.Runtime.InteropServices.ComTypes.IPersistFile persistFile = GetIPersistFile();

            if (persistFile == null) throw new COMException("IPersistFile�C���^�[�t�F�C�X���擾�ł��܂���ł����B");

            // �ǂݍ���
            persistFile.Load(linkFile, 0x00000000);

            // �t���O������
            uint flags = (uint)resolveFlags;

            if ((resolveFlags & ShellLinkResolveFlags.SLR_NO_UI) == ShellLinkResolveFlags.SLR_NO_UI)
            {
                flags |= (uint)(timeOutMilliseconds << 16);
            }

            // �V���[�g�J�b�g�Ɋւ������ǂݍ���
            if (isUnicodeEnvironment)
            {
                shellLinkW.Resolve(hWnd, flags);
            }
            else
            {
                shellLinkA.Resolve(hWnd, flags);
            }

            // �J�����g�t�@�C�����w��
            currentFile = linkFile;
        }

        #endregion
    }
}
