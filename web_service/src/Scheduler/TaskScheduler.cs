/***
** Copyright (C) 2020-2023 neuRay Labs. All rights reserved.
**
** The information and source code contained herein is the exclusive 
** property of neuRay Labs and may not be disclosed, examined, reproduced, redistributed, used in source and binary forms, in whole or in part  
** without explicit written authorization from the company.
**
***/
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