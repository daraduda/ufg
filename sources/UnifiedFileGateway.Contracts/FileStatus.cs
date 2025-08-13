using System.Runtime.Serialization;

namespace UnifiedFileGateway.Contracts
{
	[DataContract]
	public enum FileStatus
	{
		[EnumMember]
		Scanning,

		[EnumMember]
		Clean,

		[EnumMember]
		Infected,

		[EnumMember]
		NotFound
	}
}