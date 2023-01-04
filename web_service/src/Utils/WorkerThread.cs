/***
** Copyright (C) 2020-2023 neuRay Labs. All rights reserved.
**
** The information and source code contained herein is the exclusive 
** property of neuRay Labs and may not be disclosed, examined, reproduced, redistributed, used in source and binary forms, in whole or in part  
** without explicit written authorization from the company.
**
***/
namespace WebService.Utils;

public abstract class WorkerThread<T> 
{
    private Queue<T> WorkQueue;
    private Task? WorkerTask;
    private bool RunWorker;
    private AutoResetEvent RunWorkerSignal;
    private int MaxCapacity;
    private object QueueLock;
    
    private string WorkerName;

    public WorkerThread(string name, int maxCapacity)
    {
        WorkerName = name;
        WorkQueue = new Queue<T>();
        RunWorker = false;
        RunWorkerSignal = new AutoResetEvent(true);
        MaxCapacity = maxCapacity;
        QueueLock = new Object();
    }

    protected void Enqueue(T workItem)
    {
        lock (QueueLock)
        {
            if (WorkQueue.Count == MaxCapacity)
            {
                // remove the oldest item from the queue in favor of the new one.
                System.Console.WriteLine($"Warning: Worker thread '{WorkerName}' is full! dropping item!");
                WorkQueue.Dequeue();
            }

            WorkQueue.Enqueue(workItem);
            RunWorkerSignal.Set();            
        }
    }

    private bool TryDequeue(out T workItem)
    {
        lock (QueueLock)
        {
            return WorkQueue.TryDequeue(out workItem!);        
        }        
    }

    public void StartWorker()
    {
        if (WorkerTask != null)
            return;

        WorkerTask = new Task(async () => 
        {
            await WorkerMainLoop();
        });

        RunWorker = true;
        RunWorkerSignal.Set();
        WorkerTask.Start();
    }

    public void StopWorker()
    {
        if (WorkerTask == null)
            return;
        
        RunWorker = false;
        RunWorkerSignal.Set();
        WorkerTask.Wait();
        WorkerTask = null;
    }

    protected abstract Task DoWork(T workItem);

    private async Task WorkerMainLoop()
    {
        while (RunWorker)
        {
            T workItem;

            if (TryDequeue(out workItem!))
            {
                try
                {
                    await DoWork(workItem); 
                }
                catch (Exception ex)
                {
                    System.Console.WriteLine("Unexpected exception in worker thread!");
                    System.Console.WriteLine(ex);
                } 
            }
            else
            {
                // wait for signal that either the quere has content or we need to exit
                RunWorkerSignal.WaitOne();
            }                
        }
    }
}
