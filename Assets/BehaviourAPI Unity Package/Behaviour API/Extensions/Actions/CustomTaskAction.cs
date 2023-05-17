using UnityEngine;

namespace BehaviourAPI.UnityExtensions
{
    using Core;

    /// <summary>
    /// Action to perform a custom task defined by the agent.
    /// </summary>
    public class CustomTaskAction : UnityAction
    {
        /// <summary>
        /// The name of the task
        /// </summary>
        public string taskName;

        string taskId;

        public override string DisplayInfo => "Execute task \"$taskName\"";

        /// <summary>
        /// Create a CustomTaskAction
        /// </summary>
        /// <param name="taskName">The name of the task that this action will perform.</param>
        public CustomTaskAction(string taskName)
        {
            this.taskName = taskName;
        }

        public override void Start()
        {
            taskId = context.CustomTasks.CreateTask(taskName);
            if (taskId != null)
            {
                if (!context.CustomTasks.StartTask(taskId))
                {
                    taskId = null;
                }
            }
        }

        public override Status Update()
        {
            if (taskId != null)
            {
                var status = context.CustomTasks.GetTaskStatus(taskId);
                return status;
            }
            else
            {
                return Status.Failure;
            }
        }

        public override void Pause()
        {
            context.CustomTasks.PauseTask(taskId);
        }

        public override void Stop()
        {
            context.CustomTasks.CancelTask(taskId);
        }

        public override void Unpause()
        {
            context.CustomTasks.UnpauseTask(taskId);
        }
    }
}