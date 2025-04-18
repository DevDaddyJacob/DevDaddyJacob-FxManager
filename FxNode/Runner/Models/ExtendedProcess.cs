using Microsoft.Win32.SafeHandles;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Security;

namespace DevDaddyJacob.FxManager.Node.Runner.Models
{
    /// <summary>
    /// Extension of <see cref="Process"/> with support for FD3 for FiveM usage
    ///
    /// and yes, this is shitty, but I aint got anything better
    /// </summary>
    internal class ExtendedProcess
    {
        #region Fields

        private const uint HANDLE_FLAG_INHERIT = 0x00000001;

        private readonly Process _process;
        private SafeFileHandle _fd3ReadPipe;
        private SafeFileHandle _fd3WritePipe;
        private FileStream? _fd3FileStream;
        private StreamReader? _fd3Reader;

        #endregion Fields

        #region Properties

        public StreamReader? StandardOutput3 
        { 
            get => _fd3Reader; 
        }

        #endregion Properties

        #region Constructors

        /// <inheritdoc cref="Process()"/>
        public ExtendedProcess()
        {
            // Create the wrapped process
            _process = new Process();


            // Attach pipe for stdio[3] (aka File Descriptor 3)
            _process.Disposed += (sender, e) =>
            {
                if (_fd3Reader != null) { _fd3Reader.Dispose(); _fd3Reader = null; }
                if (_fd3FileStream != null) { _fd3FileStream.Dispose(); _fd3FileStream = null; }
            };

            // Create pipe for stdio[3]
            if (!CreatePipe(out _fd3ReadPipe, out _fd3WritePipe, IntPtr.Zero, 0))
            {
                throw new System.ComponentModel.Win32Exception(Marshal.GetLastWin32Error());
            }

            // Make read handle inheritable
            SetHandleInformation(_fd3ReadPipe, HANDLE_FLAG_INHERIT, HANDLE_FLAG_INHERIT);
        }

        #endregion Constructors

        #region Extern Methods

        // Windows API functions
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool CreatePipe(out SafeFileHandle hReadPipe, out SafeFileHandle hWritePipe, IntPtr lpPipeAttributes, int nSize);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool SetHandleInformation(SafeFileHandle hObject, uint dwMask, uint dwFlags);

        #endregion Extern Methods

        #region Methods

        /// <inheritdoc cref="Process.Start()"/>
        [SupportedOSPlatform("maccatalyst")]
        [UnsupportedOSPlatform("ios")]
        [UnsupportedOSPlatform("tvos")]
        public bool Start()
        {
            _process.StartInfo.EnvironmentVariables["FD3"] = _fd3WritePipe.DangerousGetHandle().ToString();

            bool result = _process.Start();

            // Close the write handle in the parent so we can read from it
            _fd3WritePipe.Close();

            _fd3FileStream = new FileStream(_fd3ReadPipe, FileAccess.Read);
            _fd3Reader = new StreamReader(_fd3FileStream);

            return result;
        }

        #endregion Methods

        #region System.Diagnostics.Process Wrapper

        #region Properties Wrappers

        /// <inheritdoc cref="Process.BasePriority"/>
        public int BasePriority
        { 
            get => _process.BasePriority;
        }

        /// <inheritdoc cref="Process.Container"/>
        [Browsable(false)]
        public IContainer? Container 
        { 
            get => _process.Container;
        }

        /// <inheritdoc cref="Process.EnableRaisingEvents"/>
        public bool EnableRaisingEvents
        {
            get => _process.EnableRaisingEvents;
            set => _process.EnableRaisingEvents = value;
        }

        /// <inheritdoc cref="Process.ExitCode"/>
        public int ExitCode
        {
            get => _process.ExitCode;
        }

        /// <inheritdoc cref="Process.ExitTime"/>
        public DateTime ExitTime
        {
            get => _process.ExitTime;
        }

