using System;
using System.Collections.Generic;
using Fibertest.StringResources;

namespace Fibertest.WpfClient
{
    public class StepModel
    {
        public Guid NodeId { get; set; }
        public string Title { get; set; } = string.Empty;
        public Guid EquipmentId { get; set; }

        // for first StepModel (RTU) it is empty, than list contains path from previous node to current through adjustment points
        // public List<FiberVm> FiberVms { get; set; }  
        public List<Guid> FiberIds { get; set; } = null!; 

        public override string ToString()
        {
            return string.IsNullOrEmpty(Title) ? Resources.SID____noname_node_ : Title;
        }
    }
}