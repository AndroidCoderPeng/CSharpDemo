using System.Collections.Generic;

namespace CSharpDemo.Model
{
    public class ReginModel
    {
        public List<Piont> position { get; set; }
        public string color { get; set; }
        public string code { get; set; }
    }

    public class Piont
    {
        public double x { get; set; }
        public double y { get; set; }
    }
}