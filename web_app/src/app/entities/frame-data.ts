
//
// {"device_id":"6b817ec6-fb4b-fbe2-54f6-bddf7c1ce187","device_name":"2133121322","timestamp":"2022-10-13T07:26:32.9471835Z","tracks":[
//    {"track_id":0,"position-x":1,"position-y":2,"position-z":1,"velocity-x":0,"velocity-y":0,"velocity-z":0,"acceleration-x":0,"acceleration-y":0,"acceleration-z":0}]}
//
export interface FrameData {
    device_id: string
    device_name: string
    timestamp: string

    tracks: TrackData[]
    points: PointData[]
    targets_index: number[]
}

export interface TrackData {
    track_id: number
    position_x : number
    position_y : number
    position_z : number
    velocity_x : number
    velocity_y : number
    velocity_z : number
    acceleration_x : number
    acceleration_y : number
    acceleration_z : number
}

export interface PointData {
    range: number
    azimuth : number
    elevation : number
    doppler : number
    position_x : number
    position_y : number
    position_z : number
}