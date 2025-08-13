using System.IO;
using System.Collections.Generic;
using System.ServiceModel;
using System.Threading.Tasks;

namespace UnifiedFileGateway.Contracts
{
	[ServiceContract]
	public interface IFileService
	{
		[OperationContract]
		Task UploadFile(FileUploadMessage message);

		[OperationContract]
		Task<Stream> DownloadFile(string fileName);

		[OperationContract]
		Task DeleteFile(string fileName);

		[OperationContract]
		Task<string> GetFileStatus(string fileName);

		[OperationContract]
		Task<string[]> GetFiles(string folder = "CentralStorage");

		[OperationContract]
		Task<Dictionary<string, int>> GetFileStatistics();
	}
}