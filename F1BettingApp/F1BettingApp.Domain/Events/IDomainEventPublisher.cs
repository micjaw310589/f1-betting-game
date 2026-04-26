using System.Threading.Tasks;

namespace F1BettingApp.Domain.Events
{
    /// <summary>
    /// Interface for domain event publishing
    /// </summary>
    public interface IDomainEventPublisher
    {
        /// <summary>
        /// Publishes a domain event
        /// </summary>
        /// <typeparam name="TEvent">The type of domain event</typeparam>
        /// <param name="domainEvent">The domain event to publish</param>
        /// <returns>Task representing the asynchronous operation</returns>
        Task PublishAsync<TEvent>(TEvent domainEvent) where TEvent : IDomainEvent;

        /// <summary>
        /// Subscribes to a specific type of domain event
        /// </summary>
        /// <typeparam name="TEvent">The type of domain event</typeparam>
        /// <param name="handler">The event handler</param>
        void Subscribe<TEvent>(Func<TEvent, Task> handler) where TEvent : IDomainEvent;

        /// <summary>
        /// Unsubscribes from a specific type of domain event
        /// </summary>
        /// <typeparam name="TEvent">The type of domain event</typeparam>
        /// <param name="handler">The event handler</param>
        void Unsubscribe<TEvent>(Func<TEvent, Task> handler) where TEvent : IDomainEvent;
    }
}