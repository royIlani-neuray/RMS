/***
** Copyright (C) 2020-2024 neuRay Labs. All rights reserved.
**
** The information and source code contained herein is the exclusive 
** property of neuRay Labs and may not be disclosed, examined, reproduced, redistributed, used in source and binary forms, in whole or in part  
** without explicit written authorization from the company.
**
***/
import { Injectable } from '@angular/core';
import { Subject } from 'rxjs';
import { FrameData } from '../entities/frame-data';

export interface GateIdPrediction {
  track_id: number
  identity: string
}

export interface SmartFanGesturesPrediction {
  track_id: number
  gesture: string
}

export interface FallDetectionData {
  track_id: number
}

@Injectable()
export class RadarWebsocketService {

  private connected : boolean
  private socket : WebSocket  
  private frameDataSubject: Subject<FrameData>;
  private gateIdPredictionsSubject: Subject<GateIdPrediction[]>;
  private fanGesturesPredictionsSubject: Subject<SmartFanGesturesPrediction[]>;
  private fallDetectionSubject: Subject<FallDetectionData>;
  
  constructor() 
  { 
    this.connected = false

    this.frameDataSubject = new Subject<FrameData>();
    this.gateIdPredictionsSubject = new Subject<GateIdPrediction[]>();
    this.fanGesturesPredictionsSubject = new Subject<SmartFanGesturesPrediction[]>();
    this.fallDetectionSubject = new Subject<FallDetectionData>();
  }

  public Connect(radarId : string)
  {
    if (this.connected)
    {
      this.Disconnect()
    }
    
    console.log(`connecting to radar device: ${radarId}`)
    this.socket = new WebSocket("ws://" + window.location.host + "/websocket/ws/radars/" + radarId);
    
    this.socket.onopen = (event) => {
      console.log('Radar Websocket connection state: [Open]')
      this.connected = true
    }

    this.socket.onerror = function (event) {
      console.log('Radar Websocket connection state: [Error]')
    }

    this.socket.onclose = (event) => {
      console.log('Radar Websocket connection state: [Closed]')
      this.Disconnect()
    };

    this.socket.onmessage = (event) => {
      //console.log('Websockets Message -' + event.data)
      let message = JSON.parse(event.data)
      
      if (message['type'] == 'FRAME_DATA')
      {
        this.frameDataSubject.next(message['data'])
      }
      else if (message['type'] == 'GATE_ID_PREDICTIONS')
      {
        this.gateIdPredictionsSubject.next(message['data'])
      }
      else if (message['type'] == 'SMART_FAN_GESTURES_PREDICTIONS')
      {
        this.fanGesturesPredictionsSubject.next(message['data'])
      }
      else if (message['type'] == 'FALL_DETECTION')
      {
        this.fallDetectionSubject.next(message['data'])
      }
    }
  }

  public Disconnect()
  { 
    if (!this.connected)
      return

    // Close the connection, if open.
    if (this.socket.readyState === WebSocket.OPEN) 
    {
      this.socket.close();
    }

    this.connected = false
  }

  public IsConnected()
  {
    return this.connected
  }

  public GetFrameData() 
  {
    return this.frameDataSubject;
  }

  public GetGateIdPredictions()
  {
    return this.gateIdPredictionsSubject;
  }

  public GetFanGesturesPredictions()
  {
    return this.fanGesturesPredictionsSubject;
  }

  public GetFallDetectionData() 
  {
    return this.fallDetectionSubject;
  }

}
