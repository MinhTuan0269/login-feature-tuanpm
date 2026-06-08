using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories.Commons
{
    public class BaseEntity<T> where T : struct
    {
        public T Id { get; set; }
        public DateTime CreatedAt { get; set; }
        protected BaseEntity()
        {
         
            var type = this.GetType();
            var createdAtProp = type.GetProperty("CreatedAt", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.DeclaredOnly);
            
            if (createdAtProp != null && createdAtProp.CanWrite)
            {
                createdAtProp.SetValue(this, DateTime.UtcNow); 
            }
            else
            {
                CreatedAt = DateTime.UtcNow; 
            }
        }

    }
    public abstract class BaseEntityGuid : BaseEntity<Guid>
    {
        protected BaseEntityGuid() : base()
        {
            var type = this.GetType();
            var idProp = type.GetProperty("Id", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.DeclaredOnly);
            
            if (idProp != null && idProp.CanWrite)
            {
                idProp.SetValue(this, Guid.NewGuid()); 
            }
            else
            {
                Id = Guid.NewGuid(); 
            }
        }
    }

    public abstract class BaseEntityInt : BaseEntity<int>
    {
        protected BaseEntityInt() : base()
        {
        }
    }
}
