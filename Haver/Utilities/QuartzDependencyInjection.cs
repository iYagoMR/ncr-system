using Quartz;

namespace Haver.Utilities
{
    public static class QuartzDependencyInjection
    {
        public static void AddInfrastructure(this IServiceCollection services)
        {
            services.AddQuartz(options =>
            {
                var jobKey = JobKey.Create(nameof(Perform24hJobs));
                options.AddJob<Perform24hJobs>(jobKey)
                       .AddTrigger(trigger => trigger.ForJob(jobKey)
                                                     .WithSimpleSchedule(schedule => schedule.WithIntervalInHours(24).RepeatForever()));
            });

            services.AddQuartzHostedService(options =>
            {
                options.WaitForJobsToComplete = true;
            });
        }

    }
}
