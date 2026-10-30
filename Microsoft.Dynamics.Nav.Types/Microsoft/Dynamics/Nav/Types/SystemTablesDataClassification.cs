using System;
using System.Collections.Generic;
using Microsoft.Dynamics.Nav.Types.Metadata;

namespace Microsoft.Dynamics.Nav.Types;

public static class SystemTablesDataClassification
{
	private static Dictionary<Tuple<int, int>, ALDataClassification> systemTableClassification = new Dictionary<Tuple<int, int>, ALDataClassification>
	{
		{
			Tuple.Create(2000000001, 50),
			ALDataClassification.EndUserIdentifiableInformation
		},
		{
			Tuple.Create(2000000009, 2),
			ALDataClassification.EndUserIdentifiableInformation
		},
		{
			Tuple.Create(2000000053, 1),
			ALDataClassification.EndUserIdentifiableInformation
		},
		{
			Tuple.Create(2000000055, 1),
			ALDataClassification.EndUserPseudonymousIdentifiers
		},
		{
			Tuple.Create(2000000055, 2),
			ALDataClassification.EndUserIdentifiableInformation
		},
		{
			Tuple.Create(2000000067, 1),
			ALDataClassification.EndUserIdentifiableInformation
		},
		{
			Tuple.Create(2000000068, 3),
			ALDataClassification.CustomerContent
		},
		{
			Tuple.Create(2000000068, 4),
			ALDataClassification.CustomerContent
		},
		{
			Tuple.Create(2000000068, 5),
			ALDataClassification.CustomerContent
		},
		{
			Tuple.Create(2000000068, 6),
			ALDataClassification.CustomerContent
		},
		{
			Tuple.Create(2000000068, 7),
			ALDataClassification.CustomerContent
		},
		{
			Tuple.Create(2000000068, 9),
			ALDataClassification.CustomerContent
		},
		{
			Tuple.Create(2000000068, 11),
			ALDataClassification.EndUserIdentifiableInformation
		},
		{
			Tuple.Create(2000000068, 14),
			ALDataClassification.EndUserIdentifiableInformation
		},
		{
			Tuple.Create(2000000073, 3),
			ALDataClassification.EndUserPseudonymousIdentifiers
		},
		{
			Tuple.Create(2000000073, 6),
			ALDataClassification.EndUserIdentifiableInformation
		},
		{
			Tuple.Create(2000000075, 3),
			ALDataClassification.EndUserPseudonymousIdentifiers
		},
		{
			Tuple.Create(2000000075, 6),
			ALDataClassification.EndUserIdentifiableInformation
		},
		{
			Tuple.Create(2000000080, 3),
			ALDataClassification.EndUserPseudonymousIdentifiers
		},
		{
			Tuple.Create(2000000080, 6),
			ALDataClassification.EndUserIdentifiableInformation
		},
		{
			Tuple.Create(2000000110, 6),
			ALDataClassification.EndUserIdentifiableInformation
		},
		{
			Tuple.Create(2000000110, 1),
			ALDataClassification.EndUserPseudonymousIdentifiers
		},
		{
			Tuple.Create(2000000111, 9),
			ALDataClassification.EndUserIdentifiableInformation
		},
		{
			Tuple.Create(2000000111, 1),
			ALDataClassification.EndUserPseudonymousIdentifiers
		},
		{
			Tuple.Create(2000000114, 7),
			ALDataClassification.EndUserIdentifiableInformation
		},
		{
			Tuple.Create(2000000114, 9),
			ALDataClassification.CustomerContent
		},
		{
			Tuple.Create(2000000120, 1),
			ALDataClassification.EndUserPseudonymousIdentifiers
		},
		{
			Tuple.Create(2000000120, 2),
			ALDataClassification.EndUserIdentifiableInformation
		},
		{
			Tuple.Create(2000000120, 3),
			ALDataClassification.EndUserIdentifiableInformation
		},
		{
			Tuple.Create(2000000120, 7),
			ALDataClassification.EndUserPseudonymousIdentifiers
		},
		{
			Tuple.Create(2000000120, 11),
			ALDataClassification.EndUserIdentifiableInformation
		},
		{
			Tuple.Create(2000000120, 14),
			ALDataClassification.EndUserIdentifiableInformation
		},
		{
			Tuple.Create(2000000120, 15),
			ALDataClassification.EndUserPseudonymousIdentifiers
		},
		{
			Tuple.Create(2000000121, 1),
			ALDataClassification.EndUserPseudonymousIdentifiers
		},
		{
			Tuple.Create(2000000121, 2),
			ALDataClassification.CustomerContent
		},
		{
			Tuple.Create(2000000121, 4),
			ALDataClassification.CustomerContent
		},
		{
			Tuple.Create(2000000121, 5),
			ALDataClassification.CustomerContent
		},
		{
			Tuple.Create(2000000121, 3),
			ALDataClassification.EndUserIdentifiableInformation
		},
		{
			Tuple.Create(2000000151, 1),
			ALDataClassification.OrganizationIdentifiableInformation
		},
		{
			Tuple.Create(2000000154, 6),
			ALDataClassification.EndUserIdentifiableInformation
		},
		{
			Tuple.Create(2000000160, 28),
			ALDataClassification.OrganizationIdentifiableInformation
		},
		{
			Tuple.Create(2000000175, 2),
			ALDataClassification.EndUserIdentifiableInformation
		},
		{
			Tuple.Create(2000000175, 3),
			ALDataClassification.EndUserIdentifiableInformation
		},
		{
			Tuple.Create(2000000181, 13),
			ALDataClassification.EndUserIdentifiableInformation
		},
		{
			Tuple.Create(2000000184, 13),
			ALDataClassification.EndUserIdentifiableInformation
		},
		{
			Tuple.Create(2000000188, 1),
			ALDataClassification.EndUserPseudonymousIdentifiers
		},
		{
			Tuple.Create(2000000189, 4),
			ALDataClassification.EndUserPseudonymousIdentifiers
		},
		{
			Tuple.Create(2000000196, 9),
			ALDataClassification.EndUserIdentifiableInformation
		},
		{
			Tuple.Create(2000000197, 1),
			ALDataClassification.EndUserPseudonymousIdentifiers
		},
		{
			Tuple.Create(2000000197, 2),
			ALDataClassification.EndUserIdentifiableInformation
		},
		{
			Tuple.Create(2000000197, 3),
			ALDataClassification.OrganizationIdentifiableInformation
		},
		{
			Tuple.Create(2000000197, 6),
			ALDataClassification.EndUserIdentifiableInformation
		},
		{
			Tuple.Create(2000000197, 7),
			ALDataClassification.OrganizationIdentifiableInformation
		},
		{
			Tuple.Create(2000000095, 8),
			ALDataClassification.EndUserPseudonymousIdentifiers
		},
		{
			Tuple.Create(2000000095, 10),
			ALDataClassification.CustomerContent
		},
		{
			Tuple.Create(2000000096, 3),
			ALDataClassification.EndUserPseudonymousIdentifiers
		},
		{
			Tuple.Create(2000000098, 3),
			ALDataClassification.EndUserPseudonymousIdentifiers
		},
		{
			Tuple.Create(2000000201, 2),
			ALDataClassification.CustomerContent
		},
		{
			Tuple.Create(2000000107, 4),
			ALDataClassification.EndUserIdentifiableInformation
		},
		{
			Tuple.Create(2000000107, 5),
			ALDataClassification.CustomerContent
		},
		{
			Tuple.Create(2000000107, 6),
			ALDataClassification.CustomerContent
		}
	};

	public static ALDataClassification SystemTableClassification(int tableId, int fieldNo)
	{
		if (systemTableClassification.TryGetValue(Tuple.Create(tableId, fieldNo), out var value))
		{
			return value;
		}
		return ALDataClassification.SystemMetadata;
	}
}
