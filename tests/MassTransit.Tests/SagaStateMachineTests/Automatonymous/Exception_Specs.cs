﻿namespace MassTransit.Tests.SagaStateMachineTests.Automatonymous
{
    using System;
    using System.Threading.Tasks;
    using NUnit.Framework;


    [TestFixture]
    public class When_an_action_throws_an_exception
    {
        [Test]
        public void Should_capture_the_exception_message()
        {
            Assert.That(_instance.ExceptionMessage, Is.EqualTo("Boom!"));
        }

        [Test]
        public void Should_capture_the_exception_type()
        {
            Assert.That(_instance.ExceptionType, Is.EqualTo(typeof(ApplicationException)));
        }

        [Test]
        public void Should_have_called_the_async_if_block()
        {
            Assert.That(_instance.CalledThenClauseAsync, Is.True);
        }

        [Test]
        public void Should_have_called_the_async_then_block()
        {
            Assert.That(_instance.ThenAsyncShouldBeCalled, Is.True);
        }

        [Test]
        public void Should_have_called_the_exception_handler()
        {
            Assert.That(_instance.CurrentState, Is.EqualTo(_machine.Failed));
        }

        [Test]
        public void Should_have_called_the_false_async_condition_else_block()
        {
            Assert.That(_instance.ElseAsyncShouldBeCalled, Is.True);
        }

        [Test]
        public void Should_have_called_the_false_condition_else_block()
        {
            Assert.That(_instance.ElseShouldBeCalled, Is.True);
        }

        [Test]
        public void Should_have_called_the_first_action()
        {
            Assert.That(_instance.Called, Is.True);
        }

        [Test]
        public void Should_have_called_the_first_if_block()
        {
            Assert.That(_instance.CalledThenClause, Is.True);
        }

        [Test]
        public void Should_not_have_called_the_false_async_condition_then_block()
        {
            Assert.That(_instance.ThenAsyncShouldNotBeCalled, Is.False);
        }

        [Test]
        public void Should_not_have_called_the_false_condition_then_block()
        {
            Assert.That(_instance.ThenShouldNotBeCalled, Is.False);
        }

        [Test]
        public void Should_not_have_called_the_regular_exception()
        {
            Assert.That(_instance.ShouldNotBeCalled, Is.False);
        }

        [Test]
        public void Should_not_have_called_the_second_action()
        {
            Assert.That(_instance.NotCalled, Is.True);
        }

        Instance _instance;
        InstanceStateMachine _machine;

        [OneTimeSetUp]
        public void Specifying_an_event_activity()
        {
            _instance = new Instance();
            _machine = new InstanceStateMachine();

            _machine.RaiseEvent(_instance, _machine.Initialized).Wait();
        }


        class Instance :
            SagaStateMachineInstance
        {
            public Instance()
            {
                NotCalled = true;
            }

            public bool Called { get; set; }
            public bool NotCalled { get; set; }
            public Type ExceptionType { get; set; }
            public string ExceptionMessage { get; set; }
            public State CurrentState { get; set; }

            public bool ShouldNotBeCalled { get; set; }

            public bool CalledThenClause { get; set; }
            public bool CalledThenClauseAsync { get; set; }

            public bool ThenShouldNotBeCalled { get; set; }
            public bool ElseShouldBeCalled { get; set; }
            public bool ThenAsyncShouldNotBeCalled { get; set; }
            public bool ElseAsyncShouldBeCalled { get; set; }

            public bool ThenAsyncShouldBeCalled { get; set; }
            public Guid CorrelationId { get; set; }
        }


        class InstanceStateMachine :
            MassTransitStateMachine<Instance>
        {
            public InstanceStateMachine()
            {
                During(Initial,
                    When(Initialized)
                        .Then(context => context.Instance.Called = true)
                        .Then(_ =>
                        {
                            throw new ApplicationException("Boom!");
                        })
                        .Then(context => context.Instance.NotCalled = false)
                        .Catch<ApplicationException>(ex => ex
                            .If(context => true, b => b
                                .Then(context => context.Instance.CalledThenClause = true)
                            )
                            .IfAsync(context => Task.FromResult(true), b => b
                                .Then(context => context.Instance.CalledThenClauseAsync = true)
                            )
                            .IfElse(context => false,
                                b => b.Then(context => context.Instance.ThenShouldNotBeCalled = true),
                                b => b.Then(context => context.Instance.ElseShouldBeCalled = true)
                            )
                            .IfElseAsync(context => Task.FromResult(false),
                                b => b.Then(context => context.Instance.ThenAsyncShouldNotBeCalled = true),
                                b => b.Then(context => context.Instance.ElseAsyncShouldBeCalled = true)
                            )
                            .Then(context =>
                            {
                                context.Instance.ExceptionMessage = context.Exception.Message;
                                context.Instance.ExceptionType = context.Exception.GetType();
                            })
                            .ThenAsync(context =>
                            {
                                context.Instance.ThenAsyncShouldBeCalled = true;
                                return Task.CompletedTask;
                            })
                            .TransitionTo(Failed))
                        .Catch<Exception>(ex => ex
                            .Then(context => context.Instance.ShouldNotBeCalled = true)));
            }

            public State Failed { get; private set; }

            public Event Initialized { get; private set; }
        }
    }


    [TestFixture]
    public class When_the_exception_does_not_match_the_type
    {
        [Test]
        public void Should_capture_the_exception_message()
        {
            Assert.That(_instance.ExceptionMessage, Is.EqualTo("Boom!"));
        }

        [Test]
        public void Should_capture_the_exception_type()
        {
            Assert.That(_instance.ExceptionType, Is.EqualTo(typeof(ApplicationException)));
        }

        [Test]
        public void Should_have_called_the_exception_handler()
        {
            Assert.That(_instance.CurrentState, Is.EqualTo(_machine.Failed));
        }

        [Test]
        public void Should_have_called_the_first_action()
        {
            Assert.That(_instance.Called, Is.True);
        }

        [Test]
        public void Should_not_have_called_the_second_action()
        {
            Assert.That(_instance.NotCalled, Is.True);
        }

        Instance _instance;
        InstanceStateMachine _machine;

        [OneTimeSetUp]
        public void Specifying_an_event_activity()
        {
            _instance = new Instance();
            _machine = new InstanceStateMachine();

            _machine.RaiseEvent(_instance, _machine.Initialized).Wait();
        }


        class Instance :
            SagaStateMachineInstance
        {
            public Instance()
            {
                NotCalled = true;
            }

            public bool Called { get; set; }
            public bool NotCalled { get; set; }
            public Type ExceptionType { get; set; }
            public string ExceptionMessage { get; set; }
            public State CurrentState { get; set; }
            public Guid CorrelationId { get; set; }
        }


        class InstanceStateMachine :
            MassTransitStateMachine<Instance>
        {
            public InstanceStateMachine()
            {
                InstanceState(x => x.CurrentState);

                During(Initial,
                    When(Initialized)
                        .Then(context => context.Instance.Called = true)
                        .Then(_ => throw new ApplicationException("Boom!"))
                        .Then(context => context.Instance.NotCalled = false)
                        .Catch<Exception>(ex => ex
                            .Then(context =>
                            {
                                context.Instance.ExceptionMessage = context.Exception.Message;
                                context.Instance.ExceptionType = context.Exception.GetType();
                            })
                            .TransitionTo(Failed)));
            }

            public State Failed { get; private set; }

            public Event Initialized { get; private set; }
        }
    }


    [TestFixture]
    public class When_the_exception_is_caught
    {
        [Test]
        public void Should_have_called_the_subsequent_action()
        {
            Assert.That(_instance.Called, Is.True);
        }

        Instance _instance;
        InstanceStateMachine _machine;

        [OneTimeSetUp]
        public void Specifying_an_event_activity()
        {
            _instance = new Instance();
            _machine = new InstanceStateMachine();

            _machine.RaiseEvent(_instance, _machine.Initialized).Wait();
        }


        class Instance :
            SagaStateMachineInstance
        {
            public bool Called { get; set; }
            public State CurrentState { get; set; }
            public Guid CorrelationId { get; set; }
        }


        class InstanceStateMachine :
            MassTransitStateMachine<Instance>
        {
            public InstanceStateMachine()
            {
                InstanceState(x => x.CurrentState);

                During(Initial,
                    When(Initialized)
                        .Then(_ =>
                        {
                            throw new ApplicationException("Boom!");
                        })
                        .Catch<Exception>(ex => ex)
                        .Then(context => context.Instance.Called = true));
            }

            public State Failed { get; private set; }
            public Event Initialized { get; private set; }
        }
    }


    [TestFixture]
    public class When_the_exception_is_caught_within_an_else
    {
        [Test]
        public void Should_have_called_the_subsequent_action()
        {
            Assert.That(_machine.Failed, Is.EqualTo(_instance.CurrentState));
        }

        Instance _instance;
        InstanceStateMachine _machine;

        [OneTimeSetUp]
        public async Task Specifying_an_event_activity()
        {
            _instance = new Instance();
            _machine = new InstanceStateMachine();

            await _machine.RaiseEvent(_instance, _machine.Initialized);
        }


        class Instance :
            SagaStateMachineInstance
        {
            public bool Called { get; set; }
            public State CurrentState { get; set; }
            public Guid CorrelationId { get; set; }
        }


        class InstanceStateMachine :
            MassTransitStateMachine<Instance>
        {
            public InstanceStateMachine()
            {
                InstanceState(x => x.CurrentState);

                During(Initial,
                    When(Initialized)
                        .IfElse(x => false, then => then.TransitionTo(Completed),
                            @else => @else
                                .Then(_ => throw new ApplicationException("Boom!"))
                                .TransitionTo(NotCompleted)
                                .Catch<Exception>(ex => ex.TransitionTo(Failed))
                        )
                );
            }

            public State Completed { get; private set; }
            public State NotCompleted { get; private set; }
            public State Failed { get; private set; }
            public Event Initialized { get; private set; }
        }
    }


    [TestFixture]
    public class When_an_action_throws_an_exception_on_data_events
    {
        [Test]
        public void Should_capture_the_exception_message()
        {
            Assert.That(_instance.ExceptionMessage, Is.EqualTo("Boom!"));
        }

        [Test]
        public void Should_capture_the_exception_type()
        {
            Assert.That(_instance.ExceptionType, Is.EqualTo(typeof(ApplicationException)));
        }

        [Test]
        public void Should_have_called_the_async_if_block()
        {
            Assert.That(_instance.CalledSecondThenClause, Is.True);
        }

        [Test]
        public void Should_have_called_the_exception_handler()
        {
            Assert.That(_instance.CurrentState, Is.EqualTo(_machine.Failed));
        }

        [Test]
        public void Should_have_called_the_false_async_condition_else_block()
        {
            Assert.That(_instance.ElseAsyncShouldBeCalled, Is.True);
        }

        [Test]
        public void Should_have_called_the_false_condition_else_block()
        {
            Assert.That(_instance.ElseShouldBeCalled, Is.True);
        }

        [Test]
        public void Should_have_called_the_first_action()
        {
            Assert.That(_instance.Called, Is.True);
        }

        [Test]
        public void Should_have_called_the_first_if_block()
        {
            Assert.That(_instance.CalledThenClause, Is.True);
        }

        [Test]
        public void Should_not_have_called_the_false_async_condition_then_block()
        {
            Assert.That(_instance.ThenAsyncShouldNotBeCalled, Is.False);
        }

        [Test]
        public void Should_not_have_called_the_false_condition_then_block()
        {
            Assert.That(_instance.ThenShouldNotBeCalled, Is.False);
        }

        [Test]
        public void Should_not_have_called_the_second_action()
        {
            Assert.That(_instance.NotCalled, Is.True);
        }

        Instance _instance;
        InstanceStateMachine _machine;

        [OneTimeSetUp]
        public void Specifying_an_event_activity()
        {
            _instance = new Instance();
            _machine = new InstanceStateMachine();

            _machine.RaiseEvent(_instance, _machine.Initialized, new Init()).Wait();
        }


        class Instance :
            SagaStateMachineInstance
        {
            public Instance()
            {
                NotCalled = true;
            }

            public bool Called { get; set; }
            public bool NotCalled { get; set; }
            public Type ExceptionType { get; set; }
            public string ExceptionMessage { get; set; }
            public State CurrentState { get; set; }

            public bool CalledThenClause { get; set; }
            public bool CalledSecondThenClause { get; set; }

            public bool ThenShouldNotBeCalled { get; set; }
            public bool ElseShouldBeCalled { get; set; }
            public bool ThenAsyncShouldNotBeCalled { get; set; }
            public bool ElseAsyncShouldBeCalled { get; set; }
            public Guid CorrelationId { get; set; }
        }


        class Init
        {
        }


        class InstanceStateMachine :
            MassTransitStateMachine<Instance>
        {
            public InstanceStateMachine()
            {
                InstanceState(x => x.CurrentState);

                During(Initial,
                    When(Initialized)
                        .Then(context => context.Instance.Called = true)
                        .Then(_ => throw new ApplicationException("Boom!"))
                        .Then(context => context.Instance.NotCalled = false)
                        .Catch<Exception>(ex => ex
                            .If(context => true, b => b
                                .Then(context => context.Instance.CalledThenClause = true)
                            )
                            .IfAsync(context => Task.FromResult(true), b => b
                                .Then(context => context.Instance.CalledSecondThenClause = true)
                            )
                            .IfElse(context => false,
                                b => b.Then(context => context.Instance.ThenShouldNotBeCalled = true),
                                b => b.Then(context => context.Instance.ElseShouldBeCalled = true)
                            )
                            .IfElseAsync(context => Task.FromResult(false),
                                b => b.Then(context => context.Instance.ThenAsyncShouldNotBeCalled = true),
                                b => b.Then(context => context.Instance.ElseAsyncShouldBeCalled = true)
                            )
                            .Then(context =>
                            {
                                context.Instance.ExceptionMessage = context.Exception.Message;
                                context.Instance.ExceptionType = context.Exception.GetType();
                            })
                            .TransitionTo(Failed)));
            }

            public State Failed { get; private set; }

            public Event<Init> Initialized { get; private set; }
        }
    }


    [TestFixture]
    public class When_an_action_throws_an_exception_and_catches_it
    {
        [Test]
        public void Should_finalize_in_catch_block()
        {
            Assert.That(_instance.CurrentState, Is.EqualTo(_machine.Final));
        }

        Instance _instance;
        InstanceStateMachine _machine;

        [OneTimeSetUp]
        public async Task Specifying_an_event_activity()
        {
            _instance = new Instance();
            _machine = new InstanceStateMachine();

            await _machine.RaiseEvent(_instance, _machine.Initialized, new Init());
        }


        class Instance :
            SagaStateMachineInstance
        {
            public State CurrentState { get; set; }
            public Guid CorrelationId { get; set; }
        }


        class Init
        {
        }


        class InstanceStateMachine :
            MassTransitStateMachine<Instance>
        {
            public InstanceStateMachine()
            {
                InstanceState(x => x.CurrentState);

                During(Initial,
                    When(Initialized)
                        .Then(_ => throw new ApplicationException("Boom!"))
                        .Catch<Exception>(ex => ex
                            .Finalize()));
            }

            public Event<Init> Initialized { get; private set; }
        }
    }
}
