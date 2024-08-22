/***
** Copyright (C) 2020-2024 neuRay Labs. All rights reserved.
**
** The information and source code contained herein is the exclusive 
** property of neuRay Labs and may not be disclosed, examined, reproduced, redistributed, used in source and binary forms, in whole or in part  
** without explicit written authorization from the company.
**
***/
using Serilog;

public abstract class TaskScheduler {

    private Task? SchedulerTask;
    private double SchedulingPeriodMinutes;
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

    public TaskScheduler(double schedulingPeriodMinutes)
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
                Log.Error($"Unexpected exeption in scheduler", ex);
            }

            Thread.Sleep((int)(SchedulingPeriodMinutes * 60 * 1000));
        }
    }

    public abstract void RunTask();

}