using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RichardSzalay.PocketCiTray.Services;

namespace RichardSzalay.PocketCiTray.Tests.CommonTests.Services
{
    public class ApplicationSettingsSpec
    {
        [TestClass]
        public class when_shrinking_and_unshrinking_no_days
        {
            private DayOfWeek[] result;

            [ClassInitialize]
            public void because_of()
            {
                int shrunk = ApplicationSettings.ShrinkDaysOfWeek();

                this.result = ApplicationSettings.UnshrinkDaysOfWeek(shrunk);
            }

            [TestMethod]
            public void it_should_return_no_days()
            {
                Assert.AreEqual(0, result.Length);
            }
        }

        [TestClass]
        public class when_shrinking_and_unshrinking_week_days
        {
            private DayOfWeek[] result;

            [ClassInitialize]
            public void because_of()
            {
                int shrunk = ApplicationSettings.ShrinkDaysOfWeek(DayOfWeek.Monday, 
                    DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday,
                    DayOfWeek.Friday);

                this.result = ApplicationSettings.UnshrinkDaysOfWeek(shrunk);
            }

            [TestMethod]
            public void it_should_return_week_days()
            {
                Assert.AreEqual(5, result.Length);
                Assert.AreEqual(DayOfWeek.Monday, result[0]);
                Assert.AreEqual(DayOfWeek.Friday, result[4]);
            }
        }

        [TestClass]
        public class when_shrinking_and_unshrinking_weekend_days
        {
            private DayOfWeek[] result;

            [ClassInitialize]
            public void because_of()
            {
                int shrunk = ApplicationSettings.ShrinkDaysOfWeek(DayOfWeek.Saturday, DayOfWeek.Sunday);

                this.result = ApplicationSettings.UnshrinkDaysOfWeek(shrunk);
            }

            [TestMethod]
            public void it_should_return_weekend_days()
            {
                Assert.AreEqual(2, result.Length);
                Assert.AreEqual(DayOfWeek.Sunday, result[0]);
                Assert.AreEqual(DayOfWeek.Saturday, result[1]);
            }
        }
    }
}
