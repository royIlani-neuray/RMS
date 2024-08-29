/***
** Copyright (C) 2020-2024 neuRay Labs. All rights reserved.
**
** The information and source code contained herein is the exclusive 
** property of neuRay Labs and may not be disclosed, examined, reproduced, redistributed, used in source and binary forms, in whole or in part  
** without explicit written authorization from the company.
**
***/
using WebService.Entites;
using WebService.RadarLogic.Tracking;

namespace WebService.Services.LineCrossing;

public class TrackLineCrossingManager
{
    private Radar radar;
    private readonly List<LineSettings> lines;
    private readonly Dictionary<uint, List<LineCrossingDetector>> trackDetectors;

    public TrackLineCrossingManager(Radar radar, List<LineSettings> lineSettingsList)
    {
        this.lines = lineSettingsList;
        trackDetectors = [];
        this.radar = radar;
    }

    public void UpdateTracks(FrameData frame)
    {
        var currentTrackIds = new HashSet<uint>(frame.TracksList.ConvertAll(track => track.TrackId));

        // Remove detectors for tracks that are no longer present
        foreach (var trackId in new List<uint>(trackDetectors.Keys))
        {
            if (!currentTrackIds.Contains(trackId))
            {
                trackDetectors.Remove(trackId);
            }
        }

        // Add detectors for new tracks and update positions
        foreach (var track in frame.TracksList)
        {
            if (!trackDetectors.ContainsKey(track.TrackId))
            {
                // Initialize detectors for each line for this track
                trackDetectors[track.TrackId] = [];
                foreach (var line in lines)
                {
                    if (line.IsEnabled)
                    {
                        trackDetectors[track.TrackId].Add(new LineCrossingDetector(line));
                    }
                }
            }

            // Run detector for each line for this track
            foreach (var detector in trackDetectors[track.TrackId])
            {
                var result = detector.CheckForCrossing((track.PositionX, track.PositionY));
                
                if (result != null)
                {
                    radar.Log.Debug($"Track {track.TrackId} crossed the line '{detector.Line.Name}', direction: {result}");

                    radar.RadarWebSocket.SendLineCrossingEvent(new 
                    {
                        radar_id = radar.Id,
                        track_id = track.TrackId,
                        line_name = detector.Line.Name,
                        direction = result
                    });
                }
            }
        }
    }
}