using System;
using NUnit.Framework;
using TradeLogic;

namespace TradeLogic.UnitTests
{
    [TestFixture]
    public class SessionConfigTests
    {
        [Test]
        public void SessionStartLocal_IsCorrect()
        {
            Assert.That(SessionConfig.SessionStartLocal, Is.EqualTo(new TimeSpan(6, 30, 0)));
        }

        [Test]
        public void SessionEndLocal_IsCorrect()
        {
            Assert.That(SessionConfig.SessionEndLocal, Is.EqualTo(new TimeSpan(16, 30, 0)));
        }

        [Test]
        public void TimeZoneId_IsEasternStandardTime()
        {
            Assert.That(SessionConfig.TimeZoneId, Is.EqualTo("Eastern Standard Time"));
        }

        [Test]
        public void GetSessionStartET_ReturnsCorrectTime()
        {
            var et = new DateTime(2024, 1, 15, 10, 0, 0);
            var sessionStart = SessionConfig.GetSessionStartET(et);
            
            Assert.That(sessionStart.Year, Is.EqualTo(2024));
            Assert.That(sessionStart.Month, Is.EqualTo(1));
            Assert.That(sessionStart.Day, Is.EqualTo(15));
            Assert.That(sessionStart.Hour, Is.EqualTo(6));
            Assert.That(sessionStart.Minute, Is.EqualTo(30));
            Assert.That(sessionStart.Second, Is.EqualTo(0));
        }

        [Test]
        public void GetSessionStartET_WithDifferentTimes_ReturnsSameSessionStart()
        {
            var et1 = new DateTime(2024, 1, 15, 6, 30, 0);
            var et2 = new DateTime(2024, 1, 15, 14, 0, 0);
            var et3 = new DateTime(2024, 1, 15, 16, 29, 59);
            
            var start1 = SessionConfig.GetSessionStartET(et1);
            var start2 = SessionConfig.GetSessionStartET(et2);
            var start3 = SessionConfig.GetSessionStartET(et3);
            
            Assert.That(start1, Is.EqualTo(start2));
            Assert.That(start2, Is.EqualTo(start3));
        }

        [Test]
        public void GetSessionEndET_ReturnsCorrectTime()
        {
            var et = new DateTime(2024, 1, 15, 10, 0, 0);
            var sessionEnd = SessionConfig.GetSessionEndET(et);
            
            Assert.That(sessionEnd.Year, Is.EqualTo(2024));
            Assert.That(sessionEnd.Month, Is.EqualTo(1));
            Assert.That(sessionEnd.Day, Is.EqualTo(15));
            Assert.That(sessionEnd.Hour, Is.EqualTo(16));
            Assert.That(sessionEnd.Minute, Is.EqualTo(30));
            Assert.That(sessionEnd.Second, Is.EqualTo(0));
        }

        [Test]
        public void GetSessionEndET_WithDifferentTimes_ReturnsSameSessionEnd()
        {
            var et1 = new DateTime(2024, 1, 15, 6, 30, 0);
            var et2 = new DateTime(2024, 1, 15, 14, 0, 0);
            var et3 = new DateTime(2024, 1, 15, 16, 29, 59);
            
            var end1 = SessionConfig.GetSessionEndET(et1);
            var end2 = SessionConfig.GetSessionEndET(et2);
            var end3 = SessionConfig.GetSessionEndET(et3);
            
            Assert.That(end1, Is.EqualTo(end2));
            Assert.That(end2, Is.EqualTo(end3));
        }

        [Test]
        public void GetSessionStartET_WithDifferentDates_ReturnsDifferentTimes()
        {
            var et1 = new DateTime(2024, 1, 15, 10, 0, 0);
            var et2 = new DateTime(2024, 1, 16, 10, 0, 0);
            
            var start1 = SessionConfig.GetSessionStartET(et1);
            var start2 = SessionConfig.GetSessionStartET(et2);
            
            Assert.That(start1, Is.Not.EqualTo(start2));
            Assert.That(start1.Day, Is.EqualTo(15));
            Assert.That(start2.Day, Is.EqualTo(16));
        }

        [Test]
        public void GetSessionEndET_WithDifferentDates_ReturnsDifferentTimes()
        {
            var et1 = new DateTime(2024, 1, 15, 10, 0, 0);
            var et2 = new DateTime(2024, 1, 16, 10, 0, 0);
            
            var end1 = SessionConfig.GetSessionEndET(et1);
            var end2 = SessionConfig.GetSessionEndET(et2);
            
            Assert.That(end1, Is.Not.EqualTo(end2));
            Assert.That(end1.Day, Is.EqualTo(15));
            Assert.That(end2.Day, Is.EqualTo(16));
        }

        [Test]
        public void SessionStart_IsBeforeSessionEnd()
        {
            var et = new DateTime(2024, 1, 15, 10, 0, 0);
            var start = SessionConfig.GetSessionStartET(et);
            var end = SessionConfig.GetSessionEndET(et);
            
            Assert.That(start, Is.LessThan(end));
        }

        [Test]
        public void SessionDuration_Is10Hours()
        {
            var et = new DateTime(2024, 1, 15, 10, 0, 0);
            var start = SessionConfig.GetSessionStartET(et);
            var end = SessionConfig.GetSessionEndET(et);
            
            var duration = end - start;
            Assert.That(duration.TotalHours, Is.EqualTo(10));
        }
    }
}

