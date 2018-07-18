﻿using Quartz;
using System;
using System.Threading.Tasks;

namespace Wirehome.Model.Extensions
{
    public class TriggerJob : IJob
    {
        public async Task Execute(IJobExecutionContext context)
        {
            var trigger = context.GetDataContext<TriggerJobDataDTO>();
            var time = context.FireTimeUtc.Add(trigger.FinishCommandTime.GetValueOrDefault());

            if (await trigger.Condition.Validate().ConfigureAwait(false))
            {
                await trigger.Actor.ExecuteCommand(trigger.Command).ConfigureAwait(false);
            }

            if (trigger.FinishCommand != null)
            {
                await ScheduleFinishJob(trigger, time, context.Scheduler).ConfigureAwait(false);
            }
        }

        private static Task ScheduleFinishJob(TriggerJobDataDTO trigger, DateTimeOffset time, IScheduler scheduler)
        {
            var triggerData = new TriggerJobDataDTO
            {
                Condition = trigger.Condition,
                Actor = trigger.Actor,
                Command = trigger.FinishCommand
            };

            var job = JobBuilder.Create<TriggerJob>()
                                       .WithIdentity($"{nameof(TriggerJob)}_{Guid.NewGuid()}")
                                       .SetJobData(triggerData.ToJobDataMap())
                                       .Build();

            var finishTrigger = TriggerBuilder.Create()
                                        .WithIdentity($"{nameof(TriggerJob)}_{Guid.NewGuid()}")
                                        .StartAt(time)
                                        .Build();
            return scheduler.ScheduleJob(job, finishTrigger, trigger.Token);
        }
    }
}