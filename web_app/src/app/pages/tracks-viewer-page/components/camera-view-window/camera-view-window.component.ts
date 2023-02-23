import { AfterViewInit, Component, ElementRef, OnInit, ViewChild } from '@angular/core';
import { Router } from '@angular/router';
import { Camera } from 'src/app/entities/camera';
import { CameraWebsocketService } from 'src/app/services/camera-websocket.service';
import { CamerasService } from 'src/app/services/cameras.service';
import VideoConverter from 'h264-converter';
import { setLogger } from 'h264-converter';

@Component({
  selector: 'app-camera-view-window',
  templateUrl: './camera-view-window.component.html',
  styleUrls: ['./camera-view-window.component.css'],
  providers: [CameraWebsocketService]
})
export class CameraViewWindowComponent implements OnInit, AfterViewInit {

  constructor(private camerasService : CamerasService, 
              private cameraWebsocket : CameraWebsocketService,
              private router : Router) { }

  @ViewChild('video') private videoRef : ElementRef;

  camera : Camera | null
  frameDataSubscription! : any
  streamingStarted = false

  converter : VideoConverter

  ngOnInit(): void {
  }

  ngAfterViewInit(): void {
  }

  convertAndAddFrame(base64Frame : string) {
    var dataUrl = "data:application/octet-binary;base64," + base64Frame;

    fetch(dataUrl)
      .then(res => res.arrayBuffer())
      .then(buffer => {
        let frameBytes = new Uint8Array(buffer)
        this.converter.appendRawData(frameBytes)
      })
  }

  setCamera(cameraId : string)
  {
    if (this.frameDataSubscription != null)
    {
      this.frameDataSubscription.unsubscribe()
      this.frameDataSubscription = null
    }

    // request the camera device info based on the given camera id
    this.camerasService.getCamera(cameraId).subscribe({
      next : (camera) => {
        this.camera = camera
        
        this.cameraWebsocket.Connect(cameraId)
        
        //setLogger(console.log, console.error);
        this.converter = new VideoConverter(this.videoRef.nativeElement, 15, 1)
        this.converter.play();
        

        // we have the radar info, now subscribe for tracks streaming
        this.frameDataSubscription = this.cameraWebsocket.GetFrameData().subscribe({
          next : (frameData) => 
          {
            
            if (!this.streamingStarted && frameData.segment_type != "SPS")
            {
              // stream must start with an I-FRAME
              return
            }
            
            this.streamingStarted = true
            this.convertAndAddFrame(frameData.segment_data)
          }
        })

      },
      error : (err) => err.status == 504 ? this.router.navigate(['/no-service']) : this.router.navigate(['/error-404'])
    })

  }

}
