/***
** Copyright (C) 2020-2023 neuRay Labs. All rights reserved.
**
** The information and source code contained herein is the exclusive 
** property of neuRay Labs and may not be disclosed, examined, reproduced, redistributed, used in source and binary forms, in whole or in part  
** without explicit written authorization from the company.
**
***/
using DeviceEmulator.Recordings;
using Microsoft.AspNetCore.Mvc;

namespace DeviceEmulator.Controllers;


[ApiController]
[Route("emulator/recordings")]
public class RecordingsController : ControllerBase
{
    private readonly ILogger<RecordingsController> _logger;

    public RecordingsController(ILogger<RecordingsController> logger)
    {
        _logger = logger;
    }

    [HttpGet]
    public List<RecordingsManager.RecordingInfo> GetRecordings()
    {
        return RecordingsManager.Instance.GetRecordingsList();
    }

    [HttpDelete("{recordingFile}")]
    public void DeleteRecording(string recordingFile)
    {        
        RecordingsManager.Instance.DeleteRecording(recordingFile);
    }

    [HttpGet("{recordingFile}/download")]
    public async Task<IActionResult> DownloadRecording(string recordingFile)
    {
        var fileData = await RecordingsManager.Instance.DownloadRecording(recordingFile);

        string zipFileName = Path.Combine(Path.GetFileNameWithoutExtension(recordingFile), ".zip");
        return File(fileData, "application/zip", zipFileName);
    }

    [HttpPost()]
    [DisableRequestSizeLimit]
    public async Task UploadRecording()
    {
        if (Request.Form.Files.Count != 1)
        {
            throw new BadRequestException("Only one file allowed to upload.");
        }

        var stream = Request.Form.Files[0].OpenReadStream();

        using (var ms = new MemoryStream())
        {
            await stream.CopyToAsync(ms);
            ms.Seek(0, SeekOrigin.Begin);
            RecordingsManager.Instance.UploadRecording(ms);
        }
    }   
}
