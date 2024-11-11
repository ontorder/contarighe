using contarighe;
using System.Text.RegularExpressions;
using Windows.Win32.System.Console;

SessionResults results = new(0, 0, 0, new Top10Record[10]);
SearchFlags state = new(true, false, false, false, false, "\\.cs$", null, Directory.GetCurrentDirectory());
CursorState MoveCursor_State = new(0, 1, 10);

var rxFileEnum = CreateRxFileEnum(state.EnumFileFilter, state.IsFileEnumCaseSensitive);
var rxPathExclide = (Regex?)null;

Console.CursorVisible = false;
var consoleHandle = Windows.Win32.PInvoke.GetStdHandle_SafeHandle(STD_HANDLE.STD_INPUT_HANDLE);
var scmSucc = Windows.Win32.PInvoke.SetConsoleMode(consoleHandle, CONSOLE_MODE.ENABLE_INSERT_MODE |
    CONSOLE_MODE.ENABLE_PROCESSED_INPUT | CONSOLE_MODE.ENABLE_VIRTUAL_TERMINAL_INPUT | CONSOLE_MODE.ENABLE_WINDOW_INPUT);
if (scmSucc == false) throw new Exception();
//var gcmSucc = Windows.Win32.PInvoke.GetConsoleMode(consoleHandle, out var consoleMode);

var printMenu = true;
var action = MenuAction.None;
do
{
    if (printMenu)
    {
        PrintMenu();
        printMenu = false;
        MoveCursor(CursorDirection.Init);
    }

    var kp = ReadInput();

    switch (kp.Meta)
    {
        case ConsoleKey.DownArrow: MoveCursor(CursorDirection.Down); break;
        case ConsoleKey.End: MoveCursor(CursorDirection.End); break;
        case ConsoleKey.Home: MoveCursor(CursorDirection.Start); break;
        case ConsoleKey.PageDown: MoveCursor(CursorDirection.PageDown); break;
        case ConsoleKey.PageUp: MoveCursor(CursorDirection.PageUp); break;
        case ConsoleKey.UpArrow: MoveCursor(CursorDirection.Up); break;

        default:
            action = MapChoiceToAction(MoveCursor_State.Y, kp.Input, kp.Meta);
            if (action != MenuAction.Exit && action != MenuAction.None) TryChoice(action);
            break;
    }
}
while (action != MenuAction.Exit);

MenuAction MapChoiceToAction(int cursorY, char menuChoiceResp, ConsoleKey meta)
{
    return (cursorY, menuChoiceResp, meta) switch
    {
        (1, _, ConsoleKey.Enter) or (_, '1', _) or (_, 'f', _) or (_, 'F', _) => MenuAction.SetFilter,
        (2, _, ConsoleKey.Enter) or (_, '2', _) or (_, 'r', _) or (_, 'R', _) => MenuAction.SetRecursion,
        (3, _, ConsoleKey.Enter) or (_, '3', _) or (_, 'c', _) or (_, 'C', _) => MenuAction.SetComments,
        (4, _, ConsoleKey.Enter) or (_, '4', _) or (_, 'v', _) or (_, 'V', _) => MenuAction.SetEmptyLines,
        (5, _, ConsoleKey.Enter) or (_, '5', _) or (_, 'z', _) or (_, 'Z', _) => MenuAction.ResetState,
        (6, _, ConsoleKey.Enter) or (_, '6', _) or (_, 'm', _) or (_, 'M', _) => MenuAction.SetCaseSensitivity,
        (7, _, ConsoleKey.Enter) or (_, '7', _) or (_, 'e', _) or (_, 'E', _) => MenuAction.SetExcludePath,
        (8, _, ConsoleKey.Enter) or (_, '8', _) or (_, 'p', _) or (_, 'P', _) => MenuAction.SetOverridePath,
        (9, _, ConsoleKey.Enter) or (_, '0', _) => MenuAction.Execute,
        (10, _, ConsoleKey.Enter) or (_, 'q', _) => MenuAction.Exit,
        _ => MenuAction.None,
    };
}

