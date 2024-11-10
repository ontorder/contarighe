namespace contarighe;

public static class Vt100
{
    public static string AbsolutePosition(int x, int y) => $"\u001b[{y};{x}f";
    public static string CursorDown(int n) => $"\u001b[{n}B";
    public static string CursorHorizontalAbsolute(int n) => $"\u001b[{n}G";
    public static string CursorLeft(int n) => $"\u001b[{n}C";
    public static string CursorNextLine(int n) => $"\u001b[{n}E";
    public static string CursorPosition(int x, int y) => $"\u001b[{y};{x}H";
    public static string CursorPrevLine(int n) => $"\u001b[{n}F";
    public static string CursorRight(int n) => $"\u001b[{n}D";
    public static string CursorUp(int n) => $"\u001b[{n}A";
    public static string DeviceStatus() => "\u001b[6n";
    public static string DeviceStatusReport() => "\u001b[0n";
    public static string EraseDisplay(int n) => $"\u001b[{n}J";
    public static string EraseLine(int n) => $"\u001b[{n}K";
    public static string HideCursor() => "\u001b[?25l";
    public static string ResetMode(params int[] n) => $"\u001b[{string.Join(";", n)}l";
    public static string ResetScrollRegion() => "\u001b[r";
    public static string RestoreCursorPosition() => "\u001b[u";
    public static string SaveCursorPosition() => "\u001b[s";
    public static string ScrollDown(int n) => $"\u001b[{n}T";
    public static string ScrollUp(int n) => $"\u001b[{n}S";
    public static string SelectGraphicRendition(params int[] n) => $"\u001b[{string.Join(";", n)}m";
    public static string SetAlternateScreenBuffer() => "\u001b[?1049h";
    public static string SetMode(params int[] n) => $"\u001b[{string.Join(";", n)}h";
    public static string SetScrollRegion(int top, int bottom) => $"\u001b[{top};{bottom}r";
    public static string ShowCursor() => "\u001b[?25h";
    public static string VerticalPositionAbsolute(int n) => $"\u001b[{n}d";
}
