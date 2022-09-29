using System.Collections.Generic;
using System.Net.Sockets;

namespace LDocBuilder.Core
{
    public class LDocBuilder
    {
        public List<LDoc> Ldocs = new List<LDoc>();

        public LDocBuilder()
        {
            
        }

        public void Add(LDoc doc)
        {
            Ldocs.Add(doc);
        }
    }
}