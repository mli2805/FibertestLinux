using System.Collections.Generic;
using Fibertest.Graph;

namespace KadastrLoader
{
    public class LoadedAlready
    {
        public List<Well> Wells { get; set; } = new List<Well>();
        public List<Conpoint> Conpoints { get; set; } = new List<Conpoint>();
    }
}