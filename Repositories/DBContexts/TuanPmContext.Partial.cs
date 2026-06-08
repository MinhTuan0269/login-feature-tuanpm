using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Repositories.DBContexts;

public partial class TuanPmContext
{
    public override int SaveChanges()
    {
        GenerateIds();
        return base.SaveChanges();
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        GenerateIds();
        return base.SaveChangesAsync(cancellationToken);
    }

    private void GenerateIds()
    {
        var entries = ChangeTracker.Entries().Where(e => e.State == EntityState.Added);
        foreach (var entry in entries)
        {
            var idProp = entry.Entity.GetType().GetProperty("Id");
            if (idProp != null && idProp.PropertyType == typeof(Guid))
            {
                var currentId = (Guid)idProp.GetValue(entry.Entity);
                if (currentId == Guid.Empty)
                {
                    idProp.SetValue(entry.Entity, Guid.NewGuid());
                }
            }

            var createdAtProp = entry.Entity.GetType().GetProperty("CreatedAt");
            if (createdAtProp != null && createdAtProp.PropertyType == typeof(DateTime))
            {
                createdAtProp.SetValue(entry.Entity, DateTime.UtcNow);
            }
        }
    }
}
