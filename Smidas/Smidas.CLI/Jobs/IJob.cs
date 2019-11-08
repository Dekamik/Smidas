using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace Smidas.CLI.Jobs
{
    public interface IJob<T>
    {
        T Run();
    }
}
