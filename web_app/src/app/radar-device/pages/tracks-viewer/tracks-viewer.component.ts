import { AfterViewInit, Component, ElementRef, Input, OnInit, ViewChild } from '@angular/core';
import { Router } from '@angular/router';
import { FrameData } from 'src/app/entities/frame-data';
import { RadarDevice, RadarDeviceBrief } from 'src/app/entities/radar-device';
import * as THREE from "three";
import { PerspectiveCamera, PlaneGeometry } from 'three';
import { OrbitControls } from 'three/examples/jsm/controls/OrbitControls'
import { DevicesService } from '../../services/devices.service';
import { WebsocketService } from '../../services/websocket.service';

@Component({
  selector: 'app-page-tracks-viewer',
  templateUrl: './tracks-viewer.component.html',
  styleUrls: ['./tracks-viewer.component.css']
})
export class TracksViewerComponent implements OnInit, AfterViewInit {

  selectedDeviceId : string = ''
  radarDevice : RadarDevice
  deviceList: RadarDeviceBrief[] = [];
  lastframeData: FrameData

  @ViewChild('canvas') private canvasRef : ElementRef;
  
  private get canvas() : HTMLCanvasElement {
    return this.canvasRef.nativeElement;
  }

  private renderer!: THREE.WebGLRenderer
  private scene!: THREE.Scene
  private camera!: THREE.PerspectiveCamera;
  private controls!: OrbitControls

  private planeGridSize: number = 20
  private planeGridDivisions: number = 20 // number of blocks
  private planeGrid: THREE.GridHelper

  constructor(private devicesService : DevicesService,
              private websocketService : WebsocketService,
              private router : Router) { }

  ngOnInit(): void {
    this.getDeviceList()
  }

  public getDeviceList()
  {
    this.devicesService.getRadarDevices().subscribe({
      next : (response) => this.deviceList = response as RadarDeviceBrief[],
      error : (err) => this.router.navigate(['/no-service'])
    })
  }

  public getDevice()
  {
    // request the radar device info based on the given device id
    this.devicesService.getRadarDevice(this.selectedDeviceId).subscribe({
      next : (response) => {
        this.radarDevice = response as RadarDevice
        
        // we have the radar info, now subscribe for tracks streaming
        this.websocketService.GetFrameData().subscribe({
          next : (frameData) => {
            this.lastframeData = frameData as FrameData

            // update the scene with the latest frame data
            this.updateScene()
          }
        })

      },
      error : (err) => err.status == 504 ? this.router.navigate(['/no-service']) : this.router.navigate(['/error-404'])
    })
  }

  public RadarDeviceSelected(deviceId : string)
  {
    this.selectedDeviceId = deviceId
    this.getDevice()
  }

  private initializeScene() 
  {
    this.scene = new THREE.Scene()
    //this.scene.background = new THREE.Color(0x000000)

    let aspectRatio = (this.canvas.clientWidth / this.canvas.clientHeight)

    this.camera = new PerspectiveCamera(
      45,
      aspectRatio,
      0.1,
      1000
    );

    this.camera.position.set(0,2,-17)

    this.renderer = new THREE.WebGLRenderer({ canvas: this.canvas })
    this.renderer.setPixelRatio(devicePixelRatio)
    this.renderer.setSize(this.canvas.clientWidth, this.canvas.clientHeight)

    this.controls = new OrbitControls(this.camera, this.renderer.domElement)
    this.controls.target.set(0,5,10) // change orbit center from [0,0,0] to plane center
    this.controls.listenToKeyEvents(window)
    this.controls.update()

    this.planeGrid = new THREE.GridHelper(this.planeGridSize,this.planeGridDivisions)
    this.planeGrid.position.z += (this.planeGridSize / 2)
    this.scene.add(this.planeGrid)

  }

  private startRenderingLoop()
  {
    let component: TracksViewerComponent = this;
    (function render() 
    {
      requestAnimationFrame(render)
      component.controls.update()
      component.renderer.render(component.scene, component.camera)
    }())
  }

  ngAfterViewInit(): void {
    this.initializeScene()
    this.startRenderingLoop()
  }

  private updateScene()
  {
    let scene = new THREE.Scene()
    scene.add(this.planeGrid)

    this.lastframeData.tracks.forEach(function (track) 
    {
      let boxGeometry = new THREE.BoxGeometry(1,2,1)
      let boxEdges = new THREE.EdgesGeometry(boxGeometry)
      let box = new THREE.LineSegments(boxEdges, new THREE.LineBasicMaterial( { color: 0xffffff } ) )
      box.position.set(track.position_x,track.position_y,track.position_z)
      scene.add(box)

    });

    this.scene = scene
  }
}
