public abstract class TaskScheduler {

    private Task? SchedulerTask;
    private int SchedulingPeriodMinutes;
    private bool runScheduler = false;

    public void Start()
    {
        if (SchedulerTask != null)
            return;
        
        SchedulerTask = new Task(() => 
        {
            SchedulingLoop();
        });

        runScheduler = true;
        SchedulerTask.Start();
    }

    public TaskScheduler(int schedulingPeriodMinutes)
    {
        this.SchedulingPeriodMinutes = schedulingPeriodMinutes;
    }

    private void SchedulingLoop()
    {
        while (runScheduler)
        {
            try
            {
                RunTask();
            }
            catch (Exception ex)
            {
                System.Console.WriteLine($"Unexpected exeption in scheduler: {ex.Message}");
            }

            Thread.Sleep(SchedulingPeriodMinutes * 60 * 1000);
        }
    }

    public abstract void RunTask();

}