export class VertragEntry {
    id: number;
    beginn: string;
    ende: string | undefined;

    constructor(e: VertragEntry) {
        this.id = e.id;
        this.beginn = new Date(e.beginn).toLocaleDateString("de-DE");
        this.ende = e?.ende ? new Date(e.ende).toLocaleDateString("de-DE") : undefined;
    }
}