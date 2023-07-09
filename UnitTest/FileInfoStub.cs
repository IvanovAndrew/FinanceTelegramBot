﻿using Infrastructure;

namespace UnitTest;

public class FileInfoStub : ITelegramFileInfo
{
    public string FileId { get; set; }
    public string? FileName { get; set; }
    public string? MimeType { get; set; }
    public IFile? Content { get; set; }
}