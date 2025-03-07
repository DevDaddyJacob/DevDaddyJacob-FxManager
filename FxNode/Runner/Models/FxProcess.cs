using Microsoft.Win32.SafeHandles;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace DevDaddyJacob.FxManager.Node.Runner.Models
{
    /// <summary>
    /// Extension of <see cref="System.Diagnostics.Process"/> with support for FD3 for FiveM usage
    ///
    /// and yes, this is shitty, but I aint got anything better
    /// </summary>
    internal class FxProcess
    {
        // Windows API functions
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool CreatePipe(out SafeFileHandle hReadPipe, out SafeFileHandle hWritePipe, IntPtr lpPipeAttributes, int nSize);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool SetHandleInformation(SafeFileHandle hObject, uint dwMask, uint dwFlags);

        private const uint HANDLE_FLAG_INHERIT = 0x00000001;

        private Process _process;
        public Process Process { get => _process; }
        public Process P { get => Process; }

        private SafeFileHandle _fd3ReadPipe;
        private SafeFileHandle _fd3WritePipe;
        private FileStream? _fd3FileStream;
        private StreamReader? _fd3Reader;
        public StreamReader? FD3 { get => _fd3Reader; }

        /// <inheritdoc cref="System.Diagnostics.Process()"/>
        public FxProcess()
        {
            _process = new Process();
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

        /// <inheritdoc cref="System.Diagnostics.Process.Start()"/>
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
    }
}
