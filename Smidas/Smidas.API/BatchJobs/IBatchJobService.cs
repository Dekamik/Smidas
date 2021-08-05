using System.Threading.Tasks;

namespace Smidas.API.BatchJobs
{
    public interface IBatchJobService
    {
        Task RunOnIndex(string index);
    }
}