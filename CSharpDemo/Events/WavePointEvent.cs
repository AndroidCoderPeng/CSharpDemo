using System.Collections.Generic;
using System.Windows;
using Prism.Events;

namespace CSharpDemo.Events
{
    public class WavePointEvent : PubSubEvent<List<Point>>
    {
    }
}