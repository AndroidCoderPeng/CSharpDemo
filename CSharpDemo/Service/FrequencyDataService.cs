using System;

namespace CSharpDemo.Service
{
    public class FrequencyDataService : IFrequencyDataService
    {
        private readonly Random _random = new Random();

        public string GetFrequency()
        {
            return _random.Next(0, 3000).ToString();
        }
    }
}