        /// <inheritdoc cref="Process.Handle"/>
        public IntPtr Handle
        {
            get => _process.Handle;
        }

        /// <inheritdoc cref="Process.HandleCount"/>
        public int HandleCount
        {
            get => _process.HandleCount;
        }

        /// <inheritdoc cref="Process.HasExited"/>
        public bool HasExited
        {
            get => _process.HasExited;
        }

        /// <inheritdoc cref="Process.Id"/>
        public int Id
        {
            get => _process.Id;
        }

        /// <inheritdoc cref="Process.MachineName"/>
        public string MachineName
        {
            get => _process.MachineName;
        }

        /// <inheritdoc cref="Process.MainModule"/>
        public System.Diagnostics.ProcessModule? MainModule
        {
            get => _process.MainModule;
        }

        /// <inheritdoc cref="Process.MainWindowHandle"/>
        public IntPtr MainWindowHandle
        {
            get => _process.MainWindowHandle;
        }

        /// <inheritdoc cref="Process.MainWindowTitle"/>
        public string MainWindowTitle
        {
            get => _process.MainWindowTitle;
        }

        /// <inheritdoc cref="Process.MaxWorkingSet"/>
        public IntPtr MaxWorkingSet
        {
            [SupportedOSPlatform("maccatalyst")]
            [UnsupportedOSPlatform("ios")]
            [UnsupportedOSPlatform("tvos")]
            get => _process.MaxWorkingSet;
            [SupportedOSPlatform("freebsd")]
            [SupportedOSPlatform("macos")]
            [SupportedOSPlatform("maccatalyst")]
            [SupportedOSPlatform("windows")]
            set => _process.MaxWorkingSet = value;
        }

        /// <inheritdoc cref="Process.MinWorkingSet"/>
        public IntPtr MinWorkingSet
        {
            [SupportedOSPlatform("maccatalyst")]
            [UnsupportedOSPlatform("ios")]
            [UnsupportedOSPlatform("tvos")]
            get => _process.MinWorkingSet;
            [SupportedOSPlatform("freebsd")]
            [SupportedOSPlatform("macos")]
            [SupportedOSPlatform("maccatalyst")]
            [SupportedOSPlatform("windows")]
            set => _process.MinWorkingSet = value;
        }

        /// <inheritdoc cref="Process.Modules"/>
        public ProcessModuleCollection Modules
        {
            get => _process.Modules;
        }

        /// <inheritdoc cref="Process.NonpagedSystemMemorySize"/>
        [Obsolete("Process.NonpagedSystemMemorySize has been deprecated because the type of the property can't represent all valid results. Use System.Diagnostics.Process.NonpagedSystemMemorySize64 instead.")]
        public int NonpagedSystemMemorySize
        {
            get => _process.NonpagedSystemMemorySize;
        }

        /// <inheritdoc cref="Process.NonpagedSystemMemorySize64"/>
        public long NonpagedSystemMemorySize64
        {
            get => _process.NonpagedSystemMemorySize64;
        }

        /// <inheritdoc cref="Process.PagedMemorySize"/>
        [Obsolete("Process.PagedMemorySize has been deprecated because the type of the property can't represent all valid results. Use System.Diagnostics.Process.PagedMemorySize64 instead.")]
        public int PagedMemorySize
        {
            get => _process.PagedMemorySize;
        }

        /// <inheritdoc cref="Process.PagedMemorySize64"/>
        public long PagedMemorySize64
        {
            get => _process.PagedMemorySize64;
        }

        /// <inheritdoc cref="Process.PagedSystemMemorySize"/>
        [Obsolete("Process.PagedSystemMemorySize has been deprecated because the type of the property can't represent all valid results. Use System.Diagnostics.Process.PagedSystemMemorySize64 instead.")]
        public int PagedSystemMemorySize
        {
            get => _process.PagedSystemMemorySize;
        }

