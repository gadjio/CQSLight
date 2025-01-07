using System;
using Newtonsoft.Json;
using PGMS.CQSLight.Extensions;

namespace PGMS.CQSLight.Infra.Commands
{


    public interface IEvent : IDomainMessage
    {
        Guid AggregateId { get; set; }
        Guid Id { get; }

        string ByUserId { get; set; }
        string ByUsername { get; set; }

        string CommandType { get; set; }
	}

    public interface IDomainEvent : IEvent
    {
	    long Timestamp { get; set; }

	    string GetJSonSerialisation();
    }

    public abstract class DomainEvent<T> : Event, IEquatable<DomainEvent<T>>, IDomainEvent
    {
        protected DomainEvent(T parameters)
        {
            Parameters = parameters;
        }

        public T Parameters { get; set; }

        public string GetJSonSerialisation()
        {
            return JsonConvert.SerializeObject(Parameters);
        }

        public bool Equals(DomainEvent<T> other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return base.Equals(other) && Equals(Parameters, other.Parameters);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((DomainEvent<T>)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (base.GetHashCode() * 397) ^ (Parameters != null ? Parameters.GetHashCode() : 0);
            }
        }
    }

    [Serializable]
    public abstract class Event : IEvent, IEquatable<Event>
    {
        protected Event()
        {
            Id = Guid.NewGuid();
            Timestamp = DateTime.Now.ToEpoch();
        }

        public Guid Id { get; private set; }

        public string ByUserId { get; set; }
        public string ByUsername { get; set; }

        public string CommandType { get; set; }

        public long Timestamp { get; set; }

        public Guid AggregateId { get; set; }

        public bool Equals(Event other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return other.Id.Equals(Id);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj is Event) return Equals((Event)obj);
            return false;
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }
    }
}