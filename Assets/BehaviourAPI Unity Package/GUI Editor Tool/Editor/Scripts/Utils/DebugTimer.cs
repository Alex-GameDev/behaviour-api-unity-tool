using System;

namespace BehaviourAPI.Unity.Editor
{
    public class DebugTimer : IDisposable
    {
        DateTime m_StartTime;
        public DebugTimer()
        {
            m_StartTime = DateTime.Now;
        }

        public void Dispose()
        {
            //Debug.Log((DateTime.Now - m_StartTime).TotalMilliseconds);
        }
    }
}
