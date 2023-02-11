export type VertragEntry = {
    id: number;
    beginn: string; // new Date(e.beginn).toLocaleDateString("de-DE")
    ende: string | undefined; // e?.ende ? new Date(e.ende).toLocaleDateString("de-DE") : undefined
}