void MoveCursor(CursorDirection direction)
{
    switch (direction)
    {
        case CursorDirection.Down:
            if (MoveCursor_State.Y == MoveCursor_State.MaxY) break;
            Console.Write($"{Vt100.AbsolutePosition(MoveCursor_State.X, MoveCursor_State.Y)}");
            Console.Write("  ");
            ++MoveCursor_State.Y;
            Console.Write($"{Vt100.AbsolutePosition(MoveCursor_State.X, MoveCursor_State.Y)}");
            Console.Write("->");
            break;

        case CursorDirection.End:
            if (MoveCursor_State.Y == MoveCursor_State.MaxY) break;
            Console.Write($"{Vt100.AbsolutePosition(MoveCursor_State.X, MoveCursor_State.Y)}");
            Console.Write("  ");
            MoveCursor_State.Y = MoveCursor_State.MaxY;
            Console.Write($"{Vt100.AbsolutePosition(MoveCursor_State.X, MoveCursor_State.Y)}");
            Console.Write("->");
            break;

        case CursorDirection.Init:
            Console.Write($"{Vt100.AbsolutePosition(MoveCursor_State.X, MoveCursor_State.Y)}");
            Console.Write("->");
            break;

        case CursorDirection.PageDown:
            if (MoveCursor_State.Y == MoveCursor_State.MaxY) break;
            var pageDownY = Math.Min(MoveCursor_State.Y + 4, MoveCursor_State.MaxY);
            Console.Write($"{Vt100.AbsolutePosition(MoveCursor_State.X, MoveCursor_State.Y)}");
            Console.Write("  ");
            MoveCursor_State.Y = pageDownY;
            Console.Write($"{Vt100.AbsolutePosition(MoveCursor_State.X, MoveCursor_State.Y)}");
            Console.Write("->");
            break;

        case CursorDirection.PageUp:
            if (MoveCursor_State.Y == 1) break;
            var pageUpY = Math.Max(MoveCursor_State.Y - 4, 1);
            Console.Write($"{Vt100.AbsolutePosition(MoveCursor_State.X, MoveCursor_State.Y)}");
            Console.Write("  ");
            MoveCursor_State.Y = pageUpY;
            Console.Write($"{Vt100.AbsolutePosition(MoveCursor_State.X, MoveCursor_State.Y)}");
            Console.Write("->");
            break;

        case CursorDirection.Start:
            if (MoveCursor_State.Y == 1) break;
            Console.Write($"{Vt100.AbsolutePosition(MoveCursor_State.X, MoveCursor_State.Y)}");
            Console.Write("  ");
            MoveCursor_State.Y = 1;
            Console.Write($"{Vt100.AbsolutePosition(MoveCursor_State.X, MoveCursor_State.Y)}");
            Console.Write("->");
            break;

        case CursorDirection.Up:
            if (MoveCursor_State.Y == 1) break;
            Console.Write($"{Vt100.AbsolutePosition(MoveCursor_State.X, MoveCursor_State.Y)}");
            Console.Write("  ");
            --MoveCursor_State.Y;
            Console.Write($"{Vt100.AbsolutePosition(MoveCursor_State.X, MoveCursor_State.Y)}");
            Console.Write("->");
            break;
    }
}

bool AnyOtherInput()
{
    var data = new INPUT_RECORD[9];
    var succ = Windows.Win32.PInvoke.PeekConsoleInput(consoleHandle, data, out var nEvents);
    var keydata = data.Where(_ => _.EventType == 1);
    return keydata.Any();
}