        /// <inheritdoc cref="Process.PagedSystemMemorySize64"/>
        public long PagedSystemMemorySize64
        {
            get => _process.PagedSystemMemorySize64;
        }

        /// <inheritdoc cref="Process.PeakPagedMemorySize"/>
        [Obsolete("Process.PeakPagedMemorySize has been deprecated because the type of the property can't represent all valid results. Use System.Diagnostics.Process.PeakPagedMemorySize64 instead.")]
        public int PeakPagedMemorySize
        {
            get => _process.PeakPagedMemorySize;
        }

        /// <inheritdoc cref="Process.PeakPagedMemorySize64"/>
        public long PeakPagedMemorySize64
        {
            get => _process.PeakPagedMemorySize64;
        }

        /// <inheritdoc cref="Process.PeakVirtualMemorySize"/>
        [Obsolete("Process.PeakVirtualMemorySize has been deprecated because the type of the property can't represent all valid results. Use System.Diagnostics.Process.PeakVirtualMemorySize64 instead.")]
        public int PeakVirtualMemorySize
        {
            get => _process.PeakVirtualMemorySize;
        }

        /// <inheritdoc cref="Process.PeakVirtualMemorySize64"/>
        public long PeakVirtualMemorySize64
        {
            get => _process.PeakVirtualMemorySize64;
        }

        /// <inheritdoc cref="Process.PeakWorkingSet"/>
        [Obsolete("Process.PeakWorkingSet has been deprecated because the type of the property can't represent all valid results. Use System.Diagnostics.Process.PeakWorkingSet64 instead.")]
        public int PeakWorkingSet
        {
            get => _process.PeakWorkingSet;
        }

        /// <inheritdoc cref="Process.PeakWorkingSet64"/>
        public long PeakWorkingSet64
        {
            get => _process.PeakWorkingSet64;
        }

        /// <inheritdoc cref="Process.PriorityBoostEnabled"/>
        public bool PriorityBoostEnabled
        {
            get => _process.PriorityBoostEnabled;
            set => _process.PriorityBoostEnabled = value;
        }

        /// <inheritdoc cref="Process.PriorityClass"/>
        public ProcessPriorityClass PriorityClass
        {
            get => _process.PriorityClass;
            set => _process.PriorityClass = value;
        }

        /// <inheritdoc cref="Process.PrivateMemorySize"/>
        [Obsolete("Process.PrivateMemorySize has been deprecated because the type of the property can't represent all valid results. Use System.Diagnostics.Process.PrivateMemorySize64 instead.")]
        public int PrivateMemorySize
        {
            get => _process.PrivateMemorySize;
        }

        /// <inheritdoc cref="Process.PrivateMemorySize64"/>
        public long PrivateMemorySize64
        {
            get => _process.PrivateMemorySize64;
        }

        /// <inheritdoc cref="Process.PrivilegedProcessorTime"/>
        [SupportedOSPlatform("maccatalyst")]
        [UnsupportedOSPlatform("ios")]
        [UnsupportedOSPlatform("tvos")]
        public TimeSpan PrivilegedProcessorTime
        {
            get => _process.PrivilegedProcessorTime;
        }

        /// <inheritdoc cref="Process.ProcessName"/>
        public string ProcessName
        {
            get => _process.ProcessName;
        }

        /// <inheritdoc cref="Process.ProcessorAffinity"/>
        [SupportedOSPlatform("windows")]
        [SupportedOSPlatform("linux")]
        public IntPtr ProcessorAffinity
        {
            get => _process.ProcessorAffinity;
            set => _process.ProcessorAffinity = value;
        }

        /// <inheritdoc cref="Process.Responding"/>
        public bool Responding
        {
            get => _process.Responding;
        }

        /// <inheritdoc cref="Process.SafeHandle"/>
        public SafeProcessHandle SafeHandle
        {
            get => _process.SafeHandle;
        }

        /// <inheritdoc cref="Process.SessionId"/>
        public int SessionId
        {
            get => _process.SessionId;
        }

