using System;

namespace BehaviourAPI.Core
{
    public interface IStatusHandler
    {
        Action<Status> StatusChanged { get; set; }

        Status Status { get; }

        void Start();

        void Update();

        void Stop();
    }

}