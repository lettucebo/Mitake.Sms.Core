using Ci.Result;
using Mitake.Sms.Core.Models;

namespace Mitake.Sms.Core;

public static class StatusFlagExtensions
{
    public static CiStatus ToCiStatus(this StatusFlag status)
    {
        if (status is StatusFlag.Zero or StatusFlag.One or StatusFlag.Two or StatusFlag.Four)
            return CiStatus.Success;
        else if (status is StatusFlag.C or StatusFlag.D or StatusFlag.E or StatusFlag.F or StatusFlag.H or StatusFlag.K or StatusFlag.L or StatusFlag.M or StatusFlag.N
                 or StatusFlag.P or StatusFlag.R or StatusFlag.S)
            return CiStatus.UnAuthorized;
        else
            return CiStatus.Failure;
    }
}