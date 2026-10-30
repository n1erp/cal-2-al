using System;
using System.Security.Cryptography;
using Microsoft.Dynamics.Nav.Types;

namespace Microsoft.Dynamics.Nav.Runtime;

public class TenantConfiguration
{
	private const int MagicNumber = 66;

	public NavTenantSettings Settings { get; }

	public TenantConfigurationState State { get; private set; }

	public long Timestamp { get; }

	public int? ExclusiveAccessOwnerServerId { get; set; }

	public bool CanTenantBeMounted
	{
		get
		{
			if (State != TenantConfigurationState.Active)
			{
				return State == TenantConfigurationState.ExclusiveAccess;
			}
			return true;
		}
	}

	public TenantConfiguration(NavTenantSettings settings, long timestamp, TenantConfigurationState initialState, int? exclusiveAccessOwnerServerId = null)
	{
		Settings = settings;
		Timestamp = timestamp;
		State = initialState;
		ExclusiveAccessOwnerServerId = exclusiveAccessOwnerServerId;
	}

	internal string CalculateExclusiveAccessTicket()
	{
		SHA256 sHA = SHA256.Create();
		return Convert.ToBase64String(sHA.ComputeHash(BitConverter.GetBytes(Timestamp ^ 0x42)));
	}
}
