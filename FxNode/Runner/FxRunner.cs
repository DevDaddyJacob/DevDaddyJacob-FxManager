using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Intrinsics.X86;
using System.Text;
using System.Threading.Tasks;

namespace DevDaddyJacob.Runner.FxRunner
{
    internal class FxRunner
    {
        #region Fields
        #endregion Fields

        #region Constructor
        #endregion Constructor

        #region Public Methods

        #region Signal Methods

        /// <summary>
        /// Gracefully shutdown when socket gets an exit event.  
        /// Sends the quit command and wait for the process to exit. 
        /// 
        /// https://github.com/tabarra/txAdmin/blob/052fe005b42bc151fc6bb2e9d72702d0257a599d/core/modules/FxRunner/index.ts#L53
        /// </summary>
        public Task HandleShutdown()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Receives the signal that all the pre-start processes are complete
        /// 
        /// https://github.com/tabarra/txAdmin/blob/052fe005b42bc151fc6bb2e9d72702d0257a599d/core/modules/FxRunner/index.ts#L65
        /// </summary>
        public void SignalStartReady()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Handles boot signals related to bind errors and sets the backoff delay.  
        /// On successfull bind, the backoff delay is reset to 0.  
        /// On bind error, the backoff delay is increased by 5s, up to 45s.
        /// 
        /// https://github.com/tabarra/txAdmin/blob/052fe005b42bc151fc6bb2e9d72702d0257a599d/core/modules/FxRunner/index.ts#L90
        /// </summary>
        /// <param name="required">If the backoff is needed or not</param>
        /// <returns>The new backoff delay in ms</returns>
        public int SignalSpawnBackoffRequest(bool required)
        {
            throw new NotImplementedException();
        }

        #endregion Signal Methods

        #region Power Action Methods

        /// <summary>
        /// Spawns the FXServer and sets up all the event handlers.
        /// 
        /// https://github.com/tabarra/txAdmin/blob/052fe005b42bc151fc6bb2e9d72702d0257a599d/core/modules/FxRunner/index.ts#L111
        /// </summary>
        /// <param name="shouldAnnounce"></param>
        /// <returns>Returns null if successfully started, otherwise returns an error message</returns>
        public async Task<string?> SpawnServer(bool shouldAnnounce = false)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Restarts the FXServer
        /// 
        /// https://github.com/tabarra/txAdmin/blob/052fe005b42bc151fc6bb2e9d72702d0257a599d/core/modules/FxRunner/index.ts#L256
        /// </summary>
        /// <param name="reason">The reason for the restart</param>
        /// <param name="author">Who requested the restart</param>
        /// <returns>Returns null if successfully restarted, otherwise returns an error message</returns>
        public async Task<string?> RestartServer(string reason, string author)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Kills the FXServer child process.
        /// 
        /// https://github.com/tabarra/txAdmin/blob/052fe005b42bc151fc6bb2e9d72702d0257a599d/core/modules/FxRunner/index.ts#L299
        /// </summary>
        /// <param name="reason">The reason for the stop</param>
        /// <param name="author">Who requested the stop</param>
        /// <param name="isRestarting">isRestarting might be true even if not called by this.restartServer().</param>
        /// <returns>Returns null if successfully killed, otherwise returns an error message</returns>
        public async Task<string?> KillServer(string reason, string author, bool isRestarting = false)
        {
            throw new NotImplementedException();
        }

        #endregion Power Action Methods

        #endregion Public Methods

        #region Private Methods
        #endregion Private Methods
    }
}
