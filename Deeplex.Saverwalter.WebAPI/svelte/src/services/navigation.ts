import { walter_goto } from './utils';

export const navigation = {
    adresse: (id: number) => walter_goto(`/adressen/${id}`),
    betriebskostenrechnung: (id: number) =>
        walter_goto(`/betriebskostenrechnungen/${id}`),
    erhaltungsaufwendung: (id: number) =>
        walter_goto(`/erhaltungsaufwendungen/${id}`),
    kontakt: (id: number) => walter_goto(`/kontakte/${id}`),
    miete: (id: number) => walter_goto(`/mieten/${id}`),
    mietminderung: (id: number) => walter_goto(`/mietminderungen/${id}`),
    umlage: (id: number) => walter_goto(`/umlagen/${id}`),
    umlagetyp: (id: number) => walter_goto(`/umlagetypen/${id}`),
    vertrag: (id: number) => walter_goto(`/vertraege/${id}`),
    vertragversion: (id: number) => walter_goto(`/vertragversionen/${id}`),
    wohnung: (id: number) => walter_goto(`/wohnungen/${id}`),
    zaehler: (id: number) => walter_goto(`/zaehler/${id}`),
    zaehlerstand: (id: number) => walter_goto(`/zaehlerstaende/${id}`)
};
