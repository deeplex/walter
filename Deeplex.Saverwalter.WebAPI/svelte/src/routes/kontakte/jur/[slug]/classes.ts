import { PersonEntry } from "../../person/classes";

export class JuristischePersonEntry extends PersonEntry {
    name: string;

    constructor(e: JuristischePersonEntry) {
        super(e);
    }
}