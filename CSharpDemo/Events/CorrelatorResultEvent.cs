using Prism.Events;

namespace CSharpDemo.Events
{
    public class CorrelatorResultEvent<T> : PubSubEvent<T>
    {
    }
}