StdinValue ReadInput()
{
    var state = ReadState.Initial;
    StdinValue? parsed = null;

    do
    {
        var kp = Console.ReadKey(true);
        switch (state)
        {
            case ReadState.Initial:
                if (kp.KeyChar == '\x1b')
                {
                    if (AnyOtherInput())
                        state = ReadState.VtEscape;
                    else
                        parsed = new(true, default, ConsoleKey.Escape);
                }
                else
                {
                    (state, parsed) = kp.KeyChar switch
                    {
                        '\r' => (ReadState.Initial, new StdinValue(true, kp.KeyChar, ConsoleKey.Enter)),
                        ' ' => (ReadState.Initial, new StdinValue(true, kp.KeyChar, ConsoleKey.Spacebar)),
                        '\n' => (ReadState.Initial, new StdinValue(true, kp.KeyChar, ConsoleKey.Enter)),
                        _ => (ReadState.Initial, new StdinValue(false, kp.KeyChar, default))
                    };
                }
                break;

            case ReadState.VtEscape:
                state = kp.KeyChar switch
                {
                    '[' => ReadState.VtBracket,
                    _ => ReadState.VtNonBracket,
                };
                break;

            case ReadState.VtBracket:
                (state, parsed) = kp.KeyChar switch
                {
                    'A' => (ReadState.Initial, new StdinValue(true, default, ConsoleKey.UpArrow)),
                    'B' => (ReadState.Initial, new StdinValue(true, default, ConsoleKey.DownArrow)),
                    'C' => (ReadState.Initial, new StdinValue(true, default, ConsoleKey.RightArrow)),
                    'D' => (ReadState.Initial, new StdinValue(true, default, ConsoleKey.LeftArrow)),
                    '6' => (ReadState._3CharSequence, new(true, default, ConsoleKey.PageDown)),
                    '5' => (ReadState._3CharSequence, new(true, default, ConsoleKey.PageUp)),
                    'H' => (ReadState.Initial, new StdinValue(true, default, ConsoleKey.Home)),
                    'F' => (ReadState.Initial, new StdinValue(true, default, ConsoleKey.End)),
                    _ => throw new NotImplementedException(),
                };
                break;

            case ReadState._3CharSequence:
                state = ReadState.Initial;
                switch (kp.KeyChar)
                {
                    case '~': break;
                    default: throw new NotImplementedException();
                }
                break;
        }
    }
    while (parsed == null);
    return parsed.Value;
}

void PrintPrompt() => Console.Write(": ");

void PrintMenu()
{
    Console.WriteLine("   cambia filtro");
    Console.WriteLine("   impostazioni ricorsività");
    Console.WriteLine("   conteggia commenti");
    Console.WriteLine("   filtra righe vuote");
    Console.WriteLine("   azzera conteggi per ogni sessione");
    Console.WriteLine("   impostazioni maiuscole/minuscole");
    Console.WriteLine("   escludi cartelle");
    Console.WriteLine("   override percorso");
    Console.WriteLine("   esegui conteggio");
    Console.WriteLine("   esci");
}

void ChoiceFilter()
{
    Console.WriteLine("Filtro attuale: {0}", state.EnumFileFilter);
    while (true)
    {
        try
        {
            Console.WriteLine("Immetti un nuovo filtro (regexp javascript):");
            Console.WriteLine(" - windows: *.cs, js: /\\.cs$/i, digitare: \\.cs$");
            Console.WriteLine(" - windows:    ?, js:    /.?/i, digitare:    .?");
            Console.WriteLine(" - windows:    *, js: //, /.*/, digitare:    .*");
            Console.WriteLine(" - un carattere qualsiasi, ma non 's', digitare: [^s]?");
            Console.WriteLine(" - file senza punto/estensione (.est), digitare: \\\\[^\\.]*$");
            PrintPrompt();
            var filterResp = Console.ReadLine();

            rxFileEnum = CreateRxFileEnum(filterResp, state.IsFileEnumCaseSensitive);
            Console.WriteLine("[filtro creato]");
            state.EnumFileFilter = filterResp;
            break;
        }
        catch (ArgumentException filterErr)
        {
            Console.WriteLine("Attenzione, errore nel filtro: {0}", filterErr.Message);
            continue;
        }
    }
}

void ChoiceFileRecursion()
{
    Console.WriteLine("Ricerca file con ricorsività: {0}", state.IsEnumeraFileRicorsione.ToString());
    string enumRecursivelyResp;
    do
    {
        Console.WriteLine("Vuoi attivare la ricorsività? (s|n): ");
        PrintPrompt();
        enumRecursivelyResp = Console.ReadLine();
    }
    while (enumRecursivelyResp != "s" && enumRecursivelyResp != "n");
    state.IsEnumeraFileRicorsione = enumRecursivelyResp == "s";
}

