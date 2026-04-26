using F1BettingApp.Domain.Events;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace F1BettingApp.Infrastructure.Events
{
    /// <summary>
    /// Implementation of domain event publisher
    /// </summary>
    public class DomainEventPublisher : IDomainEventPublisher
    {
        private readonly Dictionary<Type, List<Func<IDomainEvent, Task>>> _handlers = new();

        /// <summary>
        /// Publishes a domain event to all subscribers
        /// </summary>
        /// <typeparam name="TEvent">The type of domain event</typeparam>
        /// <param name="domainEvent">The domain event to publish</param>
        /// <returns>Task representing the asynchronous operation</returns>
        public async Task PublishAsync<TEvent>(TEvent domainEvent) where TEvent : IDomainEvent
        {
            if (domainEvent == null)
                throw new ArgumentNullException(nameof(domainEvent));

            var eventType = typeof(TEvent);

            if (_handlers.TryGetValue(eventType, out var handlers))
            {
                foreach (var handler in handlers)
                {
                    try
                    {
                        await handler(domainEvent);
                    }
                    catch (Exception ex)
                    {
                        // Log error but don't stop other handlers
                        Console.Error.WriteLine($"Error handling domain event {eventType.Name}: {ex.Message}");
                    }
                }
            }
        }

        /// <summary>
        /// Subscribes to a specific type of domain event
        /// </summary>
        /// <typeparam name="TEvent">The type of domain event</typeparam>
        /// <param name="handler">The event handler</param>
        public void Subscribe<TEvent>(Func<TEvent, Task> handler) where TEvent : IDomainEvent
        {
            if (handler == null)
                throw new ArgumentNullException(nameof(handler));

            var eventType = typeof(TEvent);

            if (!_handlers.ContainsKey(eventType))
            {
                _handlers[eventType] = new List<Func<IDomainEvent, Task>>();
            }

            _handlers[eventType].Add(e => handler((TEvent)e));
        }

        /// <summary>
        /// Unsubscribes from a specific type of domain event
        /// </summary>
        /// <typeparam name="TEvent">The type of domain event</typeparam>
        /// <param name="handler">The event handler</param>
        public void Unsubscribe<TEvent>(Func<TEvent, Task> handler) where TEvent : IDomainEvent
        {
            if (handler == null)
                throw new ArgumentNullException(nameof(handler));

            var eventType = typeof(TEvent);

            if (_handlers.TryGetValue(eventType, out var handlers))
            {
                var wrapper = new Func<IDomainEvent, Task>(e => handler((TEvent)e));
                handlers.Remove(wrapper);
            }
        }
    }
}