using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevDaddyJacob.ActionResults
{
    public abstract class ActionResult
    {
        #region Fields

        private ActionResultCode _code;
        private string? _message;

        #endregion Fields

        #region Properties

        public virtual ActionResultCode Code
        {
            get => _code;
        }

        public virtual string? Message
        {
            get => _message;
        }

        #endregion Properties

        #region Constructors

        public ActionResult(ActionResultCode code, string? message)
        {
            _code = code;
            _message = message;
        }

        public ActionResult(ActionResultCode code) : this(code, null)
        { 
        }

        public ActionResult(ulong code) : this(code, null)
        {
        }

        #endregion Constructors

        #region Object Class

        public override bool Equals(object? obj)
        {
            return Equals(obj as ActionResult);
        }

        public override int GetHashCode()
        {
            return _code.GetHashCode();
        }

        #endregion Object Class

        #region IEquatable Interface

        public bool Equals(ActionResult? other)
        {
            return other is not null && other.Code == _code;
        }

        public bool Equals(ulong other)
        {
            return other == _code.Value;
        }

        #endregion IEquatable Interface
    }
}