void ChoiceComments()
{
    Console.WriteLine("Conteggio righe di soli commenti (righe che iniziano esclusivamente per //): {0}", state.IsCommenti.ToString());
    string text5;
    do
    {
        Console.WriteLine("Vuoi attivare il conteggio commenti? (s|n): ");
        PrintPrompt();
        text5 = Console.ReadLine();
    }
    while (text5 != "s" && text5 != "n");
    state.IsCommenti = text5 == "s";
}

void ChoiceEmptyLines()
{
    Console.WriteLine("Conteggio righe vuote: {0}", state.IsRigheVuote.ToString());
    string countEmptyResp;
    do
    {
        Console.WriteLine("Vuoi attivare il conteggio delle righe vuote (righe composte al più di soli spazi, tab e ascii(9))? (s|n): ");
        PrintPrompt();
        countEmptyResp = Console.ReadLine();
    }
    while (countEmptyResp != "s" && countEmptyResp != "n");
    state.IsRigheVuote = countEmptyResp == "s";
}

void ChoiceResetState()
{
    Console.WriteLine("Azzeramento righe globali: {0}", state.IsRigheGlobali.ToString());
    string countGlobalResp;
    do
    {
        Console.WriteLine("Vuoi resettare il conteggio delle righe globali ad ogni ciclo? (s|n): ");
        PrintPrompt();
        countGlobalResp = Console.ReadLine();
    }
    while (countGlobalResp != "s" && countGlobalResp != "n");
    state.IsRigheGlobali = countGlobalResp == "s";
}

void ChoiceCaseSensitivity()
{
    Console.WriteLine("Distinzione maiuscole/minuscole: {0}", state.IsFileEnumCaseSensitive.ToString());
    string caseSensResp;
    do
    {
        Console.WriteLine("Vuoi? (s|n): ");
        PrintPrompt();
        caseSensResp = Console.ReadLine();
    }
    while (caseSensResp != "s" && caseSensResp != "n");
    state.IsFileEnumCaseSensitive = caseSensResp == "s";
}

void ChoiceExcludePath()
{
    Console.WriteLine($"Filtro esclusione percorso: {state.FolderExclude}");
    PrintPrompt();
    state.FolderExclude = Console.ReadLine();
    if (false == string.IsNullOrWhiteSpace(state.FolderExclude))
        rxPathExclide = CreateRxFileEnum(state.FolderExclude, state.IsFileEnumCaseSensitive);
}

void ChoiceOverridePath()
{
    Console.WriteLine($"Percorso attuale: {state.EnumPath}");
    PrintPrompt();
    var pathResp = Console.ReadLine();
    if (false == string.IsNullOrWhiteSpace(pathResp))
        state.EnumPath = pathResp;
}

void Execute()
{
    Console.WriteLine("Eseguo il conteggio con il filtro /{0}/{3}, ricorsività {1}, azzeramento righe globali {2}",
        state.EnumFileFilter, state.IsEnumeraFileRicorsione.ToString(), state.IsRigheGlobali.ToString(), state.IsFileEnumCaseSensitive ? "i" : "");

    if (state.IsRigheGlobali)
        results = new();

    var fileEnum = Directory.EnumerateFiles(state.EnumPath, "*",
        state.IsEnumeraFileRicorsione ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly);

    var readFileErr = new LinkedList<string>();
    foreach (string fileItem in fileEnum)
    {
        ++results.TotEnumeratedFiles;

        if (rxPathExclide != null && rxPathExclide.Match(fileItem).Success) continue;
        if (false == rxFileEnum.Match(fileItem).Success) continue;

        var foundLines = ConsumeFile(state, readFileErr, fileItem);
        if (foundLines > 0)
        {
            ++results.FileCount;
            results.TotFoundLines += foundLines;
            ManageTop10(fileItem, foundLines);
            Console.WriteLine("File: {0}, righe: {1}", fileItem, foundLines);
        }
    }
    if (readFileErr.Count != 0)
    {
        Console.WriteLine("Ci sono stati errori nell'aprire i seguenti file:");
        foreach (string errored in readFileErr) Console.WriteLine(errored);
    }

    Console.WriteLine("N. file processati: {0}, n. file totali: {1}, righe totali: {2}",
        results.FileCount, results.TotEnumeratedFiles, results.TotFoundLines.ToString("N0"));
    Console.WriteLine("Top 10:");
    for (int id = 0; id < 10; ++id) Console.WriteLine("{0}. {1} - {2}", id + 1, results.Top10[id].FileNamePath, results.Top10[id].LineCount);
    Console.WriteLine("Premi Invio per continuare...");
    Console.ReadLine();
}

