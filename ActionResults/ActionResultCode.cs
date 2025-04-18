using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace DevDaddyJacob.ActionResults
{
    // 0x8000000000000000   1000000000000000000000000000000000000000000000000000000000000000    9223372036854775808 64-bit unsigned
    // 0x7f80000000000000	0111111110000000000000000000000000000000000000000000000000000000	9187343239835811840	64-bit unsigned	
    // 0x7fffffff800000	    0000000001111111111111111111111111111111100000000000000000000000	36028797010575360	64-bit unsigned	
    // 0x7fffff	            0000000000000000000000000000000000000000011111111111111111111111	8388607	            64-bit unsigned	
    public struct ActionResultCode
    {
        #region Fields

        private const int SUCCESS_FLAG_RSHIFT = 63;
        private const byte SUCCESS_FLAG_RAW_MASK = 0x1;
        private const ulong SUCCESS_FLAG_MASK = (ulong)SUCCESS_FLAG_RAW_MASK << SUCCESS_FLAG_RSHIFT;

        private const int SCOPE_FLAG_RSHIFT = 55;
        private const byte SCOPE_FLAG_RAW_MASK = 0xff;
        private const ulong SCOPE_FLAG_MASK = (ulong)SCOPE_FLAG_RAW_MASK << SCOPE_FLAG_RSHIFT;

        private const int FAMILY_FLAG_RSHIFT = 23;
        private const uint FAMILY_FLAG_RAW_MASK = 0xffffffffu;
        private const ulong FAMILY_FLAG_MASK = (ulong)FAMILY_FLAG_RAW_MASK << FAMILY_FLAG_RSHIFT;

        private const int IDENT_FLAG_RSHIFT = 0;
        private const uint IDENT_FLAG_RAW_MASK = 0x7fffffu;
        private const ulong IDENT_FLAG_MASK = (ulong)IDENT_FLAG_RAW_MASK << IDENT_FLAG_RSHIFT;

        private ulong _value;

        #endregion Fields

        #region Properties

        public ulong Value
        {
            get => _value;
        }

        // Bits: 63
        // Number of Bits: 1 Bit
        // Description: Flag indicating if the code represents a success or failure (1 = success)
        // Retrieval: value >>> 63
        // https://bitwisecmd.com/#0xfffffffffffffffful%2C%3E%3E%3E%2C63
        public bool Successful
        {
            get => Extract(ref _value, SUCCESS_FLAG_MASK, SCOPE_FLAG_RSHIFT) == 1;
            set => Insert(ref _value, (value ? 0x1ul : 0x0ul), SUCCESS_FLAG_MASK, SUCCESS_FLAG_RSHIFT);
        }

        // Bits: 62 to 55
        // Number of Bits: 8 Bit
        // Description: The scope of the code
        // Retrieval: (value & 0x7f80000000000000ul) >> 55
        // https://bitwisecmd.com/#0xfffffffffffffffful%2C%26%2C0x7f80000000000000ul%2C%3E%3E%2C55ul
        public byte Scope
        {
            get => (byte)Extract(ref _value, SCOPE_FLAG_MASK, SCOPE_FLAG_RSHIFT);
            set => Insert(ref _value, value, SCOPE_FLAG_MASK, SCOPE_FLAG_RSHIFT);
        }

        // Bits: 54 to 23
        // Number of Bits: 32 Bit
        // Description: The number identifying the family the code belongs to
        // Retrieval: (value & 0x7fffffff800000ul) >> 23
        // https://bitwisecmd.com/#0xfffffffffffffffful%2C%26%2C0x007fffffff800000ul%2C%3E%3E%2C23
        public uint Family
        {
            get => (uint)Extract(ref _value, FAMILY_FLAG_MASK, FAMILY_FLAG_RSHIFT);
            set => Insert(ref _value, value, FAMILY_FLAG_MASK, FAMILY_FLAG_RSHIFT);
        }

        // Bits: 22 to 0
        // Number of Bits: 23 Bit (Max value of 8388607)
        // Description: The number Uniquely identifying the code within the family
        // Retrieval: (value & 0x7fffffff800000ul)
        // https://bitwisecmd.com/#0xfffffffffffffffful%2C%26%2C0x7ffffful
        public uint Identifier
        {
            get => (uint)Extract(ref _value, IDENT_FLAG_MASK, IDENT_FLAG_RSHIFT);
            set => Insert(ref _value, value, IDENT_FLAG_MASK, IDENT_FLAG_RSHIFT);
        }

        #endregion Properties

        #region Operators

        public static implicit operator ulong(ActionResultCode result)
        {
            return result.Value;
        }

        public static explicit operator ActionResultCode(ulong value)
        {
            return new ActionResultCode(value);
        }

        #endregion Operators

        #region Constructors

        private ActionResultCode(ulong value)
        {
            _value = value;
        }

        public ActionResultCode(bool success, byte scope, uint family, uint identifier)
        {
            Sanitize(ref identifier, IDENT_FLAG_RAW_MASK);

            _value = 0;
            Successful = success;
            Scope = scope;
            Family = family;
            Identifier = identifier;
        }

        public ActionResultCode(ulong success, ulong scope, ulong family, ulong identifier)
        {
            Sanitize(ref success, SUCCESS_FLAG_RAW_MASK);
            Sanitize(ref scope, SCOPE_FLAG_RAW_MASK);
            Sanitize(ref family, FAMILY_FLAG_RAW_MASK);
            Sanitize(ref identifier, IDENT_FLAG_RAW_MASK);

            _value = 0;
            Successful = success == 1;
            Scope = (byte)scope;
            Family = (uint)family;
            Identifier = (uint)identifier;
        }

        #endregion Constructors

        #region Private Methods

        private static void Sanitize(ref ulong value, ulong mask)
            => value &= mask;

        private static void Sanitize(ref uint value, uint mask)
            => value &= mask;

        private static void Sanitize(ref byte value, byte mask)
            => value &= mask;

        private static ulong Extract(ref ulong value, ulong mask, int shift)
        {
            return (value & mask) >> shift;
        }

        private static void Insert(ref ulong value, ulong newValue, ulong mask, int shift)
        {
            // Clear the mask
            value &= mask;

            // Insert the new value
            value |= newValue << shift;
        }

        #endregion Private Methods
    }
}

