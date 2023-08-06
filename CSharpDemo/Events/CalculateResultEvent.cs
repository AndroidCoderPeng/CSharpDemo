using MathWorks.MATLAB.NET.Arrays;
using Prism.Events;

namespace CSharpDemo.Events
{
    public class CalculateResultEvent : PubSubEvent<MWArray[]>
    {
    }
}