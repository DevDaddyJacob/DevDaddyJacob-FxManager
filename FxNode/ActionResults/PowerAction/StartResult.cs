using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevDaddyJacob.FxManager.Node.ActionResults.PowerAction
{
    internal class StartResult : PowerActionResult
    {
        private bool _isSuccessful;

        public override bool IsSuccessful 
        { 
            get => _isSuccessful; 
        }

        private StartResult(int code, bool isSuccessful, string message) : base(code, message) 
        {
            _isSuccessful = isSuccessful;
        }

        public static StartResult CannotStartWhileShuttingDown => new()
    }
}
