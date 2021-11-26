using System;
using FluentAssertions;
using NSubstitute;
using NUnit.Framework;
using Vostok.Commons.Helpers.Observable;

namespace Vostok.Commons.Helpers.Tests.Observable
{
    [TestFixture]
    internal class CachingObservable_Tests : CachingObservable_Tests_Base
    {
        [SetUp]
        public void SetUp()
        {
            Observable = new CachingObservable<string>();
        }
    }
    [TestFixture]
    internal class LockFreeReadingCachingObservable_Tests : CachingObservable_Tests_Base
    {
        [SetUp]
        public void SetUp()
        {
            Observable = new LockFreeReadingCachingObservable<string>();
        }
    }

    internal abstract class CachingObservable_Tests_Base
    {
        protected ICachingObservable<string> Observable;

        private IObserver<string> observer1;
        private IObserver<string> observer2;
        private Exception error1;
        private Exception error2;

        [SetUp]
        public void TestSetup()
        {
            observer1 = Substitute.For<IObserver<string>>();
            observer2 = Substitute.For<IObserver<string>>();

            error1 = new Exception();
            error2 = new Exception();
        }

        [Test]
        public void Subscribe_should_do_nothing_when_nothing_has_been_observed_yet()
        {
            Observable.Subscribe(observer1);

            observer1.ReceivedCalls().Should().BeEmpty();
        }

        [Test]
        public void Subscribe_should_pass_initial_value_to_observer_immediately()
        {
            Observable = new CachingObservable<string>("initial");

            Observable.Subscribe(observer1);

            observer1.ReceivedCalls().Should().HaveCount(1);
            observer1.Received().OnNext("initial");
        }

        [Test]
        public void Subscribe_should_pass_latest_value_to_observer_immediately()
        {
            Observable.Next("1");
            Observable.Next("2");

            Observable.Subscribe(observer1);

            observer1.ReceivedCalls().Should().HaveCount(1);
            observer1.Received().OnNext("2");
        }

        [Test]
        public void Subscribe_should_pass_latest_value_to_observer_immediately_even_after_completee()
        {
            Observable.Next("1");
            Observable.Next("2");

            Observable.Complete();

            Observable.Subscribe(observer1);

            observer1.ReceivedCalls().Should().HaveCount(2);
            observer1.Received().OnNext("2");
            observer1.Received().OnCompleted();
        }

        [Test]
        public void Subscribe_should_cause_the_observer_to_receive_all_later_updates()
        {
            Observable.Subscribe(observer1);

            Observable.Next("1");
            Observable.Next("2");

            observer1.ReceivedCalls().Should().HaveCount(2);
            observer1.Received().OnNext("1");
            observer1.Received().OnNext("2");
        }

        [Test]
        public void Error_should_complete_all_current_observers()
        {
            Observable.Subscribe(observer1);
            Observable.Subscribe(observer2);

            Observable.Error(error1);

            observer1.Received().OnError(error1);
            observer2.Received().OnError(error1);
        }

        [Test]
        public void Error_should_do_nothing_when_another_error_has_been_seen_earlier()
        {
            Observable.Subscribe(observer1);
            Observable.Subscribe(observer2);

            Observable.Error(error1);
            Observable.Error(error2);

            observer1.DidNotReceive().OnError(error2);
            observer2.DidNotReceive().OnError(error2);
        }

        [Test]
        public void Error_should_cause_subsequent_next_calls_to_have_no_effect()
        {
            Observable.Subscribe(observer1);
            Observable.Subscribe(observer2);

            Observable.Error(error1);

            Observable.Next("value");

            observer1.DidNotReceive().OnNext("value");
            observer2.DidNotReceive().OnNext("value");
        }

        [Test]
        public void Error_should_prevent_any_further_subscriptions()
        {
            Observable.Error(error1);

            Observable.Subscribe(observer1);

            Observable.Next("value");

            observer1.DidNotReceive().OnNext("value");
        }

        [Test]
        public void Error_should_prevent_further_complete()
        {
            Observable.Error(error1);

            Observable.Subscribe(observer1);

            Observable.Complete();

            observer1.DidNotReceive().OnCompleted();
        }

        [Test]
        public void Error_should_cause_all_new_subscribers_to_immediately_complete_with_error()
        {
            Observable.Error(error1);

            Observable.Subscribe(observer1);

            observer1.Received().OnError(error1);
        }

        [Test]
        public void Next_should_propagate_new_values_to_all_subscribed_observers()
        {
            Observable.Subscribe(observer1);
            Observable.Subscribe(observer2);

            Observable.Next("1");

            observer1.Received().OnNext("1");
            observer2.Received().OnNext("1");
        }

        [Test]
        public void Next_should_send_nulls()
        {
            Observable.Subscribe(observer1);

            Observable.Next(null);
            Observable.Next("1");

            observer1.Received().OnNext(null);
            observer1.Received().OnNext("1");
        }

        [Test]
        public void Complete_should_complete_all_current_observers()
        {
            Observable.Subscribe(observer1);
            Observable.Subscribe(observer2);

            Observable.Complete();

            observer1.Received().OnCompleted();
            observer2.Received().OnCompleted();
        }

        [Test]
        public void Complete_should_prevent_any_further_subscriptions()
        {
            Observable.Complete();

            Observable.Subscribe(observer1);

            Observable.Next("value");

            observer1.DidNotReceive().OnNext("value");
        }

        [Test]
        public void Complete_should_prevent_any_further_events()
        {
            Observable.Subscribe(observer1);

            Observable.Complete();

            Observable.Next("value");

            observer1.DidNotReceive().OnNext("value");
        }

        [Test]
        public void Complete_should_send_value()
        {
            Observable.Subscribe(observer1);

            Observable.Complete("complete");

            Observable.Next("value");

            observer1.Received().OnNext("complete");
            observer1.DidNotReceive().OnNext("value");
        }

        [Test]
        public void Disposing_a_subscription_should_cause_the_subscriber_to_stop_getting_notifications()
        {
            Observable.Subscribe(observer1);
            Observable.Subscribe(observer2).Dispose();

            Observable.Next("1");

            observer1.Received().OnNext("1");
            observer2.DidNotReceive().OnNext("1");
        }

        [Test]
        public void Exception_in_one_of_the_observers_should_not_prevent_invocation_of_the_rest()
        {
            Observable.Subscribe(observer1);
            Observable.Subscribe(observer2);

            observer1
                .WhenForAnyArgs(o => o.OnNext(default))
                .Throw(new Exception());

            Observable.Next("1");

            observer1.Received().OnNext("1");
            observer2.Received().OnNext("1");
        }
    }
}