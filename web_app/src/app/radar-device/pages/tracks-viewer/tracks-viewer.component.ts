import { AfterViewInit, Component, ElementRef, Input, OnInit, ViewChild } from '@angular/core';
import { Router } from '@angular/router';
import { FrameData } from 'src/app/entities/frame-data';
import { RadarDevice, RadarDeviceBrief } from 'src/app/entities/radar-device';
import * as THREE from "three";
import { PerspectiveCamera } from 'three';
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

  // Importent - Three.js rendering coordinates:
  // X: positive => canvas left side. negative => canvas right side
  // Y: positive => canvas top side. negative => canvas bottom
  // Z: positive => far side (canvas depth). negative => close side.
  //
  // For Radar: 
  //  X positive and negative are the opposite of Three.js X, 
  //  Radar Y is Three.js Z
  //  Radar Z is Three.js Y


  private updateScene()
  {
    if (this.radarDevice.radar_settings == null)
      return

    let scene = new THREE.Scene()

    if (this.radarDevice.radar_settings.boundary_box != null)
    {
        let boundingBoxSizeX = Math.abs(this.radarDevice.radar_settings.boundary_box.x_max - this.radarDevice.radar_settings.boundary_box.x_min)
        let boundingBoxSizeY = Math.abs(this.radarDevice.radar_settings.boundary_box.z_max - this.radarDevice.radar_settings.boundary_box.z_min)
        let boundingBoxSizeZ = Math.abs(this.radarDevice.radar_settings.boundary_box.y_max - this.radarDevice.radar_settings.boundary_box.y_min)
        let boundingBoxZoffset = this.radarDevice.radar_settings.boundary_box.y_min

        // draw the floor (plane grid)
        let planeGridSize = Math.max(this.radarDevice.radar_settings.boundary_box.y_max, boundingBoxSizeX)
        let planeGridDivisions = planeGridSize
        let planeGrid = new THREE.GridHelper(planeGridSize,planeGridDivisions)
        planeGrid.position.z += (planeGridSize / 2)
        scene.add(planeGrid)

        // draw the bounding box
        let boxGeometry = new THREE.BoxGeometry(boundingBoxSizeX,boundingBoxSizeY,boundingBoxSizeZ)
        let boxEdges = new THREE.EdgesGeometry(boxGeometry)
        let box = new THREE.LineSegments(boxEdges, new THREE.LineBasicMaterial( { color: 0xff00ff } ) )
        box.position.set(0,0,boundingBoxZoffset + (boundingBoxSizeZ/2))
        scene.add(box)         
    }

    if (this.radarDevice.radar_settings.sensor_position != null)
    {
      // draw the radar
      let radarGeometery = new THREE.BoxGeometry(0.5,0.5,0.2)
      let radar = new THREE.Mesh(radarGeometery, new THREE.MeshBasicMaterial({ color: 0xc91616 }))
      radar.position.set(0,this.radarDevice.radar_settings.sensor_position.height,0)
      scene.add(radar)  
    }

    // draw tracks

    this.lastframeData.tracks.forEach(function (track) 
    {
      let boxGeometry = new THREE.BoxGeometry(1,2,1)
      let boxEdges = new THREE.EdgesGeometry(boxGeometry)
      let box = new THREE.LineSegments(boxEdges, new THREE.LineBasicMaterial( { color: 0xffffff } ) )
      box.position.set(-track.position_x,track.position_z,track.position_y)
      scene.add(box)

    });

    this.scene = scene
  }
}
