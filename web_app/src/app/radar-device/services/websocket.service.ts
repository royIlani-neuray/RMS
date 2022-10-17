import { Injectable } from '@angular/core';
import { Observable, Subscriber } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class WebsocketService {

  private socket : WebSocket  

  constructor() 
  { 
    this.socket = new WebSocket('ws://localhost:16500')
    this.socket.onopen = function (event) {
      console.log('Websockets connection state: [Open]')
    }

    this.socket.onerror = function (event) {
      console.log('Websockets connection state: [Error]')
    }

    this.socket.onclose = function (event) {
      console.log('Websockets connection state: [Closed]')
    };

  }

  public GetFrameData() 
  {
    return new Observable((subscriber) => 
    {
      this.socket.onmessage = function (event) {
        //console.log('Websockets Message -' + event.data)
        let frameData = JSON.parse(event.data)
        subscriber.next(frameData);
      }
    });
  }
}