        /// <inheritdoc cref="Process.Site"/>
        [Browsable(false)]
        public virtual ISite? Site
        {
            get => _process.Site;
            set => _process.Site = value;
        }

        /// <inheritdoc cref="Process.StandardError"/>
        public StreamReader StandardError
        {
            get => _process.StandardError;
        }

        /// <inheritdoc cref="Process.StandardInput"/>
        public StreamWriter StandardInput
        {
            get => _process.StandardInput;
        }

        /// <inheritdoc cref="Process.StandardOutput"/>
        public StreamReader StandardOutput
        {
            get => _process.StandardOutput;
        }

        /// <inheritdoc cref="Process.StartInfo"/>
        public ProcessStartInfo StartInfo
        {
            get => _process.StartInfo;
            set => _process.StartInfo = value;
        }

        /// <inheritdoc cref="Process.StartTime"/>
        [SupportedOSPlatform("maccatalyst")]
        [UnsupportedOSPlatform("ios")]
        [UnsupportedOSPlatform("tvos")]
        public DateTime StartTime
        {
            get => _process.StartTime;
        }

        /// <inheritdoc cref="Process.SynchronizingObject"/>
        public ISynchronizeInvoke? SynchronizingObject
        {
            get => _process.SynchronizingObject;
            set => _process.SynchronizingObject = value;
        }

        /// <inheritdoc cref="Process.Threads"/>
        public ProcessThreadCollection Threads
        {
            get => _process.Threads;
        }

        /// <inheritdoc cref="Process.TotalProcessorTime"/>
        [SupportedOSPlatform("maccatalyst")]
        [UnsupportedOSPlatform("ios")]
        [UnsupportedOSPlatform("tvos")]
        public TimeSpan TotalProcessorTime
        {
            get => _process.TotalProcessorTime;
        }

        /// <inheritdoc cref="Process.UserProcessorTime"/>
        [SupportedOSPlatform("maccatalyst")]
        [UnsupportedOSPlatform("ios")]
        [UnsupportedOSPlatform("tvos")]
        public TimeSpan UserProcessorTime
        {
            get => _process.UserProcessorTime;
        }

        /// <inheritdoc cref="Process.VirtualMemorySize"/>
        [System.Obsolete("Process.VirtualMemorySize has been deprecated because the type of the property can't represent all valid results. Use System.Diagnostics.Process.VirtualMemorySize64 instead.")]
        public int VirtualMemorySize
        {
            get => _process.VirtualMemorySize;
        }

        /// <inheritdoc cref="Process.VirtualMemorySize64"/>
        public long VirtualMemorySize64
        {
            get => _process.VirtualMemorySize64;
        }

        /// <inheritdoc cref="Process.WorkingSet"/>
        [System.Obsolete("Process.WorkingSet has been deprecated because the type of the property can't represent all valid results. Use System.Diagnostics.Process.WorkingSet64 instead.")]
        public int WorkingSet
        {
            get => _process.WorkingSet;
        }

        /// <inheritdoc cref="Process.WorkingSet64"/>
        public long WorkingSet64
        {
            get => _process.WorkingSet64;
        }

        #endregion Properties Wrappers

        #region Static Methods Wrappers

        /// <inheritdoc cref="Process.EnterDebugMode()"/>
        public static void EnterDebugMode()
            => Process.EnterDebugMode();

        /// <inheritdoc cref="Process.GetCurrentProcess()"/>
        public static Process GetCurrentProcess()
            => Process.GetCurrentProcess();

        /// <inheritdoc cref="Process.GetProcessById()"/>
        public static Process GetProcessById(int processId, string machineName)
            => Process.GetProcessById(processId, machineName);

        /// <inheritdoc cref="Process.GetProcessById()"/>
        public static Process GetProcessById(int processId)
            => Process.GetProcessById(processId);