void ManageTop10(string fileNamePath, int lineCount)
{
    var foundId = -1;
    for (int id = 0; id < 10; ++id)
    {
        if (lineCount > results.Top10[id].LineCount)
        {
            foundId = id;
            break;
        }
    }

    if (foundId < 0) return;

    for (int moveId = 9; moveId > foundId; --moveId)
        results.Top10[moveId] = results.Top10[moveId - 1];
    results.Top10[foundId] = new(fileNamePath, lineCount);
}

static int ConsumeFile(SearchFlags state, LinkedList<string> readFileErr, string item)
{
    FileStream fileStream;
    try
    {
        fileStream = new FileStream(item, FileMode.Open, FileAccess.Read, FileShare.Read);
        if (fileStream.Length == 0)
        {
            fileStream.Dispose();
            return 0;
        }
    }
    catch (Exception)
    {
        readFileErr.AddLast(item);
        return 0;
    }

    using var streamReader = new StreamReader(fileStream);
    var foundLines = 0;

    string? codeline;
    do
    {
        if (streamReader.ReadLine() is not string notempty) break;
        codeline = notempty.Trim('\t', ' ');

        if (string.IsNullOrWhiteSpace(codeline)
            ? state.IsRigheVuote
            : !codeline.StartsWith("//") && !codeline.StartsWith("///") || state.IsCommenti)
            foundLines++;
    }
    while (codeline != null);

    fileStream.Dispose();
    return foundLines;
}

static Regex CreateRxFileEnum(string pattern, bool isCaseSensitive)
    => new(pattern, RegexOptions.ECMAScript | RegexOptions.Compiled | (isCaseSensitive ? RegexOptions.None : RegexOptions.IgnoreCase));

void TryChoice(MenuAction action)
{
    switch (action)
    {
        case MenuAction.SetFilter: ChoiceFilter(); break;
        case MenuAction.SetRecursion: ChoiceFileRecursion(); break;
        case MenuAction.SetComments: ChoiceComments(); break;
        case MenuAction.SetEmptyLines: ChoiceEmptyLines(); break;
        case MenuAction.ResetState: ChoiceResetState(); break;
        case MenuAction.SetCaseSensitivity: ChoiceCaseSensitivity(); break;
        case MenuAction.SetExcludePath: ChoiceExcludePath(); break;
        case MenuAction.SetOverridePath: ChoiceOverridePath(); break;
        case MenuAction.Execute: Execute(); break;
    }
}

delegate string _fd_strstr(string _s);
record struct SearchFlags(
    bool IsEnumeraFileRicorsione,
    bool IsCommenti,
    bool IsRigheVuote,
    bool IsRigheGlobali,
    bool IsFileEnumCaseSensitive,
    string EnumFileFilter,
    string? FolderExclude,
    string EnumPath);
record struct SessionResults(int TotFoundLines, int TotEnumeratedFiles, int FileCount, Top10Record[] Top10);
record struct Top10Record(string FileNamePath, int LineCount);
enum ReadState { Initial, VtEscape, VtBracket, VtNonBracket, _3CharSequence }
record struct StdinValue(bool IsMeta, char Input, ConsoleKey Meta);
enum CursorDirection { Init, Up, Down, PageUp, PageDown, Start, End }
record struct CursorState(int X, int Y, int MaxY);
enum MenuAction { SetFilter, SetRecursion, SetComments, SetEmptyLines, ResetState, SetCaseSensitivity, SetExcludePath, SetOverridePath, Execute, Exit, None }