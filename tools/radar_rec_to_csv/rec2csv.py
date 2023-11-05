"""
** Copyright (C) 2020-2023 neuRay Labs. All rights reserved.
**
** The information and source code contained herein is the exclusive 
** property of neuRay Labs and may not be disclosed, examined, reproduced, redistributed, used in source and binary forms, in whole or in part  
** without explicit written authorization from the company.
**
"""
import os
import json
import datetime
import argparse
import struct

def process_frame(frameNumber, frame, tracksCsv, pointsCsv, pointsCountCsv):
    timestamp = frame['timestamp']  # '2023-11-02T16:16:51.7757134Z'
    tracks = frame['tracks']
    points = frame['points']

    if len(points) > 0:
        for point in points:
            pointsCsv.write(f"{frameNumber},{timestamp},{point['range']},{point['azimuth']},{point['elevation']},{point['doppler']},{point['position_x']},{point['position_y']},{point['position_z']}\n")

    pointsCountCsv.write(f'{frameNumber},{timestamp},{len(points)}\n')

    if len(tracks) > 0:
        # 'tracks': [{'track_id': 0, 'position_x': -0.7867653, 'position_y': 6.244761, 'position_z': 2.1098669, 'velocity_x': -0.15174803, 'velocity_y': 1.2044636, 'velocity_z': 0.021190666, 'acceleration_x': 0, 'acceleration_y': 0, 'acceleration_z': 0}]
        for track in tracks:
            tracksCsv.write(f"{frameNumber},{timestamp},{track['track_id']},{track['position_x']},{track['position_y']},{track['position_z']},{track['velocity_x']},{track['velocity_y']},{track['velocity_z']}\n")

def generate_csv(inputfile, outputDir):
    tracksCsvPath = os.path.join(outputDir, 'tracks.csv')
    pointsCsvPath = os.path.join(outputDir, 'points.csv')
    pointsCountCsvPath = os.path.join(outputDir, 'points_count.csv')

    tracksCsv = open(tracksCsvPath, mode="w")
    pointsCsv = open(pointsCsvPath, mode="w")
    pointsCountCsv = open(pointsCountCsvPath, mode="w")
    
    tracksCsv.write('frame,timestamp,track_id,position_x,position_y,position_z,velocity_x,velocity_y,velocity_z\n')
    pointsCsv.write('frame,timestamp,range,azimuth,elevation,doppler,position_x,position_y,position_z\n')
    pointsCountCsv.write('frame,timestamp,points_count\n')

    with open(inputfile, mode="rb") as recFile:
        frameNumber = 0
        frameRate = struct.unpack('f', recFile.read(4))
        #print(f"recording frame rate: {frameRate}")

        while True:
            frameNumber += 1
            sizeBytes = recFile.read(4)

            if len(sizeBytes) == 0:
                break;
            
            jsonSizeBytes = struct.unpack('i', sizeBytes)[0]
            #print(f"size bytes: {jsonSizeBytes}")

            if jsonSizeBytes == 0:
                continue  # empty frame

            jsonBytes = recFile.read(jsonSizeBytes)
            frame = json.loads(jsonBytes.decode('utf8'))
            process_frame(frameNumber, frame, tracksCsv, pointsCsv, pointsCountCsv)
    
    tracksCsv.close()
    pointsCsv.close()
    pointsCountCsv.close()



if __name__ == '__main__':
    parser = argparse.ArgumentParser()
    parser.add_argument("-i", "--inputfile", help="recording file path", required=True)
    parser.add_argument("-o", "--outputDir", help="output directory", required=False)
    args = parser.parse_args()

    inputfile = args.inputfile
    outputDir = args.outputDir

    if outputDir == None:
        outputDir = os.getcwd()

    if not os.path.exists(inputfile):
        print("Error: cannot open recording file. invalid path provided.")
    else:
        generate_csv(inputfile, outputDir)