        /// <inheritdoc cref="Process.GetProcesses()"/>
        [SupportedOSPlatform("maccatalyst")]
        [UnsupportedOSPlatform("ios")]
        [UnsupportedOSPlatform("tvos")]
        public static Process[] GetProcesses()
            => Process.GetProcesses();

        /// <inheritdoc cref="Process.GetProcesses(string)"/>
        [SupportedOSPlatform("maccatalyst")]
        [UnsupportedOSPlatform("ios")]
        [UnsupportedOSPlatform("tvos")]
        public static Process[] GetProcesses(string machineName)
            => Process.GetProcesses(machineName);

        /// <inheritdoc cref="Process.GetProcessesByName(string?, string)"/>
        [SupportedOSPlatform("maccatalyst")]
        [UnsupportedOSPlatform("ios")]
        [UnsupportedOSPlatform("tvos")]
        public static Process[] GetProcessesByName(string? processName, string machineName)
            => Process.GetProcessesByName(processName, machineName);

        /// <inheritdoc cref="Process.GetProcessesByName(string?)"/>
        [SupportedOSPlatform("maccatalyst")]
        [UnsupportedOSPlatform("ios")]
        [UnsupportedOSPlatform("tvos")]
        public static Process[] GetProcessesByName(string? processName)
            => Process.GetProcessesByName(processName);

        /// <inheritdoc cref="Process.LeaveDebugMode()"/>
        public static void LeaveDebugMode()
            => Process.LeaveDebugMode();

        /// <inheritdoc cref="Process.Start(ProcessStartInfo)"/>
        [SupportedOSPlatform("maccatalyst")]
        [UnsupportedOSPlatform("ios")]
        [UnsupportedOSPlatform("tvos")]
        public static Process? Start(ProcessStartInfo startInfo)
            => Process.Start(startInfo);

        /// <inheritdoc cref="Process.Start(string, IEnumerable{string})"/>
        [SupportedOSPlatform("maccatalyst")]
        [UnsupportedOSPlatform("ios")]
        [UnsupportedOSPlatform("tvos")]
        public static Process Start(string fileName, IEnumerable<string> arguments)
            => Process.Start(fileName, arguments);

        /// <inheritdoc cref="Process.Start(string, string, SecureString, string)"/>
        [System.CLSCompliant(false)]
        [SupportedOSPlatform("windows")]
        public static Process? Start(string fileName, string userName, SecureString password, string domain)
            => Process.Start(fileName, userName, password, domain);

        /// <inheritdoc cref="Process.Start(string, string, string, SecureString, string)"/>
        [System.CLSCompliant(false)]
        [SupportedOSPlatform("windows")]
        public static Process? Start(string fileName, string arguments, string userName, SecureString password, string domain)
            => Process.Start(fileName, arguments, userName, password, domain);

        /// <inheritdoc cref="Process.Start(string, string)"/>
        [SupportedOSPlatform("maccatalyst")]
        [UnsupportedOSPlatform("ios")]
        [UnsupportedOSPlatform("tvos")]
        public static Process Start(string fileName, string arguments)
            => Process.Start(fileName, arguments);

        /// <inheritdoc cref="Process.Start(string)"/>
        [SupportedOSPlatform("maccatalyst")]
        [UnsupportedOSPlatform("ios")]
        [UnsupportedOSPlatform("tvos")]
        public static Process Start(string fileName)
            => Process.Start(fileName);

        #endregion Static Methods Wrappers

        #region Methods Wrappers

        /// <inheritdoc cref="Process.BeginErrorReadLine()"/>
        public void BeginErrorReadLine()
            => _process.BeginErrorReadLine();

        /// <inheritdoc cref="Process.BeginOutputReadLine()"/>
        public void BeginOutputReadLine()
            => _process.BeginOutputReadLine();

        /// <inheritdoc cref="Process.CancelErrorRead()"/>
        public void CancelErrorRead()
            => _process.CancelErrorRead();

