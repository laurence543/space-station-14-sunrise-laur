using System.Linq;
// Custom Code
//using System;
//using System.Collections.Generic;
//using System.Text;
//using System.Threading.Tasks;
// //////////////

using Robust.Shared.Configuration;
using Content.Shared.CCVar;
using Robust.Shared.Network;

// Custom Code
//using System.Threading.Tasks;
//using Robust.Server.Player;
//using Robust.Shared.Network;
//using System.Net;
// //////////////


namespace Content.Server.Connection.Whitelist.Conditions;

/// <summary>
/// Condition that matches if the player's HW is in the HW Whitelist.
/// </summary>
public sealed partial class ConditionIDWhitelist : WhitelistCondition
{
    // Custom Code
    // IP try
    //[Dependency] private readonly IConfigurationManager _cfg = default!;

    //public bool IsAllowed(NetConnectingArgs e)
    //{
    //    var allowedIPs = _cfg.GetCVar(CCVars.WhitelistAllowedIPs);
    //    var clientIP = e.IP.Address.ToString();
    //    return allowedIPs.Contains(clientIP);
    //}



}
