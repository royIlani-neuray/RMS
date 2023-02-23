/***
** Copyright (C) 2020-2023 neuRay Labs. All rights reserved.
**
** The information and source code contained herein is the exclusive 
** property of neuRay Labs and may not be disclosed, examined, reproduced, redistributed, used in source and binary forms, in whole or in part  
** without explicit written authorization from the company.
**
***/
using WebService.RadarLogic.Tracking;

namespace WebService.RadarLogic.Tracking.Applications;

public interface ITrackingApplication 
{
    public delegate int ReadTIData(byte[] dataArray, int size);
    public FrameData GetNextFrame(ReadTIData readTIDataFunction);
}