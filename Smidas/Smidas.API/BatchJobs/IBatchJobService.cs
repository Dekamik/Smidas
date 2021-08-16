using System.Threading.Tasks;

namespace Smidas.API.BatchJobs
{
    public interface IBatchJobService
    {
        void RunOnIndex(string index);
    }
}