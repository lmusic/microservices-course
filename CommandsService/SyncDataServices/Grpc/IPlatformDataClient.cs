using System.Collections.Generic;
using CommandsService.Models;

namespace CommandsService.SynDataServices.Grpc
{
    public interface IPlatformDataClient
    {
        IEnumerable<Platform> GetAllPlatforms();
    }
}