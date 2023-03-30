using System;
using CSharpDemo.Service;

namespace CSharpDemo.ServiceImpl
{
    public class FrequencyDataServiceImpl : IFrequencyDataService
    {
        private readonly Random _random = new Random();

        public string GetFrequency()
        {
            return _random.Next(0, 3000).ToString();
        }
    }
}