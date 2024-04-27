// See https://aka.ms/new-console-template for more information

using ICSharpCode.SharpZipLib.Zip;
using System.Text.Json;


// get args 
if (args.Length == 0)
{
    GoCliMode();
}
else
{
    // fast tools mode
    string action = args[0];
    string p4kPath = args[1];
    switch (action)
    {
        case "extract_memory":
            string filePath = args[2];
            var p4kFile = OpenP4kFile(p4kPath);
            var fileEntries = p4kFile.Entries.Find(e => e.Name == filePath);
            if (fileEntries == null)
            {
                SendJsonMessage("error: not found", null);
                return;
            }
            var data = p4kFile.ExtractToMemory(fileEntries);
            SendJsonMessage("data: Uint8List", data);
            break;
    }
}

unp4k.P4KFile OpenP4kFile(string path)
{
    unp4k.P4KFile p4kFile = new(new FileInfo(path));
    return p4kFile;
}

void GoCliMode()
{
    SendJsonMessage("info: startup", null);

    // get input
    string? input = Console.ReadLine();

    // check if input is null or empty
    if (string.IsNullOrEmpty(input))
    {
        SendJsonMessage("error: No_input_provided", null);
        return;
    }

    SendJsonMessage("info: Reading_p4k_file", null);
    // read as file
    unp4k.P4KFile p4kFile = OpenP4kFile(input);

    var p4kFilesPathList = p4kFile.Entries.Select(e => GenFileData(e));
    var p4kPathFileMap = p4kFile.Entries.ToDictionary(e => e.Name, e => e);

    SendJsonMessage("data: P4K_Files", p4kFilesPathList);

    // loop mode
    SendJsonMessage("info: All Ready", null);
    while (true)
    {
        input = Console.ReadLine();
        if (input == null) continue;
        if (input == "exit")
        {
            SendJsonMessage("info: Exiting", null);
            return;
        }
        else if (input.StartsWith("extract<:,:>") || input.StartsWith("extract_open<:,:>"))
        {
            var paths = input.Split("<:,:>");
            if (paths.Length != 3)
            {
                SendJsonMessage("error: Invalid_command", null);
                continue;
            }
            if (p4kPathFileMap.TryGetValue(paths[1], out var entry))
            {
                p4kFile.Extract(entry, new FileInfo(paths[2]));
                if (input.StartsWith("extract<:,:>"))
                {
                    SendJsonMessage("info: Extracted", paths[2]);
                }
                else if (input.StartsWith("extract_open<:,:>"))
                {
                    SendJsonMessage("info: Extracted_Open", paths[2]);
                }
            }
            else
            {
                SendJsonMessage("error: File_not_found", null);
            }
        }
        else if (input.StartsWith("extract_memory<:,:>"))
        {
            var paths = input.Split("<:,:>");
            if (paths.Length != 2)
            {
                SendJsonMessage("error: Invalid_command", null);
                continue;
            }
            var fileEntries = p4kFile.Entries.Find(e => e.Name == paths[1]);
            if (fileEntries == null)
            {
                SendJsonMessage("error: not found", null);
                continue;
            }
            var data = p4kFile.ExtractToMemory(fileEntries);
            SendJsonMessage("data: Uint8List", data);
        }
        else
        {
            SendJsonMessage("error: Invalid_command", null);
        }
    }

}

Dictionary<string, object> GenFileData(ZipEntry e)
{
    return new Dictionary<string, object>
    {
        { "name", e.Name },
        { "size", e.Size },
        { "compressedSize", e.CompressedSize },
        { "isDirectory", e.IsDirectory },
        { "isFile", e.IsFile },
        { "isEncrypted", e.IsCrypted },
        { "isUnicodeText", e.IsUnicodeText },
        { "dateTime", e.DateTime },
        { "version", e.Version}
    };
}


void SendJsonMessage(String action, Object? data)
{
    var map = new Dictionary<string, object> { { "action", action } };

    if (data != null)
    {
        map.Add("data", data);
    }

    Console.WriteLine(JsonSerializer.Serialize(map));
}