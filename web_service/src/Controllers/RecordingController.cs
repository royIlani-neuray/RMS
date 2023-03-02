/***
** Copyright (C) 2020-2023 neuRay Labs. All rights reserved.
**
** The information and source code contained herein is the exclusive 
** property of neuRay Labs and may not be disclosed, examined, reproduced, redistributed, used in source and binary forms, in whole or in part  
** without explicit written authorization from the company.
**
***/
using Microsoft.AspNetCore.Mvc;
using WebService.Recordings;

namespace WebService.Controllers;

[ApiController]
[Route("recordings")]
public class RecordingsController : ControllerBase
{
    [HttpGet]
    public List<RecordingInfo> GetRecordings()
    {
        return RecordingsManager.Instance.GetRecordings();
    }

    [HttpDelete("{recordingName}")]
    public void DeleteRecording(string recordingName)
    {        
        RecordingsManager.Instance.DeleteRecording(recordingName);
    }
    
    [HttpGet("{recordingName}/download")]
    public IActionResult DownloadRecording(string recordingName)
    {
        var fileData = RecordingsManager.Instance.DownloadRecording(recordingName, out string archiveFileName);
        return File(fileData, "application/zip", archiveFileName);
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
