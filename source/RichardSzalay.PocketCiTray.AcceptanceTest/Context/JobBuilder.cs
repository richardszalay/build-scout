using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RichardSzalay.PocketCiTray.AcceptanceTest.Context
{
    public class JobBuilder
    {
        private Random random = new Random();
        public string Name { get; set; }
        private BuildResult lastResult;
        public BuildResult LastResult
        {
            get
            {
                if (RandomLastResult)
                {
                    return (BuildResult)random.Next(0, 3);
                }
                else
                {
                    return lastResult;
                }
            }
            set { lastResult = value; }
        }

        public bool RandomLastResult { get; set; }
    }
}
