using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TF2_RenderDemo_Batcher.Models
{
    public class DemoEvent
    {
        public string name { get; set; }
        public string value { get; set; }
        public int value_i { get => Convert.ToInt32(value); }
        public int tick { get; set; }
    }
}
