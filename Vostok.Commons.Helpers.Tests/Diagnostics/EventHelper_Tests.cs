#if NET5_0 || NET6_0
using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Linq;
using System.Threading;
using FluentAssertions;
using FluentAssertions.Extensions;
using NUnit.Framework;
using Vostok.Commons.Helpers.Diagnostics;
using Vostok.Commons.Testing;

namespace Vostok.Commons.Helpers.Tests.Diagnostics;

[TestFixture]
internal class EventHelper_Tests
{
    [Test]
    public void Should_get_polling_counter_value()
    {
        var args = new List<EventWrittenEventArgs>();
        using var source = new TestEventSource(true, false);
        using var listener = new TestListener(args);

        Action waitAction = () => args.Should().NotBeEmpty();
        waitAction.ShouldPassIn(10.Seconds());
        var eventData = args.First();

        EventHelper.TryGetCounterPayload(eventData, out var payload);
        EventHelper.TryGetMeanCounterValue(payload, out var mean);

        mean.Should().Be(TestEventSource.PollingCounterValue);
    }

    [Test]
    public void Should_get_incrementing_counter_value()
    {
        var args = new List<EventWrittenEventArgs>();
        using var source = new TestEventSource(false, true);
        using var listener = new TestListener(args);

        Action waitAction = () => args.Should().NotBeEmpty();
        waitAction.ShouldPassIn(10.Seconds());
        var eventData = args.Last();

        EventHelper.TryGetCounterPayload(eventData, out var payload);
        EventHelper.TryGetIncrementingCounterValue(payload, out var value);

        value.Should().BePositive();
    }

    [Test]
    public void Should_get_mean_and_incrementing_counter_value()
    {
        var args = new List<EventWrittenEventArgs>();
        using var source = new TestEventSource(true, true);
        using var listener = new TestListener(args);

        Action waitAction = () => args.Should().NotBeEmpty();
        waitAction.ShouldPassIn(10.Seconds());

        var meanValues = new List<long>();
        var incrementingValues = new List<long>();
        foreach (var eventData in args.ToList())
        {
            if (EventHelper.TryGetCounterValue(eventData, TestEventSource.PollingCounterName, out var value))
                meanValues.Add(value);
            if (EventHelper.TryGetCounterValue(eventData, TestEventSource.IncrementingPollingCounterName, out value))
                incrementingValues.Add(value);
        }

        meanValues.Should().NotBeEmpty();
        incrementingValues.Should().NotBeEmpty();
    }

    private class TestListener : EventListener
    {
        private readonly List<EventWrittenEventArgs> args;

        public TestListener(List<EventWrittenEventArgs> args)
        {
            this.args = args;
        }

        protected override void OnEventSourceCreated(EventSource eventSource)
        {
            if (eventSource.Name == "TestSource")
                EnableEvents(eventSource,
                    EventLevel.Verbose,
                    EventKeywords.All,
                    new Dictionary<string, string>
                    {
                        {"EventCounterIntervalSec", "1"}
                    });
        }

        protected override void OnEventWritten(EventWrittenEventArgs eventData)
        {
            if (eventData.EventId != -1)
                return;

            args.Add(eventData);
        }
    }

    [EventSource(Name = "TestSource")]
    private class TestEventSource : EventSource
    {
        public const long PollingCounterValue = 123;
        public const string PollingCounterName = "pollingCounter";
        public const string IncrementingPollingCounterName = "incrementingPollingCounter";

        private readonly bool enablePollingCounter;
        private readonly bool enableIncrementingPollingCounter;

        private PollingCounter pollingCounter;

        private IncrementingPollingCounter incrementingPollingCounter;
        private long incrementingPollingCounterValue = 321;

        public TestEventSource(bool enablePollingCounter, bool enableIncrementingPollingCounter)
        {
            this.enablePollingCounter = enablePollingCounter;
            this.enableIncrementingPollingCounter = enableIncrementingPollingCounter;
        }

        protected override void OnEventCommand(EventCommandEventArgs command)
        {
            if (command.Command == EventCommand.Enable)
            {
                if (enablePollingCounter)
                    pollingCounter ??= new PollingCounter(PollingCounterName, this, () => PollingCounterValue)
                    {
                        DisplayName = "Polling Counter"
                    };

                if (enableIncrementingPollingCounter)
                    incrementingPollingCounter ??= new IncrementingPollingCounter(IncrementingPollingCounterName, this, () => Interlocked.Increment(ref incrementingPollingCounterValue))
                    {
                        DisplayName = "Incrementing Polling Counter",
                        DisplayRateTimeScale = TimeSpan.FromSeconds(1)
                    };
            }
        }
    }
}
#endif