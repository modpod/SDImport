using System;

// program starts and asks for input ("arguments")
Console.WriteLine("Start - \"5\" for 5d, \"4\" for T4i");
string argline = Console.ReadLine();
// start main method with your input
Main(argline);

// main method looks at your input and decides which processing method to run
static void Main(string arg = null)
{
    // 'try' as in "try to do this"
    try
    {
        if (arg != null)
        {
            // a switch statement examines a variable and does what you tell it to here depending on the value of that variable
            // our variable here is args. the only case i have written is 5 for my 5D camera
            switch (arg)
            {
                case "5":
                    // this will call the Process5D() method below, and if it's successful, break out of the switch
                    if (Process5d()) break;
                    else throw new Exception("5D backup failed");
                case "4":
                    // this will call the Process5D() method below, and if it's successful, break out of the switch
                    if (ProcessT4i()) break;
                    else throw new Exception("T4i backup failed");
                    // 'default' is the case for anything not defined previously, so right now anything other than "5" 
                default:
                    Console.WriteLine("Bad argument");
                    break;
            }
        }
    }
    // 'catch' as in "if an error is thrown, catch it"
    catch (Exception ex)
    {
        // just write the error out to the console. May or may not be useful, but will probably give a line number at least
        Console.WriteLine(ex.Message);
        Console.WriteLine(ex.StackTrace);
    }
    // 'finally' runs after the try/catch regardless of whether there was an error
    finally
    {
        // this ending Console.Read() will just ensure the console doesn't immediately close when the program finishes
        Console.WriteLine("Finished - press enter to exit...");
        Console.Read();
    }
}

static bool Process5d()
{
    bool ok = false;

    // define initial parameters
    int filesNum = 0;
    string filesNumString = string.Empty;
    // this is the drive letter of the removable device (sd card, usb stick, etc)
    string sdRoot = "E:";
    // this camera makes numbered folders, starting with 100 - only looking there for now
    int sdDirNum = 100;
    // Path.Combine builds the name of the full directory for me here: E:\DCIM\100EOS5D
    string sdDir = Path.Combine(sdRoot, "DCIM", sdDirNum.ToString() + "EOS5D");
    // this one won't ever change, so we can just define it directly.
    // to do it with like the other one, the equivalent would be Path.Combine("D:", "Photo", "5D")
    // (\ is a special character, so you need to 'escape' it by putting it after another \ when you're defining it like this)
    string localDir = "D:\\Photo\\5D";
    // this would just make a string "2023-06-12" - we're going to create that directory later
    string todayDir = DateTime.Now.ToString("yyyy-MM-dd");
    // we're going to make copies to the SD card itself and our local directory
    // so these would be, e.x., "D:\Photo\5D\2023-06-12"
    string localTodayDir = Path.Combine(localDir, todayDir);
    string sdTodayDir = Path.Combine(sdRoot, todayDir);

    try
    {
        if (Directory.Exists(sdRoot))
        {
            if (Directory.Exists(localDir))
            {
                // create local dir
                if (!Directory.Exists(localTodayDir)) Directory.CreateDirectory(localTodayDir);

                // create backup dir on SD card
                if (!Directory.Exists(sdTodayDir)) Directory.CreateDirectory(sdTodayDir);

                foreach (string sdDirFile in Directory.GetFiles(sdDir))
                {
                    // increment number of files copied
                    filesNum++;

                    // this just adds spaces to the console output lines so the numbers align
                    if (filesNum < 10) filesNumString = filesNum.ToString() + "   ";
                    else if (filesNum < 100) filesNumString = filesNum.ToString() + "  ";
                    else if (filesNum < 1000) filesNumString = filesNum.ToString() + " ";
                    else filesNumString = filesNum.ToString();

                    // get the file extension of the file to copy 
                    string sdDirFileExtension = sdDirFile.Substring(sdDirFile.LastIndexOf('.'), 4);

                    // get the rest of the file name
                    string sdDirFilename = Path.GetFileName(sdDirFile).Replace(sdDirFileExtension, string.Empty);

                    // get the time it was created as a string in the format _yyyy-MM-dd_hh.mm.ss, yyyy = four-digit year, etc
                    string sdDirFileTimestamp = File.GetCreationTime(sdDirFile).ToString("_yyyy-MM-dd_hh.mm.ss");

                    // define new folders inside sdTodayDir and localBackupExtensionDir, one for each file extension found
                    string sdBackupExtensionDir = Path.Combine(sdTodayDir, sdDirFileExtension.Replace(".", ""));
                    string localBackupExtensionDir = Path.Combine(localTodayDir, sdDirFileExtension.Replace(".", ""));

                    // if those folders don't exist, create them
                    if (!Directory.Exists(sdBackupExtensionDir)) Directory.CreateDirectory(sdBackupExtensionDir);
                    if (!Directory.Exists(localBackupExtensionDir)) Directory.CreateDirectory(localBackupExtensionDir);

                    // define two file names, one inside each of those folders
                    // these two files are going to be what we copy to, they don't exist yet
                    string sdBackupFile = Path.Combine(sdBackupExtensionDir, sdDirFilename + sdDirFileTimestamp + sdDirFileExtension);
                    string localBackupFile = Path.Combine(localBackupExtensionDir, sdDirFilename + sdDirFileTimestamp + sdDirFileExtension);

                    // if they do exist for some reason, throw an error
                    if (File.Exists(sdBackupFile)) throw new Exception("File on SD already exists: " + sdBackupFile);
                    else
                    {
                        // copy the file and output what you wrote to the console
                        File.Copy(sdDirFile, sdBackupFile, true);
                        Console.WriteLine("     " + sdBackupFile);
                    }

                    // then for the other one
                    if (File.Exists(localBackupFile)) throw new Exception("Local file already exists: " + localBackupFile);
                    else
                    {
                        File.Move(sdDirFile, localBackupFile, true);
                        Console.WriteLine("     " + localBackupFile);
                    }

                    Console.WriteLine(filesNumString + " OK");
                    Console.WriteLine();
                }
            }
            else throw new Exception("Local dir not found at " + localDir);
        }
        else throw new Exception("5D SD card not found at " + sdRoot);

        // if we got to this point, we looped through all the files successfully (otherwise we would have thrown an error and exited the loop)
        Console.WriteLine("Moved " + filesNum + " files OK");
        ok = true;
    }
    // if we throw an error it gets caught here
    catch (Exception ex)
    {
        // this will output what the error was (or at least where it happened) to the console
        Console.WriteLine(ex.Message);
        Console.WriteLine(ex.StackTrace);
        ok = false;
    }

    // finally, return whatever our status was
    return ok;
}

