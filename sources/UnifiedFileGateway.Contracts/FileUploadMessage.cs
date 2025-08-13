		using System;
using System.IO;
using System.ServiceModel;

namespace UnifiedFileGateway.Contracts
{
	[MessageContract]
	public class FileUploadMessage : IDisposable
	{
		[MessageHeader(MustUnderstand = true)]
		public string? FileName { get; set; }

		[MessageBodyMember(Order = 1)]
		public Stream? FileData { get; set; }

		public void Dispose()
		{
			FileData?.Dispose();
		}
	}
}