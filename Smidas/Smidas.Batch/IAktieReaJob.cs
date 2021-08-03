using System.Collections.Generic;
using System.Threading.Tasks;
using Smidas.Common;
using Smidas.Core.Stocks;

namespace Smidas.Batch
{
    public interface IAktieReaJob
    {
        /// <summary>
        /// Runs analysis based on the specified input.
        /// 
        /// 1 = OMX Stockholm
        /// 2 = OMX Copenhagen
        /// 3 = OMX Helsinki
        /// 4 = Oslo OBX
        /// </summary>
        /// <param name="input">The input.</param>
        /// <returns></returns>
        Task<IEnumerable<Stock>> Run(AktieReaQuery query);
    }
}