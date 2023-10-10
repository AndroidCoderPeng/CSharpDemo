using System.Collections.Generic;
using Prism.Events;

namespace CSharpDemo.Events
{
    public class WavePointEvent : PubSubEvent<List<double>>
    {
    }
}