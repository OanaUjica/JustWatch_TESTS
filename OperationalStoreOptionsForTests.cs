using IdentityServer4.EntityFramework.Options;
using Microsoft.Extensions.Options;

namespace Tests_JustWatch
{
    public class OperationalStoreOptionsForTests : IOptions<OperationalStoreOptions>
    {
        public OperationalStoreOptions Value => new();
    }
}
