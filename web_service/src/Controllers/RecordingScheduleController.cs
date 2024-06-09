/***
** Copyright (C) 2020-2024 neuRay Labs. All rights reserved.
**
** The information and source code contained herein is the exclusive 
** property of neuRay Labs and may not be disclosed, examined, reproduced, redistributed, used in source and binary forms, in whole or in part  
** without explicit written authorization from the company.
**
***/
using WebService.Entites;
using WebService.Context;
using WebService.Actions.RecordingSchedules;
using Microsoft.AspNetCore.Mvc;

namespace WebService.Controllers;

[ApiController]
[Route("schedules")]
public class RecordingScheduleController : ControllerBase
{
    private readonly ILogger<RecordingScheduleController> _logger;

    public RecordingScheduleController(ILogger<RecordingScheduleController> logger)
    {
        _logger = logger;
    }

    private void ValidateScheduleId(string scheduleId)
    {
        if (string.IsNullOrWhiteSpace(scheduleId) || !Guid.TryParse(scheduleId, out _))
            throw new BadRequestException("invalid schedule id provided.");
    }

    [HttpGet]
    public List<RecordingSchedule> GetSchedules()
    {
        return RecordingScheduleContext.Instance.GetSchedules();
    }

    [HttpGet("{scheduleId}")]
    public RecordingSchedule GetSchedule(string scheduleId)
    {
        ValidateScheduleId(scheduleId);
        if (!RecordingScheduleContext.Instance.IsScheduleExist(scheduleId))
            throw new NotFoundException("There is no schedule with the provided id");

        return RecordingScheduleContext.Instance.GetSchedule(scheduleId);
    }

    [HttpPost]
    public void AddSchedule([FromBody] AddRecordingScheduleArgs args)
    {
        AddRecordingScheduleAction action = new AddRecordingScheduleAction(args);
        action.Run();
        return;
    }

    [HttpPost("{scheduleId}")]
    public void UpdateSchedule(string scheduleId, [FromBody] UpdateRecordingScheduleArgs args)
    {
        ValidateScheduleId(scheduleId);
        if (!RecordingScheduleContext.Instance.IsScheduleExist(scheduleId))
            throw new NotFoundException("There is no schedule with the provided id");

        UpdateRecordingSchedule action = new UpdateRecordingSchedule(scheduleId, args);
        action.Run();
        return;
    }

    [HttpDelete("{scheduleId}")]
    public void DeleteSchedule(string scheduleId)
    {        
        ValidateScheduleId(scheduleId); 
        var action = new DeleteRecordingScheduleAction(scheduleId);
        action.Run();
    }

}