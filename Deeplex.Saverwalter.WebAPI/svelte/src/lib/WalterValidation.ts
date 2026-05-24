// Copyright (c) 2023-2024 Kai Lawrence
// AGPL-3.0 license

type E = Record<string, unknown>;
const s = (v: unknown): boolean => typeof v === 'string' && v.trim().length > 0;
const n = (v: unknown): boolean => v != null;
const id = (v: unknown): boolean => !!(v as E)?.id;

export const validateAdresse = (e: unknown): boolean => {
    const x = e as E;
    return s(x?.strasse) && s(x?.hausnummer) && s(x?.postleitzahl) && s(x?.stadt);
};
export const validateWohnung = (e: unknown): boolean => {
    const x = e as E;
    return validateAdresse(x?.adresse) && s(x?.bezeichnung) && n(x?.wohnflaeche) && n(x?.nutzflaeche) && n(x?.miteigentumsanteile) && n(x?.einheiten);
};
export const validateZaehler = (e: unknown): boolean => {
    const x = e as E;
    return s(x?.kennnummer) && id(x?.typ);
};
export const validateZaehlerstand = (e: unknown): boolean => {
    const x = e as E;
    return n(x?.stand) && s(x?.datum) && s(x?.einheit);
};
export const validateKontakt = (e: unknown): boolean => s((e as E)?.name);
export const validateBetriebskostenrechnung = (e: unknown): boolean => {
    const x = e as E;
    return n(x?.betreffendesJahr) && n(x?.betrag) && s(x?.datum) && id(x?.typ) && id(x?.umlage);
};
export const validateUmlage = (e: unknown): boolean => {
    const x = e as E;
    return id(x?.typ) && id(x?.schluessel);
};
export const validateUmlagetyp = (e: unknown): boolean => s((e as E)?.bezeichnung);
export const validateErhaltungsaufwendung = (e: unknown): boolean => {
    const x = e as E;
    return s(x?.bezeichnung) && id(x?.aussteller) && s(x?.datum) && id(x?.wohnung) && n(x?.betrag);
};
export const validateVertrag = (e: unknown): boolean => id((e as E)?.wohnung);
export const validateVertragQuickAdd = (e: unknown): boolean => {
    const x = e as E;
    const v = (x?.versionen as E[])?.[0];
    return id(x?.wohnung) && s(v?.beginn) && n(v?.grundmiete) && n(v?.personenzahl);
};
export const validateVertragVersion = (e: unknown): boolean => {
    const x = e as E;
    return s(x?.beginn) && n(x?.grundmiete) && n(x?.personenzahl);
};
export const validateGarage = (e: unknown): boolean => {
    const x = e as E;
    return s(x?.kennung) && id(x?.besitzer);
};
export const validateGarageVertrag = (e: unknown): boolean => {
    const x = e as E;
    return id(x?.garage);
};
export const validateGarageVertragQuickAdd = (e: unknown): boolean => {
    const x = e as E;
    const v = (x?.versionen as E[])?.[0];
    return id(x?.garage) && s(v?.beginn) && n(v?.garagenMiete);
};
export const validateGarageVertragVersion = (e: unknown): boolean => {
    const x = e as E;
    return s(x?.beginn) && n(x?.garagenMiete);
};
export const validateWohnungVersion = (e: unknown): boolean => {
    const x = e as E;
    return s(x?.beginn) && n(x?.wohnflaeche) && n(x?.nutzflaeche) && n(x?.miteigentumsanteile) && n(x?.einheiten);
};
export const validateUmlageVersion = (e: unknown): boolean => {
    const x = e as E;
    return s(x?.beginn) && id(x?.schluessel);
};
