/***
** Copyright (C) 2020-2024 neuRay Labs. All rights reserved.
**
** The information and source code contained herein is the exclusive 
** property of neuRay Labs and may not be disclosed, examined, reproduced, redistributed, used in source and binary forms, in whole or in part  
** without explicit written authorization from the company.
**
***/

namespace WebService.Entites;

public interface IEntity {

    public enum EntityTypes {
        Radar,
        Camera, 
        RadarTemplate,
        User,
        RecordingSchedule
    }

    public EntityTypes EntityType { get; }

    // each entity has a unique id.
    public string Id { get; }

    // all entities are stored in file system based db.
    public string StoragePath { get; }

    // lock that is used for entity sync
    public ReaderWriterLockSlim EntityLock { get; set; }
}