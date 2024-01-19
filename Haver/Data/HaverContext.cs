using Haver.Models;
using Microsoft.EntityFrameworkCore;

namespace Haver.Data
{
    public class HaverContext : DbContext
    {
        public HaverContext(DbContextOptions<HaverContext> options) : base(options)
        {

        }



    }
}