        /// <inheritdoc cref="Process.CancelOutputRead()"/>
        public void CancelOutputRead()
            => _process.CancelOutputRead();

        /// <inheritdoc cref="Process.Close()"/>
        public void Close()
            => _process.Close();

        /// <inheritdoc cref="Process.CloseMainWindow()"/>
        public bool CloseMainWindow()
            => _process.CloseMainWindow();

        /// <inheritdoc cref="Process.Dispose()"/>
        public void Dispose()
            => _process.Dispose();

        /// <inheritdoc cref="Process.Equals(object?)"/>
        public override bool Equals(object? obj)
            => _process.Equals(obj);

        /// <inheritdoc cref="Process.GetHashCode()"/>
        public override int GetHashCode()
            => _process.GetHashCode();

        /// <inheritdoc cref="Process.GetType()"/>
        public new Type GetType()
            => _process.GetType();

        /// <inheritdoc cref="Process.Kill()"/>
        [SupportedOSPlatform("maccatalyst")]
        [UnsupportedOSPlatform("ios")]
        [UnsupportedOSPlatform("tvos")]
        public void Kill()
            => _process.Kill();

        /// <inheritdoc cref="Process.Kill(bool)"/>
        [SupportedOSPlatform("maccatalyst")]
        [UnsupportedOSPlatform("ios")]
        [UnsupportedOSPlatform("tvos")]
        public void Kill(bool entireProcessTree)
            => _process.Kill(entireProcessTree);

        /// <inheritdoc cref="Process.Refresh()"/>
        public void Refresh()
            => _process.Refresh();

        /// <inheritdoc cref="Process.ToString()"/>
        public override string ToString()
            => _process.ToString();

        /// <inheritdoc cref="Process.WaitForExit()"/>
        public void WaitForExit()
            => _process.WaitForExit();

        /// <inheritdoc cref="Process.WaitForExit(int)"/>
        public bool WaitForExit(int milliseconds)
            => _process.WaitForExit(milliseconds);

        /// <inheritdoc cref="Process.WaitForExit(TimeSpan)"/>
        public bool WaitForExit(TimeSpan timeout)
            => _process.WaitForExit(timeout);

        /// <inheritdoc cref="Process.WaitForExitAsync(CancellationToken)"/>
        public Task WaitForExitAsync(CancellationToken cancellationToken = default)
            => _process.WaitForExitAsync(cancellationToken);

        /// <inheritdoc cref="Process.WaitForInputIdle()"/>
        public bool WaitForInputIdle()
            => _process.WaitForInputIdle();

        /// <inheritdoc cref="Process.WaitForInputIdle(int)"/>
        public bool WaitForInputIdle(int milliseconds)
            => _process.WaitForInputIdle(milliseconds);

        /// <inheritdoc cref="Process.WaitForInputIdle(TimeSpan)"/>
        public bool WaitForInputIdle(TimeSpan timeout)
            => _process.WaitForInputIdle(timeout);

        #endregion Methods Wrappers

        #region Event Wrappers

        [Browsable(false)]
        public event EventHandler? Disposed
        {
            add => _process.Disposed += value;
            remove => _process.Disposed -= value;
        }

        /// <inheritdoc cref="Process.ErrorDataReceived">
        public event DataReceivedEventHandler? ErrorDataReceived
        {
            add => _process.ErrorDataReceived += value;
            remove => _process.ErrorDataReceived -= value;
        }

        /// <inheritdoc cref="Process.Exited">
        public event EventHandler Exited
        {
            add => _process.Exited += value;
            remove => _process.Exited -= value;
        }

        /// <inheritdoc cref="Process.OutputDataReceived">
        public event DataReceivedEventHandler? OutputDataReceived
        {
            add => _process.OutputDataReceived += value;
            remove => _process.OutputDataReceived -= value;
        }

        #endregion Event Wrappers

        #endregion System.Diagnostics.Process Wrapper
    }
}
