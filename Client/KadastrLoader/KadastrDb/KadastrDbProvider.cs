using System.Collections.Generic;
using System.Threading.Tasks;
using Fibertest.Graph;
using Microsoft.EntityFrameworkCore;

namespace KadastrLoader
{
    public class KadastrDbProvider
    {
        private readonly KadastrDbSettings _kadastrDbSettings;

        public KadastrDbProvider(KadastrDbSettings kadastrDbSettings)
        {
            _kadastrDbSettings = kadastrDbSettings;
        }

        public void Init()
        {
            _kadastrDbSettings.Init();
            using (var dbContext = new KadastrDbContext(_kadastrDbSettings.Options))
            {
                dbContext.Database.EnsureCreated();
            }
        }

        public async Task<List<Well>> GetWells()
        {
            using (var dbContext = new KadastrDbContext(_kadastrDbSettings.Options))
            {
                return await dbContext.Wells.ToListAsync();
            }
        }

        public async Task<List<Conpoint>> GetConpoints()
        {
            using (var dbContext = new KadastrDbContext(_kadastrDbSettings.Options))
            {
                return await dbContext.Conpoints.ToListAsync();
            }
        }

        public async Task<int> AddWell(Well well)
        {
            using (var dbContext = new KadastrDbContext(_kadastrDbSettings.Options))
            {
                dbContext.Wells.Add(well);
                return await dbContext.SaveChangesAsync();
            }
        }

        public async Task<int> AddConpoint(Conpoint conpoint)
        {
            using (var dbContext = new KadastrDbContext(_kadastrDbSettings.Options))
            {
                dbContext.Conpoints.Add(conpoint);
                return await dbContext.SaveChangesAsync();
            }
        }
    }
}