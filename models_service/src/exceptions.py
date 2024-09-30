"""
** Copyright (C) 2020-2024 neuRay Labs. All rights reserved.
**
** The information and source code contained herein is the exclusive 
** property of neuRay Labs and may not be disclosed, examined, reproduced, redistributed, used in source and binary forms, in whole or in part  
** without explicit written authorization from the company.
**
"""
from fastapi import HTTPException

class BadRequestException(HTTPException):
    def __init__(self, detail: str = "Invalid request"):
        super().__init__(status_code=400, detail=detail)

class NotFoundException(HTTPException):
    def __init__(self, detail: str = "Resource not found"):
        super().__init__(status_code=404, detail=detail)

class ServiceErrorException(HTTPException):
    def __init__(self, detail: str = "Internal server error"):
        super().__init__(status_code=500, detail=detail)