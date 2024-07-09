/***
** Copyright (C) 2020-2024 neuRay Labs. All rights reserved.
**
** The information and source code contained herein is the exclusive 
** property of neuRay Labs and may not be disclosed, examined, reproduced, redistributed, used in source and binary forms, in whole or in part  
** without explicit written authorization from the company.
**
***/
namespace WebService.RadarLogic.Tracking.Applications;

public static class TrackingApplicationUtils
{
    public static void CalcCartesianFromSpherical(FrameData.Point point)
    {
        point.PositionX = (float) (point.Range * Math.Sin(point.Azimuth) * Math.Cos(point.Elevation));
        point.PositionY = (float) (point.Range * Math.Cos(point.Azimuth) * Math.Cos(point.Elevation));
        point.PositionZ = (float) (point.Range * Math.Sin(point.Elevation));
    }

    public static void RotatePoint(float radarAzimuthDegrees, float radarElevationDegrees, float x, float y, float z, out float rotatedX, out float rotatedY, out float rotatedZ)
    {
        double azimuth = (Math.PI / 180) * radarAzimuthDegrees; 
        double elevation = (Math.PI / 180) * radarElevationDegrees; 

        double[,] rotationMatrix = new double[3, 3] { { Math.Cos(azimuth), Math.Cos(elevation) * Math.Sin(azimuth), Math.Sin(elevation) * Math.Sin(azimuth) }, 
                                                      { -Math.Sin(azimuth), Math.Cos(elevation) * Math.Cos(azimuth), Math.Sin(elevation) * Math.Cos(azimuth) }, 
                                                      { 0, -Math.Sin(elevation),  Math.Cos(elevation) } };
        
        rotatedX = (float) (x * rotationMatrix[0,0] + y * rotationMatrix[0,1] + z * rotationMatrix[0,2]);
        rotatedY = (float) (x * rotationMatrix[1,0] + y * rotationMatrix[1,1] + z * rotationMatrix[1,2]);
        rotatedZ = (float) (x * rotationMatrix[2,0] + y * rotationMatrix[2,1] + z * rotationMatrix[2,2]);

        // System.Console.WriteLine($"Point XYZ = [{x}, {y}, {z}], Rotation: [{rotatedX}, {rotatedY}, {rotatedZ}]");   
    }
}