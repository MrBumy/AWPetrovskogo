using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AWPetrovskogo.Data
{
    internal class ConnectObject
    {
        public static AWPetrovskogoEntities connect;
        public static AWPetrovskogoEntities GetConnect()
        {
            if (connect == null)
            {
                connect = new AWPetrovskogoEntities();
            }
            return connect;
        }
    }
}
