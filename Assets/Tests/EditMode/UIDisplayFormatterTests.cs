using NUnit.Framework;
using RadioDispatch.Calls;
using RadioDispatch.UI;
using RadioDispatch.Units;

namespace Tests.EditMode
{
    public class UIDisplayFormatterTests
    {
        [Test]
        public void BuildCallSummary_ReturnsFriendlyString()
        {
            var call = new CallData
            {
                Id = "1023",
                Title = "Domestic Disturbance",
                Location = "2nd & Willow",
                Priority = CallPriority.High,
                TimeSinceCreated = 75f
            };

            var summary = UIDisplayFormatter.BuildCallSummary(call);

            StringAssert.Contains("#1023", summary);
            StringAssert.Contains("Domestic Disturbance", summary);
            StringAssert.Contains("2nd & Willow", summary);
            StringAssert.Contains("Priority: High", summary);
            StringAssert.Contains("01:15", summary);
        }

        [Test]
        public void BuildUnitSummary_FallsBackToIdWhenNameMissing()
        {
            var unit = new Unit
            {
                Id = "21",
                DisplayName = string.Empty,
                Type = "Patrol",
                CurrentZone = "Downtown",
                Status = UnitStatus.Available
            };

            var summary = UIDisplayFormatter.BuildUnitSummary(unit);

            StringAssert.Contains("21 (Patrol)", summary);
            StringAssert.Contains("Available", summary);
            StringAssert.Contains("Downtown", summary);
        }

        [Test]
        public void BuildCallSummary_HandlesNullGracefully()
        {
            Assert.AreEqual("No call data", UIDisplayFormatter.BuildCallSummary(null));
        }

        [Test]
        public void BuildUnitSummary_HandlesNullGracefully()
        {
            Assert.AreEqual("No unit data", UIDisplayFormatter.BuildUnitSummary(null));
        }
    }
}
