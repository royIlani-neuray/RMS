import { AfterViewInit, Component, ElementRef, Input, OnInit, ViewChild } from '@angular/core';
import * as THREE from "three";
import { PerspectiveCamera, PlaneGeometry } from 'three';
import { OrbitControls } from 'three/examples/jsm/controls/OrbitControls'

@Component({
  selector: 'app-page-tracks-viewer',
  templateUrl: './tracks-viewer.component.html',
  styleUrls: ['./tracks-viewer.component.css']
})
export class TracksViewerComponent implements OnInit, AfterViewInit {

  @ViewChild('canvas') private canvasRef : ElementRef;
  
  @Input() public size: number = 200;
  @Input() public fieldOfView: number = 45;
  

  @Input('nearClipping') public nearClippingPlane: number = 0.1;
  @Input('farClipping') public farClippingPlane: number = 1000;

  
  private get canvas() : HTMLCanvasElement {
    return this.canvasRef.nativeElement;
  }

  private loader = new THREE.TextureLoader()
  private geometry = new THREE.BoxGeometry(1,1,1)
  private edges = new THREE.EdgesGeometry(this.geometry)
  private line = new THREE.LineSegments(this.edges, new THREE.LineBasicMaterial( { color: 0xffffff } ) )
  private material =  new THREE.MeshBasicMaterial({transparent : true})
  private cube: THREE.Mesh = new THREE.Mesh(this.geometry, this.material)


  private renderer!: THREE.WebGLRenderer
  private scene!: THREE.Scene
  private camera!: THREE.PerspectiveCamera;
  private controls!: OrbitControls

  constructor() { }

  ngOnInit(): void {
  }


  private createScene() 
  {
    this.scene = new THREE.Scene()
    this.scene.background = new THREE.Color(0x000000)

    let aspectRatio = this.getAspectRatio()

    this.camera = new PerspectiveCamera(
      this.fieldOfView,
      aspectRatio,
      this.nearClippingPlane,
      this.farClippingPlane
    );

    this.camera.position.set(0,2,-17)

    this.renderer = new THREE.WebGLRenderer({ canvas: this.canvas })
    this.renderer.setPixelRatio(devicePixelRatio)
    this.renderer.setSize(this.canvas.clientWidth, this.canvas.clientHeight)

    this.controls = new OrbitControls(this.camera, this.renderer.domElement)
    this.controls.target.set(0,5,10) // change orbit center from [0,0,0] to plane center
    this.controls.listenToKeyEvents(window)
    this.controls.update()


    //this.scene.add(this.cube)
    this.scene.add(this.line)

    //var plane = new THREE.Mesh(new PlaneGeometry(20,20), new THREE.MeshBasicMaterial({visible: false, side : THREE.DoubleSide}))
    //plane.rotateX(-Math.PI / 2)
    //this.scene.add(plane)
    var planeGrid = new THREE.GridHelper(20,20)
    planeGrid.position.z += 10
    this.scene.add(planeGrid)

  }

  private getAspectRatio()
  {
    return this.canvas.clientWidth / this.canvas.clientHeight
  }

  private startRenderingLoop()
  {
    

    let component: TracksViewerComponent = this;
    (function render() {
      requestAnimationFrame(render)

      //component.controls.update(0.01)
      component.controls.update()

      component.renderer.render(component.scene, component.camera)
    }())
  }

  ngAfterViewInit(): void {
    this.createScene()
    this.startRenderingLoop()
  }
}
