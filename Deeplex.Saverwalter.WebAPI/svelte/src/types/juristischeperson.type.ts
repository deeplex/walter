import type { PersonEntry } from "$types";

export type JuristischePersonEntry = {
    mitglieder: PersonEntry[];
} & PersonEntry