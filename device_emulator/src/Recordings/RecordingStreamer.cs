/***
** Copyright (C) 2020-2023 neuRay Labs. All rights reserved.
**
** The information and source code contained herein is the exclusive 
** property of neuRay Labs and may not be disclosed, examined, reproduced, redistributed, used in source and binary forms, in whole or in part  
** without explicit written authorization from the company.
**
***/
using System.Net.Sockets;

namespace DeviceEmulator.Recordings;

public class RecordingStreamer {

    #region Singleton
    
    private static object singletonLock = new object();
    private static volatile RecordingStreamer? instance; 

    public static RecordingStreamer Instance {
        get 
        {
            if (instance == null)
            {
                lock (singletonLock)
                {
                    if (instance == null)
                        instance = new RecordingStreamer();
                }
            }

            return instance;
        }
    }

    private RecordingStreamer() 
    {
        streamFrames = false;
    }

    #endregion

    private const float IDLE_STREAMING_RATE = 5; // 5 fps
    private const string RecordingPath = "./data/recordings";

    private bool streamFrames;
    private Task? streamingTask;

    private NetworkStream? dataStream;

    private float streamRate = IDLE_STREAMING_RATE;

    private RecordingDataReader? reader;
    private bool loopForever;

    private object readerLock = new Object();

    public void SetRecordingSource(string filePath, bool loopForeaver)
    {        
        lock (readerLock)
        {
            reader = new RecordingDataReader(filePath);
            streamRate = reader.FrameRate;
            this.loopForever = loopForeaver;
        }  
    }

    public void SetDataStream(NetworkStream dataStream)
    {
        this.dataStream = dataStream;
    }

    public void StartStreaming()
    {
        if (streamingTask != null)
            return;

        streamFrames = true;
        streamingTask = Task.Run(() => StreamData());
    }

    public void StopStreaming()
    {
        if (streamingTask == null)
            return;

        streamFrames = false;
        streamingTask.Wait();
        streamingTask = null;
    }

    public void StreamData()
    {
        while (streamFrames)
        {
            try
            {
                DateTime start = DateTime.Now;

                if (reader == null)
                {
                    uint frameSize = 0; // tell RMS to generate an empty frame
                    dataStream!.Write(BitConverter.GetBytes(frameSize));
                }
                else
                {
                    lock (readerLock)
                    {
                        byte[]? frameBytes = null;

                        bool gotFrame = reader.GetNextFrame(out frameBytes);

                        if (!gotFrame)
                        {
                            if (loopForever)
                            {
                                System.Console.WriteLine("Got to end of recording. rewinding...");
                                reader.Rewind();
                                reader.GetNextFrame(out frameBytes);
                            }
                            else
                            {
                                System.Console.WriteLine("Got to end of recording.");
                                reader = null;
                                continue;
                            }
                        }

                        if (frameBytes != null)
                        {
                            uint frameSize = (uint) frameBytes!.Length;
                            dataStream!.Write(BitConverter.GetBytes(frameSize));
                            dataStream!.Write(frameBytes);
                        }
                        else
                        {
                            uint frameSize = 0; // tell RMS to generate an empty frame
                            dataStream!.Write(BitConverter.GetBytes(frameSize));
                        }
                        
                    }
                }
                
                DateTime end = DateTime.Now;
                TimeSpan ts = end - start;
                int timeSlice = (int) ((1000 / streamRate) - ts.TotalMilliseconds);
                Thread.Sleep(timeSlice);
            }
            catch (Exception ex)
            {
                System.Console.WriteLine("Excpetion in data streamer... error: " + ex);
                break;
            }
        }

    }
}