/*
 the directory structure for the T4i looks like this, starting from E:\, where my SD card reader mounts to when i insert a card:
E:\
E:\MISC
E:\DCIM\100CANON
E:\DCIM\101CANON
E:\DCIM\EOSMISC
 
we don't care about MISC or EOSMISC, the ones ending in CANON have the pictures in them
(a new folder is created every so often by the camera when too many files are in the last one, so it can read them faster)
 */

static bool ProcessT4i()
{
    bool ok = false;

    int filesNum = 0;
    string filesNumString = string.Empty;
    string sdRoot = "E:";
    string sdDir = Path.Combine(sdRoot, "DCIM");
    string localDir = "D:\\Photo\\T4i";
    string todayDir = DateTime.Now.ToString("yyyy-MM-dd");
    string localTodayDir = Path.Combine(localDir, todayDir);
    string sdTodayDir = Path.Combine(sdRoot, todayDir);

    try
    {
        if (Directory.Exists(sdRoot))
        {
            if (Directory.Exists(localDir))
            {
                if (!Directory.Exists(localTodayDir)) Directory.CreateDirectory(localTodayDir);

                if (!Directory.Exists(sdTodayDir)) Directory.CreateDirectory(sdTodayDir);

                Console.WriteLine(" --> " + sdDir);

                foreach (string sdDirSubdirectory in Directory.GetDirectories(sdDir).Where(x => x.EndsWith("CANON")))
                {
                    Console.WriteLine("  -> " + sdDirSubdirectory);

                    foreach (string file in Directory.GetFiles(sdDirSubdirectory))
                    {
                        filesNum++;

                        if (filesNum < 10) filesNumString = filesNum.ToString() + "   ";
                        else if (filesNum < 100) filesNumString = filesNum.ToString() + "  ";
                        else if (filesNum < 1000) filesNumString = filesNum.ToString() + " ";
                        else filesNumString = filesNum.ToString();

                        string sdDirFileExtension = file.Substring(file.LastIndexOf('.'), 4);

                        string sdDirFilename = Path.GetFileName(file).Replace(sdDirFileExtension, string.Empty);

                        string sdDirFileTimestamp = File.GetCreationTime(file).ToString("_yyyy-MM-dd_hh.mm.ss");

                        string sdBackupExtensionDir = Path.Combine(sdTodayDir, sdDirFileExtension.Replace(".", ""));
                        string localBackupExtensionDir = Path.Combine(localTodayDir, sdDirFileExtension.Replace(".", ""));

                        if (!Directory.Exists(sdBackupExtensionDir)) Directory.CreateDirectory(sdBackupExtensionDir);
                        if (!Directory.Exists(localBackupExtensionDir)) Directory.CreateDirectory(localBackupExtensionDir);

                        string sdBackupFile = Path.Combine(sdBackupExtensionDir, sdDirFilename + sdDirFileTimestamp + sdDirFileExtension);
                        string localBackupFile = Path.Combine(localBackupExtensionDir, sdDirFilename + sdDirFileTimestamp + sdDirFileExtension);

                        if (File.Exists(sdBackupFile)) throw new Exception("File on SD already exists: " + sdBackupFile);
                        else
                        {
                            File.Copy(file, sdBackupFile, true);
                            Console.WriteLine("   - " + sdBackupFile);
                        }

                        if (File.Exists(localBackupFile)) throw new Exception("Local file already exists: " + localBackupFile);
                        else
                        {
                            File.Move(file, localBackupFile, true);
                            Console.WriteLine("   > " + localBackupFile);
                        }

                        Console.WriteLine(filesNumString + " OK");
                        Console.WriteLine();
                    }
                }
            }
            else throw new Exception("Local dir not found at " + localDir);
        }
        else throw new Exception("5D SD card not found at " + sdRoot);

        Console.WriteLine("Moved " + filesNum + " files OK");
        ok = true;
    }
    catch (Exception ex)
    {
        Console.WriteLine(ex.Message);
        Console.WriteLine(ex.StackTrace);
        ok = false;
    }

    return ok;
}