using System.Text.RegularExpressions;

SessionResults results = new(0, 0, 0, new Top10Record[10]);
SearchFlags state = new(true, false, false, false, false, "\\.cs$", null, Directory.GetCurrentDirectory());

var rxFileEnum = CreateRxFileEnum(state.EnumFileFilter, state.IsFileEnumCaseSensitive);
var rxPathExclide = (Regex?)null;
string? menuChoiceResp;

var realChoice = true;
do
{
    if (realChoice) PrintMenu();
    PrintPrompt();
    menuChoiceResp = Console.ReadLine();
    realChoice = TryChoice();
    if (realChoice == false) Console.SetCursorPosition(0, Console.CursorTop - 1);
}
while (menuChoiceResp != "q");

void PrintPrompt() => Console.Write(": ");

void PrintMenu()
{
    Console.WriteLine(Environment.NewLine);
    Console.WriteLine("1/f  cambia filtro");
    Console.WriteLine("2/r  impostazioni ricorsività");
    Console.WriteLine("3/c  conteggia commenti");
    Console.WriteLine("4/v  filtra righe vuote");
    Console.WriteLine("5/z  azzera conteggi per ogni sessione");
    Console.WriteLine("6/m  impostazioni maiuscole/minuscole");
    Console.WriteLine("7/e  escludi cartelle");
    Console.WriteLine("8/p  override percorso");
    Console.WriteLine("0    esegui conteggio");
    Console.WriteLine("q    esci");
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

bool TryChoice()
{
    switch (menuChoiceResp)
    {
        case "1": case "f": case "F": ChoiceFilter(); break;
        case "2": case "r": case "R": ChoiceFileRecursion(); break;
        case "3": case "c": case "C": ChoiceComments(); break;
        case "4": case "v": case "V": ChoiceEmptyLines(); break;
        case "5": case "z": case "Z": ChoiceResetState(); break;
        case "6": case "m": case "M": ChoiceCaseSensitivity(); break;
        case "7": case "e": case "E": ChoiceExcludePath(); break;
        case "8": case "p": case "P": ChoiceOverridePath(); break;
        case "0": Execute(); break;
        case "q": break;
        default: return false;
    }
    return true;
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