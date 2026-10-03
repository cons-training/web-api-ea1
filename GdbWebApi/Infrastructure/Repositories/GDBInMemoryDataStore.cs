using System.Data;
using GdbWebApi.Data;

namespace GdbWebApi.Infrastructure.Repositories
{
    public static class GDBInMemoryDataStore
    {
        public static DataSet DataSet { get; } =
            GDBInMemoryDB.CreateDataSet();
    }
}